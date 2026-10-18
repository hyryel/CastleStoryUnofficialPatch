using System.Collections.Generic;
using Brix.Utils;
using Brix.Utils.Collections;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

internal class DeepLayer : Layer
{
	public const byte InternalType = byte.MaxValue;

	public VoxelTerrainType GetDeepType(Vector3 voxelPos, Color32 texColor)
	{
		float distanceFromTop = GetDistanceFromTop(texColor, voxelPos);
		VoxelTerrainType result = (VoxelTerrainType)atlas.DefaultDeepType;
		List<KeyValuePair<VoxelTerrainType, VoxelNoiseConfigurator.NoiseConfig>> list = VoxelNoiseConfigurator.GetTerrainTypesWithNoise().ToPooledList();
		for (int i = 0; i < list.Count; i++)
		{
			KeyValuePair<VoxelTerrainType, VoxelNoiseConfigurator.NoiseConfig> keyValuePair = list[i];
			VoxelTerrainType key = keyValuePair.Key;
			VoxelNoiseConfigurator.NoiseConfig value = keyValuePair.Value;
			double value2 = value.NoiseComponent.GetValue(voxelPos);
			if ((!value.UseRange || (distanceFromTop >= (float)value.Start && distanceFromTop <= (float)value.End)) && value2 > (double)value.Threshold)
			{
				result = key;
			}
		}
		CollectionPool.Release(ref list);
		return result;
	}

	private float GetDistanceFromTop(Color32 texColor, Vector3 position)
	{
		return (int)texColor.b;
	}

	public override void GetVoxelData(XYZ p, ref Color32 data)
	{
		if (data.r == byte.MaxValue)
		{
			data.r = (byte)GetDeepType(p.ToVector3(), data);
		}
	}

	public override void InitialData(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels2);
		for (int i = 0; i < count; i++)
		{
			ref Color32 reference = ref orAllocPixels[i];
			reference = pixels2[i];
			byte r = orAllocPixels[i].r;
			if (r == byte.MaxValue)
			{
				r = (byte)GetDeepType((Soft.DecodeXYZ(Soft.DecodeInt(pixels[i])) + resultPosition).ToVector3(), orAllocPixels[i]);
				if (r == 0)
				{
					orAllocPixels[i] = default(Color32);
				}
				else
				{
					orAllocPixels[i].r = r;
				}
			}
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}
}
