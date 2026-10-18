using System.Collections.Generic;
using System.Linq;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;

namespace Brix.Engine.FlatWorld.Layers;

public class LayersDrawer
{
	public class DrawInfo
	{
		public SmallOrderedSubset_8from15 usedLayers;

		public bool ready;

		public bool drawn;

		public int checksum;

		public int drawnFrameIndex;

		public IFlatWorldColors cachedWorld;

		public bool Occupied => usedLayers.Count > 0;

		public void RecalculateUsedLayers(LayersDrawer drawer, XYZ p)
		{
			usedLayers.Clear();
			for (int i = 0; i < drawer.layers.Count; i++)
			{
				LowResLayer lowRes = drawer.layers[i].LowRes;
				if (lowRes.IsVisibleOrUncertain(p))
				{
					usedLayers.Add(i);
				}
			}
		}

		public void RecalculateChecksum(LayersDrawer drawer)
		{
			checksum = 0;
			for (int i = 0; i < usedLayers.Count; i++)
			{
				checksum += drawer.layers[usedLayers[i]].checksum;
			}
		}
	}

	public List<Layer> layers;

	public VoxelBounds bounds;

	public int frameIndex = 1;

	private Dictionary<XYZ, DrawInfo> mvInfo = new Dictionary<XYZ, DrawInfo>();

	public bool Ready => layers != null;

	public void Clear()
	{
		lock (mvInfo)
		{
			foreach (KeyValuePair<XYZ, DrawInfo> item in mvInfo)
			{
				DrawInfo value = item.Value;
				if (value.cachedWorld != null)
				{
					value.cachedWorld.Destroy();
				}
			}
			mvInfo.Clear();
		}
		layers = null;
		bounds = default(VoxelBounds);
	}

	public void InitLayers(Layers layers, XYZ mvSize)
	{
		lock (mvInfo)
		{
			foreach (KeyValuePair<XYZ, DrawInfo> item in mvInfo)
			{
				DrawInfo value = item.Value;
				value.ready = false;
			}
		}
		this.layers = layers.layers;
		for (int i = 0; i < this.layers.Count; i++)
		{
			this.layers[i].DestroyLowRes();
			this.layers[i].InitLowRes(mvSize);
		}
		InitOccupied();
	}

	public IEnumerable<XYZ> FindNearestRedrawPositions(XYZ p, int searchRadius = 5)
	{
		IEnumerable<XYZ> source = Calcule.CubeConcentrique(p, searchRadius);
		return source.Where(CheckDirty);
	}

	public XYZ FindNearestRedrawPosition(XYZ p, int searchRadius = 5)
	{
		return FindNearestRedrawPositions(p, searchRadius).FirstOrDefault();
	}

	public DrawInfo InitInfo(XYZ p)
	{
		DrawInfo value;
		lock (mvInfo)
		{
			if (!mvInfo.TryGetValue(p, out value))
			{
				value = new DrawInfo();
				mvInfo.Add(p, value);
			}
		}
		if (!value.ready)
		{
			int checksum = value.checksum;
			value.RecalculateUsedLayers(this, p);
			value.RecalculateChecksum(this);
			if (value.checksum != checksum)
			{
				value.drawn = false;
			}
			else if (value.checksum == 0)
			{
				value.drawn = true;
				value.drawnFrameIndex = 0;
			}
			value.ready = true;
		}
		return value;
	}

	public IFlatWorldColors GetCachedWorldIfExists(XYZ p)
	{
		DrawInfo drawInfo = InitInfo(p);
		return drawInfo.cachedWorld;
	}

	public IFlatWorldColors GetCachedWorld(XYZ p)
	{
		DrawInfo drawInfo = InitInfo(p);
		return drawInfo.cachedWorld ?? (drawInfo.cachedWorld = new FlatWorldColors());
	}

	public void ReleaseCachedWorld(XYZ p)
	{
		DrawInfo drawInfo = InitInfo(p);
		if (drawInfo.cachedWorld != null)
		{
			drawInfo.cachedWorld.Destroy();
		}
		drawInfo.cachedWorld = null;
	}

	public bool IsOccupied(XYZ p)
	{
		for (int i = 0; i < layers.Count; i++)
		{
			LowResLayer lowRes = layers[i].LowRes;
			if (lowRes.IsVisibleOrUncertain(p))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckOccupied(XYZ p)
	{
		return InitInfo(p).Occupied;
	}

	public void InitOccupied()
	{
		HashSet<XYZ> hashSet = CollectionPool.RequestHashSet<XYZ>();
		GetOccupied(hashSet);
		lock (mvInfo)
		{
			foreach (XYZ item in hashSet)
			{
				mvInfo.NewKey(item);
			}
		}
		hashSet.Dispose();
	}

	public void InitOccupied(XYZ start, XYZ size)
	{
		lock (mvInfo)
		{
			IEnumerable<XYZ> occupied = GetOccupied(start, size);
			foreach (XYZ item in occupied)
			{
				mvInfo.NewKey(item);
			}
		}
	}

	public IEnumerable<XYZ> GetOccupied()
	{
		HashSet<XYZ> hashSet = new HashSet<XYZ>();
		GetOccupied(hashSet);
		return hashSet;
	}

	public void GetOccupied(HashSet<XYZ> outOccupied)
	{
		for (int i = 0; i < layers.Count; i++)
		{
			outOccupied.UnionWith(layers[i].LowRes.GetVisibleOrUncertain());
		}
	}

	public IEnumerable<XYZ> GetOccupied(XYZ start, XYZ size)
	{
		for (int i = 0; i < layers.Count; i++)
		{
			LowResLayer lowRes = layers[i].LowRes;
			foreach (XYZ item in lowRes.GetVisibleOrUncertain(start, size))
			{
				yield return item;
			}
		}
	}

	public bool CheckDirty(XYZ p)
	{
		return !InitInfo(p).drawn;
	}

	public bool NeedsDrawConfirmation(XYZ p)
	{
		return InitInfo(p).drawnFrameIndex == frameIndex;
	}

	public void MarkAsDrawn(XYZ p)
	{
		DrawInfo drawInfo = InitInfo(p);
		drawInfo.drawn = true;
		drawInfo.drawnFrameIndex = frameIndex;
	}

	public void MarkAsDirty(XYZ p)
	{
		lock (mvInfo)
		{
			if (mvInfo.TryGetValue(p, out var value))
			{
				value.ready = (value.drawn = false);
				return;
			}
			value = new DrawInfo();
			mvInfo.Add(p, value);
		}
	}

	public void MarkAsDrawConfirmed(XYZ p)
	{
		DrawInfo drawInfo = InitInfo(p);
		drawInfo.drawnFrameIndex = 0;
	}

	public void GetDirty(ICollection<XYZ> results)
	{
		lock (mvInfo)
		{
			foreach (KeyValuePair<XYZ, DrawInfo> item in mvInfo)
			{
				XYZ key = item.Key;
				if (CheckDirty(key))
				{
					results.Add(key);
				}
			}
		}
	}

	public void GetDirty(XYZ start, XYZ size, ICollection<XYZ> results)
	{
		XYZ xYZ = start + size;
		lock (mvInfo)
		{
			foreach (KeyValuePair<XYZ, DrawInfo> item in mvInfo)
			{
				XYZ key = item.Key;
				if (key >= start && key < xYZ && CheckDirty(key))
				{
					results.Add(key);
				}
			}
		}
	}

	public void GetNeedingDrawConfirmation(ICollection<XYZ> results)
	{
		lock (mvInfo)
		{
			foreach (KeyValuePair<XYZ, DrawInfo> item in mvInfo)
			{
				XYZ key = item.Key;
				if (NeedsDrawConfirmation(key))
				{
					results.Add(key);
				}
			}
		}
	}
}
