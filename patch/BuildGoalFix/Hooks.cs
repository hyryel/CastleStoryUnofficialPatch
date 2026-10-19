using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine;
using Task = Brix.Game.AI.Task;
using Path = System.IO.Path;

namespace CastleStory.BuildGoalFix
{
    // Bug : BuildGoal.IsMatchFor ne rejette un Goal déjà pris que pour les anti-blueprints.
    // Pour un bloc normal, tous les Brictron disponibles s'inscrivent sur le même Blueprint.
    // Correctif : limiter le nombre de workers d'un BuildGoal au nombre d'unités encore attendues
    // par son récepteur (minimum 1).
    // v1.2 : limite au niveau du projet (BuildGoalProvider) et borne du ramassage.
    // v1.3 : les Brictron qui portent encore des objets du projet entre deux tâches comptent dans la couverture ;
    //        un Brictron chargé passe avant les Brictron inscrits mais encore à vide ; journal par ordre.
    // v1.3.2 : porteur compté dès la pose (Blueprint transformé → BuildGoal absent/périmé) ; borne de ramassage
    //          plafonnée à la place du port ; sélection d'objet : sacs de Brictron exclus, objets réservés exclus,
    //          autre objet du même conteneur si le premier est réservé, objet réservé au ramassage.
    // v1.3.3 : porteur vide gardé tant que sa Task est en cours (il est enregistré avant de prendre) ;
    //          cible du ramassage remplacée en entrée de _GoPickUp si elle a été réservée entre-temps (cache par frame).
    // v1.3.4 : prise (Transaction.TakeFromEnumerable) réordonnée : ses objets réservés, puis libres, puis ceux des
    //          autres ; cible invalide (réservée par un autre, ou déjà dans un sac) → recherche fraîche.
    // v1.3.5 : objet déjà porté par le Brictron rendu tel quel (v1.3.4 le remplaçait → boucles d'échec hors chantier) ;
    //          E n'exclut que les sacs des autres Brictron (chercheur mémorisé par un hook en entrée de la recherche).
    // v1.3.6 : E n'admet un conteneur que s'il a encore un objet libre (ou réservé par le chercheur) de la ressource :
    //          la recherche saute les palettes épuisées au lieu d'échouer sur la plus proche.
    // v1.4 : journal unifié Logistique.log (Journal.cs, clé LogAll) : embauches de tous projets et stations, Tasks,
    //        ramassages/dépôts effectifs, garnottes, échanges, production, dépôt au sol, fin de Task, distances/odomètre.
    // v1.5 : réservation de capacité des stockages (Storage.cs, clé StorageReserve).
    // v1.8 : palettes : compléter un stockage entamé avant d'en entamer un autre (Storage.cs, clé CompleteFirst).
    // v1.7 : délais de réflexion raccourcis (clé FastThinking) : constantes de Task..ctor et de LookForWork remplacées
    //        par des appels (DelayOnFail, DelayOnFinished, LookForWorkTicks), Labor._tickOffset 2 → 1.
    // v1.6 : ordre de construction (BuildOrder.cs, clés BuildReach / BuildPlan) : Blueprint inaccessible ou dont la
    //        pose couperait l'accès d'un autre → non attribué ; bonus de priorité de l'intérieur vers l'extérieur.
    // v1.9 : ordre de construction global (BuildPlanCore.cs) : plan prouvé jusqu'à l'état final, Blueprints impossibles
    //        sans escalier signalés et jamais attribués, garde exacte à l'attribution ; calcul sans gel (voir BuildOrder.cs).
    // v1.9.2 : un Brictron inactif (Activity.Chilling) traverse les Brictron alliés comme un Brictron au travail
    //          (clé PassThroughIdle, hook T sur Builder.CanPassThrough).
    // Appelé depuis l'IL de Assembly-CSharp-firstpass, réécrit sur disque par BuildGoalFix.Patcher (install.sh).
    public static class Hooks
    {
        private static readonly string Dir = Path.GetDirectoryName(typeof(Hooks).Assembly.Location);
        private static readonly string LogPath = Path.Combine(Dir, "BuildGoalFix.log");

        // A : BuildGoal.IsMatchFor rejette (GoalTooCrowded) un Blueprint déjà couvert par assez de workers.
        private static readonly bool PatchMatch;
        // B : Project.GetPrepareFunction refait le test juste avant Hire (ferme la fenêtre de course).
        private static readonly bool PatchPrepare;
        // v1.2 : refuse un Brictron si les autres Brictron engagés couvrent déjà la demande restante du projet.
        private static readonly bool ProjectLimit;
        // v1.2 : borne le ramassage d'un Brictron à la demande restante du projet non couverte par les autres.
        private static readonly bool ClampPickupEnabled;
        // Journalise chaque rejet et chaque purge.
        private static readonly bool Verbose;
        // Journalise une fois la capacité (base/restante) de chaque type de Blueprint.
        private static readonly bool LogCapacity;
        // Journalise par ordre (projet) : composition, embauches, ramassages, fins de tâche.
        private static readonly bool LogOrders;
        // v1.3.2 : sélection d'objet à ramasser (sacs de Brictron et objets réservés exclus, réservation de l'objet).
        private static readonly bool PickupFilter;
        // v1.4 : journal unifié (Journal.cs).
        private static readonly bool LogAll;
        // v1.5 : réservation de capacité des stockages (Storage.cs).
        private static readonly bool StorageReserve;
        // v1.6 : garde d'accessibilité / non-coupure et plan de construction (BuildOrder.cs).
        private static readonly bool BuildReach;
        private static readonly bool BuildPlan;
        // v1.7 : délais de réflexion raccourcis (pause après échec / succès, cycle LookForWork, décalage anti-rafale).
        private static readonly bool FastThinking;
        // v1.8 : palettes, compléter avant d'entamer (Storage.cs).
        private static readonly bool CompleteFirst;
        // v1.9.2 : Brictron inactifs traversent les alliés (Builder.CanPassThrough).
        private static readonly bool PassThroughIdle;

        private static readonly FieldInfo WorkersField =
            typeof(Goal).GetField("workers", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool _errorLogged;

        static Hooks()
        {
            var cfg = LoadConfig(Path.Combine(Dir, "BuildGoalFix.cfg"));
            PatchMatch = cfg["FilterIsMatchFor"];
            PatchPrepare = cfg["GuardPrepare"];
            ProjectLimit = cfg["ProjectLimit"];
            ClampPickupEnabled = cfg["ClampPickup"];
            Verbose = cfg["Verbose"];
            LogCapacity = cfg["LogCapacity"];
            LogOrders = cfg["LogOrders"];
            PickupFilter = cfg["PickupFilter"];
            LogAll = cfg["LogAll"];
            StorageReserve = cfg["StorageReserve"];
            BuildReach = cfg["BuildReach"];
            BuildPlan = cfg["BuildPlan"];
            FastThinking = cfg["FastThinking"];
            CompleteFirst = cfg["CompleteFirst"];
            PassThroughIdle = cfg["PassThroughIdle"];
            Storage.CompleteFirst = CompleteFirst;
            if (FastThinking) SetTickOffset(1);
            Journal.Enabled = LogAll;
            Storage.Enabled = StorageReserve;
            BuildOrder.ReachEnabled = BuildReach;
            BuildOrder.PlanEnabled = BuildPlan;
            Log(string.Format("Hooks v1.9.2 chargés : FilterIsMatchFor={0} GuardPrepare={1} ProjectLimit={2} ClampPickup={3} Verbose={4} LogCapacity={5} LogOrders={6} PickupFilter={7} LogAll={8} StorageReserve={9} BuildReach={10} BuildPlan={11} FastThinking={12} CompleteFirst={13} PassThroughIdle={14}",
                PatchMatch, PatchPrepare, ProjectLimit, ClampPickupEnabled, Verbose, LogCapacity, LogOrders, PickupFilter, LogAll, StorageReserve, BuildReach, BuildPlan, FastThinking, CompleteFirst, PassThroughIdle));
        }

        // --- v1.7 : délais de réflexion (ticks de 20 ms) ; appelés depuis l'IL à la place des constantes vanilla ---
        // Task..ctor : pause après une Task en échec (vanilla 30 = 0,6 s) et après une Task réussie (vanilla 10).
        public static int DelayOnFail() { return FastThinking ? 8 : 30; }
        public static int DelayOnFinished() { return FastThinking ? 4 : 10; }
        // Labor.LookForWorkSequence (2 attentes) et période de la séquence dans Labor.Initialize (vanilla 10).
        public static int LookForWorkTicks() { return FastThinking ? 4 : 10; }

        // --- v1.9.2 : T, sur chaque ret de Builder.CanPassThrough (appelé à chaque recherche de chemin et à chaque pas bloqué) ---
        // Vanilla : Activity != Fighting && Activity != Chilling && (IsWorker() || hackedDigGoal libéré). Un Brictron qui flâne
        // (Chilling : errance, assis au ralliement) ne traverse donc pas les alliés et échoue face à un Brictron immobile.
        // Ici, Chilling traverse aussi ; le combat (Fighting) et les ennemis/neutres (test d'alliance de VoxelPresence) inchangés.
        public static bool AfterCanPassThrough(bool result, Labor labor)
        {
            return result || (PassThroughIdle && labor != null && labor.Activity == Activity.Chilling);
        }

        // Labor._tickOffset (vanilla 2) : décalage entre Brictron qui cherchent du travail au même tick
        // (OffsetWaitTime = (n % 8) * _tickOffset + 1, soit 1 à 15 ticks en vanilla, 1 à 8 avec 1).
        private static void SetTickOffset(int value)
        {
            try
            {
                FieldInfo f = typeof(Labor).GetField("_tickOffset", BindingFlags.Static | BindingFlags.NonPublic);
                if (f != null) f.SetValue(null, value);
            }
            catch (Exception e) { Log("Réglage _tickOffset impossible : " + e.Message); }
        }

        // Accès pour BuildOrder.
        internal static bool OrdersLogged { get { return LogOrders; } }
        internal static void LogLine(string msg) { Log(msg); }
        internal static string OrderTagOf(BuildGoal goal) { return OrderTag(goal); }
        internal static List<BuildGoal> ProjectGoalsOf(BuildGoal goal) { return ProjectGoals(goal); }
        // Vrai si un Brictron a une Task vivante (en cours ou en attente) sur ce Blueprint.
        internal static bool HasLiveWorker(BuildGoal goal)
        {
            var workers = WorkersField.GetValue(goal) as List<Labor>;
            if (workers == null) return false;
            foreach (Labor w in workers)
                if (w != null && !w.IsNullOrReleased() && IsLive(w, goal.gameObject)) return true;
            return false;
        }

        // Force le constructeur statique (config) depuis une autre classe de hooks appelée avant Hooks.
        internal static void EnsureLoaded() { }

        public static bool AfterIsMatchFor(bool result, BuildGoal goal, Labor labor, AIErrorsStore errorDestination)
        {
            try
            {
                if (LogCapacity) LogCapacityOnce(goal);
                if (BuildReach) BuildOrder.SeeLabor(labor);      // v1.9 : registre des Brictron sans FindObjectsOfType
                if (!result) return result;
                // v1.6 : Blueprint inaccessible, ou dont la pose couperait l'accès d'un autre.
                string why;
                if (BuildReach && !BuildOrder.Allowed(goal, out why))
                {
                    if (Verbose) Log(string.Format("{0} {1} refusé à {2} : {3}", OrderTag(goal), goal.name, labor != null ? labor.name : "?", why));
                    errorDestination.AddDecisionError(SharedWorkException<Exceptions.CantReach>.Get.Problem, labor);
                    return false;
                }
                if (!PatchMatch) return result;
                // Filtre : stats projet mises en cache pour le pas fixe courant (IsMatchFor est appelé pour chaque Goal).
                if (!IsCrowded(goal, labor) && !(ProjectLimit && IsProjectCovered(goal, labor, true))) return result;
                errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
                return false;
            }
            catch (Exception e)
            {
                LogError(e);
                return result;
            }
        }

        public static Func<BaseLabor, bool> WrapPrepare(Func<BaseLabor, bool> original, Goal goal)
        {
            BuildGoal build = goal as BuildGoal;
            bool guard = build != null && (PatchPrepare || BuildReach);
            if (!guard && !LogAll) return original;
            return labor =>
            {
                bool crowded = false;
                if (guard)
                {
                    try
                    {
                        // Test atomique avant Hire : stats projet recalculées (d'autres Brictron ont pu être
                        // embauchés plus tôt dans le même pas fixe).
                        string why;
                        crowded = (BuildReach && !BuildOrder.Allowed(build, out why))
                            || (PatchPrepare && (IsCrowded(build, labor as Labor)
                                || (ProjectLimit && IsProjectCovered(build, labor as Labor, false))));
                    }
                    catch (Exception e) { LogError(e); crowded = false; }
                    if (crowded) return false;
                }
                // v1.4 : le Goal est connu ici, Project.Hire (hook J1) le lit pour le journal unifié.
                Journal.CurrentGoal = goal;
                bool hired;
                try { hired = original(labor); }
                finally { Journal.CurrentGoal = null; }
                if (hired && build != null && BuildReach) BuildOrder.OnHired(build);   // v1.9 : état du plan changé
                if (hired && build != null && LogOrders) LogHire(build, labor as Labor);
                return hired;
            };
        }

        // Entrée de Goal.EndWork : journal de fin de tâche sur un BuildGoal (succès/échec, ce qui reste, ce qui est porté).
        public static void OnEndWork(Goal goal, Task task)
        {
            try
            {
                if (!LogOrders) return;
                BuildGoal build = goal as BuildGoal;
                if (build == null || task == null) return;
                Labor labor = task.Worker as Labor;
                string outcome = task.result.ToString() + FailureDetail(task);
                if (build.IsAnti)
                {
                    Log(string.Format("{0} {1} (démolition) : {2} fin de tâche {3}",
                        OrderTag(build), build.name, labor != null ? labor.name : "?", outcome));
                    return;
                }
                ProjectStats stats = ComputeStats(build);
                if (task.result != TaskResult.Succes) outcome += BuildOrder.Describe(build);
                Log(string.Format("{0} {1} : {2} fin de tâche {3} ; reste {4} ; porte {5}",
                    OrderTag(build), build.name, labor != null ? labor.name : "?", outcome,
                    build.recepteur != null ? Describe(build.recepteur.CurrentCapacity) : "?",
                    labor != null && labor.recepteur != null ? Describe(labor.recepteur.ContentDescription) : "?")
                    + " ; projet " + stats.Summary(null));
            }
            catch (Exception e) { LogError(e); }
        }

        // Entrée de PickUpNode.TakeSome : borne le ramassage d'un Brictron qui travaille pour un BuildGoal
        // à ce que le projet attend encore, moins ce que les autres Brictron engagés couvrent déjà.
        public static int ClampPickup(Labor labor, GameComponent item, int upTo)
        {
            try
            {
                if (!ClampPickupEnabled || labor == null || item == null) return upTo;
                Task task = labor.CurrentTask;
                GameObject giver = task != null ? task.Giver : null;
                BuildGoal goal = giver != null ? giver.GetComponent<BuildGoal>() : null;
                if (goal == null || goal.IsAnti) return upTo;
                Ressource res = Apparence.PrincipaleOf(item.gameObject);
                if (res == null) return upTo;
                Type type = res.GetType();
                RegisterCarrier(labor, goal);
                ProjectStats stats = ComputeStats(goal);
                int limit = Math.Max(1, stats.Remaining(type) - stats.CoverageExcluding(type, labor));
                // v1.3.2 : jamais plus que la place du port pour ce type (PlankBlock 3, StoneBlock 1…).
                int fit = labor.recepteur != null ? labor.recepteur.CanFitUpTo(item.AssetKey) : 0;
                if (fit > 0 && fit < limit) limit = fit;
                int result = upTo >= 0 && upTo <= limit ? upTo : limit;
                if (LogOrders || (Verbose && result != upTo))
                    Log(string.Format("{0} {1} ramasse {2} pour {3} : borne {4} (demandé {5}, port {6}) ; projet {7}",
                        OrderTag(goal), labor.name, type.Name, goal.name, result, upTo, fit, stats.Summary(labor)));
                return result;
            }
            catch (Exception e)
            {
                LogError(e);
                return upTo;
            }
        }

        // --- v1.3.2 : sélection de l'objet à ramasser ---

        // Fin de Knowledge._ClosestObjectToPickup_Test : exclut les sacs de Brictron (le ramassage y échoue toujours,
        // ObjectIsInSomeoneElseBag) et les objets isolés déjà réservés par un autre Brictron. Les conteneurs
        // (stock, atelier) réservés restent admis : leur réservation vanilla dure toute la tâche du premier venu.
        // Brictron dont la recherche est en cours (entrée de Knowledge.ClosestObjectToPickup, hook I) : son propre
        // sac reste admis, comme en vanilla (il utilise ce qu'il porte au lieu d'aller au stock).
        private static Labor _searcher;

        public static void BeginSearch(Labor source)
        {
            _searcher = source;
        }

        public static bool AfterPickupTest(bool result, IGameComponent gc, Ressource res)
        {
            try
            {
                if (!result || !PickupFilter || gc == null) return result;
                Recepteur r = gc.recepteur;
                Labor self = _searcher != null && !_searcher.IsNullOrReleased() ? _searcher : null;
                if (r != null && r.Fonction == FonctionDeContenant.personnage)
                    return self != null && gc.GameObject == self.gameObject;
                // v1.3.6 : conteneur admis seulement s'il reste un objet de la ressource libre (ou réservé par le chercheur).
                if (r != null && res != null && r.Carries(res)) return FirstFreeSibling(null, r, res, self) != null;
                if (r == null && Knowledge.Instance != null && Knowledge.Instance.IsReserved(gc.GameObject)) return false;
                return result;
            }
            catch (Exception e) { LogError(e); return result; }
        }

        // Fin de Labor.FindOneResource : vanilla prend toujours le premier objet du conteneur, donc tous les
        // Brictron envoyés au même stock visent le même objet et un seul l'obtient (ObjectHasBeenMovedFromRecepteur).
        // Si cet objet est réservé, on prend le premier objet non réservé du même conteneur ; aucun → null
        // (échec immédiat CannotFindWhatINeedToPickUp, sans déplacement).
        public static GameObject AfterFindOneResource(GameObject result, Labor labor, Ressource res)
        {
            try
            {
                if (!PickupFilter || result == null || res == null || Knowledge.Instance == null) return result;
                if (!Knowledge.Instance.IsReserved(result)) return result;
                GameComponent gc = result.GetComponent<GameComponent>();
                Recepteur parent = gc != null ? gc.ParentRecepteur : null;
                if (parent == null || parent.IsNullOrReleased()) return result;
                GameObject other = FirstFreeSibling(result, parent, res, labor);
                if (Verbose)
                    Log(string.Format("{0} : {1} réservé dans {2}, cible {3}", labor != null ? labor.name : "?",
                        result.name, parent.name, other != null ? other.name : "aucune"));
                return other;
            }
            catch (Exception e) { LogError(e); return result; }
        }

        // Objets réservés par les hooks (G) : pour reconnaître sa propre réservation. Entrées périmées sans effet
        // (IsReserved fait foi).
        private static readonly Dictionary<GameObject, Labor> ReservedBy = new Dictionary<GameObject, Labor>();

        // Entrée de PickUpNode._GoPickUp (item = Hooks.OnGoPickUp(labor, item)) : la cible vient d'un cache par
        // frame de décision (Labor.Find), trouvée à l'évaluation du Goal, avant l'embauche. Si un autre Brictron l'a
        // réservée entre-temps : premier objet non réservé du même conteneur (null → CannotFindWhatINeedToPickUp,
        // sans déplacement). Puis réserve l'objet lui-même (vanilla ne réserve que son conteneur) jusqu'à WorkEnded.
        public static GameComponent OnGoPickUp(Labor labor, GameComponent item)
        {
            try
            {
                if (!PickupFilter || labor == null || item == null || item.IsNullOrReleased() || labor.signal == null) return item;
                Knowledge k = Knowledge.Instance;
                if (k == null) return item;
                GameObject go = item.gameObject;
                Recepteur parent = item.ParentRecepteur;
                // v1.3.5 : déjà porté par ce Brictron → vanilla répond Node.Empty juste après ; ne rien changer
                // (v1.3.4 remplaçait la cible → échec et ré-embauche en boucle sur les flux transport/artisanat).
                if ((labor.recepteur != null && (parent == labor.recepteur || labor.recepteur.Carries(item)))) return item;
                Labor owner;
                bool reservedByOther = k.IsReserved(go) && !(ReservedBy.TryGetValue(go, out owner) && owner == labor);
                bool inBag = parent != null && !parent.IsNullOrReleased() && parent.Fonction == FonctionDeContenant.personnage;
                if (reservedByOther || inBag)
                {
                    // v1.3.4 : recherche fraîche (filtre E appliqué) au lieu d'énumérer le parent courant, qui peut
                    // être le sac du Brictron qui vient de ramasser l'objet (v1.3.3 : ObjectIsInSomeoneElseBag).
                    Ressource res = Apparence.PrincipaleOf(go);
                    GameObject other = null;
                    if (res != null)
                    {
                        IGameComponent found = k.ClosestObjectToPickup(labor, res, labor.faction);
                        if (found != null && !found.IsNullOrReleased())
                            other = found.recepteur != null ? FirstFreeSibling(null, found.recepteur, res, labor) : found.GameObject;
                    }
                    if (Verbose)
                        Log(string.Format("{0} : {1} {2}, cible {3}", labor.name, go.name,
                            inBag ? "déjà dans le sac de " + parent.name : "réservé depuis la décision",
                            other != null ? other.name : "aucune"));
                    if (other == null) return null;
                    go = other;
                    item = other.GetComponent<GameComponent>();
                    if (item == null) return null;
                }
                k.ReserveUntil(go, labor.signal.WorkEnded);
                if (ReservedBy.Count > 256) ReservedBy.Clear();
                ReservedBy[go] = labor;
                return item;
            }
            catch (Exception e) { LogError(e); return item; }
        }

        // Entrée de Transaction.TakeFromEnumerable (enumerable = Hooks.OrderPickup(destination, enumerable)) :
        // vanilla prend les `upTo` premiers objets de même clé du conteneur, pas l'objet désigné, et vole donc
        // les objets réservés par d'autres (ObjectHasBeenMovedFromRecepteur). Ordre : réservés par ce Brictron,
        // puis libres, puis réservés par d'autres.
        public static ICollection<GameObject> OrderPickup(Recepteur destination, ICollection<GameObject> items)
        {
            try
            {
                if (!PickupFilter || destination == null || items == null || items.Count < 2 || Knowledge.Instance == null) return items;
                Labor labor = destination.GetComponent<Labor>();
                if (labor == null) return items;
                var mine = new List<GameObject>(); var free = new List<GameObject>(); var others = new List<GameObject>();
                foreach (GameObject go in items)
                {
                    if (go == null) continue;
                    Labor owner;
                    if (!Knowledge.Instance.IsReserved(go)) free.Add(go);
                    else if (ReservedBy.TryGetValue(go, out owner) && owner == labor) mine.Add(go);
                    else others.Add(go);
                }
                if (others.Count == 0 && mine.Count == 0) return items;
                var ordered = new List<GameObject>(items.Count);
                ordered.AddRange(mine); ordered.AddRange(free); ordered.AddRange(others);
                return ordered;
            }
            catch (Exception e) { LogError(e); return items; }
        }

        // Premier objet du conteneur, autre que `except`, de même ressource principale, non réservé
        // (ou réservé par `self`, v1.3.6).
        private static GameObject FirstFreeSibling(GameObject except, Recepteur parent, Ressource res, Labor self)
        {
            if (parent == null || parent.IsNullOrReleased() || res == null) return null;
            foreach (IDescriptor d in parent.StoredItems)
            {
                GameObject go = d != null ? d.GameObject : null;
                if (go == null || go == except || !Apparence.PrincipaleIs(go, res)) continue;
                Labor owner;
                if (Knowledge.Instance.IsReserved(go) && !(self != null && ReservedBy.TryGetValue(go, out owner) && owner == self)) continue;
                return go;
            }
            return null;
        }

        private static readonly FieldInfo TaskLogsField =
            typeof(Task).GetField("Logs", BindingFlags.Instance | BindingFlags.NonPublic);

        // Raison d'un échec : FailException de la Task si posée, sinon dernière entrée de son journal interne.
        internal static string FailureDetail(Task task)
        {
            if (task.result == TaskResult.Succes) return "";
            if (task.Exception != null) return " (" + task.Exception.GetType().Name + ")";
            var logs = TaskLogsField != null ? TaskLogsField.GetValue(task) as Queue<string> : null;
            if (logs == null || logs.Count == 0) return "";
            string last = null;
            foreach (string l in logs) last = l;
            return " (" + last + ")";
        }

        // --- v1.2 : demande et couverture au niveau du projet (BuildGoalProvider) ---

        private sealed class ProjectStats
        {
            public readonly Dictionary<Type, int> RemainingByType = new Dictionary<Type, int>();
            public readonly Dictionary<Type, int> CoverageByType = new Dictionary<Type, int>();
            public readonly Dictionary<Labor, Dictionary<Type, int>> Contribution = new Dictionary<Labor, Dictionary<Type, int>>();

            public int Remaining(Type t) { int v; return RemainingByType.TryGetValue(t, out v) ? v : 0; }

            public int CoverageExcluding(Type t, Labor labor)
            {
                int total, own = 0;
                if (!CoverageByType.TryGetValue(t, out total)) return 0;
                Dictionary<Type, int> c;
                if (labor != null && Contribution.TryGetValue(labor, out c)) c.TryGetValue(t, out own);
                return total - own;
            }

            public void Add(Labor w, Type t, int c)
            {
                Dictionary<Type, int> contrib;
                if (!Contribution.TryGetValue(w, out contrib)) Contribution[w] = contrib = new Dictionary<Type, int>();
                if (contrib.ContainsKey(t)) return;
                contrib[t] = c;
                int cov;
                CoverageByType.TryGetValue(t, out cov);
                CoverageByType[t] = cov + c;
            }

            // « PlankBlock reste 2 couvert 3 [Hanjo=3] » ; exclude = Brictron retiré de la couverture affichée.
            public string Summary(Labor exclude)
            {
                var parts = new List<string>();
                foreach (var kv in RemainingByType)
                {
                    var who = new List<string>();
                    foreach (var c in Contribution)
                    {
                        int v;
                        if (c.Key != exclude && c.Value.TryGetValue(kv.Key, out v)) who.Add(c.Key.name + "=" + v);
                    }
                    parts.Add(string.Format("{0} reste {1} couvert {2} [{3}]", kv.Key.Name, kv.Value,
                        CoverageExcluding(kv.Key, exclude), string.Join(", ", who.ToArray())));
                }
                return parts.Count == 0 ? "rien à livrer" : string.Join(" ; ", parts.ToArray());
            }
        }

        // Brictron ayant ramassé pour un projet : leur charge compte dans la couverture même entre deux tâches
        // (sinon un autre Brictron est embauché pendant les ~10 ticks où le porteur n'est inscrit nulle part).
        private static readonly Dictionary<Labor, BuildGoalProvider> Carriers = new Dictionary<Labor, BuildGoalProvider>();

        private static void RegisterCarrier(Labor labor, BuildGoal goal)
        {
            if (goal.Provider != null) Carriers[labor] = goal.Provider;
        }

        // Vrai si labor porte déjà un objet que le goal attend encore.
        private static bool CarriesFor(Labor labor, BuildGoal goal)
        {
            if (labor == null || labor.recepteur == null || goal.recepteur == null) return false;
            foreach (Ressource res in goal.recepteur.CurrentCapacity.GetRessources().ToList())
                if (goal.recepteur.CurrentCapacity.Value(res) > 0 && labor.recepteur.ContentDescription.Value(res.GetType()) > 0)
                    return true;
            return false;
        }

        private static string OrderTag(BuildGoal goal)
        {
            return goal.Provider != null ? "[Ordre #" + goal.Provider.GetInstanceID() + "]" : "[Ordre ?]";
        }

        // Composition des ordres déjà journalisée (id provider → signature) pour ne la réécrire que si elle change.
        private static readonly Dictionary<int, string> LoggedCompositions = new Dictionary<int, string>();

        private static void LogComposition(BuildGoal goal, ProjectStats stats)
        {
            if (goal.Provider == null) return;
            var names = new List<string>();
            foreach (BuildGoal b in ProjectGoals(goal)) names.Add(b.name);
            names.Sort();
            string signature = names.Count + " Blueprint(s) : " + string.Join(", ", names.ToArray());
            int id = goal.Provider.GetInstanceID();
            string previous;
            if (LoggedCompositions.TryGetValue(id, out previous) && previous == signature) return;
            LoggedCompositions[id] = signature;
            Log(string.Format("{0} {1} ; {2}{3}", OrderTag(goal), signature, stats.Summary(null), LogAll ? Journal.Extent(ProjectGoals(goal)) : ""));
        }

        private static void LogHire(BuildGoal goal, Labor labor)
        {
            if (labor == null) return;
            if (goal.IsAnti)
            {
                Log(string.Format("{0} {1} embauché sur {2} (démolition)", OrderTag(goal), labor.name, goal.name));
                return;
            }
            ProjectStats stats = ComputeStats(goal);
            LogComposition(goal, stats);
            Log(string.Format("{0} {1} embauché sur {2} ; porte {3} ; projet {4}", OrderTag(goal), labor.name, goal.name,
                labor.recepteur != null ? Describe(labor.recepteur.ContentDescription) : "?", stats.Summary(labor)));
        }

        private static readonly Dictionary<BuildGoalProvider, KeyValuePair<float, ProjectStats>> StatsCache =
            new Dictionary<BuildGoalProvider, KeyValuePair<float, ProjectStats>>();

        // Vrai si, pour chaque ressource que le goal attend encore, les autres Brictron engagés sur le projet
        // (charge portée, ou place libre s'ils n'ont encore rien) couvrent déjà toute la demande restante du projet.
        // Jamais vrai si personne d'autre n'est engagé.
        private static bool IsProjectCovered(BuildGoal goal, Labor labor, bool cached)
        {
            if (goal == null || labor == null || goal.IsAnti || goal.recepteur == null) return false;
            // Un Brictron qui porte déjà ce que le goal attend livre toujours : sa charge réduit la demande des autres.
            if (CarriesFor(labor, goal)) return false;
            ProjectStats stats = cached ? CachedStats(goal) : ComputeStats(goal);
            bool any = false;
            foreach (Ressource res in goal.recepteur.CurrentCapacity.GetRessources().ToList())
            {
                if (goal.recepteur.CurrentCapacity.Value(res) <= 0) continue;
                Type t = res.GetType();
                int remaining = stats.Remaining(t), covered = stats.CoverageExcluding(t, labor);
                if (covered < remaining) return false;
                any = true;
            }
            if (any && Verbose && !cached)
                Log(string.Format("{0} refusé sur {1} : demande du projet déjà couverte", labor.name, goal.name));
            return any;
        }

        private static ProjectStats CachedStats(BuildGoal goal)
        {
            BuildGoalProvider provider = goal.Provider;
            if (provider == null) return ComputeStats(goal);
            KeyValuePair<float, ProjectStats> entry;
            if (StatsCache.TryGetValue(provider, out entry) && entry.Key == Time.fixedTime) return entry.Value;
            if (StatsCache.Count > 64) StatsCache.Clear();
            ProjectStats stats = ComputeStats(goal);
            StatsCache[provider] = new KeyValuePair<float, ProjectStats>(Time.fixedTime, stats);
            return stats;
        }

        private static ProjectStats ComputeStats(BuildGoal goal)
        {
            var stats = new ProjectStats();
            foreach (BuildGoal b in ProjectGoals(goal))
            {
                Recepteur r = b.recepteur;
                if (r == null) continue;
                var needed = new List<Type>();
                foreach (Ressource res in r.CurrentCapacity.GetRessources().ToList())
                {
                    int v = r.CurrentCapacity.Value(res);
                    if (v <= 0) continue;
                    Type t = res.GetType();
                    needed.Add(t);
                    stats.RemainingByType[t] = stats.Remaining(t) + v;
                }
                if (needed.Count == 0) continue;
                var workers = WorkersField.GetValue(b) as List<Labor>;
                if (workers == null) continue;
                foreach (Labor w in workers)
                {
                    if (w == null || w.IsNullOrReleased() || !IsLive(w, b.gameObject) || w.recepteur == null) continue;
                    foreach (Type t in needed)
                    {
                        int carried = w.recepteur.ContentDescription.Value(t);
                        stats.Add(w, t, carried > 0 ? carried : Math.Max(0, w.recepteur.CurrentCapacity.Value(t)));
                    }
                }
            }
            AddCarriers(goal.Provider, stats);
            return stats;
        }

        // Porteurs enregistrés pour ce projet mais inscrits sur aucun de ses goals (entre deux tâches) :
        // seule leur charge réelle compte. Ignorés s'ils travaillent ailleurs ; retirés une fois vides ou détruits.
        private static void AddCarriers(BuildGoalProvider provider, ProjectStats stats)
        {
            if (provider == null || Carriers.Count == 0) return;
            var gone = new List<Labor>();
            foreach (var kv in Carriers)
            {
                Labor c = kv.Key;
                if (c == null || c.IsNullOrReleased() || c.recepteur == null || kv.Value == null)
                {
                    gone.Add(c);
                    continue;
                }
                Task task = c.CurrentTask;
                bool running = task != null && task.Status != TaskProgression.Finished && task.Status != TaskProgression.Failed;
                // v1.3.3 : enregistré en entrée de TakeSome, donc encore vide à cet instant ; retiré seulement
                // s'il est vide sans Task en cours (v1.3.2 et antérieures le retiraient aussitôt : registre toujours vide).
                if (c.recepteur.IsEmpty())
                {
                    if (!running) gone.Add(c);
                    continue;
                }
                if (kv.Value != provider || stats.Contribution.ContainsKey(c) || !c.enabled) continue;
                // Ignoré seulement s'il a une Task encore en cours pour un BuildGoal vivant d'un autre projet.
                // Juste après une pose, la Task est encore en cours mais son Giver (Blueprint transformé) n'a plus
                // de BuildGoal valide (absent, libéré ou sans Provider) : le porteur compte (v1.3.2).
                if (running && task.Giver != null)
                {
                    BuildGoal working = task.Giver.GetComponent<BuildGoal>();
                    if (working != null && !working.IsNullOrReleased() && working.Provider != null
                        && working.Provider != provider) continue;
                }
                foreach (Type t in new List<Type>(stats.RemainingByType.Keys))
                {
                    int carried = c.recepteur.ContentDescription.Value(t);
                    if (carried > 0) stats.Add(c, t, carried);
                }
            }
            foreach (Labor c in gone) Carriers.Remove(c);
        }

        // Blueprints commités et constructibles du même projet (le goal lui-même inclus).
        private static List<BuildGoal> ProjectGoals(BuildGoal goal)
        {
            var list = new List<BuildGoal>();
            BuildGoalProvider provider = goal.Provider;
            if (provider != null)
                foreach (Goal g in provider.All().ToList())
                {
                    BuildGoal b = g as BuildGoal;
                    if (b != null && !b.IsAnti && b.Commited && b.enabled) list.Add(b);
                }
            if (!list.Contains(goal)) list.Add(goal);
            return list;
        }

        // Workers réellement actifs sur le Goal, hors labor. Goal.workers peut garder des entrées
        // périmées (PendingTask abandonnée ou remplacée, arbre gelé, PhaseOut, Goal poolé) :
        // on ne compte que ceux dont la Task courante ou en attente vient de ce Goal et n'est pas finie.
        // Les entrées détruites/libérées sont purgées.
        // carriersOnly : ne compter que les workers qui portent déjà ce que le goal attend.
        private static int OtherWorkers(BuildGoal goal, Labor labor, bool carriersOnly)
        {
            var workers = WorkersField.GetValue(goal) as List<Labor>;
            if (workers == null) return 0;
            int purged = workers.RemoveAll(w => w == null || w.IsNullOrReleased());
            if (purged > 0 && Verbose)
                Log(string.Format("{0} : {1} worker(s) détruit(s) purgé(s)", goal.name, purged));
            int count = 0;
            foreach (Labor w in workers)
                if (w != labor && IsLive(w, goal.gameObject) && (!carriersOnly || CarriesFor(w, goal))) count++;
            return count;
        }

        private static bool IsLive(Labor w, GameObject giver)
        {
            if (!w.enabled) return false;
            Task current = w.CurrentTask;
            if (current != null && current.Giver == giver
                && current.Status != TaskProgression.Finished && current.Status != TaskProgression.Failed)
                return true;
            Task pending = w.PendingTask;
            return pending != null && pending.Giver == giver;
        }

        // Unités encore attendues par le récepteur du Blueprint. 1 au minimum : en cas de doute,
        // un worker passe toujours quand personne n'est inscrit, donc pas de blocage possible.
        private static int RemainingSlots(BuildGoal goal)
        {
            Recepteur r = goal.recepteur;
            if (r == null) return 1;
            int sum = 0;
            foreach (Ressource res in r.CurrentCapacity.GetRessources().ToList())
                sum += Math.Max(0, r.CurrentCapacity.Value(res));
            return Math.Max(1, sum);
        }

        private static bool IsCrowded(BuildGoal goal, Labor labor)
        {
            if (goal == null || labor == null || goal.IsAnti) return false;
            // Un Brictron chargé n'est pas bloqué par des inscrits encore à vide : il livre avant eux.
            bool carrier = CarriesFor(labor, goal);
            int others = OtherWorkers(goal, labor, carrier);
            if (others == 0) return false;
            int slots = RemainingSlots(goal);
            if (others < slots) return false;
            if (Verbose)
                Log(string.Format("{0} refusé sur {1} : {2} workers{3} / {4} slots", labor.name, goal.name, others,
                    carrier ? " chargés" : "", slots));
            return true;
        }

        // Log unique de la capacité de chaque type de Blueprint, pour valider l'unité « 1 objet = 1 ».
        private static readonly HashSet<string> LoggedCapacities = new HashSet<string>();

        private static void LogCapacityOnce(BuildGoal goal)
        {
            if (goal == null || goal.IsAnti) return;
            FactoryImprint imprint = goal.GetComponent<FactoryImprint>();
            string key = imprint != null ? imprint.AssetKey.Name : goal.name;
            if (!LoggedCapacities.Add(key)) return;
            Recepteur r = goal.recepteur;
            if (r == null) return;
            Log(string.Format("Capacité {0} : base [{1}] restante [{2}] → slots {3}",
                key, Describe(r.BaseCapacity), Describe(r.CurrentCapacity), RemainingSlots(goal)));
        }

        internal static string Describe(Description d)
        {
            if (d == null) return "null";
            var parts = new List<string>();
            foreach (Ressource res in d.GetRessources().ToList())
                parts.Add(res.GetType().Name + "=" + d.Value(res));
            return string.Join(", ", parts.ToArray());
        }

        private static Dictionary<string, bool> LoadConfig(string path)
        {
            var cfg = new Dictionary<string, bool>
            {
                { "FilterIsMatchFor", true }, { "GuardPrepare", true }, { "ProjectLimit", true }, { "ClampPickup", true },
                { "Verbose", false }, { "LogCapacity", true }, { "LogOrders", true }, { "PickupFilter", true }, { "LogAll", true },
                { "StorageReserve", true }, { "BuildReach", true }, { "BuildPlan", true },
                { "FastThinking", true }, { "CompleteFirst", true }, { "PassThroughIdle", true },
            };
            try
            {
                var lines = new List<string>(File.Exists(path) ? File.ReadAllLines(path) : new[] { "# BuildGoalFix : true/false" });
                var seen = new HashSet<string>();
                foreach (string line in lines)
                {
                    string[] kv = line.Split('=');
                    if (line.TrimStart().StartsWith("#") || kv.Length != 2) continue;
                    string key = kv[0].Trim();
                    if (!cfg.ContainsKey(key)) continue;
                    cfg[key] = kv[1].Trim().ToLowerInvariant() == "true";
                    seen.Add(key);
                }
                // Ajoute au fichier les clés absentes (config créée par une version précédente).
                int count = lines.Count;
                foreach (var kv in cfg)
                    if (!seen.Contains(kv.Key)) lines.Add(kv.Key + " = " + kv.Value.ToString().ToLowerInvariant());
                if (lines.Count != count || !File.Exists(path)) File.WriteAllLines(path, lines.ToArray());
            }
            catch (Exception e) { Log("Config illisible, valeurs par défaut : " + e.Message); }
            return cfg;
        }

        internal static void LogError(Exception e)
        {
            if (_errorLogged) return;
            _errorLogged = true;
            Log("Erreur dans un hook (comportement d'origine conservé) : " + e);
        }

        private static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, string.Format("[{0:HH:mm:ss}] {1}\n", DateTime.Now, msg)); }
            catch { }
        }
    }
}
