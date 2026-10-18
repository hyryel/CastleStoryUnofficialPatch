using UnityEngine;

namespace Brix.Legacy.Geometrie;

public class Sommet
{
	public int iSommet;

	public Face[] f;

	public Arete[] a;

	public Vector3 position;

	public Sommet(int i)
	{
		iSommet = i;
		f = new Face[3];
		a = new Arete[3];
	}
}
