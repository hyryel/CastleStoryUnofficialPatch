#!/usr/bin/env python3
"""Résumé de Logistique.log (journal unifié v1.4).
Usage : logistique_report.py [chemin du log] [--since HH:MM:SS] [--ordre P#id]
Sortie : par ordre (projet/station) : embauches, tâches, ramassages, dépôts, échecs, distances (d à l'embauche,
trajet par tâche, durée), étendue des ordres multiples ; puis les trajets les plus longs et les tâches échouées."""
import re, sys, collections, statistics
path = "/games/steamapps/common/Castle Story/Castle Story_Data/Managed/Logistique.log"
since = None; only = None
args = sys.argv[1:]
while args:
    a = args.pop(0)
    if a == "--since": since = args.pop(0)
    elif a == "--ordre": only = args.pop(0)
    else: path = a
line_re = re.compile(r"^\[(\d\d:\d\d:\d\d)\] t=([\d.]+) (\w+) (?:\[([^\]]*)\] )?([^;]*?)(?: ; (.*))?$")
def fields(rest):
    d = {}
    for part in (rest or "").split(" ; "):
        if "=" in part:
            k, v = part.split("=", 1); d[k] = v
    return d
def num(v):
    try: return float(v)
    except: return None
orders = collections.defaultdict(lambda: collections.defaultdict(list))
events = collections.Counter(); fails = []; trips = []; extents = {}
with open(path, encoding="utf-8") as f:
    for line in f:
        m = line_re.match(line.rstrip("\n"))
        if not m: continue
        clock, t, evt, order, who, rest = m.groups()
        if since and clock < since: continue
        order = order or "-"
        if only and only not in order: continue
        d = fields(rest); events[evt] += 1; o = orders[order]
        if evt == "EMBAUCHE":
            o["emb"].append(who); x = num(d.get("d", "")); o["d_emb"].append(x) if x is not None else None
            if d.get("ok") == "False": o["emb_ko"].append(who)
        elif evt == "TACHE": o["tache"].append(d.get("desc", ""))
        elif evt == "RAMASSE": o["ram"].append(d.get("pris", "")); x = num(d.get("d", "")); o["d_ram"].append(x) if x is not None else None
        elif evt == "DEPOSE": o["dep"].append(d.get("déposé", ""))
        elif evt == "FIN":
            o["fin"].append(d.get("résultat", "")); x = num(d.get("trajet", "")); y = num(d.get("durée", ""))
            if x is not None: o["trajet"].append(x); trips.append((x, y, clock, order, who, d.get("desc", ""), d.get("résultat", "")))
            if not d.get("résultat", "").startswith("Succes"): fails.append((clock, order, who, d.get("desc", ""), d.get("résultat", "")))
        elif evt == "AU_SOL": o["sol"].append(who)
        elif evt == "ECHANGE": o["echange"].append(who)
        elif evt == "PRODUIT": o["produit"].append(d.get("crée", ""))
        elif evt == "ABSORBE": o["absorbe"].append(who)
def st(xs):
    xs = [x for x in xs if x is not None]
    return "-" if not xs else "n=%d moy=%.0f max=%.0f" % (len(xs), statistics.mean(xs), max(xs))
print("Événements :", ", ".join("%s=%d" % kv for kv in sorted(events.items())))
print()
print("%-40s %4s %4s %4s %4s %4s %4s %3s  %-22s %-24s" % ("ordre", "emb", "ko", "tach", "ram", "dep", "fin", "ech", "d_embauche", "trajet/tâche"))
for order, o in sorted(orders.items(), key=lambda kv: -len(kv[1]["tache"])):
    nfail = sum(1 for r in o["fin"] if not r.startswith("Succes"))
    print("%-40s %4d %4d %4d %4d %4d %4d %3d  %-22s %-24s" % (order[:40], len(o["emb"]), len(o["emb_ko"]), len(o["tache"]), len(o["ram"]), len(o["dep"]), len(o["fin"]), nfail, st(o["d_emb"]), st(o["trajet"])))
    extra = []
    if o["sol"]: extra.append("au_sol=%d" % len(o["sol"]))
    if o["echange"]: extra.append("échanges=%d" % len(o["echange"]))
    if o["produit"]: extra.append("produits=%s" % collections.Counter(o["produit"]).most_common(3))
    if o["absorbe"]: extra.append("absorbe=%d" % len(o["absorbe"]))
    if extra: print("    " + " ; ".join(extra))
print()
print("Trajets les plus longs :")
for x, y, clock, order, who, desc, res in sorted(trips, reverse=True)[:15]:
    print("  %s %6.0f m %6s s  %-28s %-8s %-40s %s" % (clock, x, "%.0f" % y if y is not None else "?", order[:28], who, desc[:40], res))
print()
print("Tâches non réussies (%d) :" % len(fails))
for clock, order, who, desc, res in fails[:40]:
    print("  %s %-28s %-8s %-40s %s" % (clock, order[:28], who, desc[:40], res))
