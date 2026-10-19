# -*- coding: utf-8 -*-
"""Planificateur d'ordre de construction (v1.9), portage 1:1 de patch/BuildGoalFix/BuildPlanCore.cs.

Mêmes noms, même structure, mêmes ordres de parcours : le C# est comparé instantané par instantané (cs/PlanHarness).

Modèle (06 §10.1, formes lues dans level15/level17, voir shapes.json) :
- case debout p : tête p+1 libre ; p vide sur p-1 plein, ou p en pente, ou volume walkable ;
- déplacement : 4 voisins, même hauteur sinon ±1 voxel ; coût du pathfinder 10 / montée +240 / descente +160 ;
- Blueprint : cases `fantome` (deviennent pleines), `access` (pieds du poseur, −2..+1 autour), `mainSupportReq` ;
  DefaultBP : tout Blueprint support posé avant (BPSupport) + N − MaxMissingSupport supports pleins ;
  planche/poutre : un appui suffit (ancrage par chaîne) ;
- le poseur va à la case d'accès la moins coûteuse (A* multi-cibles) : cases « probables » = coût ≤ min + Tolerance.

Plan :
1. Pelage à rebours depuis l'état final : retirer un Blueprint dont aucun restant ne dépend et dont les cases
   d'accès probables avant la pose restent reliées au bord après la pose ; ordre de construction = inverse.
   Blocage : le plus haut retirable est déclaré non constructible (signalé), pelage recommencé sans lui
   (au-delà de SingleExclusions reprises : toute la rangée bloquée d'un coup).
2. Garde à l'attribution (en cache par état) : X posé maintenant (état actuel, puis état « poses en cours
   terminées ») : le poseur ressort, les poseurs des attribués aussi, le reste se pèle sans nouvelle exclusion.
   Brictron enfermés : réévalué à chaque appel sur les positions courantes ; poches futures → évacuation.
3. Voies rapides : Hire (X testé attribué : ordre = pelage du test + suite) ; Placed (attribué posé).
"""
from array import array

# Bits de Grid.cell (monde réel lu dans les voxels)
NONEMPTY, FULL, NATURE, WALKABLE, NOFLOOR = 1, 2, 4, 8, 16
# Grid.over : cases rendues « posées » virtuellement
O_NONE, O_FULL, O_SLOPE, O_NOFLOOR, O_PASS = 0, 1, 2, 3, 4

MARGIN, MARGIN_DOWN, MARGIN_UP = 6, 3, 4
MAX_CELLS = 600000


class Counters:
    """Coût de calcul (pour le rapport) : floods complets, cases visitées, croissances incrémentales, pelages."""
    def __init__(self):
        self.floods = 0
        self.flood_cells = 0
        self.grows = 0
        self.grow_cells = 0
        self.peels = 0
        self.peel_steps = 0
        self.scan = 0

    def add(self, o):
        for k in vars(self):
            setattr(self, k, getattr(self, k) + getattr(o, k))

    def cost(self):
        # unité ~ lecture de case : flood (visites ×4 voisins) + balayage des accès
        return self.flood_cells * 6 + self.grow_cells * 6 + self.scan


class Grid:
    def __init__(self, x0, y0, z0, nx, ny, nz):
        self.x0, self.y0, self.z0, self.nx, self.ny, self.nz = x0, y0, z0, nx, ny, nz
        self.n = nx * ny * nz
        self.strideX = nz * ny
        self.cell = bytearray(self.n)
        self.over = bytearray(self.n)
        self.mark = array('i', bytes(4 * self.n))
        self.gen = 0
        self.queue = array('i', bytes(4 * self.n))
        self.c = Counters()

    @property
    def Count(self):
        return self.n

    def Index(self, x, y, z):
        return (x * self.nz + z) * self.ny + y

    def IndexOf(self, p):
        x, y, z = p[0] - self.x0, p[1] - self.y0, p[2] - self.z0
        if 0 <= x < self.nx and 0 <= y < self.ny and 0 <= z < self.nz:
            return (x * self.nz + z) * self.ny + y
        return -1

    def Position(self, i):
        ny, nz = self.ny, self.nz
        return (self.x0 + i // self.strideX, self.y0 + i % ny, self.z0 + (i // ny) % nz)

    # v1.9.1 : case d'un Blueprint qui vient d'être posé mais que la lecture des voxels ne voit pas encore
    # (posé ≠ visible) : comptée comme le bloc posé. Rend vrai si la grille la voit déjà (relue).
    def AssumePlaced(self, i, kind):
        b = self.cell[i]
        if kind == O_PASS:
            return True
        if b & NONEMPTY:
            return True
        if kind == O_SLOPE:
            self.cell[i] = b | NONEMPTY
        elif kind == O_NOFLOOR:
            self.cell[i] = b | NONEMPTY | FULL | NOFLOOR
        else:
            self.cell[i] = b | NONEMPTY | FULL
        return False

    # Case debout (pieds en i).
    def StandAt(self, i):
        cell, over, ny = self.cell, self.over, self.ny
        b = cell[i]
        if b & NATURE:
            return False
        o = over[i]
        if o == O_FULL or o == O_NOFLOOR:
            return False
        y = i % ny
        if y + 1 < ny:
            bu = cell[i + 1]
            ou = over[i + 1]
            if (bu & NONEMPTY) or ou == O_FULL or ou == O_NOFLOOR or ou == O_SLOPE:
                return False
        if o == O_SLOPE:
            return True
        if b & WALKABLE:
            return True
        if b & NONEMPTY:
            return (b & FULL) == 0          # pente du monde
        if y == 0:
            return False
        bd = cell[i - 1]
        od = over[i - 1]
        if od == O_FULL:
            return True
        if od != O_NONE:
            return False
        return (bd & FULL) != 0 and (bd & NOFLOOR) == 0

    def Reached(self, i):
        return i >= 0 and self.mark[i] == self.gen

    # Parcours en largeur des cases debout depuis le bord horizontal de la grille (= reste du monde).
    def Flood(self):
        self.gen += 1
        gen, mark, q = self.gen, self.mark, self.queue
        nx, ny, nz = self.nx, self.ny, self.nz
        tail = 0
        for x in range(nx):
            for z in range(nz):
                if x != 0 and z != 0 and x != nx - 1 and z != nz - 1:
                    continue
                base = (x * nz + z) * ny
                for y in range(ny):
                    i = base + y
                    if self.StandAt(i):
                        mark[i] = gen
                        q[tail] = i
                        tail += 1
        visited = self._Bfs(0, tail)
        self.c.floods += 1
        self.c.flood_cells += visited + 2 * (nx + nz) * ny

    def _Bfs(self, head, tail):
        gen, mark, q = self.gen, self.mark, self.queue
        nx, ny, nz, sx = self.nx, self.ny, self.nz, self.strideX
        stand = self.StandAt
        start = head
        while head < tail:
            i = q[head]
            head += 1
            y = i % ny
            z = (i // ny) % nz
            x = i // sx
            for ok, off in ((x + 1 < nx, sx), (x > 0, -sx), (z + 1 < nz, ny), (z > 0, -ny)):
                if not ok:
                    continue
                n = i + off
                if stand(n):
                    if mark[n] != gen:
                        mark[n] = gen
                        q[tail] = n
                        tail += 1
                    continue
                if y > 0 and stand(n - 1) and mark[n - 1] != gen:
                    mark[n - 1] = gen
                    q[tail] = n - 1
                    tail += 1
                if y + 1 < ny and stand(n + 1) and mark[n + 1] != gen:
                    mark[n + 1] = gen
                    q[tail] = n + 1
                    tail += 1
        return tail - start

    # Coût de chemin depuis le bord (Dijkstra), modèle du pathfinder : pas 10, montée +240, descente +160
    # (unités de 10 : 1, 25, 17). Marque aussi les cases atteintes (mark == gen). Sert à prévoir la case d'accès
    # que choisira le poseur (la moins coûteuse depuis l'extérieur, A* multi-cibles du jeu).
    def CostFlood(self, cost):
        import heapq
        self.gen += 1
        gen, mark = self.gen, self.mark
        nx, ny, nz, sx = self.nx, self.ny, self.nz, self.strideX
        hq = []
        for x in range(nx):
            for z in range(nz):
                if x != 0 and z != 0 and x != nx - 1 and z != nz - 1:
                    continue
                base = (x * nz + z) * ny
                for y in range(ny):
                    i = base + y
                    if self.StandAt(i):
                        mark[i] = gen
                        cost[i] = 0
                        hq.append((0, i))
        heapq.heapify(hq)
        popped = 0
        while hq:
            d, i = heapq.heappop(hq)
            if d != cost[i]:
                continue
            popped += 1
            y = i % ny
            z = (i // ny) % nz
            x = i // sx
            for ok, off in ((x + 1 < nx, sx), (x > 0, -sx), (z + 1 < nz, ny), (z > 0, -ny)):
                if not ok:
                    continue
                n = i + off
                if self.StandAt(n):
                    cand = ((n, 1),)
                else:
                    cand = ()
                    if y > 0 and self.StandAt(n - 1):
                        cand += ((n - 1, 17),)
                    if y + 1 < ny and self.StandAt(n + 1):
                        cand += ((n + 1, 25),)
                for c, w in cand:
                    nd = d + w
                    if mark[c] != gen or nd < cost[c]:
                        mark[c] = gen
                        cost[c] = nd
                        heapq.heappush(hq, (nd, c))
        self.c.floods += 1
        self.c.flood_cells += 2 * popped + 2 * (nx + nz) * ny

    def CopyMarks(self, dst):
        gen, mark = self.gen, self.mark
        for i in range(self.n):
            dst[i] = 1 if mark[i] == gen else 0

    # Distance au bord (pas) dans l'état courant ; -1 si non relié (pour le bonus de profondeur).
    def FloodDist(self, dist):
        self.Flood()
        for i in range(self.n):
            dist[i] = -1
        gen, mark = self.gen, self.mark
        nx, ny, nz, sx = self.nx, self.ny, self.nz, self.strideX
        from collections import deque
        dq = deque()
        for x in range(nx):
            for z in range(nz):
                if x != 0 and z != 0 and x != nx - 1 and z != nz - 1:
                    continue
                for y in range(ny):
                    i = (x * nz + z) * ny + y
                    if mark[i] == gen:
                        dist[i] = 0
                        dq.append(i)
        while dq:
            i = dq.popleft()
            y = i % ny
            z = (i // ny) % nz
            x = i // sx
            for ok, off in ((x + 1 < nx, sx), (x > 0, -sx), (z + 1 < nz, ny), (z > 0, -ny)):
                if not ok:
                    continue
                n = i + off
                if self.StandAt(n):
                    cand = (n,)
                else:
                    cand = ()
                    if y > 0 and self.StandAt(n - 1):
                        cand += (n - 1,)
                    if y + 1 < ny and self.StandAt(n + 1):
                        cand += (n + 1,)
                for c in cand:
                    if dist[c] < 0:
                        dist[c] = dist[i] + 1
                        dq.append(c)


class _Marks:
    """Vue « atteinte ? » sur les marques courantes de la grille (pour Likely)."""
    def __init__(self, grid):
        self.mark, self.gen = grid.mark, grid.gen

    def __getitem__(self, i):
        return self.mark[i] == self.gen


class GoalInfo:
    def __init__(self, gid, name):
        self.Id = gid
        self.Name = name
        self.Cells = []          # indices de grille (deviennent pleines)
        self.Kind = []           # O_FULL / O_SLOPE / O_NOFLOOR / O_PASS par case
        self.Access = []         # indices de grille
        self.SupportCells = []   # indices (mainSupportReq)
        self.SupportAny = False  # planches/poutres (validateurs récursifs) : un support suffit
        self.MaxMissing = 0      # ValidatorComponent.MaxMissingSupport
        self.WorldSupport = 0    # cases support déjà pleines dans le monde
        self.MinY = 0
        self.Supporters = []     # index des GoalInfo qui le portent
        self.Dependents = []     # index des GoalInfo qui reposent sur lui
        self.InFlight = False
        self.Placed = False      # attribué puis posé (voie rapide) : cases pleines dans le monde
        self.Excluded = False
        self.Reason = ""
        self.Reach = False       # accès relié dans l'état de base
        self.Bonus = 0
        self.Rank = -1           # rang dans l'ordre de construction (0 = premier)


class BuildPlanCore:
    """Cœur du plan (sans Unity). Le simulateur et BuildOrder.cs l'alimentent avec la grille et les Blueprints."""

    BonusPerStep, MaxDepthSteps = 3, 20
    SingleExclusions = 4     # au-delà : exclusion par couche (toute la rangée bloquée la plus haute d'un coup)
    Tolerance = 20           # écart de coût (×10 du pathfinder) en deçà duquel une case d'accès peut être choisie

    def __init__(self, grid, infos):
        self.Grid = grid
        self.Infos = infos           # liste de GoalInfo, Id = index
        self.R = []                  # Blueprints restants (ni posés, ni attribués, ni exclus)
        self.Order = []              # ordre de construction (index), base
        self.BaseMarks = bytearray(grid.n)
        self.FutureMarks = bytearray(grid.n)
        self.BaseCost = array('i', bytes(4 * grid.n))
        self.FutureCost = array('i', bytes(4 * grid.n))
        self.Cost = array('i', bytes(4 * grid.n))
        self.InR = bytearray(len(infos))
        self.BaseBuilt = bytearray(len(infos))   # attribués (et candidat testé) : posés dans l'état de base
        self.Tried = [0] * len(infos)
        self.Seen = [0] * len(infos)
        self.SeenGen = 0
        self.Cache = {}              # gid -> (ok, raison) : partie structurelle
        self.Lost = {}               # gid -> cases reliées que X couperait (test des Brictron à chaque appel)
        self.PushCheck = True        # v1.9.3 : Brictron poussé sur X (pieds ou tête dans X) compté s'il serait enfermé
        self.HirePrefix = {}         # gid -> pelage des Blueprints d'avant X, X posé (test réussi)
        self.Disabled = False
        self.Note = ""
        self.People = []             # cases debout des Brictron (indices)
        self.Threatened = set()      # cases de Brictron qu'une pose refusée enfermerait
        self.Stuck = ""

    # --- état virtuel ---
    def SetBuilt(self, g, built):
        over = self.Grid.over
        info = self.Infos[g]
        for k, c in enumerate(info.Cells):
            over[c] = info.Kind[k] if built else O_NONE

    def SetInFlight(self, built):
        for info in self.Infos:
            if info.InFlight:
                self.SetBuilt(info.Id, built)

    # Cases d'accès que le poseur peut choisir : atteintes, de coût ≤ minimum + Tolerance (venant de l'extérieur).
    def Likely(self, g, marks, cost):
        acc = self.Infos[g].Access
        best = -1
        for a in acc:
            if marks[a] and (best < 0 or cost[a] < best):
                best = cost[a]
        if best < 0:
            return []
        return [a for a in acc if marks[a] and cost[a] <= best + self.Tolerance]

    def AccessMarked(self, g):
        grid = self.Grid
        for a in self.Infos[g].Access:
            grid.c.scan += 1
            if grid.mark[a] == grid.gen:
                return True
        return False

    # --- calcul de base ---
    def Compute(self):
        infos, grid = self.Infos, self.Grid
        # Dépendances de support (cases support ∩ cases d'un autre Blueprint).
        owner = {}
        for info in infos:
            for c in info.Cells:
                owner[c] = info.Id
        for info in infos:
            info.Supporters = []
            info.Dependents = []
        for info in infos:
            info.WorldSupport = 0
            for s in info.SupportCells:
                o = owner.get(s, -1)
                if o >= 0 and o != info.Id:
                    if o not in info.Supporters:
                        info.Supporters.append(o)
                        infos[o].Dependents.append(info.Id)
                elif (grid.cell[s] & FULL) or (grid.cell[s] & WALKABLE):
                    info.WorldSupport += 1
        # Sans support (transitif) : DefaultBP → au moins N − MaxMissing cases support pleines ou à construire ;
        # planches/poutres → au moins une.
        changed = True
        while changed:
            changed = False
            for info in infos:
                if info.Excluded or info.InFlight or not info.SupportCells:
                    continue
                n = info.WorldSupport
                for s in info.SupportCells:
                    o = owner.get(s, -1)
                    if o >= 0 and o != info.Id and not infos[o].Excluded:
                        n += 1
                need = 1 if info.SupportAny else len(info.SupportCells) - info.MaxMissing
                if n < need:
                    info.Excluded = True
                    info.Reason = "sans support"
                    changed = True
        self.ComputeBonus()
        # État de base : monde + Blueprints attribués (posés quoi qu'il arrive).
        for info in infos:
            self.BaseBuilt[info.Id] = 1 if info.InFlight else 0
            if info.InFlight:
                self.SetBuilt(info.Id, True)
        # Pelage ; une exclusion change l'état final : on recommence sans les exclus, pour que l'ordre ne repose
        # jamais sur un Blueprint qui ne sera pas construit (au plus une reprise par Blueprint).
        restarts = 0
        while True:
            self.R = [i.Id for i in infos if not i.InFlight and not i.Excluded]
            ok, back = self.Peel(self.R, True, restarts >= self.SingleExclusions)
            if ok:
                break
            restarts += 1
        self.Order = list(reversed(back))
        for rank, g in enumerate(self.Order):
            infos[g].Rank = rank
        self.R = [g for g in self.R if not infos[g].Excluded]
        # État futur (attribués posés) : cases atteintes et coûts (test du poseur après les poses en cours).
        self.FloodFuture()
        # Accès dans l'état actuel (attribués PAS encore posés : le poseur doit y aller maintenant) ; journal, test.
        self.FloodBase()
        self.BaseList = [i for i in range(grid.n) if self.BaseMarks[i]]
        for info in infos:
            info.Reach = any(self.BaseMarks[a] for a in info.Access)
        self.Cache.clear()
        self.Lost.clear()
        self.HirePrefix.clear()
        self.Threatened = set()
        return ok

    # --- voies rapides (évitent un recalcul complet) ---

    # X vient d'être attribué alors que son test venait de réussir dans cet état : le nouvel ordre est l'ordre du
    # pelage du test (Blueprints d'avant X, X posé) suivi des Blueprints d'après X, inchangés.
    def CanFastHire(self, g):
        r = self.Cache.get(g)
        return r is not None and r[0] and g in self.HirePrefix and not self.Infos[g].InFlight

    def Hire(self, g):
        x = self.Infos[g]
        prefix = self.HirePrefix[g]
        x.InFlight = True
        self.BaseBuilt[g] = 1
        self.SetBuilt(g, True)
        rank = x.Rank
        order = list(reversed(prefix)) + (self.Order[rank + 1:] if rank >= 0 else [])
        for i in self.Infos:
            i.Rank = -1
        self.Order = order
        for k, h in enumerate(order):
            self.Infos[h].Rank = k
        self.R = [h for h in self.R if h != g]
        self.FloodFuture()
        self.Cache.clear()
        self.Lost.clear()
        self.HirePrefix.clear()
        self.Threatened = set()

    # Blueprints attribués devenus posés (cases relues dans Grid.cell par l'appelant) : l'état futur ne change pas,
    # seul l'état actuel change (accès, cases que X couperait).
    def Placed(self, gs=()):
        for g in gs:
            self.Infos[g].Placed = True
        grid = self.Grid
        self.FloodBase()
        self.BaseList = [i for i in range(grid.n) if self.BaseMarks[i]]
        for info in self.Infos:
            info.Reach = any(self.BaseMarks[a] for a in info.Access)
        self.Cache.clear()
        self.Lost.clear()
        self.HirePrefix.clear()
        self.Threatened = set()

    def FloodFuture(self):
        grid = self.Grid
        grid.CostFlood(self.FutureCost)
        grid.CopyMarks(self.FutureMarks)

    def FloodBase(self):
        grid = self.Grid
        self.SetInFlight(False)
        grid.CostFlood(self.BaseCost)
        grid.CopyMarks(self.BaseMarks)
        self.SetInFlight(True)

    # g peut être posé en dernier parmi les restants : aucun restant ne perd son support sans lui.
    def Removable(self, g):
        infos, inR = self.Infos, self.InR
        for d in infos[g].Dependents:
            if not inR[d]:
                continue
            if not infos[d].SupportAny:
                return False         # DefaultBP : tout support Blueprint doit être posé avant (BPSupport)
            if not self.Anchored(d, g):
                return False
        return True

    # Planche/poutre d reliée à un appui (monde, Blueprint ordinaire ou attribué) par des restants, sans g.
    def Anchored(self, d, skip):
        infos, inR = self.Infos, self.InR
        self.SeenGen += 1
        gen = self.SeenGen
        stack = [d]
        self.Seen[d] = gen
        while stack:
            n = stack.pop()
            if infos[n].WorldSupport > 0:
                return True
            for s in infos[n].Supporters:
                if s == skip or self.Seen[s] == gen:
                    continue
                if self.BaseBuilt[s]:
                    return True
                if not inR[s]:
                    continue
                if not infos[s].SupportAny:
                    return True
                self.Seen[s] = gen
                stack.append(s)
        return False

    # Pelage à rebours. R : Blueprints à construire après l'état virtuel courant (posés virtuellement ici).
    # Rend (ok, ordre de retrait). allowExclude : un blocage déclare le plus haut « du dessus » non constructible.
    def Peel(self, R, allowExclude, batch=False):
        infos, grid = self.Infos, self.Grid
        grid.c.peels += 1
        inR = self.InR
        for g in R:
            inR[g] = 1
            self.SetBuilt(g, True)
        grid.Flood()
        remaining = list(R)
        removed = []
        tried = self.Tried
        epoch = 1
        for g in R:
            tried[g] = 0
        ok = True
        self.Stuck = ""
        while remaining:
            grid.c.peel_steps += 1
            best = -1
            for g in remaining:
                if tried[g] == epoch or not self.Removable(g) or not self.AccessMarked(g):
                    continue
                if best < 0 or self.Prefer(g, best):
                    best = g
            if best < 0:
                victim = -1
                for g in remaining:
                    if not self.Removable(g):
                        continue
                    if victim < 0 or infos[g].MinY > infos[victim].MinY or (infos[g].MinY == infos[victim].MinY and g < victim):
                        victim = g
                if victim < 0:
                    victim = remaining[0]
                if not self.Stuck:
                    self.Stuck = infos[victim].Name
                if not allowExclude:
                    for g in remaining:
                        self.SetBuilt(g, False)
                        inR[g] = 0
                    return False, removed
                infos[victim].Excluded = True
                infos[victim].Reason = "inaccessible une fois le reste construit (escalier manquant ?)"
                if batch:
                    for g in remaining:
                        if g != victim and infos[g].MinY == infos[victim].MinY and self.Removable(g):
                            infos[g].Excluded = True
                            infos[g].Reason = infos[victim].Reason
                for g in remaining:
                    self.SetBuilt(g, False)
                    inR[g] = 0
                return False, removed
            acc = [a for a in infos[best].Access if grid.mark[a] == grid.gen]   # atteintes après la pose
            self.SetBuilt(best, False)
            grid.CostFlood(self.Cost)                                           # état avant la pose
            # Les cases d'accès que le poseur peut choisir avant la pose doivent rester atteintes après (sinon il
            # resterait dans une poche) ; il en faut au moins une.
            likely = self.Likely(best, _Marks(grid), self.Cost)
            okb = bool(likely) and all(a in acc for a in likely)
            if okb:
                remaining.remove(best)
                removed.append(best)
                inR[best] = 0
                epoch += 1
            else:
                self.SetBuilt(best, True)
                grid.Flood()
                tried[best] = epoch
        return ok, removed

    # Préférence de retrait (= posé le plus tard) : le plus haut, puis le plus extérieur (bonus faible).
    def Prefer(self, a, b):
        ia, ib = self.Infos[a], self.Infos[b]
        if ia.MinY != ib.MinY:
            return ia.MinY > ib.MinY
        if ia.Bonus != ib.Bonus:
            return ia.Bonus < ib.Bonus
        return a > b

    # Bonus « intérieur d'abord » : profondeur des accès dans l'état final (inchangé depuis v1.6).
    def ComputeBonus(self):
        infos, grid = self.Infos, self.Grid
        for info in infos:
            if not info.Excluded:
                self.SetBuilt(info.Id, True)
        dist = array('i', bytes(4 * grid.n))
        grid.FloodDist(dist)
        raw = {}
        mx = 0
        for info in infos:
            d = -1
            for a in info.Access:
                if dist[a] >= 0 and (d < 0 or dist[a] < d):
                    d = dist[a]
            raw[info.Id] = d
            mx = max(mx, d)
        for info in infos:
            d = raw[info.Id] if raw[info.Id] >= 0 else mx + 5
            info.Bonus = min(d, self.MaxDepthSteps) * self.BonusPerStep
        for info in infos:
            if not info.Excluded:
                self.SetBuilt(info.Id, False)

    # --- garde à l'attribution ---
    # Garde : partie structurelle (poseur, attribués, suite du plan) en cache par état ; partie « Brictron enfermés »
    # réévaluée à chaque appel sur les positions courantes (People), à partir des cases que X couperait (Lost).
    def Allowed(self, g):
        if self.Disabled:
            return True, ""
        r = self.Cache.get(g)
        if r is None:
            r = self.TestCandidate(g)
            self.Cache[g] = r
        if not r[0]:
            return r
        # Brictron dans une poche que les poses en cours vont fermer : à évacuer (pas un motif de refus de X).
        self.MarkThreatened()
        lost = self.Lost.get(g)
        if lost:
            trapped = False
            for p in self.People:
                if p in lost:
                    self.Threatened.add(p)
                    trapped = True
            if trapped:
                return False, "enfermerait un Brictron"
        return r

    def MarkThreatened(self):
        """Brictron relié aujourd'hui qu'une pose en cours va enfermer (BaseMarks sans FutureMarks) : à évacuer.
        v1.9.3 : appelé aussi à chaque frame (BuildOrder.UpdatePeople), pas seulement sur un refus."""
        for p in self.People:
            if self.BaseMarks[p] and not self.FutureMarks[p]:
                self.Threatened.add(p)

    def PushDest(self, o):
        """Case où le simulateur pousse un Brictron dont la case o cesse d'être debout : première case debout au-dessus
        (1 à 3 voxels), sinon -1 (poussé de côté)."""
        grid = self.Grid
        yo = o % grid.ny
        for dy in (1, 2, 3):
            if yo + dy >= grid.ny:
                return -1
            if grid.StandAt(o + dy):
                return o + dy
        return -1

    def ReachedOrPushed(self, a):
        """Case atteinte, ou plus debout (occupant poussé) ; v1.9.3 : poussé sur une case elle-même reliée."""
        grid = self.Grid
        if grid.mark[a] == grid.gen:
            return True
        if grid.StandAt(a):
            return False
        if not self.PushCheck:
            return True
        d = self.PushDest(a)
        return d < 0 or grid.mark[d] == grid.gen

    def TestCandidate(self, g):
        infos, grid = self.Infos, self.Grid
        x = infos[g]
        self.Lost.pop(g, None)
        if x.Excluded:
            return False, "non constructible : " + x.Reason
        if x.InFlight:
            return True, ""
        before = self.Likely(g, self.BaseMarks, self.BaseCost)
        if not before:
            return False, "inaccessible"
        # 1) État actuel + X (attribués pas encore posés) : le poseur ressort, les attribués restent accessibles,
        #    cases que X couperait (Brictron).
        self.SetInFlight(False)
        self.SetBuilt(g, True)
        self.BaseBuilt[g] = 1
        grid.Flood()
        try:
            if not all(grid.mark[a] == grid.gen for a in before):
                return False, "enfermerait le poseur"
            # Une case qui cesse d'être une case debout (remplie, ou tête bouchée) n'enferme pas : son occupant est
            # poussé. Seules comptent les cases debout coupées du bord (poches).
            for y in infos:
                if y.InFlight and not y.Placed and not all(self.ReachedOrPushed(a)
                                          for a in self.Likely(y.Id, self.BaseMarks, self.BaseCost)):
                    return False, "couperait l'accès de " + y.Name + " (attribué)"
            # Cases reliées aujourd'hui que X couperait (hors cases de X et dessous : Brictron poussé, pas enfermé).
            lost = set()
            mark, gen = grid.mark, grid.gen
            for i in self.BaseList:
                if mark[i] != gen and grid.StandAt(i):
                    lost.add(i)
            grid.c.scan += len(self.BaseList)
            # v1.9.3 : Brictron dont X remplit les pieds ou la tête : poussé vers la première case debout au-dessus
            # (hypothèse pessimiste du simulateur, ex. planche posée au-dessus de lui) ; si elle n'est pas reliée au
            # bord (planches sans rampe), il serait enfermé.
            ny = grid.ny
            for k, c in (enumerate(x.Cells) if self.PushCheck else ()):
                if x.Kind[k] == O_PASS:
                    continue
                y0 = c % ny
                for o in (c, c - 1):
                    if o == c - 1 and y0 == 0:
                        continue
                    if not self.BaseMarks[o]:
                        continue
                    d = self.PushDest(o)
                    if d >= 0 and mark[d] != gen:
                        lost.add(o)
            if lost:
                self.Lost[g] = lost
            # 2) État futur (attribués posés) + X : le poseur ressort aussi une fois les attribués posés.
            self.SetInFlight(True)
            grid.Flood()
            # (cases choisies maintenant ou après les poses en cours : le poseur peut encore y être)
            # (une case remplie par une pose en cours : son occupant est poussé, pas enfermé)
            if not all(self.ReachedOrPushed(a) for a in before) or \
                    not all(self.ReachedOrPushed(a)
                            for a in self.Likely(g, self.FutureMarks, self.FutureCost)):
                return False, "enfermerait le poseur (après les poses en cours)"
            for y in infos:
                if y.InFlight and not y.Placed and not all(self.ReachedOrPushed(a)
                                          for a in self.Likely(y.Id, self.BaseMarks, self.BaseCost)):
                    return False, "enfermerait le poseur de " + y.Name + " (attribué)"
            # Le pelage de base, depuis le même état final (X posé dans les deux cas), retire les Blueprints posés après
            # X dans le même ordre ; il suffit donc de peler ceux d'avant X (preuve suffisante, coût ∝ rang de X).
            rest = self.Order[:x.Rank] if x.Rank >= 0 else [h for h in self.R if h != g]
            ok, back = self.Peel(rest, False)
            if not ok:
                return False, "couperait la suite (" + self.Stuck + ")"
            self.HirePrefix[g] = list(back)      # ordre des Blueprints d'avant X une fois X posé (voie rapide Hire)
            return True, ""
        finally:
            self.SetInFlight(True)
            self.SetBuilt(g, False)
            self.BaseBuilt[g] = 0
