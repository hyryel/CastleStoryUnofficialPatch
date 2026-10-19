# -*- coding: utf-8 -*-
"""Exporte des instantanés de plan (grille + Blueprints) et les résultats du planificateur Python recalculés à
partir de ces seules entrées, pour comparaison avec le portage C# (cs/PlanHarness, même chargement).
Instantanés pris pendant une exécution de la variante courante (sim.CURRENT, graine 0) : au départ puis toutes les `step` poses ; chaque instantané
teste aussi la voie rapide d'attribution (Hire) sur le premier candidat autorisé.
Usage : python -I export.py [dossier_sortie] [step]  |  python -I export.py --cost [dossier_sortie]"""
import base64
import json
import os
import sys

sys.dont_write_bytecode = True
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from scenarios import SCENARIOS  # noqa: E402
import sim as S  # noqa: E402
from buildorder import Grid, GoalInfo, BuildPlanCore  # noqa: E402


def snapshots(sim, ad, name):
    ad.plan()
    out = []
    for k, c in enumerate(ad.clusters):
        s = snapshot(ad, c, '%s_c%d' % (name, k))
        if s:
            out.append(s)
    return out


def fresh_core(d):
    """Cœur recalculé depuis les seules entrées de l'instantané (identique au chargement C#)."""
    g = d['grid']
    grid = Grid(g['x0'], g['y0'], g['z0'], g['nx'], g['ny'], g['nz'])
    grid.cell[:] = base64.b64decode(g['cell'])
    infos = []
    for e in d['goals']:
        i = GoalInfo(len(infos), e['name'])
        i.Cells, i.Kind, i.Access, i.SupportCells = list(e['cells']), list(e['kind']), list(e['access']), list(e['support'])
        i.SupportAny, i.MaxMissing, i.MinY, i.InFlight = e['any'], e['maxMissing'], e['minY'], e['inFlight']
        if e['sticky'] is not None:
            i.Excluded, i.Reason = True, e['sticky']
        infos.append(i)
    core = BuildPlanCore(grid, infos)
    core.Compute()
    core.People = list(d['people'])
    return core


def snapshot(ad, c, name):
    live = c.core
    if live is None:
        return None
    grid = live.Grid
    sticky = {c.ids[gid]: r for gid, r in ad.sticky.items() if gid in c.ids}
    d = {
        'name': name,
        'grid': {'x0': grid.x0, 'y0': grid.y0, 'z0': grid.z0, 'nx': grid.nx, 'ny': grid.ny, 'nz': grid.nz,
                 'cell': base64.b64encode(bytes(grid.cell)).decode('ascii')},
        'people': list(live.People),
        # un Blueprint posé (voie rapide) a ses cases pleines dans la grille : il ne fait plus partie du plan
        'goals': [{
            'name': i.Name, 'cells': list(i.Cells), 'kind': list(i.Kind), 'access': list(i.Access),
            'support': list(i.SupportCells), 'any': i.SupportAny, 'maxMissing': i.MaxMissing, 'minY': i.MinY,
            'inFlight': i.InFlight, 'sticky': sticky.get(i.Id),
        } for i in live.Infos if not i.Placed],
    }
    core = fresh_core(d)
    infos = core.Infos
    order0 = [infos[g].Name for g in core.Order]
    allowed = {}
    for i in infos:
        ok, why = core.Allowed(i.Id)
        allowed[i.Name] = [bool(ok), why]
    hire = None
    for i in infos:
        if allowed[i.Name][0] and not i.InFlight and core.CanFastHire(i.Id):
            core.Hire(i.Id)
            after = {}
            for j in infos:
                ok, why = core.Allowed(j.Id)
                after[j.Name] = [bool(ok), why]
            hire = {'goal': i.Name, 'order': [infos[g].Name for g in core.Order], 'allowed': after}
            break
    d['expect'] = {
        'order': order0,
        'excluded': sorted(i.Name for i in infos if i.Excluded),
        'allowed': allowed,
        'bonus': {i.Name: i.Bonus for i in infos},
        'hire': hire,
    }
    return d


def main(argv):
    if argv and argv[0] == '--cost':
        # Cas de mesure : un seul instantané (départ), sans exécution.
        from scenarios import COST_CASES
        out = argv[1] if len(argv) > 1 else os.path.join(os.path.dirname(os.path.abspath(__file__)), 'cases')
        os.makedirs(out, exist_ok=True)
        for sc in COST_CASES:
            sim = S.Sim(sc, S.CURRENT, 0)
            sim.cell_owner = {c: g for g in sim.goals for c in g.cells}
            for p, ad in sim.adapters.items():
                for s in snapshots(sim, ad, 'cout_%s_p%d_pose0' % (sc.name, p)):
                    with open(os.path.join(out, s['name'] + '.json'), 'w', encoding='utf-8') as f:
                        json.dump(s, f)
                    print(s['name'], 'cases', s['grid']['nx'] * s['grid']['ny'] * s['grid']['nz'], 'Blueprints',
                          len(s['goals']), 'exclus', len(s['expect']['excluded']))
                print('coût max par calcul', ad.max_compute, vars(ad.cost))
        return
    out = argv[0] if argv else os.path.join(os.path.dirname(os.path.abspath(__file__)), 'cases')
    step = int(argv[1]) if len(argv) > 1 else 15
    os.makedirs(out, exist_ok=True)
    n = 0
    for sc in SCENARIOS:
        sim = S.Sim(sc, S.CURRENT, 0)
        snaps = []
        placed_seen = [-1]

        orig_place = sim.place

        def place(b, sim=sim, snaps=snaps, orig=orig_place):
            orig(b)
            k = sum(1 for g in sim.goals if g.placed)
            if k % step == 0 and k != placed_seen[0]:
                placed_seen[0] = k
                for p, ad in sim.adapters.items():
                    snaps.extend(snapshots(sim, ad, '%s_p%d_pose%d' % (sc.name, p, k)))
        sim.place = place
        sim.cell_owner = {c: g for g in sim.goals for c in g.cells}
        for p, ad in sim.adapters.items():
            snaps.extend(snapshots(sim, ad, '%s_p%d_pose0' % (sc.name, p)))
        sim.run()
        for s in snaps:
            with open(os.path.join(out, s['name'] + '.json'), 'w', encoding='utf-8') as f:
                json.dump(s, f)
            n += 1
    print('%d instantanés dans %s' % (n, out))


if __name__ == '__main__':
    main(sys.argv[1:])
