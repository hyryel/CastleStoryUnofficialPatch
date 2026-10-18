using System;
using System.Collections.Generic;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

public abstract class Layer
{
	public const byte DefaultVolumeValue = 2;

	public const byte DefaultDepthValue = 1;

	public const byte UnusedAlphaValue = 0;

	public static readonly Color32 EmptyColumn = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public MondeAtlas atlas;

	public Common common;

	public int checksum;

	private static int nextId = 1;

	private LowResLayer _lowRes;

	protected bool initialized;

	private FlatWorld _previewWorld;

	public LowResLayer LowRes => _lowRes ?? (_lowRes = NewLowRes());

	public bool Initialized => initialized;

	public virtual void Load(string folder)
	{
	}

	public virtual void Save(string folder)
	{
	}

	public virtual void Init(MondeAtlas atlas, Common common)
	{
		this.atlas = atlas;
		this.common = common;
		checksum = Mathf.FloorToInt(Calcule.PseudoRandom(nextId++) * 8388607f);
	}

	public virtual void InitFromHeightmap(ITexColors worldTex)
	{
		initialized = true;
	}

	public virtual void AdjustBounds(ref VoxelBounds bounds)
	{
	}

	public VoxelBounds ComputeBounds()
	{
		VoxelBounds bounds = default(VoxelBounds);
		AdjustBounds(ref bounds);
		return bounds;
	}

	public virtual void Destroy()
	{
		initialized = false;
		atlas = null;
		common = null;
		checksum = -1;
		DestroyLowRes();
	}

	public static bool Equals(Layer a, Layer b)
	{
		return a == b;
	}

	public static VoxelBounds GetBoundsFromColumnTex(ITexColors fillingTex)
	{
		Color32[] pixels = fillingTex.GetPixels32();
		VoxelBounds result = default(VoxelBounds);
		bool flag = true;
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			if (color.r < color.g)
			{
				int x = i % fillingTex.Width;
				int z = i / fillingTex.Width;
				XYZ xYZ = new XYZ(x, color.r, z);
				XYZ xYZ2 = new XYZ(x, color.g - 1, z);
				if (flag)
				{
					result = new VoxelBounds(xYZ, xYZ2);
					flag = false;
				}
				else
				{
					result.Encapsulate(xYZ);
					result.Encapsulate(xYZ2);
				}
			}
		}
		return result;
	}

	public static void Filling_AddPositive(ITexColors resultTex, ITexColors previousTex, ITexColors fillingTex)
	{
		Color32[] pixels = fillingTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels2);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			orAllocPixels[i] = Column_AddPositive(pixels[i], pixels2[i]);
		}
		resultTex.SetPixels32(orAllocPixels, fillingTex.Width, fillingTex.Height);
	}

	public static void Filling_AddNegative(Tex resultTex, Tex previousTex, Tex fillingTex)
	{
		Color32[] pixels = fillingTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels2);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			orAllocPixels[i] = Column_AddNegative(pixels[i], pixels2[i]);
		}
		resultTex.SetPixels32(orAllocPixels, fillingTex.width, fillingTex.height);
	}

	public static void Filling_AddMixed(Tex resultTex, Tex previousTex, Tex fillingTex)
	{
		Color32[] pixels = fillingTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Column_AddMixed(pixels[i], pixels2[i]);
		}
		resultTex.SetPixels32(array, fillingTex.width, fillingTex.height);
	}

	public static Color32 Column_AddPositive(Color32 filling, Color32 previous)
	{
		byte b = previous.r;
		byte b2 = previous.g;
		byte b3 = previous.b;
		byte b4 = previous.a;
		byte r = filling.r;
		byte g = filling.g;
		if (b >= b2)
		{
			b = (b3 = r);
			b4 = (b2 = g);
		}
		else if (g < b)
		{
			b = r;
		}
		else if (g < b3)
		{
			if (r <= b)
			{
				b = r;
			}
		}
		else if (g < b4)
		{
			if (r <= b)
			{
				b3 = (b = r);
			}
			else if (r <= b3)
			{
				b3 = r;
			}
		}
		else if (g < b2)
		{
			if (r <= b)
			{
				b3 = (b = r);
				b4 = g;
			}
			else if (r <= b3)
			{
				b3 = r;
				b4 = g;
			}
			else if (r <= b4)
			{
				b4 = g;
			}
		}
		else if (r <= b)
		{
			b3 = (b = r);
			b2 = (b4 = g);
		}
		else if (r > b3)
		{
			b2 = ((r <= b4) ? (b4 = g) : ((r > b2) ? g : g));
		}
		else
		{
			b3 = r;
			b2 = (b4 = g);
		}
		if (b >= b2)
		{
			return EmptyColumn;
		}
		return new Color32(b, b2, b3, b4);
	}

	public static Color32 Column_AddNegative(Color32 filling, Color32 previous)
	{
		byte b = previous.r;
		byte b2 = previous.g;
		byte b3 = previous.b;
		byte b4 = previous.a;
		byte r = filling.r;
		byte g = filling.g;
		if (b < b2 && g > b)
		{
			if (g <= b3)
			{
				if (r <= b)
				{
					b = g;
				}
			}
			else if (g < b4)
			{
				if (r <= b)
				{
					b3 = (b = g);
				}
				else if (r <= b3)
				{
					b3 = g;
				}
				else if (g - b3 < b4 - r)
				{
					b3 = g;
				}
				else
				{
					b4 = r;
				}
			}
			else if (g < b2)
			{
				if (r <= b)
				{
					b4 = (b3 = (b = r));
				}
				else if (r <= b3)
				{
					b4 = (b3 = r);
				}
				else if (r <= b4)
				{
					b4 = r;
				}
			}
			else if (r <= b)
			{
				b2 = (b4 = (b3 = (b = r)));
			}
			else if (r <= b3)
			{
				b2 = (b4 = (b3 = r));
			}
			else if (r <= b4)
			{
				b2 = (b4 = r);
			}
			else if (r <= b2)
			{
				b2 = r;
			}
		}
		if (b >= b2)
		{
			return EmptyColumn;
		}
		return new Color32(b, b2, b3, b4);
	}

	public static Color32 Column_AddMixed(Color32 filling, Color32 previous)
	{
		byte b = previous.r;
		byte b2 = previous.g;
		byte b3 = previous.b;
		byte b4 = previous.a;
		byte r = filling.r;
		byte g = filling.g;
		if (b >= b2)
		{
			b = (b3 = (b4 = r));
			b2 = g;
		}
		else if (g <= b)
		{
			b = r;
		}
		else if (g <= b3)
		{
			if (r <= b)
			{
				b = r;
			}
		}
		else if (g < b4)
		{
			if (r <= b)
			{
				b = r;
				b3 = g;
			}
			else if (r <= b3)
			{
				b3 = g;
			}
			else if (g - b3 < b4 - r)
			{
				b3 = g;
			}
			else
			{
				b4 = r;
			}
		}
		else if (g < b2)
		{
			if (r <= b)
			{
				b4 = (b3 = (b = r));
			}
			else if (r <= b3)
			{
				b4 = (b3 = r);
			}
			else if (r <= b4)
			{
				b4 = r;
			}
		}
		else if (r <= b)
		{
			b4 = (b3 = (b = r));
			b2 = g;
		}
		else if (r <= b3)
		{
			b4 = (b3 = r);
			b2 = g;
		}
		else if (r > b4)
		{
			b2 = ((r > b2) ? g : g);
		}
		else
		{
			b4 = r;
			b2 = g;
		}
		if (b >= b2)
		{
			return EmptyColumn;
		}
		return new Color32(b, b2, b3, b4);
	}

	public static Tex GetTemporaryTranslatedColumnTex_Shaders(Tex resultTex, Tex tex, XYZ start, int width, int height)
	{
		if (start == XYZ.zero && width == tex.width && height == tex.height)
		{
			return tex;
		}
		Shaders.Retarget(resultTex, tex, new Vector2(start.x, start.z), new Vector2(width, height));
		if (start.y != 0)
		{
			Tex tex2 = new Tex();
			Vector4 addIf = Vector4.one * ((float)start.y * -0.003921569f);
			Shaders.AddIfBetween(tex2, resultTex, resultTex, new Color32(0, 0, 0, 0), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), addIf);
			FlatWorld.Swap(resultTex, tex2);
			Tex.Destroy(tex2);
		}
		return resultTex;
	}

	public static ITexColors GetTemporaryTranslatedColumnTex_Soft(ITexColors resultTex, ITexColors tex, XYZ start, int width, int height, bool ignorePartialRanges = false)
	{
		if (start == XYZ.zero && width == tex.Width && height == tex.Height)
		{
			return tex;
		}
		Color32[] pixels = tex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, width, height);
		XYZ xYZ = XYZ.Max(start, XYZ.zero);
		XYZ xYZ2 = xYZ - start;
		int num = Mathf.Clamp(tex.Width - xYZ.x, 0, width - xYZ2.x);
		int num2 = Mathf.Clamp(tex.Height - xYZ.z, 0, height - xYZ2.z);
		Soft.Blit32(pixels, tex.Width, orAllocPixels, width, xYZ.x, xYZ.z, xYZ2.x, xYZ2.z, num, num2);
		if (num < width)
		{
			for (int i = 0; i < num2; i++)
			{
				int num3 = i * width + num;
				for (int j = num; j < width; j++)
				{
					orAllocPixels[num3++] = EmptyColumn;
				}
			}
		}
		if (num2 < height)
		{
			for (int k = num2; k < height; k++)
			{
				int num4 = k * width;
				for (int l = 0; l < width; l++)
				{
					orAllocPixels[num4++] = EmptyColumn;
				}
			}
		}
		if (start.y != 0)
		{
			for (int m = 0; m < orAllocPixels.Length; m++)
			{
				orAllocPixels[m] = TranslateColumn(orAllocPixels[m], -start.y);
			}
		}
		if (ignorePartialRanges)
		{
			for (int n = 0; n < orAllocPixels.Length; n++)
			{
				OverwritePartialRanges(ref orAllocPixels[n]);
			}
		}
		resultTex.SetPixels32(orAllocPixels, width, height);
		return resultTex;
	}

	private static void OverwritePartialRanges(ref Color32 c)
	{
		c.b = c.r;
		c.a = c.g;
	}

	public static Color32 ClampColumn(Color32 c, int yMin, int maxHeight)
	{
		if (c.r >= c.g)
		{
			return c;
		}
		yMin = Mathf.Max(yMin, 0);
		int yMax = Mathf.Min(yMin + maxHeight, 255);
		ClampHeight(ref c.r, yMin, yMax);
		ClampHeight(ref c.g, yMin, yMax);
		ClampHeight(ref c.b, yMin, yMax);
		ClampHeight(ref c.a, yMin, yMax);
		if (c.r >= c.g)
		{
			return EmptyColumn;
		}
		return c;
	}

	private static void ClampHeight(ref byte c, int yMin, int yMax)
	{
		c = (byte)Mathf.Clamp(c, yMin, yMax);
	}

	public static Color32 TranslateColumn(Color32 c, int dy)
	{
		if (c.r >= c.g)
		{
			return c;
		}
		TranslateHeight(ref c.r, dy);
		TranslateHeight(ref c.g, dy);
		TranslateHeight(ref c.b, dy);
		TranslateHeight(ref c.a, dy);
		if (c.r >= c.g)
		{
			c = EmptyColumn;
		}
		return c;
	}

	private static void TranslateHeight(ref byte c, int dy)
	{
		c = (byte)Mathf.Clamp(c + dy, 0, 255);
	}

	public static Color32 ScaleColumn(Color32 c, int scaleY)
	{
		if (c.r >= c.g)
		{
			return EmptyColumn;
		}
		c.r = (byte)Mathf.Clamp(Calcule.Floor(c.r, scaleY), 0, 255);
		c.g = (byte)Mathf.Clamp(Calcule.Ceil(c.g, scaleY), 0, 255);
		c.b = (byte)Mathf.Clamp(Calcule.Ceil(c.b, scaleY), 0, 255);
		c.a = (byte)Mathf.Clamp(Calcule.Floor(c.a, scaleY), 0, 255);
		if (c.b > c.a)
		{
			c.b = c.a;
		}
		return c;
	}

	private static void CanonicalizeColumn(ref byte bottomMin, ref byte topMax, ref byte bottomMax, ref byte topMin)
	{
		if (bottomMin >= topMax)
		{
			bottomMin = EmptyColumn.r;
			bottomMax = EmptyColumn.g;
			topMin = EmptyColumn.b;
			topMax = EmptyColumn.a;
			return;
		}
		if (bottomMax < bottomMin)
		{
			bottomMax = bottomMin;
		}
		if (topMin > topMax)
		{
			topMin = topMax;
		}
		if (bottomMax > topMin)
		{
			bottomMax = topMin;
		}
	}

	public static Color32 CanonicalizeColumn(Color32 c)
	{
		CanonicalizeColumn(ref c.r, ref c.g, ref c.b, ref c.a);
		return c;
	}

	public static void Filling_LowRes(ITexColors resultTex, ITexColors previousTex, XYZ resultPosition, XYZ parentSize, XYZ scale)
	{
		XYZ xYZ = XYZ.Div(parentSize + scale - XYZ.one, scale);
		Filling_LowResXZ(resultTex, previousTex, resultPosition.x, resultPosition.z, xYZ.x, xYZ.z, scale.x, scale.z);
		Filling_LowResY(resultTex, resultTex, resultPosition.y, xYZ.y, scale.y);
	}

	public static IEnumerable<Color32> GetNonEmptyColumns(Color32[] columnColors, int columnStride, int sx0, int sy0, int sx1, int sy1)
	{
		int j = sy0 * columnStride + sx0;
		int skip = columnStride - sx1 + sx0;
		int sy2 = sy0;
		while (sy2 < sy1)
		{
			int sx2 = sx0;
			while (sx2 < sx1)
			{
				Color32 c = columnColors[j];
				if (c.r < c.g)
				{
					yield return c;
				}
				sx2++;
				j++;
			}
			sy2++;
			j += skip;
		}
	}

	public static Color32 GetLowResColumn(IEnumerable<Color32> nonEmptyColumns, int totalColumns)
	{
		Color32 result = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);
		int num = 0;
		foreach (Color32 nonEmptyColumn in nonEmptyColumns)
		{
			if (nonEmptyColumn.r < result.r)
			{
				result.r = nonEmptyColumn.r;
			}
			if (nonEmptyColumn.g > result.g)
			{
				result.g = nonEmptyColumn.g;
			}
			if (nonEmptyColumn.b > result.b)
			{
				result.b = nonEmptyColumn.b;
			}
			if (nonEmptyColumn.a < result.a)
			{
				result.a = nonEmptyColumn.a;
			}
			if (nonEmptyColumn.r > result.b)
			{
				result.b = nonEmptyColumn.r;
			}
			if (nonEmptyColumn.g < result.a)
			{
				result.a = nonEmptyColumn.g;
			}
			num++;
		}
		if (num < totalColumns)
		{
			result.b = (result.a = (byte)((result.r + result.g) / 2));
		}
		return result;
	}

	public static Color32 GetLowResColumn_Maximum(IEnumerable<Color32> nonEmptyColumns, int totalColumns)
	{
		byte b = byte.MaxValue;
		byte b2 = 0;
		foreach (Color32 nonEmptyColumn in nonEmptyColumns)
		{
			if (nonEmptyColumn.r < b)
			{
				b = nonEmptyColumn.r;
			}
			if (nonEmptyColumn.g > b2)
			{
				b2 = nonEmptyColumn.g;
			}
		}
		return new Color32(b, b2, b, b2);
	}

	public static Color32 GetLowResColumn_Minimum(IEnumerable<Color32> nonEmptyColumns, int totalColumns)
	{
		byte b = 0;
		byte b2 = byte.MaxValue;
		foreach (Color32 nonEmptyColumn in nonEmptyColumns)
		{
			if (nonEmptyColumn.b > b)
			{
				b = nonEmptyColumn.b;
			}
			if (nonEmptyColumn.a < b2)
			{
				b2 = nonEmptyColumn.a;
			}
		}
		return new Color32(b, b2, b, b2);
	}

	public static Color32 GetLowResColumn_Columns(IEnumerable<Color32> nonEmptyColumns, int totalColumns)
	{
		byte b = byte.MaxValue;
		byte b2 = 0;
		byte b3 = 0;
		byte b4 = byte.MaxValue;
		foreach (Color32 nonEmptyColumn in nonEmptyColumns)
		{
			if (nonEmptyColumn.r < b)
			{
				b = nonEmptyColumn.r;
			}
			if (nonEmptyColumn.g > b2)
			{
				b2 = nonEmptyColumn.g;
			}
			if (nonEmptyColumn.b > b3)
			{
				b3 = nonEmptyColumn.b;
			}
			if (nonEmptyColumn.a < b4)
			{
				b4 = nonEmptyColumn.a;
			}
		}
		return new Color32(b, b2, b3, b4);
	}

	private static void Filling_LowResXZ(ITexColors resultTex, ITexColors previousTex, int resultX, int resultZ, int resultWidth, int resultHeight, int scaleX, int scaleZ, Func<IEnumerable<Color32>, int, Color32> getLowResColumn)
	{
		Color32[] pixels = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, resultWidth, resultHeight);
		int value = resultZ;
		value = Mathf.Clamp(value, 0, previousTex.Height);
		for (int i = 0; i < resultHeight; i++)
		{
			if (value >= previousTex.Height)
			{
				break;
			}
			int value2 = resultZ + (i + 1) * scaleZ;
			value2 = Mathf.Clamp(value2, 0, previousTex.Height);
			int value3 = resultX;
			value3 = Mathf.Clamp(value3, 0, previousTex.Width);
			for (int j = 0; j < resultWidth; j++)
			{
				if (value3 >= previousTex.Width)
				{
					break;
				}
				int value4 = resultX + (j + 1) * scaleX;
				value4 = Mathf.Clamp(value4, 0, previousTex.Width);
				int num = i * resultWidth + j;
				IEnumerable<Color32> nonEmptyColumns = GetNonEmptyColumns(pixels, previousTex.Width, value3, value, value4, value2);
				Color32 c = getLowResColumn(nonEmptyColumns, (value4 - value3) * (value2 - value));
				orAllocPixels[num] = CanonicalizeColumn(c);
				value3 = value4;
			}
			value = value2;
		}
		resultTex.SetPixels32(orAllocPixels, resultWidth, resultHeight);
	}

	public static void Filling_LowResXZ(ITexColors resultTex, ITexColors previousTex, int resultX, int resultZ, int resultWidth, int resultHeight, int scaleX, int scaleZ)
	{
		Filling_LowResXZ(resultTex, previousTex, resultX, resultZ, resultWidth, resultHeight, scaleX, scaleZ, GetLowResColumn);
	}

	public static void Filling_LowResXZ_Maximum(Tex resultTex, Tex previousTex, int resultX, int resultZ, int resultWidth, int resultHeight, int scaleX, int scaleZ)
	{
		Filling_LowResXZ(resultTex, previousTex, resultX, resultZ, resultWidth, resultHeight, scaleX, scaleZ, GetLowResColumn_Maximum);
	}

	public static void Filling_LowResXZ_Minimum(Tex resultTex, Tex previousTex, int resultX, int resultZ, int resultWidth, int resultHeight, int scaleX, int scaleZ)
	{
		Filling_LowResXZ(resultTex, previousTex, resultX, resultZ, resultWidth, resultHeight, scaleX, scaleZ, GetLowResColumn_Minimum);
	}

	public static void Filling_LowResXZ_MinMax(ITexColors resultTex, ITexColors previousTex, int resultX, int resultZ, int resultWidth, int resultHeight, int scaleX, int scaleZ)
	{
		Filling_LowResXZ(resultTex, previousTex, resultX, resultZ, resultWidth, resultHeight, scaleX, scaleZ, GetLowResColumn_Columns);
	}

	public static void Filling_LowResY(ITexColors resultTex, ITexColors previousTex, int resultY, int sizeY, int scaleY)
	{
		Color32[] pixels = previousTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(resultTex, previousTex, pixels);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			Color32 c = pixels[i];
			c = TranslateColumn(c, -resultY);
			c = ScaleColumn(c, scaleY);
			c = ClampColumn(c, 0, sizeY);
			orAllocPixels[i] = c;
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.Width, previousTex.Height);
	}

	public LowResLayer InitLowRes(XYZ scale, bool forced = false)
	{
		if (_lowRes == null)
		{
			_lowRes = NewLowRes();
		}
		if (forced || !_lowRes.Initialized)
		{
			_lowRes.Init(this, scale);
		}
		return _lowRes;
	}

	public FlatWorld InitPreviewWorld(Common common, int softAlgos = 0, bool forced = false)
	{
		if (_previewWorld == null)
		{
			_previewWorld = new FlatWorld();
		}
		if (forced || _previewWorld.fillingTex.empty)
		{
			Layers layers = new Layers();
			layers.AddLayer(this);
			XYZ zero = XYZ.zero;
			_previewWorld.InitFromLayers(layers, common);
			_previewWorld.InitPreRender(common, zero, 0, softAlgos, force: true);
			_previewWorld.InitRender(common, zero, 0, softAlgos, force: true);
		}
		return _previewWorld;
	}

	public virtual LowResLayer NewLowRes()
	{
		return new LowResLayer();
	}

	public void DestroyLowRes()
	{
		if (_lowRes != null)
		{
			_lowRes.Destroy();
		}
	}

	public virtual void IsNonEmpty(XYZ p, ref bool result)
	{
	}

	public virtual void GetVoxelData(XYZ p, ref Color32 data)
	{
	}

	public virtual void GetPotentialTopVoxels(ICollection<XYZ> result)
	{
	}

	public virtual void GetPotentialBottomVoxels(ICollection<XYZ> result)
	{
	}

	public virtual void GetPotentialVisibleVoxels(ICollection<XYZ> result)
	{
	}

	public virtual void Filling(ITexColors resultTex, ITexColors previousTex, XYZ resultPosition, XYZ resultSize)
	{
	}

	public virtual void InitialData(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
	}

	public virtual void ProcessedData(ITexColors resultTex, ITexColors offsetTex, ITexColors previousTex, XYZ resultPosition, int count)
	{
	}
}
