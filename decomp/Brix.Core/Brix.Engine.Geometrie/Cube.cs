using Brix.Core;
using UnityEngine;

namespace Brix.Engine.Geometrie;

public class Cube : ICube, IGeometrie
{
	private class CubeVide : ICube, IGeometrie
	{
		public Vector3 GetSommet(int i)
		{
			return Vector3.zero;
		}

		public Vector3 GetNormale(int i)
		{
			return Vector3.zero;
		}

		public Vector3 GetTangente(int i)
		{
			return Vector3.zero;
		}
	}

	public static Cube Modele = new Cube();

	public static ICube Vide = new CubeVide();

	private CubeCopy copie;

	public virtual CubeCopy Copie
	{
		get
		{
			if (copie == null)
			{
				copie = new CubeCopy(this);
			}
			return copie;
		}
	}

	public virtual Vector3 GetSommet(int i)
	{
		return CubeModele.GetSommet(i);
	}

	public virtual Vector3 GetNormale(int i)
	{
		return CubeModele.GetNormale(i);
	}

	public virtual Vector3 GetTangente(int i)
	{
		return CubeModele.GetTangente(i);
	}

	public virtual Vector3[] GetSommets()
	{
		return Copie.SommetPositions;
	}

	public virtual Vector3[] GetNormales()
	{
		return Copie.Normales;
	}

	public virtual Vector3[] GetTangentes()
	{
		return Copie.Tangentes;
	}
}
