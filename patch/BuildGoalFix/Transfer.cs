using System;
using System.Collections.Generic;
using System.IO;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.InstructionType;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Legacy;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Factories;
using Motus.Behavior;
using UnityEngine;
using UnityEngine.Networking;
using Task = Brix.Game.AI.Task;
using Path = System.IO.Path;

namespace CastleStory.BuildGoalFix
{
    // v1.10 : action joueur « Transférer » (clé Transfer) : vider une palette SOURCE dans une palette DESTINATION.
    //
    // Interface (client) [O] : la roue d'une palette sélectionnée (menu PaletteAnywhere, « Sel: Palette » du menu Game)
    //   est définie en texte (Info/Livre/PieMenu_Menus.txt, PieMenu_Buttons.txt), lue une seule fois par Livre.Lire
    //   (cache par patron|emplacement). Rien n'est écrit sur disque : hook U remplace les File.ReadAllText de Livre.Lire
    //   par ReadLivre, qui ajoute deux BUTTON au texte des boutons et met deux « Button: » à la place des deux premiers
    //   « Hole » de premier niveau de PaletteAnywhere. Libellés en clair : préfixe « && » (I2Helper.TryGet le retire).
    //   Hook V (fin de Instructions.Batire) déclare Event_/Condition_StockpileTransfer(Stop) dans commendes/conditions.
    //   Geste : sélectionner la (les) palette(s) source, ouvrir la roue sur la palette destination, « Transférer ici ».
    //   « Arrêter le transfert » : roue de la palette source, n'importe où.
    // Réseau [O] : pas de nouvelle commande UNet (tissage impossible) ; on détourne UNetProjectCmd.CmdStockpileAcceptOnly
    //   (GameObject palette, string ressource) : chaîne « @xfer:<netId destination> » ou « @xfer:stop ». Hook W en entrée
    //   (retour anticipé) côté serveur ; propriété vérifiée comme vanilla (Affiliation.IsOwnedBy, source ET destination).
    //   Hôte non patché : le vanilla ferait ClearExclusions puis lèverait une exception (ressource inconnue) → la palette
    //   source repasse « accepter tout ». L'hôte doit donc avoir le patch (l'IA ne tourne que chez lui).
    // Exécution (serveur) : registre Jobs (source → destination, une tâche par source). Les Brictron INACTIFS s'en
    //   chargent : hook X en sortie de IdleProject._GetInstruction (dernier recours de LookForWork, donc seulement un
    //   Brictron sans autre travail), sac vide, Builder, ni LockedToOperable ni opérateur, au plus MaxWorkers par tâche,
    //   source et destination accessibles. Choix : les Brictron sélectionnés au moment de l'ordre seraient des ordres
    //   « User » (autonomie levée, file d'ordres vidée par le premier ordre suivant, rien pour enchaîner les allers-retours) ;
    //   les inactifs enchaînent sans micro-gestion et ne retirent personne d'un chantier.
    //   Un aller-retour = une Task « Transfert » (LaborInstruction<GameObject>, Giver null, DecisionSource.Project) :
    //   PickStep (objet libre de la source que la destination accepte, borne = min(port, place libre de la destination
    //   moins réservations v1.5 et prises prévues des autres)) → GoPickUp → StoreStep → GoStoreIn(destination) → DoneStep.
    //   Pas d'embauche (labor.Project inchangé), Activity = Idle comme GoStore de l'IdleProject.
    // Interactions avec les hooks existants :
    //   G (Hooks.OnGoPickUp) : pour un Brictron en transfert, objet réservé par un autre → autre objet libre de la MÊME
    //     palette (jamais une recherche ailleurs).
    //   L (Storage.AfterTest) : une source en cours de transfert refuse tout dépôt automatique (sinon CompleteFirst la
    //     re-remplirait : c'est une palette entamée) ; elle est aussi ignorée comme « palette entamée » de StartedElsewhere.
    //   M (Storage.OnGoStoreIn) : la destination choisie par le joueur n'est jamais redirigée (réservation posée quand même).
    //   C (ClampPickup) : sans effet (Giver null).
    // Fin : source vide, destination pleine ou n'acceptant plus rien de la source, palette détruite ou changée de camp,
    //   MaxConsecutiveFailures échecs de suite, arrêt par le joueur, nouvel ordre depuis la même source (remplacé) ou ordre
    //   inverse. La fin est journalisée quand le dernier aller-retour en cours est terminé.
    // Persistance : aucune. Le registre vit en mémoire sur l'hôte ; il est perdu au rechargement d'une partie (relancer
    //   l'ordre). Choix volontaire : une reprise indexée par position aurait à gérer palettes déplacées/détruites/recréées
    //   et un fichier annexe hors sauvegarde ; le coût (un clic) ne le justifie pas.
    // Journal : BuildGoalFix.log « [Transfert #n] » (début, chaque aller-retour, échecs, fin + raison) ;
    //   Logistique.log TRANSFERT_DEBUT / TRANSFERT_TRAJET / TRANSFERT_ECHEC / TRANSFERT_FIN (+ TACHE/RAMASSE/DEPOSE/FIN
    //   habituels, ordre « T »).
    public static class Transfer
    {
        public static bool Enabled;
        public const string Description = "Transfert";
        private const string Prefix = "@xfer:";
        private const string StopArg = "stop";
        private const int MaxWorkers = 3;
        private const int MaxConsecutiveFailures = 4;
        private const float StartGrace = 3f;            // Prepare → Task visible sur le Brictron

        static Transfer() { Hooks.EnsureLoaded(); }

        private sealed class Job
        {
            public int Id;
            public Recepteur Src, Dst;
            public Faction Faction;
            public string Requester;
            public float Started;
            public int Trips, Moved, Failures, Consecutive;
            public string EndReason;                     // non null : plus d'attribution, fin journalisée à 0 trajet en cours
        }

        private sealed class Trip
        {
            public Job Job;
            public float Since;
            public int Planned;                          // objets prévus au ramassage (avant dépôt)
            public int Load;                             // objets en sac au départ vers la destination
            public string LoadDesc;
            public bool Storing, Done, Cancelled;
        }

        private static readonly List<Job> Jobs = new List<Job>();
        private static readonly Dictionary<Labor, Trip> Trips = new Dictionary<Labor, Trip>();
        private static int _nextId;

        private static LaborInstruction<GameObject> _instruction;
        private static LaborInstruction<GameObject> Instruction
        {
            get
            {
                return _instruction ?? (_instruction = new LaborInstruction<GameObject>(Description, IconKeys.PickUp,
                    (Labor labor, GameObject src) => TripNode(labor, src)));
            }
        }

        // ===================== Interface (client) =====================

        // U : remplace File.ReadAllText(path) dans Livre.Lire(patron, emplacement).
        public static string ReadLivre(string path)
        {
            string text = File.ReadAllText(path);
            try
            {
                if (!Enabled || path == null) return text;
                string name = Path.GetFileName(path);
                if (name == "PieMenu_Buttons.txt") return text + ButtonsText;
                if (name == "PieMenu_Menus.txt") return PatchMenus(text);
            }
            catch (Exception e) { Hooks.LogError(e); }
            return text;
        }

        // Syntaxe Livre : « Clé : valeur », ni « : » ni « / » ni guillemet dans la valeur ; « && » = texte non traduit.
        private const string ButtonsText =
            "\n\nBUTTON StockpileTransfer\n" +
            "Name : &&Transférer ici\n" +
            "Condition : Condition_StockpileTransfer\n" +
            "Effect : Event_StockpileTransfer\n" +
            "Icon : _Drop_Into\n" +
            "Tooltip : &&Les Brictron inactifs portent le contenu de la palette sélectionnée dans celle-ci, jusqu'à ce que la source soit vide ou la destination pleine\n" +
            "\n" +
            "BUTTON StockpileTransferStop\n" +
            "Name : &&Arrêter le transfert\n" +
            "Condition : Condition_StockpileTransferStop\n" +
            "Effect : Event_StockpileTransferStop\n" +
            "Icon : UI_Trash\n" +
            "Sound : Negative\n" +
            "Tooltip : &&Arrête le transfert en cours depuis la palette sélectionnée\n";

        // Les deux premiers « Hole » de premier niveau (hors Submenu/If/Sel/Hover) du menu PaletteAnywhere.
        private static string PatchMenus(string text)
        {
            string[] lines = text.Split('\n');
            string[] buttons = { "\tButton: StockpileTransfer", "\tButton: StockpileTransferStop" };
            bool inMenu = false;
            int depth = 0, done = 0;
            for (int i = 0; i < lines.Length && done < buttons.Length; i++)
            {
                string t = lines[i].Trim();
                if (t.StartsWith("MENU "))
                {
                    inMenu = t.Substring(5).Trim() == "PaletteAnywhere";
                    depth = 0;
                    continue;
                }
                if (!inMenu) continue;
                if (t.StartsWith("Submenu") || t.StartsWith("If") || t.StartsWith("Sel") || t.StartsWith("Hover")) depth++;
                else if (t == "End") depth--;
                else if (depth == 0 && t == "Hole")
                {
                    lines[i] = buttons[done] + (lines[i].EndsWith("\r") ? "\r" : "");
                    done++;
                }
            }
            Hooks.LogLine(done == buttons.Length
                ? "Transfert : boutons ajoutés à la roue PaletteAnywhere"
                : "Transfert : roue PaletteAnywhere sans emplacement libre (" + done + "/2), boutons absents");
            return string.Join("\n", lines);
        }

        // V : fin de Instructions.Batire().
        public static void AfterBatire()
        {
            try
            {
                if (!Enabled) return;
                Instructions.commendes["Event_StockpileTransfer"] = SendTransfer;
                Instructions.commendes["Event_StockpileTransferStop"] = SendStop;
                Instructions.conditions["Condition_StockpileTransfer"] = () => CanTransfer();
                Instructions.conditions["Condition_StockpileTransferStop"] = () => CanStop();
            }
            catch (Exception e) { Hooks.LogError(e); }
        }

        private static bool IsStorage(Recepteur r)
        {
            return r != null && !r.IsNullOrReleased() && r.Fonction == FonctionDeContenant.entrepot;
        }

        private static Recepteur TargetStorage()
        {
            GameObject go = Instructions.objet as GameObject;
            Recepteur r = go != null ? go.Get<Recepteur>() : null;
            return IsStorage(r) && Affiliation.BelongsToMe(r.gameObject) ? r : null;
        }

        private static List<Recepteur> SelectedStorages()
        {
            var list = new List<Recepteur>();
            MonoBehaviour[] sujets = Instructions.sujets;
            if (sujets == null) return list;
            foreach (MonoBehaviour mb in sujets)
            {
                if (mb == null) continue;
                Recepteur r = mb.Get<Recepteur>();
                if (IsStorage(r) && Affiliation.BelongsToMe(r.gameObject) && !list.Contains(r)) list.Add(r);
            }
            return list;
        }

        private static bool CanTransfer()
        {
            try
            {
                Recepteur dst = TargetStorage();
                if (dst == null) return false;
                foreach (Recepteur src in SelectedStorages())
                    if (src != dst && !src.IsEmpty()) return true;
                return false;
            }
            catch (Exception e) { Hooks.LogError(e); return false; }
        }

        // Sur l'hôte, le registre est connu : bouton actif seulement si une palette sélectionnée est une source en cours.
        private static bool CanStop()
        {
            try
            {
                List<Recepteur> sel = SelectedStorages();
                if (sel.Count == 0) return false;
                if (!NetworkServer.active) return true;
                foreach (Recepteur r in sel) if (ActiveJobFrom(r) != null) return true;
                return false;
            }
            catch (Exception e) { Hooks.LogError(e); return false; }
        }

        private static UNetProjectCmd LocalCmd()
        {
            return User.LocalUser != null ? User.LocalUser.GetComponent<UNetProjectCmd>() : null;
        }

        private static void SendTransfer()
        {
            try
            {
                Recepteur dst = TargetStorage();
                UNetProjectCmd cmd = LocalCmd();
                if (dst == null || cmd == null) return;
                NetworkIdentity id = dst.gameObject.GetComponent<NetworkIdentity>();
                if (id == null) { Hooks.LogLine("Transfert : destination sans NetworkIdentity, ordre non envoyé"); return; }
                foreach (Recepteur src in SelectedStorages())
                    if (src != dst) cmd.CallCmdStockpileAcceptOnly(src.gameObject, Prefix + id.netId.Value);
            }
            catch (Exception e) { Hooks.LogError(e); }
        }

        private static void SendStop()
        {
            try
            {
                UNetProjectCmd cmd = LocalCmd();
                if (cmd == null) return;
                foreach (Recepteur src in SelectedStorages()) cmd.CallCmdStockpileAcceptOnly(src.gameObject, Prefix + StopArg);
            }
            catch (Exception e) { Hooks.LogError(e); }
        }

        // ===================== Commande (serveur) =====================

        // W : entrée de UNetProjectCmd.CmdStockpileAcceptOnly(recepteurGO, ressourceString) ; vrai = commande consommée.
        public static bool OnAcceptOnly(UNetProjectCmd cmd, GameObject srcGo, string arg)
        {
            if (arg == null || !arg.StartsWith(Prefix)) return false;
            try
            {
                if (!Enabled) { Hooks.LogLine("Transfert : ordre reçu mais désactivé (Transfer=false), ignoré"); return true; }
                GameObject player = cmd != null ? cmd.gameObject : null;
                Recepteur src = srcGo != null ? srcGo.GetComponent<Recepteur>() : null;
                string who = player != null ? player.name : "?";
                if (player == null || !IsStorage(src) || !Affiliation.IsOwnedBy(srcGo, player))
                {
                    Hooks.LogLine("Transfert : ordre refusé (source invalide ou d'un autre joueur) de " + who);
                    return true;
                }
                string rest = arg.Substring(Prefix.Length);
                if (rest == StopArg)
                {
                    Job job = ActiveJobFrom(src);
                    if (job != null) End(job, "arrêté par " + who);
                    else Hooks.LogLine(string.Format("Transfert : arrêt demandé par {0} sur {1} : aucun transfert en cours", who, src.name));
                    return true;
                }
                uint id;
                GameObject dstGo = uint.TryParse(rest, out id) ? NetworkServer.FindLocalObject(new NetworkInstanceId(id)) : null;
                Recepteur dst = dstGo != null ? dstGo.GetComponent<Recepteur>() : null;
                if (!IsStorage(dst) || dst == src || !Affiliation.IsOwnedBy(dstGo, player))
                {
                    Hooks.LogLine(string.Format("Transfert : ordre refusé de {0} : destination invalide ({1})", who, dstGo != null ? dstGo.name : rest));
                    return true;
                }
                Start(src, dst, who);
            }
            catch (Exception e) { Hooks.LogError(e); }
            return true;
        }

        private static void Start(Recepteur src, Recepteur dst, string who)
        {
            foreach (Job j in Jobs.ToArray())
            {
                if (j.EndReason != null) continue;
                if (j.Src == src) End(j, "remplacé par un nouvel ordre");
                else if (j.Src == dst && j.Dst == src) End(j, "remplacé par le transfert inverse");
            }
            string refusal = null;
            if (src.IsEmpty()) refusal = "source vide";
            // HasRoomForSomeOf est faux aussi pour une source « storeOnly » (champ privé, non vidable).
            else if (!dst.HasRoomForSomeOf(src)) refusal = dst.IsFull() ? "destination pleine" : "la destination n'accepte rien de la source (ou source non vidable)";
            if (refusal != null)
            {
                Hooks.LogLine(string.Format("Transfert : ordre de {0} refusé ({1}) : {2} [{3}] → {4} [{5}]", who, refusal,
                    src.name, Hooks.Describe(src.ContentDescription), dst.name, Hooks.Describe(dst.ContentDescription)));
                return;
            }
            GameComponent sgc = src;
            var job = new Job { Id = ++_nextId, Src = src, Dst = dst, Faction = sgc.faction, Requester = who, Started = Time.time };
            Jobs.Add(job);
            string msg = string.Format("{0} → {1} ; distance {2:F0} m ; source [{3}] ; destination [{4}] ; demandé par {5}",
                src.name, dst.name, Vector3.Distance(src.transform.position, dst.transform.position),
                Hooks.Describe(src.ContentDescription), Hooks.Describe(dst.ContentDescription), who);
            Hooks.LogLine(Tag(job) + " début : " + msg);
            Journal.Event("TRANSFERT_DEBUT", Tag(job) + " " + msg);
        }

        // ===================== Attribution (serveur) =====================

        // X : sortie de IdleProject._GetInstruction(labor, result) : remplace l'errance/la pause d'un Brictron inactif
        // à sac vide par un aller-retour de transfert.
        public static void AfterIdleInstruction(IdleProject idle, Labor labor, Var<OrderPackage<Labor>> result)
        {
            try
            {
                if (!Enabled || Jobs.Count == 0 || result == null || !Eligible(labor)) return;
                Purge();
                Job best = null;
                float bestD = float.MaxValue;
                foreach (Job j in Jobs.ToArray())    // copie : CheckEnd peut retirer une tâche finie de Jobs
                {
                    if (j.EndReason != null || CheckEnd(j) || j.Faction != labor.faction) continue;
                    if (LiveWorkers(j, null) >= MaxWorkers) continue;
                    GameObject item; int upTo;
                    if (!Plan(j, labor, out item, out upTo)) continue;
                    float d = Vector3.Distance(labor.transform.position, j.Src.transform.position);
                    if (d >= bestD) continue;
                    Knowledge k = Knowledge.Instance;
                    if (k != null && (!k.CanReach(labor, j.Src) || !k.CanReach(labor, j.Dst))) continue;
                    best = j; bestD = d;
                }
                if (best == null) return;
                Job chosen = best;
                OrderPackage<Labor> pkg = Order.Package.OfGameObject(Instruction, chosen.Src.gameObject, Labor.DecisionSource.Project);
                pkg.SetPrepare(worker => Prepare(worker as Labor, chosen));
                result.Set(pkg);
            }
            catch (Exception e) { Hooks.LogError(e); }
        }

        private static bool Eligible(Labor labor)
        {
            if (labor == null || labor.IsNullOrReleased() || !labor.enabled || labor.recepteur == null || !labor.recepteur.IsEmpty()) return false;
            if (labor.Profession == null || labor.Profession.OccupationType != Occupation.Type.Builder) return false;
            if (labor.Autonomy == AutonomyStatus.LockedToOperable || labor.Operable != null || labor.IsInDanger) return false;
            Trip t;
            return !(Trips.TryGetValue(labor, out t) && IsLive(labor, t));
        }

        private static bool Prepare(Labor labor, Job job)
        {
            try
            {
                if (labor == null || job.EndReason != null || LiveWorkers(job, labor) >= MaxWorkers) return false;
                labor.Activity = Activity.Idle;
                Trips[labor] = new Trip { Job = job, Since = Time.time };
                return true;
            }
            catch (Exception e) { Hooks.LogError(e); return false; }
        }

        // Objet à prendre et borne : premier objet libre de la source que la destination accepte ; borne = place du port,
        // place libre de la destination (réservations v1.5 et prises prévues des autres déduites), objets de même type libres.
        private static bool Plan(Job j, Labor labor, out GameObject item, out int upTo)
        {
            item = null; upTo = 0;
            int plannedOthers = 0;
            foreach (var kv in Trips)
                if (kv.Value.Job == j && kv.Key != labor && !kv.Value.Storing && !kv.Value.Done && IsLive(kv.Key, kv.Value))
                    plannedOthers += kv.Value.Planned;
            foreach (IDescriptor d in j.Src.StoredItems)
            {
                GameObject go = d != null ? d.GameObject : null;
                if (go == null || go.IsNullOrReleased() || !Hooks.IsFreeFor(go, labor) || !j.Dst.HasRoomFor(go)) continue;
                item = go;
                break;
            }
            if (item == null) return false;
            GameComponent gc = item.GetComponent<GameComponent>();
            if (gc == null) { item = null; return false; }
            int fit = labor.recepteur.CanFitUpTo(gc.AssetKey);
            if (fit <= 0) { item = null; return false; }
            int sameKey = 0;
            foreach (IDescriptor d in j.Src.StoredItems)
                if (d != null && d.GameObject != null && d.AssetKey == gc.AssetKey && Hooks.IsFreeFor(d.GameObject, labor)) sameKey++;
            int limit = Math.Min(fit, sameKey - plannedOthers);
            int room = Storage.FreeRoom(j.Dst, gc, labor);
            if (room >= 0) limit = Math.Min(limit, room - plannedOthers);
            if (limit <= 0) { item = null; return false; }
            upTo = limit;
            return true;
        }

        // ===================== Aller-retour (Task « Transfert ») =====================

        private static Node TripNode(Labor labor, GameObject src)
        {
            return (StaticNode)Node.Sequence.Do((Func<Labor, Node>)PickStep, labor).Label("Transfert : ramasser")
                .Do((Func<Labor, Node>)StoreStep, labor).Label("Transfert : déposer")
                .Do((Action<Labor>)DoneStep, labor);
        }

        private static Node PickStep(Labor labor)
        {
            try
            {
                Trip t;
                if (labor == null || !Trips.TryGetValue(labor, out t)) return Node.FailWork<Exceptions.NullTarget>();
                Job j = t.Job;
                GameObject item; int upTo;
                if (j.EndReason != null || CheckEnd(j) || !Plan(j, labor, out item, out upTo))
                {
                    t.Cancelled = true;      // plus rien à faire pour ce Brictron : pas un échec du transfert
                    return Node.FailWork<Exceptions.CannotFindWhatINeedToPickUp>();
                }
                t.Planned = upTo;
                return labor.GoPickUp(item.GetComponent<GameComponent>(), upTo);
            }
            catch (Exception e) { Hooks.LogError(e); return Node.FailWork<Exceptions.NullTarget>(); }
        }

        private static Node StoreStep(Labor labor)
        {
            try
            {
                Trip t;
                if (labor == null || !Trips.TryGetValue(labor, out t)) return Node.FailWork<Exceptions.NullTarget>();
                if (labor.recepteur.IsEmpty()) return Node.FailWork<Exceptions.InventoryBecameEmpty>();
                t.Load = labor.recepteur.StoredItems.Count;
                t.LoadDesc = Hooks.Describe(labor.recepteur.ContentDescription);
                t.Storing = true;    // avant GoStoreIn : le hook M lit ce drapeau pour ne pas rediriger
                return labor.GoStoreIn(t.Job.Dst);
            }
            catch (Exception e) { Hooks.LogError(e); return Node.FailWork<Exceptions.NullTarget>(); }
        }

        private static void DoneStep(Labor labor)
        {
            try
            {
                Trip t;
                if (labor == null || !Trips.TryGetValue(labor, out t)) return;
                Job j = t.Job;
                int deposited = t.Load - labor.recepteur.StoredItems.Count;
                t.Done = true;
                j.Trips++;
                j.Moved += deposited;
                j.Consecutive = 0;
                string msg = string.Format("{0} aller-retour {1} : {2} objet(s) déposé(s) [{3}] ; total {4} ; source [{5}] ; destination [{6}]{7}",
                    labor.name, j.Trips, deposited, t.LoadDesc, j.Moved, Hooks.Describe(j.Src.ContentDescription),
                    Hooks.Describe(j.Dst.ContentDescription), labor.recepteur.IsEmpty() ? "" : " ; reste en sac [" + Hooks.Describe(labor.recepteur.ContentDescription) + "]");
                Hooks.LogLine(Tag(j) + " " + msg);
                Journal.Event("TRANSFERT_TRAJET", Tag(j) + " " + msg);
                CheckEnd(j);
            }
            catch (Exception e) { Hooks.LogError(e); }
        }

        // Y : entrée de Task.OnFinally.
        public static void OnTaskEnd(Task task)
        {
            try
            {
                if (task == null || Trips.Count == 0) return;
                Labor labor = task.Worker as Labor;
                Trip t;
                if (labor == null || task.Description != Description || !Trips.TryGetValue(labor, out t)) return;
                Trips.Remove(labor);
                Job j = t.Job;
                if (task.result != TaskResult.Succes && !t.Done && !t.Cancelled)
                {
                    j.Failures++;
                    j.Consecutive++;
                    string msg = string.Format("{0} échec ({1}{2}) en {3} ; échecs de suite {4}", labor.name, task.result,
                        Hooks.FailureDetail(task), t.Storing ? "allant déposer" : "allant ramasser", j.Consecutive);
                    Hooks.LogLine(Tag(j) + " " + msg);
                    Journal.Event("TRANSFERT_ECHEC", Tag(j) + " " + msg);
                    if (j.EndReason == null && j.Consecutive >= MaxConsecutiveFailures) End(j, "échecs répétés");
                }
                if (j.EndReason == null) CheckEnd(j);
                FinalizeEnded();
            }
            catch (Exception e) { Hooks.LogError(e); }
        }

        // ===================== Requêtes des autres hooks =====================

        // L / StartedElsewhere : source d'un transfert actif → aucun dépôt automatique.
        internal static bool BlocksStorage(Recepteur r)
        {
            return Enabled && r != null && ActiveJobFrom(r) != null;
        }

        // M : destination imposée par le joueur pour ce Brictron (pas de redirection).
        internal static bool IsForcedDestination(Labor labor, GameComponent contenant)
        {
            Trip t;
            return Enabled && labor != null && contenant != null && Trips.TryGetValue(labor, out t) && t.Storing && !t.Done
                && t.Job.Dst == contenant.recepteur;
        }

        // G : Brictron en train de ramasser pour un transfert (la cible reste dans la source choisie).
        internal static bool IsPicking(Labor labor)
        {
            Trip t;
            return Enabled && labor != null && Trips.TryGetValue(labor, out t) && !t.Storing && !t.Done;
        }

        // ===================== Registre =====================

        private static Job ActiveJobFrom(Recepteur src)
        {
            foreach (Job j in Jobs) if (j.EndReason == null && j.Src == src) return j;
            return null;
        }

        private static bool IsLive(Labor labor, Trip t)
        {
            if (labor == null || labor.IsNullOrReleased() || t.Done) return false;
            if (Time.time - t.Since < StartGrace) return true;
            Task cur = labor.CurrentTask;
            if (cur != null && cur.Description == Description && cur.Status != TaskProgression.Finished && cur.Status != TaskProgression.Failed) return true;
            Task pending = labor.PendingTask;
            return pending != null && pending.Description == Description;
        }

        private static int LiveWorkers(Job j, Labor exclude)
        {
            int n = 0;
            foreach (var kv in Trips) if (kv.Value.Job == j && kv.Key != exclude && IsLive(kv.Key, kv.Value)) n++;
            return n;
        }

        // Vrai si la tâche est (ou vient d'être mise) en fin.
        private static bool CheckEnd(Job j)
        {
            if (j.EndReason != null) return true;
            string reason = null;
            if (j.Src == null || j.Src.IsNullOrReleased() || j.Dst == null || j.Dst.IsNullOrReleased()) reason = "palette détruite";
            else if (((GameComponent)j.Src).faction != j.Faction || ((GameComponent)j.Dst).faction != j.Faction) reason = "palette changée de camp";
            else if (j.Src.IsEmpty()) reason = "source vide";
            else if (j.Dst.IsFull()) reason = "destination pleine";
            else if (!j.Dst.HasRoomForSomeOf(j.Src)) reason = "la destination n'accepte plus rien de la source";
            if (reason == null) return false;
            End(j, reason);
            return true;
        }

        private static void End(Job j, string reason)
        {
            if (j.EndReason != null) return;
            j.EndReason = reason;
            FinalizeEnded();
        }

        // Fin journalisée quand plus aucun aller-retour de la tâche n'est en cours.
        private static void FinalizeEnded()
        {
            for (int i = Jobs.Count - 1; i >= 0; i--)
            {
                Job j = Jobs[i];
                if (j.EndReason == null || LiveWorkers(j, null) > 0) continue;
                Jobs.RemoveAt(i);
                bool alive = j.Src != null && !j.Src.IsNullOrReleased() && j.Dst != null && !j.Dst.IsNullOrReleased();
                string msg = string.Format("fin : {0} ; {1} objet(s) en {2} aller-retour(s), {3} échec(s), {4:F0} s{5}",
                    j.EndReason, j.Moved, j.Trips, j.Failures, Time.time - j.Started,
                    alive ? string.Format(" ; source [{0}] ; destination [{1}]", Hooks.Describe(j.Src.ContentDescription), Hooks.Describe(j.Dst.ContentDescription)) : "");
                Hooks.LogLine(Tag(j) + " " + msg);
                Journal.Event("TRANSFERT_FIN", Tag(j) + " " + msg);
            }
        }

        private static void Purge()
        {
            List<Labor> gone = null;
            foreach (var kv in Trips)
                if (!IsLive(kv.Key, kv.Value)) { if (gone == null) gone = new List<Labor>(); gone.Add(kv.Key); }
            if (gone != null) foreach (Labor l in gone) Trips.Remove(l);
            FinalizeEnded();
        }

        private static string Tag(Job j) { return "[Transfert #" + j.Id + "]"; }
    }
}
