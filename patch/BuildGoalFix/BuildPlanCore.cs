using System;
using System.Collections.Generic;

namespace CastleStory.BuildGoalFix
{
    // v1.9 : cœur du plan d'ordre de construction, sans dépendance Unity (compilé aussi hors jeu par
    // patch/verify/buildsim/cs pour comparaison avec le simulateur). Portage 1:1 de patch/verify/buildsim/buildorder.py :
    // mêmes noms, mêmes règles, mêmes ordres de parcours.
    //
    // Modèle (06 §10.1 ; formes réelles lues dans level15/level17, VolumeDataComponent / ValidatorComponent) :
    //  - case debout p : tête p+1 libre ; p vide sur p-1 plein, ou p en pente, ou volume walkable ;
    //  - pas : 4 voisins, même hauteur sinon ±1 voxel ; coût du pathfinder 10 / montée +240 / descente +160 ;
    //  - Blueprint : cases fantome (deviennent pleines), access (pieds du poseur, −2..+1 autour des cases),
    //    mainSupportReq (support). DefaultBP : tout Blueprint support doit être posé avant (BPSupport) et
    //    N − MaxMissingSupport cases support pleines ; planche/poutre (validateurs récursifs) : un appui suffit.
    //  - le poseur va à la case d'accès la moins coûteuse (A* multi-cibles) : cases « probables » = coût depuis
    //    le bord ≤ minimum + Tolerance.
    //
    // Plan :
    //  1. Pelage à rebours depuis l'état final : on retire un à un les Blueprints dont aucun restant ne dépend et dont
    //     les cases d'accès probables avant la pose restent reliées au bord après la pose (pas de poche pour le
    //     poseur) ; l'ordre de construction est l'inverse. Blocage : le plus haut des retirables est déclaré non
    //     constructible et le pelage recommence sans lui (au-delà de SingleExclusions : toute la rangée bloquée).
    //  2. Garde à l'attribution (test exact, en cache par état) : X posé maintenant, le poseur ressort (maintenant et
    //     après les poses en cours), les poseurs des Blueprints attribués aussi, et le reste se pèle sans nouvelle
    //     exclusion. Les Brictron enfermés sont testés à chaque appel (positions courantes).
    //  3. Voies rapides : attribution d'un X testé (ordre = pelage du test + suite inchangée) ; pose d'un attribué
    //     (seul l'état actuel change).
    // Aucune allocation dans les boucles : tableaux et listes réutilisés d'un calcul à l'autre.
    public sealed class PlanGrid
    {
        public const byte NonEmpty = 1, Full = 2, Nature = 4, Walkable = 8, NoFloor = 16;
        public const byte ONone = 0, OFull = 1, OSlope = 2, ONoFloor = 3, OPass = 4;
        public const int StepCost = 1, DownCost = 17, UpCost = 25;   // ×10 = coûts du pathfinder

        public int X0, Y0, Z0, Nx, Ny, Nz, N, StrideX;
        public byte[] Cell = new byte[0];   // monde réel (voxels)
        public byte[] Over = new byte[0];   // pose virtuelle (ONone, OFull, OSlope, ONoFloor, OPass)
        public int[] Mark = new int[0];
        public int[] Queue = new int[0];
        public int Gen;
        private long[] heap = new long[0];
        // Coût (journal / simulateur)
        public long Floods, FloodCells, Scan;

        public void Resize(int x0, int y0, int z0, int nx, int ny, int nz)
        {
            X0 = x0; Y0 = y0; Z0 = z0; Nx = nx; Ny = ny; Nz = nz;
            N = nx * ny * nz;
            StrideX = nz * ny;
            if (Cell.Length < N)
            {
                int cap = N + N / 4;
                Cell = new byte[cap];
                Over = new byte[cap];
                Mark = new int[cap];
                Queue = new int[cap];
                Gen = 0;
            }
            else
            {
                Array.Clear(Cell, 0, N);
                Array.Clear(Over, 0, N);
            }
        }

        public long Cost { get { return FloodCells * 6 + Scan; } }
        public void ResetCost() { Floods = FloodCells = Scan = 0; }

        public int Index(int x, int y, int z) { return (x * Nz + z) * Ny + y; }

        public int IndexOf(int px, int py, int pz)
        {
            int x = px - X0, y = py - Y0, z = pz - Z0;
            if (x >= 0 && y >= 0 && z >= 0 && x < Nx && y < Ny && z < Nz) return (x * Nz + z) * Ny + y;
            return -1;
        }

        // v1.9.1 : case d'un Blueprint qui vient d'être posé mais que la lecture des voxels ne voit pas encore
        // (posé ≠ visible) : comptée comme le bloc posé. Rend vrai si la grille la voit déjà (relue).
        public bool AssumePlaced(int i, byte kind)
        {
            byte b = Cell[i];
            if (kind == OPass) return true;
            if ((b & NonEmpty) != 0) return true;
            if (kind == OSlope) Cell[i] = (byte)(b | NonEmpty);
            else if (kind == ONoFloor) Cell[i] = (byte)(b | NonEmpty | Full | NoFloor);
            else Cell[i] = (byte)(b | NonEmpty | Full);
            return false;
        }

        public void Position(int i, out int x, out int y, out int z)
        {
            x = X0 + i / StrideX;
            y = Y0 + i % Ny;
            z = Z0 + (i / Ny) % Nz;
        }

        // Case debout (pieds en i).
        public bool StandAt(int i)
        {
            byte b = Cell[i];
            if ((b & Nature) != 0) return false;
            byte o = Over[i];
            if (o == OFull || o == ONoFloor) return false;
            int y = i % Ny;
            if (y + 1 < Ny)
            {
                byte bu = Cell[i + 1];
                byte ou = Over[i + 1];
                if ((bu & NonEmpty) != 0 || ou == OFull || ou == ONoFloor || ou == OSlope) return false;
            }
            if (o == OSlope) return true;
            if ((b & Walkable) != 0) return true;
            if ((b & NonEmpty) != 0) return (b & Full) == 0;      // pente du monde
            if (y == 0) return false;
            byte bd = Cell[i - 1];
            byte od = Over[i - 1];
            if (od == OFull) return true;
            if (od != ONone) return false;
            return (bd & Full) != 0 && (bd & NoFloor) == 0;
        }

        public bool Reached(int i) { return i >= 0 && Mark[i] == Gen; }

        // Parcours en largeur des cases debout depuis le bord horizontal de la grille (= reste du monde).
        public void Flood()
        {
            Gen++;
            int tail = 0;
            for (int x = 0; x < Nx; x++)
                for (int z = 0; z < Nz; z++)
                {
                    if (x != 0 && z != 0 && x != Nx - 1 && z != Nz - 1) continue;
                    int b = (x * Nz + z) * Ny;
                    for (int y = 0; y < Ny; y++)
                    {
                        int i = b + y;
                        if (StandAt(i))
                        {
                            Mark[i] = Gen;
                            Queue[tail++] = i;
                        }
                    }
                }
            int visited = Bfs(0, tail);
            Floods++;
            FloodCells += visited + 2 * (Nx + Nz) * Ny;
        }

        private int Bfs(int head, int tail)
        {
            int start = head;
            int gen = Gen;
            while (head < tail)
            {
                int i = Queue[head++];
                int y = i % Ny, z = (i / Ny) % Nz, x = i / StrideX;
                for (int k = 0; k < 4; k++)
                {
                    int off;
                    if (k == 0) { if (x + 1 >= Nx) continue; off = StrideX; }
                    else if (k == 1) { if (x <= 0) continue; off = -StrideX; }
                    else if (k == 2) { if (z + 1 >= Nz) continue; off = Ny; }
                    else { if (z <= 0) continue; off = -Ny; }
                    int n = i + off;
                    if (StandAt(n))
                    {
                        if (Mark[n] != gen) { Mark[n] = gen; Queue[tail++] = n; }
                        continue;
                    }
                    if (y > 0 && StandAt(n - 1) && Mark[n - 1] != gen) { Mark[n - 1] = gen; Queue[tail++] = n - 1; }
                    if (y + 1 < Ny && StandAt(n + 1) && Mark[n + 1] != gen) { Mark[n + 1] = gen; Queue[tail++] = n + 1; }
                }
            }
            return tail - start;
        }

        // Coût de chemin depuis le bord (Dijkstra, tas binaire réutilisé), modèle du pathfinder : pas 10, montée
        // +240, descente +160 (unités de 10 : 1, 25, 17). Marque aussi les cases atteintes (Mark == Gen). Sert à
        // prévoir la case d'accès que choisira le poseur (la moins coûteuse depuis l'extérieur).
        public void CostFlood(int[] cost)
        {
            Gen++;
            int gen = Gen;
            int count = 0;
            for (int x = 0; x < Nx; x++)
                for (int z = 0; z < Nz; z++)
                {
                    if (x != 0 && z != 0 && x != Nx - 1 && z != Nz - 1) continue;
                    int b = (x * Nz + z) * Ny;
                    for (int y = 0; y < Ny; y++)
                    {
                        int i = b + y;
                        if (!StandAt(i)) continue;
                        Mark[i] = gen;
                        cost[i] = 0;
                        Push(ref count, 0, i);
                    }
                }
            int popped = 0;
            while (count > 0)
            {
                long top = Pop(ref count);
                int d = (int)(top >> 32), i = (int)(top & 0xffffffff);
                if (d != cost[i]) continue;
                popped++;
                int y = i % Ny, z = (i / Ny) % Nz, x = i / StrideX;
                for (int k = 0; k < 4; k++)
                {
                    int off;
                    if (k == 0) { if (x + 1 >= Nx) continue; off = StrideX; }
                    else if (k == 1) { if (x <= 0) continue; off = -StrideX; }
                    else if (k == 2) { if (z + 1 >= Nz) continue; off = Ny; }
                    else { if (z <= 0) continue; off = -Ny; }
                    int n = i + off;
                    if (StandAt(n)) { Relax(ref count, cost, gen, n, d + StepCost); continue; }
                    if (y > 0 && StandAt(n - 1)) Relax(ref count, cost, gen, n - 1, d + DownCost);
                    if (y + 1 < Ny && StandAt(n + 1)) Relax(ref count, cost, gen, n + 1, d + UpCost);
                }
            }
            Floods++;
            FloodCells += 2 * popped + 2 * (Nx + Nz) * Ny;
        }

        private void Relax(ref int count, int[] cost, int gen, int c, int nd)
        {
            if (Mark[c] == gen && nd >= cost[c]) return;
            Mark[c] = gen;
            cost[c] = nd;
            Push(ref count, nd, c);
        }

        // Tas binaire (coût << 32 | case).
        private void Push(ref int count, int d, int i)
        {
            if (count >= heap.Length) Array.Resize(ref heap, Math.Max(1024, heap.Length * 2));
            long v = ((long)d << 32) | (uint)i;
            int k = count++;
            while (k > 0)
            {
                int p = (k - 1) >> 1;
                if (heap[p] <= v) break;
                heap[k] = heap[p];
                k = p;
            }
            heap[k] = v;
        }

        private long Pop(ref int count)
        {
            long top = heap[0];
            long last = heap[--count];
            int k = 0;
            while (true)
            {
                int c = 2 * k + 1;
                if (c >= count) break;
                if (c + 1 < count && heap[c + 1] < heap[c]) c++;
                if (heap[c] >= last) break;
                heap[k] = heap[c];
                k = c;
            }
            if (count > 0) heap[k] = last;
            return top;
        }

        public void CopyMarks(byte[] dst)
        {
            for (int i = 0; i < N; i++) dst[i] = (byte)(Mark[i] == Gen ? 1 : 0);
        }

        // Distance au bord (pas) dans l'état courant ; -1 si non relié (bonus de profondeur).
        public void FloodDist(int[] dist)
        {
            Flood();
            for (int i = 0; i < N; i++) dist[i] = -1;
            int tail = 0;
            for (int x = 0; x < Nx; x++)
                for (int z = 0; z < Nz; z++)
                {
                    if (x != 0 && z != 0 && x != Nx - 1 && z != Nz - 1) continue;
                    for (int y = 0; y < Ny; y++)
                    {
                        int i = (x * Nz + z) * Ny + y;
                        if (Mark[i] == Gen) { dist[i] = 0; Queue[tail++] = i; }
                    }
                }
            int head = 0;
            while (head < tail)
            {
                int i = Queue[head++];
                int y = i % Ny, z = (i / Ny) % Nz, x = i / StrideX;
                for (int k = 0; k < 4; k++)
                {
                    int off;
                    if (k == 0) { if (x + 1 >= Nx) continue; off = StrideX; }
                    else if (k == 1) { if (x <= 0) continue; off = -StrideX; }
                    else if (k == 2) { if (z + 1 >= Nz) continue; off = Ny; }
                    else { if (z <= 0) continue; off = -Ny; }
                    int n = i + off;
                    if (StandAt(n))
                    {
                        if (dist[n] < 0) { dist[n] = dist[i] + 1; Queue[tail++] = n; }
                        continue;
                    }
                    if (y > 0 && StandAt(n - 1) && dist[n - 1] < 0) { dist[n - 1] = dist[i] + 1; Queue[tail++] = n - 1; }
                    if (y + 1 < Ny && StandAt(n + 1) && dist[n + 1] < 0) { dist[n + 1] = dist[i] + 1; Queue[tail++] = n + 1; }
                }
            }
        }
    }

    public sealed class PlanGoal
    {
        public int Id;
        public string Name;
        public object Tag;                                      // BuildGoal en jeu
        public readonly List<int> Cells = new List<int>();     // indices de grille (deviennent pleines)
        public readonly List<byte> Kind = new List<byte>();    // OFull / OSlope / ONoFloor / OPass par case
        public readonly List<int> Access = new List<int>();
        public readonly List<int> SupportCells = new List<int>();
        public bool SupportAny;          // planches/poutres : un appui suffit
        public int MaxMissing;           // ValidatorComponent.MaxMissingSupport
        public int WorldSupport;         // cases support déjà pleines dans le monde
        public int MinY;
        public readonly List<int> Supporters = new List<int>();
        public readonly List<int> Dependents = new List<int>();
        public bool InFlight, Placed, Excluded, Reach;
        public string Reason = "";
        public int Bonus;
        public int Rank = -1;

        public void Clear()
        {
            Name = null; Tag = null;
            Cells.Clear(); Kind.Clear(); Access.Clear(); SupportCells.Clear(); Supporters.Clear(); Dependents.Clear();
            SupportAny = false; MaxMissing = 0; WorldSupport = 0; MinY = 0;
            InFlight = Placed = Excluded = Reach = false; Reason = ""; Bonus = 0; Rank = -1;
        }
    }

    public sealed class BuildPlanCore
    {
        public const int BonusPerStep = 3, MaxDepthSteps = 20;
        public const int SingleExclusions = 4;   // au-delà : exclusion par couche (la rangée bloquée la plus haute)
        public const int Tolerance = 20;         // écart de coût (×10 du pathfinder) en deçà duquel une case d'accès peut être choisie
        public const string ReasonInaccessible = "inaccessible une fois le reste construit (escalier manquant ?)";
        public const string ReasonNoSupport = "sans support";

        public readonly PlanGrid Grid = new PlanGrid();
        public readonly List<PlanGoal> Infos = new List<PlanGoal>();
        private readonly List<PlanGoal> pool = new List<PlanGoal>();
        public readonly List<int> R = new List<int>();
        public readonly List<int> Order = new List<int>();
        public byte[] BaseMarks = new byte[0];
        public byte[] FutureMarks = new byte[0];
        private int[] baseCost = new int[0], futureCost = new int[0], cost = new int[0];
        public int[] FutureCost { get { return futureCost; } }
        public int[] BaseCost { get { return baseCost; } }
        private byte[] inR = new byte[0];
        private byte[] baseBuilt = new byte[0];
        private int[] tried = new int[0];
        private int[] seen = new int[0];
        private int seenGen;
        private sbyte[] cacheState = new sbyte[0];          // 0 inconnu, 1 autorisé, 2 refusé (partie structurelle)
        private string[] cacheReason = new string[0];
        private List<int>[] lostCells = new List<int>[0];   // cases debout reliées que X couperait (triées)
        private List<int>[] hirePrefix = new List<int>[0];  // pelage des Blueprints d'avant X, X posé (test réussi)
        private bool[] hasPrefix = new bool[0];
        public readonly List<int> People = new List<int>();
        public readonly List<int> Threatened = new List<int>();
        public bool Disabled;
        public bool PushCheck = true;   // v1.9.3 : Brictron poussé sur X (pieds ou tête dans X) compté s'il serait enfermé
        public string Stuck = "";
        public long Peels, PeelSteps, Tests;
        private readonly Dictionary<int, int> owner = new Dictionary<int, int>();
        private readonly List<int> remaining = new List<int>();
        private readonly List<int> removed = new List<int>();
        private readonly List<int> acc = new List<int>();
        private readonly List<int> likely = new List<int>();
        private readonly List<int> likely2 = new List<int>();
        private readonly List<int> stack = new List<int>();
        private readonly List<int> before = new List<int>();
        private readonly List<int> rest = new List<int>();
        private readonly List<int> order2 = new List<int>();
        private readonly List<int> baseList = new List<int>();   // cases reliées dans l'état actuel (croissant)
        private int[] dist = new int[0];

        // --- alimentation ---
        public void Reset()
        {
            Infos.Clear();
            R.Clear();
            Order.Clear();
            People.Clear();
            Threatened.Clear();
            Disabled = false;
            Stuck = "";
        }

        public PlanGoal AddGoal(string name, object tag)
        {
            int id = Infos.Count;
            if (pool.Count <= id) pool.Add(new PlanGoal());
            PlanGoal g = pool[id];
            g.Clear();
            g.Id = id;
            g.Name = name;
            g.Tag = tag;
            Infos.Add(g);
            return g;
        }

        private void EnsureCapacity()
        {
            int n = Infos.Count;
            if (inR.Length < n)
            {
                int cap = n + 16;
                inR = new byte[cap]; baseBuilt = new byte[cap]; tried = new int[cap]; seen = new int[cap];
                cacheState = new sbyte[cap]; cacheReason = new string[cap]; hasPrefix = new bool[cap];
                var old = lostCells;
                lostCells = new List<int>[cap];
                Array.Copy(old, lostCells, Math.Min(old.Length, cap));
                var oldp = hirePrefix;
                hirePrefix = new List<int>[cap];
                Array.Copy(oldp, hirePrefix, Math.Min(oldp.Length, cap));
            }
            int len = Grid.Cell.Length;
            if (BaseMarks.Length < Grid.N) BaseMarks = new byte[len];
            if (FutureMarks.Length < Grid.N) FutureMarks = new byte[len];
            if (baseCost.Length < Grid.N) baseCost = new int[len];
            if (futureCost.Length < Grid.N) futureCost = new int[len];
            if (cost.Length < Grid.N) cost = new int[len];
            if (dist.Length < Grid.N) dist = new int[len];
            for (int i = 0; i < n; i++)
            {
                inR[i] = 0; baseBuilt[i] = 0; tried[i] = 0; seen[i] = 0;
            }
            ClearCache();
            seenGen = 0;
        }

        // --- état virtuel ---
        public void SetBuilt(int g, bool built)
        {
            PlanGoal info = Infos[g];
            byte[] over = Grid.Over;
            for (int k = 0; k < info.Cells.Count; k++) over[info.Cells[k]] = built ? info.Kind[k] : PlanGrid.ONone;
        }

        private void SetInFlight(bool built)
        {
            for (int g = 0; g < Infos.Count; g++) if (Infos[g].InFlight) SetBuilt(g, built);
        }

        // Cases d'accès que le poseur peut choisir : atteintes (marks), de coût ≤ minimum + Tolerance.
        private void Likely(int g, byte[] marks, int[] costs, List<int> outList)
        {
            outList.Clear();
            List<int> a = Infos[g].Access;
            int best = -1;
            for (int k = 0; k < a.Count; k++)
                if (marks[a[k]] != 0 && (best < 0 || costs[a[k]] < best)) best = costs[a[k]];
            if (best < 0) return;
            for (int k = 0; k < a.Count; k++)
                if (marks[a[k]] != 0 && costs[a[k]] <= best + Tolerance) outList.Add(a[k]);
        }

        // Même chose sur les marques courantes de la grille (état du pelage).
        private void LikelyNow(int g, int[] costs, List<int> outList)
        {
            outList.Clear();
            List<int> a = Infos[g].Access;
            int[] mark = Grid.Mark;
            int gen = Grid.Gen;
            int best = -1;
            for (int k = 0; k < a.Count; k++)
                if (mark[a[k]] == gen && (best < 0 || costs[a[k]] < best)) best = costs[a[k]];
            if (best < 0) return;
            for (int k = 0; k < a.Count; k++)
                if (mark[a[k]] == gen && costs[a[k]] <= best + Tolerance) outList.Add(a[k]);
        }

        private bool AccessMarked(int g)
        {
            List<int> a = Infos[g].Access;
            int[] mark = Grid.Mark;
            int gen = Grid.Gen;
            for (int k = 0; k < a.Count; k++)
            {
                Grid.Scan++;
                if (mark[a[k]] == gen) return true;
            }
            return false;
        }

        // Toutes les cases de la liste sont atteintes (ou, si allowFilled, ne sont plus des cases debout ; v1.9.3 : et
        // l'occupant, poussé sur la première case debout au-dessus, y reste relié).
        private bool AllReached(List<int> cells, bool allowFilled)
        {
            int[] mark = Grid.Mark;
            int gen = Grid.Gen;
            for (int k = 0; k < cells.Count; k++)
            {
                int a = cells[k];
                if (mark[a] == gen) continue;
                if (allowFilled && !Grid.StandAt(a))
                {
                    if (!PushCheck) continue;
                    int d = PushDest(a);
                    if (d < 0 || mark[d] == gen) continue;
                }
                return false;
            }
            return true;
        }

        // v1.9.3 : case où un Brictron dont la case o cesse d'être debout est poussé : première case debout au-dessus
        // (1 à 3 voxels), sinon -1 (poussé de côté). Hypothèse pessimiste du simulateur [D].
        private int PushDest(int o)
        {
            int yo = o % Grid.Ny;
            for (int dy = 1; dy <= 3; dy++)
            {
                if (yo + dy >= Grid.Ny) return -1;
                if (Grid.StandAt(o + dy)) return o + dy;
            }
            return -1;
        }

        // Brictron relié aujourd'hui qu'une pose en cours va enfermer (BaseMarks sans FutureMarks) : à évacuer.
        // v1.9.3 : appelé aussi à chaque frame (BuildOrder.UpdatePeople), pas seulement sur un refus d'attribution.
        public void MarkThreatened()
        {
            for (int k = 0; k < People.Count; k++)
            {
                int p = People[k];
                if (BaseMarks[p] != 0 && FutureMarks[p] == 0 && !Threatened.Contains(p)) Threatened.Add(p);
            }
        }

        // --- calcul de base ---
        public bool Compute()
        {
            EnsureCapacity();
            int count = Infos.Count;
            // Dépendances de support (cases support ∩ cases d'un autre Blueprint).
            owner.Clear();
            for (int g = 0; g < count; g++)
                foreach (int c in Infos[g].Cells) owner[c] = g;
            for (int g = 0; g < count; g++) { Infos[g].Supporters.Clear(); Infos[g].Dependents.Clear(); }
            for (int g = 0; g < count; g++)
            {
                PlanGoal info = Infos[g];
                info.WorldSupport = 0;
                foreach (int s in info.SupportCells)
                {
                    int o;
                    if (owner.TryGetValue(s, out o) && o != g)
                    {
                        if (!info.Supporters.Contains(o))
                        {
                            info.Supporters.Add(o);
                            Infos[o].Dependents.Add(g);
                        }
                    }
                    else if ((Grid.Cell[s] & PlanGrid.Full) != 0 || (Grid.Cell[s] & PlanGrid.Walkable) != 0)
                        info.WorldSupport++;
                }
            }
            // Sans support (transitif) : DefaultBP → au moins N − MaxMissing cases support pleines ou à construire ;
            // planches/poutres → au moins une.
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int g = 0; g < count; g++)
                {
                    PlanGoal info = Infos[g];
                    if (info.Excluded || info.InFlight || info.SupportCells.Count == 0) continue;
                    int n = info.WorldSupport;
                    foreach (int s in info.SupportCells)
                    {
                        int o;
                        if (owner.TryGetValue(s, out o) && o != g && !Infos[o].Excluded) n++;
                    }
                    int need = info.SupportAny ? 1 : info.SupportCells.Count - info.MaxMissing;
                    if (n < need)
                    {
                        info.Excluded = true;
                        info.Reason = ReasonNoSupport;
                        changed = true;
                    }
                }
            }
            ComputeBonus();
            // État de base : monde + Blueprints attribués (posés quoi qu'il arrive).
            for (int g = 0; g < count; g++)
            {
                baseBuilt[g] = (byte)(Infos[g].InFlight ? 1 : 0);
                if (Infos[g].InFlight) SetBuilt(g, true);
            }
            // Pelage ; une exclusion change l'état final : on recommence sans les exclus, pour que l'ordre ne repose
            // jamais sur un Blueprint qui ne sera pas construit.
            bool ok;
            int restarts = 0;
            while (true)
            {
                R.Clear();
                for (int g = 0; g < count; g++) if (!Infos[g].InFlight && !Infos[g].Excluded) R.Add(g);
                ok = Peel(R, true, restarts >= SingleExclusions);
                if (ok) break;
                restarts++;
            }
            Order.Clear();
            for (int k = removed.Count - 1; k >= 0; k--) Order.Add(removed[k]);
            for (int k = 0; k < Order.Count; k++) Infos[Order[k]].Rank = k;
            // État futur (attribués posés) : cases atteintes et coûts (test du poseur après les poses en cours).
            FloodFuture();
            // Accès dans l'état actuel (attribués PAS encore posés : le poseur doit y aller maintenant) ; journal, test.
            FloodBase();
            Threatened.Clear();
            ClearCache();
            return ok;
        }

        private void FloodFuture()
        {
            Grid.CostFlood(futureCost);
            Grid.CopyMarks(FutureMarks);
        }

        private void FloodBase()
        {
            SetInFlight(false);
            Grid.CostFlood(baseCost);
            Grid.CopyMarks(BaseMarks);
            SetInFlight(true);
            baseList.Clear();
            for (int i = 0; i < Grid.N; i++) if (BaseMarks[i] != 0) baseList.Add(i);
            for (int g = 0; g < Infos.Count; g++)
            {
                PlanGoal info = Infos[g];
                info.Reach = false;
                foreach (int a in info.Access) if (BaseMarks[a] != 0) { info.Reach = true; break; }
            }
        }

        // --- voies rapides (évitent un recalcul complet) ---

        // X vient d'être attribué alors que son test venait de réussir dans cet état : le nouvel ordre est l'ordre du
        // pelage du test (Blueprints d'avant X, X posé) suivi des Blueprints d'après X, inchangés.
        public bool CanFastHire(int g)
        {
            return cacheState[g] == 1 && hasPrefix[g] && !Infos[g].InFlight;
        }

        public void Hire(int g)
        {
            PlanGoal x = Infos[g];
            List<int> prefix = hirePrefix[g];
            x.InFlight = true;
            baseBuilt[g] = 1;
            SetBuilt(g, true);
            int rank = x.Rank;
            order2.Clear();
            for (int k = prefix.Count - 1; k >= 0; k--) order2.Add(prefix[k]);
            if (rank >= 0) for (int k = rank + 1; k < Order.Count; k++) order2.Add(Order[k]);
            for (int k = 0; k < Infos.Count; k++) Infos[k].Rank = -1;
            Order.Clear();
            Order.AddRange(order2);
            for (int k = 0; k < Order.Count; k++) Infos[Order[k]].Rank = k;
            R.Remove(g);
            FloodFuture();
            ClearCache();
            Threatened.Clear();
        }

        // Blueprints attribués devenus posés (cases relues dans Grid.Cell par l'appelant) : l'état futur ne change
        // pas, seul l'état actuel change (accès, cases que X couperait).
        public void Placed(List<int> gs)
        {
            for (int k = 0; k < gs.Count; k++) Infos[gs[k]].Placed = true;
            FloodBase();
            ClearCache();
            Threatened.Clear();
        }

        // g peut être posé en dernier parmi les restants : aucun restant ne perd son support sans lui.
        private bool Removable(int g)
        {
            List<int> deps = Infos[g].Dependents;
            for (int k = 0; k < deps.Count; k++)
            {
                int d = deps[k];
                if (inR[d] == 0) continue;
                if (!Infos[d].SupportAny) return false;     // DefaultBP : tout support Blueprint posé avant (BPSupport)
                if (!Anchored(d, g)) return false;
            }
            return true;
        }

        // Planche/poutre d reliée à un appui (monde, Blueprint ordinaire ou attribué) par des restants, sans g.
        private bool Anchored(int d, int skip)
        {
            seenGen++;
            int gen = seenGen;
            stack.Clear();
            stack.Add(d);
            seen[d] = gen;
            while (stack.Count > 0)
            {
                int n = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                if (Infos[n].WorldSupport > 0) return true;
                List<int> sup = Infos[n].Supporters;
                for (int k = 0; k < sup.Count; k++)
                {
                    int s = sup[k];
                    if (s == skip || seen[s] == gen) continue;
                    if (baseBuilt[s] != 0) return true;
                    if (inR[s] == 0) continue;
                    if (!Infos[s].SupportAny) return true;
                    seen[s] = gen;
                    stack.Add(s);
                }
            }
            return false;
        }

        // Pelage à rebours de list (posés virtuellement ici) depuis l'état virtuel courant. Ordre de retrait dans
        // `removed`. allowExclude : un blocage déclare le plus haut retirable non constructible (batch : toute sa
        // rangée) et rend false.
        private bool Peel(List<int> list, bool allowExclude, bool batch = false)
        {
            Peels++;
            remaining.Clear();
            removed.Clear();
            for (int k = 0; k < list.Count; k++)
            {
                int g = list[k];
                inR[g] = 1;
                SetBuilt(g, true);
                remaining.Add(g);
                tried[g] = 0;
            }
            Grid.Flood();
            int epoch = 1;
            Stuck = "";
            while (remaining.Count > 0)
            {
                PeelSteps++;
                int best = -1;
                for (int k = 0; k < remaining.Count; k++)
                {
                    int g = remaining[k];
                    if (tried[g] == epoch || !Removable(g) || !AccessMarked(g)) continue;
                    if (best < 0 || Prefer(g, best)) best = g;
                }
                if (best < 0)
                {
                    int victim = -1;
                    for (int k = 0; k < remaining.Count; k++)
                    {
                        int g = remaining[k];
                        if (!Removable(g)) continue;
                        if (victim < 0 || Infos[g].MinY > Infos[victim].MinY || (Infos[g].MinY == Infos[victim].MinY && g < victim))
                            victim = g;
                    }
                    if (victim < 0) victim = remaining[0];
                    if (Stuck.Length == 0) Stuck = Infos[victim].Name;
                    if (allowExclude)
                    {
                        Infos[victim].Excluded = true;
                        Infos[victim].Reason = ReasonInaccessible;
                        if (batch)
                            for (int k = 0; k < remaining.Count; k++)
                            {
                                int g = remaining[k];
                                if (g != victim && Infos[g].MinY == Infos[victim].MinY && Removable(g))
                                {
                                    Infos[g].Excluded = true;
                                    Infos[g].Reason = ReasonInaccessible;
                                }
                            }
                    }
                    for (int k = 0; k < remaining.Count; k++)
                    {
                        SetBuilt(remaining[k], false);
                        inR[remaining[k]] = 0;
                    }
                    return false;
                }
                acc.Clear();
                foreach (int a in Infos[best].Access) if (Grid.Mark[a] == Grid.Gen) acc.Add(a);   // atteintes après la pose
                SetBuilt(best, false);
                Grid.CostFlood(cost);                                                             // état avant la pose
                // Les cases d'accès que le poseur peut choisir avant la pose doivent rester atteintes après (sinon il
                // resterait dans une poche) ; il en faut au moins une.
                LikelyNow(best, cost, likely);
                bool okb = likely.Count > 0;
                for (int k = 0; k < likely.Count && okb; k++) if (!acc.Contains(likely[k])) okb = false;
                if (okb)
                {
                    remaining.Remove(best);
                    removed.Add(best);
                    inR[best] = 0;
                    epoch++;
                }
                else
                {
                    SetBuilt(best, true);
                    Grid.Flood();
                    tried[best] = epoch;
                }
            }
            return true;
        }

        // Préférence de retrait (= posé le plus tard) : le plus haut, puis le plus extérieur.
        private bool Prefer(int a, int b)
        {
            PlanGoal ia = Infos[a], ib = Infos[b];
            if (ia.MinY != ib.MinY) return ia.MinY > ib.MinY;
            if (ia.Bonus != ib.Bonus) return ia.Bonus < ib.Bonus;
            return a > b;
        }

        // Bonus « intérieur d'abord » : profondeur des accès dans l'état final (inchangé depuis v1.6).
        private void ComputeBonus()
        {
            int count = Infos.Count;
            for (int g = 0; g < count; g++) if (!Infos[g].Excluded) SetBuilt(g, true);
            Grid.FloodDist(dist);
            int mx = 0;
            for (int g = 0; g < count; g++)
            {
                int d = -1;
                foreach (int a in Infos[g].Access)
                    if (dist[a] >= 0 && (d < 0 || dist[a] < d)) d = dist[a];
                Infos[g].Rank = d;     // stockage temporaire de la profondeur brute
                if (d > mx) mx = d;
            }
            for (int g = 0; g < count; g++)
            {
                int d = Infos[g].Rank >= 0 ? Infos[g].Rank : mx + 5;
                Infos[g].Bonus = Math.Min(d, MaxDepthSteps) * BonusPerStep;
                Infos[g].Rank = -1;
            }
            for (int g = 0; g < count; g++) if (!Infos[g].Excluded) SetBuilt(g, false);
        }

        // --- garde à l'attribution ---
        public bool IsCached(int g) { return cacheState[g] != 0; }

        // Partie structurelle (poseur, attribués, suite du plan) en cache par état ; partie « Brictron enfermés »
        // réévaluée à chaque appel sur les positions courantes (People), à partir des cases que X couperait.
        public bool Allowed(int g, out string reason)
        {
            reason = null;
            if (Disabled) return true;
            if (cacheState[g] == 0)
            {
                string r;
                bool ok = TestCandidate(g, out r);
                cacheState[g] = (sbyte)(ok ? 1 : 2);
                cacheReason[g] = r;
            }
            reason = cacheReason[g];
            if (cacheState[g] != 1) return false;
            // Brictron dans une poche que les poses en cours vont fermer : à évacuer (pas un motif de refus de X).
            MarkThreatened();
            List<int> lost = lostCells[g];
            if (lost != null && lost.Count > 0)
            {
                bool trapped = false;
                for (int k = 0; k < People.Count; k++)
                {
                    int p = People[k];
                    if (lost.BinarySearch(p) < 0) continue;
                    if (!Threatened.Contains(p)) Threatened.Add(p);
                    trapped = true;
                }
                if (trapped) { reason = "enfermerait un Brictron"; return false; }
            }
            return true;
        }

        public void ClearCache()
        {
            for (int g = 0; g < Infos.Count; g++)
            {
                cacheState[g] = 0;
                cacheReason[g] = null;
                hasPrefix[g] = false;
                if (lostCells[g] != null) lostCells[g].Clear();
            }
        }

        private bool TestCandidate(int g, out string reason)
        {
            Tests++;
            reason = null;
            PlanGoal x = Infos[g];
            if (lostCells[g] == null) lostCells[g] = new List<int>();
            List<int> lost = lostCells[g];
            lost.Clear();
            hasPrefix[g] = false;
            if (x.Excluded) { reason = "non constructible : " + x.Reason; return false; }
            if (x.InFlight) return true;
            Likely(g, BaseMarks, baseCost, before);
            if (before.Count == 0) { reason = "inaccessible"; return false; }
            // 1) État actuel + X (attribués pas encore posés) : le poseur ressort, les poseurs des attribués aussi,
            //    cases que X couperait (Brictron).
            SetInFlight(false);
            SetBuilt(g, true);
            baseBuilt[g] = 1;
            try
            {
                Grid.Flood();
                if (!AllReached(before, false)) { reason = "enfermerait le poseur"; return false; }
                // Une case qui cesse d'être une case debout (remplie, ou tête bouchée) n'enferme pas : son occupant est
                // poussé. Seules comptent les cases debout coupées du bord (poches).
                for (int y = 0; y < Infos.Count; y++)
                {
                    if (!Infos[y].InFlight || Infos[y].Placed) continue;
                    Likely(y, BaseMarks, baseCost, likely2);
                    if (!AllReached(likely2, true)) { reason = "couperait l'accès de " + Infos[y].Name + " (attribué)"; return false; }
                }
                // Cases debout reliées aujourd'hui que X couperait.
                int[] mark = Grid.Mark;
                int gen = Grid.Gen;
                for (int k = 0; k < baseList.Count; k++)
                {
                    int i = baseList[k];
                    if (mark[i] != gen && Grid.StandAt(i)) lost.Add(i);
                }
                Grid.Scan += baseList.Count;
                // v1.9.3 : Brictron dont X remplit les pieds ou la tête : poussé vers la première case debout au-dessus
                // (planche posée au-dessus de lui) ; si elle n'est pas reliée au bord, il serait enfermé.
                if (PushCheck)
                {
                    int ny = Grid.Ny, before0 = lost.Count;
                    for (int k = 0; k < x.Cells.Count; k++)
                    {
                        if (x.Kind[k] == PlanGrid.OPass) continue;
                        int c = x.Cells[k];
                        for (int o = c; o >= c - 1; o--)
                        {
                            if (o == c - 1 && c % ny == 0) continue;
                            if (BaseMarks[o] == 0) continue;
                            int d = PushDest(o);
                            if (d >= 0 && mark[d] != gen && !lost.Contains(o)) lost.Add(o);
                        }
                    }
                    if (lost.Count != before0) lost.Sort();   // BinarySearch dans Allowed
                }
                // 2) État futur (attribués posés) + X : le poseur ressort aussi une fois les attribués posés (cases
                //    choisies maintenant ou après les poses en cours : il peut encore y être).
                SetInFlight(true);
                Grid.Flood();
                Likely(g, FutureMarks, futureCost, likely);
                // (une case remplie par une pose en cours : son occupant est poussé, pas enfermé)
                if (!AllReached(before, true) || !AllReached(likely, true))
                {
                    reason = "enfermerait le poseur (après les poses en cours)";
                    return false;
                }
                for (int y = 0; y < Infos.Count; y++)
                {
                    if (!Infos[y].InFlight || Infos[y].Placed) continue;
                    Likely(y, BaseMarks, baseCost, likely2);
                    if (!AllReached(likely2, true)) { reason = "enfermerait le poseur de " + Infos[y].Name + " (attribué)"; return false; }
                }
                // Le pelage de base, depuis le même état final (X posé dans les deux cas), retire les Blueprints posés
                // après X dans le même ordre : il suffit de peler ceux d'avant X (preuve suffisante, coût ∝ rang de X).
                rest.Clear();
                if (x.Rank >= 0) for (int k = 0; k < x.Rank; k++) rest.Add(Order[k]);
                else for (int k = 0; k < R.Count; k++) if (R[k] != g) rest.Add(R[k]);
                if (!Peel(rest, false)) { reason = "couperait la suite (" + Stuck + ")"; return false; }
                if (hirePrefix[g] == null) hirePrefix[g] = new List<int>();
                hirePrefix[g].Clear();
                hirePrefix[g].AddRange(removed);   // ordre des Blueprints d'avant X une fois X posé (voie rapide)
                hasPrefix[g] = true;
                return true;
            }
            finally
            {
                SetInFlight(true);
                SetBuilt(g, false);
                baseBuilt[g] = 0;
            }
        }
    }
}
