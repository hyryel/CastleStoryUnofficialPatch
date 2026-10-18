namespace Brix.Engine.FlatWorld;

public static class IFlatWorldColorsExtender
{
	public static void CopyFrom(this IFlatWorldColors self, IFlatWorldColors other)
	{
		self.Filling.CopyFrom(other.Filling);
		self.Column.CopyFrom(other.Column);
		self.Xz.CopyFrom(other.Xz);
		self.Offset.CopyFrom(other.Offset);
		self.Type.CopyFrom(other.Type);
		self.Volume.CopyFrom(other.Volume);
		self.Depth.CopyFrom(other.Depth);
		self.Smooth.CopyFrom(other.Smooth);
		self.Level.CopyFrom(other.Level);
		self.Config.CopyFrom(other.Config);
		self.Normal.CopyFrom(other.Normal);
		self.UvBase.CopyFrom(other.UvBase);
		self.UvTrans.CopyFrom(other.UvTrans);
		self.UvSpecial.CopyFrom(other.UvSpecial);
		self.CountOffsets = other.CountOffsets;
	}

	public static void CopyTo(this IFlatWorldColors self, IFlatWorldColors other)
	{
		other.CopyFrom(self);
	}

	public static void Swap(this IFlatWorldColors self, IFlatWorldColors other)
	{
		self.Filling.Swap(other.Filling);
		self.Column.Swap(other.Column);
		self.Xz.Swap(other.Xz);
		self.Offset.Swap(other.Offset);
		self.Type.Swap(other.Type);
		self.Volume.Swap(other.Volume);
		self.Depth.Swap(other.Depth);
		self.Smooth.Swap(other.Smooth);
		self.Level.Swap(other.Level);
		self.Config.Swap(other.Config);
		self.Normal.Swap(other.Normal);
		self.UvBase.Swap(other.UvBase);
		self.UvTrans.Swap(other.UvTrans);
		self.UvSpecial.Swap(other.UvSpecial);
		self.SwapCount(other);
	}

	public static void SwapPostRender(this IFlatWorldColors self, IFlatWorldColors other)
	{
		self.Filling.Swap(other.Filling);
		self.Column.Swap(other.Column);
		self.Xz.Swap(other.Xz);
		self.Offset.Swap(other.Offset);
		self.Type.Swap(other.Type);
		self.Volume.Swap(other.Volume);
		self.Level.Swap(other.Level);
		self.Config.Swap(other.Config);
		self.SwapCount(other);
	}

	private static void SwapCount(this IFlatWorldColors self, IFlatWorldColors other)
	{
		int countOffsets = self.CountOffsets;
		self.CountOffsets = other.CountOffsets;
		other.CountOffsets = countOffsets;
	}
}
