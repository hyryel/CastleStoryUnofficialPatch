# -*- coding: utf-8 -*-
"""Batterie de scénarios (à compléter, jamais réduire). Sol : y=0 plein, cases debout à y=1."""
from world import World, brick2, brick1, Goal
from buildorder import O_FULL, O_NOFLOOR

X, Z = (1, 0, 0), (0, 0, 1)


class Scenario:
    def __init__(self, name, expect, fn, bots=6, size=(32, 12, 32), stock=((4, 1, 4), (5, 1, 4), (4, 1, 5)),
                 expect_signaled=(0, 0), bot_starts=(), read_lag=0, events=(), check=None):
        self.name, self.expect, self.fn, self.bots = name, expect, fn, bots
        self.size, self.stock = size, list(stock)
        self.expect_signaled = expect_signaled
        self.bot_starts = list(bot_starts)
        self.read_lag = read_lag          # ticks pendant lesquels une pose n'est pas encore lue par le plan
        self.events = list(events)        # (nom, prédicat(sim), fonction(monde, goals)) : projet modifié en cours
        self.check = check                # attente propre au scénario : check(sim) -> None ou motif d'échec

    def build(self):
        w = World(*self.size)
        goals = []
        self.fn(w, goals)
        return w, goals, self.stock


def wall_line(goals, x0, z, length, rows, y0=1, tag='W', project=0):
    """Mur droit le long de x, briques doubles en quinconce (simple brique aux bouts des rangées décalées)."""
    for r in range(rows):
        y = y0 + r
        x = x0
        if r % 2 == 1:
            brick1(goals, '%s_r%d_x%d' % (tag, r + 1, x), (x, y, z), project)
            x += 1
        while x + 1 < x0 + length:
            brick2(goals, '%s_r%d_x%d' % (tag, r + 1, x), (x, y, z), X, project)
            x += 2
        if x < x0 + length:
            brick1(goals, '%s_r%d_x%d' % (tag, r + 1, x), (x, y, z), project)


def ring(goals, x0, z0, s, rows, y0=1, tag='T', project=0, skip=()):
    """Anneau carré s×s en briques doubles, disposition du journal v1.8 (rangées impaires : côtés z pleins ;
    rangées paires : côtés x pleins). skip : cases (x, z) laissées libres (porte)."""
    x1, z1 = x0 + s - 1, z0 + s - 1
    for r in range(rows):
        y = y0 + r
        if r % 2 == 0:
            segs = [((x, y, zz), X) for zz in (z0, z1) for x in range(x0, x1, 2)]
            segs += [((xx, y, z), Z) for xx in (x0, x1) for z in range(z0 + 1, z1 - 1, 2)]
            singles = [(x1, y, zz) for zz in (z0, z1)] if s % 2 else []
            singles += [(xx, y, z1 - 1) for xx in (x0, x1)] if (s - 2) % 2 else []
        else:
            segs = [((xx, y, z), Z) for xx in (x0, x1) for z in range(z0, z1, 2)]
            segs += [((x, y, zz), X) for zz in (z0, z1) for x in range(x0 + 1, x1 - 1, 2)]
            singles = [(xx, y, z1) for xx in (x0, x1)] if s % 2 else []
            singles += [(x1 - 1, y, zz) for zz in (z0, z1)] if (s - 2) % 2 else []
        for c in singles:
            if (c[0], c[2]) not in skip:
                brick1(goals, '%s_r%d_%d_%d' % (tag, r + 1, c[0], c[2]), c, project)
        for p, d in segs:
            q = (p[0] + d[0], p[1], p[2] + d[2])
            if (p[0], p[2]) in skip or (q[0], q[2]) in skip:
                for c in (p, q):
                    if (c[0], c[2]) not in skip:
                        brick1(goals, '%s_r%d_%d_%d' % (tag, r + 1, c[0], c[2]), c, project)
                continue
            brick2(goals, '%s_r%d_%d_%d' % (tag, r + 1, p[0], p[2]), p, d, project)


def column(goals, x, z, h, y0=1, tag='C', project=0):
    for k in range(h):
        brick1(goals, '%s_%d_%d_%d' % (tag, x, z, y0 + k), (x, y0 + k, z), project)


# ------------------------------------------------------------------ scénarios

def s_mur_ouvert(w, goals):
    wall_line(goals, 10, 15, 10, 2)


def s_couloir(w, goals):
    # couloir de largeur 1 (z=15) entre deux murs existants de 3, fermé au fond (x=21)
    for x in range(8, 22):
        for y in range(1, 4):
            w.full((x, y, 14))
            w.full((x, y, 16))
    for y in range(1, 4):
        w.full((22, y, 15))
    column(goals, 12, 15, 2, tag='porche')     # colonne de 2 : coupe le couloir
    column(goals, 18, 15, 2, tag='milieu')
    brick1(goals, 'fond_20', (20, 1, 15))
    brick1(goals, 'fond_21', (21, 1, 15))


def s_piece_porte(w, goals):
    ring(goals, 10, 10, 7, 3, skip={(12, 10), (13, 10)})
    goals.append(Goal(len(goals), 'Porte', 'Door', (13, 1, 10), 0))
    column(goals, 13, 13, 2, tag='pilier')


def s_piece_fermee(w, goals):
    ring(goals, 10, 10, 6, 2)
    column(goals, 12, 12, 1, tag='dedans')
    brick1(goals, 'dedans_2', (13, 1, 13))


def s_etage(w, goals):
    # plate-forme existante 1 de haut (on y monte), mur de 2 rangées sur le pourtour, Brictron dessus
    for x in range(10, 18):
        for z in range(10, 18):
            w.full((x, 1, z))
    ring(goals, 10, 10, 8, 2, y0=2)
    column(goals, 13, 13, 2, y0=2, tag='pilier')


def s_tour4(w, goals):
    ring(goals, 10, 10, 6, 4)


def s_tour4_escalier(w, goals):
    ring(goals, 10, 10, 6, 4)
    # escalier extérieur en blocs le long du côté sud (z=9) : hauteurs 1, 2, 3, 4
    for k, x in enumerate(range(11, 15)):
        column(goals, x, 9, k + 1, tag='esc')


def s_deux_chantiers(w, goals):
    wall_line(goals, 4, 6, 8, 2, tag='A', project=0)
    wall_line(goals, 50, 18, 8, 2, tag='B', project=1)


def s_creneaux_2(w, goals):
    wall_line(goals, 10, 15, 10, 2)
    for x in range(10, 20, 4):
        goals.append(Goal(len(goals), 'Merlon_%d' % x, 'Stone_MerlonDouble', (x, 3, 15), 1))


def s_creneaux_3(w, goals):
    wall_line(goals, 10, 15, 10, 3)
    for x in range(10, 20, 4):
        goals.append(Goal(len(goals), 'Merlon_%d' % x, 'Stone_MerlonDouble', (x, 4, 15), 1))


def s_creneaux_3_escalier(w, goals):
    wall_line(goals, 10, 15, 10, 3)
    for x in range(10, 20, 4):
        goals.append(Goal(len(goals), 'Merlon_%d' % x, 'Stone_MerlonDouble', (x, 4, 15), 1))
    # escalier en bout de mur (x=7..9) : hauteurs 1, 2, 3
    for k, x in enumerate(range(7, 10)):
        column(goals, x, 15, k + 1, tag='esc')


def s_poutres(w, goals):
    # deux murs de 3 parallèles, poutres en travers à y=4 posées sur les murs, planches entre
    wall_line(goals, 10, 12, 6, 3, tag='N')
    wall_line(goals, 10, 16, 6, 3, tag='S')
    for x in range(10, 16):
        for z in range(13, 16):
            if x == 15:
                goals.append(Goal(len(goals), 'Poutre_%d_%d' % (x, z), 'Wood_Beam', (x, 3, z), 0))
            else:
                goals.append(Goal(len(goals), 'Planche_%d_%d' % (x, z), 'Wood_SinglePlank', (x, 3, z), 0))


def s_en_l_air(w, goals):
    wall_line(goals, 10, 15, 6, 1)
    brick1(goals, 'flottant', (20, 4, 20))
    brick1(goals, 'flottant_dessus', (20, 5, 20))


def s_reel_v18(w, goals):
    """Tour du journal v1.8 (ordre #-1251598) : anneau x 543..548, z 715..720, rangées y 101..104, briques et
    orientations relevées dans logistique-2026-10-19-v1.8.log (cible_pos). Translation : (-533, -100, -705).
    Stock (palettes) en hauteur à l'ouest, comme dans la partie (y=105 → 5), rampe de terrain."""
    T = lambda x, y, z: (x - 533, y - 100, z - 705)
    rows13 = [(21, 544, 715, (-1, 0, 0)), (20, 546, 715, (-1, 0, 0)), (19, 548, 715, (-1, 0, 0)),
              (11, 544, 720, (-1, 0, 0)), (12, 546, 720, (-1, 0, 0)), (13, 548, 720, (-1, 0, 0)),
              (7, 543, 717, (0, 0, -1)), (8, 543, 719, (0, 0, -1)), (16, 548, 717, (0, 0, -1)), (15, 548, 719, (0, 0, -1))]
    rows24 = [(24, 543, 716, (0, 0, -1)), (25, 543, 718, (0, 0, -1)), (26, 543, 720, (0, 0, -1)),
              (29, 545, 720, (-1, 0, 0)), (30, 547, 720, (-1, 0, 0)), (35, 548, 720, (0, 0, -1)),
              (36, 548, 718, (0, 0, -1)), (37, 548, 716, (0, 0, -1)), (39, 545, 715, (-1, 0, 0)), (40, 547, 715, (-1, 0, 0))]
    ids = {101: 100, 102: 200, 103: 300, 104: 400}   # rangée en centaines : noms uniques (numéros du jeu + 100×rangée)
    for y in (101, 102, 103, 104):
        layout = rows13 if y in (101, 103) else rows24
        for (n, x, z, d) in layout:
            brick2(goals, 'Stone_DoubleBrick (%d)' % (n + ids[y]), T(x, y, z), d)
    # terrain : plateau à y=5 (x <= 2), rampe de x=3 à x=6
    for x in range(0, 7):
        h = 4 if x <= 2 else 4 - (x - 2)
        for z in range(0, w.grid.nz):
            for y in range(1, h + 1):
                w.full((x, y, z))


def s_maison_etage(w, goals):
    """Inspirée de la partie v1.6 : pièce 7×7, murs de 4 rangées, porte, escalier intérieur en blocs,
    plancher à y=4 (planches), créneaux sur le pourtour à y=5."""
    s = 7
    ring(goals, 10, 10, s, 4, skip={(12, 10), (13, 10)})
    goals.append(Goal(len(goals), 'Porte', 'Door', (13, 1, 10), 0))
    # escalier intérieur le long du mur nord (z=15) : x=11..13, hauteurs 1..3 → sommet debout y=4
    for k, x in enumerate(range(11, 14)):
        column(goals, x, 15, k + 1, tag='esc')
    # plancher y=4 sur l'intérieur (x 11..15, z 11..15) sauf trémie au-dessus de l'escalier (z=15, x 11..13)
    for x in range(11, 16):
        for z in range(11, 16):
            if z == 15 and 11 <= x <= 13:
                continue
            # planches orientées est-ouest (appuis x±1 : murs est/ouest, planches voisines)
            goals.append(Goal(len(goals), 'Planche_%d_%d' % (x, z), 'Wood_SinglePlank', (x, 4, z), 1))
    # créneaux sur le mur à y=5, un sur deux
    for x in range(10, 16, 3):
        goals.append(Goal(len(goals), 'Merlon_n_%d' % x, 'Stone_MerlonDouble', (x, 5, 16), 1))


# --- v1.9.1 : journal buildgoalfix-2026-10-19-v1.9.log (ordre #-1132798) ---
# Tour 6×6 sans escalier ; les poses ne sont lues par le plan que ~0,8 s plus tard (Blueprint retiré de la liste avant
# que son bloc soit lisible : en v1.9, la brique du dessus passe « sans support » et l'exclusion est retenue) ; une
# 5e rangée est ajoutée pendant le chantier (26 poses sur 40, comme à 17:49:51).

READ_LAG = 40


def placed_count(sim):
    return sum(1 for g in sim.goals if g.placed)


def stalled(sim):
    """Plus personne au travail et plus rien à poser hors signalés (le joueur réagit)."""
    if any(b.goal is not None for b in sim.bots):
        return False
    sig = sim.signaled_all()
    return all(g.placed or g.id in sig for g in sim.goals)


def add_row5(w, goals):
    ring_row(goals, 10, 10, 6, 4, tag='T')


def add_stairs5(w, goals):
    # escalier extérieur en blocs le long du côté sud (z=9) : hauteurs 1..5
    for k, x in enumerate(range(11, 16)):
        column(goals, x, 9, k + 1, tag='esc')


def ring_row(goals, x0, z0, s, r, y0=1, tag='T', project=0):
    """Rangée r (0 = la plus basse) de l'anneau `ring`."""
    tmp = []
    ring(tmp, x0, z0, s, r + 1, y0, tag, project)
    y = y0 + r
    for g in tmp:
        if g.miny == y:
            g.id = len(goals)
            goals.append(g)


def row_of(g):
    return g.miny


def check_tour5(sim):
    sig = sim.signaled_all()
    rows = {}
    for g in sim.goals:
        if g.name.startswith('esc'):
            continue
        rows.setdefault(row_of(g), []).append(g)
    for y in (1, 2, 3):
        left = [g.name for g in rows.get(y, []) if not g.placed]
        if left:
            return 'rangée %d incomplète : %s' % (y, left[:3])
    if not any(g.placed for g in rows.get(4, [])):
        return 'aucune brique de la 4e rangée posée'
    wrong = [g.name for g in sim.goals if not g.placed and sig.get(g.id) == 'sans support' and g.miny <= 4
             and all(o.placed for o in sim.goals if o.miny == g.miny - 1 and set(o.cells) & set(g.support))]
    if wrong:
        return 'signalé « sans support » alors que le dessous est posé : %s' % wrong[:3]
    return None


def check_all_placed(sim):
    left = [g.name for g in sim.goals if not g.placed]
    return None if not left else '%d non posés : %s' % (len(left), left[:3])


SCENARIOS = [
    Scenario('mur_ouvert', 'tout construit', s_mur_ouvert),
    Scenario('couloir_1', 'fond avant porche', s_couloir),
    Scenario('piece_porte', 'tout construit', s_piece_porte),
    Scenario('piece_fermee', 'intérieur puis murs', s_piece_fermee),
    Scenario('etage_brictron', 'pas d\'enfermement', s_etage, bot_starts=[(13, 2, 13), (14, 2, 14)]),
    Scenario('tour_4_sans_escalier', 'fin ou signalé, jamais bloqué', s_tour4, expect_signaled=(1, 10)),
    Scenario('tour_4_escalier', 'tout construit', s_tour4_escalier),
    Scenario('deux_chantiers', 'tout construit', s_deux_chantiers, size=(64, 10, 26),
             stock=((30, 1, 12), (31, 1, 12), (30, 1, 13))),
    Scenario('creneaux_mur2', 'tout construit', s_creneaux_2),
    Scenario('creneaux_mur3', 'créneaux signalés (≥1)', s_creneaux_3, expect_signaled=(1, 3)),
    Scenario('creneaux_mur3_escalier', 'tout construit', s_creneaux_3_escalier),
    Scenario('planches_poutres', 'tout construit', s_poutres),
    Scenario('en_l_air', '2 signalés (sans support)', s_en_l_air, expect_signaled=(2, 2)),
    Scenario('reel_v18', 'fin ou signalé, jamais bloqué', s_reel_v18, bots=7, size=(24, 12, 24),
             stock=((1, 5, 8), (1, 5, 9), (2, 5, 8)), expect_signaled=(1, 10)),
    Scenario('maison_etage', 'tout construit', s_maison_etage, bots=8, size=(30, 12, 30)),
    Scenario('reel_v19_tour5_relecture', 'r1-3 posées, r4 en partie, reste signalé', s_tour4, bots=7,
             read_lag=READ_LAG, events=[('5e rangée', lambda sim: placed_count(sim) >= 26, add_row5)],
             check=check_tour5, expect_signaled=(11, 19)),
    Scenario('reel_v19_tour5_escalier', 'puis escalier ajouté : tout posé', s_tour4, bots=7,
             read_lag=READ_LAG, events=[('5e rangée', lambda sim: placed_count(sim) >= 26, add_row5),
                                        ('escalier', lambda sim: '5e rangée' in sim.fired and stalled(sim), add_stairs5)],
             check=check_all_placed),
]


# ------------------------------------------------------------------ cas de mesure (coût seulement, hors batterie)

def s_grand_ecart(w, goals):
    """Un seul projet étalé : 4 tours de 5 rangées aux coins d'un carré de 150 (≈ 100 Blueprints, grille ≈ 400 000
    cases). Sert à mesurer le pire coût de calcul (export.py --cost)."""
    for (x0, z0) in ((10, 10), (154, 10), (10, 154), (154, 154)):
        ring(goals, x0, z0, 4, 5, tag='T%d_%d' % (x0, z0))


COST_CASES = [
    Scenario('grand_ecart', 'mesure de coût', s_grand_ecart, bots=8, size=(176, 14, 176),
             stock=((80, 1, 80), (81, 1, 80), (80, 1, 81)), expect_signaled=(0, 40)),
]
