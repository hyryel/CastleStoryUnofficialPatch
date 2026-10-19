# Portage fidèle de BuildOrder.Grid (Stand / Flood / TouchesReach / coupure / profondeur) sur un monde synthétique.
from collections import deque
M,MD,MU=6,3,4
def run(world_full, goals, people=()):
    # world_full: set de cases pleines ; goals: dict nom -> (cells, access)
    xs=[p[0] for g in goals.values() for p in g[0]+g[1]]; ys=[p[1] for g in goals.values() for p in g[0]+g[1]]; zs=[p[2] for g in goals.values() for p in g[0]+g[1]]
    x0,y0,z0=min(xs)-M,min(ys)-MD,min(zs)-M; nx,ny,nz=max(xs)-min(xs)+1+2*M,max(ys)-min(ys)+1+MD+MU,max(zs)-min(zs)+1+2*M
    inb=lambda p:0<=p[0]-x0<nx and 0<=p[1]-y0<ny and 0<=p[2]-z0<nz
    def stand(p,ov):
        if p in ov: return False
        up=(p[0],p[1]+1,p[2]); dn=(p[0],p[1]-1,p[2])
        if up in ov or up in world_full: return False
        if p in world_full: return False  # pas de pente dans ce monde
        return dn in ov or dn in world_full
    def flood(ov):
        d={}; q=deque()
        for x in range(nx):
            for z in range(nz):
                if x not in (0,nx-1) and z not in (0,nz-1): continue
                for y in range(ny):
                    p=(x0+x,y0+y,z0+z)
                    if stand(p,ov): d[p]=0; q.append(p)
        while q:
            p=q.popleft()
            for dx,dz in ((1,0),(-1,0),(0,1),(0,-1)):
                n=(p[0]+dx,p[1],p[2]+dz)
                cand=[n] if stand(n,ov) else [c for c in ((n[0],n[1]-1,n[2]),(n[0],n[1]+1,n[2])) if stand(c,ov)]
                for c in cand:
                    if inb(c) and c not in d: d[c]=d[p]+1; q.append(c)
        return d
    r0=flood(set())
    reach={k:any(a in r0 for a in g[1]) for k,g in goals.items()}
    cut={}
    for k,(cells,acc_) in goals.items():
        if not reach[k]: continue
        touches=any(c in r0 or (c[0],c[1]-1,c[2]) in r0 for c in cells)
        if not touches: continue
        rx=flood(set(cells))
        v=[o for o,(oc,oa) in goals.items() if o!=k and reach[o] and not any(a in rx for a in oa)]
        if not any(a in rx for a in acc_): v.append('poseur')
        cov=set(cells)|{(c[0],c[1]-1,c[2]) for c in cells}
        v+=['Brictron@'+str(p) for p in people if p in r0 and p not in rx and p not in cov]
        if v: cut[k]=v
    allc=set(c for g in goals.values() for c in g[0]); dep=flood(allc)
    depth={k:min([dep[a] for a in g[1] if a in dep],default=-1) for k,g in goals.items()}
    return reach,cut,depth
ground={(x,-1,z) for x in range(-30,31) for z in range(-30,31)}
acc=lambda c:[(c[0]+dx,c[1],c[2]+dz) for dx,dz in ((1,0),(-1,0),(0,1),(0,-1))]
# 1) Couloir de largeur 1 entre deux murs hauts ; A au milieu du couloir, B au fond (cul-de-sac)
walls=set()
for x in range(-3,6):
    for y in range(0,3): walls.add((x,y,1)); walls.add((x,y,-1))
for y in range(0,3): walls.add((6,y,0))  # fond fermé
A=(0,0,0); B=(5,1,0)  # B posé au fond, en hauteur 1 sur un bloc existant
w=ground|walls|{(5,0,0)}
r,c,d=run(w,{'A':([A],acc(A)),'B':([B],acc(B))})
print('couloir   reach',r,'coupe',c,'profondeur',d)
# 2) Mur ouvert de 3 blocs au sol : aucune coupure attendue
G={f'W{i}':([(i,0,5)],acc((i,0,5))) for i in range(3)}
r,c,d=run(ground,G); print('mur       coupe',c)
# 3) Pièce : anneau de 8 blocs autour de (0,0,0) + 1 Blueprint intérieur I. Fermer l'anneau coupe I.
ring=[(x,0,z) for x in (-1,0,1) for z in (-1,0,1) if (x,z)!=(0,0)]
ringw={(x,y,z) for (x,_,z) in ring for y in (1,2)}  # anneau déjà construit en hauteur 1..2 sauf la porte
door=(1,0,0); built={p for p in ring if p!=door}|{(x,y,z) for (x,y,z) in ringw if (x,z)!=(1,0)}
G={'I':([(0,1,0)],acc((0,1,0))+[(0,0,0)]),'Porte':([door,(1,1,0),(1,2,0)],acc(door))}
r,c,d=run(ground|built,G); print('pièce     reach',r,'coupe',c,'profondeur',d)
# 4) Blueprint en l'air (y=3) sans support : inaccessible (informatif)
r,c,d=run(ground,{'H':([(0,3,0)],acc((0,3,0)))}); print('hauteur   reach',r)
# 1bis) Couloir : A au milieu, B au fond posé au sol (accès (4,0,0)) → A coupe B, B ne coupe pas A ; B plus profond
w=ground|walls
r,c,d=run(w,{'A':([A],acc(A)),'B':([(5,0,0)],[(4,0,0)])})
print('couloir2  reach',r,'coupe',c,'profondeur',d)
# 5) Blocage mutuel : couloir de 1, deux Blueprints qui se coupent l'un l'autre impossible ici -> vérifie que
#    deux blocs côte à côte dans le couloir : le plus proche de l'entrée coupe l'autre seulement
r,c,d=run(w,{'P':([(-1,0,0)],acc((-1,0,0))),'Q':([(2,0,0)],acc((2,0,0)))})
print('couloir3  coupe',c,'profondeur',d)
# 6) Couloir : A = bloc double hauteur (0,0..1,0) → coupe B au fond ; B ne coupe pas A
r,c,d=run(w,{'A':([(0,0,0),(0,1,0)],acc((0,0,0))),'B':([(5,0,0)],[(4,0,0)])})
print('couloir4  coupe',c,'profondeur',d)
# 7) Même chose avec un premier bloc déjà posé à (0,0,0) et A = (0,1,0) au-dessus
r,c,d=run(w|{(0,0,0)},{'A':([(0,1,0)],[(-1,0,0),(1,0,0)]),'B':([(5,0,0)],[(4,0,0)])})
print('couloir5  coupe',c)

# 8) Étage sans escalier : plateforme 3x3 à y=2 (dessus y=3) atteinte par une marche (bloc (2,0,0) puis (2,1,0) ... ).
#    Marches : (3,0,0) hauteur 1 → (2,1,0)... ; Brictron à l'étage (1,3,1). X = Blueprint qui recouvre la marche haute.
plat={(x,y,z) for x in (-1,0,1) for z in (-1,0,1) for y in (0,1,2)}
steps={(2,0,0),(2,1,0),(3,0,0)}   # escalier de fortune : sol→(3,1,0)→(2,2,0)→plateforme (1,3,0)
w8=ground|plat|steps
X8=[(3,1,0),(3,2,0)]              # pose qui coupe la marche
r,c,d=run(w8,{'X':(X8,[(4,0,0),(3,0,1)]),'Y':([(5,0,5)],[(5,0,4)])},people=[(0,3,0)])
print('étage     reach',r,'coupe',c)
