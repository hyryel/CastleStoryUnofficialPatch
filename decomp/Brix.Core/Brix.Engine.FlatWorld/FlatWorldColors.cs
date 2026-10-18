namespace Brix.Engine.FlatWorld;

public class FlatWorldColors : IFlatWorldColors
{
	protected TexColors Filling = new TexColors();

	protected TexColors Column = new TexColors();

	protected TexColors Xz = new TexColors();

	protected TexColors Offset = new TexColors();

	protected TexColors Type = new TexColors();

	protected TexColors Volume = new TexColors();

	protected TexColors Depth = new TexColors();

	protected TexColors Smooth = new TexColors();

	protected TexColors Level = new TexColors();

	protected TexColors Config = new TexColors();

	protected TexColors Normal = new TexColors();

	protected TexColors UvBase = new TexColors();

	protected TexColors UvTrans = new TexColors();

	protected TexColors UvSpecial = new TexColors();

	protected int CountOffsets;

	ITexColors IFlatWorldColors.Filling => Filling;

	ITexColors IFlatWorldColors.Column => Column;

	ITexColors IFlatWorldColors.Xz => Xz;

	ITexColors IFlatWorldColors.Offset => Offset;

	ITexColors IFlatWorldColors.Type => Type;

	ITexColors IFlatWorldColors.Volume => Volume;

	ITexColors IFlatWorldColors.Depth => Depth;

	ITexColors IFlatWorldColors.Smooth => Smooth;

	ITexColors IFlatWorldColors.Level => Level;

	ITexColors IFlatWorldColors.Config => Config;

	ITexColors IFlatWorldColors.Normal => Normal;

	ITexColors IFlatWorldColors.UvBase => UvBase;

	ITexColors IFlatWorldColors.UvTrans => UvTrans;

	ITexColors IFlatWorldColors.UvSpecial => UvSpecial;

	int IFlatWorldColors.CountOffsets
	{
		get
		{
			return CountOffsets;
		}
		set
		{
			CountOffsets = value;
		}
	}

	public void Destroy()
	{
		Filling.Destroy();
		Column.Destroy();
		Xz.Destroy();
		Offset.Destroy();
		Type.Destroy();
		Volume.Destroy();
		Depth.Destroy();
		Smooth.Destroy();
		Level.Destroy();
		Config.Destroy();
		Normal.Destroy();
		UvBase.Destroy();
		UvTrans.Destroy();
		UvSpecial.Destroy();
		CountOffsets = 0;
	}
}
