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
    //   (b) non-coupure : un Blueprint n'est pas attribué si le construire retire de R toutes les cases d'accès
    //       d'un autre Blueprint du projet accessible aujourd'hui.
    //   Sécurités : si aucun Blueprint n'est accessible (modèle en défaut) ou si tous les accessibles coupent
    //   quelqu'un (blocage mutuel), le contrôle est levé pour le projet jusqu'au prochain calcul.
    // Étape 2 (plan, clé BuildPlan) : bonus de priorité « de l'intérieur vers l'extérieur » : profondeur des
    //   cases d'accès (distance au bord) dans l'état final (tous les Blueprints construits) ; un Blueprint
    //   inaccessible dans l'état final (pièce fermée) passe en tête. Bonus ajouté à BuildGoal.PriorityFor.
    // Calcul par projet, mis en cache : refait si la liste des Blueprints change ou au bout de Refresh secondes.
    public static class BuildOrder
    {
        public static bool ReachEnabled;
        public static bool PlanEnabled;

        private const float Refresh = 3f;
        private const int Margin = 6, MarginDown = 3, MarginUp = 4;
        private const int MaxCells = 600000;
        private const int BonusPerStep = 3, MaxDepthSteps = 20;

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
            public bool Disabled;
            public string Note = "";
            public Dictionary<BuildGoal, GoalInfo> Infos = new Dictionary<BuildGoal, GoalInfo>();
            public string LoggedState;
        }

        private static readonly Dictionary<BuildGoalProvider, Plan> Plans = new Dictionary<BuildGoalProvider, Plan>();

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
            List<BuildGoal> goals = Hooks.ProjectGoalsOf(goal);
            string sig = Signature(goals);
            Plan plan;
            if (Plans.TryGetValue(provider, out plan) && plan.Signature == sig && UnityEngine.Time.time - plan.Time < Refresh)
                return plan;
            if (Plans.Count > 32) Plans.Clear();
            plan = Compute(goals);
            plan.Signature = sig;
            plan.Time = UnityEngine.Time.time;
            Plan previous;
            if (Plans.TryGetValue(provider, out previous)) plan.LoggedState = previous.LoggedState;
            Plans[provider] = plan;
            LogPlan(goal, plan);
            return plan;
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
                bool door = IsPassable(g);
                if (!door) foreach (XYZ c in g.volume.blueprint) info.Cells.Add(c);
                foreach (XYZ a in g.volume.access) info.Access.Add(a);
                infos.Add(info);
                plan.Infos[g] = info;
            }
            if (infos.Count == 0) return plan;

            var grid = Grid.Build(infos);
            if (grid == null) { plan.Disabled = true; plan.Note = "zone trop grande, contrôle levé"; return plan; }

            // État actuel.
            bool[] r0 = grid.Reach(null);
            int reachable = 0;
            foreach (GoalInfo i in infos) { i.Reach = grid.AnyIn(r0, i.Access); if (i.Reach) reachable++; }
            if (reachable == 0)
            {
                plan.Disabled = true;
                plan.Note = "aucun Blueprint accessible selon le modèle, contrôle levé";
            }
            else if (ReachEnabled)
            {
                // (b) Coupure : un parcours par Blueprint accessible qui occupe une case debout de R0 (ou sa tête).
                int allowed = 0;
                foreach (GoalInfo x in infos)
                {
                    if (!x.Reach) continue;
                    if (grid.TouchesReach(r0, x.Cells))
                    {
                        bool[] rx = grid.Reach(x.Cells);
                        foreach (GoalInfo g in infos)
                        {
                            if (g == x || !g.Reach || grid.AnyIn(rx, g.Access)) continue;
                            x.Allowed = false;
                            x.CutVictim = g.Goal.name;
                            break;
                        }
                    }
                    if (x.Allowed) allowed++;
                }
                if (allowed == 0)
                {
                    foreach (GoalInfo x in infos) x.Allowed = true;
                    plan.Note = "tous les Blueprints accessibles en coupent un autre, coupure ignorée";
                }
            }

            // Étape 2 : profondeur dans l'état final.
            if (PlanEnabled)
            {
                var all = new List<XYZ>();
                foreach (GoalInfo i in infos) all.AddRange(i.Cells);
                int[] depth = grid.Depth(all);
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
            plan.Note += (plan.Note.Length > 0 ? " ; " : "") + string.Format("{0} cases, {1} ms", grid.Count, sw.ElapsedMilliseconds);
            return plan;
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
            if (state == plan.LoggedState) return;
            plan.LoggedState = state;
            Hooks.LogLine(string.Format("{0} accès : {1}{2} ; {3}", Hooks.OrderTagOf(goal), state,
                names.Count > 0 ? " [" + string.Join(", ", names.ToArray()) + "]" : "", plan.Note));
        }

        // --- Grille locale ---

        private sealed class Grid
        {
            private int x0, y0, z0, nx, ny, nz;
            private byte[] cell;   // bits : 1 plein/non vide, 2 plein entier, 4 blocage nature, 8 volume walkable
            public int Count { get { return cell.Length; } }

            private static readonly XYZ[] Dirs = { new XYZ(1, 0, 0), new XYZ(-1, 0, 0), new XYZ(0, 0, 1), new XYZ(0, 0, -1) };

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
                return g;
            }

            private static void Extend(XYZ p, ref int minX, ref int minY, ref int minZ, ref int maxX, ref int maxY, ref int maxZ)
            {
                if (p.x < minX) minX = p.x; if (p.y < minY) minY = p.y; if (p.z < minZ) minZ = p.z;
                if (p.x > maxX) maxX = p.x; if (p.y > maxY) maxY = p.y; if (p.z > maxZ) maxZ = p.z;
            }

            private int Index(int x, int y, int z) { return (x * nz + z) * ny + y; }

            private bool Local(XYZ p, out int x, out int y, out int z)
            {
                x = p.x - x0; y = p.y - y0; z = p.z - z0;
                return x >= 0 && y >= 0 && z >= 0 && x < nx && y < ny && z < nz;
            }

            private int IndexOf(XYZ p)
            {
                int x, y, z;
                return Local(p, out x, out y, out z) ? Index(x, y, z) : -1;
            }

            // Hors grille : lecture directe du monde.
            private byte Raw(XYZ p, HashSet<int> overlay, out bool added)
            {
                int i = IndexOf(p);
                added = i >= 0 && overlay != null && overlay.Contains(i);
                if (i >= 0) return cell[i];
                byte b = 0;
                if (Voxel.IsNonEmpty(p)) { b |= 1; if (Voxel.IsFull(p)) b |= 2; }
                return b;
            }

            private bool Stand(XYZ p, HashSet<int> overlay)
            {
                bool addedP, addedUp, addedDown;
                byte bp = Raw(p, overlay, out addedP);
                if (addedP || (bp & 4) != 0) return false;
                byte bu = Raw(p + XYZ.up, overlay, out addedUp);
                if (addedUp || (bu & 1) != 0) return false;
                if ((bp & 8) != 0) return true;
                if ((bp & 1) != 0) return (bp & 2) == 0;          // pente
                byte bd = Raw(p - XYZ.up, overlay, out addedDown);
                return addedDown || (bd & 2) != 0;
            }

            private HashSet<int> Overlay(List<XYZ> cells)
            {
                if (cells == null || cells.Count == 0) return null;
                var o = new HashSet<int>();
                foreach (XYZ c in cells) { int i = IndexOf(c); if (i >= 0) o.Add(i); }
                return o;
            }

            // Cases debout reliées au bord horizontal de la grille.
            public bool[] Reach(List<XYZ> solidCells)
            {
                int[] d = Flood(Overlay(solidCells));
                var r = new bool[d.Length];
                for (int i = 0; i < d.Length; i++) r[i] = d[i] >= 0;
                return r;
            }

            // Distance (en pas) au bord, -1 si non relié.
            public int[] Depth(List<XYZ> solidCells) { return Flood(Overlay(solidCells)); }

            private int[] Flood(HashSet<int> overlay)
            {
                var dist = new int[cell.Length];
                for (int i = 0; i < dist.Length; i++) dist[i] = -1;
                var queue = new Queue<XYZ>();
                for (int x = 0; x < nx; x++)
                    for (int z = 0; z < nz; z++)
                    {
                        if (x != 0 && z != 0 && x != nx - 1 && z != nz - 1) continue;
                        for (int y = 0; y < ny; y++)
                        {
                            var p = new XYZ(x0 + x, y0 + y, z0 + z);
                            if (!Stand(p, overlay)) continue;
                            dist[Index(x, y, z)] = 0;
                            queue.Enqueue(p);
                        }
                    }
                while (queue.Count > 0)
                {
                    XYZ p = queue.Dequeue();
                    int dp = dist[IndexOf(p)];
                    foreach (XYZ dir in Dirs)
                    {
                        XYZ n = p + dir;
                        if (Stand(n, overlay)) { Visit(n, dp, dist, queue); continue; }
                        XYZ lo = n - XYZ.up, hi = n + XYZ.up;
                        if (Stand(lo, overlay)) Visit(lo, dp, dist, queue);
                        if (Stand(hi, overlay)) Visit(hi, dp, dist, queue);
                    }
                }
                return dist;
            }

            private void Visit(XYZ n, int dp, int[] dist, Queue<XYZ> queue)
            {
                int i = IndexOf(n);
                if (i < 0 || dist[i] >= 0) return;
                dist[i] = dp + 1;
                queue.Enqueue(n);
            }

            public bool AnyIn(bool[] reach, List<XYZ> cells)
            {
                foreach (XYZ c in cells) { int i = IndexOf(c); if (i >= 0 && reach[i]) return true; }
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

            // Vrai si une des cases (ou la case sous elle : la tête d'un Brictron) est une case debout de R.
            public bool TouchesReach(bool[] reach, List<XYZ> cells)
            {
                foreach (XYZ c in cells)
                {
                    int i = IndexOf(c);
                    if (i >= 0 && reach[i]) return true;
                    int j = IndexOf(c - XYZ.up);
                    if (j >= 0 && reach[j]) return true;
                }
                return false;
            }
        }
    }
}
