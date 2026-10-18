using Brix.Core;
using UnityEngine;

namespace Brix.Engine.Geometrie;

public class Geometrie : IGeometrie
{
	public Vector3[] sommets;

	public Vector3[] normales;

	public Vector3[] tangentes;

	public static int NiveauFromVecteur(Vector3 s)
	{
		float f = (s.y + 0.5f) * 3f;
		return Mathf.RoundToInt(f);
	}

	public static float HauteurFromNiveau(int niveau)
	{
		return Mathf.Lerp(-0.5f, 0.5f, (float)niveau / 3f);
	}

	public static int GetNiveaux(IGeometrie geometrie)
	{
		int num = 0;
		num |= NiveauFromVecteur(geometrie.GetSommet(0));
		num |= NiveauFromVecteur(geometrie.GetSommet(1)) << 4;
		num |= NiveauFromVecteur(geometrie.GetSommet(3)) << 8;
		return num | (NiveauFromVecteur(geometrie.GetSommet(2)) << 12);
	}

	public static IGeometrie FromNiveaux(int niveaux)
	{
		Geometrie geometrie = Cube.Modele.Copy(8, 6);
		geometrie.sommets[0].y = HauteurFromNiveau(niveaux & 3);
		geometrie.sommets[1].y = HauteurFromNiveau((niveaux >> 4) & 3);
		geometrie.sommets[3].y = HauteurFromNiveau((niveaux >> 8) & 3);
		geometrie.sommets[2].y = HauteurFromNiveau((niveaux >> 12) & 3);
		return geometrie;
	}

	public Vector3 GetSommet(int i)
	{
		return sommets[i];
	}

	public Vector3 GetNormale(int i)
	{
		return normales[i];
	}

	public Vector3 GetTangente(int i)
	{
		return tangentes[i];
	}
}
