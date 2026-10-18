using System;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public static class MatExtender
{
	private static Material[] FlatWorldMaterials = new Material[56];

	public static Material CreateMaterial(Mat mat)
	{
		string name = Enum.GetName(typeof(Mat), mat);
		Shader shader = Shader.Find("FlatWorld/" + name);
		if (shader != null)
		{
			return new Material(shader);
		}
		return new Material(Shader.Find("Diffuse"));
	}

	public static Material GetMaterial(this Mat mat)
	{
		return FlatWorldMaterials[(int)mat] ?? (FlatWorldMaterials[(int)mat] = CreateMaterial(mat));
	}
}
