using System.Collections.Generic;
using UnityEngine;

public static class ShaderPropertyInfoGenerator
{
	public static string[] acceptedTypeNames = new string[4] { "COLOR", "RANGE", "FLOAT", "VECTOR" };

	public static ShaderPropertyInfo[] Generate(MeshRenderer meshRenderer)
	{
		List<ShaderPropertyInfo> list = new List<ShaderPropertyInfo>();
		return list.ToArray();
	}
}
