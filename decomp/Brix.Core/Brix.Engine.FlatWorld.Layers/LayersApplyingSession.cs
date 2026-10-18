using System.Collections.Generic;
using Brix.Engine.FlatWorld.SoftAlgos;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

public class LayersApplyingSession : Poolable
{
	public List<Layer> layers;

	public XYZ start;

	public int width;

	public int height;

	public int yMin;

	public int yMax;

	public IFlatWorldColors world;

	protected TexColors DataColors = new TexColors();

	private Columns _columns = new Columns();

	public void Filling()
	{
		Soft.Clear(world.Filling, width, height, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		for (int i = 0; i < layers.Count; i++)
		{
			layers[i].Filling(world.Filling, world.Filling, start, new XYZ(width, yMax - yMin, height));
		}
	}

	public void InitColumnAndOffset()
	{
		world.InitColumnAndOffset(_columns, yMin, yMax);
	}

	public void InitialData()
	{
		Soft.Clear(DataColors, world.Offset.Width, world.Offset.Height);
		for (int i = 0; i < layers.Count; i++)
		{
			layers[i].InitialData(DataColors, world.Offset, DataColors, start, world.CountOffsets);
		}
	}

	public void ProcessedData()
	{
		for (int i = 0; i < layers.Count; i++)
		{
			layers[i].ProcessedData(DataColors, world.Offset, DataColors, start, world.CountOffsets);
		}
	}

	public void CopyDataToWorld()
	{
		CopyDataToWorld_Soft(DataColors, world);
	}

	private static void CopyDataToWorld_Shader(Tex dataTex, FlatWorld world)
	{
		FlatWorld.RemapTex(world.typeTex, null, dataTex, new Vector4(1f, 0f, 0f, 0f), default(Vector4), default(Vector4), default(Vector4));
		FlatWorld.RemapTex(world.volumeTex, null, dataTex, new Vector4(0f, 1f, 0f, 0f), default(Vector4), default(Vector4), default(Vector4));
		FlatWorld.RemapTex(world.depthTex, null, dataTex, new Vector4(0f, 0f, 1f, 0f), default(Vector4), default(Vector4), default(Vector4));
	}

	private static void CopyDataToWorld_Soft(ITexColors dataTex, IFlatWorldColors world)
	{
		Color32[] pixels = dataTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(world.Type, dataTex, pixels);
		Color32[] orAllocPixels2 = Soft.GetOrAllocPixels32(world.Volume, dataTex, pixels);
		Color32[] orAllocPixels3 = Soft.GetOrAllocPixels32(world.Depth, dataTex, pixels);
		for (int i = 0; i < world.CountOffsets; i++)
		{
			Color32 color = pixels[i];
			orAllocPixels[i] = new Color32(color.r, 0, 0, 0);
			orAllocPixels2[i] = new Color32(color.g, 0, 0, 0);
			orAllocPixels3[i] = new Color32(color.b, 0, 0, 0);
		}
		world.Type.SetPixels32(orAllocPixels, dataTex.Width, dataTex.Height);
		world.Volume.SetPixels32(orAllocPixels2, dataTex.Width, dataTex.Height);
		world.Depth.SetPixels32(orAllocPixels3, dataTex.Width, dataTex.Height);
	}

	public void Init(IFlatWorldColors world1, Layers layers1, XYZ resultPosition = default(XYZ), int resultWidth = 0, int resultHeight = 0, int yMin = 0, int yMax = 255)
	{
		if (resultWidth == 0)
		{
			resultWidth = 1024;
		}
		if (resultHeight == 0)
		{
			resultHeight = 1024;
		}
		layers = layers1.layers;
		world = world1;
		start = resultPosition;
		width = resultWidth;
		height = resultHeight;
		this.yMin = yMin;
		this.yMax = yMax;
		DataColors = new TexColors();
	}
}
