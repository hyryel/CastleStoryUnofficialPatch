using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.External.Signals;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

[JsonObject(MemberSerialization.OptIn)]
public class TerrainSurfaceLayer : Layer, ISelectableLayer
{
	public class LowResTerrainSurface : LowResLayer
	{
		private readonly HashSet<XYZ> _addedSet = new HashSet<XYZ>();

		private TerrainSurfaceLayer _parentLayer;

		private XYZ _scale;

		private SignalConnection _parentAddConnection = SignalConnection.Empty;

		private SignalConnection _parentClearConnection = SignalConnection.Empty;

		public override void Init(Layer parentLayer, XYZ scale)
		{
			base.Init(parentLayer, scale);
			_parentLayer = (TerrainSurfaceLayer)parentLayer;
			_scale = scale;
			foreach (XYZ point in _parentLayer.addedSet.GetPoints())
			{
				_addedSet.Add(XYZ.Div(point, scale));
			}
			_parentAddConnection = _parentLayer.addedSet.AddSignal.Connect(OnParentAdd);
			_parentClearConnection = _parentLayer.addedSet.ClearSignal.Connect(OnParentClear);
		}

		public override void Destroy()
		{
			base.Destroy();
			_parentAddConnection.Disconnect();
			_addedSet.Clear();
		}

		public void OnParentAdd(XYZ p)
		{
			_addedSet.Add(XYZ.Div(p, _scale));
		}

		public void OnParentClear()
		{
			_addedSet.Clear();
		}

		public override bool IsOccupiedOrUncertain(XYZ p)
		{
			return _addedSet.Contains(p) || base.IsOccupiedOrUncertain(p);
		}

		public override IEnumerable<XYZ> GetOccupiedOrUncertain()
		{
			return _addedSet.Union(base.GetOccupiedOrUncertain());
		}

		public override IEnumerable<XYZ> GetOccupiedOrUncertain(XYZ start, XYZ size)
		{
			XYZ end = start + size;
			return _addedSet.Where((XYZ p) => p >= start && p < end).Union(base.GetOccupiedOrUncertain(start, size));
		}
	}

	private static BrixLogger logger = new BrixLogger();

	[JsonProperty]
	public ColumnPointSet addedSet = new ColumnPointSet();

	public Dictionary<XYZ, VoxelTerrainResources> p2resources = new Dictionary<XYZ, VoxelTerrainResources>();

	private byte Sand => (byte)atlas.SandType;

	[JsonProperty]
	private KeyValuePair<XYZ, VoxelTerrainResources>[] pResourcesArray
	{
		get
		{
			return p2resources.ToArray();
		}
		set
		{
			p2resources = value.ToDictionary((KeyValuePair<XYZ, VoxelTerrainResources> kv) => kv.Key, (KeyValuePair<XYZ, VoxelTerrainResources> kv) => kv.Value);
		}
	}

	public override void Load(string folder)
	{
		string path = Path.Combine(folder, "TerrainSurfaceLayer.json");
		if (File.Exists(path))
		{
			string value = File.ReadAllText(path);
			JsonConvert.PopulateObject(value, this);
		}
	}

	public override void Save(string folder)
	{
		string contents = JsonConvert.SerializeObject(this);
		string path = Path.Combine(folder, "TerrainSurfaceLayer.json");
		File.WriteAllText(path, contents);
	}

	public override void Destroy()
	{
		base.Destroy();
		addedSet.Clear();
	}

	public override void IsNonEmpty(XYZ p, ref bool result)
	{
		if (!result && addedSet.Contains(p))
		{
			result = true;
		}
	}

	public override void GetVoxelData(XYZ p, ref Color32 data)
	{
		if (addedSet.Contains(p))
		{
			VoxelTerrainResources voxelTerrainResources = p2resources[p];
			byte r = (byte)voxelTerrainResources.MostCommonType;
			byte g = (byte)voxelTerrainResources.TotalVolume;
			data = new Color32(r, g, 1, 0);
		}
	}

	public override void GetPotentialTopVoxels(ICollection<XYZ> result)
	{
		foreach (XYZ point in addedSet.GetPoints())
		{
			result.Add(point);
		}
	}

	public override void GetPotentialBottomVoxels(ICollection<XYZ> result)
	{
		foreach (XYZ point in addedSet.GetPoints())
		{
			result.Add(point);
		}
	}

	public override void GetPotentialVisibleVoxels(ICollection<XYZ> result)
	{
		foreach (XYZ point in addedSet.GetPoints())
		{
			result.Add(point);
		}
	}

	public override void Filling(ITexColors resultTex, ITexColors previousTex, XYZ resultPosition, XYZ resultSize)
	{
		Filling_PositiveLookupCheck(resultTex, resultTex, resultPosition, resultSize);
	}

	public Color32 Filling_PositiveColumn(Color32 c, IntClasse xz)
	{
		byte b = c.r;
		byte b2 = c.g;
		byte b3 = c.b;
		byte b4 = c.a;
		int num;
		int num2;
		if (b < b2)
		{
			num = xz.BinarySearch(b4);
			if (num < 0)
			{
				num = ~num;
			}
			num2 = xz.BinarySearch(b3);
			if (num2 < 0)
			{
				num2 = ~num2;
			}
		}
		else
		{
			num = 0;
			num2 = xz.Count;
		}
		for (int i = num; i < xz.Count; i++)
		{
			int num3 = xz[i];
			if (num3 >= b4)
			{
				if (num3 == b4)
				{
					b4++;
				}
				if (num3 >= b2)
				{
					b2 = (byte)(num3 + 1);
				}
			}
		}
		for (int num4 = num2 - 1; num4 >= 0; num4--)
		{
			int num5 = xz[num4];
			if (num5 < b3)
			{
				if (num5 == b3 - 1)
				{
					b3--;
				}
				if (num5 < b)
				{
					b = (byte)num5;
				}
			}
		}
		if (b >= b2)
		{
			b = (b3 = (b4 = (b2 = byte.MaxValue)));
		}
		else
		{
			if (b3 < b)
			{
				b3 = b;
			}
			if (b4 > b2)
			{
				b4 = b2;
			}
			if (b3 > b4)
			{
				b3 = b4;
			}
		}
		return new Color32(b, b2, b3, b4);
	}

	public void Filling_PositiveLookupCheck(ITexColors resultTex, ITexColors previousTex, XYZ resultPosition, XYZ resultSize)
	{
		Color32[] pixels = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels);
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 c = (orAllocPixels[i] = pixels[i]);
			int num = i % previousTex.Width;
			int num2 = i / previousTex.Width;
			XZ p = new XZ(num + resultPosition.x, num2 + resultPosition.z);
			IntClasse column = addedSet.GetColumn(p);
			if (column != null)
			{
				if (resultPosition.y != 0)
				{
					c = Layer.TranslateColumn(c, resultPosition.y);
					c = Filling_PositiveColumn(c, column);
					c = Layer.TranslateColumn(c, -resultPosition.y);
					c = Layer.ClampColumn(c, 0, resultSize.y);
				}
				else
				{
					c = Filling_PositiveColumn(c, column);
				}
				orAllocPixels[i] = c;
			}
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public override void InitialData(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		InitialData_AddedSet(resultTex, offsetTex, previousTex, resultPosition, count);
	}

	public void InitialData_AddedSet(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels2);
		for (int i = 0; i < count; i++)
		{
			orAllocPixels[i] = pixels2[i];
			XYZ xYZ = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i]));
			XYZ xYZ2 = xYZ + resultPosition;
			if (!addedSet.Contains(xYZ2))
			{
				continue;
			}
			VoxelTerrainResources valueOrDefault = p2resources.GetValueOrDefault(xYZ2);
			if (valueOrDefault == null)
			{
				continue;
			}
			byte b = (byte)valueOrDefault.MostCommonType;
			byte b2 = (byte)valueOrDefault.TotalVolume;
			byte b3 = 1;
			if (b2 == 0)
			{
				continue;
			}
			if (!atlas.IsCroute(b))
			{
				if (b2 < 4)
				{
					b = Sand;
				}
				else
				{
					b2 = FlatWorld.InternalVolume.r;
					b3 = FlatWorld.InternalDepth.r;
				}
			}
			orAllocPixels[i] = new Color32(b, b2, b3, 0);
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public override LowResLayer NewLowRes()
	{
		return new LowResTerrainSurface();
	}
}
