using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Brix.Engine.Abstract;
using Brix.Engine.FlatWorld.Layers;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class FlatWorld : IFlatWorldColors
{
	public Tex fillingTex = new Tex();

	public Tex columnTex = new Tex();

	public Tex xzTex = new Tex();

	public Tex offsetTex = new Tex();

	public Tex typeTex = new Tex();

	public Tex volumeTex = new Tex();

	public Tex depthTex = new Tex();

	public Tex smoothTex = new Tex();

	public Tex levelTex = new Tex();

	public Tex branchingTex = new Tex();

	public Tex normalTex = new Tex();

	public Tex typeLookupTex = new Tex();

	public Tex typeTransTex = new Tex();

	public Tex uvBaseTex = new Tex();

	public Tex uvTransTex = new Tex();

	public Tex uvSpecialTex = new Tex();

	public Tex configTex = new Tex();

	public Tex columnCountTex = new Tex();

	public Tex A = new Tex();

	public Tex B = new Tex();

	public Tex C = new Tex();

	public Tex D = new Tex();

	public Tex E = new Tex();

	public Tex F = new Tex();

	public Tex U = new Tex();

	public Tex V = new Tex();

	public Tex W = new Tex();

	public Tex Z = new Tex();

	public int gridI = -1;

	public int gridJ = -1;

	public int countOffsets;

	private static readonly FlatWorldThreadPool _threadPool = new FlatWorldThreadPool(20);

	private static readonly LayersApplyingSession LayersApplyingSession = new LayersApplyingSession();

	public static readonly Color32 EmptyType = default(Color32);

	public static readonly Color32 EmptyVolume = default(Color32);

	public static readonly Color32 EmptyDepth = default(Color32);

	public static readonly Color32 InternalType = new Color32(byte.MaxValue, 0, 0, 0);

	public static readonly Color32 InternalVolume = new Color32(byte.MaxValue, 0, 0, 0);

	public static readonly Color32 InternalDepth = new Color32(byte.MaxValue, 0, 0, 0);

	private static readonly int[] BranchingDx = new int[4] { 0, 0, 1, -1 };

	private static readonly int[] BranchingDz = new int[4] { 1, -1, 0, 0 };

	private static readonly Vector4[] BranchingLevelMasks = new Vector4[4]
	{
		new Vector4(2f, 3f, -1f, -1f),
		new Vector4(-1f, -1f, 0f, 1f),
		new Vector4(-1f, 0f, -1f, 2f),
		new Vector4(1f, -1f, 3f, -1f)
	};

	private static readonly int[] ConfigDx = new int[6] { 0, 0, 0, 1, -1, 0 };

	private static readonly int[] ConfigDy = new int[6] { 1, 0, 0, 0, 0, -1 };

	private static readonly int[] ConfigDz = new int[6] { 0, 1, -1, 0, 0, 0 };

	private static Stack<float> timers = new Stack<float>();

	public Tex[] Texes => new Tex[28]
	{
		fillingTex, columnTex, xzTex, offsetTex, typeTex, volumeTex, depthTex, smoothTex, levelTex, branchingTex,
		normalTex, typeLookupTex, typeTransTex, uvBaseTex, uvTransTex, uvSpecialTex, configTex, columnCountTex, A, B,
		C, D, E, F, U, V, W, Z
	};

	ITexColors IFlatWorldColors.Filling => fillingTex;

	ITexColors IFlatWorldColors.Column => columnTex;

	ITexColors IFlatWorldColors.Xz => xzTex;

	ITexColors IFlatWorldColors.Offset => offsetTex;

	ITexColors IFlatWorldColors.Type => typeTex;

	ITexColors IFlatWorldColors.Volume => volumeTex;

	ITexColors IFlatWorldColors.Depth => depthTex;

	ITexColors IFlatWorldColors.Smooth => smoothTex;

	ITexColors IFlatWorldColors.Level => levelTex;

	ITexColors IFlatWorldColors.Config => configTex;

	ITexColors IFlatWorldColors.Normal => normalTex;

	ITexColors IFlatWorldColors.UvBase => uvBaseTex;

	ITexColors IFlatWorldColors.UvTrans => uvTransTex;

	ITexColors IFlatWorldColors.UvSpecial => uvSpecialTex;

	int IFlatWorldColors.CountOffsets
	{
		get
		{
			return countOffsets;
		}
		set
		{
			countOffsets = value;
		}
	}

	public void Destroy()
	{
		Tex.Destroy(Texes);
	}

	public void Destroy(IWorker worker)
	{
		Worker.AddWork(worker, delegate(Tex[] texes)
		{
			Tex.Destroy(texes);
		}, Texes);
	}

	public void CacheIn2D(IWorker worker = null, bool immediate = true)
	{
		Worker.AddWork(worker, Neo.EnumAll(_CacheIn2D(immediate)));
	}

	public void FreeVRAM(IWorker worker = null)
	{
		Worker.AddWork(worker, delegate(Tex[] texes)
		{
			for (int i = 0; i < texes.Length; i++)
			{
				texes[i]?.FreeVRAM(cacheIn2D: false);
			}
		}, Texes);
	}

	public void Free2D(IWorker worker = null)
	{
		Worker.AddWork(worker, delegate(Tex[] texes)
		{
			for (int i = 0; i < texes.Length; i++)
			{
				texes[i]?.Free2D(cacheInRenderTexture: false);
			}
		}, Texes);
	}

	private IEnumerator _CacheIn2D(bool immediate = true)
	{
		Tex[] texes = Texes;
		foreach (Tex tex in texes)
		{
			if (tex != null)
			{
				yield return tex.Clean2D_Coroutine(immediate);
			}
		}
	}

	public void Save2D(string path)
	{
		Save2D(path, "Filling", fillingTex);
		Save2D(path, "Column", columnTex);
		Save2D(path, "XZ", xzTex);
		Save2D(path, "Offset", offsetTex);
		Save2D(path, "Type", typeTex);
		Save2D(path, "Volume", volumeTex);
		Save2D(path, "Depth", depthTex);
	}

	public void Save2D(string path, string name, Tex tex)
	{
		if (!tex.empty)
		{
			byte[] bytes = tex.EncodeToPNG();
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			File.WriteAllBytes(path + "/" + name + ".png", bytes);
		}
	}

	public void Load2D(string path)
	{
		Load2D(path, "Filling", fillingTex);
		Load2D(path, "Column", columnTex);
		Load2D(path, "XZ", xzTex);
		Load2D(path, "Offset", offsetTex);
		Load2D(path, "Type", typeTex);
		Load2D(path, "Volume", volumeTex);
		Load2D(path, "Depth", depthTex);
	}

	public void Load2D(string path, string name, Tex tex)
	{
		byte[] bytes = File.ReadAllBytes(path + "/" + name + ".png");
		tex.LoadPNG(bytes);
	}

	public void Clear(int width = 0, int height = 0)
	{
		if (width == 0)
		{
			width = fillingTex.width;
		}
		if (height == 0)
		{
			height = fillingTex.height;
		}
		ClearTex(fillingTex, null, default(Color32), width, height);
		ClearTex(columnTex, fillingTex);
		ClearTex(xzTex, columnTex);
		ClearTex(offsetTex, null, default(Color32), 8, 8);
		ClearTex(typeTex, offsetTex);
		ClearTex(volumeTex, offsetTex);
		ClearTex(depthTex, offsetTex);
		ClearTex(smoothTex, offsetTex);
		countOffsets = 0;
	}

	public void SwapPreRender(FlatWorld r)
	{
		Swap(fillingTex, r.fillingTex);
		Swap(columnTex, r.columnTex);
		Swap(xzTex, r.xzTex);
		Swap(offsetTex, r.offsetTex);
		Swap(typeTex, r.typeTex);
		Swap(volumeTex, r.volumeTex);
		Swap(depthTex, r.depthTex);
		Donnees.Swap(ref countOffsets, ref r.countOffsets);
	}

	public void SwapRender(FlatWorld r)
	{
		Swap(smoothTex, r.smoothTex);
		Swap(levelTex, r.levelTex);
		Swap(branchingTex, r.branchingTex);
		Swap(normalTex, r.normalTex);
		Swap(typeLookupTex, r.typeLookupTex);
		Swap(typeTransTex, r.typeTransTex);
		Swap(uvBaseTex, r.uvBaseTex);
		Swap(uvTransTex, r.uvTransTex);
		Swap(uvSpecialTex, r.uvSpecialTex);
		Swap(configTex, r.configTex);
		Swap(columnCountTex, r.columnCountTex);
	}

	public void SwapPostRender(FlatWorld r)
	{
		SwapPreRender(r);
		Swap(levelTex, r.levelTex);
		Swap(configTex, r.configTex);
	}

	private void Filling(Tex worldTex, bool soft)
	{
		if (soft)
		{
			Soft.Filling(fillingTex, worldTex);
		}
		else
		{
			Shaders.Filling(fillingTex, worldTex);
		}
	}

	public void Columns(bool soft)
	{
		if (soft)
		{
			Soft.Columns(columnTex, fillingTex, 0, 255);
		}
		else
		{
			Shaders.Columns(columnTex, fillingTex);
		}
	}

	private int XZ(bool soft)
	{
		if (soft)
		{
			return Soft.XZ(xzTex, columnTex);
		}
		GetXZTex(xzTex, U, columnTex);
		return Soft.GetOffsetCount(xzTex);
	}

	private void Offset(int size, bool soft)
	{
		if (soft)
		{
			Soft.Offset(offsetTex, xzTex, columnTex, size);
		}
		else
		{
			GetOffsetTex(offsetTex, A, xzTex, columnTex, size);
		}
	}

	public void XZAndOffset(int soft)
	{
		countOffsets = XZ(soft.Contains(Step.Sum));
		Offset(1 << Calcule.GetBestSquarePower(countOffsets), soft.Contains(Step.Offset));
	}

	public void FullColumns(bool soft)
	{
		if (soft)
		{
			Soft.Remap(columnTex, null, fillingTex, new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0.5f, 0.5f, 0f, 0f), new Vector4(0.5f, 0.5f, 0f, 0f));
		}
		else
		{
			Shaders.Remap(columnTex, null, fillingTex, new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0.5f, 0.5f, 0f, 0f), new Vector4(0.5f, 0.5f, 0f, 0f));
		}
	}

	private void Smooths(Texture typeAtlasTex, int lod, bool soft)
	{
		if (soft)
		{
			Soft.Smooths(smoothTex, typeTex, volumeTex, offsetTex, lod, countOffsets);
		}
		else
		{
			GetSmoothTex(smoothTex, typeTex, depthTex, typeAtlasTex, lod);
		}
	}

	public void InitLevelTex()
	{
		GetTopBottomTex(C, A, B, offsetTex, xzTex, columnTex, volumeTex);
		Color32 color = new Color32(33, 0, 0, 0);
		Shaders.AddIfBetween(E, smoothTex, C, color, color, -Vector4.one);
		GetLevelTex(levelTex, A, B, C, D, offsetTex, xzTex, columnTex, volumeTex, E);
	}

	public void InitBranchingTex()
	{
		GetTopBottomTex(C, A, B, offsetTex, xzTex, columnTex, volumeTex);
		GetBranchingTex(branchingTex, A, B, offsetTex, xzTex, columnTex, volumeTex, levelTex, C);
	}

	public void InitTypeTransitionTex(Texture typeAtlasTex)
	{
		GetTypeTransitionTex(typeTransTex, A, B, offsetTex, xzTex, columnTex, typeTex, branchingTex, typeAtlasTex);
	}

	public void InitUvTex(Texture noiseTex, Texture typeAtlasTex, XYZ worldStart)
	{
		GetUvBaseTex(uvBaseTex, typeTex, offsetTex, xzTex, noiseTex, typeAtlasTex, worldStart);
		GetUvTransitionTex(uvTransTex, A, B, typeTex, typeTransTex, offsetTex, xzTex, noiseTex, typeAtlasTex);
		GetUvSpecialTex(uvSpecialTex, typeTex, uvBaseTex, typeAtlasTex);
	}

	public void InitConfigTex()
	{
		InitBranchingTex();
		GetConfigTex(configTex, A, B, offsetTex, xzTex, columnTex, volumeTex, branchingTex, levelTex);
	}

	public void InitNormalTex()
	{
		GetAverageNormalTex(normalTex, A, B, C, D, E, offsetTex, xzTex, columnTex, levelTex, configTex, volumeTex);
	}

	public void InitTypeLookupTex(Texture permTex, Texture typeSampleTex, Texture warpTex, XYZ worldStart)
	{
		GetTypeLookupTex(typeLookupTex, A, B, offsetTex, typeTex, permTex, typeSampleTex, warpTex, worldStart);
	}

	public void InitColumnCountTex()
	{
		GetColumnCountTex(columnCountTex, A, columnTex, Vector3.zero, new Vector3(columnTex.width, 256f, columnTex.height), 4);
	}

	public void InitSoftLevelConfig()
	{
		Soft.LevelConfig(levelTex, configTex, columnTex, xzTex, offsetTex, volumeTex, smoothTex, countOffsets);
	}

	public void InitSoftNormalUv(Tex noiseTex, XYZ offsetStart)
	{
		Soft.NormalUvs(normalTex, uvBaseTex, uvTransTex, uvSpecialTex, columnTex, xzTex, offsetTex, typeTex, volumeTex, levelTex, noiseTex, offsetStart, countOffsets);
	}

	private void MergeColumns(FlatWorld sup, bool soft)
	{
		Tex tex = sup.columnTex;
		if (soft)
		{
			Soft.MergeColumns(fillingTex, tex);
		}
		else
		{
			Shaders.MergeColumns(fillingTex, tex);
		}
	}

	private void MergeTypesVolumesDepths(FlatWorld sup, bool soft)
	{
		if (soft)
		{
			Soft.MergeTypesVolumesDepths(typeTex, volumeTex, depthTex, offsetTex, sup.xzTex, sup.columnTex, sup.typeTex, sup.volumeTex);
			return;
		}
		GetTypeLodTex(typeTex, A, B, C, D, offsetTex, sup.xzTex, sup.columnTex, sup.typeTex, sup.configTex);
		GetVolumeLodTex(volumeTex, A, B, offsetTex, sup.xzTex, sup.columnTex, sup.volumeTex);
		GetDepthLodTex(depthTex, A, sup.typeTex);
	}

	public void InitWorldHeightmap(Tex A, Tex worldFullTex, Vector2 outputStart, Vector2 outputSize, Common common, int soft = 0, bool force = false)
	{
		if (soft.Contains(Step.Section))
		{
			Soft.Retarget(A, worldFullTex, outputStart, outputSize);
		}
		else
		{
			Shaders.Retarget(A, worldFullTex, outputStart, outputSize);
		}
		InitWorldHeightmap(A, common, soft, force);
	}

	public void InitWorldHeightmap(Tex worldTex, Common common, int soft = 0, bool force = false)
	{
		if (force || fillingTex.empty || columnTex.empty)
		{
			Filling(worldTex, soft.Contains(Step.Column));
			Columns(soft.Contains(Step.Column));
			force = true;
		}
		if (force || xzTex.empty || offsetTex.empty)
		{
			XZAndOffset(soft);
			force = true;
		}
		if (force || typeTex.empty || volumeTex.empty || depthTex.empty)
		{
			if (soft.Contains(Step.Type) || soft.Contains(Step.Volume) || soft.Contains(Step.Depth))
			{
				Soft.TypesVolumesDepths(typeTex, volumeTex, depthTex, offsetTex, worldTex, countOffsets);
			}
			if (!soft.Contains(Step.Type))
			{
				GetTypeTex(typeTex, offsetTex, worldTex);
			}
			if (!soft.Contains(Step.Volume))
			{
				GetVolumeTex(volumeTex, offsetTex, worldTex);
			}
			if (!soft.Contains(Step.Depth))
			{
				GetDepthTex(depthTex, offsetTex, worldTex);
			}
		}
	}

	public static void InitFromLayers(List<FlatWorld> worlds, Brix.Engine.FlatWorld.Layers.Layers layers, Common common, List<XYZ> starts, List<XYZ> paddedStarts, XYZ border, XYZ mvSize, int width = 0, int height = 0, int lod = 0)
	{
		int num;
		for (int i = 0; i < worlds.Count; i += num)
		{
			num = Mathf.Min(worlds.Count - i, _threadPool.Count);
			for (int j = 0; j < num; j++)
			{
				_threadPool[j].Renderer.Init(worlds[i + j], layers, common, starts[i + j], paddedStarts[i + j], border, mvSize, width, height, lod, useColliders: false, useTangents: false);
				_threadPool[j].Renderer.PushWorldToColors();
			}
			for (int k = 0; k < num; k++)
			{
				_threadPool[k].Thread = new Thread(_threadPool[k].InitFromLayersAction);
				_threadPool[k].Thread.Start();
			}
			for (int l = 0; l < num; l++)
			{
				_threadPool[l].Thread.Join();
				_threadPool[l].Thread = null;
			}
			for (int m = 0; m < num; m++)
			{
				_threadPool[m].Renderer.PopWorldColors();
			}
		}
	}

	public void InitFromLayers(Brix.Engine.FlatWorld.Layers.Layers layers, Common common, XYZ start = default(XYZ), int width = 0, int height = 0, int yMin = 0, int yMax = 255)
	{
		LayersApplyingSession layersApplyingSession = LayersApplyingSession;
		layersApplyingSession.Init(this, layers, start, width, height, yMin, yMax);
		layersApplyingSession.Filling();
		layersApplyingSession.InitColumnAndOffset();
		layersApplyingSession.InitialData();
		layersApplyingSession.ProcessedData();
		layersApplyingSession.CopyDataToWorld();
	}

	public static void InitPreRender(List<FlatWorld> worlds, Brix.Engine.FlatWorld.Layers.Layers layers, Common common, List<XYZ> starts, List<XYZ> paddedStarts, XYZ border, XYZ mvSize, int width, int height, int lod = 0)
	{
		int num = 0;
		while (num < worlds.Count)
		{
			int num2 = Mathf.Min(worlds.Count - num, _threadPool.Count);
			for (int i = 0; i < num2; i++)
			{
				_threadPool[i].Renderer.Init(worlds[num], layers, common, starts[num], paddedStarts[num], border, mvSize, width, height, lod, useColliders: false, useTangents: false);
				_threadPool[i].Renderer.PushWorldToColors();
				num++;
			}
			for (int j = 0; j < num2; j++)
			{
				_threadPool[j].Thread = new Thread(_threadPool[j].InitPreRenderAction);
				_threadPool[j].Thread.Start();
			}
			for (int k = 0; k < num2; k++)
			{
				_threadPool[k].Thread.Join();
				_threadPool[k].Thread = null;
			}
			for (int l = 0; l < num2; l++)
			{
				_threadPool[l].Renderer.PopWorldColors();
			}
		}
	}

	public void InitPreRender(Common common, XYZ offsetStart, int lod = 0, int soft = 0, bool force = false)
	{
		if (force || smoothTex.empty)
		{
			Smooths(common.typeAtlasTex, lod, soft.Contains(Step.Smooth));
			force = true;
		}
		if (force || levelTex.empty || configTex.empty)
		{
			if (soft.Contains(Step.Level) || soft.Contains(Step.Config))
			{
				InitSoftLevelConfig();
			}
			if (!soft.Contains(Step.Level))
			{
				InitLevelTex();
			}
			if (!soft.Contains(Step.Config))
			{
				InitConfigTex();
			}
		}
	}

	public static void InitRender(List<FlatWorld> worlds, Brix.Engine.FlatWorld.Layers.Layers layers, Common common, List<XYZ> starts, List<XYZ> paddedStarts, XYZ border, XYZ mvSize, int width, int height, int lod = 0)
	{
		int num = 0;
		while (num < worlds.Count)
		{
			int num2 = Mathf.Min(worlds.Count - num, _threadPool.Count);
			for (int i = 0; i < num2; i++)
			{
				_threadPool[i].Renderer.Init(worlds[num], layers, common, starts[num], paddedStarts[num], border, mvSize, width, height, lod, useColliders: false, useTangents: false);
				_threadPool[i].Renderer.PushWorldToColors();
				num++;
			}
			for (int j = 0; j < num2; j++)
			{
				_threadPool[j].Thread = new Thread(_threadPool[j].InitRenderAction);
				_threadPool[j].Thread.Start();
			}
			for (int k = 0; k < num2; k++)
			{
				_threadPool[k].Thread.Join();
				_threadPool[k].Thread = null;
			}
			for (int l = 0; l < num2; l++)
			{
				_threadPool[l].Renderer.PopWorldColors();
			}
		}
	}

	public void InitRender(Common common, XYZ offsetStart, int lod = 0, int soft = 0, bool force = false)
	{
		if (force || normalTex.empty || uvBaseTex.empty || uvTransTex.empty || uvSpecialTex.empty)
		{
			if (soft.Contains(Step.Normal) || soft.Contains(Step.Uv))
			{
				InitSoftNormalUv(common.GetProperlySizedNoiseTex(typeTex.width), offsetStart);
			}
			if (!soft.Contains(Step.Normal) || !soft.Contains(Step.Uv))
			{
				InitBranchingTex();
			}
			if (!soft.Contains(Step.Normal))
			{
				InitNormalTex();
			}
			if (!soft.Contains(Step.Uv))
			{
				InitTypeTransitionTex(common.typeAtlasTex);
			}
			if (!soft.Contains(Step.Uv))
			{
				InitUvTex(common.GetProperlySizedNoiseTex(typeTex.width), common.typeAtlasTex, offsetStart);
			}
		}
	}

	public void InitLod(FlatWorld sup, Common common, int soft, XYZ worldStart)
	{
		MergeColumns(sup, soft.Contains(Step.Merge));
		Columns(soft.Contains(Step.Column));
		XZAndOffset(soft);
		MergeTypesVolumesDepths(sup, soft.Contains(Step.Merge));
	}

	public IEnumerable<int> GetVisibleVoxelOffsets()
	{
		return Soft.GetNonEmptyOffsets(configTex);
	}

	public IEnumerable<int> GetVisibleVoxelOffsets(XYZ start, XYZ size)
	{
		return from kv in Soft.GetNonEmptyOffsets(configTex, xzTex, columnTex, start, size)
			select kv.Key;
	}

	public IEnumerable<KeyValuePair<int, XYZ>> GetVisibleVoxels()
	{
		return Soft.GetNonEmptyOffsets(configTex, xzTex, columnTex);
	}

	public IEnumerable<KeyValuePair<int, XYZ>> GetVisibleVoxels(XYZ start, XYZ size)
	{
		return Soft.GetNonEmptyOffsets(configTex, xzTex, columnTex, start, size);
	}

	public IEnumerable<KeyValuePair<int, XYZ>> GetSurfaceVoxels()
	{
		return Soft.GetSurfaceOffsets(configTex, xzTex, columnTex);
	}

	public IEnumerable<KeyValuePair<int, XYZ>> GetSurfaceVoxels(XYZ start, XYZ size)
	{
		return Soft.GetSurfaceOffsets(configTex, xzTex, columnTex, start, size);
	}

	public XYZ GetVoxelPosition(int offset)
	{
		return Soft.DecodeXYZ(Soft.DecodeInt(offsetTex.GetPixel(offset % offsetTex.width, offset / offsetTex.width)));
	}

	public IEnumerable<XYZ> GetVoxelPositions(IEnumerable<int> offsets)
	{
		Color32[] offsetColors = offsetTex.GetPixels32();
		return offsets.Select((int i) => Soft.DecodeXYZ(Soft.DecodeInt(offsetColors[i])));
	}

	public void Heightmap(Tex resultTex)
	{
		Soft.GetHeightmap(resultTex, fillingTex, xzTex, columnTex, typeTex, volumeTex);
	}

	private static void Stitch(Tex resultTex, Tex tex1, Tex tex2, Tex tex3, Tex tex4, int border, int inputSize, bool soft)
	{
		if (soft)
		{
			Soft.Stitch(resultTex, tex1, tex2, tex3, tex4, border, inputSize);
		}
		else
		{
			Shaders.Stitch(resultTex, tex1, tex2, tex3, tex4, border, inputSize);
		}
	}

	private static void StitchData(Tex resultTex, Tex offsetTex, Tex xzStitchedTex, Tex columnStitchedTex, Tex tex1, Tex tex2, Tex tex3, Tex tex4, Color32 emptyColor, Color32 internalColor, bool soft)
	{
		if (soft)
		{
			Soft.StitchData(resultTex, offsetTex, xzStitchedTex, columnStitchedTex, tex1, tex2, tex3, tex4, emptyColor, internalColor);
		}
		else
		{
			Shaders.StitchData(resultTex, offsetTex, xzStitchedTex, columnStitchedTex, tex1, tex2, tex3, tex4, emptyColor, internalColor);
		}
	}

	public void Stitch2x2(FlatWorld w1, FlatWorld w2, FlatWorld w3, FlatWorld w4, int border, int inputSize, int soft = 0)
	{
		bool soft2 = soft.Contains(Step.Section);
		bool soft3 = soft.Contains(Step.Stitch);
		Stitch(fillingTex, w1.fillingTex, w2.fillingTex, w3.fillingTex, w4.fillingTex, border, inputSize, soft2);
		Columns(soft.Contains(Step.Column));
		XZAndOffset(soft);
		Stitch(U, w1.xzTex, w2.xzTex, w3.xzTex, w4.xzTex, border, inputSize, soft2);
		Stitch(V, w1.columnTex, w2.columnTex, w3.columnTex, w4.columnTex, border, inputSize, soft2);
		StitchData(typeTex, offsetTex, U, V, w1.typeTex, w2.typeTex, w3.typeTex, w4.typeTex, EmptyType, InternalType, soft3);
		StitchData(volumeTex, offsetTex, U, V, w1.volumeTex, w2.volumeTex, w3.volumeTex, w4.volumeTex, EmptyVolume, InternalVolume, soft3);
		StitchData(depthTex, offsetTex, U, V, w1.depthTex, w2.depthTex, w3.depthTex, w4.depthTex, EmptyDepth, InternalDepth, soft3);
		Tex.Destroy(smoothTex, levelTex, branchingTex, normalTex, typeLookupTex, typeTransTex, uvBaseTex, uvTransTex, uvSpecialTex, configTex);
		Tex.Destroy(A, B, C, D, E, F, U, V, W, Z);
	}

	public static void GetWorldTex(Tex worldTex, Texture loadedTex)
	{
		Shaders.World(worldTex, loadedTex);
	}

	public static void GetFillingTex(Tex fillingTex, Tex worldTex)
	{
		Shaders.Filling(fillingTex, worldTex);
	}

	public static void GetFillingLodTex(Tex resultTex, Tex supTex)
	{
		Shaders.MergeColumns(resultTex, supTex);
	}

	public static void GetColumnTex(Tex columnTex, Tex fillingTex)
	{
		Shaders.Columns(columnTex, fillingTex);
	}

	public static void GetXZTex(Tex xzTex, Tex U, Texture columnTex)
	{
		Shaders.XZInit(U, columnTex);
		for (int i = 0; 1 << i < columnTex.width; i++)
		{
			Shaders.XZSumCol(xzTex, U, 1 << i);
			Swap(U, xzTex);
		}
		for (int j = 0; 1 << j < columnTex.height; j++)
		{
			Shaders.XZSumRow(xzTex, U, 1 << j);
			Swap(U, xzTex);
		}
		Shaders.XZEnd(xzTex, U, columnTex);
	}

	public static void GetOffsetTex(Tex offsetTex, Tex A, Tex xzTex, Texture columnTex, int size = 0)
	{
		if (size == 0)
		{
			size = 1 << Calcule.GetBestSquarePower(Soft.GetOffsetCount(xzTex));
		}
		ClearTex(A, null, new Color32(0, 0, 0, 0), size, size);
		int num = xzTex.width * xzTex.height;
		for (int i = 0; 1 << i < num; i++)
		{
			Shaders.OffsetSearch(offsetTex, A, xzTex, 1 << i);
			Swap(A, offsetTex);
		}
		Shaders.OffsetXYZ(offsetTex, A, xzTex, columnTex);
	}

	public static int GetXZAndOffsetTex(Tex xzTex, Tex offsetTex, Tex A, Tex U, Tex columnTex)
	{
		GetXZTex(xzTex, U, columnTex);
		int offsetCount = Soft.GetOffsetCount(xzTex);
		GetOffsetTex(offsetTex, A, xzTex, columnTex, 1 << Calcule.GetBestSquarePower(offsetCount));
		return offsetCount;
	}

	public static void GetTypeTex(Tex typeTex, Texture offsetTex, Texture worldTex, Vector3 translation = default(Vector3))
	{
		Shaders.Types(typeTex, offsetTex, worldTex, translation, 255);
	}

	public static void GetVolumeTex(Tex volumeTex, Texture offsetTex, Texture worldTex, Vector3 translation = default(Vector3))
	{
		Shaders.Volumes(volumeTex, offsetTex, worldTex, translation);
	}

	public static void GetDepthTex(Tex depthTex, Texture offsetTex, Texture worldTex, Vector3 translation = default(Vector3))
	{
		Shaders.Depths(depthTex, offsetTex, worldTex, translation);
	}

	public static void GetTypeLookupTex(Tex resultTex, Tex A, Tex B, Texture offsetTex, Texture typeTex, Texture permTex, Texture typeSampleTex, Texture warpTex, XYZ worldStart)
	{
		Vector3 vector = Vector3.one * 0.231f;
		Vector3 vector2 = Vector3.Scale(worldStart.ToVector3(), vector);
		Vector3 translation = vector2 + new Vector3(1.23f, 2.48f, 3.91f);
		Shaders.FractalNoise(resultTex, offsetTex, permTex, vector2, vector);
		Swap(A, resultTex);
		RemapTex(resultTex, null, A, new Vector4(1f, 0f, 0f, 0f), Vector4.zero, Vector4.zero, Vector4.zero);
		Swap(A, resultTex);
		Shaders.FractalNoise(resultTex, offsetTex, permTex, translation, vector);
		Swap(B, resultTex);
		RemapTex(resultTex, A, B, Vector4.zero, new Vector4(1f, 0f, 0f, 0f), Vector4.zero, Vector4.zero, additive: true);
		Swap(A, resultTex);
		Shaders.WarpedNoiseLookup(resultTex, A, typeSampleTex, warpTex);
	}

	public static void GetProcessedTypeTex(Tex resultTex, Tex A, Tex B, Texture typeTex, Texture depthTex, Texture offsetTex, Texture typeLookupTex)
	{
		Shaders.TypesSurface(A, depthTex, 255);
		Shaders.CopyIfBetween(resultTex, typeTex, typeTex, A, InternalType, InternalType);
		Swap(B, resultTex);
		Shaders.CopyIfBetween(resultTex, B, typeTex, A, EmptyType, EmptyType);
		Swap(A, resultTex);
		Shaders.CopyIfBetween(resultTex, A, A, typeLookupTex, InternalType, InternalType);
	}

	public static void ProcessDepthTex(Tex depthTex, Tex A, Tex B, Tex C, Texture offsetTex, Texture xzTex, Texture columnTex)
	{
		TranslateTex(A, offsetTex, xzTex, columnTex, depthTex, Vector3.up, Vector3.one, EmptyDepth, InternalDepth);
		TranslateTex(B, offsetTex, xzTex, columnTex, depthTex, -Vector3.up, Vector3.one, EmptyDepth, InternalDepth);
		Swap(C, depthTex);
		Shaders.DepthFill(depthTex, A, B, C, default(Color32), InternalDepth);
	}

	public static void GetTypeSampleTex(Tex resultTex, Tex A, Texture typeSampleTex, Color[] colors, int[] types)
	{
		ClearTex(resultTex, typeSampleTex);
		for (int i = 0; i < colors.Length; i++)
		{
			Swap(A, resultTex);
			Shaders.TypeLookupInit(resultTex, A, typeSampleTex, colors[i], types[i]);
		}
		Swap(A, resultTex);
		RemapTex(resultTex, null, A, new Vector4(1f, 0f, 0f, 0f), Vector4.zero, Vector4.zero, Vector4.zero);
	}

	public static void GetSmoothTex(Tex resultTex, Texture typeProcessedTex, Texture depthTex, Texture typeAtlasTex, int lod = 0)
	{
		Shaders.Smooths(resultTex, typeProcessedTex, depthTex, typeAtlasTex, lod);
	}

	public static void GetSmoothTypeTex(Tex resultTex, Texture previousTex, Texture typeProcessedTex, int startType, int endType = 0)
	{
		if (endType == 0)
		{
			endType = startType;
		}
		Color32 lowColor = new Color32((byte)startType, 0, 0, 0);
		Color32 highColor = new Color32((byte)endType, 0, 0, 0);
		Shaders.AddIfBetween(resultTex, previousTex, typeProcessedTex, lowColor, highColor, new Vector4(1f, 0f, 0f, 0f));
	}

	public static void GetSmoothDepthTex(Tex resultTex, Tex A, Texture previousTex, Texture depthTex)
	{
		ClearTex(A, previousTex);
		Shaders.CopyIfBetween(highColor: new Color32(1, 0, 0, 0), resultTex: resultTex, previousTex: A, compareTex: depthTex, nextTex: previousTex, lowColor: default(Color32));
	}

	public static void GetTypeLodTex(Tex typeTex, Tex A, Tex B, Tex C, Tex D, Texture offsetTex, Texture xzSupTex, Texture columnSupTex, Texture typeSupTex, Texture configSupTex)
	{
		Shaders.AddIfBetween(typeTex, typeSupTex, configSupTex, default(Color32), default(Color32), new Vector4(1f, 0f, 0f, 0f));
		ClearTex(B, offsetTex);
		ClearTex(C, offsetTex);
		for (int i = 0; i < 8; i++)
		{
			TranslateTex(translation: new Vector3(i & 1, i >> 2, (i >> 1) & 1), resultTex: A, offsetTex: offsetTex, xzTex: xzSupTex, columnTex: columnSupTex, dataTex: typeTex, scale: Vector3.one * 2f, emptyColor: EmptyType, internalColor: InternalType);
			Vector4 vector = new Vector4(1f, 0f, 0f, 0f);
			int num = i / 2;
			CopyTex(D, B);
			RemapTex(B, D, A, vector * ((num == 0) ? 1 : 0), vector * ((num == 1) ? 1 : 0), vector * ((num == 2) ? 1 : 0), vector * ((num == 3) ? 1 : 0), additive: true);
			Swap(B, C);
		}
		Shaders.MergeTypes(typeTex, offsetTex, B, C);
	}

	public static void GetVolumeLodTex(Tex volumeTex, Tex A, Tex B, Texture offsetTex, Texture xzSupTex, Texture columnSupTex, Texture volumeSupTex)
	{
		Vector4 rMask = new Vector4(1f, 0f, 0f, 0f);
		ClearTex(A, offsetTex);
		for (int i = 0; i < 8; i++)
		{
			Vector3 translation = new Vector3(i & 1, i >> 2, (i >> 1) & 1);
			TranslateTex(volumeTex, offsetTex, xzSupTex, columnSupTex, volumeSupTex, translation, Vector3.one * 2f, EmptyVolume, InternalVolume);
			Swap(B, volumeTex);
			Shaders.AddIfBetween(volumeTex, B, B, InternalVolume, InternalVolume, new Vector4(-50f / 51f, 0.125f, 0f, 0f));
			RemapTex(B, A, volumeTex, rMask, new Vector4(0f, 1f, 0f, 0f), Vector4.zero, Vector4.zero, additive: true);
			Swap(A, B);
		}
		RemapTex(volumeTex, null, A, new Vector4(0.125f, 0f, 0f, 0f), Vector4.zero, Vector4.zero, Vector4.zero);
		Swap(B, volumeTex);
		Shaders.AddIfBetween(volumeTex, B, A, new Color32(0, byte.MaxValue, 0, 0), new Color32(byte.MaxValue, byte.MaxValue, 0, 0), new Vector4(1f, 0f, 0f, 0f));
		Swap(B, volumeTex);
		AddNonZeroTex(volumeTex, B, A, new Color32(1, 0, 0, 0), new Vector4(1f, 0.5f, 0f, 0f));
	}

	public static void GetDepthLodTex(Tex depthTex, Tex A, Texture typeTex)
	{
		ClearTex(A, typeTex);
		AddNonZeroTex(depthTex, A, typeTex, new Color32(1, 0, 0, 0), new Vector4(1f, 0f, 0f, 0f));
	}

	public static void GetLevelTex(Tex levelTex, Tex A, Tex B, Tex C, Tex E, Texture offsetTex, Texture xzTex, Texture columnTex, Texture volumeTex, Texture smoothTex)
	{
		Vector4 vector = new Vector4(1f, 0f, 0f, 0f);
		TranslateTex(A, offsetTex, xzTex, columnTex, volumeTex, Vector3.up, Vector3.one, EmptyVolume, InternalVolume);
		TranslateTex(B, offsetTex, xzTex, columnTex, volumeTex, -Vector3.up, Vector3.one, EmptyVolume, InternalVolume);
		RemapTex(C, A, B, vector * 0f, vector, vector * 0f, vector * 0f, additive: true);
		AddNonZeroTex(E, volumeTex, C, InternalVolume, new Vector4(1f, 1f, 0f, 0f));
		ClearTex(levelTex, offsetTex);
		for (int i = 0; i < 4; i++)
		{
			ClearTex(C, offsetTex);
			Vector3 vector2 = new Vector3(i % 2, 0f, -(i / 2 % 2));
			for (int j = 0; j < 4; j++)
			{
				TranslateTex(translation: vector2 + new Vector3(j % 2 - 1, 0f, 1 - j / 2), resultTex: A, offsetTex: offsetTex, xzTex: xzTex, columnTex: columnTex, dataTex: E, scale: Vector3.one, emptyColor: EmptyVolume, internalColor: InternalVolume);
				RemapTex(B, C, A, vector * ((j == 0) ? 1 : 0), vector * ((j == 1) ? 1 : 0), vector * ((j == 2) ? 1 : 0), vector * ((j == 3) ? 1 : 0), additive: true);
				Swap(C, B);
			}
			Shaders.CrustLevel(A, C, volumeTex, smoothTex);
			RemapTex(B, levelTex, A, vector * ((i == 0) ? 1 : 0), vector * ((i == 1) ? 1 : 0), vector * ((i == 2) ? 1 : 0), vector * ((i == 3) ? 1 : 0), additive: true);
			Swap(levelTex, B);
		}
	}

	public static void GetTopBottomTex(Tex resultTex, Tex A, Tex B, Texture offsetTex, Texture xzTex, Texture columnTex, Texture dataTex)
	{
		Color32 emptyColor = default(Color32);
		Color32 internalColor = new Color32(byte.MaxValue, 0, 0, 0);
		ClearTex(A, offsetTex);
		TranslateTex(B, offsetTex, xzTex, columnTex, dataTex, Vector3.up, Vector3.one, emptyColor, internalColor);
		AddNonZeroTex(resultTex, A, B, new Vector4(0.003921569f, 0f, 0f, 0f));
		Swap(A, resultTex);
		TranslateTex(B, offsetTex, xzTex, columnTex, dataTex, -Vector3.up, Vector3.one, emptyColor, internalColor);
		AddNonZeroTex(resultTex, A, B, new Vector4(0.1254902f, 0f, 0f, 0f));
	}

	public static void GetBranchingHeightTex_Partial(Tex resultTex, Tex A, Tex B, Tex C, Texture offsetTex, Texture xzTex, Texture columnTex, Texture volumeTex, Vector3 translation, Vector4 m)
	{
		CopyTex(C, resultTex);
		TranslateTex(A, offsetTex, xzTex, columnTex, volumeTex, translation - Vector3.up, Vector3.one, default(Color32), default(Color32));
		Swap(B, resultTex);
		Shaders.CopyIfBetween(resultTex, C, A, B, default(Color32), default(Color32), Vector4.zero, m * (1f / 3f));
		TranslateTex(A, offsetTex, xzTex, columnTex, volumeTex, translation, Vector3.one, default(Color32), default(Color32));
		Swap(B, resultTex);
		Shaders.CopyIfBetween(resultTex, C, A, B, default(Color32), default(Color32), Vector4.zero, m * (2f / 3f));
		TranslateTex(A, offsetTex, xzTex, columnTex, volumeTex, translation + Vector3.up, Vector3.one, default(Color32), default(Color32));
		Swap(B, resultTex);
		Shaders.CopyIfBetween(resultTex, C, A, B, default(Color32), default(Color32), Vector4.zero, m * 1f);
	}

	public static void GetBranchingTex(Tex resultTex, Tex A, Tex B, Texture offsetTex, Texture xzTex, Texture columnTex, Texture volumeTex, Texture levelTex, Texture topBottomTex)
	{
		Vector4 vector = new Vector4(1f, 0f, 0f, 0f);
		ClearTex(resultTex, offsetTex);
		for (int i = 0; i < 4; i++)
		{
			Vector4 vector2 = new Vector4((i == 0) ? 1 : 0, (i == 1) ? 1 : 0, (i == 2) ? 1 : 0, (i == 3) ? 1 : 0);
			Vector3 translation = new Vector3(BranchingDx[i], 0f, BranchingDz[i]);
			Vector4 levelMask = BranchingLevelMasks[i];
			Shaders.BranchingLevel(A, offsetTex, xzTex, columnTex, volumeTex, levelTex, topBottomTex, translation, levelMask);
			Swap(B, resultTex);
			RemapTex(resultTex, B, A, vector * vector2.x, vector * vector2.y, vector * vector2.z, vector * vector2.w, additive: true);
		}
	}

	public static void GetNormalTex(Tex resultTex, Tex A, Texture levelTex, Texture configTex)
	{
		Shaders.Normals(resultTex, levelTex, configTex);
		Shaders.NormalEncode(A, resultTex);
		Vector4 vector = new Vector4(1f, 0f, 0f, 0f);
		Shaders.Remap(resultTex, null, A, vector, vector, vector, vector);
	}

	public static void GetAverageNormalTex(Tex resultTex, Tex A, Tex B, Tex C, Tex D, Tex E, Texture offsetTex, Texture xzTex, Texture columnTex, Texture levelTex, Texture configTex, Texture volumeTex)
	{
		Shaders.Normals(E, levelTex, configTex);
		int[] array = new int[4] { 0, 1, 0, 1 };
		int[] array2 = new int[4] { 0, 0, -1, -1 };
		int[] array3 = new int[4] { -1, 0, -1, 0 };
		int[] array4 = new int[4] { 1, 1, 0, 0 };
		Vector4 vector = new Vector4(1f, 0f, 0f, 0f);
		ClearTex(resultTex, offsetTex);
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector2 = new Vector3(array[i], 0f, array2[i]);
			Vector4 vector3 = new Vector4((i == 0) ? 1 : 0, (i == 1) ? 1 : 0, (i == 2) ? 1 : 0, (i == 3) ? 1 : 0);
			ClearTex(D, offsetTex);
			for (int j = 0; j < 4; j++)
			{
				Vector3 vector4 = new Vector3(array3[j], 0f, array4[j]);
				Vector4 levelMask = vector3 * (4 - j) - Vector4.one;
				Shaders.BranchingLevel(A, offsetTex, xzTex, columnTex, volumeTex, levelTex, configTex, vector2 + vector4, levelMask);
				Swap(B, D);
				RemapTex(D, B, A, vector * ((j == 0) ? 1 : 0), vector * ((j == 1) ? 1 : 0), vector * ((j == 2) ? 1 : 0), vector * ((j == 3) ? 1 : 0), additive: true);
			}
			Vector4 transU = Vector4.one * array[i] + new Vector4(array3[0], array3[1], array3[2], array3[3]);
			Vector4 transV = Vector4.one * array2[i] + new Vector4(array4[0], array4[1], array4[2], array4[3]);
			Shaders.BranchingTranslate(B, offsetTex, xzTex, columnTex, E, D, default(Color32), default(Color32), Vector4.one * 0.25f, partial: false, transU, transV);
			Shaders.NormalEncode(A, B);
			RemapTex(B, resultTex, A, vector * ((i == 0) ? 1 : 0), vector * ((i == 1) ? 1 : 0), vector * ((i == 2) ? 1 : 0), vector * ((i == 3) ? 1 : 0), additive: true);
			Swap(resultTex, B);
		}
	}

	public static void GetConfigPatchTex2(Tex resultTex, Tex A, Tex B, Tex C, Tex D, Texture offsetTex, Texture xzSupTex, Texture columnSupTex, Texture levelTex, Texture levelSupTex)
	{
		ClearTex(B, offsetTex);
		ClearTex(C, offsetTex);
		for (int i = 0; i < 8; i++)
		{
			Vector3 translation = new Vector3(i & 1, i >> 2, (i >> 1) & 1);
			TranslateTex(A, offsetTex, xzSupTex, columnSupTex, levelSupTex, translation, Vector3.one * 2f, EmptyType, InternalType);
		}
	}

	public static void GetConfigPatchTex(Tex resultTex, Tex A, Tex B, Texture offsetTex, Texture xzTex, Texture columnTex, Texture typeTex, Texture volumeTex, Texture configTex)
	{
		Color32 emptyColor = default(Color32);
		Color32 internalColor = new Color32(byte.MaxValue, 0, 0, 0);
		ClearTex(resultTex, volumeTex);
		for (int i = 0; i < 6; i++)
		{
			Vector3 translation = new Vector3(ConfigDx[i], ConfigDy[i], ConfigDz[i]);
			TranslateTex(A, offsetTex, xzTex, columnTex, volumeTex, translation, Vector3.one, emptyColor, internalColor);
			Swap(B, resultTex);
			Shaders.AddIfBetween(resultTex, B, A, new Color32(1, 0, 0, 0), new Color32(128, 0, 0, 0), new Vector4((float)(1 << i) / 255f, 0f, 0f, 0f));
		}
		Swap(B, resultTex);
		Shaders.BitwiseAnd(resultTex, B, configTex, flipSecond: true);
		Swap(B, resultTex);
		Shaders.AddIfBetween(resultTex, B, typeTex, new Color32(1, 0, 0, 0), new Color32(128, 0, 0, 0), Vector4.zero, -Vector3.one);
	}

	public static void GetConfigTex(Tex resultTex, Tex A, Tex B, Texture offsetTex, Texture xzTex, Texture columnTex, Texture dataTex, Texture branchingTex, Texture levelTex)
	{
		Color32 emptyColor = default(Color32);
		Color32 internalColor = new Color32(byte.MaxValue, 0, 0, 0);
		TranslateTex(A, offsetTex, xzTex, columnTex, dataTex, -Vector3.up, Vector3.one, emptyColor, internalColor);
		TranslateTex(B, offsetTex, xzTex, columnTex, dataTex, Vector3.up, Vector3.one, emptyColor, internalColor);
		RemapTex(resultTex, A, dataTex, Vector4.zero, new Vector4(1f, 0f, 0f, 0f), Vector4.zero, Vector4.zero, additive: true);
		Swap(A, resultTex);
		RemapTex(resultTex, A, B, Vector4.zero, Vector4.zero, new Vector4(1f, 0f, 0f, 0f), Vector4.zero, additive: true);
		Swap(A, resultTex);
		Shaders.BranchingConfig(resultTex, branchingTex, A, levelTex);
	}

	public static void GetConfigTex_2(Tex resultTex, Tex A, Tex B, Tex C, Texture offsetTex, Texture xzTex, Texture columnTex, Texture dataTex)
	{
		Color32 emptyColor = default(Color32);
		Color32 internalColor = new Color32(byte.MaxValue, 0, 0, 0);
		ClearTex(A, dataTex);
		AddNonZeroTex(B, A, dataTex, new Vector4(0.003921569f, 0f, 0f, 0f), new Vector4(0f, 0.5f, 0.5f, 0.5f));
		TranslateTex(A, offsetTex, xzTex, columnTex, B, -Vector3.up, Vector3.one, emptyColor, internalColor);
		TranslateTex(C, offsetTex, xzTex, columnTex, B, Vector3.up, Vector3.one, emptyColor, internalColor);
		RemapTex(resultTex, A, B, Vector4.zero, new Vector4(1f, 0f, 0f, 0f), Vector4.zero, Vector4.zero, additive: true);
		Swap(A, resultTex);
		RemapTex(resultTex, A, C, Vector4.zero, Vector4.zero, new Vector4(1f, 0f, 0f, 0f), Vector4.zero, additive: true);
		Swap(A, resultTex);
		ClearTex(resultTex, dataTex);
		TranslateTex(B, offsetTex, xzTex, columnTex, A, Vector3.forward, Vector3.one, emptyColor, internalColor);
		Swap(C, resultTex);
		Shaders.Add(resultTex, C, B, Vector4.one * 1f, Vector4.one * 2f, Vector4.zero);
		TranslateTex(B, offsetTex, xzTex, columnTex, A, -Vector3.forward, Vector3.one, emptyColor, internalColor);
		Swap(C, resultTex);
		Shaders.Add(resultTex, C, B, Vector4.one * 1f, Vector4.one * 4f, Vector4.zero);
		TranslateTex(B, offsetTex, xzTex, columnTex, A, Vector3.right, Vector3.one, emptyColor, internalColor);
		Swap(C, resultTex);
		Shaders.Add(resultTex, C, B, Vector4.one * 1f, Vector4.one * 8f, Vector4.zero);
		TranslateTex(B, offsetTex, xzTex, columnTex, A, -Vector3.right, Vector3.one, emptyColor, internalColor);
		Swap(C, resultTex);
		Shaders.Add(resultTex, C, B, Vector4.one * 1f, Vector4.one * 16f, Vector4.zero);
		Swap(A, resultTex);
		Shaders.Branching(resultTex, A);
	}

	public static void GetHackedBranchingTex(Tex resultTex, Tex A, Tex B, Tex C, Texture offsetTex, Texture xzTex, Texture columnTex, Texture dataTex)
	{
		Color32 emptyColor = default(Color32);
		Color32 internalColor = new Color32(byte.MaxValue, 0, 0, 0);
		Shaders.AddIfBetween(C, dataTex, dataTex, new Color32(1, 0, 0, 0), new Color32(byte.MaxValue, 0, 0, 0), new Vector4(1f, 0f, 0f, 0f));
		ClearTex(resultTex, dataTex);
		TranslateTex(A, offsetTex, xzTex, columnTex, C, Vector3.forward, Vector3.one, emptyColor, internalColor);
		Swap(B, resultTex);
		RemapTex(resultTex, B, A, Vector4.one, Vector4.zero, Vector4.zero, Vector4.zero, additive: true);
		TranslateTex(A, offsetTex, xzTex, columnTex, C, -Vector3.forward, Vector3.one, emptyColor, internalColor);
		Swap(B, resultTex);
		RemapTex(resultTex, B, A, Vector4.zero, Vector4.one, Vector4.zero, Vector4.zero, additive: true);
		TranslateTex(A, offsetTex, xzTex, columnTex, C, Vector3.right, Vector3.one, emptyColor, internalColor);
		Swap(B, resultTex);
		RemapTex(resultTex, B, A, Vector4.zero, Vector4.zero, Vector4.one, Vector4.zero, additive: true);
		TranslateTex(A, offsetTex, xzTex, columnTex, C, -Vector3.right, Vector3.one, emptyColor, internalColor);
		Swap(B, resultTex);
		RemapTex(resultTex, B, A, Vector4.zero, Vector4.zero, Vector4.zero, Vector4.one, additive: true);
	}

	public static void GetTypeTransitionTex(Tex resultTex, Tex A, Tex B, Texture offsetTex, Texture xzTex, Texture columnTex, Texture typeProcessedTex, Texture branchingTex, Texture typeAtlasTex)
	{
		Shaders.BranchingTranslate(resultTex, offsetTex, xzTex, columnTex, typeProcessedTex, branchingTex, EmptyType, InternalType, new Vector4(1f, 0f, 0f, 0f), partial: true, new Vector4(0f, 0f, 1f, -1f), new Vector4(1f, -1f, 0f, 0f));
		Swap(A, resultTex);
		ClearTex(B, A);
		Shaders.TypeTransitions(resultTex, B, typeProcessedTex, A, typeAtlasTex);
	}

	public static void GetUvBaseTex(Tex resultTex, Texture typeProcessedTex, Texture offsetTex, Texture xzTex, Texture noiseTex, Texture typeAtlasTex, XYZ worldStart)
	{
		Shaders.UvBase(resultTex, typeProcessedTex, typeAtlasTex, noiseTex, offsetTex, xzTex, worldStart.ToVector3());
	}

	public static void GetUvTransitionTex(Tex resultTex, Tex A, Tex B, Texture typeProcessedTex, Texture typeTransTex, Texture offsetTex, Texture xzTex, Texture noiseTex, Texture typeAtlasTex)
	{
		Shaders.UvTransitions(resultTex, typeTransTex, typeAtlasTex);
	}

	public static void GetUvSpecialTex(Tex resultTex, Texture typeProcessedTex, Texture uvBaseTex, Texture typeAtlasTex)
	{
		Shaders.UvSpecial(resultTex, typeProcessedTex, typeAtlasTex, uvBaseTex);
	}

	public static void GetSampleTex(Tex resultTex, Texture xzTex, Texture columnTex, Texture offsetTex, Texture configTex, Vector3 corner, Vector3 size)
	{
	}

	public static void GetColumnCountTex(Tex resultTex, Tex Z, Texture columnTex, Vector3 sampleStart, Vector3 sampleSize, int mipLevel = 0)
	{
		Shaders.ColumnCountInit(resultTex, columnTex, sampleStart, sampleSize);
		if (mipLevel != 0)
		{
			for (int i = 0; i < mipLevel; i++)
			{
				Shaders.MergeAdd(Z, resultTex);
				Swap(resultTex, Z);
			}
		}
	}

	public static void GetMultiplayerHeightmap(Tex resultTex, Tex A, Texture heightmapTex, int nPlayers)
	{
		int[] array = new int[4] { 0, 2, 1, 3 };
		ClearTex(resultTex, heightmapTex);
		for (int i = 0; i < nPlayers; i++)
		{
			Swap(A, resultTex);
			Shaders.Rotate(resultTex, A, heightmapTex, array[i]);
		}
	}

	public static void GetPartialFillingTex(Tex resultTex, Tex Z, Texture fillingFullTex, Vector3 start, Vector3 size)
	{
		Shaders.Retarget(Z, fillingFullTex, new Vector2(start.x, start.z), new Vector2(size.x, size.z));
		byte b = (byte)start.y;
		byte b2 = (byte)(start.y + size.y - 1f);
		float num = (0f - start.y) / 255f;
		Shaders.Threshold(resultTex, Z, new Color32(b, b, b, b), new Color32(b2, b2, b2, b2), new Vector4(num, num, num, num));
	}

	public static void GetAddedFillingTex(Tex resultTex, Texture fillingTex, Texture fillingTex2, Vector3 translationSecondTex)
	{
		Shaders.FillingAdd(resultTex, fillingTex, fillingTex2, translationSecondTex);
	}

	public static void GetOverwrittenDataTex(Tex resultTex, Texture offsetTex, Texture dataTex, Texture xzTex2, Texture columnTex2, Texture dataTex2, Vector3 translation2, Color32 internalColor)
	{
		Shaders.OverwriteData(resultTex, offsetTex, dataTex, xzTex2, columnTex2, dataTex2, translation2, internalColor);
	}

	public static void TranslateTex(Tex resultTex, Texture offsetTex, Texture xzTex, Texture columnTex, Texture dataTex, Vector3 translation, Vector3 scale, Color32 emptyColor, Color32 internalColor)
	{
		Shaders.Translate(resultTex, offsetTex, xzTex, columnTex, dataTex, translation, scale, emptyColor, internalColor);
	}

	public static void CopyTex(Tex resultTex, Texture dataTex)
	{
		Shaders.Copy(resultTex, dataTex);
	}

	public static void ClearTex(Tex resultTex, Texture emptyTex, Color32 color = default(Color32), int width = 0, int height = 0)
	{
		Shaders.Clear(resultTex, emptyTex, color, width, height);
	}

	public static void RemapTex(Tex resultTex, Texture previousTex, Texture dataTex, Vector4 rMask, Vector4 gMask, Vector4 bMask, Vector4 aMask, bool additive = false)
	{
		Shaders.Remap(resultTex, additive ? previousTex : null, dataTex, rMask, gMask, bMask, aMask);
	}

	public static void AddNonZeroTex(Tex resultTex, Texture previousTex, Texture dataTex, Color32 color, Vector4 zeroMask = default(Vector4))
	{
		Shaders.AddNonZero(resultTex, previousTex, dataTex, new Vector4((int)color.r, (int)color.g, (int)color.b, (int)color.a) * 0.003921569f, zeroMask);
	}

	public static void AddNonZeroTex(Tex resultTex, Texture previousTex, Texture dataTex, Vector4 add, Vector4 zeroMask = default(Vector4))
	{
		Shaders.AddNonZero(resultTex, previousTex, dataTex, add, zeroMask);
	}

	public static void Swap(Tex A, Tex B)
	{
		A.Swap(B);
	}

	public static void StartTimer()
	{
		timers.Push(Time.realtimeSinceStartup);
	}

	public static float StopTimer(Func<float, string> getMessage)
	{
		float num = timers.Pop();
		float num2 = Time.realtimeSinceStartup - num;
		string text = getMessage(num2);
		if (!string.IsNullOrEmpty(text))
		{
			Debug.Log(text);
		}
		return num2;
	}

	public static float StopTimer(string s)
	{
		return StopTimer((float duration) => s + ": " + duration + " seconds.");
	}

	public static float StopTimer()
	{
		return StopTimer((float d) => (string)null);
	}
}
