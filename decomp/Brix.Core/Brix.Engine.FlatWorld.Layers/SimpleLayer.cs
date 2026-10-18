namespace Brix.Engine.FlatWorld.Layers;

public abstract class SimpleLayer : Layer
{
	public TexColors fillingTex = new TexColors();

	private TexColors translatedColumnTex = new TexColors();

	public override void Destroy()
	{
		fillingTex.Destroy();
		translatedColumnTex.Destroy();
		base.Destroy();
	}

	public override void InitFromHeightmap(ITexColors worldTex)
	{
		base.InitFromHeightmap(worldTex);
		fillingTex.CopyFrom(worldTex);
	}

	public override void AdjustBounds(ref VoxelBounds bounds)
	{
		bounds.Encapsulate(Layer.GetBoundsFromColumnTex(fillingTex));
	}

	public ITexColors GetTemporaryTranslatedColumnTex(ITexColors tex, XYZ start, int width, int height, bool ignorePartialRanges = false)
	{
		return Layer.GetTemporaryTranslatedColumnTex_Soft(translatedColumnTex, tex, start, width, height, ignorePartialRanges);
	}

	public void ReleaseTemporaryTex(ITexColors tempTex, ITexColors originalTex)
	{
		if (tempTex != originalTex && tempTex != translatedColumnTex)
		{
			tempTex.Destroy();
		}
	}
}
