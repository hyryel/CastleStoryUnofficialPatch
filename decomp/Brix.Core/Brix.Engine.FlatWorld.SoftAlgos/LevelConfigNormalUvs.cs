using System.Linq;
using Brix.Engine.TerrainAtlas;
using Brix.Utils;
using Brix.Utils.Collections;
using UnityEngine;

namespace Brix.Engine.FlatWorld.SoftAlgos;

public class LevelConfigNormalUvs : Poolable
{
	private readonly int[] iConfig = new int[6] { 14, 16, 10, 22, 4, 12 };

	private readonly int[] iLevel = new int[9] { 7, 16, 25, 4, 13, 22, 1, 10, 19 };

	private readonly int[] iTrans = new int[19]
	{
		14, 14, 14, 15, 16, 17, 9, 10, 11, 21,
		22, 23, 3, 4, 5, 12, 12, 12, 12
	};

	private readonly int[] iVolumes = Enumerable.Range(0, 27).ToArray();

	private readonly int[] iTypes = Enumerable.Range(0, 27).ToArray();

	private readonly int[] iRubik = new int[27];

	private readonly int[] lvs = new int[9];

	private readonly int[] volumes = new int[27];

	private readonly int[] types = new int[27];

	private readonly Color32[] lvColors = new Color32[27];

	private readonly Vector3[] normals = new Vector3[9];

	public static byte ComputeAverageLevel(int v1, int v2, int v3, int v4)
	{
		if (v1 > 4 || v2 > 4 || v3 > 4 || v4 > 4)
		{
			return byte.MaxValue;
		}
		if (v1 <= 0 || v2 <= 0 || v3 <= 0 || v4 <= 0)
		{
			return 0;
		}
		int num = v1 + v2 + v3 + v4 - 2 >> 2;
		return (byte)(num * 85);
	}

	public static int ComputeTotalLevel(Color32 lv)
	{
		return lv.r + lv.g + lv.b + lv.a;
	}

	public void LevelConfig(ITexColors levelTex, ITexColors configTex, ITexColors columnTex, ITexColors xzTex, ITexColors offsetTex, ITexColors volumeTex, ITexColors smoothTex, int count)
	{
		Color32[] pixels = columnTex.GetPixels32();
		Color32[] pixels2 = xzTex.GetPixels32();
		Color32[] pixels3 = offsetTex.GetPixels32();
		Color32[] pixels4 = volumeTex.GetPixels32();
		Color32[] pixels5 = smoothTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(levelTex, offsetTex, pixels3);
		Color32[] orAllocPixels2 = Soft.GetOrAllocPixels32(configTex, offsetTex, pixels3);
		for (int i = 0; i < count; i++)
		{
			if (pixels4[i].r == 0)
			{
				orAllocPixels[i] = new Color32(0, 0, 0, 0);
				orAllocPixels2[i] = new Color32(0, 0, 0, 0);
				continue;
			}
			XYZ p = Soft.DecodeXYZ(Soft.DecodeInt(pixels3[i]));
			Soft.GetOffsetRubik(iRubik, p, pixels2, pixels, columnTex.Width, columnTex.Height);
			for (int j = 0; j < iVolumes.Length; j++)
			{
				volumes[iVolumes[j]] = Soft.GetOffsetDataR(iRubik[iVolumes[j]], pixels4, 0, 5);
			}
			for (int k = 0; k < 9; k++)
			{
				int num = 1 + k % 3 * 3 + k / 3 * 9;
				if (volumes[num] > 0 && volumes[num - 1] > 0 && volumes[num + 1] > 0)
				{
					volumes[num] = 5;
				}
			}
			if (pixels5[i].r == 0)
			{
				volumes[13] = 5;
			}
			int num2 = 0;
			for (int l = 0; l < iConfig.Length; l++)
			{
				if (volumes[iConfig[l]] == 0)
				{
					num2 |= 1 << l;
				}
			}
			bool flag = volumes[14] == 0;
			bool flag2 = !flag && volumes[12] == 0;
			for (int m = 1; m < 5; m++)
			{
				if (volumes[iConfig[m]] != 0)
				{
					bool flag3 = volumes[iConfig[m] + 1] == 0;
					bool flag4 = !flag3 && volumes[iConfig[m] - 1] == 0;
					if ((flag && flag4) || (flag2 && flag3))
					{
						num2 |= 1 << m;
					}
				}
			}
			for (int n = 0; n < iLevel.Length; n++)
			{
				lvs[n] = volumes[iLevel[n]];
			}
			orAllocPixels[i] = new Color32(ComputeAverageLevel(lvs[0], lvs[1], lvs[3], lvs[4]), ComputeAverageLevel(lvs[1], lvs[2], lvs[4], lvs[5]), ComputeAverageLevel(lvs[3], lvs[4], lvs[6], lvs[7]), ComputeAverageLevel(lvs[4], lvs[5], lvs[7], lvs[8]));
			orAllocPixels2[i] = new Color32((byte)num2, 0, 0, 0);
		}
		levelTex?.SetPixels32(orAllocPixels, offsetTex.Width, offsetTex.Height);
		configTex?.SetPixels32(orAllocPixels2, offsetTex.Width, offsetTex.Height);
	}

	private static Vector3 GetLocalNormal(Color32 lv)
	{
		return new Vector3(lv.b - lv.a - (lv.g - lv.r), 510f, lv.b - lv.r - (lv.g - lv.a));
	}

	private static Vector3 GetAverageNormal(int yRef, Color32 lvs0, Color32 lvs1, Color32 lvs2, Color32 lvs3, Vector3 n0, Vector3 n1, Vector3 n2, Vector3 n3)
	{
		Vector3 zero = Vector3.zero;
		if (lvs0.a == yRef)
		{
			zero += n0;
		}
		if (lvs1.b == yRef)
		{
			zero += n1;
		}
		if (lvs2.g == yRef)
		{
			zero += n2;
		}
		if (lvs3.r == yRef)
		{
			zero += n3;
		}
		return zero;
	}

	private static int GetWeightedNoiseRow(int noise, WeightedNoiseData rowWeights)
	{
		int num = rowWeights.row0 + rowWeights.row1 + rowWeights.row2 + rowWeights.row3;
		int num2 = num * (noise & 0x3F) / 64;
		if (num2 < rowWeights.row3)
		{
			return 3;
		}
		num2 -= rowWeights.row3;
		if (num2 < rowWeights.row2)
		{
			return 2;
		}
		num2 -= rowWeights.row2;
		if (num2 < rowWeights.row1)
		{
			return 1;
		}
		return 0;
	}

	private static int GetWeightedNoiseVariant(int r, WeightedNoiseData rowWeights)
	{
		int weightedNoiseRow = GetWeightedNoiseRow(r, rowWeights);
		int num = r % 4;
		return weightedNoiseRow * 4 + num;
	}

	public void NormalUvs(ITexColors normalTex, ITexColors uvBaseTex, ITexColors uvTransTex, ITexColors uvSpecialTex, ITexColors columnTex, ITexColors xzTex, ITexColors offsetTex, ITexColors typeTex, ITexColors volumeTex, ITexColors levelTex, ITexColors noiseTex, XYZ worldStart, int count)
	{
		Color32[] pixels = columnTex.GetPixels32();
		Color32[] pixels2 = xzTex.GetPixels32();
		Color32[] pixels3 = offsetTex.GetPixels32();
		Color32[] pixels4 = typeTex.GetPixels32();
		Color32[] pixels5 = volumeTex.GetPixels32();
		Color32[] pixels6 = levelTex.GetPixels32();
		Color32[] pixels7 = noiseTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(normalTex, offsetTex, pixels3);
		Color32[] orAllocPixels2 = Soft.GetOrAllocPixels32(uvBaseTex, offsetTex, pixels3);
		Color32[] orAllocPixels3 = Soft.GetOrAllocPixels32(uvTransTex, offsetTex, pixels3);
		Color32[] orAllocPixels4 = Soft.GetOrAllocPixels32(uvSpecialTex, offsetTex, pixels3);
		MondeAtlas atlas = MondeAtlas.GetAtlas("Terrain_LOD0");
		for (int i = 0; i < count; i++)
		{
			XYZ xYZ = Soft.DecodeXYZ(Soft.DecodeInt(pixels3[i]));
			Soft.GetOffsetRubik(iRubik, xYZ, pixels2, pixels, columnTex.Width, columnTex.Height);
			for (int j = 0; j < iTypes.Length; j++)
			{
				types[iTypes[j]] = Soft.GetOffsetDataR(iRubik[iTypes[j]], pixels4, 0, 255);
			}
			Color32 color = pixels5[i];
			if (color.r > 0 && color.r <= 4)
			{
				for (int k = 0; k < iLevel.Length; k++)
				{
					int num = iLevel[k];
					int offsetDataR = Soft.GetOffsetDataR(iRubik[num], pixels5, 0, 5);
					int num2 = ((offsetDataR >= 5) ? 1 : ((offsetDataR == 0) ? (-1) : 0));
					Color32 offsetData = Soft.GetOffsetData(iRubik[num + num2], pixels6, new Color32(0, 0, 0, 0), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					normals[k] = GetLocalNormal(offsetData);
					int num3 = (num2 + 1) * 3;
					lvColors[k] = new Color32((byte)((offsetData.r + 42) / 85 + num3), (byte)((offsetData.g + 42) / 85 + num3), (byte)((offsetData.b + 42) / 85 + num3), (byte)((offsetData.a + 42) / 85 + num3));
				}
				orAllocPixels[i] = new Color32(Soft.EncodeNormal(GetAverageNormal(lvColors[4].r, lvColors[0], lvColors[1], lvColors[3], lvColors[4], normals[0], normals[1], normals[3], normals[4]).normalized), Soft.EncodeNormal(GetAverageNormal(lvColors[4].g, lvColors[1], lvColors[2], lvColors[4], lvColors[5], normals[1], normals[2], normals[4], normals[5]).normalized), Soft.EncodeNormal(GetAverageNormal(lvColors[4].b, lvColors[3], lvColors[4], lvColors[6], lvColors[7], normals[3], normals[4], normals[6], normals[7]).normalized), Soft.EncodeNormal(GetAverageNormal(lvColors[4].a, lvColors[4], lvColors[5], lvColors[7], lvColors[8], normals[4], normals[5], normals[7], normals[8]).normalized));
			}
			else
			{
				orAllocPixels[i] = new Color32(119, 119, 119, 119);
			}
			byte r = pixels4[i].r;
			Vector2 uv = atlas.GetUv(r);
			XYZ xYZ2 = xYZ + worldStart;
			int num4 = Soft.RepeatIndex(xYZ2.x + 7 * xYZ2.y, noiseTex.Width) + Soft.RepeatIndex(xYZ2.z + 17 * xYZ2.y, noiseTex.Height) * noiseTex.Width;
			int num5 = ((!atlas.UsesWeightedNoise(r)) ? (pixels7[num4].r * 16 / 255 % 16) : GetWeightedNoiseVariant(pixels7[num4].r, atlas.GetNoiseRowWeights(r)));
			orAllocPixels2[i] = new Color32((byte)Mathf.RoundToInt(uv.x * 85f), (byte)Mathf.RoundToInt(uv.y * 85f), (byte)(num5 % 4 * 85), (byte)(num5 / 4 * 85));
			if (atlas.HasUvSpecial(r))
			{
				Vector2 uvSpecial = atlas.GetUvSpecial(r);
				orAllocPixels3[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				orAllocPixels4[i] = new Color32((byte)Mathf.RoundToInt(uvSpecial.x * 85f), (byte)Mathf.RoundToInt(uvSpecial.y * 85f), (byte)(num5 % 4 * 85), (byte)(num5 / 4 * 85));
				continue;
			}
			int layer = atlas.GetLayer(r);
			SmallOrderedSubset_8from255 transTypeList = default(SmallOrderedSubset_8from255);
			int num6 = 0;
			bool flag = atlas.IsCroute(r);
			for (int l = 0; l < iTrans.Length; l++)
			{
				if (!flag)
				{
					int num7 = iTrans[l] % 3;
					if (num7 == 0 || (num7 == 2 && !atlas.IsCroute(types[iTrans[l]])))
					{
						continue;
					}
				}
				else if (!IsGoodBranching(lvColors, new XYZ(iTrans[l] / 9 - 1, iTrans[l] % 3 - 1, iTrans[l] % 9 / 3 - 1)))
				{
					continue;
				}
				int num8 = types[iTrans[l]];
				if (num8 != r && atlas.HasUvTransition(num8))
				{
					int layer2 = atlas.GetLayer(num8);
					if (layer2 >= layer)
					{
						int orAddIndexOf = transTypeList.GetOrAddIndexOf(num8);
						num6 |= 1 << orAddIndexOf * 8 + l / 3;
					}
				}
			}
			orAllocPixels3[i] = TransBitsToColor(num6, transTypeList, atlas);
			orAllocPixels4[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		normalTex?.SetPixels32(orAllocPixels, offsetTex.Width, offsetTex.Height);
		uvBaseTex?.SetPixels32(orAllocPixels2, offsetTex.Width, offsetTex.Height);
		uvTransTex?.SetPixels32(orAllocPixels3, offsetTex.Width, offsetTex.Height);
		uvSpecialTex?.SetPixels32(orAllocPixels4, offsetTex.Width, offsetTex.Height);
	}

	private static Color32 TransBitsToColor(int transBits, SmallOrderedSubset_8from255 transTypeList, MondeAtlas atlas)
	{
		Color32 result = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		if (transBits == 0)
		{
			return result;
		}
		result.r = TransBitsToColorComponent(transBits, atlas.GetUvTransition(transTypeList[0]));
		transBits >>= 8;
		if (transBits == 0)
		{
			return result;
		}
		result.g = TransBitsToColorComponent(transBits, atlas.GetUvTransition(transTypeList[1]));
		transBits >>= 8;
		if (transBits == 0)
		{
			return result;
		}
		result.b = TransBitsToColorComponent(transBits, atlas.GetUvTransition(transTypeList[2]));
		transBits >>= 8;
		if (transBits == 0)
		{
			return result;
		}
		result.a = TransBitsToColorComponent(transBits, atlas.GetUvTransition(transTypeList[3]));
		return result;
	}

	private static byte TransBitsToColorComponent(int transBits, Vector2 uvTrans)
	{
		int num = 1 - ((transBits >> 1) & 1);
		int num2 = 1 - ((transBits >> 2) & 1);
		int num3 = 1 - ((transBits >> 3) & 1);
		int num4 = 1 - ((transBits >> 4) & 1);
		return (byte)(num | (num3 << 1) | (Mathf.RoundToInt(uvTrans.x) << 2) | (num2 << 4) | (num4 << 5) | (Mathf.RoundToInt(uvTrans.y) << 6));
	}

	private static bool IsGoodBranching(Color32[] lvs, XYZ d)
	{
		Color32 color = lvs[4];
		if (d.x == 1 && d.z == 0)
		{
			return color.g == lvs[5].r && color.a == lvs[5].b;
		}
		if (d.x == -1 && d.z == 0)
		{
			return color.r == lvs[3].g && color.b == lvs[3].a;
		}
		if (d.x == 0 && d.z == 1)
		{
			return color.r == lvs[1].b && color.g == lvs[1].a;
		}
		if (d.x == 0 && d.z == -1)
		{
			return color.b == lvs[7].r && color.a == lvs[7].g;
		}
		return true;
	}
}
