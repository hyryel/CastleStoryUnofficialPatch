using System;
using System.Collections.Generic;
using System.Diagnostics;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Pathfinding.Test;
using UnityEngine;

namespace CastleStory.BuildGoalFix
{
    // v1.6 : ordre de construction d'un projet (BuildGoalProvider).
    // Constat : un Brictron pose un bloc qui coupe le chemin vers le Blueprint d'un autre (PathExecutionFailed
    // après un long trajet). Vanilla ordonne seulement par hauteur et distance, Brictron par Brictron.
    //
    // Modèle (06 §10) : grille locale autour du projet. Case « debout » p (comme Voxel.IsOpenAndWalkable) :
    // p+1 vide, et p en pente ou p vide sur p-1 plein ; pas d'arbre/blocage nature ; volume « walkable » admis.
    // Déplacement : 4 voisins, même hauteur sinon ±1 voxel (TerrainPathNode.AddNeighborBranches).
    // Les cases « fantome » d'un Blueprint deviennent pleines une fois construit (portes exclues : passables).
    // R(S) = cases debout reliées au bord de la zone (= au reste du monde) dans l'état S.
    //
    // Étape 1 (garde, clé BuildReach) :
    //   (a) accès : un Blueprint est « accessible » si une de ses cases d'accès est dans R(actuel). Informatif
    //       seulement (journal) : le modèle ignore les échelles, et vanilla vérifie déjà l'accès réel au choix
    //       (BuildGoalFinder) ; refuser sur le modèle pourrait bloquer un Blueprint atteignable.
    //   (b) non-coupure : un Blueprint X n'est pas attribué si le construire retire de R
    //       - toutes les cases d'accès d'un autre Blueprint du projet accessible aujourd'hui ;
    //       - la case d'un Brictron qui est aujourd'hui dans R (v1.6.1 : Brictron enfermés à l'étage) ;
    //       - toutes ses propres cases d'accès (celui qui pose resterait enfermé).
    //   Un Brictron inactif qui serait enfermé reçoit un ordre de redescendre vers le bord (v1.6.1).
    //   Sécurités : si aucun Blueprint n'est accessible (modèle en défaut) ou si tous les accessibles coupent
    //   quelque chose (blocage mutuel), le contrôle est levé pour le projet jusqu'au prochain calcul.
    // Étape 2 (plan, clé BuildPlan) : bonus de priorité « de l'intérieur vers l'extérieur » : profondeur des
    //   cases d'accès (distance au bord) dans l'état final (tous les Blueprints construits) ; un Blueprint
    //   inaccessible dans l'état final (pièce fermée) passe en tête. Bonus ajouté à BuildGoal.PriorityFor.
    // Calcul par projet, mis en cache : refait si la liste des Blueprints change ou au bout de Refresh secondes.
    // v1.6.1 : carte des cases debout précalculée une fois par calcul ; un test de coupure ne réévalue que les
    //          cases touchées par la pose (parcours sur index) : 647 ms → quelques ms (gel régulier du jeu).
    public static class BuildOrder
    {
        public static bool ReachEnabled;
        public static bool PlanEnabled;

        private const float Refresh = 3f;
        private const int Margin = 6, MarginDown = 3, MarginUp = 4;
        private const int MaxCells = 600000;
        private const int BonusPerStep = 3, MaxDepthSteps = 20;
        private const float EvacuateEvery = 8f;

        static BuildOrder() { Hooks.EnsureLoaded(); }

        private sealed class GoalInfo
        {
            public BuildGoal Goal;
            public List<XYZ> Cells = new List<XYZ>();   // deviennent pleines (vide pour une porte)
            public List<XYZ> Access = new List<XYZ>();
            public bool Reach;
            public bool Allowed = true;
            public string CutVictim;
            public int Bonus;
        }

        private sealed class Plan
        {
            public string Signature;
            public float Time;
            public int Frame;
            public bool Disabled;
            public string Note = "";
            public long Ms;
            public Dictionary<BuildGoal, GoalInfo> Infos = new Dictionary<BuildGoal, GoalInfo>();
            public string LoggedState;
        }

        private static readonly Dictionary<BuildGoalProvider, Plan> Plans = new Dictionary<BuildGoalProvider, Plan>();
        private static readonly Dictionary<Labor, float> LastEvacuation = new Dictionary<Labor, float>();
        private static Labor[] _labors = new Labor[0];
        private static float _laborsTime = -100f;

        // --- API pour les hooks ---

        // Vrai si le Blueprint peut être attribué maintenant (étape 1). reason : motif du refus.
        public static bool Allowed(BuildGoal goal, out string reason)
        {
            reason = null;
            if (!ReachEnabled || goal == null || goal.IsAnti || goal.Provider == null) return true;
            Plan plan = GetPlan(goal);
            GoalInfo info;
            if (plan == null || plan.Disabled || !plan.Infos.TryGetValue(goal, out info)) return true;
            if (!info.Allowed) { reason = "couperait l'accès de " + info.CutVictim; return false; }
            return true;
        }

        // Ret de BuildGoal.PriorityFor : bonus du plan (étape 2).
        public static int AfterPriority(int result, BuildGoal goal, Labor labor, ref float time)
        {
            try
            {
                if (!PlanEnabled || goal == null || goal.IsAnti || goal.Provider == null) return result;
                Plan plan = GetPlan(goal);
                GoalInfo info;
                if (plan == null || !plan.Infos.TryGetValue(goal, out info)) return result;
                return result + info.Bonus;
            }
            catch (Exception e) { Hooks.LogError(e); return result; }
        }

        // Pour le journal d'échec : état d'accès d'un Blueprint selon le dernier calcul.
        public static string Describe(BuildGoal goal)
        {
            try
            {
                if (goal == null || goal.Provider == null) return "";
                Plan plan;
                GoalInfo info;
                if (!Plans.TryGetValue(goal.Provider, out plan) || !plan.Infos.TryGetValue(goal, out info)) return " ; accès ?";
                return " ; accès " + (info.Reach ? "oui" : "NON") + (info.Allowed ? "" : " (reporté : coupe " + info.CutVictim + ")");
            }
            catch { return ""; }
        }

        // --- Calcul ---

        private static Plan GetPlan(BuildGoal goal)
        {
            BuildGoalProvider provider = goal.Provider;
            Plan plan;
            bool known = Plans.TryGetValue(provider, out plan);
            // Même frame : pas de relecture de la liste des Blueprints (IsMatchFor/PriorityFor appelés pour chaque goal).
            if (known && plan.Frame == UnityEngine.Time.frameCount) return plan;
            List<BuildGoal> goals = Hooks.ProjectGoalsOf(goal);
            string sig = Signature(goals);
            if (known && plan.Signature == sig && UnityEngine.Time.time - plan.Time < Refresh)
            {
                plan.Frame = UnityEngine.Time.frameCount;
                return plan;
            }
            if (Plans.Count > 32) Plans.Clear();
            Plan fresh = Compute(goals);
            fresh.Signature = sig;
            fresh.Time = UnityEngine.Time.time;
            fresh.Frame = UnityEngine.Time.frameCount;
            if (known) fresh.LoggedState = plan.LoggedState;
            Plans[provider] = fresh;
            LogPlan(goal, fresh);
            return fresh;
        }

        private static string Signature(List<BuildGoal> goals)
        {
            int h = goals.Count;
            foreach (BuildGoal g in goals) h = h * 31 + (g != null ? g.GetInstanceID() : 0);
            return goals.Count + ":" + h;
        }

        private static Plan Compute(List<BuildGoal> goals)
        {
            var plan = new Plan();
            var sw = Stopwatch.StartNew();
            var infos = new List<GoalInfo>();
            foreach (BuildGoal g in goals)
            {
                if (g == null || g.IsNullOrReleased() || g.IsAnti || g.volume == null) continue;
                var info = new GoalInfo { Goal = g };
                if (!IsPassable(g)) foreach (XYZ c in g.volume.blueprint) info.Cells.Add(c);
                foreach (XYZ a in g.volume.access) info.Access.Add(a);
                infos.Add(info);
                plan.Infos[g] = info;
            }
            if (infos.Count == 0) return plan;

            var grid = Grid.Build(infos);
            if (grid == null) { plan.Disabled = true; plan.Note = "zone trop grande, contrôle levé"; return plan; }

            // État actuel.
            int[] r0 = grid.Flood(null);
            int reachable = 0;
            foreach (GoalInfo i in infos) { i.Reach = grid.AnyIn(r0, i.Access); if (i.Reach) reachable++; }
            if (reachable == 0)
            {
                plan.Disabled = true;
                plan.Note = "aucun Blueprint accessible selon le modèle, contrôle levé";
            }
            else if (ReachEnabled)
            {
                // Brictron présents dans la zone et reliés au bord : à ne pas enfermer.
                var people = new List<KeyValuePair<Labor, int>>();
                foreach (Labor l in Labors())
                {
                    if (l == null || l.IsNullOrReleased() || !l.enabled) continue;
                    int cell = grid.StandCellOf(l.transform.position);
                    if (cell >= 0 && r0[cell] >= 0) people.Add(new KeyValuePair<Labor, int>(l, cell));
                }
                var trapped = new HashSet<Labor>();
                // (b) Coupure : un parcours par Blueprint accessible qui occupe une case debout de R0 (ou sa tête).
                int allowed = 0;
                foreach (GoalInfo x in infos)
                {
                    if (!x.Reach) continue;
                    if (grid.TouchesReach(r0, x.Cells))
                    {
                        int[] rx = grid.Flood(x.Cells);
                        if (!grid.AnyIn(rx, x.Access)) { x.Allowed = false; x.CutVictim = "celui qui le pose"; }
                        if (x.Allowed)
                            foreach (GoalInfo g in infos)
                            {
                                if (g == x || !g.Reach || grid.AnyIn(rx, g.Access)) continue;
                                x.Allowed = false;
                                x.CutVictim = g.Goal.name;
                                break;
                            }
                        foreach (var kv in people)
                        {
                            // Brictron debout sur la case posée (ou dessous) : simple passage, pas un enfermement.
                            if (rx[kv.Value] >= 0 || grid.Covers(x.Cells, kv.Value)) continue;
                            trapped.Add(kv.Key);
                            if (x.Allowed) { x.Allowed = false; x.CutVictim = "Brictron " + kv.Key.name; }
                        }
                    }
                    if (x.Allowed) allowed++;
                }
                if (allowed == 0)
                {
                    foreach (GoalInfo x in infos) x.Allowed = true;
                    plan.Note = "tous les Blueprints accessibles coupent quelque chose, coupure ignorée";
                }
                else Evacuate(grid, r0, trapped, people);
            }

            // Étape 2 : profondeur dans l'état final.
            if (PlanEnabled)
            {
                var all = new List<XYZ>();
                foreach (GoalInfo i in infos) all.AddRange(i.Cells);
                int[] depth = grid.Flood(all);
                int max = 0;
                var raw = new Dictionary<GoalInfo, int>();
                foreach (GoalInfo i in infos)
                {
                    int d = grid.MinDepth(depth, i.Access);
                    raw[i] = d;
                    if (d > max) max = d;
                }
                foreach (GoalInfo i in infos)
                {
                    int d = raw[i] < 0 ? max + 5 : raw[i];   // inaccessible une fois tout construit : avant la fermeture
                    i.Bonus = Math.Min(d, MaxDepthSteps) * BonusPerStep;
                }
            }
            plan.Ms = sw.ElapsedMilliseconds;
            plan.Note += (plan.Note.Length > 0 ? " ; " : "") + string.Format("{0} cases, {1} ms", grid.Count, plan.Ms);
            return plan;
        }

        // Brictron de la zone (rafraîchi toutes les 2 s : FindObjectsOfType est coûteux).
        private static Labor[] Labors()
        {
            if (UnityEngine.Time.time - _laborsTime > 2f)
            {
                _labors = UnityEngine.Object.FindObjectsOfType<Labor>();
                _laborsTime = UnityEngine.Time.time;
            }
            return _labors;
        }

        // Brictron inactif qui serait enfermé par une pose à venir : ordre de marcher vers le bord de la zone
        // (case reliée la plus proche). Un Brictron occupé finira sa tâche ; il sera traité au calcul suivant.
        private static void Evacuate(Grid grid, int[] r0, HashSet<Labor> trapped, List<KeyValuePair<Labor, int>> people)
        {
            if (trapped.Count == 0) return;
            float now = UnityEngine.Time.time;
            foreach (var kv in people)
            {
                Labor l = kv.Key;
                if (!trapped.Contains(l) || !IsIdle(l)) continue;
                float last;
                if (LastEvacuation.TryGetValue(l, out last) && now - last < EvacuateEvery) continue;
                int target = grid.NearestBorder(r0, kv.Value);
                if (target < 0) continue;
                XYZ dest = grid.Position(target);
                try
                {
                    Order.Move.WalkToward.Send(l, Location.Of(dest), Labor.DecisionSource.Project);
                    LastEvacuation[l] = now;
                    if (LastEvacuation.Count > 256) LastEvacuation.Clear();
                    Hooks.LogLine(string.Format("Évacuation : {0} serait enfermé, envoyé vers {1},{2},{3}", l.name, dest.x, dest.y, dest.z));
                }
                catch (Exception e) { Hooks.LogError(e); }
            }
        }

        private static bool IsIdle(Labor l)
        {
            if (l.PendingTask != null) return false;
            if (!(l.Project == null || l.Project is IdleProject)) return false;
            Brix.Game.AI.Task t = l.CurrentTask;
            return t == null || t.Giver == null;
        }

        // Porte (Door, HeavyDoor) : passable une fois construite, ne compte pas comme obstacle.
        private static bool IsPassable(BuildGoal g)
        {
            string key = g.AssetKey != null ? g.AssetKey.Name : g.name;
            return key != null && key.IndexOf("Door", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void LogPlan(BuildGoal goal, Plan plan)
        {
            if (!Hooks.OrdersLogged) return;
            int unreachable = 0, deferred = 0;
            var names = new List<string>();
            foreach (GoalInfo i in plan.Infos.Values)
            {
                if (!i.Reach) { unreachable++; if (names.Count < 6) names.Add(i.Goal.name + " inaccessible"); }
                else if (!i.Allowed) { deferred++; if (names.Count < 6) names.Add(i.Goal.name + " coupe " + i.CutVictim); }
            }
            string state = string.Format("{0} Blueprint(s), {1} inaccessible(s), {2} reporté(s){3}", plan.Infos.Count, unreachable, deferred,
                plan.Disabled ? ", contrôle levé" : "");
            // Calcul lent : toujours journalisé (surveillance des gels).
            if (state == plan.LoggedState && plan.Ms < 30) return;
            plan.LoggedState = state;
            Hooks.LogLine(string.Format("{0} accès : {1}{2} ; {3}", Hooks.OrderTagOf(goal), state,
                names.Count > 0 ? " [" + string.Join(", ", names.ToArray()) + "]" : "", plan.Note));
        }

        // --- Grille locale ---

        private sealed class Grid
        {
            private int x0, y0, z0, nx, ny, nz;
            private byte[] cell;    // bits : 1 non vide, 2 plein entier, 4 blocage nature, 8 volume walkable
            private bool[] stand0;  // cases debout dans l'état actuel
            private int strideX;    // pas d'index : y → 1, z → ny, x → nz*ny
            public int Count { get { return cell.Length; } }

            public static Grid Build(List<GoalInfo> infos)
            {
                int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue, maxZ = int.MinValue;
                foreach (GoalInfo i in infos)
                {
                    foreach (XYZ p in i.Cells) Extend(p, ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
                    foreach (XYZ p in i.Access) Extend(p, ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
                }
                if (minX == int.MaxValue) return null;
                var g = new Grid
                {
                    x0 = minX - Margin, y0 = minY - MarginDown, z0 = minZ - Margin,
                    nx = maxX - minX + 1 + 2 * Margin, ny = maxY - minY + 1 + MarginDown + MarginUp, nz = maxZ - minZ + 1 + 2 * Margin,
                };
                long n = (long)g.nx * g.ny * g.nz;
                if (n > MaxCells) return null;
                g.strideX = g.nz * g.ny;
                g.cell = new byte[n];
                TestPathfinding tp = TestPathfinding.Instance;
                for (int x = 0; x < g.nx; x++)
                    for (int z = 0; z < g.nz; z++)
                        for (int y = 0; y < g.ny; y++)
                        {
                            var p = new XYZ(g.x0 + x, g.y0 + y, g.z0 + z);
                            byte b = 0;
                            if (Voxel.IsNonEmpty(p)) { b |= 1; if (Voxel.IsFull(p)) b |= 2; }
                            if (TestPathfinding.IsNatureBlocker(p)) b |= 4;
                            if (tp != null && tp.IsAWalkableObstacle(p)) b |= 8;
                            g.cell[g.Index(x, y, z)] = b;
                        }
                g.stand0 = new bool[n];
                for (int i = 0; i < n; i++) g.stand0[i] = g.StandAt(i, null);
                return g;
            }

            private static void Extend(XYZ p, ref int minX, ref int minY, ref int minZ, ref int maxX, ref int maxY, ref int maxZ)
            {
                if (p.x < minX) minX = p.x; if (p.y < minY) minY = p.y; if (p.z < minZ) minZ = p.z;
                if (p.x > maxX) maxX = p.x; if (p.y > maxY) maxY = p.y; if (p.z > maxZ) maxZ = p.z;
            }

            private int Index(int x, int y, int z) { return (x * nz + z) * ny + y; }

            private int IndexOf(XYZ p)
            {
                int x = p.x - x0, y = p.y - y0, z = p.z - z0;
                return x >= 0 && y >= 0 && z >= 0 && x < nx && y < ny && z < nz ? Index(x, y, z) : -1;
            }

            public XYZ Position(int i)
            {
                int y = i % ny, z = (i / ny) % nz, x = i / strideX;
                return new XYZ(x0 + x, y0 + y, z0 + z);
            }

            // Case debout d'indice i, avec des cases rendues pleines (overlay). Au-dessus/au-dessous de la grille :
            // tête libre, sol absent (marges de 3 dessous et 4 dessus).
            private bool StandAt(int i, HashSet<int> overlay)
            {
                if (overlay != null && overlay.Contains(i)) return false;
                byte bp = cell[i];
                if ((bp & 4) != 0) return false;
                int y = i % ny;
                if (y + 1 < ny)
                {
                    if ((cell[i + 1] & 1) != 0 || (overlay != null && overlay.Contains(i + 1))) return false;
                }
                if ((bp & 8) != 0) return true;
                if ((bp & 1) != 0) return (bp & 2) == 0;          // pente
                if (y == 0) return false;
                return (cell[i - 1] & 2) != 0 || (overlay != null && overlay.Contains(i - 1));
            }

            // Cases dont l'état debout change quand `solid` devient plein : la case, celle du dessous (tête), celle du
            // dessus (nouveau sol). Les autres gardent stand0.
            private Dictionary<int, bool> Changes(List<XYZ> solid)
            {
                if (solid == null || solid.Count == 0) return null;
                var overlay = new HashSet<int>();
                foreach (XYZ c in solid) { int i = IndexOf(c); if (i >= 0) overlay.Add(i); }
                var changes = new Dictionary<int, bool>();
                foreach (int i in overlay)
                {
                    int y = i % ny;
                    changes[i] = false;
                    if (y > 0) changes[i - 1] = StandAt(i - 1, overlay);
                    if (y + 1 < ny) changes[i + 1] = StandAt(i + 1, overlay);
                }
                return changes;
            }

            private bool S(int i, Dictionary<int, bool> changes)
            {
                bool v;
                return changes != null && changes.TryGetValue(i, out v) ? v : stand0[i];
            }

            // Distance (en pas) au bord horizontal de la grille par les cases debout ; -1 si non relié.
            public int[] Flood(List<XYZ> solid)
            {
                Dictionary<int, bool> ch = Changes(solid);
                var dist = new int[cell.Length];
                for (int i = 0; i < dist.Length; i++) dist[i] = -1;
                var queue = new Queue<int>();
                for (int x = 0; x < nx; x++)
                    for (int z = 0; z < nz; z++)
                    {
                        if (x != 0 && z != 0 && x != nx - 1 && z != nz - 1) continue;
                        for (int y = 0; y < ny; y++)
                        {
                            int i = Index(x, y, z);
                            if (!S(i, ch)) continue;
                            dist[i] = 0;
                            queue.Enqueue(i);
                        }
                    }
                while (queue.Count > 0)
                {
                    int i = queue.Dequeue();
                    int y = i % ny, z = (i / ny) % nz, x = i / strideX;
                    int d = dist[i] + 1;
                    Step(i, x + 1 < nx, strideX, y, d, dist, queue, ch);
                    Step(i, x > 0, -strideX, y, d, dist, queue, ch);
                    Step(i, z + 1 < nz, ny, y, d, dist, queue, ch);
                    Step(i, z > 0, -ny, y, d, dist, queue, ch);
                }
                return dist;
            }

            // Voisin horizontal : même hauteur si debout, sinon un voxel plus bas et/ou plus haut.
            private void Step(int i, bool inside, int offset, int y, int d, int[] dist, Queue<int> queue, Dictionary<int, bool> ch)
            {
                if (!inside) return;
                int n = i + offset;
                if (S(n, ch)) { Visit(n, d, dist, queue); return; }
                if (y > 0 && S(n - 1, ch)) Visit(n - 1, d, dist, queue);
                if (y + 1 < ny && S(n + 1, ch)) Visit(n + 1, d, dist, queue);
            }

            private static void Visit(int n, int d, int[] dist, Queue<int> queue)
            {
                if (dist[n] >= 0) return;
                dist[n] = d;
                queue.Enqueue(n);
            }

            public bool AnyIn(int[] dist, List<XYZ> cells)
            {
                foreach (XYZ c in cells) { int i = IndexOf(c); if (i >= 0 && dist[i] >= 0) return true; }
                return false;
            }

            public int MinDepth(int[] depth, List<XYZ> cells)
            {
                int best = -1;
                foreach (XYZ c in cells)
                {
                    int i = IndexOf(c);
                    if (i >= 0 && depth[i] >= 0 && (best < 0 || depth[i] < best)) best = depth[i];
                }
                return best;
            }

            // Vrai si une des cases (ou la case sous elle : la tête d'un Brictron) est une case debout reliée.
            public bool TouchesReach(int[] dist, List<XYZ> cells)
            {
                foreach (XYZ c in cells)
                {
                    int i = IndexOf(c);
                    if (i >= 0 && dist[i] >= 0) return true;
                    int j = IndexOf(c - XYZ.up);
                    if (j >= 0 && dist[j] >= 0) return true;
                }
                return false;
            }

            // Vrai si la case i (ou celle de sa tête) fait partie des cases posées.
            public bool Covers(List<XYZ> cells, int i)
            {
                foreach (XYZ c in cells)
                {
                    int j = IndexOf(c);
                    if (j == i || j == i + 1) return true;
                }
                return false;
            }

            // Case debout d'un Brictron : sa case, sinon celle du dessous ou du dessus.
            public int StandCellOf(Vector3 position)
            {
                XYZ p = XYZ.FromVector3(position);
                foreach (XYZ q in new[] { p, p - XYZ.up, p + XYZ.up })
                {
                    int i = IndexOf(q);
                    if (i >= 0 && stand0[i]) return i;
                }
                return -1;
            }

            // Case de bord (distance 0) la plus proche de `from` en suivant les cases debout de l'état actuel.
            public int NearestBorder(int[] r0, int from)
            {
                var seen = new HashSet<int> { from };
                var queue = new Queue<int>();
                queue.Enqueue(from);
                while (queue.Count > 0)
                {
                    int i = queue.Dequeue();
                    if (r0[i] == 0) return i;
                    int y = i % ny, z = (i / ny) % nz, x = i / strideX;
                    foreach (int n in Neighbors(i, x, y, z))
                        if (seen.Add(n)) queue.Enqueue(n);
                }
                return -1;
            }

            private IEnumerable<int> Neighbors(int i, int x, int y, int z)
            {
                var offs = new List<int>();
                if (x + 1 < nx) offs.Add(strideX); if (x > 0) offs.Add(-strideX);
                if (z + 1 < nz) offs.Add(ny); if (z > 0) offs.Add(-ny);
                foreach (int o in offs)
                {
                    int n = i + o;
                    if (stand0[n]) { yield return n; continue; }
                    if (y > 0 && stand0[n - 1]) yield return n - 1;
                    if (y + 1 < ny && stand0[n + 1]) yield return n + 1;
                }
            }
        }
    }
}
