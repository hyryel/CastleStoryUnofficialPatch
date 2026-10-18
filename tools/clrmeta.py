#!/usr/bin/env python3
"""Minimal ECMA-335 metadata reader: dumps TypeDefs (with base type) and MethodDefs.
Usage: clrmeta.py <assembly.dll> [--methods]"""
import struct, sys

def u16(b, o): return struct.unpack_from('<H', b, o)[0]
def u32(b, o): return struct.unpack_from('<I', b, o)[0]

class Meta:
    def __init__(self, path):
        b = self.b = open(path, 'rb').read()
        pe = u32(b, 0x3c)
        nsec = u16(b, pe + 6)
        opt = pe + 24
        magic = u16(b, opt)
        dd = opt + (96 if magic == 0x10b else 112)
        cli_rva = u32(b, dd + 14 * 8)
        sec = opt + u16(b, pe + 20)
        self.secs = [(u32(b, sec + i*40 + 12), u32(b, sec + i*40 + 8), u32(b, sec + i*40 + 20)) for i in range(nsec)]
        cli = self.off(cli_rva)
        md = self.off(u32(b, cli + 8))
        vlen = u32(b, md + 12)
        p = md + 16 + vlen + 2
        n = u16(b, p); p += 2
        self.streams = {}
        for _ in range(n):
            so, ss = u32(b, p), u32(b, p + 4); p += 8
            e = b.index(b'\0', p); name = b[p:e].decode(); p = (e + 4) & ~3
            self.streams[name] = (md + so, ss)
        self.parse_tables()

    def off(self, rva):
        for va, vs, raw in self.secs:
            if va <= rva < va + max(vs, 1): return rva - va + raw
        raise ValueError(hex(rva))

    def string(self, i):
        s = self.streams['#Strings'][0] + i
        return self.b[s:self.b.index(b'\0', s)].decode('utf-8', 'replace')

    def parse_tables(self):
        b = self.b; t = self.streams.get('#~', self.streams.get('#-'))[0]
        hs = b[t + 6]
        self.si = 4 if hs & 1 else 2; self.gi = 4 if hs & 2 else 2; self.bi = 4 if hs & 4 else 2
        valid = struct.unpack_from('<Q', b, t + 8)[0]
        p = t + 24
        self.rows = [0] * 64
        for i in range(64):
            if valid >> i & 1: self.rows[i] = u32(b, p); p += 4
        def idx(tbl): return 4 if self.rows[tbl] > 0xffff else 2
        def coded(tbls, bits): return 4 if max(self.rows[x] for x in tbls) >= (1 << (16 - bits)) else 2
        S, G, B = self.si, self.gi, self.bi
        TDOR = coded([2, 1, 27], 2); RS = coded([0, 26, 35, 1], 2)
        HC = coded([4, 8, 23], 2); HFM = coded([4, 8], 1); HDS = coded([2, 6, 32], 2)
        MRP = coded([2, 1, 26, 6, 27], 3); HS = coded([20, 23], 1); MDOR = coded([6, 10], 1)
        MF = coded([4, 6], 1); IMPL = coded([38, 35, 39], 2); TOMD = coded([2, 6], 1)
        HCA = coded([6, 4, 1, 2, 8, 9, 10, 0, 14, 23, 20, 17, 26, 27, 32, 35, 38, 39, 40], 5)
        CAT = coded([6, 10], 3)
        size = {0: 2+S+G*3, 1: RS+S+S, 2: 4+S+S+TDOR+idx(4)+idx(6), 3: idx(4), 4: 2+S+B,
                5: idx(6), 6: 4+2+2+S+B+idx(8), 7: idx(8), 8: 2+2+S, 9: idx(2)+TDOR,
                10: MRP+S+B, 11: 2+HC+B, 12: HCA+CAT+B, 13: HFM+B, 14: 2+HDS+B, 15: 2+4+idx(2),
                16: 4+idx(4), 17: B, 18: idx(2)+idx(20), 19: idx(20), 20: 2+S+TDOR, 21: idx(2)+idx(23),
                22: idx(23), 23: 2+S+B, 24: 2+idx(6)+HS, 25: idx(2)+MDOR+MDOR, 26: S, 27: B,
                28: 2+MF+S+idx(26), 29: 4+idx(4), 30: 4+4, 31: 4, 32: 4+2+2+2+2+4+B+S+S, 33: 4, 34: 12,
                35: 2+2+2+2+4+B+S+S+B, 36: 4+idx(35), 37: 12+idx(35), 38: 4+S+B, 39: 4+4+S+S+IMPL,
                40: 4+4+S+IMPL, 41: idx(2)+idx(2), 42: 2+2+TOMD+S, 43: MDOR+B, 44: idx(42)+TDOR}
        self.HCA = HCA; self.CAT = CAT; self.MRP = MRP
        self.sizes = size; self.tbl = {}
        for i in range(64):
            if self.rows[i]: self.tbl[i] = p; p += self.rows[i] * size[i]
        self.TDOR = TDOR; self.RS = RS

    def rd(self, o, n): return u16(self.b, o) if n == 2 else u32(self.b, o)

    def typerefs(self):
        out = []
        for r in range(self.rows[1]):
            o = self.tbl[1] + r * self.sizes[1] + self.RS
            out.append((self.string(self.rd(o + self.si, self.si)), self.string(self.rd(o, self.si))))
        return out

    def typedefs(self):
        S = self.si; out = []
        mi = 4 if self.rows[6] > 0xffff else 2; fi = 4 if self.rows[4] > 0xffff else 2
        for r in range(self.rows[2]):
            o = self.tbl[2] + r * self.sizes[2]
            flags = u32(self.b, o)
            name = self.string(self.rd(o + 4, S)); ns = self.string(self.rd(o + 4 + S, S))
            ext = self.rd(o + 4 + 2*S, self.TDOR)
            ml = self.rd(o + 4 + 2*S + self.TDOR + fi, mi)
            out.append(dict(flags=flags, name=name, ns=ns, ext=ext, mlist=ml))
        return out

    def nested(self):
        n = {}; ti = 4 if self.rows[2] > 0xffff else 2
        for r in range(self.rows[41]):
            o = self.tbl[41] + r * self.sizes[41]
            n[self.rd(o, ti)] = self.rd(o + ti, ti)
        return n

    def methods(self):
        out = []
        for r in range(self.rows[6]):
            o = self.tbl[6] + r * self.sizes[6]
            out.append(self.string(self.rd(o + 8, self.si)))
        return out

    def method_owner(self, tds):
        """MethodDef row (1-based) -> TypeDef row (1-based)."""
        own = [0] * (self.rows[6] + 1)
        for i, t in enumerate(tds, 1):
            end = tds[i]['mlist'] if i < len(tds) else self.rows[6] + 1
            for r in range(t['mlist'], end): own[r] = i
        return own

    def member_names(self, full, tds):
        """(HasCustomAttribute tag, row) -> 'Type::member' for methods (0), fields (1), properties (9)."""
        names = {}; mown = self.method_owner(tds); meths = self.methods()
        for r in range(1, self.rows[6] + 1): names[(0, r)] = f"{full(mown[r])}::{meths[r - 1]}()"
        fi = 4 if self.rows[4] > 0xffff else 2; mi = 4 if self.rows[6] > 0xffff else 2
        flist = [self.rd(self.tbl[2] + r * self.sizes[2] + 4 + 2 * self.si + self.TDOR, fi) for r in range(self.rows[2])]
        for i in range(len(flist)):
            end = flist[i + 1] if i + 1 < len(flist) else self.rows[4] + 1
            for r in range(flist[i], end):
                o = self.tbl[4] + (r - 1) * self.sizes[4]
                names[(1, r)] = f"{full(i + 1)}::{self.string(self.rd(o + 2, self.si))}"
        ti = 4 if self.rows[2] > 0xffff else 2; pi = 4 if self.rows[23] > 0xffff else 2
        pmap = [(self.rd(self.tbl[21] + r * self.sizes[21], ti), self.rd(self.tbl[21] + r * self.sizes[21] + ti, pi)) for r in range(self.rows[21])]
        for k, (parent, first) in enumerate(pmap):
            end = pmap[k + 1][1] if k + 1 < len(pmap) else self.rows[23] + 1
            for r in range(first, end):
                o = self.tbl[23] + (r - 1) * self.sizes[23]
                names[(9, r)] = f"{full(parent)}::{self.string(self.rd(o + 2, self.si))}"
        return names

    def all_attributes(self, full, refs, tds):
        """Yields (target name, attribute type name) for methods, fields, properties and types."""
        names = self.member_names(full, tds); own = self.method_owner(tds)
        def mrp_class(r):
            o = self.tbl[10] + (r - 1) * self.sizes[10]
            c = self.rd(o, self.MRP); tag, ix = c & 7, c >> 3
            if tag == 0: return full(ix)
            if tag == 1: ns, n = refs[ix - 1]; return (ns + '.' if ns else '') + n
            return '?'
        for r in range(self.rows[12]):
            o = self.tbl[12] + r * self.sizes[12]
            p = self.rd(o, self.HCA); c = self.rd(o + self.HCA, self.CAT)
            tag, ix = c & 7, c >> 3
            attr = mrp_class(ix) if tag == 3 else full(own[ix]) if tag == 2 else '?'
            pt, pr = p & 31, p >> 5
            if pt == 3: yield full(pr), attr
            elif (pt, pr) in names: yield names[(pt, pr)], attr

    def type_attributes(self, full, refs, tds):
        """Yields (typedef row, attribute type name) for attributes applied to TypeDefs."""
        own = self.method_owner(tds)
        def mrp_class(r):
            o = self.tbl[10] + (r - 1) * self.sizes[10]
            c = self.rd(o, self.MRP); tag, ix = c & 7, c >> 3
            if tag == 0: return full(ix)
            if tag == 1: ns, n = refs[ix - 1]; return (ns + '.' if ns else '') + n
            return '?'
        for r in range(self.rows[12]):
            o = self.tbl[12] + r * self.sizes[12]
            p = self.rd(o, self.HCA); c = self.rd(o + self.HCA, self.CAT)
            if p & 31 != 3: continue
            tag, ix = c & 7, c >> 3
            name = mrp_class(ix) if tag == 3 else full(own[ix]) if tag == 2 else '?'
            yield p >> 5, name

def main():
    m = Meta(sys.argv[1]); refs = m.typerefs(); tds = m.typedefs(); nest = m.nested()
    meths = m.methods() if '--methods' in sys.argv else None
    def full(i):
        t = tds[i - 1]; n = t['name']
        if i in nest: return full(nest[i]) + '/' + n
        return (t['ns'] + '.' if t['ns'] else '') + n
    def base(e):
        tag, ix = e & 3, e >> 2
        if ix == 0: return ''
        if tag == 0: return full(ix)
        if tag == 1: ns, n = refs[ix - 1]; return (ns + '.' if ns else '') + n
        return '<spec>'
    if '--attrs' in sys.argv:
        for row, a in m.type_attributes(full, refs, tds): print(f"{full(row)}\t{a}")
        return
    if '--all-attrs' in sys.argv:
        for target, a in m.all_attributes(full, refs, tds): print(f"{target}\t{a}")
        return
    for i, t in enumerate(tds, 1):
        kind = 'interface' if t['flags'] & 0x20 else 'class'
        line = f"{full(i)}\t{kind}\t{base(t['ext'])}"
        if meths is not None:
            end = tds[i]['mlist'] if i < len(tds) else len(meths) + 1
            line += '\t' + ','.join(meths[t['mlist'] - 1:end - 1])
        print(line)

if __name__ == '__main__':
    main()
