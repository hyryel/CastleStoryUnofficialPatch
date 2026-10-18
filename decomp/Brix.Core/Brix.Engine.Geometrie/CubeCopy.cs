using Brix.Core;
using UnityEngine;

namespace Brix.Engine.Geometrie;

public class CubeCopy : ICube, IGeometrie
{
	public Vector3[] SommetPositions = new Vector3[14];

	public Vector3[] Normales = new Vector3[30];

	public Vector3[] Tangentes = new Vector3[30];

	public CubeCopy(ICube iCube)
	{
		for (int i = 0; i < 14; i++)
		{
			SommetPositions[i] = iCube.GetSommet(i);
		}
		for (int j = 0; j < 30; j++)
		{
			Normales[j] = iCube.GetNormale(j);
		}
		for (int k = 0; k < 30; k++)
		{
			Tangentes[k] = iCube.GetTangente(k);
		}
	}

	public Vector3 GetSommet(int i)
	{
		return SommetPositions[i];
	}

	public Vector3 GetNormale(int i)
	{
		return Normales[i];
	}

	public Vector3 GetTangente(int i)
	{
		return Tangentes[i];
	}
}
