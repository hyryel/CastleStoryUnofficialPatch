using Brix.Engine.FlatWorld.Layers;
using Brix.Engine.FlatWorld.SoftAlgos;
using Brix.Util;

namespace Brix.Engine.FlatWorld;

public static class FlatWorldExtender
{
	public static void InitFromLayers(this IFlatWorldColors world, LayersApplyingSession session, Brix.Engine.FlatWorld.Layers.Layers layers, XYZ paddedStart, int width, int height, int yMin, int yMax)
	{
		session.Init(world, layers, paddedStart, width, height, yMin, yMax);
		session.Filling();
		session.InitColumnAndOffset();
		session.InitialData();
		session.ProcessedData();
		session.CopyDataToWorld();
	}

	public static void InitColumnAndOffset(this IFlatWorldColors world, Columns columns, int yMin, int yMax)
	{
		columns.Process(world.Column, world.Filling, yMin, yMax);
		world.CountOffsets = Soft.XZ(world.Xz, world.Column);
		int size = 1 << Calcule.GetBestSquarePower(world.CountOffsets);
		Soft.Offset(world.Offset, world.Xz, world.Column, size);
	}

	public static void InitPreRender(this IFlatWorldColors world, Common common, XYZ paddedStart, int lod, LevelConfigNormalUvs levelConfigNormalUvs)
	{
		Soft.Smooths(world.Smooth, world.Type, world.Volume, world.Offset, lod, world.CountOffsets);
		levelConfigNormalUvs.LevelConfig(world.Level, world.Config, world.Column, world.Xz, world.Offset, world.Volume, world.Smooth, world.CountOffsets);
	}

	public static void InitRender(this IFlatWorldColors world, Common common, XYZ paddedStart, LevelConfigNormalUvs levelConfigNormalUvs)
	{
		levelConfigNormalUvs.NormalUvs(world.Normal, world.UvBase, world.UvTrans, world.UvSpecial, world.Column, world.Xz, world.Offset, world.Type, world.Volume, world.Level, common.GetProperlySizedNoiseTexColors(world.Type.Width), paddedStart, world.CountOffsets);
	}
}
