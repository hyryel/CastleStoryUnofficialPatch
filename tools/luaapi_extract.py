#!/usr/bin/env python3
"""Extract [LuaDrivable] members + Lua globals from decompiled firstpass sources.
Usage: luaapi_extract.py <decomp_dir> <out_json>
"""
import json, os, re, sys

ROOT = sys.argv[1]
OUT = sys.argv[2]

ATTR_RE = re.compile(r'^\s*\[LuaDrivable\("([^"]*)"(?:\s*,\s*UseExtern\s*=\s*(true|false))?\)\]')
OTHER_ATTR_RE = re.compile(r'^\s*\[[^\]]*\]\s*$')
TYPE_DECL_RE = re.compile(r'\b(class|struct|interface|enum)\s+([A-Za-z_][\w]*)(<[^>{]*>)?')
NS_RE = re.compile(r'^namespace\s+([\w.]+)\s*;')
ARG_RE = re.compile(r'args\.(AsUserData|AsComponent|AsFloat|AsString|AsType|AsGameObject|AsBool|AsInt|AsEnum|AsTable|AsLong|RawGet)\s*(<([^>(]*(?:<[^>]*>)?)>)?\(\s*(\d+)\s*(?:,[^;]*?DataType\.(\w+))?')
ARGIDX_RE = re.compile(r'args\[(\d+)\]')
RET_RE = re.compile(r'return\s+(DynValue\.(\w+)|UserData\.CreateWithDescriptor\(([^;]*)|LuaLibs\.CreateLuaDescribable|[^;]+);')
GLOB_RE = re.compile(r'(?:script|s|sc)\.Globals(?:\.Set\(\s*"([^"]+)"|\[\s*"([^"]+)"\s*\]\s*=)\s*,?\s*([^;]*);')
DESC_RE = re.compile(r'new\s+LuaObjectDescriptor<([\w.<>]+)>\(\s*(?:typeof\(([\w.]+)\))?\s*\)')
TABLESET_RE = re.compile(r'\.Table\.Set\(\s*"([^"]+)"')

ARGMAP = {'AsFloat': 'number', 'AsInt': 'int', 'AsLong': 'int', 'AsString': 'string', 'AsBool': 'bool',
          'AsGameObject': 'driver', 'AsTable': 'table', 'RawGet': 'any'}


def strip_generic(t):
    return t


def find_block_end(lines, i):
    """From line i (containing or preceding '{'), return index of line closing the block."""
    depth = 0
    started = False
    for j in range(i, len(lines)):
        s = re.sub(r'"(?:\\.|[^"\\])*"', '""', lines[j])
        s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
        for ch in s:
            if ch == '{':
                depth += 1; started = True
            elif ch == '}':
                depth -= 1
        if started and depth <= 0:
            return j
        if not started and (s.rstrip().endswith(';')):
            return j
    return len(lines) - 1


def parse_args(body):
    params = {}
    for m in ARG_RE.finditer(body):
        kind, _, gen, idx, dt = m.groups()
        idx = int(idx)
        if kind in ('AsUserData', 'AsComponent', 'AsEnum'):
            t = (gen or '?').split('.')[-1]
            if kind == 'AsEnum':
                t = 'enum ' + t
            elif kind == 'AsComponent':
                t = 'driver<' + t + '>'
        elif kind == 'AsType':
            t = (dt or 'any').lower()
        else:
            t = ARGMAP.get(kind, kind)
        opt = 'allowNil: true' in body[m.start():m.start() + 200].split(';')[0]
        if idx not in params:
            params[idx] = t + ('?' if opt else '')
    for m in ARGIDX_RE.finditer(body):
        idx = int(m.group(1))
        params.setdefault(idx, 'any')
    if not params:
        return '', 0
    mx = max(params)
    ps = ['%d:%s' % (i, params.get(i, '?')) for i in range(mx + 1)]
    return ', '.join(ps), mx + 1


def parse_ret(body):
    kinds = []
    for m in RET_RE.finditer(body):
        full = m.group(1)
        dv = m.group(2)
        if dv:
            k = {'Void': 'nil', 'Nil': 'nil', 'NewNil': 'nil', 'Invalid': 'nil', 'NewNumber': 'number', 'NewBoolean': 'bool',
                 'True': 'bool', 'False': 'bool', 'NewString': 'string', 'NewTable': 'table', 'NewTuple': 'tuple',
                 'FromObject': 'object', 'NewCallback': 'function', 'NewClosure': 'function', 'NewUserData': 'userdata'}.get(dv, dv)
            if dv == 'FromObject':
                mm = re.search(r'FromObject\([^,]+,\s*(.*)\)\s*$', full)
                if mm:
                    arg = mm.group(1)
                    if re.search(r'\.ToList\(\)|List<|\[\]|ToArray', arg):
                        k = 'list'
        elif m.group(3) is not None:
            k = 'userdata'
        elif 'CreateLuaDescribable' in full:
            k = 'driver'
        else:
            k = 'value'
        if k not in kinds:
            kinds.append(k)
    if 'nil' in kinds and len(kinds) > 1:
        kinds.remove('nil'); kinds.append('nil')
    return '|'.join(kinds) if kinds else '?'


def main():
    members = []
    globals_ = []
    descs = {}  # declaring class -> (userdata type, global name)
    for dp, dn, fn in os.walk(ROOT):
        for f in fn:
            if not f.endswith('.cs'):
                continue
            path = os.path.join(dp, f)
            rel = os.path.relpath(path, ROOT)
            with open(path, encoding='utf-8', errors='replace') as fh:
                text = fh.read()
            if 'LuaDrivable' not in text and 'Globals' not in text:
                continue
            lines = text.split('\n')
            ns = ''
            stack = []  # (name, depth_at_open)
            depth = 0
            pending_type = None
            i = 0
            while i < len(lines):
                line = lines[i]
                m = NS_RE.match(line)
                if m:
                    ns = m.group(1)
                if not line.lstrip().startswith('//'):
                    tm = TYPE_DECL_RE.search(line)
                    if tm and not line.strip().startswith('[') and '(' not in line.split(tm.group(0))[0] and 'new ' not in line and 'typeof' not in line:
                        pending_type = tm.group(2)
                s = re.sub(r'"(?:\\.|[^"\\])*"', '""', line)
                s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
                am = ATTR_RE.match(line)
                if am:
                    luaname, ext = am.group(1), am.group(2) == 'true'
                    j = i + 1
                    while j < len(lines) and OTHER_ATTR_RE.match(lines[j]):
                        j += 1
                    decl = lines[j].strip()
                    cls = '.'.join(x[0] for x in stack)
                    full = (ns + '.' if ns else '') + cls.replace('.', '/', ) if False else (ns + '.' if ns else '') + '/'.join(x[0] for x in stack)
                    is_static = bool(re.search(r'\bstatic\b', decl))
                    if '(ScriptExecutionContext' in decl or ('(' in decl and 'CallbackArguments' in decl):
                        kind = 'method'
                        nm = re.search(r'(\w+)\s*\(ScriptExecutionContext', decl)
                        csname = nm.group(1) if nm else '?'
                        end = find_block_end(lines, j)
                        body = '\n'.join(lines[j:end + 1])
                        params, nparams = parse_args(body)
                        ret = parse_ret(body)
                        cstype = 'DynValue(ctx,args)'
                        access = 'call'
                    else:
                        dm = re.match(r'((?:(?:public|private|protected|internal|static|readonly|new|override|virtual|const|abstract|extern|unsafe|sealed|volatile)\s+)*)([\w.<>\[\], ?]+?)\s+(\w+)\s*(=>|=|;|\{|$)', decl)
                        if dm:
                            cstype = dm.group(2).strip(); csname = dm.group(3); tail = dm.group(4)
                        else:
                            cstype = '?'; csname = '?'; tail = ''
                        if tail in ('=>',):
                            kind = 'property'; access = 'get'
                        elif tail == '{' or (tail == '' and j + 1 < len(lines) and lines[j + 1].strip() == '{'):
                            kind = 'property'
                            end = find_block_end(lines, j)
                            body = '\n'.join(lines[j:end + 1])
                            g = re.search(r'\bget\b', body) is not None
                            st = re.search(r'\bset\b', body) is not None
                            ps = re.search(r'private\s+set|protected\s+set', body) is not None
                            access = ('get' if g else '') + ('/set' if st else '')
                            access = access.strip('/')
                        else:
                            kind = 'field'
                            access = 'get' if re.search(r'\b(readonly|const)\b', decl) else 'get/set'
                        if cstype.startswith('LuaHook'):
                            kind = 'hook'; access = 'hook'
                        params, ret = '', cstype
                        nparams = 0
                    members.append(dict(lua=luaname, kind=kind, static=is_static, cstype=cstype, csname=csname,
                                        cls=full, file=rel, line=j + 1, params=params, nparams=nparams,
                                        ret=ret, access=access, extern=ext))
                for gm in GLOB_RE.finditer(line):
                    name = gm.group(1) or gm.group(2)
                    val = gm.group(3).strip()
                    full = (ns + '.' if ns else '') + '/'.join(x[0] for x in stack)
                    globals_.append(dict(name=name, value=val[:120], cls=full, file=rel, line=i + 1))
                for dm in DESC_RE.finditer(line):
                    full = (ns + '.' if ns else '') + '/'.join(x[0] for x in stack)
                    descs.setdefault(full, []).append(dict(udtype=dm.group(1), meta=dm.group(2)))
                # brace tracking
                for ch in s:
                    if ch == '{':
                        depth += 1
                        if pending_type:
                            stack.append((pending_type, depth)); pending_type = None
                    elif ch == '}':
                        if stack and stack[-1][1] == depth:
                            stack.pop()
                        depth -= 1
                i += 1
    json.dump(dict(members=members, globals=globals_, descs=descs), open(OUT, 'w'), indent=1)
    print(len(members), 'members;', len(globals_), 'globals;', len(descs), 'desc classes')


main()
