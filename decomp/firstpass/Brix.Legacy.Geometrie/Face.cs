using UnityEngine;

namespace Brix.Legacy.Geometrie;

public class Face
{
	public int iFace;

	public Sommet[] s;

	public Arete[] a;

	public Vector3 normale;

	public Face(int i)
	{
		iFace = i;
		s = new Sommet[4];
		a = new Arete[4];
	}
}
