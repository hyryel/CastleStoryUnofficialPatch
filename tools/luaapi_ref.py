#!/usr/bin/env python3
"""Build 04-api-lua-ref.tsv: [LuaDrivable] members + registered globals, cross-referenced with Lua scripts.
Usage: luaapi_ref.py <api.json> <decomp_dir> <game_info_dir> <assetstrings.txt> <out.tsv> <out_stats.json>
"""
import json, os, re, sys
from collections import Counter, defaultdict

api = json.load(open(sys.argv[1]))
DECOMP = sys.argv[2]
INFO = sys.argv[3]
ASSETSTR = set(open(sys.argv[4]).read().split('\n'))
OUT = sys.argv[5]
STATS = sys.argv[6]

# ---------- Lua corpus ----------
def strip_lua_comments(src):
    src = re.sub(r'--\[(=*)\[.*?\]\1\]', lambda m: '\n' * m.group(0).count('\n'), src, flags=re.S)
    out = []
    for line in src.split('\n'):
        # naive: cut at -- not inside a string
        res, q = '', None
        i = 0
        while i < len(line):
            c = line[i]
            if q:
                if c == '\\':
                    res += line[i:i + 2]; i += 2; continue
                if c == q:
                    q = None
            elif c in '"\'':
                q = c
            elif line.startswith('--', i):
                break
            res += c; i += 1
        out.append(res)
    return '\n'.join(out)

files = {}
for base in ('Lua', 'Maps'):
    for dp, dn, fn in os.walk(os.path.join(INFO, base)):
        for f in fn:
            p = os.path.join(dp, f)
            rel = os.path.relpath(p, INFO)
            if f.endswith('.lua'):
                files[rel] = strip_lua_comments(open(p, encoding='utf-8', errors='replace').read())
            elif base == 'Maps' and f.endswith('.json'):
                files[rel] = open(p, encoding='utf-8', errors='replace').read()

# Lua globals assigned in scripts (to exclude from handle candidates)
lua_assigned = set()
for t in files.values():
    for m in re.finditer(r'(?m)^\s*(?:local\s+)?(?:function\s+)?([A-Za-z_]\w*)\s*(?:=|\()', t):
        lua_assigned.add(m.group(1))
    for m in re.finditer(r'\blocal\s+([A-Za-z_][\w,\s]*)', t):
        for n in re.split(r'\s*,\s*', m.group(1)):
            lua_assigned.add(n.strip())

# ---------- usage index (built once) ----------
MEM_RE = re.compile(r'(?:([A-Za-z_]\w*)|[\]\)])?\s*([.:])\s*([A-Za-z_]\w*)\b(\s*=\s*function)?')
BARE_RE = re.compile(r'(?<![\w.:])([A-Za-z_]\w*)\b')
mem_idx = defaultdict(Counter)   # name -> Counter(file)
mem_recv = defaultdict(Counter)  # name -> Counter(receiver)
mem_recv_file = defaultdict(Counter)  # (name, recv) -> Counter(file)
bare_idx = defaultdict(Counter)
for rel, t in files.items():
    if not rel.endswith('.lua'):
        continue
    t2 = re.sub(r'"(?:\\.|[^"\\\n])*"|\'(?:\\.|[^\'\\\n])*\'', '""', t)
    t2 = re.sub(r'\.\.', '  ', t2)  # concat operator
    t2 = re.sub(r'(?<![\w])\d+\.\d*', '0', t2)
    for m in MEM_RE.finditer(t2):
        recv, _, name, isdef = m.groups()
        ls = t2.rfind('\n', 0, m.start()) + 1
        if isdef or re.search(r'\bfunction\s+[\w.:]*$', t2[ls:m.start()]):
            continue
        mem_idx[name][rel] += 1
        if recv:
            mem_recv[name][recv] += 1
            mem_recv_file[(name, recv)][rel] += 1
    for m in BARE_RE.finditer(t2):
        bare_idx[m.group(1)][rel] += 1


def_idx = Counter()
for rel, t in files.items():
    if not rel.endswith('.lua'):
        continue
    for m in re.finditer(r'function\s+(?:[\w.]*[.:])?([A-Za-z_]\w*)\s*\(|[.:]([A-Za-z_]\w*)\s*=\s*function|(?:^|\n)\s*(?:local\s+)?([A-Za-z_]\w*)\s*=\s*function|[{,]\s*([A-Za-z_]\w*)\s*=', t):
        n = next(g for g in m.groups() if g)
        def_idx[n] += 1


def member_uses(name, receivers=None):
    if receivers:
        per = Counter()
        for r in receivers:
            per.update(mem_recv_file.get((name, r), Counter()))
        return per, Counter({r: mem_recv[name][r] for r in receivers})
    return mem_idx.get(name, Counter()), mem_recv.get(name, Counter())


def bare_uses(name):
    return bare_idx.get(name, Counter())

# ---------- exposure paths ----------
glob_by_cls = defaultdict(list)
for g in api['globals']:
    if 'GetStaticProxy' in g['value'] or g['value'].endswith('value)') or g['value'] in ('table', 'dynValue)'):
        glob_by_cls[g['cls']].append(g['name'])

descs = api['descs']
META = {  # LuaMetaDrivable classes -> target component
    'Brix.Lua.MetaDrivers.LuaMetaRectTransform': 'RectTransform', 'Brix.Lua.MetaDrivers.LuaMetaSphereCollider': 'SphereCollider',
    'Brix.Lua.MetaDrivers.LuaMetaCollider': 'Collider', 'Brix.Lua.MetaDrivers.LuaMetaTransform': 'Transform',
    'Brix.Lua.MetaDrivers.LuaMetaLight': 'Light'}

def expose(cls):
    if cls in descs:
        d = descs[cls][0]
        g = [x for x in glob_by_cls.get(cls, []) if not x.startswith('hk_')]
        gname = g[0] if g else ''
        if gname:
            return 'global %s (userdata<%s>)' % (gname, d['udtype']), [gname]
        return 'userdata<%s>' % d['udtype'], []
    if cls == 'Brix.Lua.LuaStaticDriver':
        return 'static driver (<handle>.X)', []
    if cls == 'Brix.Lua.LuaDriver':
        return 'driver (any instance)', []
    if cls in META:
        return 'driver -> %s (LuaMetaDrivable)' % META[cls], []
    if cls == 'Brix.Lua.LuaHookBase':
        return 'hook object', []
    return 'driver -> component', []

# ---------- rows ----------
rows = []
name_count = Counter(m['lua'] for m in api['members'])
recv_global_candidates = Counter()
for m in api['members']:
    path, recvs = expose(m['cls'])
    per, recv = member_uses(m['lua'])
    perq = None
    if recvs:
        perq, _ = member_uses(m['lua'], recvs)
    if m['lua'].startswith('__') and m['lua'] != '__Factory':  # metamethods (__eq, __add...) used implicitly
        pass
    total = sum(per.values())
    ex = per.most_common(1)[0][0] if per else ''
    # observed receivers that look like handles (global, not Lua-assigned, present in assets)
    handles = [r for r, _ in recv.most_common() if r in ASSETSTR and r not in lua_assigned and not r[0].islower() or r.startswith('fy_')]
    handles = [h for h in handles if h in ASSETSTR or h.startswith('fy_')][:4]
    for h in handles:
        recv_global_candidates[h] += 1
    rows.append(dict(layer='LuaDrivable', expose=path, lua=m['lua'], kind=m['kind'], static='static' if m['static'] else 'instance',
                     access=m['access'], params=m['params'], ret=m['ret'], cls=m['cls'], cs=m['csname'], cstype=m['cstype'],
                     src='%s:%d' % (m['file'], m['line']), uses=total, files=len(per), example=ex,
                     qualified=(sum(perq.values()) if perq is not None else ''), homonyms=name_count[m['lua']] - 1, luadef=def_idx.get(m['lua'], 0),
                     receivers=','.join('%s(%d)' % (r, c) for r, c in recv.most_common(3))))

# ---------- registered globals / table functions (non-LuaDrivable) ----------
REG_RE = re.compile(r'static\s+void\s+(\w+)\(Script\s+(\w+)\)')
for dp, dn, fn in os.walk(DECOMP):
    for f in fn:
        if not f.endswith('.cs'):
            continue
        p = os.path.join(dp, f)
        t = open(p, encoding='utf-8', errors='replace').read()
        if 'Globals' not in t:
            continue
        lines = t.split('\n')
        for i, line in enumerate(lines):
            mm = REG_RE.search(line)
            if not mm or mm.group(1) in ('DishookScript', 'RemoveAllAtScript'):
                continue
            # body
            depth = 0; started = False; j = i
            while j < len(lines):
                depth += lines[j].count('{') - lines[j].count('}')
                if '{' in lines[j]:
                    started = True
                if started and depth <= 0:
                    break
                j += 1
            body = '\n'.join(lines[i:j + 1])
            rel = os.path.relpath(p, DECOMP)
            gl = re.findall(r'Globals(?:\.Set\(\s*"([^"]+)"|\[\s*"([^"]+)"\s*\]\s*=)', body)
            gnames = [a or b for a, b in gl]
            keys = [k for k in re.findall(r'(?<!Globals)(?:\.Set\(\s*"([^"]+)"|\w\[\s*"([^"]+)"\s*\]\s*=)', body)]
            keys = [a or b for a, b in keys]
            tabglob = [g for g in gnames if not re.search(r'Globals(?:\.Set\(\s*"%s"\s*,\s*(?:_?desc|DynValue\.From|UserData|new LuaHook|LuaRegistry|_enum|LuaDriver\.desc|[A-Z]\w+;)|\["%s"\]\s*=\s*(?:new|DynValue|LuaRegistry|_enum|LuaDriver|UserData|LuaLibs|[A-Z][A-Za-z]+;))' % (re.escape(g), re.escape(g)), body)]
            owner = tabglob[-1] if tabglob else ''
            for g in dict.fromkeys(gnames):
                if g.startswith('__LUAMARKER') or g in ('hk_testSig', 'hk_disSig'):
                    continue
                per = bare_uses(g) if '.' not in g else Counter()
                kind = 'hook' if g.startswith('hk_') else 'global'
                rows.append(dict(layer='Globals:' + mm.group(1), expose='global', lua=g, kind=kind, static='static', access='',
                                 params='', ret='', cls=rel.replace('/', '.').replace('.cs', ''), cs='', cstype='',
                                 src='%s:%d' % (rel, i + 1), uses=sum(per.values()), files=len(per),
                                 example=per.most_common(1)[0][0] if per else '', qualified='', homonyms='', luadef='', receivers=''))
            if owner:
                for k in dict.fromkeys(keys):
                    if k in gnames:
                        continue
                    per, recv = member_uses(k, [owner])
                    kind = 'hook' if k.startswith('hk_') else 'table member'
                    rows.append(dict(layer='Globals:' + mm.group(1), expose='global %s (table)' % owner, lua=k, kind=kind,
                                     static='static', access='', params='', ret='', cls=rel.replace('/', '.').replace('.cs', ''),
                                     cs='', cstype='', src='%s:%d' % (rel, i + 1), uses=sum(per.values()), files=len(per),
                                     example=per.most_common(1)[0][0] if per else '', qualified='', homonyms='', luadef='', receivers=''))

# dedupe globals rows (same name, same layer)
seen = set(); final = []
for r in rows:
    key = (r['layer'], r['expose'], r['lua'], r['cls'], r['cs'])
    if key in seen:
        continue
    seen.add(key); final.append(r)

cols = ['layer', 'expose', 'cls', 'lua', 'kind', 'static', 'access', 'params', 'ret', 'cs', 'cstype', 'src', 'uses', 'files',
        'example', 'qualified', 'homonyms', 'luadef', 'receivers']
with open(OUT, 'w') as fh:
    fh.write('\t'.join(cols) + '\n')
    for r in sorted(final, key=lambda r: (r['layer'] != 'LuaDrivable', r['cls'], r['kind'], r['lua'])):
        fh.write('\t'.join(str(r[c]).replace('\t', ' ').replace('\n', ' ') for c in cols) + '\n')

# handles observed in Lua (global receivers not defined in Lua and present in assets)
allrecv = Counter()
for t in files.values():
    for m in re.finditer(r'(?<![\w.:])([A-Za-z_]\w*)\s*[.:]\s*[A-Za-z_]', t):
        allrecv[m.group(1)] += 1
handles = {h: c for h, c in allrecv.items() if h not in lua_assigned and (h in ASSETSTR) and h not in
           ('string', 'table', 'math', 'coroutine', 'os', 'io', 'bit32', 'json', 'self', 'dynamic')}
json.dump(dict(nfiles=len(files), handles=sorted(handles.items(), key=lambda x: -x[1])), open(STATS, 'w'), indent=1)
print(len(final), 'rows;', len(files), 'files scanned')
