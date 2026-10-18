using System;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.Game.Components;

public class WoodBlockValidator : DefaultBlockValidator
{
	private static System.Random rand = new System.Random();

	private Factory.AssetKey[] splinterKeys = new Factory.AssetKey[4]
	{
		Shards.Splinter_1,
		Shards.Splinter_2,
		Shards.Splinter_3,
		Shards.Splinter_4
	};

	public override void SetUnstable(ref ValidationData vData)
	{
		Vector3 position = vData.Position.ToVector3();
		Quaternion rotation = vData.Rotation;
		if (DestroyBlock(vData))
		{
			SpawnSplinters(position, rotation);
		}
	}

	protected virtual void SpawnSplinters(Vector3 position, Quaternion rotation)
	{
		Factory.Instantiate(splinterKeys[rand.Next(0, 4)], GetLocalRandomPos(position), rotation);
		Factory.Instantiate(splinterKeys[rand.Next(0, 4)], GetLocalRandomPos(position), rotation);
	}

	private Vector3 GetLocalRandomPos(Vector3 position)
	{
		Vector3 result = position;
		result.x += (float)(rand.NextDouble() - 0.5);
		result.y += (float)(rand.NextDouble() - 0.5);
		result.z += (float)(rand.NextDouble() - 0.5);
		return result;
	}
}
