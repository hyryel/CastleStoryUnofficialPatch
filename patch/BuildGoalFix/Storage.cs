using System;
using System.Collections.Generic;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Smooth.Slinq;
using UnityEngine;
using Task = Brix.Game.AI.Task;

namespace CastleStory.BuildGoalFix
{
    // v1.5 (volet A) : réservation de capacité des stockages (Palette, Toolrack : Fonction == entrepot).
    // Vanilla : Knowledge.ClosestStorageWithRoomFor ne demande que la place pour 1 objet, sans compter les Brictron
    // déjà en route vers ce stockage ; ils arrivent sur un stockage plein (StorageBecameFull, trajet perdu).
    // Hooks (clé StorageReserve) :
    //   K entrée Knowledge.ClosestStorageWithRoomFor(source, item, faction) → BeginSearch : mémorise le chercheur.
    //   L ret de Knowledge._ClosestStorageWithRoomFor_Test(gc, item)      → AfterTest : refus si la place libre,
    //     moins ce que les autres Brictron ont réservé, est nulle.
    //   M entrée StoreNode.GoStoreIn(labor, contenant)                     → OnGoStoreIn : si le stockage visé
    //     (cache par frame de décision) est déjà couvert par les réservations, recherche fraîche ; puis réserve
    //     la charge du Brictron sur le stockage retenu.
    //   N ret de Transaction.StoreEverything(from, into, upTo)            → AfterStore : dépôt fait → libération.
    //   O entrée Task.OnFinally                                           → OnTaskEnd : fin de la Task qui a réservé.
    // v1.8 (palettes, proposition A, clé CompleteFirst) : compléter avant d'entamer. Dans L, un stockage vide de la
    //   ressource est refusé s'il existe un stockage connu déjà entamé de cette ressource, de la même faction,
    //   accessible, avec de la place libre (réservations déduites), à moins de CompleteSlack mètres de plus que lui.
    //   But : une palette de pierre atteint 80 (conversion en 4 blocs) au lieu de plusieurs palettes entamées.
    // Réservations expirées au bout de MaxAge secondes ou si le Brictron est détruit / n'a plus la ressource.
    // État serveur uniquement, non sérialisé, aucun message réseau.
    public static class Storage
    {
        public static bool Enabled;
        public static bool CompleteFirst;
        private const float MaxAge = 90f;
        private const float CompleteSlack = 20f;

        // Stockages vus par le test L (le jeu n'en tient pas de liste accessible).
        private static readonly HashSet<Recepteur> KnownStorages = new HashSet<Recepteur>();

        // Enabled est posé par le constructeur statique de Hooks (lecture de BuildGoalFix.cfg).
        static Storage() { Hooks.EnsureLoaded(); }

        private sealed class Reservation
        {
            public Labor Labor;
            public Type Res;
            public int Amount;
            public Task Task;
            public float Since;
        }

        // Stockage → réservations en cours (une par Brictron).
        private static readonly Dictionary<Recepteur, List<Reservation>> Pending = new Dictionary<Recepteur, List<Reservation>>();
        private static Labor _searcher;

        public static void BeginSearch(Labor source, IGameComponent item, Faction faction)
        {
            _searcher = source;
        }

        public static bool AfterTest(bool result, IGameComponent gc, IGameComponent item)
        {
            try
            {
                if (!result || !Enabled || gc == null || item == null) return result;
                Recepteur r = gc.recepteur;
                if (r == null || r.Fonction != FonctionDeContenant.entrepot) return result;
                if (KnownStorages.Count > 512) KnownStorages.RemoveWhere(x => x == null || x.IsNullOrReleased());
                KnownStorages.Add(r);
                Labor self = _searcher != null && !_searcher.IsNullOrReleased() ? _searcher : null;
                Ressource res = Apparence.PrincipaleOf(item);
                if (res == null) return result;
                int room = Room(r, item, res);
                if (room >= 0 && room - Reserved(r, res.GetType(), self) <= 0) return false;
                if (CompleteFirst && self != null && StartedElsewhere(r, gc, item, res, self)) return false;
                return true;
            }
            catch (Exception e) { Hooks.LogError(e); return result; }
        }

        public static GameComponent OnGoStoreIn(Labor labor, GameComponent contenant)
        {
            try
            {
                if (!Enabled || labor == null || contenant == null || contenant.IsNullOrReleased()) return contenant;
                Recepteur r = contenant.recepteur;
                if (r == null || r.Fonction != FonctionDeContenant.entrepot || labor.recepteur == null || labor.recepteur.IsEmpty())
                    return contenant;
                IDescriptor first = labor.recepteur.StoredItems.Slinq().FirstOrDefault();
                IGameComponent item = first != null ? first.GameComponent : null;
                Ressource res = item != null ? Apparence.PrincipaleOf(item) : null;
                if (res == null) return contenant;
                Type t = res.GetType();
                int load = labor.recepteur.ContentDescription.Value(t);
                int room = Room(r, item, res);
                if (room < 0) return contenant;
                int free = room - Reserved(r, t, labor);
                GameComponent target = contenant;
                if (free <= 0 && Knowledge.Instance != null)
                {
                    IGameComponent fresh = Knowledge.Instance.ClosestStorageWithRoomFor(labor, item, labor.faction);
                    GameComponent other = fresh != null && !fresh.IsNullOrReleased() ? fresh.GameObject.GetComponent<GameComponent>() : null;
                    if (other != null && other != contenant) target = other;
                    Journal.StorageEvent(labor, "STOCK_REDIRIGE", string.Format("prévu={0} ; libre={1} ; charge={2}{3} ; nouveau={4}",
                        contenant.name, free, t.Name, load, other != null && other != contenant ? other.name : "aucun (prévu conservé)"), target.gameObject);
                }
                Reserve(target.recepteur, labor, t, Math.Max(1, load));
                return target;
            }
            catch (Exception e) { Hooks.LogError(e); return contenant; }
        }

        public static void AfterStore(GameObject from, GameObject into, int upTo)
        {
            try
            {
                if (!Enabled || from == null || into == null) return;
                Labor labor = from.GetComponent<Labor>();
                Recepteur r = into.GetComponent<Recepteur>();
                if (labor != null && r != null) Release(r, labor);
            }
            catch (Exception e) { Hooks.LogError(e); }
        }

        public static void OnTaskEnd(Task task)
        {
            try
            {
                if (!Enabled || task == null || Pending.Count == 0) return;
                Labor labor = task.Worker as Labor;
                if (labor == null) return;
                foreach (var kv in Pending) kv.Value.RemoveAll(x => x.Labor == labor && x.Task == task);
            }
            catch (Exception e) { Hooks.LogError(e); }
        }

        // Vrai si r est vide de la ressource et qu'un autre stockage déjà entamé de cette ressource peut la recevoir.
        private static bool StartedElsewhere(Recepteur r, IGameComponent gc, IGameComponent item, Ressource res, Labor self)
        {
            Type t = res.GetType();
            if (r.ContentDescription.Value(t) > 0) return false;
            GameComponent rgc = r.GetComponent<GameComponent>();
            Faction faction = rgc != null ? rgc.faction : null;
            Vector3 from = self.transform.position;
            float limit = Vector3.Distance(from, r.transform.position) + CompleteSlack;
            GameObject itemGo = item.GameObject;
            foreach (Recepteur p in KnownStorages)
            {
                if (p == null || p == r || p.IsNullOrReleased() || p.Fonction != FonctionDeContenant.entrepot) continue;
                if (p.ContentDescription.Value(t) <= 0 || p.IsFull()) continue;
                if (Vector3.Distance(from, p.transform.position) > limit) continue;
                GameComponent pgc = p.GetComponent<GameComponent>();
                if (pgc == null || pgc.faction != faction) continue;
                if (itemGo != null && !p.HasRoomFor(itemGo)) continue;
                int room = Room(p, item, res);
                if (room >= 0 && room - Reserved(p, t, self) <= 0) continue;
                if (Knowledge.Instance != null && !Knowledge.Instance.CanReach(self, pgc)) continue;
                return true;
            }
            return false;
        }

        // Place libre pour la ressource, en objets : capacité de la ressource et encombrement (si le stockage en a un).
        // -1 = non calculable (type absent de la capacité courante, ex. outils sur un Toolrack) : vanilla conservé.
        private static int Room(Recepteur r, IGameComponent item, Ressource res)
        {
            if (r.IsNullOrReleased() || r.IsFull()) return 0;
            Description cap = r.CurrentCapacity;
            if (cap.Value(res.GetType()) <= 0 && r.BaseCapacity.Value(res.GetType()) <= 0) return -1;
            Apparence app = item.GameObject != null ? item.GameObject.GetComponent<Apparence>() : null;
            Description d = app != null ? app.Description : null;
            int unit = d != null ? Math.Max(1, d.Value(res.GetType())) : 1;
            int n = cap.Value(res.GetType()) / unit;
            int enc = cap.Value<Encombrement>(), itemEnc = d != null ? d.Value<Encombrement>() : 0;
            if (enc > 0 && itemEnc > 0) n = Math.Min(n, enc / itemEnc);
            return Math.Max(0, n);
        }

        // Quantité réservée sur ce stockage par les autres Brictron (toute ressource : un stockage est mono-ressource).
        private static int Reserved(Recepteur r, Type t, Labor exclude)
        {
            List<Reservation> list;
            if (!Pending.TryGetValue(r, out list)) return 0;
            Purge(list);
            int sum = 0;
            foreach (Reservation x in list) if (x.Labor != exclude) sum += x.Amount;
            return sum;
        }

        private static void Reserve(Recepteur r, Labor labor, Type t, int amount)
        {
            if (r == null) return;
            foreach (var kv in Pending) kv.Value.RemoveAll(x => x.Labor == labor);
            List<Reservation> list;
            if (!Pending.TryGetValue(r, out list)) Pending[r] = list = new List<Reservation>();
            list.Add(new Reservation { Labor = labor, Res = t, Amount = amount, Task = labor.CurrentTask, Since = Time.time });
            if (Pending.Count > 128) PurgeAll();
        }

        private static void Release(Recepteur r, Labor labor)
        {
            List<Reservation> list;
            if (Pending.TryGetValue(r, out list)) list.RemoveAll(x => x.Labor == labor);
        }

        private static void Purge(List<Reservation> list)
        {
            float now = Time.time;
            list.RemoveAll(x => x.Labor == null || x.Labor.IsNullOrReleased() || x.Labor.recepteur == null
                || now - x.Since > MaxAge || x.Labor.recepteur.ContentDescription.Value(x.Res) <= 0);
        }

        private static void PurgeAll()
        {
            var empty = new List<Recepteur>();
            foreach (var kv in Pending)
            {
                if (kv.Key == null || kv.Key.IsNullOrReleased()) { empty.Add(kv.Key); continue; }
                Purge(kv.Value);
                if (kv.Value.Count == 0) empty.Add(kv.Key);
            }
            foreach (Recepteur r in empty) Pending.Remove(r);
        }
    }
}
