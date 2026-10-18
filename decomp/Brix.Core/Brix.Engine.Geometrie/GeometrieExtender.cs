using Brix.Core;
using UnityEngine;

namespace Brix.Engine.Geometrie;

public static class GeometrieExtender
{
	public static Geometrie Copy(this IGeometrie geometrie, int nSommets, int nNormales)
	{
		Vector3[] array = new Vector3[nSommets];
		Vector3[] array2 = new Vector3[nNormales];
		for (int i = 0; i < nSommets; i++)
		{
			array[i] = geometrie.GetSommet(i);
		}
		for (int j = 0; j < nNormales; j++)
		{
			array2[j] = geometrie.GetNormale(j);
		}
		return new Geometrie
		{
			sommets = array,
			normales = array2
		};
	}
}
