# -*- coding: utf-8 -*-
"""Exécuteur multi-agents : N Brictron choisissent (priorité vanilla + bonus + refus du plan), vont au stock,
vont à une case d'accès, posent ; déplacements selon le modèle (4 voisins, ±1 voxel). Mesures et détections."""
import math
import random
from collections import deque

from buildorder import (Grid, GoalInfo, BuildPlanCore, Counters, MARGIN, MARGIN_DOWN, MARGIN_UP,
                        O_FULL, O_SLOPE, O_NOFLOOR, O_PASS, FULL)

PICK_TICKS, PLACE_TICKS, AFTER_OK, AFTER_FAIL, THINK = 2, 2, 4, 8, 4
REFRESH = 150          # recalcul périodique (3 s à 50 Hz)
VALVE = 500            # 10 s sans aucune autorisation ni pose en cours : contrôle levé
STALL = 900            # ticks sans pose = arrêt
PENDING = 250          # v1.9.1 : Blueprint posé compté plein au plus 5 s en attendant que la grille relise ses cases


# ---------------------------------------------------------------- plan : adaptateurs

class Adapter:
    """Interface commune : allowed(goal) -> (bool, raison), bonus(goal), signaled() -> {gid: raison}."""
    name = 'vanilla'

    def __init__(self, sim, project):
        self.sim, self.project = sim, project
        self.cost = Counters()
        self.computes = 0
        self.max_compute = 0
        self.max_test = 0
        self.tests = 0

    def allowed(self, g):
        return True, ''

    def bonus(self, g):
        return 0

    def signaled(self):
        return {}

    def threatened(self):
        return set()

    def on_change(self):
        pass


def region_grid(sim, goals):
    xs = [p[0] for g in goals for p in g.cells + g.access]
    ys = [p[1] for g in goals for p in g.cells + g.access]
    zs = [p[2] for g in goals for p in g.cells + g.access]
    x0, y0, z0 = min(xs) - MARGIN, min(ys) - MARGIN_DOWN, min(zs) - MARGIN
    nx = max(xs) - min(xs) + 1 + 2 * MARGIN
    ny = max(ys) - min(ys) + 1 + MARGIN_DOWN + MARGIN_UP
    nz = max(zs) - min(zs) + 1 + 2 * MARGIN
    grid = Grid(x0, y0, z0, nx, ny, nz)
    wg = sim.world.grid
    for x in range(nx):
        for z in range(nz):
            for y in range(ny):
                wi = wg.IndexOf((x0 + x, y0 + y, z0 + z))
                grid.cell[grid.Index(x, y, z)] = sim.plan_cell(wi) if wi >= 0 else 0
    return grid


def Clusterize(goals):
    """Groupes de Blueprints dont les emprises (cases + accès + marges de la grille) se touchent : chaque groupe a sa
    grille et son plan (BuildOrder.Clusterize). Deux groupes disjoints ne peuvent ni se couper ni s'enfermer."""
    n = len(goals)
    box = []
    for g in goals:
        pts = g.cells + g.access
        box.append((min(p[0] for p in pts) - MARGIN, min(p[1] for p in pts) - MARGIN_DOWN, min(p[2] for p in pts) - MARGIN,
                    max(p[0] for p in pts) + MARGIN, max(p[1] for p in pts) + MARGIN_UP, max(p[2] for p in pts) + MARGIN))
    parent = list(range(n))

    def find(i):
        while parent[i] != i:
            parent[i] = parent[parent[i]]
            i = parent[i]
        return i
    for i in range(n):
        a = box[i]
        for j in range(i + 1, n):
            b = box[j]
            if a[0] <= b[3] and b[0] <= a[3] and a[1] <= b[4] and b[1] <= a[4] and a[2] <= b[5] and b[2] <= a[5]:
                ri, rj = find(i), find(j)
                if ri != rj:
                    parent[max(ri, rj)] = min(ri, rj)
    groups = {}
    for i in range(n):
        groups.setdefault(find(i), []).append(goals[i])
    return [groups[k] for k in sorted(groups)]


class Cluster:
    def __init__(self):
        self.core = None
        self.sig = None
        self.time = -10 ** 9
        self.ids = {}
        self.last_ok = 0


class NewOrder(Adapter):
    """v1.9 : plan global par pelage + garde exacte à l'attribution (BuildPlanCore), un plan par groupe."""
    name = 'v1.9'

    def __init__(self, sim, project):
        Adapter.__init__(self, sim, project)
        self.sig = None
        self.clusters = []
        self.where = {}          # gid -> Cluster
        self.time = -10 ** 9
        self.sticky = {}         # exclusions conservées tant que le projet ne gagne pas de Blueprint
        self.known = set()
        self.valve = 0
        self.fasts = 0
        self.pending = {}        # v1.9.1 : gid -> (cellules, kinds, tick) des attribués disparus (posés), BuildOrder.Pending

    FIX = False              # v1.9.1

    def signature(self):
        gs = [g for g in self.sim.goals if g.project == self.project and not g.placed]
        return (tuple(g.id for g in gs), tuple(g.id for g in gs if g.worker is not None))

    def plan(self):
        sig = self.signature()
        if sig == self.sig and self.sim.t - self.time < REFRESH:
            return
        self.sig = sig
        self.time = self.sim.t
        goals = [g for g in self.sim.goals if g.project == self.project and not g.placed]
        ids_now = {g.id for g in goals}
        if self.FIX:
            # BuildOrder.Recluster : attribué disparu = posé ; compté plein jusqu'à ce que la grille relise ses cases.
            byid = {g.id: g for g in self.sim.goals}
            for c in self.clusters:
                if c.core is None:
                    continue
                for gid, i in c.ids.items():
                    if gid not in ids_now and c.core.Infos[i].InFlight:
                        self.pending[gid] = (byid[gid], self.sim.t)
        if not ids_now <= self.known:
            self.sticky.clear()
        self.known |= ids_now
        olds = list(self.clusters)
        used = set()
        self.clusters = []
        self.where = {}
        for members in Clusterize(goals):
            ids = {g.id for g in members}
            csig = (tuple(g.id for g in members), tuple(g.id for g in members if g.worker is not None))
            c = None
            for o in olds:
                if id(o) not in used and ids <= set(o.ids):
                    c = o
                    break
            if c is not None and c.sig == csig and self.sim.t - c.time < REFRESH:
                pass
            elif c is not None and self.sim.t - c.time < REFRESH and self.try_fast(c, members, csig):
                pass
            else:
                c = c or Cluster()
                self.compute(c, members, csig)
            used.add(id(c))
            self.clusters.append(c)
            for g in members:
                self.where[g.id] = c

    def try_fast(self, c, members, csig):
        """BuildOrder.TryFast : attributions et poses traitées sans recalcul complet quand c'est exact."""
        if c.core is None or c.core.Disabled:
            return False
        core = c.core
        ids = {g.id for g in members}
        gone = [gid for gid in c.ids if gid not in ids]
        goals = {g.id: g for g in self.sim.goals}
        for gid in gone:
            if not core.Infos[c.ids[gid]].InFlight or not goals[gid].placed:
                return False                     # disparu sans pose (annulé) : recalcul complet
            if not self.sim.visible(goals[gid]):
                return False                     # BuildOrder.BuiltInWorld : pose pas encore lue → recalcul complet
        hired = []
        for g in members:
            info = core.Infos[c.ids[g.id]]
            if info.InFlight and g.worker is None:
                return False                     # tâche terminée sans pose (échec) : recalcul complet
            if not info.InFlight and g.worker is not None:
                hired.append(c.ids[g.id])
        for h in hired:
            if not core.CanFastHire(h):
                return False
            core.Hire(h)
        if gone:
            wg = self.sim.world.grid
            for gid in gone:
                for cell in goals[gid].cells:
                    for dy in (-1, 0, 1):
                        p = (cell[0], cell[1] + dy, cell[2])
                        i = core.Grid.IndexOf(p)
                        if i >= 0:
                            core.Grid.cell[i] = self.sim.plan_cell(wg.IndexOf(p))
            core.Placed([c.ids[gid] for gid in gone])
        for gid in gone:
            del c.ids[gid]
        c.sig = csig
        self.fasts += 1
        self.cost.add(core.Grid.c)
        core.Grid.c = Counters()
        return True

    def apply_pending(self, grid):
        """BuildOrder.ApplyPending : cases des Blueprints posés que les voxels ne montrent pas encore comptées comme
        posées ; entrée retirée dès que le bloc est lisible (BuiltInWorld ; la grille vient d'être relue), au plus PENDING."""
        for gid in list(self.pending):
            g, t0 = self.pending[gid]
            if self.sim.t - t0 > PENDING or self.sim.visible(g):
                del self.pending[gid]
                continue
            for cc, k in zip(g.cells, g.kinds):
                i = grid.IndexOf(cc)
                if i >= 0:
                    grid.AssumePlaced(i, k)

    def compute(self, c, goals, csig):
        grid = region_grid(self.sim, goals)
        if self.FIX:
            self.apply_pending(grid)
        infos = []
        c.ids = {}
        for g in goals:
            info = GoalInfo(len(infos), g.name)
            for cc, k in zip(g.cells, g.kinds):
                i = grid.IndexOf(cc)
                if i >= 0:
                    info.Cells.append(i)
                    info.Kind.append(k)
            info.Access = [grid.IndexOf(a) for a in g.access if grid.IndexOf(a) >= 0]
            info.SupportCells = [grid.IndexOf(s) for s in g.support if grid.IndexOf(s) >= 0]
            info.MinY = g.miny
            info.SupportAny = g.support_any
            info.MaxMissing = g.max_missing
            info.InFlight = g.worker is not None
            r = self.sticky.get(g.id)
            if r is not None:
                info.Excluded = True
                info.Reason = r
            c.ids[g.id] = info.Id
            infos.append(info)
        core = BuildPlanCore(grid, infos)
        core.People = [i for i in (grid.IndexOf(self.sim.world.grid.Position(b.pos)) for b in self.sim.bots) if i >= 0]
        core.Compute()
        for g in goals:
            info = infos[c.ids[g.id]]
            # v1.9.1 : seule l'exclusion « inaccessible » est retenue ; « sans support » est recalculé à chaque passe.
            if info.Excluded and (not self.FIX or info.Reason != "sans support"):
                self.sticky[g.id] = info.Reason
        cost = grid.c.cost()
        self.computes += 1
        self.max_compute = max(self.max_compute, cost)
        self.cost.add(grid.c)
        grid.c = Counters()
        c.core, c.sig, c.time = core, csig, self.sim.t
        c.last_ok = self.sim.t

    def allowed(self, g):
        self.plan()
        c = self.where.get(g.id)
        if c is None or c.core is None:
            return True, ''
        core = c.core
        gi = c.ids[g.id]
        cached = gi in core.Cache
        # Positions des Brictron relues à chaque appel (BuildOrder : une fois par frame).
        core.People = [i for i in (core.Grid.IndexOf(self.sim.world.grid.Position(b.pos)) for b in self.sim.bots) if i >= 0]
        r = core.Allowed(gi)
        # Sécurité : rien d'autorisé depuis VALVE ticks, aucune pose en cours, un Blueprint non exclu reste.
        if r[0] or any(i.InFlight and not i.Placed for i in core.Infos) or all(i.Excluded or i.Placed for i in core.Infos):
            c.last_ok = self.sim.t
        elif self.sim.t - c.last_ok > VALVE:
            core.Disabled = True
            self.valve += 1
            c.last_ok = self.sim.t
            self.sim.log.append((self.sim.t, 'contrôle levé', '-', g.name))
        if not cached:
            cost = core.Grid.c.cost()
            self.tests += 1
            self.max_test = max(self.max_test, cost)
            self.cost.add(core.Grid.c)
            core.Grid.c = Counters()
        return r

    def bonus(self, g):
        self.plan()
        c = self.where.get(g.id)
        if c is None or c.core is None:
            return 0
        return c.core.Infos[c.ids[g.id]].Bonus

    def signaled(self):
        self.plan()
        out = {}
        for c in self.clusters:
            for gid, i in c.ids.items():
                info = c.core.Infos[i]
                if info.Excluded:
                    out[gid] = info.Reason
        return out

    def threatened(self):
        out = set()
        for c in self.clusters:
            if c.core is not None:
                out |= {c.core.Grid.Position(p) for p in c.core.Threatened}
        return out


class V18Order(Adapter):
    """Règles v1.6.2 → v1.8 (BuildOrder.cs au commit 5421036), pour non-régression."""
    name = 'v1.8'

    def __init__(self, sim, project):
        Adapter.__init__(self, sim, project)
        self.sig = None
        self.time = -10 ** 9
        self.allow = {}
        self.bon = {}
        self.threat = set()

    def plan(self):
        sim = self.sim
        cur = [g for g in sim.goals if g.project == self.project and not g.placed and sim.buildable(g)]
        fut = [g for g in sim.goals if g.project == self.project and not g.placed and not sim.buildable(g)]
        sig = (tuple(g.id for g in cur), tuple(g.id for g in fut), tuple(g.id for g in cur if g.worker))
        if sig == self.sig and sim.t - self.time < REFRESH:
            return
        self.sig, self.time = sig, sim.t
        self.allow, self.bon, self.threat = {}, {}, set()
        goals = cur + fut
        if not goals:
            return
        grid = region_grid(sim, goals)
        C = lambda g: [grid.IndexOf(c) for c in g.cells if grid.IndexOf(c) >= 0]
        A = lambda g: [grid.IndexOf(a) for a in g.access if grid.IndexOf(a) >= 0]
        # v1.8 : cellules pleines par case posée (pas de pente / sans sol : le code v1.8 ne les distinguait pas)
        def flood_with(gl):
            for g in gl:
                if 'Door' in g.shape:
                    continue
                for i in C(g):
                    grid.over[i] = O_FULL
            grid.Flood()
            m = bytearray(grid.n)
            grid.CopyMarks(m)
            for g in gl:
                for i in C(g):
                    grid.over[i] = 0
            return m
        inflight = [g for g in cur if g.worker is not None]
        r0 = flood_with(inflight)
        reach = {g.id: any(r0[a] for a in A(g)) for g in goals}
        if not any(reach[g.id] for g in cur):
            return
        people = []
        for b in sim.bots:
            i = grid.IndexOf(sim.world.grid.Position(b.pos))
            if i >= 0 and r0[i]:
                people.append(i)
        levels = {}
        for g in fut:
            if g.miny not in levels:
                cells = inflight + [o for o in goals if o.worker is None and o.miny < g.miny]
                levels[g.miny] = (cells, flood_with(cells), [])
            if any(levels[g.miny][1][a] for a in A(g)):
                levels[g.miny][2].append(g)

        def touches(m, g):
            for c in C(g):
                if m[c] or (c - 1 >= 0 and m[c - 1]):
                    return True
            return False
        allowed = 0
        for x in cur:
            ok = True
            if not reach[x.id] or x.worker is not None:
                self.allow[x.id] = (True, '')
                allowed += 1
                continue
            why = ''
            if touches(r0, x):
                rx = flood_with(inflight + [x])
                if not any(rx[a] for a in A(x)):
                    ok, why = False, 'celui qui le pose'
                for g in cur:
                    if ok and g is not x and reach[g.id] and not any(rx[a] for a in A(g)):
                        ok, why = False, g.name
                cov = set(C(x)) | {c - 1 for c in C(x)}
                for p in people:
                    if not rx[p] and p not in cov:
                        self.threat.add(grid.Position(p))
                        if ok:
                            ok, why = False, 'Brictron'
            for y, (cells, m, victims) in levels.items():
                if not ok:
                    break
                if not victims or x.miny < y or not touches(m, x):
                    continue
                rl = flood_with(cells + [x])
                for v in victims:
                    if not any(rl[a] for a in A(v)):
                        ok, why = False, v.name + ' (à venir)'
                        break
            self.allow[x.id] = (ok, why)
            if ok:
                allowed += 1
        if allowed == 0:
            for k in self.allow:
                self.allow[k] = (True, '')
        # bonus profondeur état final
        for g in goals:
            for i in C(g):
                grid.over[i] = O_FULL
        dist = [0] * grid.n
        from array import array
        d2 = array('i', bytes(4 * grid.n))
        grid.FloodDist(d2)
        raw = {}
        for g in goals:
            ds = [d2[a] for a in A(g) if d2[a] >= 0]
            raw[g.id] = min(ds) if ds else -1
        mx = max([v for v in raw.values()] + [0])
        for g in goals:
            d = raw[g.id] if raw[g.id] >= 0 else mx + 5
            self.bon[g.id] = min(d, 20) * 3
        self.computes += 1
        c = grid.c.cost()
        self.max_compute = max(self.max_compute, c)
        self.cost.add(grid.c)

    def allowed(self, g):
        self.plan()
        return self.allow.get(g.id, (True, ''))

    def bonus(self, g):
        self.plan()
        return self.bon.get(g.id, 0)

    def threatened(self):
        return self.threat


class NewOrder191(NewOrder):
    """v1.9.1 : « sans support » jamais retenu ; Blueprint posé compté plein tant que la grille ne l'a pas relu."""
    name = 'v1.9.1'
    FIX = True


ADAPTERS = {'vanilla': Adapter, 'v1.8': V18Order, 'v1.9': NewOrder, 'v1.9.1': NewOrder191}
CURRENT = 'v1.9.1'


# ---------------------------------------------------------------- exécuteur

class Bot:
    def __init__(self, name, pos):
        self.name = name
        self.pos = pos            # index monde (case debout)
        self.state = 'idle'
        self.goal = None
        self.timer = 0
        self.walked = 0
        self.leg = 0              # distance du trajet en cours (pour l'échec)
        self.trapped = False


class Sim:
    def __init__(self, scenario, variant, seed, nbots=None):
        self.rng = random.Random(seed)
        self.sc = scenario
        self.world, self.goals, self.stock = scenario.build()
        self.variant = variant
        self.t = 0
        self.version = 0
        self.fields = {}
        wg = self.world.grid
        self.stock_idx = [wg.IndexOf(p) for p in self.stock]
        n = nbots or scenario.bots
        starts = self.reachable_cells_near(self.stock_idx, 6)
        self.rng.shuffle(starts)
        self.bots = [Bot('B%d' % k, starts[k % len(starts)]) for k in range(n)]
        for (k, p) in enumerate(getattr(scenario, 'bot_starts', [])):
            if k < len(self.bots):
                self.bots[k].pos = wg.IndexOf(p)
        projects = sorted({g.project for g in self.goals})
        self.adapters = {p: ADAPTERS[variant](self, p) for p in projects}
        self.failures = 0
        self.fail_dist = 0
        self.trap_events = []
        self.pushes = 0
        self.last_place = 0
        self.busy_ticks = 0
        self.evacuations = 0
        self.log = []
        # Relecture des voxels en retard (jeu : Blueprint disparu avant que son bloc soit lisible) : pendant read_lag
        # ticks après la pose, le plan lit l'ancienne valeur des cases posées.
        self.read_lag = getattr(scenario, 'read_lag', 0)
        self.hidden = {}          # index monde -> (tick de pose, ancienne valeur)
        self.events = list(getattr(scenario, 'events', ()))
        self.fired = []

    # ----- monde
    def plan_cell(self, wi):
        """Valeur de la case vue par le plan (lecture des voxels), avec le retard de relecture du scénario."""
        h = self.hidden.get(wi)
        if h is not None:
            if self.t - h[0] < self.read_lag:
                return h[1]
            del self.hidden[wi]
        return self.world.grid.cell[wi]

    def visible(self, g):
        """Pose lisible par le plan (BuildOrder.BuiltInWorld)."""
        wg = self.world.grid
        return all(self.plan_cell(wg.IndexOf(c)) & 1 for c, t in zip(g.cells, g.truth) if t != O_PASS)

    def fire_events(self):
        """Changements de projet en cours de chantier (rangée ajoutée, escalier ajouté…)."""
        for ev in self.events:
            name, pred, fn = ev
            if name in self.fired or not pred(self):
                continue
            self.fired.append(name)
            fn(self.world, self.goals)
            for g in self.goals:
                for c in g.cells:
                    self.cell_owner[c] = g
            self.version += 1
            self.log.append((self.t, 'projet', '-', name))
            for a in self.adapters.values():
                a.on_change()

    def stand(self, i):
        return self.world.grid.StandAt(i)

    def neighbors(self, i):
        g = self.world.grid
        ny, nz, sx, nx = g.ny, g.nz, g.strideX, g.nx
        y = i % ny
        z = (i // ny) % nz
        x = i // sx
        for ok, off in ((x + 1 < nx, sx), (x > 0, -sx), (z + 1 < nz, ny), (z > 0, -ny)):
            if not ok:
                continue
            n = i + off
            if g.StandAt(n):
                yield n
                continue
            if y > 0 and g.StandAt(n - 1):
                yield n - 1
            if y + 1 < ny and g.StandAt(n + 1):
                yield n + 1

    def field(self, key, targets):
        """Distance (coût du pathfinder : pas 10, montée +240, descente +160) vers l'ensemble cible."""
        k = (key, self.version)
        f = self.fields.get(k)
        if f is not None:
            return f
        if len(self.fields) > 400:
            self.fields.clear()
        import heapq
        ny = self.world.grid.ny
        dist = {}
        hq = []
        for t in targets:
            if t >= 0 and self.stand(t) and t not in dist:
                dist[t] = 0
                hq.append((0, t))
        heapq.heapify(hq)
        while hq:
            d, i = heapq.heappop(hq)
            if d > dist.get(i, 1 << 60):
                continue
            yi = i % ny
            for n in self.neighbors(i):
                # coût du pas n -> i (le Brictron va de n vers la cible)
                dy = yi - n % ny
                w = 10 + (240 if dy > 0 else 160 if dy < 0 else 0)
                nd = d + w
                if nd < dist.get(n, 1 << 60):
                    dist[n] = nd
                    heapq.heappush(hq, (nd, n))
        self.fields[k] = dist
        return dist

    def border_field(self):
        g = self.world.grid
        k = ('border', self.version)
        if k in self.fields:
            return self.fields[k]
        targets = []
        for x in range(g.nx):
            for z in range(g.nz):
                if x in (0, g.nx - 1) or z in (0, g.nz - 1):
                    for y in range(g.ny):
                        targets.append(g.Index(x, y, z))
        f = self.field('border0', targets)
        self.fields[k] = f
        return f

    def reachable_cells_near(self, idx, r):
        f = self.field('stock', idx)
        return sorted(i for i, d in f.items() if d <= r)

    def buildable(self, g):
        """Support posé (vanilla : Provider.All()). DefaultBP : aucune case support occupée par un Blueprint non
        posé (BPSupport) et au moins N − MaxMissingSupport cases support pleines. Planche/poutre : un appui plein."""
        if not g.support:
            return True
        solid = 0
        for s in g.support:
            o = self.cell_owner.get(s)
            if o is not None and not o.placed and not g.support_any:
                return False
            if self.world.is_solid_support(s):
                solid += 1
        if g.support_any:
            return solid >= 1
        return solid >= len(g.support) - g.max_missing

    # ----- exécution
    def run(self, max_ticks=20000):
        self.cell_owner = {}
        for g in self.goals:
            for c in g.cells:
                self.cell_owner[c] = g
        for b in self.bots:
            b.timer = self.rng.randint(1, 8)
        while self.t < max_ticks:
            self.t += 1
            order = list(self.bots)
            self.rng.shuffle(order)
            for b in order:
                self.step(b)
            if any(b.goal is not None for b in self.bots):
                self.busy_ticks += sum(1 for b in self.bots if b.goal is not None)
            if self.events:
                self.fire_events()
            todo = [g for g in self.goals if not g.placed and g.id not in self.signaled_all()]
            if not todo and all(b.goal is None for b in self.bots):
                break
            if self.t - self.last_place > STALL:
                break
        return self.result()

    _sig_cache = (None, None)

    def signaled_all(self):
        key = (self.version, self.t // REFRESH)
        if self._sig_cache[0] == key:
            return self._sig_cache[1]
        out = {}
        for a in self.adapters.values():
            out.update(a.signaled())
        self._sig_cache = (key, out)
        return out

    def step(self, b):
        if b.timer > 0:
            b.timer -= 1
            return
        if b.state == 'idle':
            self.think(b)
        elif b.state in ('to_stock', 'to_goal', 'evac'):
            self.move(b)
        elif b.state == 'pick':
            b.state = 'to_goal'
            b.leg = 0
        elif b.state == 'place':
            self.place(b)

    def think(self, b):
        g = self.choose(b)
        if g is None:
            # évacuation (BuildOrder.Evacuate) : Brictron inactif sur une case menacée
            thr = set()
            for a in self.adapters.values():
                thr |= a.threatened()
            if self.world.grid.Position(b.pos) in thr:
                b.state = 'evac'
                b.leg = 0
                self.evacuations += 1
                return
            b.timer = THINK
            return
        g.worker = b
        for a in self.adapters.values():
            a.on_change()
        b.goal = g
        b.state = 'to_stock'
        b.leg = 0

    def choose(self, b):
        wg = self.world.grid
        here = self.field(('bot', b.pos), [b.pos])
        bp = wg.Position(b.pos)
        best, bestp = None, None
        for g in self.goals:
            if g.placed or g.worker is not None or not self.buildable(g):
                continue
            acc = [wg.IndexOf(a) for a in g.access]
            if not any(a in here for a in acc if a >= 0):
                continue                                   # BuildGoalFinder : accès réel testé
            ad = self.adapters[g.project]
            ok, why = ad.allowed(g)
            if not ok:
                continue
            ref = g.miny
            prio = (128 - ref) * 10 - math.dist(bp, g.cells[0])
            lower = False
            for a in g.access:
                if a[1] < ref:
                    o = self.cell_owner.get(a)
                    if o is not None and not o.placed and o is not g:
                        lower = True
                        break
            if lower:
                prio -= 100
            prio += ad.bonus(g)
            prio += self.rng.random() * 0.01
            if bestp is None or prio > bestp:
                best, bestp = g, prio
        return best

    def move(self, b):
        wg = self.world.grid
        if b.state == 'to_stock':
            f = self.field('stock', self.stock_idx)
        elif b.state == 'evac':
            f = self.field('stock', self.stock_idx)
        else:
            g = b.goal
            f = self.field(('goal', g.id), [wg.IndexOf(a) for a in g.access])
        d = f.get(b.pos)
        if d is None:
            if b.state == 'evac':
                b.state = 'idle'
                b.timer = THINK
                return
            self.fail(b)
            return
        if d == 0:
            if b.state == 'to_stock':
                b.state = 'pick'
                b.timer = PICK_TICKS
            elif b.state == 'evac':
                b.state = 'idle'
                b.timer = THINK
            else:
                b.state = 'place'
                b.timer = PLACE_TICKS
            return
        ny = self.world.grid.ny
        best = None
        for n in self.neighbors(b.pos):
            fn = f.get(n)
            if fn is None:
                continue
            dy = n % ny - b.pos % ny
            w = 10 + (240 if dy > 0 else 160 if dy < 0 else 0)
            if fn + w == d and (best is None or self.rng.random() < 0.5):
                best = n
        if best is None:
            self.fail(b)
            return
        b.pos = best
        b.walked += 1
        b.leg += 1

    def fail(self, b):
        self.failures += 1
        self.fail_dist += b.leg
        self.log.append((self.t, 'échec', b.name, b.goal.name if b.goal else '-'))
        if b.goal is not None:
            b.goal.worker = None
        b.goal = None
        b.state = 'idle'
        b.timer = AFTER_FAIL
        for a in self.adapters.values():
            a.on_change()

    def place(self, b):
        g = b.goal
        wg = self.world.grid
        f = self.field(('goal', g.id), [wg.IndexOf(a) for a in g.access])
        if f.get(b.pos) != 0:
            self.fail(b)
            return
        if self.read_lag:
            wg0 = self.world.grid
            for c in g.cells:
                wi = wg0.IndexOf(c)
                self.hidden[wi] = (self.t, wg0.cell[wi])
        self.world.place(g)
        g.placed = True
        g.worker = None
        self.version += 1
        self.last_place = self.t
        self.log.append((self.t, 'pose', b.name, g.name))
        b.goal = None
        b.state = 'idle'
        b.timer = AFTER_OK
        # Brictron dans une case devenue pleine (pieds ou tête) : poussé vers la case debout libre la plus proche.
        cells = {wg.IndexOf(c) for c in g.cells}
        for o in self.bots:
            if o.pos in cells or (o.pos + 1) in cells or not self.stand(o.pos):
                self.push(o)
        # enfermement : Brictron non relié au bord du monde
        bf = self.border_field()
        for o in self.bots:
            if o.pos not in bf:
                if not o.trapped:
                    self.trap_events.append((self.t, o.name, wg.Position(o.pos), g.name))
                o.trapped = True
            else:
                o.trapped = False
        for a in self.adapters.values():
            a.on_change()

    def push(self, o):
        """Brictron dans une case devenue pleine : remonté sur le bloc s'il y tient (hypothèse pessimiste [D]),
        sinon case debout libre la plus proche."""
        wg = self.world.grid
        x, y, z = wg.Position(o.pos)
        best = None
        for dy in (1, 2, 3):
            i = wg.IndexOf((x, y + dy, z))
            if i >= 0 and self.stand(i):
                best = (0, i)
                break
        for r in range(1, 4):
            if best:
                break
            for dx in range(-r, r + 1):
                for dz in range(-r, r + 1):
                    for dy in range(-2, 3):
                        i = wg.IndexOf((x + dx, y + dy, z + dz))
                        if i >= 0 and self.stand(i):
                            d = abs(dx) + abs(dz) + abs(dy)
                            if best is None or d < best[0]:
                                best = (d, i)
        if best:
            self.log.append((self.t, 'poussé', o.name, wg.Position(o.pos), wg.Position(best[1])))
            o.pos = best[1]
            self.pushes += 1

    def diagnose(self):
        """État des Blueprints restants (bloqués) : constructible ? accessible par un Brictron ? autorisé ?"""
        out = []
        wg = self.world.grid
        sig = self.signaled_all()
        for g in self.goals:
            if g.placed or g.id in sig:
                continue
            reach = any(any(wg.IndexOf(a) in self.field(('bot', b.pos), [b.pos]) for a in g.access) for b in self.bots)
            ok, why = self.adapters[g.project].allowed(g)
            out.append('%s : support=%s accès=%s autorisé=%s %s' % (g.name, self.buildable(g), reach, ok, why))
        return out

    def result(self):
        sig = self.signaled_all()
        placed = sum(1 for g in self.goals if g.placed)
        signaled = [g for g in self.goals if not g.placed and g.id in sig]
        blocked = [g for g in self.goals if not g.placed and g.id not in sig]
        bf = self.border_field()
        trapped_end = [b.name for b in self.bots if b.pos not in bf]
        span = max(self.last_place, 1)
        cost = Counters()
        computes = tests = 0
        max_compute = max_test = 0
        for a in self.adapters.values():
            cost.add(a.cost)
            computes += a.computes
            tests += a.tests
            max_compute = max(max_compute, a.max_compute)
            max_test = max(max_test, a.max_test)
        return {
            'placed': placed, 'signaled': len(signaled), 'blocked': len(blocked),
            'blocked_names': [g.name for g in blocked][:8],
            'signaled_names': [(g.name, sig[g.id]) for g in signaled][:12],
            'trap_events': len(self.trap_events), 'trapped_end': len(trapped_end),
            'trap_detail': self.trap_events[:4],
            'failures': self.failures, 'fail_dist': self.fail_dist,
            'walked': sum(b.walked for b in self.bots), 'span': span,
            'parallel': self.busy_ticks / span, 'evacuations': self.evacuations,
            'cost': cost, 'computes': computes, 'tests': tests,
            'max_compute': max_compute, 'max_test': max_test,
            'pushes': self.pushes, 'diag': self.diagnose() if blocked else [],
            'valve': sum(getattr(a, 'valve', 0) for a in self.adapters.values()),
            'fasts': sum(getattr(a, 'fasts', 0) for a in self.adapters.values()),
            'check': self.sc.check(self) if getattr(self.sc, 'check', None) else None,
        }
