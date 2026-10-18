using System;
using UnityEngine;

namespace Brix.Engine.TerrainAtlas;

[Serializable]
public class TerrainAtlasPaddingSettings : ScriptableObject
{
	public int uvAtlasWidth = 1024;

	public int uvInnerDecal = 2;

	public int uvOuterDecal = 0;
}
