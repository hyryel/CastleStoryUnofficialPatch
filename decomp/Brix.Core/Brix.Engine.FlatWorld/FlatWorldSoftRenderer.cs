using System;
using Brix.Engine.FlatWorld.Layers;
using Brix.Engine.FlatWorld.SoftAlgos;
using Brix.Engine.Geometrie;
using Brix.Utils;
using Brix.Utils.Logging;

namespace Brix.Engine.FlatWorld;

public class FlatWorldSoftRenderer : Poolable
{
	private static BrixLogger logger = new BrixLogger();

	public FlatWorldColors WorldColors = new FlatWorldColors();

	public IFlatWorldColors TempWorld;

	public IFlatWorldColors World;

	protected Brix.Engine.FlatWorld.Layers.Layers Layers;

	protected Common Common;

	public XYZ Start;

	protected XYZ PaddedStart;

	protected XYZ Border;

	protected XYZ MvSize;

	protected int Width;

	protected int Height;

	protected int Lod;

	protected bool UseColliders;

	protected bool UseTangents;

	public LayersApplyingSession LayerSession = new LayersApplyingSession();

	public MeshPool MeshPool = new MeshPool();

	public MeshAugmentablePrealloue MeshInfo;

	public MegaVoxelGeo MegaVoxelGeo;

	public MeshAugmentableSimple ColliderMeshInfo = new MeshAugmentableSimple();

	private readonly LevelConfigNormalUvs _levelConfigNormalUvs = new LevelConfigNormalUvs();

	private CubeDrawer _cubeDrawer = new CubeDrawer();

	public void Init(IFlatWorldColors world, Brix.Engine.FlatWorld.Layers.Layers layers, Common common, XYZ start, XYZ paddedStart, XYZ border, XYZ mvSize, int width, int height, int lod, bool useColliders, bool useTangents)
	{
		World = world;
		Layers = layers;
		Common = common;
		Start = start;
		PaddedStart = paddedStart;
		Border = border;
		MvSize = mvSize;
		Width = width;
		Height = height;
		Lod = lod;
		UseColliders = useColliders;
		UseTangents = useTangents;
	}

	public void PushWorldToColors()
	{
		TempWorld = World;
		World.CopyTo(WorldColors);
		World = WorldColors;
	}

	public void PopWorldColors()
	{
		World = TempWorld;
		World.CopyFrom(WorldColors);
		TempWorld = null;
	}

	public void InitFullFromLayers()
	{
		InitFromLayers();
		InitPreRender();
		InitRender();
		AllocateMesh();
		PopulateMesh();
	}

	public void InitFromLayers()
	{
		try
		{
			World.InitFromLayers(LayerSession, Layers, PaddedStart, Width, Height, 0, MvSize.y + Border.y * 2);
		}
		catch (Exception exception)
		{
			logger.Error(exception, "InitFromLayers");
		}
	}

	public void InitPreRender()
	{
		try
		{
			World.InitPreRender(Common, PaddedStart, Lod, _levelConfigNormalUvs);
		}
		catch (Exception exception)
		{
			logger.Error(exception, "InitPreRender");
		}
	}

	public void InitRender()
	{
		try
		{
			World.InitRender(Common, PaddedStart, _levelConfigNormalUvs);
		}
		catch (Exception exception)
		{
			logger.Error(exception, "InitRender");
		}
	}

	public void AllocateMesh()
	{
		try
		{
			int vertexCount = 0;
			int triangleCount = 0;
			TestWorldConfig3.CountLocalizedMesh(World, Border, MvSize, ref vertexCount, ref triangleCount);
			MeshInfo = MeshPool.GetPooledMesh(vertexCount, triangleCount, UseTangents);
			MeshInfo.Clear(realloc: false);
			ColliderMeshInfo.Clear();
		}
		catch (Exception exception)
		{
			logger.Error(exception, "AllocateMesh");
		}
	}

	public void PopulateMesh()
	{
		try
		{
			_cubeDrawer.SetUvDecals(Common.atlas.uvInnerDecal, Common.atlas.uvOuterDecal, Common.atlas.uvAtlasWidth / 16, Common.atlas.uvAtlasWidth / 4);
			TestWorldConfig3.CreateLocalizedMesh(World, _cubeDrawer, Border, MvSize, MeshInfo);
			if (UseColliders)
			{
				TestWorldConfig3.CreateLocalizedCollider(World, _cubeDrawer, Border, MvSize, ColliderMeshInfo);
			}
		}
		catch (Exception exception)
		{
			logger.Error(exception, "PopulateMesh");
		}
	}
}
