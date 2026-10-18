#!/usr/bin/env python3 -I
"""Dump statique des descriptions d'adjectifs (Apparence / Recepteur._baseCapacity) serialisees
dans les fichiers levelN de Unity 5.6 (serialized file v17, little endian). Lecture seule.
Usage : python3 -I unity_adjectives_dump.py <Castle Story_Data> [filtre_nom_GameObject]
Sortie TSV : fichier, pathID MonoBehaviour, GameObject, script(fileID,pathID), taille JSON, adjectifs top-level.
Script PPtr observes (build Steam 5.6.5f1) : (1,1835)=Apparence, (1,2432)=Recepteur."""
import struct, re, json, glob, os, sys, bisect

def read_objects(d):
    be = lambda o: struct.unpack('>I', d[o:o+4])[0]
    ver, doff = be(8), be(12)
    assert ver == 17, ver
    p = 16
    e = '<' if d[p] == 0 else '>'
    p += 4
    p = d.index(b'\0', p) + 1 + 4          # version string + platform
    tt = d[p]; p += 1
    nt = struct.unpack(e+'i', d[p:p+4])[0]; p += 4
    types = []
    for _ in range(nt):
        cid = struct.unpack(e+'i', d[p:p+4])[0]; p += 4 + 1 + 2
        if cid == 114: p += 16
        p += 16
        if tt:
            nn, sb = struct.unpack(e+'II', d[p:p+8]); p += 8 + nn*24 + sb
        types.append(cid)
    no = struct.unpack(e+'i', d[p:p+4])[0]; p += 4
    objs = {}
    for _ in range(no):
        p = (p + 3) & ~3
        pid = struct.unpack(e+'q', d[p:p+8])[0]
        bs, sz, ti = struct.unpack(e+'IIi', d[p+8:p+20]); p += 20
        objs[pid] = (doff + bs, sz, types[ti])
    return objs, e

def go_name(d, objs, e, pid):
    o = objs[pid][0]
    n = struct.unpack(e+'i', d[o:o+4])[0]
    p = o + 4 + n*12 + 4
    l = struct.unpack(e+'i', d[p:p+4])[0]
    return d[p+4:p+4+l].decode('utf8', 'replace')

def main(data_dir, flt=None):
    for f in sorted(glob.glob(os.path.join(data_dir, 'level*'))):
        if f.endswith('.resS'): continue
        d = open(f, 'rb').read()
        try: objs, e = read_objects(d)
        except Exception as ex:
            print(os.path.basename(f), 'ERR', ex, file=sys.stderr); continue
        for pid, (o, sz, c) in sorted(objs.items()):
            if c != 114: continue
            m = re.search(rb'\[\r\n  \{\r\n    "\$type"', d[o:o+sz])
            if not m: continue
            i = o + m.start()
            n = struct.unpack('<I', d[i-4:i])[0]
            try: j = json.loads(d[i:i+n].decode())
            except Exception: continue
            gf, gp = struct.unpack('<iq', d[o:o+12])
            sc = struct.unpack('<iq', d[o+16:o+28])
            nm = go_name(d, objs, e, gp) if gf == 0 and gp in objs and objs[gp][2] == 1 else '?'
            if flt and flt not in nm: continue
            adj = ' '.join('%s=%s' % (a['$type'].split(',')[0].split('.')[-1], a.get('_valeur', '')) for a in j)
            print('\t'.join([os.path.basename(f), str(pid), nm, '%d,%d' % sc, str(n), adj]))

if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else None)
