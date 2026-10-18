using System.Collections.Generic;
using System.IO;
using Brix.Engine.FlatWorld.SoftAlgos;
using Brix.External.Signals;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

public class HeightmapLayer : SimpleLayer, ISelectableLayer
{
	public class LowResHeightmap : LowResSimple
	{
		private XYZ _scale;

		private HeightmapLayer _parentLayer;

		private SignalConnection _parentBeginChangesConnection = SignalConnection.Empty;

		private SignalConnection _parentApplyChangesConnection = SignalConnection.Empty;

		private SignalConnection _parentFullyDirtyConnection = SignalConnection.Empty;

		private bool locked;

		private bool work;

		private Color32[] pixels;

		private readonly HashSet<XYZ> dirtyMvs = new HashSet<XYZ>();

		public override void Init(Layer parentLayer, XYZ scale)
		{
			base.Init(parentLayer, scale);
			_parentLayer = (HeightmapLayer)parentLayer;
			_scale = scale;
			_parentBeginChangesConnection = _parentLayer.BeginChangesSignal.Connect(OnParentBeginChanges);
			_parentApplyChangesConnection = _parentLayer.ApplyChangesSignal.Connect(OnParentApplyChanges);
			_parentFullyDirtyConnection = _parentLayer.FullyDirtySignal.Connect(OnParentFullyDirty);
			_parentLayer.ParentScale = _scale;
		}

		public override void Destroy()
		{
			base.Destroy();
			_parentBeginChangesConnection.Disconnect();
			_parentApplyChangesConnection.Disconnect();
			_parentFullyDirtyConnection.Disconnect();
		}

		private void OnParentBeginChanges()
		{
			locked = true;
			work = false;
			dirtyMvs.Clear();
			pixels = fillingTex.GetPixels32();
		}

		private void OnParentApplyChanges()
		{
			foreach (XYZ dirtyVoxel in _parentLayer.DirtyVoxels)
			{
				dirtyMvs.Add(XYZ.Div(dirtyVoxel, _scale));
			}
			foreach (XYZ dirtyMv in dirtyMvs)
			{
				SetDirty(dirtyMv);
			}
			locked = false;
			if (work)
			{
				fillingTex.SetPixels32(pixels, fillingTex.Width, fillingTex.Height);
			}
			pixels = null;
		}

		private void SetDirty(XYZ q)
		{
			if (!locked)
			{
				return;
			}
			int num = q.x + q.z * fillingTex.Width;
			Color32 color = pixels[num];
			if (color.r >= color.g)
			{
				color.r = (byte)q.y;
				color.g = (byte)(q.y + 1);
			}
			else if (q.y >= color.g)
			{
				color.g = (byte)(q.y + 1);
			}
			else
			{
				if (q.y >= color.r)
				{
					return;
				}
				color.r = (byte)q.y;
			}
			pixels[num] = color;
			work = true;
		}

		private void OnParentFullyDirty()
		{
			base.Init((Layer)_parentLayer, _scale);
		}
	}

	private static BrixLogger logger = new BrixLogger();

	public const byte InternalType = byte.MaxValue;

	public Signal BeginChangesSignal = new Signal();

	public Signal ApplyChangesSignal = new Signal();

	public Signal FullyDirtySignal = new Signal();

	private bool locked;

	private bool work;

	private Color32[] pixels;

	public readonly List<XYZ> DirtyVoxels = new List<XYZ>();

	public readonly HashSet<XYZ> DirtyMvs = new HashSet<XYZ>();

	public XYZ ParentScale;

	private byte Earth => (byte)atlas.EarthType;

	private byte DefaultDeepType => (byte)atlas.DefaultDeepType;

	public override void Load(string folder)
	{
		string path = Path.Combine(folder, "Heightmap.png");
		if (File.Exists(path))
		{
			Tex tex = new Tex();
			tex.LoadPNG(File.ReadAllBytes(path));
			InitFromHeightmap(tex);
			Tex.Destroy(tex);
		}
	}

	public override void Save(string folder)
	{
		Tex tex = new Tex();
		fillingTex.CopyTo(tex);
		byte[] bytes = tex.EncodeToPNG();
		Tex.Destroy(tex);
		string path = Path.Combine(folder, "Heightmap.png");
		File.WriteAllBytes(path, bytes);
	}

	public void BeginChanges()
	{
		locked = true;
		work = false;
		DirtyVoxels.Clear();
		DirtyMvs.Clear();
		pixels = fillingTex.GetPixels32();
		BeginChangesSignal.Invoke();
	}

	public void ApplyChanges()
	{
		locked = false;
		if (work)
		{
			fillingTex.SetPixels32(pixels, fillingTex.Width, fillingTex.Height);
		}
		pixels = null;
		ApplyChangesSignal.Invoke();
	}

	public bool SetColumn(XZ p, Color32 c)
	{
		if (!locked)
		{
			logger.Warn("Please use BeginChanges before setting layer data! Ignoring request.");
			return false;
		}
		int num = p.x + p.z * fillingTex.Width;
		Color32 c2 = pixels[num];
		if (Soft.EqualsRGBA(c2, c))
		{
			return false;
		}
		pixels[num] = c;
		work = true;
		if (c2.r >= c2.g)
		{
			for (byte b = c.r; b < c.g; b++)
			{
				DirtyVoxels.Add(new XYZ(p.x, b, p.z));
			}
		}
		else if (c.r >= c.g)
		{
			for (byte b2 = c2.r; b2 < c2.g; b2++)
			{
				DirtyVoxels.Add(new XYZ(p.x, b2, p.z));
			}
		}
		else
		{
			int num2 = Mathf.Min(c2.r, c.r);
			int num3 = Mathf.Max(c2.g, c.g);
			for (int i = num2; i < num3; i++)
			{
				if ((i >= c.r && i < c.g) != (i >= c2.r && i < c2.g))
				{
					DirtyVoxels.Add(new XYZ(p.x, i, p.z));
				}
			}
			DirtyVoxels.Add(new XYZ(p.x, c2.g - 1, p.z));
			DirtyVoxels.Add(new XYZ(p.x, c.g - 1, p.z));
		}
		return work;
	}

	public void SetAllColumns(Tex newFillingTex)
	{
		Soft.Copy(fillingTex, newFillingTex);
		FullyDirtySignal.Invoke();
	}

	public override void IsNonEmpty(XYZ p, ref bool result)
	{
		if (!result && p.x >= 0 && p.x < fillingTex.Width && p.z >= 0 && p.z < fillingTex.Height)
		{
			Color32 pixel = fillingTex.GetPixel(p.x, p.z);
			if (p.y >= pixel.r && p.y < pixel.g)
			{
				result = true;
			}
		}
	}

	public override void GetVoxelData(XYZ p, ref Color32 data)
	{
		if (p.x >= 0 && p.x < fillingTex.Width && p.z >= 0 && p.z < fillingTex.Height)
		{
			Color32 pixel = fillingTex.GetPixel(p.x, p.z);
			if (p.y >= pixel.r && p.y < pixel.g)
			{
				data = GetTerrainData(pixel, p);
			}
		}
	}

	public override void GetPotentialTopVoxels(ICollection<XYZ> result)
	{
		Color32[] pixels = fillingTex.GetPixels32();
		int width = fillingTex.Width;
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			if (color.g > color.r)
			{
				result.Add(new XYZ(i % width, color.g - 1, i / width));
			}
		}
	}

	public override void GetPotentialBottomVoxels(ICollection<XYZ> result)
	{
		Color32[] pixels = fillingTex.GetPixels32();
		int width = fillingTex.Width;
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			if (color.r < color.g)
			{
				result.Add(new XYZ(i % width, color.r, i / width));
			}
		}
	}

	public override void GetPotentialVisibleVoxels(ICollection<XYZ> result)
	{
		TexColors texColors = new TexColors();
		new Columns().Process(texColors, fillingTex, 0, 255);
		Color32[] pixels = texColors.GetPixels32();
		int width = texColors.Width;
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			for (byte b = color.r; b < color.g; b++)
			{
				result.Add(new XYZ(i % width, b, i / width));
			}
		}
	}

	public override void Filling(ITexColors resultTex, ITexColors previousTex, XYZ resultPosition, XYZ resultSize)
	{
		int width = fillingTex.Width;
		int num = resultPosition.z * width + resultPosition.x;
		Color32[] pixels = fillingTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels2);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			int num2 = i % resultTex.Width;
			int num3 = i / resultTex.Width;
			int num4 = num + num3 * width + num2;
			if (num4 < 0 || num4 >= pixels.Length)
			{
				orAllocPixels[i] = pixels2[i];
				continue;
			}
			Color32 color = pixels[num4];
			if (color.r >= color.g)
			{
				orAllocPixels[i] = pixels2[i];
				continue;
			}
			Color32 color2 = pixels2[i];
			if (resultPosition.y != 0)
			{
				color2 = Layer.TranslateColumn(color2, resultPosition.y);
			}
			color2 = Layer.Column_AddPositive(new Color32(color.r, color.g, color.r, color.g), color2);
			if (resultPosition.y != 0)
			{
				color2 = Layer.TranslateColumn(color2, -resultPosition.y);
			}
			color2 = Layer.ClampColumn(color2, 0, resultSize.y);
			orAllocPixels[i] = color2;
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public void InitialData_1(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		int width = fillingTex.Width;
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = fillingTex.GetPixels32();
		Color32[] pixels3 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels3);
		for (int i = 0; i < count; i++)
		{
			XYZ o = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i])) + resultPosition;
			int num = o.z * width + o.x;
			if (num < 0 || num >= pixels2.Length)
			{
				orAllocPixels[i] = pixels3[i];
				continue;
			}
			Color32 f = pixels2[num];
			if (o.y < f.r || o.y >= f.g)
			{
				orAllocPixels[i] = pixels3[i];
			}
			else
			{
				orAllocPixels[i] = GetTerrainData(f, o);
			}
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	private Color32 GetTerrainData(Color32 f, XYZ o)
	{
		byte r = FlatWorld.InternalType.r;
		byte g = FlatWorld.InternalVolume.r;
		byte b = (byte)(f.g - o.y);
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

	public override void InitialData(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
		int width = fillingTex.Width;
		int num = resultPosition.z * width + resultPosition.x;
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = fillingTex.GetPixels32();
		Color32[] pixels3 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels3);
		for (int i = 0; i < count; i++)
		{
			orAllocPixels[i] = pixels3[i];
			XYZ xYZ = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i]));
			int num2 = num + xYZ.z * width + xYZ.x;
			if (num2 < 0 || num2 >= pixels2.Length)
			{
				continue;
			}
			Color32 color = pixels2[num2];
			xYZ.y += resultPosition.y;
			if (xYZ.y < color.r || xYZ.y >= color.g)
			{
				continue;
			}
			byte r = FlatWorld.InternalType.r;
			byte g = FlatWorld.InternalVolume.r;
			byte b = (byte)(color.g - xYZ.y);
			if (xYZ.y == color.g - 1)
			{
				r = color.b;
				g = (byte)(Mathf.Clamp(255 - color.a, 0, 3) + 1);
			}
			else if (xYZ.y < color.r + 3 || xYZ.y >= color.g - 3)
			{
				r = Earth;
				if (xYZ.y == color.r)
				{
					g = 2;
				}
			}
			orAllocPixels[i] = new Color32(r, g, b, 0);
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public override LowResLayer NewLowRes()
	{
		return new LowResHeightmap();
	}
}
