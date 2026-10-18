using Brix.Core;
using UnityEngine;

namespace Brix.Engine.Geometrie;

public class CubeNiveaux : IGeometrie
{
	public const float HauteurMin = -0.5f;

	public const float HauteurMax = 0.5f;

	public const int NiveauMax = 3;

	public int niveaux;

	public CubeNiveaux(int niveaux)
	{
		this.niveaux = niveaux;
	}

	public Vector3 GetSommet(int i)
	{
		Vector3 sommet = Cube.Modele.GetSommet(i);
		int num = (niveaux >> 4 * i) & 3;
		sommet.y = Mathf.Lerp(-0.5f, 0.5f, (float)num / 3f);
		return sommet;
	}

	public Vector3 GetNormale(int i)
	{
		return Cube.Modele.GetNormale(i);
	}

	public Vector3 GetTangente(int i)
	{
		return Cube.Modele.GetTangente(i);
	}
}
