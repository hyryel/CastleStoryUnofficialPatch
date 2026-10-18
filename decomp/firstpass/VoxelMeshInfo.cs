using Brix.Utils.Logging;
using UnityEngine;

public struct VoxelMeshInfo
{
	private static BrixLogger logger = new BrixLogger(typeof(VoxelMeshInfo).FullName);

	private int xyz;

	private byte typeVariante;

	private byte faceConfig;

	private byte niveauxDessus;

	public int type => (typeVariante >> 4) & 0xF;

	public int variante => typeVariante & 0xF;

	public int config => faceConfig;

	public int mode => ((niveauxDessus & 0xFF) != 255) ? 1 : 0;

	public int niveaux => DecompacterNiveaux(niveauxDessus & 0xFF);

	public int x => xyz & 0x2FF;

	public int y => (xyz >> 10) & 0x2FF;

	public int z => xyz >> 20;

	public VoxelMeshInfo(int petitX, int petitY, int petitZ, int type, int variante, int faceConfig, int sommetConfig = -1, int niveauxDessus = -1, int niveauxDessous = -1, int shadowmap = -1)
	{
		xyz = (petitX & 0x2FF) | ((petitY & 0x2FF) << 10) | ((petitZ & 0x2FF) << 20);
		if (type > 15 || type < 0)
		{
			logger.Info("VoxelMeshInfo type overflow");
		}
		if (variante > 15 || variante < 0)
		{
			logger.Info("VoxelMeshInfo variante overflow");
		}
		typeVariante = (byte)(((type & 0xF) << 4) | (variante & 0xF));
		this.faceConfig = (byte)(faceConfig & 0x3F);
		this.niveauxDessus = (byte)CompacterNiveaux(niveauxDessus);
	}

	private static int CompacterNiveaux(int niv)
	{
		return (niv & 3) | ((niv & 0x30) >> 2) | ((niv & 0x300) >> 2) | ((niv & 0x3000) >> 8);
	}

	private static int DecompacterNiveaux(int niv)
	{
		return (niv & 3) | ((niv << 2) & 0x30) | ((niv << 2) & 0x300) | ((niv << 8) & 0x3000);
	}

	public static int GetSommetConfigFromFaceConfig(int faceConfig)
	{
		return (((faceConfig & 0x13) != 0) ? 1 : 0) | (((faceConfig & 0xB) != 0) ? 2 : 0) | (((faceConfig & 0x15) != 0) ? 4 : 0) | (((faceConfig & 0xD) != 0) ? 8 : 0) | (((faceConfig & 0x32) != 0) ? 16 : 0) | (((faceConfig & 0x2A) != 0) ? 32 : 0) | (((faceConfig & 0x34) != 0) ? 64 : 0) | (((faceConfig & 0x2C) != 0) ? 128 : 0);
	}

	public void GetCentre(out Vector3 p)
	{
		p.x = x;
		p.y = y;
		p.z = z;
	}

	public float[] GetHauteurs(float[] hauteurs)
	{
		hauteurs[0] = (float)((niveauxDessus >> 0) & 3) * (1f / 6f);
		hauteurs[1] = (float)((niveauxDessus >> 2) & 3) * (1f / 6f);
		hauteurs[2] = (float)((niveauxDessus >> 4) & 3) * (1f / 6f);
		hauteurs[3] = (float)((niveauxDessus >> 6) & 3) * (1f / 6f);
		hauteurs[4] = -0.5f;
		hauteurs[5] = -0.5f;
		hauteurs[6] = -0.5f;
		hauteurs[7] = -0.5f;
		return hauteurs;
	}

	public void GetUv(out Vector2 uv, int lod = 0)
	{
		if (lod == 0)
		{
			uv.x = typeVariante & 0xF;
			uv.y = typeVariante >> 4;
		}
		else
		{
			uv.x = (typeVariante & 3) | ((typeVariante >> 2) & 0xC);
			uv.y = ((typeVariante >> 2) & 3) | ((typeVariante >> 4) & 0xC);
		}
	}
}
