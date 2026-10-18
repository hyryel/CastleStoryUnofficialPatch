using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.External.Signals;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

[JsonObject(MemberSerialization.OptIn)]
public class FillRangeLayer : Layer, ISelectableLayer
{
	public class LowResFillRange : LowResLayer
	{
		private XYZ _scale;

		private XZ _xzScale;

		private FillRangeLayer _parentLayer;

		private SignalConnection _parentAddColumnConnection = SignalConnection.Empty;

		private SignalConnection _parentRemoveColumnConnection = SignalConnection.Empty;

		private SignalConnection _parentClearColumnConnection = SignalConnection.Empty;

		private bool work;

		private HashSet<XZ> dirtyColumns = new HashSet<XZ>();

		private Dictionary<XZ, IntClasse> columns = new Dictionary<XZ, IntClasse>();

		public override void Init(Layer parentLayer, XYZ scale)
		{
			base.Init(parentLayer, scale);
			_parentLayer = (FillRangeLayer)parentLayer;
			_scale = scale;
			_xzScale = new XZ(_scale.x, _scale.z);
			_parentAddColumnConnection = _parentLayer.AddColumnSignal.Connect(OnParentAddColumn);
			_parentRemoveColumnConnection = _parentLayer.RemoveColumnSignal.Connect(OnParentRemoveColumn);
			_parentClearColumnConnection = _parentLayer.ClearColumnSignal.Connect(OnParentClearColumn);
			InitColumns();
		}

		public override void Destroy()
		{
			base.Destroy();
			_parentAddColumnConnection.Disconnect();
			_parentRemoveColumnConnection.Disconnect();
			_parentClearColumnConnection.Disconnect();
			columns.Clear();
			dirtyColumns.Clear();
		}

		private void OnParentAddColumn(XZ p, Color32 c)
		{
			dirtyColumns.Add(XZ.Div(p, _xzScale));
		}

		private void OnParentRemoveColumn(XZ p, Color32 c)
		{
			dirtyColumns.Add(XZ.Div(p, _xzScale));
		}

		private void OnParentClearColumn(XZ p)
		{
			dirtyColumns.Add(XZ.Div(p, _xzScale));
		}

		private void AddFinerColumn(IntClasse ys, Color32 c)
		{
			for (int i = Calcule.Floor(c.r, _scale.y); i <= Calcule.Floor(c.g - 1, _scale.y); i++)
			{
				ys.Ajout(i);
			}
		}

		private void UpdateColumn(XZ q)
		{
			IntClasse value;
			bool flag = columns.TryGetValue(q, out value);
			if (value == null)
			{
				value = new IntClasse();
			}
			else
			{
				value.Clear();
			}
			XZ xZ = XZ.Mul(q, _xzScale);
			for (int i = 0; i < _xzScale.z; i++)
			{
				for (int j = 0; j < _xzScale.x; j++)
				{
					Classeur<Color32> valueOrDefault = _parentLayer.Columns.GetValueOrDefault(xZ + new XZ(j, i));
					if (valueOrDefault != null)
					{
						for (int k = 0; k < valueOrDefault.Count; k++)
						{
							AddFinerColumn(value, valueOrDefault[k]);
						}
					}
				}
			}
			if (flag && value.Count == 0)
			{
				columns.Remove(q);
			}
			else if (!flag && value.Count > 0)
			{
				columns.Add(q, value);
			}
		}

		private void InitColumns()
		{
			columns.Clear();
			Dictionary<XZ, Classeur<Color32>>.Enumerator enumerator = _parentLayer.Columns.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<XZ, Classeur<Color32>> current = enumerator.Current;
				XZ key = XZ.Div(current.Key, _xzScale);
				IntClasse valueOrCreate = columns.GetValueOrCreate(key);
				Classeur<Color32> value = current.Value;
				for (int i = 0; i < value.Count; i++)
				{
					AddFinerColumn(valueOrCreate, value[i]);
				}
			}
		}

		private void UpdateColumns()
		{
			if (dirtyColumns.Count != 0)
			{
				HashSet<XZ>.Enumerator enumerator = dirtyColumns.GetEnumerator();
				while (enumerator.MoveNext())
				{
					UpdateColumn(enumerator.Current);
				}
				dirtyColumns.Clear();
			}
		}

		public override bool IsOccupiedOrUncertain(XYZ p)
		{
			UpdateColumns();
			return columns.GetValueOrDefault(new XZ(p.x, p.z))?.Contains(p.y) ?? false;
		}

		public override IEnumerable<XYZ> GetOccupiedOrUncertain()
		{
			UpdateColumns();
			Dictionary<XZ, IntClasse>.Enumerator e = columns.GetEnumerator();
			while (e.MoveNext())
			{
				XZ xz = e.Current.Key;
				IntClasse ys = e.Current.Value;
				for (int i = 0; i < ys.Count; i++)
				{
					yield return new XYZ(xz.x, ys[i], xz.z);
				}
			}
		}

		public override IEnumerable<XYZ> GetOccupiedOrUncertain(XYZ start, XYZ size)
		{
			UpdateColumns();
			XYZ end = start + size;
			XZ xzStart = new XZ(start.x, start.z);
			XZ xzEnd = new XZ(end.x, end.z);
			Dictionary<XZ, IntClasse>.Enumerator e = columns.GetEnumerator();
			while (e.MoveNext())
			{
				XZ xz = e.Current.Key;
				if (!(xz >= xzStart) || !(xz < xzEnd))
				{
					continue;
				}
				IntClasse ys = e.Current.Value;
				for (int i = 0; i < ys.Count; i++)
				{
					int y = ys[i];
					if (y >= start.y && y < end.y)
					{
						yield return new XYZ(xz.x, y, xz.z);
					}
				}
			}
		}
	}

	private static BrixLogger logger = new BrixLogger();

	public const byte InternalType = byte.MaxValue;

	public Signal<XZ, Color32> AddColumnSignal = new Signal<XZ, Color32>();

	public Signal<XZ, Color32> RemoveColumnSignal = new Signal<XZ, Color32>();

	public Signal<XZ> ClearColumnSignal = new Signal<XZ>();

	private bool locked;

	private bool work;

	[JsonProperty]
	public Dictionary<XZ, Classeur<Color32>> Columns = new Dictionary<XZ, Classeur<Color32>>();

	private byte Earth => (byte)atlas.EarthType;

	public override void Load(string folder)
	{
		string path = Path.Combine(folder, "Columns.json");
		if (!File.Exists(path))
		{
			LoadFromHeightmap(folder);
			return;
		}
		string value = File.ReadAllText(path);
		Dictionary<XZ, Color32[]> source = JsonConvert.DeserializeObject<Dictionary<XZ, Color32[]>>(value);
		Columns = source.ToDictionary((KeyValuePair<XZ, Color32[]> kv) => kv.Key, (KeyValuePair<XZ, Color32[]> kv) => new Classeur<Color32>(kv.Value));
	}

	public override void Save(string folder)
	{
		Dictionary<XZ, Color32[]> value = Columns.ToDictionary((KeyValuePair<XZ, Classeur<Color32>> kv) => kv.Key, (KeyValuePair<XZ, Classeur<Color32>> kv) => kv.Value.ToBuiltin());
		string contents = JsonConvert.SerializeObject(value);
		string path = Path.Combine(folder, "Columns.json");
		File.WriteAllText(path, contents);
	}

	private void LoadFromHeightmap(string folder)
	{
		HeightmapLayer heightmapLayer = new HeightmapLayer();
		heightmapLayer.Load(folder);
		InitFromHeightmap(heightmapLayer.fillingTex);
		heightmapLayer.Destroy();
	}

	public void BeginChanges()
	{
		locked = true;
		work = false;
	}

	public void ApplyChanges()
	{
		locked = false;
	}

	public bool ClearColumn(XZ p)
	{
		bool flag = Columns.Remove(p);
		if (flag)
		{
			ClearColumnSignal.Invoke(p);
		}
		return flag;
	}

	private static bool SetColor(ref Color32 c, Color32 value)
	{
		if (c.r == value.r && c.g == value.g && c.b == value.g && c.a == value.a)
		{
			return false;
		}
		c = value;
		return true;
	}

	private static int FindBottomIndex(Classeur<Color32> col, int bottom)
	{
		int i = 0;
		if (col.Count == 0 || col[col.Count - 1].r < bottom)
		{
			return col.Count;
		}
		for (int num = col.Count >> 1; num > 2; num >>= 1)
		{
			if (col[i + num].r <= bottom)
			{
				i += num;
			}
		}
		for (; col[i].r < bottom; i++)
		{
		}
		return i;
	}

	private static int FindTopIndex(Classeur<Color32> col, int top)
	{
		int i = 0;
		if (col.Count == 0 || col[col.Count - 1].g < top)
		{
			return col.Count;
		}
		for (int num = col.Count >> 1; num > 2; num >>= 1)
		{
			if (col[i + num].g <= top)
			{
				i += num;
			}
		}
		for (; col[i].g < top; i++)
		{
		}
		return i;
	}

	private static bool RemoveOverlappedColumns(Classeur<Color32> col, ref byte top, int i)
	{
		for (int j = i; j < col.Count; j++)
		{
			if (top <= col[j].g)
			{
				if (top >= col[j].r)
				{
					top = col[j].g;
				}
				else
				{
					j--;
				}
				if (j != i)
				{
					col.RemoveRange(i + 1, j - i);
				}
				break;
			}
		}
		if (col[i].g != top)
		{
			col.GetInternalArray()[i].g = top;
			return true;
		}
		return false;
	}

	private static bool AddColumnItem(Classeur<Color32> col, Color32 f)
	{
		if (col.Count == 0)
		{
			col.Add(f);
			return true;
		}
		int num = -1;
		for (int i = 0; i < col.Count; i++)
		{
			if ((i == 0 || f.r > col[i - 1].g) && f.r <= col[i].g)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			col.Add(f);
			return true;
		}
		if (f.g < col[num].r)
		{
			col.Insert(num, f);
			return true;
		}
		int num2 = -1;
		for (int j = num; j < col.Count; j++)
		{
			if ((j == col.Count - 1 || f.g < col[j + 1].r) && f.g >= col[j].r)
			{
				num2 = j;
				break;
			}
		}
		Color32 value = f;
		value.r = (byte)Mathf.Min(f.r, col[num].r);
		value.g = (byte)Mathf.Max(f.g, col[num2].g);
		value.b = ((value.g == f.g) ? f.b : col[num2].b);
		value.a = ((value.g == f.g) ? f.a : col[num2].a);
		if (num2 != num)
		{
			col.RemoveRange(num + 1, num2 - num);
		}
		return SetColor(ref col.GetInternalArray()[num], value);
	}

	private static bool AddColumnItem1(Classeur<Color32> col, Color32 f)
	{
		int i;
		for (i = 0; i < col.Count; i++)
		{
			Color32 color = col[i];
			if (f.r < color.r)
			{
				continue;
			}
			if (f.g <= color.g)
			{
				if (f.g == color.g && (color.b != f.b || color.a != f.a))
				{
					col.GetInternalArray()[i].b = color.b;
					col.GetInternalArray()[i].a = color.a;
					return true;
				}
				return false;
			}
			if (f.r <= color.g)
			{
				return RemoveOverlappedColumns(col, ref f.g, i);
			}
			if (i + 1 >= col.Count)
			{
				col.Add(f);
				return true;
			}
			if (f.g >= col[i + 1].r)
			{
				col.GetInternalArray()[i + 1].r = f.r;
				return RemoveOverlappedColumns(col, ref f.g, i + 1);
			}
			col.Insert(i + 1, f);
			return true;
		}
		if (i == col.Count)
		{
			col.Add(f);
		}
		return true;
	}

	private static bool AddColumnItem2(Classeur<Color32> col, Color32 c)
	{
		byte r = c.r;
		byte g = c.g;
		int num = FindBottomIndex(col, r);
		if (num == col.Count && (num == 0 || col[num - 1].g < r))
		{
			col.Add(c);
			return true;
		}
		if (col[num].r > g && (num == 0 || col[num - 1].g < r))
		{
			col.Insert(num, c);
			return true;
		}
		int num2 = ((col[num].r > r) ? (num - 1) : num);
		if (num2 >= 0 && col[num2].r <= r && col[num2].g >= g)
		{
			if (col[num2].g == g)
			{
				c.r = col[num2].r;
				return SetColor(ref col.GetInternalArray()[num2], c);
			}
			return false;
		}
		for (num2 = num; num2 < col.Count && col[num2].g <= g; num2++)
		{
		}
		if (num2 != num)
		{
			col.RemoveRange(num, num2 - num);
		}
		return SetColor(ref col.GetInternalArray()[num], c);
	}

	private static bool RemoveColumnItem(Classeur<Color32> col, Color32 c)
	{
		byte r = c.r;
		byte g = c.g;
		int num = FindBottomIndex(col, r);
		if (num == col.Count && (num == 0 || col[num - 1].g < r))
		{
			return false;
		}
		if (col[num].r > g && (num == 0 || col[num - 1].g < r))
		{
			return false;
		}
		int num2 = ((col[num].r > r) ? (num - 1) : num);
		if (num2 >= 0 && col[num2].r <= r && col[num2].g >= g)
		{
			if (col[num2].r == r && col[num2].g == g)
			{
				col.RemoveAt(num2);
				return true;
			}
			if (col[num2].r == r)
			{
				col.GetInternalArray()[num2].r = g;
				return true;
			}
			if (col[num2].g == g)
			{
				col.GetInternalArray()[num2].g = r;
				return true;
			}
			col.Insert(num2, new Color32(col[num2].r, r, c.b, c.a));
			col.GetInternalArray()[num2 + 1].r = g;
			return true;
		}
		return SetColor(ref col.GetInternalArray()[num], c);
	}

	public bool SetColumn(XZ p, Color32 c)
	{
		if (!locked)
		{
			logger.Warn("Please use BeginChanges before setting layer data! Ignoring request.");
			return false;
		}
		if (c.r >= c.g)
		{
			return ClearColumn(p);
		}
		Classeur<Color32> valueOrCreate = Columns.GetValueOrCreate(p);
		bool flag = false;
		if (valueOrCreate.Count != 1)
		{
			valueOrCreate.Alloc(1);
			flag = true;
		}
		flag = SetColor(ref valueOrCreate.GetInternalArray()[0], c) || flag;
		work |= flag;
		if (flag)
		{
			ClearColumnSignal.Invoke(p);
			AddColumnSignal.Invoke(p, c);
		}
		return flag;
	}

	public bool AddColumn(XZ p, Color32 c)
	{
		if (!locked)
		{
			logger.Warn("Please use BeginChanges before setting layer data! Ignoring request.");
			return false;
		}
		if (c.r >= c.g)
		{
			return false;
		}
		bool flag = AddColumnItem(Columns.GetValueOrCreate(p), c);
		work |= flag;
		if (flag)
		{
			AddColumnSignal.Invoke(p, c);
		}
		return flag;
	}

	public bool RemoveColumn(XZ p, Color32 c)
	{
		if (!locked)
		{
			logger.Warn("Please use BeginChanges before setting layer data! Ignoring request.");
			return false;
		}
		if (c.r >= c.g)
		{
			return false;
		}
		Classeur<Color32> valueOrDefault = Columns.GetValueOrDefault(p);
		if (valueOrDefault == null)
		{
			return false;
		}
		bool flag = RemoveColumnItem(valueOrDefault, c);
		work |= flag;
		if (valueOrDefault.Count == 0)
		{
			Columns.Remove(p);
		}
		if (flag)
		{
			RemoveColumnSignal.Invoke(p, c);
		}
		return flag;
	}

	public void AddAllColumns(ITexColors newFillingTex)
	{
		Color32[] pixels = newFillingTex.GetPixels32();
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			if (color.r < color.g)
			{
				XZ xZ = new XZ(i % newFillingTex.Width, i / newFillingTex.Width);
				if (AddColumnItem(Columns.GetValueOrCreate(xZ), color))
				{
					AddColumnSignal.Invoke(xZ, color);
				}
			}
		}
	}

	public void CopyToHeightmap(Tex worldTex, int width, int height)
	{
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(worldTex, width, height);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			int x = i % width;
			int z = i / width;
			Classeur<Color32> valueOrDefault = Columns.GetValueOrDefault(new XZ(x, z));
			orAllocPixels[i] = ((valueOrDefault != null && valueOrDefault.Count != 0) ? valueOrDefault[0] : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		}
		worldTex.SetPixels32(orAllocPixels, width, height);
	}

	public override void InitFromHeightmap(ITexColors worldTex)
	{
		base.InitFromHeightmap(worldTex);
		AddAllColumns(worldTex);
	}

	public override void IsNonEmpty(XYZ p, ref bool result)
	{
		if (result)
		{
			return;
		}
		Classeur<Color32> valueOrDefault = Columns.GetValueOrDefault(new XZ(p.x, p.z));
		if (valueOrDefault != null)
		{
			int num = FindTopIndex(valueOrDefault, p.y);
			if (num < valueOrDefault.Count && p.y >= valueOrDefault[num].r && p.y < valueOrDefault[num].g)
			{
				result = true;
			}
		}
	}

	public override void GetVoxelData(XYZ p, ref Color32 data)
	{
		Classeur<Color32> valueOrDefault = Columns.GetValueOrDefault(new XZ(p.x, p.z));
		if (valueOrDefault != null)
		{
			int num = FindTopIndex(valueOrDefault, p.y);
			if (num < valueOrDefault.Count && p.y >= valueOrDefault[num].r && p.y < valueOrDefault[num].g)
			{
				data = GetTerrainData(valueOrDefault[num], p);
			}
		}
	}

	public override void GetPotentialTopVoxels(ICollection<XYZ> result)
	{
		foreach (KeyValuePair<XZ, Classeur<Color32>> column in Columns)
		{
			Classeur<Color32> value = column.Value;
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].g > value[i].r)
				{
					result.Add(new XYZ(column.Key.x, value[i].g - 1, column.Key.z));
				}
			}
		}
	}

	public override void GetPotentialBottomVoxels(ICollection<XYZ> result)
	{
		foreach (KeyValuePair<XZ, Classeur<Color32>> column in Columns)
		{
			Classeur<Color32> value = column.Value;
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].r < value[i].g)
				{
					result.Add(new XYZ(column.Key.x, value[i].r, column.Key.z));
				}
			}
		}
	}

	public override void GetPotentialVisibleVoxels(ICollection<XYZ> result)
	{
		foreach (KeyValuePair<XZ, Classeur<Color32>> column in Columns)
		{
			Classeur<Color32> value = column.Value;
			if (value.Count == 0)
			{
				continue;
			}
			for (int i = 0; i < value.Count; i++)
			{
				Color32 color = value[i];
				if (color.r >= color.g)
				{
					continue;
				}
				result.Add(new XYZ(column.Key.x, color.r, column.Key.z));
				if (color.r != color.g - 1)
				{
					int num = color.r + 1;
					int num2 = color.g - 1;
					for (int j = num; j < num2; j++)
					{
						result.Add(new XYZ(column.Key.x, j, column.Key.z));
					}
					result.Add(new XYZ(column.Key.x, color.g - 1, column.Key.z));
				}
			}
		}
	}

	public override void Filling(ITexColors resultTex, ITexColors previousTex, XYZ resultPosition, XYZ resultSize)
	{
		Color32[] pixels = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			int x = resultPosition.x + i % previousTex.Width;
			int z = resultPosition.z + i / previousTex.Width;
			Classeur<Color32> valueOrDefault = Columns.GetValueOrDefault(new XZ(x, z));
			if (valueOrDefault != null)
			{
				Color32 color = pixels[i];
				if (resultPosition.y != 0)
				{
					color = Layer.TranslateColumn(color, resultPosition.y);
				}
				for (int j = 0; j < valueOrDefault.Count; j++)
				{
					Color32 color2 = valueOrDefault[j];
					color = Layer.Column_AddPositive(new Color32(color2.r, color2.g, color2.r, color2.g), color);
				}
				if (resultPosition.y != 0)
				{
					color = Layer.TranslateColumn(color, -resultPosition.y);
				}
				color = Layer.ClampColumn(color, 0, resultSize.y);
				orAllocPixels[i] = color;
			}
			else
			{
				orAllocPixels[i] = pixels[i];
			}
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public override void InitialData(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels2);
		for (int i = 0; i < count; i++)
		{
			XYZ o = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i])) + resultPosition;
			Classeur<Color32> valueOrDefault = Columns.GetValueOrDefault(new XZ(o.x, o.z));
			if (valueOrDefault != null)
			{
				int num = FindTopIndex(valueOrDefault, o.y);
				if (num < valueOrDefault.Count && o.y >= valueOrDefault[num].r && o.y < valueOrDefault[num].g)
				{
					orAllocPixels[i] = GetTerrainData(valueOrDefault[num], o);
					continue;
				}
			}
			orAllocPixels[i] = pixels2[i];
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	private Color32 GetTerrainData(Color32 f, XYZ o)
	{
		byte r = FlatWorld.InternalType.r;
		byte g = FlatWorld.InternalVolume.r;
		byte b = (byte)Mathf.Min(o.y - f.r + 1, f.g - o.y);
		if (o.y == f.g - 1)
		{
			r = f.b;
			g = (byte)(Mathf.Clamp(255 - f.a, 0, 3) + 1);
		}
		else if (o.y < f.r + 3 || o.y >= f.g - 3)
		{
			r = Earth;
			if (o.y == f.r)
			{
				g = 2;
			}
		}
		return new Color32(r, g, b, 0);
	}

	public override LowResLayer NewLowRes()
	{
		return new LowResFillRange();
	}
}
