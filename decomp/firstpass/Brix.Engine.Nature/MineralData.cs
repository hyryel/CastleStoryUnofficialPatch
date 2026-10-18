using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Engine.Nature;

public abstract class MineralData : NatureData
{
	private IVolume _volume;

	public IVolume Volume => _volume;

	public int Variation { get; set; }

	public override int MaxRessources => 40;

	public override int CurrentRessources { get; set; }

	public MineralData(float growth, int variation, XYZ rootPos)
	{
		Growth = growth;
		Variation = variation;
		RootPos = rootPos;
		CurrentRessources = MaxRessources;
		_volume = FreeVolume.Make(NatureVolume.SmallMineral, RootPos, 0);
		_volume.Register();
	}

	public int GetMeshVariationIndex()
	{
		return Variation;
	}

	public int GetVariationIndex(XYZ position)
	{
		Random.InitState(position.GetHashCode());
		return Random.Range(0, 16);
	}

	public Vector3 GetDisplacement(XYZ position)
	{
		return RootPos.ToVector3();
	}

	public void Translate(Vector3 delta)
	{
		RootPos += XYZ.FromVector3(delta);
	}

	public void Unregister()
	{
		_volume.Unregister();
	}

	public void Dispose()
	{
		_volume.Dispose();
	}

	public float GetShards(XYZ position, ref List<MineralShardData> resultShards)
	{
		Texture2D growthNoiseTex = GetGrowthNoiseTex();
		int growthNoiseTexRepeats = GetGrowthNoiseTexRepeats();
		int width = growthNoiseTex.width;
		int height = growthNoiseTex.height;
		int num = width / growthNoiseTexRepeats;
		int num2 = height / growthNoiseTexRepeats;
		int num3 = position.x % growthNoiseTexRepeats * num;
		int num4 = position.z % growthNoiseTexRepeats * num2;
		int niveaux = Voxel.GetNiveaux(position);
		int num5 = 0;
		float num6 = 0f;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				if (num5 >= 3)
				{
					break;
				}
				float r = growthNoiseTex.GetPixel(num3 + j, num4 + i).r;
				if (!(r < 0.1f))
				{
					float num7 = (float)j / ((float)num - 1f) - 0.5f;
					float num8 = (float)i / ((float)num2 - 1f) - 0.5f;
					float y = Voxel.ComputeSurfaceHeightFromNiveaux(niveaux, num7, num8);
					resultShards.Add(new MineralShardData
					{
						Weight = r,
						Offset = new Vector3(num7, y, num8)
					});
					num6 += r;
					num5++;
				}
			}
		}
		return num6;
	}

	public abstract Texture2D GetGrowthNoiseTex();

	public abstract int GetGrowthNoiseTexRepeats();

	public Vector3 GetShardDisplacement(XYZ position, MineralShardData shard)
	{
		return GetDisplacement(position) + shard.Offset;
	}

	public Quaternion GetShardOrientation(XYZ position, MineralShardData shard)
	{
		int num = GetVariationIndex(position) & 0xF;
		return Quaternion.Euler(0f, num * 45, Mathf.Lerp(-20f, 20f, (float)((num + 5) % 15) / 15f));
	}

	public float GetShardScale(XYZ position, MineralShardData shard)
	{
		return Mathf.Lerp(0.3f, 1.2f, Mathf.InverseLerp(0f, 1f, Growth * shard.Weight));
	}
}
