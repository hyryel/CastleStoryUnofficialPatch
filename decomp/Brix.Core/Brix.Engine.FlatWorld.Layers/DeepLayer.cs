using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

internal class DeepLayer : Layer, ISelectableLayer
{
	private static BrixLogger logger = new BrixLogger();

	public const byte InternalType = byte.MaxValue;

	private static readonly Vector3 translation = new Vector3(1.23f, 2.48f, 3.91f);

	private const float scale = 0.231f;

	private Color32[] typeSampleColors;

	private XY typeSampleSize;

	public override void Init(MondeAtlas atlas, Common common)
	{
		base.Init(atlas, common);
		typeSampleColors = common.TypeSampleColors.GetPixels32();
		typeSampleSize = new XY(common.TypeSampleColors.Width, common.TypeSampleColors.Height);
	}

	public override void InitialData(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels2);
		for (int i = 0; i < count; i++)
		{
			orAllocPixels[i] = pixels2[i];
			byte r = orAllocPixels[i].r;
			if (r == byte.MaxValue)
			{
				XYZ p = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i])) + resultPosition;
				orAllocPixels[i].r = GetDeepType(p);
			}
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public override void GetVoxelData(XYZ p, ref Color32 data)
	{
		if (data.r == byte.MaxValue)
		{
			data.r = GetDeepType(p);
		}
	}

	private byte GetDeepType(XYZ p)
	{
		Vector3 vector = p.ToVector3() * 0.231f;
		Vector3 v = vector + translation;
		float num = Mathf.Clamp01(common.noiseGenerator.Noise(vector));
		float num2 = Mathf.Clamp01(common.noiseGenerator.Noise(v));
		float num3 = num - Mathf.Floor(num);
		float num4 = num2 - Mathf.Floor(num2);
		XY xY = XY.Floor(num3 * (float)typeSampleSize.x, num4 * (float)typeSampleSize.y);
		Color32 color = typeSampleColors[xY.x + xY.y * typeSampleSize.x];
		byte b = color.r;
		if (atlas.IsEmpty(b))
		{
			logger.WarnFormat("DeepType doesn't exist: type={0}, p={1}", b, p);
			b = (byte)atlas.DefaultDeepType;
		}
		return b;
	}
}
