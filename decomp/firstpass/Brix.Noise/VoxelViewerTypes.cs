using System;
using UnityEngine;

namespace Brix.Noise;

[Serializable]
[HideInInspector]
public class VoxelViewerTypes
{
	public float Threshold;

	public Color32 Color;

	public string Name;

	public NoiseBaseComponent NoiseComponent;

	public bool UseRange;

	public int Start;

	public int End;
}
