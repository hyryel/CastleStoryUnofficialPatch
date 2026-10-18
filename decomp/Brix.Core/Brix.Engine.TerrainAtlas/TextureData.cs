using System;
using UnityEngine;

namespace Brix.Engine.TerrainAtlas;

[Serializable]
public class TextureData
{
	public Texture2D diffuse;

	public Texture2D normals;

	public Texture2D transitionsDiffuse;

	public Texture2D transitionsNormals;

	public Texture2D transitionsSpecular;
}
