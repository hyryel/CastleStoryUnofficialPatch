using System;
using Brix.Engine;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.Game.Components;

internal class CobbleBrickValidator : DefaultBlockValidator
{
	private static System.Random rand = new System.Random();

	public override void SetUnstable(ref ValidationData vData)
	{
		Vector3 position = vData.Position.ToVector3();
		vData.Volume.Unregister();
		IBlockData blockData = Voxel.GetBlockData(vData.Position);
		if (!blockData.IsNull)
		{
			blockData.DestroyBlock(withParticles: true, withGarnotte: true);
			SpawnGarnotte(position);
		}
	}

	protected virtual void SpawnGarnotte(Vector3 position)
	{
		Factory.Instantiate(RawResources.RawStone, GetLocalRandomPos(position), Quaternion.identity);
		Factory.Instantiate(RawResources.RawStone, GetLocalRandomPos(position), Quaternion.identity);
		Factory.Instantiate(RawResources.RawStone, GetLocalRandomPos(position), Quaternion.identity);
	}

	private Vector3 GetLocalRandomPos(Vector3 position)
	{
		Vector3 result = position;
		result.x += (float)(rand.NextDouble() - 0.5);
		result.y += (float)(rand.NextDouble() - 0.5);
		result.z += (float)(rand.NextDouble() - 0.5);
		return result;
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<CobbleBrickBPValidator>();
	}
}
