using UnityEngine;

namespace Brix.Legacy.Geometrie;

public class Arete
{
	public int iArete;

	public Face[] f;

	public Sommet[] s;

	public Vector3 vecteur;

	public Arete(int i)
	{
		iArete = i;
		f = new Face[2];
		s = new Sommet[2];
	}
}
