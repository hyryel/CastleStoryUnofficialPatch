# -*- coding: utf-8 -*-
"""Monde voxel du simulateur et Blueprints (formes réelles lues dans les données du jeu, shapes.json)."""
import json
import os

from buildorder import (Grid, NONEMPTY, FULL, NOFLOOR, WALKABLE,
                        O_FULL, O_SLOPE, O_NOFLOOR, O_PASS)

HERE = os.path.dirname(os.path.abspath(__file__))
with open(os.path.join(HERE, 'shapes.json'), encoding='utf-8') as f:
    SHAPES = json.load(f)

# Formes dont le dessus n'est pas sûrement un sol (créneaux, poutres, etc.) : pleines mais sans sol pour le plan.
UNCERTAIN = ('Merlon', 'Beam', 'Brace', 'Column', 'Parapet', 'Arch')


def rot(k, p):
    x, y, z = p
    k %= 4
    if k == 0:
        return (x, y, z)
    if k == 1:
        return (z, y, -x)
    if k == 2:
        return (-x, y, -z)
    return (-z, y, x)


def kind_of(shape, local, nw, ramp=True):
    """Nature de la case posée, vue par le plan (règle portée en C# : BuildOrder.KindOf).
    v1.9.3 : rampe (Wood_Ramp_*) = pente (case debout dans la rampe, comme une marche d'escalier) ; avant : pleine
    (dessus praticable mais 2 voxels au-dessus du sol voisin : jamais atteinte)."""
    if 'Door' in shape:
        return O_PASS
    if 'Stair' in shape and nw:
        return O_SLOPE
    if ramp and 'Ramp' in shape:
        return O_SLOPE
    if any(u in shape for u in UNCERTAIN):
        return O_NOFLOOR
    return O_FULL


class Goal:
    def __init__(self, gid, name, shape, pos, k=0, project=0, truth=None):
        self.id = gid
        self.name = name
        self.shape = shape
        self.project = project
        sh = SHAPES[shape]
        nwset = {tuple(c) for c in sh['notWallSupport']}
        self.cells = []
        self.kinds = []
        self.kinds_v192 = []      # vue du plan jusqu'à v1.9.2 (rampe pleine), pour non-régression
        for c in sh['fantome']:
            w = tuple(a + b for a, b in zip(pos, rot(k, c)))
            self.cells.append(w)
            self.kinds.append(kind_of(shape, c, tuple(c) in nwset))
            self.kinds_v192.append(kind_of(shape, c, tuple(c) in nwset, ramp=False))
        self.access = [tuple(a + b for a, b in zip(pos, rot(k, c))) for c in sh['access']]
        self.support = [tuple(a + b for a, b in zip(pos, rot(k, c))) for c in sh['mainSupportReq']]
        # Validateur du prefab : Plank/Beam = récursif (un appui suffit) ; sinon DefaultBP (MaxMissingSupport).
        self.support_any = sh['validator'] in ('PlankBPValidator', 'BeamBPValidator')
        self.max_missing = sh['maxMissing']
        # Vérité terrain (ce que devient la case dans le monde réel du simulateur) : par défaut = vue du plan,
        # sauf scénario qui teste un écart de modèle.
        self.truth = list(self.kinds) if truth is None else [truth] * len(self.cells)
        self.miny = min(c[1] for c in self.cells)
        self.placed = False
        self.worker = None


class World:
    """Grande grille (même codage que BuildOrder.Grid.cell) ; le bord = reste du monde."""

    def __init__(self, sx, sy, sz, ground=0):
        self.grid = Grid(0, 0, 0, sx, sy, sz)
        g = self.grid
        for x in range(sx):
            for z in range(sz):
                for y in range(ground + 1):
                    g.cell[g.Index(x, y, z)] = NONEMPTY | FULL

    def set(self, p, bits):
        i = self.grid.IndexOf(p)
        if i >= 0:
            self.grid.cell[i] = bits

    def full(self, p):
        self.set(p, NONEMPTY | FULL)

    def bits(self, p):
        i = self.grid.IndexOf(p)
        return self.grid.cell[i] if i >= 0 else 0

    def place(self, goal):
        for c, t in zip(goal.cells, goal.truth):
            if t == O_FULL:
                self.set(c, NONEMPTY | FULL)
            elif t == O_SLOPE:
                self.set(c, NONEMPTY)
            elif t == O_NOFLOOR:
                self.set(c, NONEMPTY | FULL | NOFLOOR)
            # O_PASS : porte, passable

    def is_solid_support(self, p):
        b = self.bits(p)
        return bool(b & FULL) or bool(b & WALKABLE)


# --- aides de construction de scénarios ---

def brick2(goals, name, p, d, project=0, shape='Stone_DoubleBrick'):
    """Double brique posée en p, couvrant p et p+d (d : vecteur unité horizontal)."""
    k = {(0, 0, 1): 0, (1, 0, 0): 1, (0, 0, -1): 2, (-1, 0, 0): 3}[d]
    g = Goal(len(goals), name, shape, p, k, project)
    goals.append(g)
    return g


def brick1(goals, name, p, project=0, shape='Stone_Brick', truth=None):
    g = Goal(len(goals), name, shape, p, 0, project, truth)
    goals.append(g)
    return g
