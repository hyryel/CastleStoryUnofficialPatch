using UnityEngine;

namespace Brix.Engine.FlatWorld;

public struct FlatWorldHandle
{
	public const int InternalType = 255;

	public const int InternalVolume = 255;

	public const int InternalDepth = 255;

	public const int InternalLevels = -1;

	public IFlatWorldColors w;

	public int i;

	public static FlatWorldHandle Empty = new FlatWorldHandle
	{
		i = -1
	};

	public static FlatWorldHandle Internal = new FlatWorldHandle
	{
		i = -2
	};

	public bool IsMissing => i == -1;

	public bool IsInternal => i == -2;

	public bool IsCustom => i >= 0;

	public int DataX => i % w.Type.Width;

	public int DataZ => i / w.Type.Width;

	public bool IsNonEmpty => IsCustom ? (w.Type.GetPixel(DataX, DataZ).r != 0) : IsInternal;

	public int Type => IsCustom ? w.Type.GetPixel(DataX, DataZ).r : ((!IsMissing) ? 255 : 0);

	public int Volume => IsCustom ? w.Volume.GetPixel(DataX, DataZ).r : ((!IsMissing) ? 255 : 0);

	public int Depth => IsCustom ? w.Depth.GetPixel(DataX, DataZ).r : ((!IsMissing) ? 255 : 0);

	public bool Smooth => IsCustom && w.Smooth.GetPixel(DataX, DataZ).r != 0;

	public int Config => IsCustom ? w.Config.GetPixel(DataX, DataZ).r : 0;

	public int Levels => IsCustom ? Soft.DecodeInt(w.Level.GetPixel(DataX, DataZ)) : ((!IsMissing) ? (-1) : 0);

	public int LegacyLevels
	{
		get
		{
			if (IsCustom)
			{
				Color32 pixel = w.Level.GetPixel(DataX, DataZ);
				return (pixel.r >> 6) | (pixel.g >> 6 << 4) | (pixel.b >> 6 << 12) | (pixel.a >> 6 << 8);
			}
			return (!IsMissing) ? 13107 : 0;
		}
	}

	public int GetProcessedType(MondeAtlas atlas)
	{
		return IsCustom ? w.Type.GetPixel(DataX, DataZ).r : ((!IsMissing) ? atlas.DefaultDeepType : 0);
	}
}
