namespace Brix.Engine.FlatWorld;

public interface IFlatWorldColors
{
	ITexColors Filling { get; }

	ITexColors Column { get; }

	ITexColors Xz { get; }

	ITexColors Offset { get; }

	ITexColors Type { get; }

	ITexColors Volume { get; }

	ITexColors Depth { get; }

	ITexColors Smooth { get; }

	ITexColors Level { get; }

	ITexColors Config { get; }

	ITexColors Normal { get; }

	ITexColors UvBase { get; }

	ITexColors UvTrans { get; }

	ITexColors UvSpecial { get; }

	int CountOffsets { get; set; }

	void Destroy();
}
