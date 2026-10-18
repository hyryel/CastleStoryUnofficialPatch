using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine.Environment;

[Serializable]
public class Biome
{
	public string Name;

	public Material BiomeMat;

	public List<Transform> Plants;

	public Transform Trees;

	public Transform Boulders;
}
