using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;

[Serializable]
public class ShaderPropertyInfo
{
	public MeshRenderer meshRenderer;

	public string typeInfo;

	public string descInfo;

	public Vector3 rangeInfo;

	public string nameInfo;

	private static HashSet<Material> selfCreated = new HashSet<Material>();

	private static bool connected;

	private static SignalConnection resetConnection;

	public ShaderPropertyInfo()
	{
	}

	public ShaderPropertyInfo(MeshRenderer meshRenderer, string typeInfo, string descInfo, string nameInfo, Vector3 rangeInfo)
	{
		this.meshRenderer = meshRenderer;
		this.typeInfo = typeInfo;
		this.descInfo = descInfo;
		this.rangeInfo = rangeInfo;
		this.nameInfo = nameInfo;
	}

	public static void Reset()
	{
		selfCreated.Clear();
		resetConnection.Disconnect();
		connected = false;
	}

	public static Material GetMaterial(MeshRenderer meshRenderer)
	{
		if (Application.isPlaying && meshRenderer != null)
		{
			if (selfCreated.Contains(meshRenderer.material))
			{
				meshRenderer.material = new Material(meshRenderer.sharedMaterial);
				selfCreated.Add(meshRenderer.material);
				if (!connected)
				{
					resetConnection = GameSignals.Connect(GameSignals.LevelUnloaded, Reset);
				}
			}
			return meshRenderer.material;
		}
		return null;
	}
}
