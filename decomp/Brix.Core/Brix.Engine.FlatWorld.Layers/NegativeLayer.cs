using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Engine.Geometrie;
using Brix.Engine.Volumes;
using Brix.External.Signals;
using Brix.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

[JsonObject(MemberSerialization.OptIn)]
public class NegativeLayer : Layer, ISelectableLayer
{
	public class LowResNegative : LowResLayer
	{
		private readonly HashSet<XYZ> _removedSet = new HashSet<XYZ>();

		private NegativeLayer _parentLayer;

		private XYZ _scale;

		private SignalConnection _parentAddConnection = SignalConnection.Empty;

		private SignalConnection _parentClearConnection = SignalConnection.Empty;

		public override void Init(Layer parentLayer, XYZ scale)
		{
			base.Init(parentLayer, scale);
			_parentLayer = (NegativeLayer)parentLayer;
			_scale = scale;
			foreach (XYZ point in _parentLayer.removedSet.GetPoints())
			{
				_removedSet.Add(XYZ.Div(point, scale));
			}
			_parentAddConnection = _parentLayer.removedSet.AddSignal.Connect(OnParentAdd);
			_parentClearConnection = _parentLayer.removedSet.ClearSignal.Connect(OnParentClear);
		}

		public override void Destroy()
		{
			base.Destroy();
			_parentAddConnection.Disconnect();
			_parentClearConnection.Disconnect();
			_removedSet.Clear();
		}

		public void OnParentAdd(XYZ p)
		{
			_removedSet.Add(XYZ.Div(p, _scale));
		}

		public void OnParentClear()
		{
			_removedSet.Clear();
		}

		public override bool IsOccupiedOrUncertain(XYZ p)
		{
			return _removedSet.Contains(p) || base.IsOccupiedOrUncertain(p);
		}

		public override IEnumerable<XYZ> GetOccupiedOrUncertain()
		{
			return _removedSet.Union(base.GetOccupiedOrUncertain());
		}

		public override IEnumerable<XYZ> GetOccupiedOrUncertain(XYZ start, XYZ size)
		{
			XYZ end = start + size;
			return _removedSet.Where((XYZ p) => p >= start && p < end).Union(base.GetOccupiedOrUncertain(start, size));
		}
	}

	[JsonProperty]
	public ColumnPointSet removedSet = new ColumnPointSet();

	private readonly XYZ[] _pelure = Calcule.Pelure(XYZ.zero, 1).ToArray();

	private byte Grass => (byte)atlas.GrassType;

	private byte Sand => (byte)atlas.SandType;

	public override void Load(string folder)
	{
		string path = Path.Combine(folder, "RemovedVoxels.json");
		if (File.Exists(path))
		{
			string value = File.ReadAllText(path);
			removedSet.AddCustomRegion(new VoxelSet(JsonConvert.DeserializeObject<XYZ[]>(value)));
		}
	}

	public override void Save(string folder)
	{
		string contents = JsonConvert.SerializeObject(removedSet.GetPoints().ToArray());
		string path = Path.Combine(folder, "RemovedVoxels.json");
		File.WriteAllText(path, contents);
	}

	public override void Destroy()
	{
		base.Destroy();
		removedSet.Clear();
	}

	public override void IsNonEmpty(XYZ p, ref bool result)
	{
		if (result && removedSet.Contains(p))
		{
			result = false;
		}
	}

	public override void GetVoxelData(XYZ p, ref Color32 data)
	{
		if (removedSet.Contains(p))
		{
			data = new Color32(0, 0, 0, 0);
		}
	}

	public override void GetPotentialTopVoxels(ICollection<XYZ> result)
	{
		foreach (XYZ point in removedSet.GetPoints())
		{
			result.Add(point - XYZ.up);
		}
	}

	public override void GetPotentialBottomVoxels(ICollection<XYZ> result)
	{
		foreach (XYZ point in removedSet.GetPoints())
		{
			result.Add(point + XYZ.up);
		}
	}

	public override void GetPotentialVisibleVoxels(ICollection<XYZ> result)
	{
		foreach (XYZ point in removedSet.GetPoints())
		{
			for (int i = 0; i < 6; i++)
			{
				result.Add(point + CubeModele.XYZDirs[i]);
			}
		}
	}

	public override void Filling(ITexColors resultTex, ITexColors previousTex, XYZ resultPosition, XYZ resultSize)
	{
		Filling_NegativeLookupCheck(resultTex, previousTex, resultPosition, resultSize);
	}

	public Color32 Filling_NegativeColumn(Color32 c, IntClasse xz)
	{
		byte b = c.r;
		byte b2 = c.g;
		byte b3 = c.b;
		byte b4 = c.a;
		int num = xz.BinarySearch(b2);
		if (num < 0)
		{
			num = ~num;
		}
		int num2 = xz.BinarySearch(b);
		if (num2 < 0)
		{
			num2 = ~num2;
		}
		for (int num3 = num - 1; num3 >= num2; num3--)
		{
			int num4 = xz[num3];
			if (num4 < b2)
			{
				if (num4 == b2 - 1)
				{
					b2--;
				}
				if (num4 < b4 && num4 >= b3)
				{
					b4 = (byte)num4;
				}
			}
		}
		for (int i = num2; i < num; i++)
		{
			int num5 = xz[i];
			if (num5 >= b)
			{
				if (num5 == b)
				{
					b++;
				}
				if (num5 >= b3 && num5 < b4)
				{
					b3 = (byte)(num5 + 1);
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

	public void Filling_NegativeLookupCheck(ITexColors resultTex, ITexColors previousTex, XYZ resultPosition, XYZ resultSize)
	{
		Color32[] pixels = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels);
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 c = (orAllocPixels[i] = pixels[i]);
			if (c.r >= c.g)
			{
				continue;
			}
			int num = i % previousTex.Width;
			int num2 = i / previousTex.Width;
			XZ p = new XZ(num + resultPosition.x, num2 + resultPosition.z);
			IntClasse column = removedSet.GetColumn(p);
			if (column != null)
			{
				if (resultPosition.y != 0)
				{
					c = Layer.TranslateColumn(c, resultPosition.y);
					c = Filling_NegativeColumn(c, column);
					c = Layer.TranslateColumn(c, -resultPosition.y);
					c = Layer.ClampColumn(c, 0, resultSize.y);
				}
				else
				{
					c = Filling_NegativeColumn(c, column);
				}
				orAllocPixels[i] = c;
			}
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public override void InitialData(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		InitialData_RemovedSet(resultTex, offsetTex, previousTex, resultPosition, count);
		InitialData_RemovedGrass(resultTex, offsetTex, resultTex, resultPosition, count);
	}

	public void InitialData_RemovedSet(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels2);
		for (int i = 0; i < count; i++)
		{
			orAllocPixels[i] = pixels2[i];
			XYZ xYZ = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i]));
			if (removedSet.Contains(xYZ + resultPosition))
			{
				orAllocPixels[i] = new Color32(0, 0, 0, 0);
			}
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public void InitialData_RemovedGrass(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels2);
		for (int i = 0; i < count; i++)
		{
			orAllocPixels[i] = pixels2[i];
			if (orAllocPixels[i].r == Sand || !atlas.IsCroute(orAllocPixels[i].r))
			{
				continue;
			}
			XYZ xYZ = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i]));
			for (int j = 0; j < _pelure.Length; j++)
			{
				if (removedSet.Contains(resultPosition + _pelure[j] + xYZ))
				{
					orAllocPixels[i].r = Sand;
				}
			}
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public override LowResLayer NewLowRes()
	{
		return new LowResNegative();
	}
}
