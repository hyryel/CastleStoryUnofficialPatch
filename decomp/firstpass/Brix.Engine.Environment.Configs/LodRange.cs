using System;

namespace Brix.Engine.Environment.Configs;

[Serializable]
public class LodRange
{
	public static LodRange Empty = new LodRange(0, 0);

	public int Min;

	public int Max;

	[NonSerialized]
	public LodTrack LodTrack;

	[NonSerialized]
	public LodRange Finer;

	[NonSerialized]
	public LodRange Coarser;

	public LodRange(int min, int max)
	{
		Min = min;
		Max = max;
	}
}
