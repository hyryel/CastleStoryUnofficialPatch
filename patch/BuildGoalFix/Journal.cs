using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Smooth.Slinq;
using UnityEngine;
using Task = Brix.Game.AI.Task;
using Path = System.IO.Path;

namespace CastleStory.BuildGoalFix
{
    // v1.4 : journal unifié de la logistique (tous projets, stations d'artisanat, ordres directs, idle) + distances.
    // Observation pure : aucun hook de ce fichier ne modifie le comportement du jeu.
    // Fichier : Logistique.log à côté de la DLL. Une ligne par événement :
    //   [HH:mm:ss] t=<Time.time> EVT [ordre] Brictron ; clé=valeur ; ...
    // ordre = P#<id>:<TypeProjet> | S#<id>:<TypeStation> | U (ordre joueur) | I (idle) | ?
    public static class Journal
    {
        private static readonly string Dir = Path.GetDirectoryName(typeof(Journal).Assembly.Location);
        private static readonly string LogPath = Path.Combine(Dir, "Logistique.log");
        public static bool Enabled;
        private static bool _errorLogged;
        private static readonly FieldInfo ProjectField = typeof(Goal).GetField("project", BindingFlags.Instance | BindingFlags.NonPublic);

        // Goal en cours d'embauche (posé par Hooks.WrapPrepare autour de Project.Hire).
        public static Goal CurrentGoal;

        // --- Odomètre : distance réellement parcourue par chaque Brictron, échantillonnée à chaque frame ---
        private sealed class Odo { public Vector3 Last; public float Total; public bool Init; }
        private static readonly Dictionary<Labor, Odo> Odos = new Dictionary<Labor, Odo>();
        private static readonly Dictionary<Labor, float> TaskStartOdo = new Dictionary<Labor, float>();
        private static readonly Dictionary<Labor, Labor.DecisionSource> LastSource = new Dictionary<Labor, Labor.DecisionSource>();
        private static JournalTracker _tracker;

        private static Odo Track(Labor l)
        {
            Odo o;
            if (!Odos.TryGetValue(l, out o)) Odos[l] = o = new Odo();
            if (_tracker == null)
            {
                var go = new GameObject("BuildGoalFix.Journal");
                go.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(go);
                _tracker = go.AddComponent<JournalTracker>();
            }
            return o;
        }

        internal static void Tick()
        {
            if (Odos.Count == 0) return;
            List<Labor> gone = null;
            foreach (var kv in Odos)
            {
                Labor l = kv.Key;
                if (l == null || l.IsNullOrReleased()) { if (gone == null) gone = new List<Labor>(); gone.Add(l); continue; }
                Vector3 p = l.transform.position;
                Odo o = kv.Value;
                if (o.Init) o.Total += Vector3.Distance(o.Last, p);
                o.Last = p; o.Init = true;
            }
            if (gone != null) foreach (Labor l in gone) { Odos.Remove(l); TaskStartOdo.Remove(l); LastSource.Remove(l); }
        }

        private static float Odometer(Labor l) { Odo o; return Odos.TryGetValue(l, out o) ? o.Total : 0f; }

        // --- Aides de format ---
        private static string Pos(Vector3 p) { return string.Format("{0:F0},{1:F0},{2:F0}", p.x, p.y, p.z); }
        private static string Pos(Component c) { return c == null ? "?" : Pos(c.transform.position); }
        private static string Pos(GameObject g) { return g == null ? "?" : Pos(g.transform.position); }
        private static string Dist(Labor l, GameObject g)
        {
            return l == null || g == null ? "?" : string.Format("{0:F1}", Vector3.Distance(l.transform.position, g.transform.position));
        }
        private static string Id(UnityEngine.Object o) { return o == null ? "?" : o.GetInstanceID().ToString(); }
        private static string Name(UnityEngine.Object o) { return o == null ? "-" : o.name; }
        private static string Type(object o) { return o == null ? "-" : o.GetType().Name; }
        private static string Bag(Labor l) { return l != null && l.recepteur != null ? Hooks.Describe(l.recepteur.ContentDescription) : "?"; }

        private static string OrderOf(Labor l, Task t)
        {
            try
            {
                GameObject giver = t != null ? t.Giver : null;
                if (giver != null)
                {
                    Goal goal = giver.GetComponent<Goal>();
                    if (goal != null)
                    {
                        Project p = ProjectField != null ? ProjectField.GetValue(goal) as Project : null;
                        if (p != null) return "P#" + Id(p) + ":" + Type(p);
                        BuildGoal b = goal as BuildGoal;
                        if (b != null && b.Provider != null) return "P#" + Id(b.Provider) + ":BuildGoalProvider";
                    }
                    IOperable<Labor> op = giver.GetComponent(typeof(IOperable<Labor>)) as IOperable<Labor>;
                    if (op != null) return "S#" + Id(giver) + ":" + Type(op);
                }
                if (l != null && l.Operable != null) return "S#" + Id(l.Operable.gameObject) + ":" + Type(l.Operable);
                Labor.DecisionSource src;
                if (l != null && LastSource.TryGetValue(l, out src) && src == Labor.DecisionSource.User) return "U";
                if (l != null && l.Project != null)
                    return (l.Project is IdleProject ? "I " : "") + "P#" + Id(l.Project) + ":" + Type(l.Project);
                return "?";
            }
            catch { return "?"; }
        }

        private static string TaskOf(Labor l)
        {
            Task t = l != null ? l.CurrentTask : null;
            if (t == null) t = l != null ? l.PendingTask : null;
            return t == null ? "-" : t.Description;
        }

        // --- Événements ---

        // Fin de Project.Hire (toutes familles de projets). CurrentGoal posé par WrapPrepare si l'embauche vient d'un Goal.
        public static bool AfterHire(bool result, Project project, Labor labor)
        {
            try
            {
                if (!Enabled || labor == null || project is IdleProject) return result;
                Track(labor);
                Goal g = CurrentGoal;
                GameObject target = g != null ? g.gameObject : (project != null ? project.gameObject : null);
                Log(string.Format("EMBAUCHE [P#{0}:{1}] {2} ; cible={3}:{4} ; ok={5} ; d={6} ; pos={7} ; cible_pos={8} ; autres={9} ; sac={10}",
                    Id(project), Type(project), labor.name, Type(g), g != null ? g.name : Name(project), result,
                    Dist(labor, target), Pos(labor), Pos(target), g != null ? g.AssignedWorkerCount().ToString() : "-", Bag(labor)));
                return result;
            }
            catch (Exception e) { Error(e); return result; }
        }

        // Fin de IOperableExtender.Hire (station d'artisanat, catapulte, activateur).
        public static bool AfterOperableHire(bool result, IOperable<Labor> operable, Labor labor)
        {
            try
            {
                if (!Enabled || labor == null || operable == null) return result;
                Track(labor);
                GameObject go = operable.gameObject;
                CraftingStation st = operable as CraftingStation;
                Log(string.Format("EMBAUCHE [S#{0}:{1}] {2} ; cible={3} ; ok={4} ; d={5} ; pos={6} ; cible_pos={7} ; recette={8} ; manque={9} ; sac={10}",
                    Id(go), Type(operable), labor.name, Name(go), result, Dist(labor, go), Pos(labor), Pos(go),
                    st != null && st.CurrentRecipe != null ? st.CurrentRecipe.Creates.Name + " (#" + st.CurrentRecipe.Info.id + ")" : "-",
                    st != null ? SafeDescribe(() => st.StuffToFetch()) : "-", Bag(labor)));
                return result;
            }
            catch (Exception e) { Error(e); return result; }
        }

        // Entrée de Labor.SendOrder : création d'une Task.
        public static void OnSendOrder(Labor labor, Task task, Labor.DecisionSource source)
        {
            try
            {
                if (!Enabled || labor == null || task == null) return;
                Track(labor);
                LastSource[labor] = source;
                TaskStartOdo[labor] = Odometer(labor);
                Log(string.Format("TACHE [{0}] {1} ; desc={2} ; giver={3} ; cible={4} ; src={5} ; d_giver={6} ; d_cible={7} ; pos={8} ; sac={9}",
                    OrderOf(labor, task), labor.name, task.Description, Name(task.Giver), Name(task.Target), source,
                    Dist(labor, task.Giver), Dist(labor, task.Target), Pos(labor), Bag(labor)));
            }
            catch (Exception e) { Error(e); }
        }

        // Transaction.StoreEverything : entrée (instantané du sac) et sortie (différence = pris / déposé).
        private sealed class Pending
        {
            public Labor Labor; public bool Pickup; public Dictionary<string, int> Before;
            public string Source; public string SourceDist;   // v1.4.1 : conteneur d'origine, lu avant le transfert
        }
        private static readonly Stack<Pending> Pendings = new Stack<Pending>();

        public static void BeforeStore(GameObject from, GameObject into, int upTo)
        {
            try
            {
                if (!Enabled) return;
                if (Pendings.Count > 32) Pendings.Clear(); // désalignement après une exception dans StoreEverything
                Labor li = into != null ? into.GetComponent<Labor>() : null;
                Labor lf = from != null ? from.GetComponent<Labor>() : null;
                Labor l = li ?? lf;
                if (l == null) { Pendings.Push(null); return; }
                Track(l);
                var p = new Pending { Labor = l, Pickup = li != null, Before = Snapshot(l.recepteur) };
                if (p.Pickup)
                {
                    GameComponent item = from != null ? from.GetComponent<GameComponent>() : null;
                    Recepteur parent = item != null ? item.ParentRecepteur : null;
                    GameObject src = parent != null && !parent.IsNullOrReleased() ? parent.gameObject : from;
                    p.Source = Name(src) + (parent != null && !parent.IsNullOrReleased() ? "/" + parent.Fonction : "/sol");
                    p.SourceDist = Dist(l, src);
                }
                Pendings.Push(p);
            }
            catch (Exception e) { Error(e); Pendings.Push(null); }
        }

        public static void AfterStore(GameObject from, GameObject into, int upTo)
        {
            try
            {
                if (!Enabled) return;
                Pending p = Pendings.Count > 0 ? Pendings.Pop() : null;
                if (p == null || p.Labor == null) return;
                Labor l = p.Labor;
                string diff = Diff(p.Before, Snapshot(l.recepteur));
                Task t = l.CurrentTask;
                if (p.Pickup)
                {
                    Log(string.Format("RAMASSE [{0}] {1} ; item={2} ; source={3} ; demandé={4} ; pris={5} ; d={6} ; pos={7} ; sac={8} ; tache={9}",
                        OrderOf(l, t), l.name, Name(from), p.Source, upTo, diff, p.SourceDist, Pos(l), Bag(l), TaskOf(l)));
                }
                else
                {
                    Recepteur dest = into != null ? into.GetComponent<Recepteur>() : null;
                    Log(string.Format("DEPOSE [{0}] {1} ; dest={2}{3} ; déposé={4} ; d={5} ; pos={6} ; reste={7} ; dest_contenu={8} ; dest_plein={9} ; tache={10}",
                        OrderOf(l, t), l.name, Name(into), dest != null ? "/" + dest.Fonction : "", diff, Dist(l, into), Pos(l), Bag(l),
                        dest != null ? Hooks.Describe(dest.ContentDescription) : "?", dest != null ? dest.IsFull().ToString() : "?", TaskOf(l)));
                }
            }
            catch (Exception e) { Error(e); }
        }

        // GarnotteEater.AbsorbGarnottes(HashSet) : Dig, récolte, Litter, Landscape. Appelée toutes les 0,1 s, le plus
        // souvent à vide : v1.4.1 journalise à la sortie, seulement si le sac a changé.
        private static readonly Stack<Dictionary<string, int>> AbsorbBefore = new Stack<Dictionary<string, int>>();

        public static void OnAbsorb(GarnotteEater eater, HashSet<GameObject> gos)
        {
            try
            {
                if (!Enabled) return;
                if (AbsorbBefore.Count > 32) AbsorbBefore.Clear();
                Labor l = eater != null ? eater.GetComponent<Labor>() : null;
                AbsorbBefore.Push(l != null ? Snapshot(l.recepteur) : null);
            }
            catch (Exception e) { Error(e); AbsorbBefore.Push(null); }
        }

        public static void AfterAbsorb(GarnotteEater eater, HashSet<GameObject> gos)
        {
            try
            {
                if (!Enabled) return;
                Dictionary<string, int> before = AbsorbBefore.Count > 0 ? AbsorbBefore.Pop() : null;
                Labor l = eater != null ? eater.GetComponent<Labor>() : null;
                if (before == null || l == null) return;
                string diff = Diff(before, Snapshot(l.recepteur));
                if (diff == "0") return;
                Track(l);
                Log(string.Format("ABSORBE [{0}] {1} ; pris={2} ; candidats={3} ; pos={4} ; sac={5} ; tache={6}",
                    OrderOf(l, l.CurrentTask), l.name, diff, gos != null ? gos.Count : 0, Pos(l), Bag(l), TaskOf(l)));
            }
            catch (Exception e) { Error(e); }
        }

        // Entrée de ConflictResolver.OperationSwitchTask : deux Brictron qui se croisent échangent sacs et tâches.
        public static void OnSwitch(Labor a, Labor b)
        {
            try
            {
                if (!Enabled || a == null || b == null) return;
                Track(a); Track(b);
                Log(string.Format("ECHANGE {0} <-> {1} ; sac_a={2} ; sac_b={3} ; tache_a={4} ; tache_b={5} ; pos={6}",
                    a.name, b.name, Bag(a), Bag(b), TaskOf(a), TaskOf(b), Pos(a)));
            }
            catch (Exception e) { Error(e); }
        }

        // Entrée de Recipe.Finish : un objet est produit par une station.
        public static void OnRecipeFinish(Recipe recipe, Recepteur station)
        {
            try
            {
                if (!Enabled || recipe == null) return;
                GameObject go = station != null ? station.gameObject : null;
                CraftingStation st = go != null ? go.GetComponent<CraftingStation>() : null;
                Log(string.Format("PRODUIT [S#{0}:{1}] {2} ; recette=#{3} ; crée={4} x{5} ; drop={6} ; pos={7} ; operateur={8}",
                    Id(go), st != null ? Type(st) : "-", Name(go), recipe.Info != null ? recipe.Info.id : -1,
                    recipe.Creates != null ? recipe.Creates.Name : "?", recipe.QryCreated, recipe.DropOnCompletion, Pos(go),
                    st != null && st.Operator != null ? st.Operator.name : "-"));
            }
            catch (Exception e) { Error(e); }
        }

        // Entrée de Recepteur.DropAll : un Brictron lâche tout au sol (après échecs de stockage).
        public static void OnDropAll(Recepteur r)
        {
            try
            {
                if (!Enabled || r == null) return;
                Labor l = r.GetComponent<Labor>();
                if (l == null) return;
                Track(l);
                Log(string.Format("AU_SOL [{0}] {1} ; contenu={2} ; pos={3} ; tache={4}",
                    OrderOf(l, l.CurrentTask), l.name, Hooks.Describe(r.ContentDescription), Pos(l), TaskOf(l)));
            }
            catch (Exception e) { Error(e); }
        }

        // Entrée de Task.OnFinally : fin de toute Task (Worker encore posé).
        public static void OnTaskEnd(Task task)
        {
            try
            {
                if (!Enabled || task == null) return;
                Labor l = task.Worker as Labor;
                if (l == null) return;
                Track(l);
                float start;
                string trajet = TaskStartOdo.TryGetValue(l, out start) ? string.Format("{0:F1}", Odometer(l) - start) : "?";
                Log(string.Format("FIN [{0}] {1} ; desc={2} ; giver={3} ; résultat={4}{5} ; durée={6:F1} ; trajet={7} ; pos={8} ; sac={9}",
                    OrderOf(l, task), l.name, task.Description, Name(task.Giver), task.result, Hooks.FailureDetail(task),
                    task.TaskStartedTimestamp > 0f ? Time.time - task.TaskStartedTimestamp : -1f, trajet, Pos(l), Bag(l)));
            }
            catch (Exception e) { Error(e); }
        }

        // v1.5 : événement de Storage (STOCK_REDIRIGE…).
        public static void StorageEvent(Labor l, string evt, string details, GameObject dest)
        {
            try
            {
                if (!Enabled || l == null) return;
                Log(string.Format("{0} [{1}] {2} ; {3} ; d={4} ; pos={5} ; tache={6}",
                    evt, OrderOf(l, l.CurrentTask), l.name, details, Dist(l, dest), Pos(l), TaskOf(l)));
            }
            catch (Exception e) { Error(e); }
        }

        // Étendue d'un ordre : distance max entre deux Blueprints, et centre (pour les ordres multiples éloignés).
        public static string Extent(List<BuildGoal> goals)
        {
            try
            {
                if (goals == null || goals.Count == 0) return "";
                float max = 0f; Vector3 sum = Vector3.zero; int n = 0;
                var pos = new List<Vector3>();
                foreach (BuildGoal b in goals) if (b != null && !b.IsNullOrReleased()) { pos.Add(b.transform.position); sum += b.transform.position; n++; }
                for (int i = 0; i < pos.Count; i++) for (int j = i + 1; j < pos.Count; j++) max = Math.Max(max, Vector3.Distance(pos[i], pos[j]));
                return n == 0 ? "" : string.Format(" ; étendue={0:F0} ; centre={1}", max, Pos(sum / n));
            }
            catch { return ""; }
        }

        // --- Sac : instantané et différence ---
        private static Dictionary<string, int> Snapshot(Recepteur r)
        {
            var d = new Dictionary<string, int>();
            if (r == null || r.ContentDescription == null) return d;
            foreach (Ressource res in r.ContentDescription.GetRessources().ToList()) d[res.GetType().Name] = r.ContentDescription.Value(res);
            return d;
        }

        private static string Diff(Dictionary<string, int> before, Dictionary<string, int> after)
        {
            var keys = new HashSet<string>(before.Keys); keys.UnionWith(after.Keys);
            var parts = new List<string>();
            foreach (string k in keys)
            {
                int a, b; before.TryGetValue(k, out b); after.TryGetValue(k, out a);
                if (a != b) parts.Add(k + (a > b ? "+" : "") + (a - b));
            }
            return parts.Count == 0 ? "0" : string.Join(", ", parts.ToArray());
        }

        private static string SafeDescribe(Func<Description> f)
        {
            try { return Hooks.Describe(f()); } catch { return "?"; }
        }

        private static void Error(Exception e)
        {
            if (_errorLogged) return;
            _errorLogged = true;
            Log("Erreur dans un hook du journal (comportement d'origine conservé) : " + e);
        }

        private static void Log(string msg)
        {
            try { File.AppendAllText(LogPath, string.Format("[{0:HH:mm:ss}] t={1:F1} {2}\n", DateTime.Now, Time.time, msg)); }
            catch { }
        }
    }

    // Composant Unity minimal : échantillonne la position des Brictron suivis à chaque frame (odomètre).
    public class JournalTracker : MonoBehaviour
    {
        private void Update() { try { Journal.Tick(); } catch { } }
    }
}
