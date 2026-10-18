using Brix.Core;
using UnityEngine;

namespace Brix.Engine.Geometrie;

public class CubeLazy : ICube, IGeometrie
{
	private ICube iCube;

	private Vector3?[] SommetPositions = new Vector3?[14];

	private Vector3?[] Normales = new Vector3?[30];

	private Vector3?[] Tangentes = new Vector3?[30];

	public CubeLazy(ICube iCube)
	{
		this.iCube = iCube;
	}

	public Vector3 GetSommet(int i)
	{
		if (!SommetPositions[i].HasValue)
		{
			SommetPositions[i] = iCube.GetSommet(i);
		}
		return SommetPositions[i].Value;
	}

	public Vector3 GetNormale(int i)
	{
		if (!Normales[i].HasValue)
		{
			Normales[i] = iCube.GetNormale(i);
		}
		return Normales[i].Value;
	}

	public Vector3 GetTangente(int i)
	{
		if (!Tangentes[i].HasValue)
		{
			Tangentes[i] = iCube.GetTangente(i);
		}
		return Tangentes[i].Value;
	}
}
