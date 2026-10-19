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
    // Ordre de construction d'un projet (BuildGoalProvider) : adaptateur Unity du plan (BuildPlanCore.cs).
    //
    // v1.6   : refus d'un Blueprint dont la pose couperait l'accès d'un autre ; bonus « intérieur d'abord ».
    // v1.6.1 : Brictron protégés (pas d'enfermement, poseur compris), évacuation des inactifs menacés.
    // v1.6.2 : Blueprints futurs (TrackedBlueprints) protégés dans l'état « rangées du dessous posées ».
    //          Échec en jeu (v1.8, tour de 4 rangées) : évalués dans cet état ils étaient déjà inaccessibles, donc
    //          jamais protégés ; une règle locale ne suffit pas.
    // v1.9   : plan global prouvé de l'état actuel à l'état final (BuildPlanCore, validé hors jeu sur la batterie
    //          patch/verify/buildsim, portage C# comparé instantané par instantané) :
    //          - tous les Blueprints commités du projet (constructibles et futurs) sont planifiés ensemble, par
    //            groupe (Blueprints dont les grilles se touchent ; groupes éloignés planifiés séparément) ;
    //          - pelage à rebours : l'ordre existe, ou les Blueprints impossibles sans escalier/appui sont SIGNALÉS
    //            (journal « NON CONSTRUCTIBLE ») et jamais attribués ; rien n'est ajouté à la place du joueur ;
    //          - garde exacte à l'attribution : X posé maintenant, le poseur ressort (maintenant et après les poses
    //            en cours), les attribués restent accessibles, aucun Brictron relié n'est enfermé (positions relues
    //            à chaque frame) et le reste du plan reste faisable ;
    //          - formes réelles (accès −2..+1 autour des cases, support, MaxMissingSupport) lues dans le Blueprint ;
    //          - Blueprints inaccessibles refusés (plus de trajets perdus en PathExecutionFailed) ;
    //          - coût : grilles réutilisées, lecture des voxels étalée sur plusieurs frames au-delà de 12 000 cases et
    //            relue en continu par tranches, aucune allocation dans les parcours, plus de FindObjectsOfType
    //            (Brictron vus par IsMatchFor et Labor.LaborStarted), tests limités à 2 ms par frame ;
    //          - sécurité : 10 s sans aucune autorisation ni pose en cours alors qu'il reste un Blueprint non signalé
    //            → contrôle levé pour ce groupe jusqu'au prochain changement (journalisé).
    // v1.9.1 : échec en jeu (tour 6×6, journal buildgoalfix-2026-10-19-v1.9.log) : une brique posée quittait la liste
    //          avant que son bloc soit lisible ; la brique du dessus passait « sans support », exclusion retenue
    //          (Sticky) puis propagée vers le haut. Correctif (scénarios reel_v19_tour5_* du simulateur) :
    //          - « sans support » recalculé à chaque calcul, jamais retenu (Sticky = « inaccessible » seulement) ;
    //          - Blueprint attribué disparu = posé : ses cases comptent pleines (Pending) jusqu'à ce que la grille
    //            les voie (ou PendingSeconds).
    public static class BuildOrder
    {
        public static bool ReachEnabled;
        public static bool PlanEnabled;

        private const float Refresh = 3f;
        private const int Margin = 6, MarginDown = 3, MarginUp = 4;
        private const int MaxCells = 600000;
        private const int SliceCells = 12000;          // au-delà : lecture des voxels étalée sur plusieurs frames
        private const double ReadBudgetMs = 2.0;       // lecture initiale des grilles, par frame (tous projets)
        private const double RefreshBudgetMs = 0.1;    // relecture continue, par frame et par groupe
        private const double TestBudgetMs = 2.0;       // tests d'attribution, par frame (tous projets)
        private const float ValveSeconds = 10f;
        private const float EvacuateEvery = 8f;
        private const float PendingSeconds = 5f;       // v1.9.1 : pose comptée pleine au plus 5 s avant d'être lue

        static BuildOrder() { Hooks.EnsureLoaded(); }

        // Forme d'un Blueprint (positions monde), lue une fois.
        private sealed class Shape
        {
            public XYZ[] Cells;
            public byte[] Kind;
            public XYZ[] Access;
            public XYZ[] Support;
            public bool SupportAny;
            public int MaxMissing;
            public int MinY;
            public int X0, Y0, Z0, X1, Y1, Z1;   // emprise (cases + accès)
        }

        // v1.9.1 : Blueprint attribué disparu (posé) dont la grille n'a pas encore lu les cases.
        private sealed class PendingPose
        {
            public Shape S;
            public float Time;
        }

        // Groupe de Blueprints planifiés ensemble (une grille, un plan).
        private sealed class Cluster
        {
            public readonly BuildPlanCore Core = new BuildPlanCore();
            public readonly List<BuildGoal> Goals = new List<BuildGoal>();
            public readonly Dictionary<BuildGoal, int> Index = new Dictionary<BuildGoal, int>();
            public long Signature = long.MinValue;
            public float Time = -100f;
            public bool HasCore, Ready, TooLarge, NeedsCompute, Used;
            public int BuildCursor, RefreshCursor;
            public int GridX0, GridY0, GridZ0, GridX1, GridY1, GridZ1;
            public float LastOk;
            public float LastEvacuation = -100f;
            public double Ms;
            public int Gc;
            public int Fast;                 // mises à jour par voie rapide depuis le dernier journal
        }

        private sealed class Plan
        {
            public readonly List<Cluster> Clusters = new List<Cluster>();
            public readonly Dictionary<BuildGoal, Cluster> Where = new Dictionary<BuildGoal, Cluster>();
            public readonly Dictionary<int, string> Sticky = new Dictionary<int, string>();   // exclusions « inaccessible » retenues
            public readonly List<PendingPose> Pending = new List<PendingPose>();               // v1.9.1 : posés pas encore lus
            public readonly HashSet<int> Known = new HashSet<int>();
            public readonly List<BuildGoal> Previous = new List<BuildGoal>();
            public long Signature = long.MinValue;
            public float Time = -100f;
            public int Frame = -1;
            public bool Dirty;
            public string LoggedState;
            public string LoggedExcluded;
        }

        private static readonly Dictionary<BuildGoalProvider, Plan> Plans = new Dictionary<BuildGoalProvider, Plan>();
        private static readonly Dictionary<BuildGoal, Shape> Shapes = new Dictionary<BuildGoal, Shape>();
        private static readonly Dictionary<Labor, float> LastEvacuation = new Dictionary<Labor, float>();
        private static readonly List<BuildGoal> Scratch = new List<BuildGoal>();
        private static readonly List<Cluster> FreeClusters = new List<Cluster>();
        private static readonly List<Cluster> OldClusters = new List<Cluster>();
        private static int[] _parent = new int[0], _next = new int[0], _head = new int[0];
        private static int _budgetFrame = -1;
        private static long _testTicks, _readTicks;

        // --- API pour les hooks ---

        // Vrai si le Blueprint peut être attribué maintenant. reason : motif du refus.
        public static bool Allowed(BuildGoal goal, out string reason)
        {
            reason = null;
            if (!ReachEnabled || goal == null || goal.IsAnti || goal.Provider == null) return true;
            Plan plan = GetPlan(goal);
            Cluster cl;
            if (plan == null || !plan.Where.TryGetValue(goal, out cl) || cl.TooLarge) return true;
            if (!cl.HasCore)
            {
                reason = "plan en calcul";
                return false;
            }
            int idx;
            BuildPlanCore core = cl.Core;
            if (core.Disabled || !cl.Index.TryGetValue(goal, out idx)) return true;
            if (!core.IsCached(idx))
            {
                // Tests limités par frame : au-delà, réponse différée (refus non mémorisé, retenté plus tard).
                NewBudgetFrame();
                if (_testTicks * 1000.0 / Stopwatch.Frequency > TestBudgetMs)
                {
                    reason = "calcul en cours";
                    return false;
                }
                long t0 = Stopwatch.GetTimestamp();
                bool r = core.Allowed(idx, out reason);
                _testTicks += Stopwatch.GetTimestamp() - t0;
                if (!r && core.Threatened.Count > 0) Evacuate(cl);
                return Check(plan, cl, r);
            }
            bool ok = core.Allowed(idx, out reason);
            if (!ok && core.Threatened.Count > 0) Evacuate(cl);
            return Check(plan, cl, ok);
        }

        private static void NewBudgetFrame()
        {
            int frame = UnityEngine.Time.frameCount;
            if (frame == _budgetFrame) return;
            _budgetFrame = frame;
            _testTicks = 0;
            _readTicks = 0;
        }

        // Sécurité : rien d'autorisé depuis ValveSeconds, aucune pose en cours, mais un Blueprint non signalé reste.
        private static bool Check(Plan plan, Cluster cl, bool ok)
        {
            float now = UnityEngine.Time.time;
            BuildPlanCore core = cl.Core;
            if (ok) { cl.LastOk = now; return true; }
            bool inFlight = false, open = false;
            foreach (PlanGoal g in core.Infos)
            {
                if (g.Placed) continue;
                if (g.InFlight) { inFlight = true; break; }
                if (!g.Excluded) open = true;
            }
            if (inFlight || !open) { cl.LastOk = now; return false; }
            if (now - cl.LastOk > ValveSeconds)
            {
                core.Disabled = true;
                Hooks.LogLine(string.Format("{0} plan : aucune attribution possible depuis {1:0} s, contrôle levé pour {2} Blueprint(s) jusqu'au prochain changement",
                    cl.Goals.Count > 0 ? Hooks.OrderTagOf(cl.Goals[0]) : "[Ordre ?]", ValveSeconds, cl.Goals.Count));
                return true;
            }
            return false;
        }

        // Après Hire (WrapPrepare) : l'état change (Blueprint attribué), recalcul au prochain appel même dans la frame.
        public static void OnHired(BuildGoal goal)
        {
            Plan plan;
            if (goal != null && goal.Provider != null && Plans.TryGetValue(goal.Provider, out plan)) plan.Dirty = true;
        }

        // Ret de BuildGoal.PriorityFor : bonus « intérieur d'abord ».
        public static int AfterPriority(int result, BuildGoal goal, Labor labor, ref float time)
        {
            try
            {
                if (!PlanEnabled || goal == null || goal.IsAnti || goal.Provider == null) return result;
                Plan plan = GetPlan(goal);
                Cluster cl;
                int idx;
                if (plan == null || !plan.Where.TryGetValue(goal, out cl) || !cl.HasCore || !cl.Index.TryGetValue(goal, out idx)) return result;
                return result + cl.Core.Infos[idx].Bonus;
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
                Cluster cl;
                int idx;
                if (!Plans.TryGetValue(goal.Provider, out plan) || !plan.Where.TryGetValue(goal, out cl) || !cl.HasCore
                    || !cl.Index.TryGetValue(goal, out idx)) return " ; accès ?";
                PlanGoal info = cl.Core.Infos[idx];
                string why = null;
                bool ok = true;
                if (cl.Core.IsCached(idx)) ok = cl.Core.Allowed(idx, out why);
                return " ; accès " + (info.Reach ? "oui" : "NON") + (info.Excluded ? " (non constructible : " + info.Reason + ")"
                    : !ok ? " (refusé : " + why + ")" : "");
            }
            catch { return ""; }
        }

        // Brictron vu (IsMatchFor) : registre sans FindObjectsOfType.
        public static void SeeLabor(Labor labor)
        {
            if (labor == null) return;
            ConnectSignal();
            KnownLabors.Add(labor);
        }

        // --- Plan par projet ---

        private static Plan GetPlan(BuildGoal goal)
        {
            BuildGoalProvider provider = goal.Provider;
            Plan plan;
            if (!Plans.TryGetValue(provider, out plan))
            {
                if (Plans.Count > 32) { Plans.Clear(); Shapes.Clear(); }
                plan = new Plan();
                Plans[provider] = plan;
            }
            int frame = UnityEngine.Time.frameCount;
            if (plan.Frame == frame && !plan.Dirty) return plan;
            bool newFrame = plan.Frame != frame;
            plan.Frame = frame;
            try
            {
                NewBudgetFrame();
                long sig = Collect(provider, goal);
                float now = UnityEngine.Time.time;
                if (sig != plan.Signature || plan.Dirty || now - plan.Time >= Refresh)
                {
                    plan.Dirty = false;
                    plan.Signature = sig;
                    plan.Time = now;
                    Recluster(plan, now);
                }
                bool computed = false;
                foreach (Cluster cl in plan.Clusters)
                {
                    if (cl.TooLarge) continue;
                    if (!cl.Ready)
                    {
                        // Lecture initiale : en une fois sous SliceCells, sinon par tranches (budget partagé par frame).
                        long t0 = Stopwatch.GetTimestamp();
                        double left = cl.Core.Grid.N <= SliceCells ? double.MaxValue
                            : ReadBudgetMs - _readTicks * 1000.0 / Stopwatch.Frequency;
                        if (left <= 0) continue;
                        cl.BuildCursor = ReadColumns(cl.Core.Grid, cl.BuildCursor, left);
                        _readTicks += Stopwatch.GetTimestamp() - t0;
                        if (cl.BuildCursor < cl.Core.Grid.Nx * cl.Core.Grid.Nz) continue;
                        cl.Ready = true;
                        cl.BuildCursor = 0;
                        cl.NeedsCompute = true;
                    }
                    else if (newFrame && !cl.NeedsCompute)
                        RefreshSlice(cl);
                    if (cl.NeedsCompute)
                    {
                        Compute(plan, cl);
                        computed = true;
                    }
                }
                if (newFrame) UpdatePeople(plan);
                if (computed) LogPlan(plan);
            }
            catch (Exception e)
            {
                Hooks.LogError(e);
                foreach (Cluster cl in plan.Clusters) cl.TooLarge = true;
            }
            return plan;
        }

        // Blueprints commités du projet (constructibles et futurs) dans Scratch ; signature (liste + attribués).
        private static long Collect(BuildGoalProvider provider, BuildGoal asked)
        {
            Scratch.Clear();
            bool seen = false;
            foreach (Blueprint b in provider.TrackedBlueprints)
            {
                if (b == null || b.IsNullOrReleased()) continue;
                BuildGoal g = b.GetComponent<BuildGoal>();
                if (g == null || g.IsAnti || !g.Commited || g.volume == null) continue;
                Scratch.Add(g);
                if (g == asked) seen = true;
            }
            if (!seen && asked != null && asked.Commited && asked.volume != null) Scratch.Add(asked);
            long h = 17;
            for (int i = 0; i < Scratch.Count; i++)
            {
                BuildGoal g = Scratch[i];
                int id = g.GetInstanceID();
                h = h * 31 + id;
                if (Hooks.HasLiveWorker(g)) h = h * 7 + id;
            }
            return h * 31 + Scratch.Count;
        }

        // Groupes : Blueprints dont les emprises (avec les marges de la grille) se touchent (union-find).
        // Grille d'un ancien groupe réutilisée si elle contient la nouvelle emprise.
        private static void Recluster(Plan plan, float now)
        {
            int n = Scratch.Count;
            if (_parent.Length < n) { _parent = new int[n + 32]; _next = new int[n + 32]; _head = new int[n + 32]; }
            for (int i = 0; i < n; i++) { _parent[i] = i; ShapeOf(Scratch[i]); }
            for (int i = 0; i < n; i++)
            {
                Shape a = Shapes[Scratch[i]];
                for (int j = i + 1; j < n; j++)
                {
                    Shape b = Shapes[Scratch[j]];
                    if (a.X0 - Margin <= b.X1 + Margin && b.X0 - Margin <= a.X1 + Margin
                        && a.Y0 - MarginDown <= b.Y1 + MarginUp && b.Y0 - MarginDown <= a.Y1 + MarginUp
                        && a.Z0 - Margin <= b.Z1 + Margin && b.Z0 - Margin <= a.Z1 + Margin)
                    {
                        int ri = Find(i), rj = Find(j);
                        if (ri != rj) _parent[Math.Max(ri, rj)] = Math.Min(ri, rj);
                    }
                }
            }
            // Listes chaînées par racine (ordre d'origine conservé).
            for (int i = 0; i < n; i++) { _head[i] = -1; _next[i] = -1; }
            for (int i = n - 1; i >= 0; i--)
            {
                int r = Find(i);
                _next[i] = _head[r];
                _head[r] = i;
            }
            // Blueprints disparus (posés ou annulés) : cases relues tout de suite dans les grilles qui les contenaient.
            Gone.Clear();
            foreach (BuildGoal old in plan.Previous)
            {
                if (old == null || (!old.IsNullOrReleased() && Scratch.Contains(old))) continue;
                Cluster was;
                Shape s;
                if (plan.Where.TryGetValue(old, out was) && was.Ready && Shapes.TryGetValue(old, out s))
                {
                    RereadAround(was.Core.Grid, s);
                    int idx;
                    if (was.HasCore && was.Index.TryGetValue(old, out idx) && was.Core.Infos[idx].InFlight)
                    {
                        if (plan.Pending.Count >= 64) plan.Pending.RemoveAt(0);
                        plan.Pending.Add(new PendingPose { S = s, Time = now });
                    }
                }
                Gone.Add(old);
            }
            plan.Previous.Clear();
            plan.Previous.AddRange(Scratch);
            bool grown = false;
            for (int i = 0; i < n; i++)
                if (!plan.Known.Contains(Scratch[i].GetInstanceID())) { grown = true; break; }
            if (grown)
            {
                plan.Sticky.Clear();                 // nouveau Blueprint (escalier ajouté ?) : exclusions réévaluées
                if (plan.Known.Count > 8192) plan.Known.Clear();
                for (int i = 0; i < n; i++) plan.Known.Add(Scratch[i].GetInstanceID());
            }
            OldClusters.Clear();
            OldClusters.AddRange(plan.Clusters);
            foreach (Cluster c in OldClusters) c.Used = false;
            plan.Clusters.Clear();
            plan.Where.Clear();
            for (int r = 0; r < n; r++)
            {
                if (_head[r] < 0 || Find(r) != r) continue;
                int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue, maxZ = int.MinValue;
                long csig = 17;
                int count = 0;
                for (int i = _head[r]; i >= 0; i = _next[i])
                {
                    Shape s = Shapes[Scratch[i]];
                    if (s.X0 < minX) minX = s.X0; if (s.Y0 < minY) minY = s.Y0; if (s.Z0 < minZ) minZ = s.Z0;
                    if (s.X1 > maxX) maxX = s.X1; if (s.Y1 > maxY) maxY = s.Y1; if (s.Z1 > maxZ) maxZ = s.Z1;
                    int id = Scratch[i].GetInstanceID();
                    csig = csig * 31 + id;
                    if (Hooks.HasLiveWorker(Scratch[i])) csig = csig * 7 + id;
                    count++;
                }
                csig = csig * 31 + count;
                int x0 = minX - Margin, y0 = minY - MarginDown, z0 = minZ - Margin;
                int x1 = maxX + Margin, y1 = maxY + MarginUp, z1 = maxZ + Margin;
                Cluster cl = null;
                bool reuse = false;
                // Ancien groupe contenant tous les membres : plan gardé tel quel, ou mis à jour par voie rapide
                // (attribution d'un Blueprint testé, pose d'un attribué) ; sinon recalcul complet.
                foreach (Cluster c in OldClusters)
                {
                    if (c.Used || c.TooLarge || !c.HasCore) continue;
                    bool all = true;
                    for (int i = _head[r]; i >= 0 && all; i = _next[i]) if (!c.Index.ContainsKey(Scratch[i])) all = false;
                    if (all) { cl = c; break; }
                }
                if (cl != null && now - cl.Time < Refresh && !grown)
                {
                    if (csig == cl.Signature) reuse = true;
                    else if (TryFast(cl, r)) { reuse = true; cl.Fast++; }
                }
                if (cl == null)
                    foreach (Cluster c in OldClusters)
                        if (!c.Used && !c.TooLarge && x0 >= c.GridX0 && x1 <= c.GridX1 && y0 >= c.GridY0 && y1 <= c.GridY1 && z0 >= c.GridZ0 && z1 <= c.GridZ1)
                        { cl = c; break; }
                if (cl == null)
                {
                    if (FreeClusters.Count > 0) { cl = FreeClusters[FreeClusters.Count - 1]; FreeClusters.RemoveAt(FreeClusters.Count - 1); }
                    else cl = new Cluster();
                    long cells = (long)(x1 - x0 + 1) * (y1 - y0 + 1) * (z1 - z0 + 1);
                    cl.TooLarge = cells > MaxCells;
                    cl.HasCore = false;
                    cl.Ready = false;
                    cl.BuildCursor = 0;
                    cl.RefreshCursor = 0;
                    cl.Signature = long.MinValue;
                    cl.LastOk = now;
                    if (cl.TooLarge)
                        Hooks.LogLine(string.Format("{0} plan : zone trop grande ({1} cases), contrôle levé pour ce groupe", Hooks.OrderTagOf(Scratch[r]), cells));
                    else
                    {
                        cl.Core.Grid.Resize(x0, y0, z0, x1 - x0 + 1, y1 - y0 + 1, z1 - z0 + 1);
                        cl.GridX0 = x0; cl.GridY0 = y0; cl.GridZ0 = z0; cl.GridX1 = x1; cl.GridY1 = y1; cl.GridZ1 = z1;
                    }
                }
                cl.Used = true;
                cl.Goals.Clear();
                for (int i = _head[r]; i >= 0; i = _next[i]) { cl.Goals.Add(Scratch[i]); plan.Where[Scratch[i]] = cl; }
                if (!reuse) cl.NeedsCompute = true;
                cl.Signature = csig;
                plan.Clusters.Add(cl);
            }
            foreach (Cluster c in OldClusters)
                if (!c.Used) { c.Goals.Clear(); c.Index.Clear(); c.HasCore = false; FreeClusters.Add(c); }
            if (FreeClusters.Count > 16) FreeClusters.RemoveRange(16, FreeClusters.Count - 16);
            foreach (BuildGoal old in Gone) Shapes.Remove(old);
        }

        private static readonly List<BuildGoal> Gone = new List<BuildGoal>();
        private static readonly HashSet<BuildGoal> Members = new HashSet<BuildGoal>();
        private static readonly List<int> GoneIdx = new List<int>();
        private static readonly List<int> HiredIdx = new List<int>();

        // Voie rapide (BuildPlanCore.Hire / Placed) : seuls des attribués ont été posés (cases pleines dans le monde)
        // et/ou des Blueprints testés ont été attribués. Sinon (annulation, échec de tâche…) : recalcul complet.
        private static bool TryFast(Cluster cl, int r)
        {
            BuildPlanCore core = cl.Core;
            if (core.Disabled) return false;
            Members.Clear();
            for (int i = _head[r]; i >= 0; i = _next[i]) Members.Add(Scratch[i]);
            GoneIdx.Clear();
            foreach (BuildGoal old in cl.Goals)
            {
                if (Members.Contains(old)) continue;
                int idx;
                Shape s;
                if (!cl.Index.TryGetValue(old, out idx) || !core.Infos[idx].InFlight || core.Infos[idx].Placed) return false;
                if (!Shapes.TryGetValue(old, out s) || !BuiltInWorld(s)) return false;     // annulé : recalcul complet
                GoneIdx.Add(idx);
            }
            HiredIdx.Clear();
            for (int i = _head[r]; i >= 0; i = _next[i])
            {
                PlanGoal info = core.Infos[cl.Index[Scratch[i]]];
                bool live = Hooks.HasLiveWorker(Scratch[i]);
                if (info.InFlight && !live) return false;                                   // tâche finie sans pose
                if (!info.InFlight && live) HiredIdx.Add(info.Id);
            }
            foreach (int h in HiredIdx)
            {
                if (!core.CanFastHire(h)) return false;
                core.Hire(h);
            }
            if (GoneIdx.Count > 0)
            {
                core.Placed(GoneIdx);
                foreach (BuildGoal old in cl.Goals) if (!Members.Contains(old)) cl.Index.Remove(old);
            }
            // cl.Time inchangé : le recalcul périodique (Refresh) reste compté depuis le dernier calcul complet.
            return true;
        }

        // Toutes les cases du Blueprint sont pleines dans le monde (il a bien été posé).
        private static bool BuiltInWorld(Shape s)
        {
            if (s.Cells.Length == 0) return false;
            foreach (XYZ c in s.Cells) if (!Voxel.IsNonEmpty(c)) return false;
            return true;
        }

        private static int Find(int i)
        {
            while (_parent[i] != i) { _parent[i] = _parent[_parent[i]]; i = _parent[i]; }
            return i;
        }

        private static Shape ShapeOf(BuildGoal g)
        {
            Shape s;
            if (Shapes.TryGetValue(g, out s)) return s;
            if (Shapes.Count > 4096) Shapes.Clear();
            s = new Shape();
            var cells = g.volume.GetCases(VProperty.fantome);
            var nw = g.volume.GetCases(VProperty.notWallSupport);
            var access = g.volume.access;
            var support = g.volume.GetCases(VProperty.mainSupportReq);
            string key = g.AssetKey != null ? g.AssetKey.Name : g.name;
            s.Cells = new XYZ[cells.Count];
            s.Kind = new byte[cells.Count];
            s.MinY = int.MaxValue;
            s.X0 = s.Y0 = s.Z0 = int.MaxValue;
            s.X1 = s.Y1 = s.Z1 = int.MinValue;
            for (int i = 0; i < cells.Count; i++)
            {
                XYZ c = cells[i];
                s.Cells[i] = c;
                bool slope = false;
                for (int j = 0; j < nw.Count; j++) if (nw[j] == c) { slope = true; break; }
                s.Kind[i] = KindOf(key, slope);
                if (c.y < s.MinY) s.MinY = c.y;
                Extend(s, c);
            }
            if (s.MinY == int.MaxValue) s.MinY = (int)g.transform.position.y;
            s.Access = new XYZ[access.Count];
            for (int i = 0; i < access.Count; i++) { s.Access[i] = access[i]; Extend(s, access[i]); }
            if (s.X0 == int.MaxValue)
            {
                XYZ p = XYZ.FromVector3(g.transform.position);
                Extend(s, p);
            }
            s.Support = new XYZ[support.Count];
            for (int i = 0; i < support.Count; i++) s.Support[i] = support[i];
            ValidatorComponent vc = g.GetComponent<ValidatorComponent>();
            s.MaxMissing = vc != null ? vc.MaxMissingSupport : 0;
            string vt = "";
            try { if (vc != null && vc.validatorType != null) vt = vc.GetValidatorType.Name; } catch { vt = ""; }
            s.SupportAny = vt.IndexOf("Plank", StringComparison.Ordinal) >= 0 || vt.IndexOf("Beam", StringComparison.Ordinal) >= 0;
            Shapes[g] = s;
            return s;
        }

        private static void Extend(Shape s, XYZ p)
        {
            if (p.x < s.X0) s.X0 = p.x; if (p.y < s.Y0) s.Y0 = p.y; if (p.z < s.Z0) s.Z0 = p.z;
            if (p.x > s.X1) s.X1 = p.x; if (p.y > s.Y1) s.Y1 = p.y; if (p.z > s.Z1) s.Z1 = p.z;
        }

        // Nature d'une case posée vue par le plan : porte passable ; case d'escalier sans appui mural = pente ;
        // créneaux, poutres, colonnes… = pleins mais leur dessus n'est pas compté comme sol (forme incertaine).
        private static byte KindOf(string key, bool notWallSupport)
        {
            if (key == null) return PlanGrid.OFull;
            if (key.IndexOf("Door", StringComparison.OrdinalIgnoreCase) >= 0) return PlanGrid.OPass;
            if (notWallSupport && key.IndexOf("Stair", StringComparison.OrdinalIgnoreCase) >= 0) return PlanGrid.OSlope;
            if (key.IndexOf("Merlon", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Beam", StringComparison.OrdinalIgnoreCase) >= 0
                || key.IndexOf("Brace", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Column", StringComparison.OrdinalIgnoreCase) >= 0
                || key.IndexOf("Parapet", StringComparison.OrdinalIgnoreCase) >= 0 || key.IndexOf("Arch", StringComparison.OrdinalIgnoreCase) >= 0)
                return PlanGrid.ONoFloor;
            return PlanGrid.OFull;
        }

        // Lit les colonnes (x, z) à partir de `from` jusqu'à épuisement du budget ; rend la colonne suivante.
        private static int ReadColumns(PlanGrid grid, int from, double budgetMs)
        {
            long t0 = Stopwatch.GetTimestamp();
            long limit = budgetMs >= double.MaxValue / 2 ? long.MaxValue : (long)(budgetMs * Stopwatch.Frequency / 1000.0);
            int columns = grid.Nx * grid.Nz;
            TestPathfinding tp = TestPathfinding.Instance;
            int col = from;
            while (col < columns)
            {
                int x = col / grid.Nz, z = col % grid.Nz;
                int b = (x * grid.Nz + z) * grid.Ny;
                for (int y = 0; y < grid.Ny; y++) grid.Cell[b + y] = ReadCell(tp, grid.X0 + x, grid.Y0 + y, grid.Z0 + z);
                col++;
                if ((col & 15) == 0 && Stopwatch.GetTimestamp() - t0 > limit) break;
            }
            return col;
        }

        private static byte ReadCell(TestPathfinding tp, int x, int y, int z)
        {
            var p = new XYZ(x, y, z);
            byte v = 0;
            if (Voxel.IsNonEmpty(p)) { v |= PlanGrid.NonEmpty; if (Voxel.IsFull(p)) v |= PlanGrid.Full; }
            if (TestPathfinding.IsNatureBlocker(p)) v |= PlanGrid.Nature;
            if (tp != null && tp.IsAWalkableObstacle(p)) v |= PlanGrid.Walkable;
            return v;
        }

        // Relecture continue de la grille (blocs posés, terrain creusé, arbres) par petites tranches.
        private static void RefreshSlice(Cluster cl)
        {
            PlanGrid grid = cl.Core.Grid;
            int columns = grid.Nx * grid.Nz;
            int next = ReadColumns(grid, cl.RefreshCursor, RefreshBudgetMs);
            cl.RefreshCursor = next >= columns ? 0 : next;
        }

        // Cases d'un Blueprint posé (et voisines verticales) relues dans le monde.
        private static void RereadAround(PlanGrid grid, Shape s)
        {
            TestPathfinding tp = TestPathfinding.Instance;
            foreach (XYZ c in s.Cells)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int i = grid.IndexOf(c.x, c.y + dy, c.z);
                    if (i >= 0) grid.Cell[i] = ReadCell(tp, c.x, c.y + dy, c.z);
                }
        }

        private static void Compute(Plan plan, Cluster cl)
        {
            int gc0 = GC.CollectionCount(0);
            long t0 = Stopwatch.GetTimestamp();
            BuildPlanCore core = cl.Core;
            PlanGrid grid = core.Grid;
            core.Reset();
            ApplyPending(plan, grid);
            cl.Index.Clear();
            for (int i = 0; i < cl.Goals.Count; i++)
            {
                BuildGoal g = cl.Goals[i];
                Shape s = ShapeOf(g);
                PlanGoal info = core.AddGoal(g.name, g);
                for (int k = 0; k < s.Cells.Length; k++)
                {
                    int c = grid.IndexOf(s.Cells[k].x, s.Cells[k].y, s.Cells[k].z);
                    if (c < 0) continue;
                    info.Cells.Add(c);
                    info.Kind.Add(s.Kind[k]);
                }
                foreach (XYZ a in s.Access) { int c = grid.IndexOf(a.x, a.y, a.z); if (c >= 0) info.Access.Add(c); }
                foreach (XYZ p in s.Support) { int c = grid.IndexOf(p.x, p.y, p.z); if (c >= 0) info.SupportCells.Add(c); }
                info.SupportAny = s.SupportAny;
                info.MaxMissing = s.MaxMissing;
                info.MinY = s.MinY;
                info.InFlight = Hooks.HasLiveWorker(g);
                string sticky;
                if (plan.Sticky.TryGetValue(g.GetInstanceID(), out sticky)) { info.Excluded = true; info.Reason = sticky; }
                cl.Index[g] = info.Id;
            }
            core.Compute();
            // v1.9.1 : seule l'exclusion « inaccessible » est retenue ; « sans support » est recalculé à chaque calcul.
            foreach (PlanGoal info in core.Infos)
                if (info.Excluded && info.Reason != BuildPlanCore.ReasonNoSupport)
                    plan.Sticky[((BuildGoal)info.Tag).GetInstanceID()] = info.Reason;
            cl.HasCore = true;
            cl.NeedsCompute = false;
            cl.Time = UnityEngine.Time.time;
            cl.LastOk = cl.Time;
            cl.Ms = (Stopwatch.GetTimestamp() - t0) * 1000.0 / Stopwatch.Frequency;
            cl.Gc = GC.CollectionCount(0) - gc0;
            UpdatePeople(cl);
        }

        // v1.9.1 : cases des Blueprints posés que les voxels ne montrent pas encore comptées comme posées ; dès que
        // le bloc est lisible (BuiltInWorld), ses cases sont relues et l'entrée retirée (au plus PendingSeconds).
        private static void ApplyPending(Plan plan, PlanGrid grid)
        {
            float now = UnityEngine.Time.time;
            for (int p = plan.Pending.Count - 1; p >= 0; p--)
            {
                PendingPose e = plan.Pending[p];
                if (now - e.Time > PendingSeconds || now < e.Time) { plan.Pending.RemoveAt(p); continue; }
                if (BuiltInWorld(e.S))
                {
                    RereadAround(grid, e.S);
                    plan.Pending.RemoveAt(p);
                    continue;
                }
                for (int k = 0; k < e.S.Cells.Length; k++)
                {
                    int i = grid.IndexOf(e.S.Cells[k].x, e.S.Cells[k].y, e.S.Cells[k].z);
                    if (i >= 0) grid.AssumePlaced(i, e.S.Kind[k]);
                }
            }
        }

        // --- Brictron ---

        // Registre : Brictron vus par IsMatchFor + signal Labor.LaborStarted ; morts filtrés à l'usage. Le signal est
        // rebranché quand le temps de jeu repart en arrière (nouvelle partie : GameSignals vide ses abonnés).
        private static readonly HashSet<Labor> KnownLabors = new HashSet<Labor>();
        private static readonly List<Labor> LaborScratch = new List<Labor>();
        private static readonly List<Labor> DeadScratch = new List<Labor>();
        private static float _signalTime = -1f;

        private static void ConnectSignal()
        {
            float now = UnityEngine.Time.time;
            if (_signalTime >= 0f && now >= _signalTime) { _signalTime = now; return; }
            if (now < _signalTime) KnownLabors.Clear();
            _signalTime = now;
            GameSignals.Connect(Labor.LaborStarted, (Labor l) => { if (l != null) KnownLabors.Add(l); });
        }

        private static List<Labor> Labors()
        {
            LaborScratch.Clear();
            DeadScratch.Clear();
            foreach (Labor l in KnownLabors)
            {
                if (l == null || l.IsNullOrReleased()) { DeadScratch.Add(l); continue; }
                if (l.enabled) LaborScratch.Add(l);
            }
            foreach (Labor l in DeadScratch) KnownLabors.Remove(l);
            return LaborScratch;
        }

        // Positions des Brictron (case debout dans la grille de chaque groupe), relues une fois par frame.
        private static void UpdatePeople(Plan plan)
        {
            foreach (Cluster cl in plan.Clusters) if (cl.HasCore) UpdatePeople(cl);
        }

        private static void UpdatePeople(Cluster cl)
        {
            BuildPlanCore core = cl.Core;
            core.People.Clear();
            foreach (Labor l in Labors())
            {
                int cell = StandCellOf(core.Grid, l.transform.position);
                if (cell >= 0) core.People.Add(cell);
            }
        }

        // Case debout d'un Brictron : sa case, sinon celle du dessous ou du dessus.
        private static int StandCellOf(PlanGrid grid, Vector3 position)
        {
            XYZ p = XYZ.FromVector3(position);
            for (int dy = 0; dy < 3; dy++)
            {
                int y = dy == 0 ? p.y : dy == 1 ? p.y - 1 : p.y + 1;
                int i = grid.IndexOf(p.x, y, p.z);
                if (i >= 0 && grid.StandAt(i)) return i;
            }
            return -1;
        }

        // Brictron inactif qu'une pose enfermerait (refus « enfermerait un Brictron ») : ordre de marcher vers la case
        // de bord reliée la plus proche. Un Brictron occupé finira sa tâche ; il sera revu au prochain refus.
        private static void Evacuate(Cluster cl)
        {
            BuildPlanCore core = cl.Core;
            if (core.Threatened.Count == 0) return;
            float now = UnityEngine.Time.time;
            if (now - cl.LastEvacuation < 1f) return;
            cl.LastEvacuation = now;
            PlanGrid grid = core.Grid;
            foreach (Labor l in Labors())
            {
                int cell = StandCellOf(grid, l.transform.position);
                if (cell < 0 || !core.Threatened.Contains(cell) || !IsIdle(l)) continue;
                float last;
                if (LastEvacuation.TryGetValue(l, out last) && now - last < EvacuateEvery) continue;
                int target = NearestBorder(core, cell);
                if (target < 0) continue;
                int x, y, z;
                grid.Position(target, out x, out y, out z);
                try
                {
                    Order.Move.WalkToward.Send(l, Location.Of(new XYZ(x, y, z)), Labor.DecisionSource.Project);
                    LastEvacuation[l] = now;
                    if (LastEvacuation.Count > 256) LastEvacuation.Clear();
                    Hooks.LogLine(string.Format("Évacuation : {0} serait enfermé, envoyé vers {1},{2},{3}", l.name, x, y, z));
                }
                catch (Exception e) { Hooks.LogError(e); }
            }
            core.Threatened.Clear();
        }

        // Case de bord reliée (état actuel) la plus proche de `from` (rare : allocations admises).
        private static int NearestBorder(BuildPlanCore core, int from)
        {
            PlanGrid g = core.Grid;
            var seen = new HashSet<int> { from };
            var queue = new Queue<int>();
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                int y = i % g.Ny, z = (i / g.Ny) % g.Nz, x = i / g.StrideX;
                if (core.BaseMarks[i] != 0 && (x == 0 || z == 0 || x == g.Nx - 1 || z == g.Nz - 1)) return i;
                for (int k = 0; k < 4; k++)
                {
                    int nx = x + (k == 0 ? 1 : k == 1 ? -1 : 0), nz = z + (k == 2 ? 1 : k == 3 ? -1 : 0);
                    if (nx < 0 || nz < 0 || nx >= g.Nx || nz >= g.Nz) continue;
                    int n = g.Index(nx, y, nz);
                    for (int dy = 0; dy < 3; dy++)
                    {
                        if ((dy == 1 && y == 0) || (dy == 2 && y + 1 >= g.Ny)) continue;
                        int m = dy == 0 ? n : dy == 1 ? n - 1 : n + 1;
                        if (core.BaseMarks[m] != 0 && seen.Add(m)) { queue.Enqueue(m); break; }
                    }
                }
            }
            return -1;
        }

        private static bool IsIdle(Labor l)
        {
            if (l.PendingTask != null) return false;
            if (!(l.Project == null || l.Project is IdleProject)) return false;
            Brix.Game.AI.Task t = l.CurrentTask;
            return t == null || t.Giver == null;
        }

        // --- Journal ---

        private static void LogPlan(Plan plan)
        {
            if (!Hooks.OrdersLogged || plan.Clusters.Count == 0 || plan.Previous.Count == 0) return;
            int total = 0, inFlight = 0, unreachable = 0, excluded = 0, cells = 0, peels = 0, steps = 0, floods = 0, gc = 0, fast = 0;
            double ms = 0;
            var names = new List<string>();
            foreach (Cluster cl in plan.Clusters)
            {
                if (!cl.HasCore) continue;
                BuildPlanCore core = cl.Core;
                foreach (PlanGoal i in core.Infos)
                {
                    total++;
                    if (i.Placed) continue;
                    if (i.InFlight) inFlight++;
                    if (i.Excluded) { excluded++; if (names.Count < 12) names.Add(((BuildGoal)i.Tag).name + " (" + i.Reason + ")"); }
                    else if (!i.Reach && !i.InFlight) unreachable++;
                }
                cells += core.Grid.N;
                ms = Math.Max(ms, cl.Ms);
                gc += cl.Gc;
                peels += (int)core.Peels; steps += (int)core.PeelSteps; floods += (int)core.Grid.Floods;
                fast += cl.Fast;
                cl.Fast = 0;
                core.Peels = core.PeelSteps = 0;
                core.Grid.ResetCost();
                cl.Gc = 0;
            }
            string tag = Hooks.OrderTagOf(plan.Previous[0]);
            string ex = string.Join(", ", names.ToArray());
            if (excluded > 0 && ex != plan.LoggedExcluded)
                Hooks.LogLine(string.Format("{0} NON CONSTRUCTIBLE sans escalier ou appui : {1} Blueprint(s) : {2}", tag, excluded, ex));
            plan.LoggedExcluded = excluded > 0 ? ex : null;
            string state = string.Format("{0} Blueprint(s) en {1} groupe(s), {2} attribué(s), {3} sans accès pour l'instant, {4} non constructible(s)",
                total, plan.Clusters.Count, inFlight, unreachable, excluded);
            // Calcul lent ou GC pendant le calcul : toujours journalisé (surveillance des gels).
            if (state == plan.LoggedState && ms < 5 && gc == 0) return;
            plan.LoggedState = state;
            Hooks.LogLine(string.Format("{0} plan : {1} ; {2} cases, {3:0.0} ms (pelages {4}, étapes {5}, parcours {6}, voies rapides {7}){8}",
                tag, state, cells, ms, peels, steps, floods, fast, gc > 0 ? " ; GC pendant le calcul" : ""));
        }
    }
}
