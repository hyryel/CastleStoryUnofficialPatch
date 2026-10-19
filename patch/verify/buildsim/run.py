# -*- coding: utf-8 -*-
"""Batterie de scénarios du simulateur d'ordre de construction.

Usage : python -I run.py [--seeds N] [--variants v1.9.1,v1.9,v1.8,vanilla] [--only nom1,nom2] [--bots N] [-v]
Sortie : tableau par scénario et variante ; code de sortie 1 si la variante courante (sim.CURRENT) échoue.
Critère (toutes graines, variantes v1.9 et suivantes) : 0 Blueprint bloqué (non posé et non signalé), 0 Brictron
enfermé (même temporairement), signalés dans la plage attendue, sécurité (contrôle levé) jamais déclenchée,
attente propre au scénario (Scenario.check) respectée.
Colonnes : poses, signalés, bloqués (max), enfermements (max), échecs de trajet, distance parcourue, durée (ticks),
parallélisme moyen, calculs complets / voies rapides / tests d'attribution par exécution, coût max d'un calcul
complet et d'un test (unités ≈ lectures de case : cases parcourues × 6 + accès balayés).

Équivalence C# : python -I export.py cases 15 ; python -I export.py --cost cases ;
                 dotnet run -c Release --project cs -- cases 20   (0 différent attendu)
"""
import os
import sys
import time

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8')
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from scenarios import SCENARIOS  # noqa: E402
from sim import Sim, CURRENT  # noqa: E402


def main(argv):
    seeds, variants, only, bots, verbose = 6, [CURRENT, 'v1.9', 'v1.8', 'vanilla'], None, None, False
    k = 0
    while k < len(argv):
        a = argv[k]
        if a == '--seeds':
            seeds = int(argv[k + 1]); k += 1
        elif a == '--variants':
            variants = argv[k + 1].split(','); k += 1
        elif a == '--only':
            only = set(argv[k + 1].split(',')); k += 1
        elif a == '--bots':
            bots = int(argv[k + 1]); k += 1
        elif a == '-v':
            verbose = True
        k += 1
    failed = []
    hdr = '%-24s %-8s %-11s %5s %5s %5s %6s %6s %6s %6s %5s %6s %6s %6s %9s %9s' % (
        'scénario', 'variante', 'résultat', 'poses', 'sign.', 'bloq.', 'enferm', 'échecs', 'dist', 'durée', 'par.',
        'compl.', 'rapid.', 'tests', 'coût/calc', 'coût/test')
    print(hdr)
    print('-' * len(hdr))
    t0 = time.time()
    for sc in SCENARIOS:
        if only and sc.name not in only:
            continue
        for v in variants:
            agg = None
            worst = 'OK'
            for seed in range(seeds):
                r = Sim(sc, v, seed, bots).run()
                bad = r['blocked'] > 0 or r['trap_events'] > 0 or r['trapped_end'] > 0
                lo, hi = sc.expect_signaled
                strict = v.startswith('v1.9')
                if strict and not (lo <= r['signaled'] <= hi) or (strict and r['valve']):
                    bad = True
                if strict and r['check']:
                    bad = True
                if bad:
                    worst = 'bloqué' if r['blocked'] else ('enfermé' if r['trap_events'] or r['trapped_end'] else
                                                           ('sécurité' if r['valve'] else
                                                            ('attente≠' if r['check'] else 'signalé≠')))
                    if verbose:
                        print('   graine %d : %s' % (seed, {k: r[k] for k in ('placed', 'signaled', 'blocked', 'trap_detail', 'failures', 'pushes')}))
                        for line in r['diag'][:8]:
                            print('      ' + line)
                        if r['check']:
                            print('      attente : ' + r['check'])
                elif worst == 'OK' and r['signaled']:
                    worst = 'signalé'
                if agg is None:
                    agg = {k: [] for k in r}
                for kk in r:
                    agg[kk].append(r[kk])
            n = seeds
            avg = lambda kk: sum(agg[kk]) / n
            mx = lambda kk: max(agg[kk])
            cost = sum(c.cost() for c in agg['cost']) / max(1, sum(agg['computes']) + sum(agg['tests']))
            print('%-24s %-8s %-11s %5.1f %5.1f %5d %6d %6.1f %6.0f %6.0f %5.2f %6.1f %6.1f %6.1f %9d %9d' % (
                sc.name, v, worst, avg('placed'), avg('signaled'), mx('blocked'), mx('trap_events') + mx('trapped_end'),
                avg('failures'), avg('walked'), avg('span'), avg('parallel'), avg('computes'), avg('fasts'), avg('tests'),
                max(agg['max_compute']), max(agg['max_test'])))
            if v == CURRENT and worst not in ('OK', 'signalé'):
                failed.append(sc.name)
            if verbose and v == CURRENT:
                print('   signalés :', agg['signaled_names'][0][:6])
    print('\n%.1f s ; échecs %s : %s' % (time.time() - t0, CURRENT, ', '.join(failed) or 'aucun'))
    return 1 if failed else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
