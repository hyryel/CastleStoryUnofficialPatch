using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Brix.Engine.Abstract;
using Brix.Engine.FlatWorld.Layers;
using Brix.Engine.FlatWorld.SoftAlgos;
using Brix.Util;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

internal class LodUpdater : MonoBehaviour
{
	public struct VoxelMergeData
	{
		public static VoxelMergeData Empty = default(VoxelMergeData);

		private int _t0;

		private int _t1;

		private int _t2;

		private int _c0;

		private int _c1;

		private int _c2;

		public void Add(int t)
		{
			if (t != 0 && t != 255)
			{
				if (_t0 == 0 || _t0 == t)
				{
					_t0 = t;
					_c0++;
				}
				else if (_t1 == 0 || _t1 == t)
				{
					_t1 = t;
					_c1++;
				}
				else if (_t2 == 0 || _t2 == t)
				{
					_t2 = t;
					_c2++;
				}
			}
		}

		public int GetMergedType()
		{
			return (_c0 >= _c1 && _c0 >= _c2) ? _t0 : ((_c1 >= _c2) ? _t1 : _t2);
		}
	}

	public class LodBatchDrawer : IPoolable, IResetable, IDisposable
	{
		private class ThreadableWork : IPoolable, IResetable, IDisposable
		{
			private LodBatchDrawer _drawer;

			private List<XYZ> _lowMvs;

			private Dictionary<XYZ, FlatWorldColors> _lowWorlds;

			private int _lod;

			private bool _sleep;

			private Thread _thread;

			private readonly ThreadStart _workAction;

			public bool IsAlive => _thread.IsAlive;

			public static ThreadableWork Get()
			{
				return ObjectPool<ThreadableWork>.Reserve();
			}

			public void ResetObject()
			{
			}

			public void Dispose()
			{
				if (_thread.IsAlive)
				{
					_thread.Abort();
				}
				ObjectPool<ThreadableWork>.Release(this);
			}

			public ThreadableWork()
			{
				_workAction = Work;
			}

			public ThreadableWork Init(LodBatchDrawer drawer, List<XYZ> lowMvs, Dictionary<XYZ, FlatWorldColors> lowWorlds, int lod, bool sleep)
			{
				_drawer = drawer;
				_lowMvs = lowMvs;
				_lowWorlds = lowWorlds;
				_lod = lod;
				_sleep = sleep;
				_thread = new Thread(_workAction);
				_thread.Start();
				return this;
			}

			private void Work()
			{
				if (_sleep)
				{
					Thread.Sleep(0);
				}
				_drawer.InitDrawer(_lod, _lowWorlds);
				_drawer.InitLodData(_lowMvs, _lowWorlds);
			}
		}

		public TestWorldConfig3 Test;

		public bool Threaded;

		private readonly Columns _columns = new Columns();

		private readonly LevelConfigNormalUvs _levelConfigNormalUvs = new LevelConfigNormalUvs();

		private readonly LodDrawer _lodDrawer = new LodDrawer();

		private Dictionary<XYZ, FlatWorldColors> _finerWorlds;

		private readonly Func<XYZ, IFlatWorldColors> _getFinerWorldFunc;

		private readonly List<XYZ> lod1Mvs = new List<XYZ>();

		private readonly List<XYZ> paddingMvs = new List<XYZ>();

		public LodBatchDrawer()
		{
			_getFinerWorldFunc = GetFinerWorld;
		}

		public static LodBatchDrawer Get()
		{
			return ObjectPool<LodBatchDrawer>.Reserve();
		}

		public LodBatchDrawer Init(TestWorldConfig3 test, bool threaded)
		{
			Test = test;
			Threaded = threaded;
			return this;
		}

		public void ResetObject()
		{
		}

		public void Dispose()
		{
			ObjectPool<LodBatchDrawer>.Release(this);
		}

		public void GetPaddedLowerMvs(List<XYZ> finerMvs, List<XYZ> resultLowerMvs, List<XYZ> resultPaddingMvs, int lodDiff)
		{
			HashSet<XYZ> hashSet = CollectionPool.RequestHashSet<XYZ>();
			for (int i = 0; i < finerMvs.Count; i++)
			{
				hashSet.Add(finerMvs[i] >> lodDiff);
			}
			HashSet<XYZ> hashSet2 = CollectionPool.RequestHashSet<XYZ>();
			foreach (XYZ item in hashSet)
			{
				foreach (XYZ item2 in Calcule.VolumeBetween(-XYZ.one, XYZ.one << lodDiff))
				{
					hashSet2.Add((item << lodDiff) + item2);
				}
			}
			HashSet<XYZ> hashSet3 = CollectionPool.RequestHashSet<XYZ>();
			foreach (XYZ item3 in hashSet2)
			{
				hashSet3.Add(item3 >> 1);
			}
			HashSet<XYZ> hashSet4 = CollectionPool.RequestHashSet<XYZ>();
			foreach (XYZ finerMv in finerMvs)
			{
				hashSet4.Add(finerMv >> 1);
			}
			hashSet4.Slinq().AddTo(resultLowerMvs);
			hashSet3.ExceptWith(hashSet4);
			hashSet3.Slinq().AddTo(resultPaddingMvs);
			hashSet.Dispose();
			hashSet2.Dispose();
			hashSet3.Dispose();
			hashSet4.Dispose();
		}

		public void QuickDrawLods(List<XYZ> finerMvs)
		{
			Neo.DoAll(QuickDrawLods_Coroutine(finerMvs));
		}

		public IEnumerator QuickDrawLods_Coroutine(List<XYZ> finerMvs)
		{
			lod1Mvs.Clear();
			paddingMvs.Clear();
			GetPaddedLowerMvs(finerMvs, lod1Mvs, paddingMvs, Test.lod);
			return _QuickDrawLods();
		}

		private IEnumerator _QuickDrawLods()
		{
			_finerWorlds = null;
			List<XYZ> lowResMvs = lod1Mvs;
			List<XYZ> lowPaddingMvs = paddingMvs;
			for (int lod = 1; lod <= Test.lod; lod++)
			{
				Dictionary<XYZ, FlatWorldColors> lowWorlds = CollectionPool.RequestDictionary<XYZ, FlatWorldColors>();
				List<XYZ> lodMvs = CollectionPool.RequestList<XYZ>();
				lowResMvs.Slinq().Concat(lowPaddingMvs.Slinq()).AddTo(lodMvs);
				if (Threaded)
				{
					using ThreadableWork work = ThreadableWork.Get().Init(this, lodMvs, lowWorlds, lod, Threaded);
					while (work.IsAlive)
					{
						yield return null;
					}
				}
				else
				{
					InitDrawer(lod, lowWorlds);
					InitLodData(lodMvs, lowWorlds);
				}
				lodMvs.Dispose();
				RenderLod(lowResMvs, lowWorlds);
				if (_finerWorlds != null)
				{
					foreach (FlatWorldColors finerWorld in _finerWorlds.Values)
					{
						Destroy(finerWorld);
					}
					_finerWorlds.Dispose();
				}
				_finerWorlds = lowWorlds;
				List<XYZ> finerMvs = CollectionPool.RequestList<XYZ>();
				lowResMvs.Slinq().AddTo(finerMvs);
				lowResMvs.Clear();
				lowPaddingMvs.Clear();
				GetPaddedLowerMvs(finerMvs, lowResMvs, lowPaddingMvs, Test.lod - lod);
				finerMvs.Dispose();
			}
			if (_finerWorlds == null)
			{
				yield break;
			}
			foreach (FlatWorldColors finerWorld2 in _finerWorlds.Values)
			{
				Destroy(finerWorld2);
			}
			_finerWorlds.Dispose();
		}

		private IFlatWorldColors GetFinerWorld(XYZ p)
		{
			if (_finerWorlds != null)
			{
				return _finerWorlds.GetValueOrDefault(p);
			}
			Test.InitLayers();
			return Test.drawer.GetCachedWorldIfExists(p);
		}

		private void InitDrawer(int lod, Dictionary<XYZ, FlatWorldColors> lowWorlds)
		{
			_lodDrawer.Init(Test, lod, _getFinerWorldFunc, lowWorlds.GetValueOrDefault<XYZ, FlatWorldColors>);
		}

		private void InitLodData(List<XYZ> lowMvs, Dictionary<XYZ, FlatWorldColors> lowWorlds)
		{
			Dictionary<XYZ, VoxelMergeData> dictionary = CollectionPool.RequestDictionary<XYZ, VoxelMergeData>();
			for (int i = 0; i < lowMvs.Count; i++)
			{
				_lodDrawer.LowMv = lowMvs[i];
				_lodDrawer.LowWorld = lowWorlds.GetValueOrCreate(lowMvs[i]);
				_lodDrawer.InsideFillingAndMergeData(dictionary);
			}
			for (int j = 0; j < lowMvs.Count; j++)
			{
				_lodDrawer.LowMv = lowMvs[j];
				_lodDrawer.LowWorld = lowWorlds.GetValueOrCreate(lowMvs[j]);
				_lodDrawer.BorderFillingAndInitRender(dictionary, _columns, _levelConfigNormalUvs);
			}
			dictionary.Dispose();
		}

		private void RenderLod(List<XYZ> lowMvs, Dictionary<XYZ, FlatWorldColors> lowWorlds)
		{
			for (int i = 0; i < lowMvs.Count; i++)
			{
				_lodDrawer.LowMv = lowMvs[i];
				_lodDrawer.LowWorld = lowWorlds.GetValueOrCreate(lowMvs[i]);
				_lodDrawer.Render();
			}
		}
	}

	public class LodDrawer
	{
		public XYZ LowMv;

		public int Lod;

		public IFlatWorldColors LowWorld;

		public Func<XYZ, IFlatWorldColors> GetFinerWorld;

		public Func<XYZ, IFlatWorldColors> GetNeighborWorld;

		public TestWorldConfig3 Test;

		private static XYZ[] UnitCube = Calcule.VolumeBetween(XYZ.zero, XYZ.one).ToArray();

		public XYZ MvSize => Test.MvSize;

		public XYZ Border => Test.Border;

		public Common Common => Test.Common;

		public MondeAtlas Atlas => Test.atlasComponent.atlas;

		public void Init(TestWorldConfig3 test, int lod, Func<XYZ, IFlatWorldColors> getFinerWorld, Func<XYZ, IFlatWorldColors> getNeighborWorld)
		{
			Test = test;
			Lod = lod;
			GetFinerWorld = getFinerWorld;
			GetNeighborWorld = getNeighborWorld;
		}

		public void InsideFillingAndMergeData(Dictionary<XYZ, VoxelMergeData> mergedVoxels)
		{
			List<XYZ> list = CollectionPool.RequestList<XYZ>();
			List<IFlatWorldColors> list2 = CollectionPool.RequestList<IFlatWorldColors>();
			GetFinerWorlds(list, list2);
			Filling(list, list2);
			MergedTypes(list, list2, mergedVoxels);
			list.Dispose();
			list2.Dispose();
		}

		public void BorderFillingAndInitRender(Dictionary<XYZ, VoxelMergeData> mergedVoxels, Columns columns, LevelConfigNormalUvs levelConfigNormalUvs)
		{
			BorderFilling();
			TypesAndVolumes(columns, mergedVoxels);
			InitRender(levelConfigNormalUvs);
		}

		private void GetFinerWorlds(List<XYZ> finerMvs, List<IFlatWorldColors> finerWorlds)
		{
			for (int i = 0; i < UnitCube.Length; i++)
			{
				XYZ xYZ = UnitCube[i];
				XYZ xYZ2 = (LowMv << 1) + xYZ;
				IFlatWorldColors flatWorldColors = GetFinerWorld(xYZ2);
				if (flatWorldColors != null && !flatWorldColors.Filling.Empty)
				{
					finerMvs.Add(xYZ2);
					finerWorlds.Add(flatWorldColors);
				}
			}
		}

		private void Filling(List<XYZ> finerMvs, List<IFlatWorldColors> finerWorlds)
		{
			AllocColumns(LowWorld, MvSize.x + Border.x * 2, MvSize.z + Border.z * 2);
			IFlatWorldColors lowWorld = LowWorld;
			TexColors texColors = (TexColors)lowWorld.Filling;
			texColors.Clear(MvSize.x + Border.x * 2, MvSize.z + Border.z * 2);
			for (int i = 0; i < finerMvs.Count; i++)
			{
				XYZ xYZ = finerMvs[i];
				IFlatWorldColors flatWorldColors = finerWorlds[i];
				ITexColors filling = flatWorldColors.Filling;
				XYZ xYZ2 = XYZ.Mul(xYZ - (LowMv << 1), MvSize >> 1) + Border;
				for (int j = 0; j < MvSize.z; j++)
				{
					for (int k = 0; k < MvSize.x; k++)
					{
						Color32 pixel = filling.GetPixel(Border.x + k, Border.z + j);
						if (pixel.r < pixel.g)
						{
							int y = Border.y;
							pixel = Layer.TranslateColumn(pixel, y * 2 - Border.y);
							pixel = Layer.ScaleColumn(pixel, 2);
							pixel = Layer.TranslateColumn(pixel, xYZ2.y - y);
							pixel = Layer.ClampColumn(pixel, 0, MvSize.y + Border.y * 2);
							if (pixel.r < pixel.g)
							{
								Color32 pixel2 = texColors.GetPixel(xYZ2.x + (k >> 1), xYZ2.z + (j >> 1));
								pixel2 = Layer.Column_AddPositive(pixel, pixel2);
								texColors.SetPixel(xYZ2.x + (k >> 1), xYZ2.z + (j >> 1), pixel2);
							}
						}
					}
				}
			}
		}

		private void BorderFilling()
		{
			IFlatWorldColors lowWorld = LowWorld;
			TexColors texColors = (TexColors)lowWorld.Filling;
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (j == 0 && i == 0)
					{
						continue;
					}
					XYZ xYZ = new XYZ(j, 0, i);
					IFlatWorldColors flatWorldColors = GetNeighborWorld(LowMv + xYZ);
					if (flatWorldColors == null || flatWorldColors.Filling.Empty)
					{
						continue;
					}
					int num = ((xYZ.x >= 0) ? ((xYZ.x > 0) ? (MvSize.x + Border.x) : Border.x) : 0);
					int num2 = ((xYZ.x != 0) ? Border.x : MvSize.x);
					int num3 = ((xYZ.z >= 0) ? ((xYZ.z > 0) ? (MvSize.z + Border.z) : Border.z) : 0);
					int num4 = ((xYZ.z != 0) ? Border.z : MvSize.z);
					int num5 = -xYZ.x * MvSize.x;
					int num6 = -xYZ.z * MvSize.z;
					ITexColors filling = flatWorldColors.Filling;
					for (int k = num; k < num + num2; k++)
					{
						for (int l = num3; l < num3 + num4; l++)
						{
							texColors.SetPixel(k, l, filling.GetPixel(k + num5, l + num6));
						}
					}
				}
			}
		}

		private void MergedTypes(List<XYZ> finerMvs, List<IFlatWorldColors> finerWorlds, Dictionary<XYZ, VoxelMergeData> mergedVoxels)
		{
			for (int i = 0; i < finerMvs.Count; i++)
			{
				XYZ a = finerMvs[i];
				IFlatWorldColors flatWorldColors = finerWorlds[i];
				XYZ xYZ = XYZ.Mul(a, MvSize);
				Color32[] pixels = flatWorldColors.Offset.GetPixels32();
				Color32[] pixels2 = flatWorldColors.Type.GetPixels32();
				Color32[] pixels3 = flatWorldColors.Config.GetPixels32();
				int countOffsets = flatWorldColors.CountOffsets;
				for (int j = 0; j < countOffsets; j++)
				{
					if (pixels3[j].r == 0)
					{
						continue;
					}
					XYZ xYZ2 = Soft.DecodeXYZ(Soft.DecodeInt(pixels[j])) - Border;
					if (xYZ2 >= XYZ.zero && xYZ2 < MvSize)
					{
						XYZ key = xYZ2 + xYZ >> 1;
						if (!mergedVoxels.TryGetValue(key, out var value))
						{
							value = default(VoxelMergeData);
							value.Add(pixels2[j].r);
							mergedVoxels.Add(key, value);
						}
						else
						{
							value.Add(pixels2[j].r);
							mergedVoxels[key] = value;
						}
					}
				}
			}
		}

		private void TypesAndVolumes(Columns columns, Dictionary<XYZ, VoxelMergeData> mergedVoxels)
		{
			AllocColumns(LowWorld, LowWorld.Filling.Width, LowWorld.Filling.Height);
			IFlatWorldColors lowWorld = LowWorld;
			ITexColors column = lowWorld.Column;
			ITexColors filling = lowWorld.Filling;
			_ = Border;
			columns.Process(column, filling, 0, MvSize.y + Border.y * 2);
			lowWorld.CountOffsets = Soft.XZ(lowWorld.Xz, lowWorld.Column);
			int num = 1 << Calcule.GetBestSquarePower(lowWorld.CountOffsets);
			AllocData(lowWorld, num, num);
			Soft.Offset(lowWorld.Offset, lowWorld.Xz, lowWorld.Column, num);
			Color32[] pixels = lowWorld.Offset.GetPixels32();
			Color32[] orAllocPixels = Soft.GetOrAllocPixels32(lowWorld.Type, lowWorld.Offset.Width, lowWorld.Offset.Height);
			Color32[] orAllocPixels2 = Soft.GetOrAllocPixels32(lowWorld.Volume, lowWorld.Offset.Width, lowWorld.Offset.Height);
			XYZ xYZ = XYZ.Mul(LowMv, MvSize);
			int countOffsets = lowWorld.CountOffsets;
			for (int i = 0; i < countOffsets; i++)
			{
				XYZ xYZ2 = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i])) - Border;
				int mergedType = mergedVoxels.GetValueOrDefault(xYZ2 + xYZ, VoxelMergeData.Empty).GetMergedType();
				if (mergedType == 0)
				{
					orAllocPixels[i].r = (orAllocPixels2[i].r = 0);
					continue;
				}
				orAllocPixels[i].r = (byte)((mergedType < 255) ? mergedType : Atlas.DefaultDeepType);
				orAllocPixels2[i].r = 1;
			}
			lowWorld.Type.SetPixels32(orAllocPixels, lowWorld.Offset.Width, lowWorld.Offset.Height);
			lowWorld.Volume.SetPixels32(orAllocPixels2, lowWorld.Offset.Width, lowWorld.Offset.Height);
		}

		private void InitRender(LevelConfigNormalUvs levelConfigNormalUvs)
		{
			IFlatWorldColors lowWorld = LowWorld;
			XYZ paddedStart = XYZ.Mul(LowMv, MvSize) - Border;
			lowWorld.InitPreRender(Common, paddedStart, Lod, levelConfigNormalUvs);
			lowWorld.InitRender(Common, paddedStart, levelConfigNormalUvs);
		}

		public void Render()
		{
			IFlatWorldColors lowWorld = LowWorld;
			XYZ xYZ = XYZ.Mul(LowMv, MvSize) - Border;
			Test.RenderMegaVoxelLod(lowWorld, xYZ + Border, Border, Lod);
		}

		private bool IsEmptyColumn(Color32 c)
		{
			return c.r >= c.g;
		}
	}

	private class Pool
	{
		private static readonly List<Pool> _pools = new List<Pool>();

		public Pool()
		{
			_pools.Add(this);
		}

		public virtual void Clear()
		{
		}

		public static void ClearAll()
		{
			for (int i = 0; i < _pools.Count; i++)
			{
				_pools[i].Clear();
			}
			_pools.Clear();
		}
	}

	private class Pool<T> : Pool
	{
		private readonly Queue<T> _queue = new Queue<T>();

		public bool IsEmpty => _queue.Count == 0;

		public void Push(T t)
		{
			_queue.Enqueue(t);
		}

		public T Pop()
		{
			return _queue.Dequeue();
		}

		public override void Clear()
		{
			_queue.Clear();
		}
	}

	private static class ObjectPool<T> where T : class, IPoolable, new()
	{
		private static readonly Pool<T> _pool = new Pool<T>();

		public static T Reserve()
		{
			return _pool.IsEmpty ? new T() : _pool.Pop();
		}

		public static void Release(T freedObject)
		{
			freedObject.ResetObject();
			_pool.Push(freedObject);
		}
	}

	private static class ArrayPool<T>
	{
		private static readonly Dictionary<int, Pool<T[]>> _pools = new Dictionary<int, Pool<T[]>>();

		public static T[] Reserve(int size)
		{
			Pool<T[]> valueOrDefault = _pools.GetValueOrDefault(size);
			return (valueOrDefault == null || valueOrDefault.IsEmpty) ? new T[size] : valueOrDefault.Pop();
		}

		public static void Release(T[] freedObject)
		{
			Pool<T[]> valueOrCreate = _pools.GetValueOrCreate(freedObject.Length);
			valueOrCreate.Push(freedObject);
		}
	}

	public TestWorldConfig3 test = null;

	public float UpdateDelay = 5f;

	public bool Threaded = true;

	private readonly HashSet<XYZ> _dirtyMvs = new HashSet<XYZ>();

	private IEnumerator _work;

	private float _lastUpdate;

	public bool IsDirty => _dirtyMvs.Count > 0;

	public void SetDirty(XYZ dirtyMv)
	{
		_dirtyMvs.Add(dirtyMv);
	}

	public void ClearDirty()
	{
		_dirtyMvs.Clear();
	}

	public void ClearWork()
	{
		if (_work != null)
		{
			(_work as IDisposable).Dispose();
			_work = null;
		}
	}

	public void ClearAll()
	{
		ClearWork();
		ClearDirty();
		Pool.ClearAll();
	}

	private void Update()
	{
		if (!test.IsWorking && !test.IsQuickDrawing())
		{
			if (Time.time >= _lastUpdate + UpdateDelay && IsDirty && _work == null)
			{
				_lastUpdate = Time.time;
				_work = Work();
			}
			else if (_work != null && !_work.MoveNext())
			{
				(_work as IDisposable).Dispose();
				_work = null;
			}
		}
	}

	private void OnDestroy()
	{
		ClearAll();
	}

	private IEnumerator QuickDraw_Coroutine()
	{
		List<XYZ> finerMvs = CollectionPool.RequestList<XYZ>();
		finerMvs.AddRange(_dirtyMvs);
		ClearDirty();
		using LodBatchDrawer drawer = LodBatchDrawer.Get().Init(test, Threaded);
		using (IDisposable e = (IDisposable)drawer.QuickDrawLods_Coroutine(finerMvs))
		{
			while ((e as IEnumerator).MoveNext())
			{
				yield return e;
			}
		}
		CollectionPool.Release(ref finerMvs);
	}

	public IEnumerator StartWorking()
	{
		if (_work == null)
		{
			_work = Work();
		}
		return _work;
	}

	private IEnumerator Work()
	{
		return QuickDraw_Coroutine();
	}

	private static void AllocColumns(IFlatWorldColors world, int width, int height)
	{
		Alloc(world.Filling, width, height);
		Alloc(world.Column, width, height);
		Alloc(world.Xz, width, height);
	}

	private static void AllocData(IFlatWorldColors world, int width, int height)
	{
		Alloc(world.Offset, width, height);
		Alloc(world.Type, width, height);
		Alloc(world.Volume, width, height);
		Alloc(world.Depth, width, height);
		Alloc(world.Smooth, width, height);
		Alloc(world.Level, width, height);
		Alloc(world.Config, width, height);
		Alloc(world.Normal, width, height);
		Alloc(world.UvBase, width, height);
		Alloc(world.UvTrans, width, height);
		Alloc(world.UvSpecial, width, height);
	}

	private static void Alloc(ITexColors tex, int width, int height)
	{
		int num = width * height;
		if (tex.Width * tex.Height != num)
		{
			if (!tex.Empty)
			{
				ArrayPool<Color32>.Release(tex.GetPixels32());
			}
			tex.SetPixels32(ArrayPool<Color32>.Reserve(num), width, height);
		}
	}

	private static void Destroy(IFlatWorldColors world)
	{
		Destroy(world.Filling);
		Destroy(world.Column);
		Destroy(world.Xz);
		Destroy(world.Offset);
		Destroy(world.Type);
		Destroy(world.Volume);
		Destroy(world.Depth);
		Destroy(world.Smooth);
		Destroy(world.Level);
		Destroy(world.Config);
		Destroy(world.Normal);
		Destroy(world.UvBase);
		Destroy(world.UvTrans);
		Destroy(world.UvSpecial);
		world.Destroy();
	}

	private static void Destroy(ITexColors tex)
	{
		if (!tex.Empty)
		{
			ArrayPool<Color32>.Release(tex.GetPixels32());
		}
		tex.Destroy();
	}
}
