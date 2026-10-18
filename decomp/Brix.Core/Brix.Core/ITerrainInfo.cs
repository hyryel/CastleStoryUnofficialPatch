namespace Brix.Core;

public interface ITerrainInfo
{
	void Cube(int indice, int config, int niveaux, int paire, int mode, int type, int variante);

	void Cube(int indice, int config, int niveaux, int paire, int mode, int type, int variante, bool foncer, uint fonce);

	void Clear();

	MeshModifiable ConstruireLOD0(MeshModifiable meshDst);

	MeshModifiable ConstruireLOD2(MeshModifiable meshDst);

	MeshModifiable ConstruireCollider(MeshModifiable meshInfo);
}
