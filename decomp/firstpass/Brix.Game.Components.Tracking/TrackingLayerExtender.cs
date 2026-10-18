using Brix.Util;

namespace Brix.Game.Components.Tracking;

public static class TrackingLayerExtender
{
	public static LongBitsEnumerable Bits(this TrackingLayer mask)
	{
		return new LongBitsEnumerable((long)mask);
	}

	public static TrackingLayer ToMask(this TrackingLayerProperty p)
	{
		return (TrackingLayer)(1 << (int)p);
	}
}
