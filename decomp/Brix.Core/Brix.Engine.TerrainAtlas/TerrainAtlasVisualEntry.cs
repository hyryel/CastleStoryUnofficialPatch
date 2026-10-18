using System;
using UnityEngine;

namespace Brix.Engine.TerrainAtlas;

[Serializable]
public class TerrainAtlasVisualEntry : ScriptableObject
{
	public Vector2 uv;

	public bool useTransitions;

	public Vector2 uvTransition;

	public int layer;

	public bool useSpecial;

	public Vector2 uvSpecial;

	public bool useWeightedNoise;

	public WeightedNoiseData weightedNoise = new WeightedNoiseData();
}
