using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Brix.Engine.FlatWorld.SoftAlgos;
using Brix.Util;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public static class Soft
{
	private class ThreadData
	{
		private Thread _thread;

		private Action<int, int> _work;

		private Action<int, int, object> _work2;

		private int _start;

		private int _count;

		private object _param;

		private int _age;

		private void Start()
		{
			if (_thread == null)
			{
				_thread = new Thread(delegate(object o)
				{
					((ThreadData)o).Work();
				});
				_thread.Start(this);
			}
		}

		public void Start(Action<int, int> work, int start, int count)
		{
			_work = work;
			_start = start;
			_count = count;
			Start();
		}

		public void Start(Action<int, int, object> work2, int start, int count, object o)
		{
			_work2 = work2;
			_param = o;
			_start = start;
			_count = count;
			Start();
		}

		public void Join()
		{
			while (_work != null || _work2 != null)
			{
				Thread.SpinWait(100);
			}
		}

		private void Work()
		{
			while (true)
			{
				if (_work != null)
				{
					_work(_start, _count);
					_work = null;
					_age = 0;
					continue;
				}
				if (_work2 != null)
				{
					_work2(_start, _count, _param);
					_work2 = null;
					_param = null;
					_age = 0;
					continue;
				}
				if (_age >= 1000)
				{
					break;
				}
				_age++;
				Thread.SpinWait(100);
			}
			_thread = null;
		}
	}

	private static BrixLogger logger = new BrixLogger();

	private static Columns _columns = new Columns();

	private static LevelConfigNormalUvs _levelConfigNormalUvs = new LevelConfigNormalUvs();

	public const int DefaultThreadCount = 0;

	private static List<ThreadData> _threads = new List<ThreadData>();

	public static Color32 EncodeInt(int x)
	{
		return new Color32
		{
			r = (byte)(x & 0xFF),
			g = (byte)((x >> 8) & 0xFF),
			b = (byte)((x >> 16) & 0xFF),
			a = (byte)((x >> 24) & 0xFF)
		};
	}

	public static int DecodeInt(Color32 c)
	{
		return c.r | (c.g << 8) | (c.b << 16) | (c.a << 24);
	}

	public static bool EqualsRGBA(Color32 c, Color32 d)
	{
		return c.r == d.r && c.g == d.g && c.b == d.b && c.a == d.a;
	}

	public static XYZ DecodeXYZ(int i)
	{
		return new XYZ(i & 0xFFF, (i >> 24) & 0xFF, (i >> 12) & 0xFFF);
	}

	public static int EncodeXYZ(XYZ p)
	{
		return p.x | (p.z << 12) | (p.y << 24);
	}

	public static Vector3 DecodeVector3(int i)
	{
		return new Vector3(i & 0xFFF, (i >> 24) & 0xFF, (i >> 12) & 0xFFF);
	}

	public static int EncodeVector3(Vector3 p)
	{
		return (int)p.x | ((int)p.z << 12) | ((int)p.y << 24);
	}

	public static Vector3 DecodeNormal(byte i, float xzRange = 1f)
	{
		int num = i % 16;
		int num2 = i / 16;
		float num3 = Mathf.Clamp(((float)num / 14f * 2f - 1f) * xzRange, -1f, 1f);
		float num4 = Mathf.Clamp(((float)num2 / 14f * 2f - 1f) * xzRange, -1f, 1f);
		float y = Mathf.Sqrt(1f - Mathf.Clamp01(num3 * num3 + num4 * num4));
		return new Vector3(num3, y, num4);
	}

	public static byte EncodeNormal(Vector3 n, float xzRange = 1f)
	{
		float num = Mathf.Clamp01(n.x / xzRange * 0.5f + 0.5f);
		float num2 = Mathf.Clamp01(n.z / xzRange * 0.5f + 0.5f);
		int num3 = Mathf.RoundToInt(num * 14f);
		int num4 = Mathf.RoundToInt(num2 * 14f);
		return (byte)(num3 + 16 * num4);
	}

	public static byte AddIfZero(byte b, byte add)
	{
		return (b > 0) ? b : add;
	}

	public static int AddIfZero(int b, int add)
	{
		return (b > 0) ? b : add;
	}

	public static int RepeatIndex(int x, int w)
	{
		int num = x % w;
		return (num < 0) ? (num + w) : num;
	}

	public static int ClampIndex(int x, int w)
	{
		return Mathf.Clamp(x, 0, w - 1);
	}

	public static int GetYOffset(Color32 c, int y)
	{
		if (y < c.r || y >= c.g)
		{
			return -2;
		}
		if (y >= c.b && y < c.a)
		{
			return -1;
		}
		int num = y - c.r;
		if (y >= c.a)
		{
			num -= c.a - c.b;
		}
		return num;
	}

	public static int GetOffset(XYZ p, Color32[] xzColors, Color32[] columnColors, int w, int h)
	{
		p.x = Mathf.Clamp(p.x, 0, w - 1);
		p.z = Mathf.Clamp(p.z, 0, h - 1);
		int num = p.x + p.z * w;
		Color32 c = columnColors[num];
		int yOffset = GetYOffset(c, p.y);
		int num2 = DecodeInt(xzColors[num]);
		return num2 + yOffset;
	}

	public static void GetOffsetCube(int[] res, XYZ start, XYZ size, Color32[] xzColors, Color32[] columnColors, int w, int h)
	{
		int num = start.x + start.z * w;
		int num2 = 0;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				int num3 = num + i + j * w;
				if (start.x + i < 0 || start.x + i >= w || start.z + j < 0 || start.z + j >= h)
				{
					num3 = Mathf.Clamp(start.x, 0, w - size.x) + w * Mathf.Clamp(start.z, 0, h - size.z);
				}
				Color32 c = columnColors[num3];
				int num4 = DecodeInt(xzColors[num3]);
				for (int k = 0; k < size.y; k++)
				{
					int yOffset = GetYOffset(c, start.y + k);
					int num5 = ((yOffset >= 0) ? (num4 + yOffset) : yOffset);
					res[num2++] = num5;
				}
			}
		}
	}

	public static void GetOffsetRubik(int[] iRubik, XYZ p, Color32[] xzColors, Color32[] columnColors, int w, int h)
	{
		GetOffsetCube(iRubik, p - XYZ.one, XYZ.one * 3, xzColors, columnColors, w, h);
	}

	public static Color32 GetOffsetData(int offset, Color32[] colors, Color32 emptyColor, Color32 internalColor)
	{
		return (offset >= 0) ? colors[offset] : ((offset == -1) ? internalColor : emptyColor);
	}

	public static int GetOffsetDataR(int offset, Color32[] colors, int emptyColor, int internalColor)
	{
		return (offset >= 0) ? colors[offset].r : ((offset == -1) ? internalColor : emptyColor);
	}

	public static int GetOffsetCount(Tex xzTex)
	{
		return DecodeInt(xzTex.GetPixel_RT(xzTex.width - 1, xzTex.height - 1));
	}

	public static int GetOffsetSubcount(Tex xzTex, int x, int y, int w, int h)
	{
		int num = 0;
		for (int i = 0; i < h; i++)
		{
			num += DecodeInt(xzTex.GetPixel(x + w - 1, y + i)) - DecodeInt(xzTex.GetPixel(x, y + i));
		}
		return num;
	}

	public static void Blit32(Color32[] src, int src_w, Color32[] dst, int dst_w, int sx, int sy, int dx, int dy, int w, int h)
	{
		int num = sx + src_w * sy;
		int num2 = dx + dst_w * dy;
		int num3 = 0;
		while (num3 < h)
		{
			Array.Copy(src, num, dst, num2, w);
			num3++;
			num += src_w;
			num2 += dst_w;
		}
	}

	public static void Blit(Color[] src, int src_w, Color[] dst, int dst_w, int sx, int sy, int dx, int dy, int w, int h)
	{
		int num = sx + src_w * sy;
		int num2 = dx + dst_w * dy;
		int num3 = 0;
		while (num3 < h)
		{
			Array.Copy(src, num, dst, num2, w);
			num3++;
			num += src_w;
			num2 += dst_w;
		}
	}

	public static void Copy(ITexColors resultTex, ITexColors dataTex)
	{
		resultTex.SetPixels32(dataTex.GetPixels32(), dataTex.Width, dataTex.Height);
	}

	public static void Clear(ITexColors resultTex, Tex dataTex)
	{
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, dataTex.width, dataTex.height);
		Array.Clear(orAllocPixels, 0, orAllocPixels.Length);
		resultTex.SetPixels32(orAllocPixels, dataTex.width, dataTex.height);
	}

	public static void Clear(ITexColors resultTex, int width, int height)
	{
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, width, height);
		Array.Clear(orAllocPixels, 0, orAllocPixels.Length);
		resultTex.SetPixels32(orAllocPixels, width, height);
	}

	public static void Clear(ITexColors resultTex, ITexColors dataTex, Color32 color)
	{
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, dataTex.Width, dataTex.Height);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			orAllocPixels[i] = color;
		}
		resultTex.SetPixels32(orAllocPixels, dataTex.Width, dataTex.Height);
	}

	public static void Clear(ITexColors resultTex, int width, int height, Color32 color)
	{
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, width, height);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			orAllocPixels[i] = color;
		}
		resultTex.SetPixels32(orAllocPixels, width, height);
	}

	public static Color32[] GetOrAllocPixels32(ITexColors resultTex, ITexColors dataTex, Color32[] dataColors)
	{
		if (resultTex == dataTex)
		{
			return dataColors;
		}
		if (resultTex != null && !resultTex.Empty && resultTex.Width * resultTex.Height == dataTex.Width * dataTex.Height)
		{
			return resultTex.GetPixels32();
		}
		return new Color32[dataColors.Length];
	}

	public static Color32[] GetOrAllocPixels32(ITexColors resultTex, int width, int height)
	{
		if (!resultTex.Empty && resultTex.Width * resultTex.Height == width * height)
		{
			return resultTex.GetPixels32();
		}
		return new Color32[width * height];
	}

	public static void GetCustomFillingTex(Tex fillingTex, XYZ[] ps)
	{
		XYZ xYZ = ps[0];
		XYZ xYZ2 = ps[0];
		for (int i = 1; i < ps.Length; i++)
		{
			xYZ = XYZ.Min(xYZ, ps[i]);
			xYZ2 = XYZ.Max(xYZ2, ps[i]);
		}
		XYZ xYZ3 = xYZ2 - xYZ;
		List<int>[,] array = new List<int>[xYZ3.x, xYZ3.z];
		for (int j = 0; j < ps.Length; j++)
		{
			XYZ xYZ4 = ps[j];
			List<int> list = array[xYZ4.x, xYZ4.z] ?? (array[xYZ4.x, xYZ4.z] = new List<int>());
			list.Add(xYZ4.y);
		}
		Func<List<int>, Color32> func = delegate(List<int> list3)
		{
			list3.Sort();
			int num7 = list3[list3.Count - 1];
			int num8 = list3[0];
			int num9 = (num7 - num8) / 2;
			int num10 = num9;
			return new Color32((byte)num8, (byte)num7, (byte)num9, (byte)num10);
		};
		int num = Mathf.Max(xYZ3.x, xYZ3.z);
		int num2 = 1 << Calcule.GetBestSquarePower(num * num);
		XYZ xYZ5 = XYZ.one * num2 - xYZ3 >> 1;
		int num3 = xYZ5.x + xYZ5.z * num2;
		Color32[] array2 = new Color32[num2 * num2];
		for (int num4 = 0; num4 < xYZ3.x; num4++)
		{
			for (int num5 = 0; num5 < xYZ3.z; num5++)
			{
				List<int> list2 = array[num4, num5];
				if (list2 != null)
				{
					int num6 = num3 + num4 + num5 * num2;
					Color32 color = func(list2);
					array2[num6] = color;
				}
			}
		}
		fillingTex.SetPixels32(array2, num2, num2);
	}

	public static void GetCustomOffsetTex(Tex offsetTex, XYZ[] ps, bool alreadySorted = false)
	{
		if (!alreadySorted)
		{
			Array.Sort(ps, (XYZ p, XYZ q) => (p.z != q.z) ? (p.z - q.z) : ((p.x != q.x) ? (p.x - q.x) : (p.y - q.y)));
		}
		GetCustomDataTex(offsetTex, ps, (XYZ p) => EncodeInt(EncodeXYZ(p)));
	}

	public static void GetCustomDataTex(Tex dataTex, XYZ[] ps, Func<XYZ, Color32> getC)
	{
		int num = 1 << Calcule.GetBestSquarePower(ps.Length);
		Color32[] array = new Color32[num * num];
		for (int i = 0; i < ps.Length; i++)
		{
			array[i] = getC(ps[i]);
		}
		dataTex.SetPixels32(array, num, num);
	}

	public static IEnumerable<int> GetNonEmptyOffsets(Tex dataTex)
	{
		Color32[] dataColors = dataTex.GetPixels32();
		for (int i = 0; i < dataColors.Length; i++)
		{
			Color32 data = dataColors[i];
			if (data.r != 0)
			{
				yield return i;
			}
		}
	}

	public static IEnumerable<int> GetNonEmptyOffsets(Tex dataTex, Tex offsetTex, XYZ start, XYZ size)
	{
		Color32[] dataColors = dataTex.GetPixels32();
		Color32[] offsetColors = offsetTex.GetPixels32();
		for (int i = 0; i < dataColors.Length; i++)
		{
			Color32 data = dataColors[i];
			if (data.r != 0)
			{
				XYZ p = DecodeXYZ(DecodeInt(offsetColors[i])) - start;
				if (!(p < XYZ.zero) && !(p >= size))
				{
					yield return i;
				}
			}
		}
	}

	public static IEnumerable<KeyValuePair<int, XYZ>> GetNonEmptyOffsets(Tex dataTex, Tex xzTex, Tex columnTex)
	{
		Color32[] dataColors = dataTex.GetPixels32();
		Color32[] columnColors = columnTex.GetPixels32();
		Color32[] xzColors = xzTex.GetPixels32();
		int iColumn = 0;
		for (int z = 0; z < columnTex.height; z++)
		{
			int x = 0;
			while (x < columnTex.width)
			{
				Color32 c = columnColors[iColumn];
				if (c.r < c.g)
				{
					byte bottom = c.r;
					byte top = c.g;
					byte stop = c.b;
					byte restart = c.a;
					int offset = DecodeInt(xzColors[iColumn]);
					byte y = bottom;
					while (y < stop)
					{
						if (dataColors[offset].r != 0)
						{
							yield return new KeyValuePair<int, XYZ>(offset, new XYZ(x, y, z));
						}
						y++;
						offset++;
					}
					byte y2 = restart;
					while (y2 < top)
					{
						if (dataColors[offset].r != 0)
						{
							yield return new KeyValuePair<int, XYZ>(offset, new XYZ(x, y2, z));
						}
						y2++;
						offset++;
					}
				}
				x++;
				iColumn++;
			}
		}
	}

	public static IEnumerable<KeyValuePair<int, XYZ>> GetNonEmptyOffsets(ITexColors dataTex, ITexColors xzTex, ITexColors columnTex, XYZ start, XYZ size)
	{
		Color32[] dataColors = dataTex.GetPixels32();
		Color32[] columnColors = columnTex.GetPixels32();
		Color32[] xzColors = xzTex.GetPixels32();
		start.x = Mathf.Max(start.x, 0);
		start.z = Mathf.Max(start.z, 0);
		size.x = Mathf.Min(size.x, xzTex.Width - start.x);
		size.z = Mathf.Min(size.z, xzTex.Height - start.z);
		if (size.x <= 0 || size.z <= 0)
		{
			yield break;
		}
		XYZ end = start + size;
		int iColumn = start.x + xzTex.Width * start.z;
		for (int z = start.z; z < end.z; z++)
		{
			int x = start.x;
			while (x < end.x)
			{
				Color32 c = columnColors[iColumn];
				int bottom = Mathf.Max(c.r, start.y);
				int top = Mathf.Min(c.g, start.y + size.y);
				if (bottom < top)
				{
					int stop = Mathf.Max(c.b, start.y);
					int restart = Mathf.Min(c.a, start.y + size.y);
					int baseOffset = DecodeInt(xzColors[iColumn]);
					int offset = baseOffset + (bottom - c.r);
					int y = bottom;
					while (y < stop)
					{
						if (dataColors[offset].r != 0)
						{
							yield return new KeyValuePair<int, XYZ>(offset, new XYZ(x, y, z));
						}
						y++;
						offset++;
					}
					offset = baseOffset + (c.b - c.r) + (restart - c.a);
					int y2 = restart;
					while (y2 < top)
					{
						if (dataColors[offset].r != 0)
						{
							yield return new KeyValuePair<int, XYZ>(offset, new XYZ(x, y2, z));
						}
						y2++;
						offset++;
					}
				}
				x++;
				iColumn++;
			}
			iColumn += xzTex.Width - size.x;
		}
	}

	public static IEnumerable<KeyValuePair<int, XYZ>> GetSurfaceOffsets(Tex dataTex, Tex xzTex, Tex columnTex)
	{
		Color32[] dataColors = dataTex.GetPixels32();
		Color32[] columnColors = columnTex.GetPixels32();
		Color32[] xzColors = xzTex.GetPixels32();
		int iColumn = 0;
		for (int z = 0; z < columnTex.height; z++)
		{
			int x = 0;
			while (x < columnTex.width)
			{
				Color32 c = columnColors[iColumn];
				if (c.r < c.g)
				{
					byte bottom = c.r;
					byte top = c.g;
					byte stop = c.b;
					byte restart = c.a;
					int offset = DecodeInt(xzColors[iColumn]) + (stop - bottom) + (top - 1 - restart);
					int y = top - 1;
					while (y >= restart)
					{
						if (dataColors[offset].r != 0)
						{
							yield return new KeyValuePair<int, XYZ>(offset, new XYZ(x, y, z));
							break;
						}
						y--;
						offset--;
					}
				}
				x++;
				iColumn++;
			}
		}
	}

	public static IEnumerable<KeyValuePair<int, XYZ>> GetSurfaceOffsets(ITexColors dataTex, ITexColors xzTex, ITexColors columnTex, XYZ start, XYZ size)
	{
		Color32[] dataColors = dataTex.GetPixels32();
		Color32[] columnColors = columnTex.GetPixels32();
		Color32[] xzColors = xzTex.GetPixels32();
		start.x = Mathf.Max(start.x, 0);
		start.z = Mathf.Max(start.z, 0);
		size.x = Mathf.Min(size.x, xzTex.Width - start.x);
		size.z = Mathf.Min(size.z, xzTex.Height - start.z);
		if (size.x <= 0 || size.z <= 0)
		{
			yield break;
		}
		XYZ end = start + size;
		int iColumn = start.x + xzTex.Width * start.z;
		for (int z = start.z; z < end.z; z++)
		{
			int x = start.x;
			while (x < end.x)
			{
				Color32 c = columnColors[iColumn];
				int bottom = Mathf.Max(c.r, start.y);
				int top = Mathf.Min(c.g, start.y + size.y);
				if (bottom < top)
				{
					int restart = Mathf.Min(c.a, start.y + size.y);
					int baseOffset = DecodeInt(xzColors[iColumn]);
					int offset = baseOffset + (c.b - c.r) + (restart - c.a) + (top - 1 - restart);
					int y = top - 1;
					while (y >= restart)
					{
						if (dataColors[offset].r != 0)
						{
							yield return new KeyValuePair<int, XYZ>(offset, new XYZ(x, y, z));
							break;
						}
						y--;
						offset--;
					}
				}
				x++;
				iColumn++;
			}
			iColumn += xzTex.Width - size.x;
		}
	}

	public static void GetRotatedAddedTex(Tex resultTex, Tex previousTex, Tex originalTex, Func<XYZ, XYZ> rotate)
	{
	}

	public static int GetInsertedFillingTex(Tex fillingTex, Tex columnTex, XYZ p, bool apply = true)
	{
		Color32 pixel = columnTex.GetPixel(p.x, p.z);
		if ((p.y >= pixel.r && p.y < pixel.b) || (p.y >= pixel.a && p.y < pixel.g))
		{
			return 0;
		}
		pixel = fillingTex.GetPixel(p.x, p.z);
		if (pixel.r >= pixel.g)
		{
			pixel.r = (byte)Mathf.Max(0, p.y);
			pixel.g = (byte)Mathf.Min(255, p.y + 1);
			pixel.b = pixel.r;
			pixel.a = pixel.g;
		}
		else if (p.y < pixel.r)
		{
			pixel.r = (byte)Mathf.Max(0, p.y - 1);
		}
		else if (p.y >= pixel.g)
		{
			pixel.g = (byte)Mathf.Min(255, p.y + 1 + 1);
		}
		else
		{
			if (p.y < pixel.b || p.y >= pixel.a)
			{
				return 0;
			}
			if (p.y - pixel.b < pixel.a - p.y)
			{
				pixel.b = (byte)Mathf.Min(pixel.a, p.y + 1 + 1);
			}
			else
			{
				pixel.a = (byte)Mathf.Max(pixel.b, p.y - 1);
			}
		}
		if (pixel.r >= pixel.g)
		{
			pixel = new Color32(1, 1, 1, 1);
		}
		else
		{
			pixel.b = (byte)Mathf.Max(pixel.b, pixel.r);
			pixel.a = (byte)Mathf.Min(pixel.a, pixel.g);
			pixel.a = (byte)Mathf.Max(pixel.a, pixel.b);
		}
		fillingTex.SetPixel(p.x, p.z, pixel, apply);
		return 1;
	}

	public static int GetRemovedFillingTex(Tex fillingTex, Tex columnTex, XYZ p, bool apply = true)
	{
		Color32 c = fillingTex.GetPixel(p.x, p.z);
		if (p.y < c.r)
		{
			c.r = (byte)Mathf.Max(0, p.y - 1);
		}
		else if (p.y >= c.g)
		{
			c.g = (byte)Mathf.Min(255, p.y + 1 + 1);
		}
		else
		{
			if (p.y < c.b || p.y >= c.a)
			{
				return 0;
			}
			if (p.y - c.b < c.a - p.y)
			{
				c.b = (byte)Mathf.Min(c.a, p.y + 1 + 1);
			}
			else
			{
				c.a = (byte)Mathf.Max(c.b, p.y - 1);
			}
		}
		if (c.r >= c.g)
		{
			c = new Color32(1, 1, 1, 1);
		}
		else
		{
			c.b = (byte)Mathf.Max(c.b, c.r);
			c.a = (byte)Mathf.Min(c.a, c.g);
			c.a = (byte)Mathf.Max(c.a, c.b);
		}
		fillingTex.SetPixel(p.x, p.z, c, apply);
		return 1;
	}

	public static void GetModifiedPixelDataTex(Tex dataTex, Tex xzTex, Tex columnTex, XYZ p, Color32 newC, bool apply = true)
	{
		Color32 pixel = columnTex.GetPixel(p.x, p.z);
		int num = p.y - pixel.r - ((p.y >= pixel.b) ? (pixel.a - pixel.b) : 0);
		pixel = xzTex.GetPixel(p.x, p.z);
		int num2 = DecodeInt(pixel) + num;
		dataTex.SetPixel(num2 % dataTex.width, num2 / dataTex.width, newC, apply);
	}

	public static int GetPartialColumnTex(Tex columnTex, Tex columnFullTex, XYZ start, XYZ size)
	{
		Color32[] pixels = columnFullTex.GetPixels32();
		int num = 0;
		Color32[] array = new Color32[size.x * size.z];
		for (int i = 0; i < size.z; i++)
		{
			int num2 = (i + start.z) * columnFullTex.width + start.x;
			int num3 = i * size.x;
			int num4 = 0;
			while (num4 < size.x)
			{
				int num5 = num4 + start.x;
				int num6 = i + start.z;
				int y = start.y;
				if (num5 >= 0 && num5 < columnFullTex.width && num6 >= 0 && num6 < columnFullTex.height && y >= 0 && y < 256)
				{
					Color32 color = pixels[num2];
					int r = color.r;
					int g = color.g;
					int b = color.b;
					int a = color.a;
					r = Mathf.Clamp(r, start.y, start.y + size.y) - start.y;
					g = Mathf.Clamp(g, start.y, start.y + size.y) - start.y;
					b = Mathf.Clamp(b, start.y, start.y + size.y) - start.y;
					a = Mathf.Clamp(a, start.y, start.y + size.y) - start.y;
					array[num3] = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
					if (r < g && (r < b || a < g))
					{
						num += b - r + (g - a);
					}
				}
				num4++;
				num2++;
				num3++;
			}
		}
		columnTex.SetPixels32(array, size.x, size.z);
		return num;
	}

	public static void GetPartialData(Tex resultTex, Tex offsetTex, XYZ start, Tex xzFullTex, Tex columnFullTex, Tex dataFullTex)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = columnFullTex.GetPixels32();
		Color32[] pixels3 = xzFullTex.GetPixels32();
		Color32[] pixels4 = dataFullTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 c = pixels[i];
			XYZ xYZ = DecodeXYZ(DecodeInt(c)) + start;
			if (xYZ.x >= 0 && xYZ.x < columnFullTex.width && xYZ.z >= 0 && xYZ.z < columnFullTex.height && xYZ.y >= 0 && xYZ.y < 256)
			{
				c = pixels2[xYZ.x + xYZ.z * columnFullTex.width];
				int r = c.r;
				int g = c.g;
				int b = c.b;
				int a = c.a;
				if (xYZ.y >= r && xYZ.y < g && (xYZ.y < b || xYZ.y >= a))
				{
					int num = xYZ.y - r - ((xYZ.y >= b) ? (a - b) : 0);
					c = pixels3[xYZ.x + xYZ.z * xzFullTex.width];
					int num2 = DecodeInt(c) + num;
					array[i] = pixels4[num2];
				}
			}
		}
		resultTex.SetPixels32(array, offsetTex.width, offsetTex.height);
	}

	public static bool GetMinimalColumnData(Tex resultTex, Tex columnTex, Tex xzTex, Tex configTex, bool debug = false)
	{
		Color32[] pixels = columnTex.GetPixels32();
		Color32[] pixels2 = xzTex.GetPixels32();
		Color32[] pixels3 = configTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			Color32 color = pixels[i];
			if (color.r < color.g)
			{
				num += color.b - color.r + (color.g - color.a);
				int num3 = DecodeInt(pixels2[i]);
				int num4 = num3 + (color.b - color.r);
				int num5 = num4 - 1;
				while (num5 >= num3 && pixels3[num5].r == 0)
				{
					color.b--;
					num5--;
				}
				int num6 = num4 + (color.g - color.a);
				for (int j = num4; j < num6 && pixels3[j].r == 0; j++)
				{
					color.a++;
				}
				int num7 = num3 + (color.b - color.r);
				for (int k = num3; k < num7 && pixels3[k].r == 0; k++)
				{
					color.r++;
				}
				int num8 = num6 - (color.g - color.a);
				int num9 = num6 - 1;
				while (num9 >= num8 && pixels3[num9].r == 0)
				{
					color.g--;
					num9--;
				}
				num2 += color.b - color.r + (color.g - color.a);
				array[i] = color;
			}
		}
		resultTex.SetPixels32(array, columnTex.width, columnTex.height);
		bool flag = num != num2;
		if (debug && flag)
		{
			logger.Info("GetMinimalColumnData() reduced from " + num + " to " + num2 + " voxels");
		}
		return flag;
	}

	public static bool GetMinimalFillingData(Tex resultTex, Tex fillingTex, Tex columnTex, bool debug = false)
	{
		Color32[] pixels = columnTex.GetPixels32();
		Color32[] pixels2 = fillingTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			Color32 color = pixels[i];
			if (color.r < color.g)
			{
				Color32 color2 = pixels2[i];
				num += color2.b - color2.r + (color2.g - color2.a);
				color2.r = (byte)Mathf.Max(color2.r, color.r);
				color2.g = (byte)Mathf.Min(color2.g, color.g);
				color2.b = (byte)Mathf.Min(color2.b, color.b);
				color2.a = (byte)Mathf.Max(color2.a, color.a);
				if (color2.r < color2.g)
				{
					color2.b = (byte)Mathf.Max(color2.b, color2.r);
					color2.a = (byte)Mathf.Min(color2.a, color2.g);
					color2.a = (byte)Mathf.Max(color2.a, color2.b);
					num2 += color2.b - color2.r + (color2.g - color2.a);
					array[i] = color2;
				}
			}
		}
		resultTex.SetPixels32(array, columnTex.width, columnTex.height);
		bool flag = num != num2;
		if (debug && flag)
		{
			logger.Info("GetMinimalFillingData() reduced from " + num + " to " + num2 + " voxels");
		}
		return flag;
	}

	public static bool GetHeightmapedFillingData(Tex resultTex, Tex fillingTex, Tex xzTex, Tex columnTex, Tex dataTex, bool debug = false)
	{
		Color32[] pixels = fillingTex.GetPixels32();
		Color32[] pixels2 = xzTex.GetPixels32();
		Color32[] pixels3 = columnTex.GetPixels32();
		Color32[] pixels4 = dataTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			Color32 color = pixels3[i];
			if (color.r >= color.g)
			{
				array[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				continue;
			}
			num += color.b - color.r + (color.g - color.a);
			int num3 = DecodeInt(pixels2[i]);
			int num4 = num3 + (color.b - color.r);
			int num5 = num4 + (color.g - color.a);
			int num6 = num3 + (color.b - color.r);
			for (int j = num3; j < num6 && pixels4[j].r == 0; j++)
			{
				color.r++;
			}
			int num7 = num5 - (color.g - color.a);
			int num8 = num5 - 1;
			while (num8 >= num7 && pixels4[num8].r == 0)
			{
				color.g--;
				num8--;
			}
			color.b = color.r;
			color.a = color.g;
			num2 += color.b - color.r + (color.g - color.a);
			array[i] = color;
		}
		resultTex.SetPixels32(array, fillingTex.width, fillingTex.height);
		bool flag = num != num2;
		if (debug && flag)
		{
			logger.Info("GetHeightmapedFillingData() reduced from " + num + " to " + num2 + " voxels");
		}
		return flag;
	}

	public static void GetHeightmap(Tex resultTex, Tex fillingTex, Tex xzTex, Tex columnTex, Tex typeTex, Tex volumeTex)
	{
		Color32[] pixels = fillingTex.GetPixels32();
		Color32[] pixels2 = xzTex.GetPixels32();
		Color32[] pixels3 = columnTex.GetPixels32();
		Color32[] pixels4 = typeTex.GetPixels32();
		Color32[] pixels5 = volumeTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Color32 color = pixels3[i];
			if (color.r < color.g)
			{
				int num = color.b - color.r + (color.g - color.a);
				int num2 = DecodeInt(pixels2[i]) + num - 1;
				byte r = pixels4[num2].r;
				byte r2 = pixels5[num2].r;
				if (r != 0 && r2 != 0 && r != FlatWorld.InternalType.r && r2 != FlatWorld.InternalVolume.r)
				{
					r2 = (byte)Mathf.Clamp(256 - r2, 0, 255);
					array[i] = new Color32(color.r, color.g, r, r2);
					continue;
				}
			}
			array[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		resultTex.SetPixels32(array, fillingTex.width, fillingTex.height);
	}

	private static bool IsOnTexEdge(int width, int height, int i)
	{
		int num = i % width;
		int num2 = i / width;
		return num == 0 || num == width - 1 || num2 == 0 || num2 == height - 1;
	}

	private static bool IsEmptyColumn(Color32 c)
	{
		return c.r >= c.g;
	}

	private static int GetVolumeDifference(Color32 c, Color32 d)
	{
		return (c.g < d.g) ? 1 : ((c.g > d.g) ? (-1) : 0);
	}

	private static int GetDefaultColumnVolume(Color32[] worldColors, int width, int height, int i)
	{
		if (IsOnTexEdge(width, height, i))
		{
			return 0;
		}
		Color32 c = worldColors[i];
		if (IsEmptyColumn(c))
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 4;
		int num4 = 3;
		int num5 = 2;
		num += GetVolumeDifference(c, worldColors[i + 1]) * num3;
		num += GetVolumeDifference(c, worldColors[i - 1]) * num3;
		num += GetVolumeDifference(c, worldColors[i + width]) * num3;
		num += GetVolumeDifference(c, worldColors[i - width]) * num3;
		num2 += num3 * 4;
		num += GetVolumeDifference(c, worldColors[i + width + 1]) * num4;
		num += GetVolumeDifference(c, worldColors[i + width - 1]) * num4;
		num += GetVolumeDifference(c, worldColors[i - width + 1]) * num4;
		num += GetVolumeDifference(c, worldColors[i - width - 1]) * num4;
		num2 += num4 * 4;
		num += GetVolumeDifference(c, worldColors[i + 2]) * num5;
		num += GetVolumeDifference(c, worldColors[i - 2]) * num5;
		num += GetVolumeDifference(c, worldColors[i + width * 2]) * num5;
		num += GetVolumeDifference(c, worldColors[i - width * 2]) * num5;
		num2 += num5 * 4;
		return Mathf.Clamp(1 + 4 * num / num2, 0, 3);
	}

	public static void GetDefaultVolumeTex(Tex resultTex, Tex worldTex)
	{
		Color32[] pixels = worldTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		for (int i = 0; i < array.Length; i++)
		{
			byte b = (byte)GetDefaultColumnVolume(pixels, worldTex.width, worldTex.height, i);
			array[i] = new Color32(b, b, b, b);
		}
		resultTex.SetPixels32(array, worldTex.width, worldTex.height);
	}

	public static float InclinationToBottom(XYZ p, XYZ top, float minInclination, float maxInclination, float maxDistance, AnimationCurve curve)
	{
		float time = Mathf.Sqrt(XYZ.SqrMagnitude(p - top)) / maxDistance;
		float t = curve.Evaluate(time);
		return Mathf.Lerp(minInclination, maxInclination, t);
	}

	private static void PropagateBottom(Color32[] worldColors, int i, int da, int db, float[] diags, XYZ[] tops, int[] borders, int w, Func<XYZ, XYZ, float> inclinationFunc)
	{
		if (!IsEmptyColumn(worldColors[i]))
		{
			float num = diags[i + da];
			float num2 = diags[i + db];
			if (num == 0f || num2 == 0f)
			{
				tops[i] = new XYZ(i % w, borders[i] - 1, i / w);
				diags[i] = (float)tops[i].y - inclinationFunc(tops[i], tops[i]);
			}
			else if (num > num2)
			{
				tops[i] = tops[i + da];
				XYZ arg = new XYZ((i + da) % w, Mathf.RoundToInt(num), (i + da) / w);
				diags[i] = num - inclinationFunc(arg, tops[i]);
			}
			else
			{
				tops[i] = tops[i + db];
				XYZ arg2 = new XYZ((i + db) % w, Mathf.RoundToInt(num2), (i + db) / w);
				diags[i] = num2 - inclinationFunc(arg2, tops[i]);
			}
		}
	}

	public static void GetDefaultBottomTex(Tex resultTex, Tex worldTex, Func<XYZ, XYZ, float> inclinationFunc = null, int topMax = 255, int minThickness = 3, bool bottomOnly = false)
	{
		topMax = Mathf.Clamp(topMax, 1, 255);
		Color32[] pixels = worldTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		int width = worldTex.width;
		if (inclinationFunc == null)
		{
			AnimationCurve curve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			inclinationFunc = (XYZ p, XYZ top) => InclinationToBottom(p, top, 0.5f, 2f, 20f, curve);
		}
		int[] array2 = new int[pixels.Length];
		for (int num = 1 + width; num < array.Length - 1 - width; num++)
		{
			if (!IsEmptyColumn(pixels[num]) && (IsEmptyColumn(pixels[num - 1]) || IsEmptyColumn(pixels[num + 1]) || IsEmptyColumn(pixels[num - width]) || IsEmptyColumn(pixels[num + width])))
			{
				array2[num] = Mathf.Min(pixels[num].g - minThickness, topMax);
			}
		}
		int[] array3 = new int[pixels.Length];
		for (int num2 = 1 + width; num2 < array.Length - 1 - width; num2++)
		{
			if (array2[num2] != 0)
			{
				array3[num2] = (byte)Mathf.Max(array2[num2], Mathf.Max(Mathf.Max(array2[num2 - 1] - 1, array2[num2 + 1] - 1), Mathf.Max(array2[num2 - width] - 1, array2[num2 + width] - 1)));
			}
		}
		float[] array4 = new float[pixels.Length];
		XYZ[] tops = new XYZ[pixels.Length];
		for (int num3 = 1 + worldTex.width; num3 < array.Length; num3++)
		{
			PropagateBottom(pixels, num3, -1, -width, array4, tops, array3, width, inclinationFunc);
		}
		float[] array5 = new float[pixels.Length];
		XYZ[] tops2 = new XYZ[pixels.Length];
		for (int num4 = 1; num4 < worldTex.height; num4++)
		{
			for (int num5 = worldTex.width - 2; num5 >= 0; num5--)
			{
				int i = num4 * worldTex.width + num5;
				PropagateBottom(pixels, i, 1, -width, array5, tops2, array3, width, inclinationFunc);
			}
		}
		float[] array6 = new float[pixels.Length];
		XYZ[] tops3 = new XYZ[pixels.Length];
		for (int num6 = worldTex.height - 2; num6 >= 0; num6--)
		{
			for (int num7 = 1; num7 < worldTex.width; num7++)
			{
				int i2 = num6 * worldTex.width + num7;
				PropagateBottom(pixels, i2, -1, width, array6, tops3, array3, width, inclinationFunc);
			}
		}
		float[] array7 = new float[pixels.Length];
		XYZ[] tops4 = new XYZ[pixels.Length];
		for (int num8 = array.Length - 1 - 1 - worldTex.width; num8 >= 0; num8--)
		{
			PropagateBottom(pixels, num8, 1, width, array7, tops4, array3, width, inclinationFunc);
		}
		int[] array8 = new int[pixels.Length];
		for (int num9 = 0; num9 < array.Length; num9++)
		{
			float f = Mathf.Max(Mathf.Max(array4[num9], array5[num9]), Mathf.Max(array6[num9], array7[num9]));
			array8[num9] = Mathf.RoundToInt(f);
		}
		for (int num10 = 0; num10 < array.Length; num10++)
		{
			if (IsEmptyColumn(pixels[num10]))
			{
				continue;
			}
			Color32 color = pixels[num10];
			color.r = (byte)Mathf.Clamp(array8[num10], 1, 255);
			if (color.r > color.g - minThickness)
			{
				color.r = (byte)Mathf.Clamp(color.g - minThickness, 1, 255);
				if (color.r > color.g - minThickness)
				{
					color.r = pixels[num10].r;
				}
			}
			if (bottomOnly)
			{
				color.g = (color.b = (color.a = color.r));
			}
			array[num10] = color;
		}
		resultTex.SetPixels32(array, worldTex.width, worldTex.height);
	}

	public static void GetDefaultBottomTex_2(Tex resultTex, Tex worldTex, bool bottomOnly = false)
	{
		Color32[] pixels = worldTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		Color32[] array2 = new Color32[pixels.Length];
		for (int i = 1 + worldTex.width; i < array.Length; i++)
		{
			if (!IsEmptyColumn(pixels[i]))
			{
				byte a = (byte)(array2[i - 1].r + 1);
				byte b = (byte)(array2[i - worldTex.width].r + 1);
				byte r = (byte)Mathf.Min(a, b);
				array2[i].r = r;
			}
		}
		for (int j = 1; j < worldTex.height; j++)
		{
			for (int num = worldTex.width - 2; num >= 0; num--)
			{
				int num2 = j * worldTex.width + num;
				if (!IsEmptyColumn(pixels[num2]))
				{
					byte a2 = (byte)(array2[num2 + 1].g + 1);
					byte b2 = (byte)(array2[num2 - worldTex.width].g + 1);
					byte g = (byte)Mathf.Min(a2, b2);
					array2[num2].g = g;
				}
			}
		}
		for (int num3 = worldTex.height - 2; num3 >= 0; num3--)
		{
			for (int k = 1; k < worldTex.width; k++)
			{
				int num4 = num3 * worldTex.width + k;
				if (!IsEmptyColumn(pixels[num4]))
				{
					byte a3 = (byte)(array2[num4 - 1].b + 1);
					byte b3 = (byte)(array2[num4 + worldTex.width].b + 1);
					byte b4 = (byte)Mathf.Min(a3, b3);
					array2[num4].b = b4;
				}
			}
		}
		for (int num5 = array.Length - 1 - 1 - worldTex.width; num5 >= 0; num5--)
		{
			if (!IsEmptyColumn(pixels[num5]))
			{
				byte a4 = (byte)(array2[num5 + 1].a + 1);
				byte b5 = (byte)(array2[num5 + worldTex.width].a + 1);
				byte a5 = (byte)Mathf.Min(a4, b5);
				array2[num5].a = a5;
			}
		}
		for (int l = 0; l < array.Length; l++)
		{
			Color32 color = array2[l];
			byte b6 = (byte)Mathf.Min(color.r, color.g, color.b, color.a);
			array[l] = new Color32(b6, b6, b6, b6);
		}
		int num6 = 100;
		for (int m = 0; m < array.Length; m++)
		{
			Color32 color2 = array[m];
			byte b7 = 100;
			int num7 = 2 + color2.r * num6 / 100;
			byte b8 = (byte)Mathf.Clamp(b7 - num7, 0, 255);
			color2 = (bottomOnly ? new Color32(b8, b8, b8, b8) : new Color32(b8, b7, pixels[m].b, pixels[m].a));
			array[m] = color2;
		}
		resultTex.SetPixels32(array, worldTex.width, worldTex.height);
	}

	private static int Distance2(int i, int j, int width)
	{
		int num = i % width;
		int num2 = i / width;
		int num3 = j % width;
		int num4 = j / width;
		int num5 = num - num3;
		int num6 = num2 - num4;
		return num5 * num5 + num6 * num6;
	}

	private static bool CompareSlope(Color32[] diagColors, int i, int j, int k, int width)
	{
		byte r = diagColors[i].r;
		byte r2 = diagColors[j].r;
		byte r3 = diagColors[k].r;
		int num = r - r3;
		int num2 = r2 - r3;
		int num3 = Distance2(i, k, width) + 1;
		int num4 = Distance2(j, k, width) + 1;
		return Mathf.Sign(num) * (float)num * (float)num * (float)num4 > Mathf.Sign(num2) * (float)num2 * (float)num2 * (float)num3;
	}

	private static void UpdateMaxSlope(Color32[] diagColors, int i, int j, int k, int width, ref int bestSoFar)
	{
		int num = (CompareSlope(diagColors, i, j, k, width) ? i : j);
		int num2 = (CompareSlope(diagColors, num, bestSoFar, k, width) ? num : bestSoFar);
		bestSoFar = num2;
	}

	public static void GetDefaultBottomTex_1(Tex resultTex, Tex worldTex)
	{
		Color32[] pixels = worldTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		Color32[] array2 = new Color32[pixels.Length];
		for (int i = 1 + worldTex.width; i < array.Length; i++)
		{
			if (!IsEmptyColumn(pixels[i]))
			{
				byte a = (byte)(array2[i - 1].r + 1);
				byte b = (byte)(array2[i - worldTex.width].r + 1);
				array2[i].r = (byte)Mathf.Min(a, b);
			}
		}
		for (int j = 1; j < worldTex.height; j++)
		{
			for (int num = worldTex.width - 2; num >= 0; num--)
			{
				int num2 = j * worldTex.width + num;
				if (!IsEmptyColumn(pixels[num2]))
				{
					byte a2 = (byte)(array2[num2 + 1].g + 1);
					byte b2 = (byte)(array2[num2 - worldTex.width].g + 1);
					array2[num2].g = (byte)Mathf.Min(a2, b2);
				}
			}
		}
		for (int num3 = worldTex.height - 2; num3 >= 0; num3--)
		{
			for (int k = 1; k < worldTex.width; k++)
			{
				int num4 = num3 * worldTex.width + k;
				if (!IsEmptyColumn(pixels[num4]))
				{
					byte a3 = (byte)(array2[num4 - 1].b + 1);
					byte b3 = (byte)(array2[num4 + worldTex.width].b + 1);
					array2[num4].b = (byte)Mathf.Min(a3, b3);
				}
			}
		}
		for (int num5 = array.Length - 1 - 1 - worldTex.width; num5 >= 0; num5--)
		{
			if (!IsEmptyColumn(pixels[num5]))
			{
				byte a4 = (byte)(array2[num5 + 1].a + 1);
				byte b4 = (byte)(array2[num5 + worldTex.width].a + 1);
				array2[num5].a = (byte)Mathf.Min(a4, b4);
			}
		}
		int[] array3 = new int[pixels.Length];
		int[] array4 = new int[pixels.Length];
		int[] array5 = new int[pixels.Length];
		int[] array6 = new int[pixels.Length];
		int[] array7 = new int[pixels.Length];
		for (int l = 0; l < array.Length; l++)
		{
			Color32 color = array2[l];
			byte b5 = (byte)Mathf.Min(color.r, color.g, color.b, color.a);
			array2[l] = new Color32(b5, b5, b5, b5);
			array3[l] = l;
			array4[l] = l;
			array5[l] = l;
			array6[l] = l;
			array7[l] = l;
		}
		for (int m = 1 + worldTex.width; m < array.Length; m++)
		{
			if (!IsEmptyColumn(pixels[m]))
			{
				int i2 = array3[m - 1];
				int j2 = array3[m - worldTex.width];
				UpdateMaxSlope(array2, i2, j2, m, worldTex.width, ref array3[m]);
			}
		}
		for (int n = 1; n < worldTex.height; n++)
		{
			for (int num6 = worldTex.width - 2; num6 >= 0; num6--)
			{
				int num7 = n * worldTex.width + num6;
				if (!IsEmptyColumn(pixels[num7]))
				{
					int i3 = array4[num7 + 1];
					int j3 = array4[num7 - worldTex.width];
					UpdateMaxSlope(array2, i3, j3, num7, worldTex.width, ref array4[num7]);
				}
			}
		}
		for (int num8 = worldTex.height - 2; num8 >= 0; num8--)
		{
			for (int num9 = 1; num9 < worldTex.width; num9++)
			{
				int num10 = num8 * worldTex.width + num9;
				if (!IsEmptyColumn(pixels[num10]))
				{
					int i4 = array5[num10 - 1];
					int j4 = array5[num10 + worldTex.width];
					UpdateMaxSlope(array2, i4, j4, num10, worldTex.width, ref array5[num10]);
				}
			}
		}
		for (int num11 = array.Length - 1 - 1 - worldTex.width; num11 >= 0; num11--)
		{
			if (!IsEmptyColumn(pixels[num11]))
			{
				int i5 = array6[num11 + 1];
				int j5 = array6[num11 + worldTex.width];
				UpdateMaxSlope(array2, i5, j5, num11, worldTex.width, ref array6[num11]);
			}
		}
		for (int num12 = 0; num12 < array.Length; num12++)
		{
			if (!IsEmptyColumn(pixels[num12]))
			{
				array7[num12] = array3[num12];
				if (CompareSlope(array2, array3[num12], array7[num12], num12, worldTex.width))
				{
					array7[num12] = array3[num12];
				}
				if (CompareSlope(array2, array4[num12], array7[num12], num12, worldTex.width))
				{
					array7[num12] = array4[num12];
				}
				if (CompareSlope(array2, array5[num12], array7[num12], num12, worldTex.width))
				{
					array7[num12] = array5[num12];
				}
				if (CompareSlope(array2, array6[num12], array7[num12], num12, worldTex.width))
				{
					array7[num12] = array6[num12];
				}
			}
		}
		for (int num13 = 0; num13 < array.Length; num13++)
		{
			int num14 = num13 % worldTex.width;
			int num15 = num13 / worldTex.width;
			int num16 = array7[num13] % worldTex.width;
			int num17 = array7[num13] / worldTex.width;
			int value = num14 - num16;
			int value2 = num15 - num17;
			byte b6 = (byte)Mathf.Clamp(Mathf.Abs(value) + Mathf.Abs(value2), 0, 255);
			array[num13] = new Color32(b6, b6, b6, byte.MaxValue);
		}
		resultTex.SetPixels32(array, worldTex.width, worldTex.height);
		File.WriteAllBytes("Assets/Tests/resultTex.png", resultTex.EncodeToPNG());
		int num18 = 100;
		for (int num19 = 0; num19 < array.Length; num19++)
		{
			Color32 color2 = array[num19];
			byte g = pixels[num19].g;
			int num20 = color2.r * num18 / 100;
			byte b7 = (byte)Mathf.Clamp(g - num20, 0, 255);
			array[num19] = new Color32(b7, b7, b7, b7);
		}
		resultTex.SetPixels32(array, worldTex.width, worldTex.height);
	}

	public static void GetDefaultBottomTex_2(Tex resultTex, Tex worldTex)
	{
		Color32[] pixels = worldTex.GetPixels32();
		Color32[] array = new Color32[pixels.Length];
		Color32[] array2 = new Color32[pixels.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Color32 c = pixels[i];
			array2[i] = new Color32(0, 0, 0, 0);
			if (!IsEmptyColumn(c))
			{
				byte b = (byte)Mathf.Clamp(c.g - 2, 0, 255);
				array2[i] = new Color32(b, b, b, b);
			}
		}
		for (int j = 1 + worldTex.width; j < array.Length; j++)
		{
			if (!IsEmptyColumn(pixels[j]))
			{
				byte b2 = (byte)Mathf.Clamp(array2[j - 1].r - 1, 0, 255);
				byte b3 = (byte)Mathf.Clamp(array2[j - worldTex.width].r - 1, 0, 255);
				if (b2 != 0 && b3 != 0)
				{
					array2[j].r = (byte)Mathf.Max(b2, b3);
				}
			}
		}
		for (int k = 1; k < worldTex.height; k++)
		{
			for (int num = worldTex.width - 2; num >= 0; num--)
			{
				int num2 = k * worldTex.width + num;
				if (!IsEmptyColumn(pixels[num2]))
				{
					byte b4 = (byte)Mathf.Clamp(array2[num2 + 1].g - 1, 0, 255);
					byte b5 = (byte)Mathf.Clamp(array2[num2 - worldTex.width].g - 1, 0, 255);
					if (b4 != 0 && b5 != 0)
					{
						array2[num2].g = (byte)Mathf.Max(b4, b5);
					}
				}
			}
		}
		for (int num3 = worldTex.height - 2; num3 >= 0; num3--)
		{
			for (int l = 1; l < worldTex.width; l++)
			{
				int num4 = num3 * worldTex.width + l;
				if (!IsEmptyColumn(pixels[num4]))
				{
					byte b6 = (byte)Mathf.Clamp(array2[num4 - 1].b - 1, 0, 255);
					byte b7 = (byte)Mathf.Clamp(array2[num4 + worldTex.width].b - 1, 0, 255);
					if (b6 != 0 && b7 != 0)
					{
						array2[num4].b = (byte)Mathf.Max(b6, b7);
					}
				}
			}
		}
		for (int num5 = array.Length - 1 - 1 - worldTex.width; num5 >= 0; num5--)
		{
			if (!IsEmptyColumn(pixels[num5]))
			{
				byte b8 = (byte)Mathf.Clamp(array2[num5 + 1].a - 1, 0, 255);
				byte b9 = (byte)Mathf.Clamp(array2[num5 + worldTex.width].a - 1, 0, 255);
				if (b8 != 0 && b9 != 0)
				{
					array2[num5].a = (byte)Mathf.Max(b8, b9);
				}
			}
		}
		for (int m = 0; m < array.Length; m++)
		{
			array[m] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			if (!IsEmptyColumn(pixels[m]))
			{
				Color32 color = array2[m];
				byte b10 = (byte)Mathf.Max(color.r, color.g, color.b, color.a);
				array[m] = new Color32(b10, b10, b10, b10);
			}
		}
		resultTex.SetPixels32(array, worldTex.width, worldTex.height);
	}

	public static void Filling(Tex resultTex, Tex worldTex)
	{
		Color32[] pixels = worldTex.GetPixels32();
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			if (color.b >= 128)
			{
				pixels[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			}
			else
			{
				pixels[i] = new Color32(color.r, color.g, color.r, color.g);
			}
		}
		resultTex.SetPixels32(pixels, worldTex.width, worldTex.height);
	}

	public static void Columns(ITexColors resultTex, ITexColors fillingTex, int yMin, int yMax)
	{
		_columns.Process(resultTex, fillingTex, yMin, yMax);
	}

	public static void TypesVolumesDepths(Tex typeTex, Tex volumeTex, Tex depthTex, Tex offsetTex, Tex worldTex, int count)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = worldTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(typeTex, offsetTex, pixels);
		Color32[] orAllocPixels2 = GetOrAllocPixels32(volumeTex, offsetTex, pixels);
		Color32[] orAllocPixels3 = GetOrAllocPixels32(depthTex, offsetTex, pixels);
		int num = 4;
		int num2 = 255;
		for (int i = 0; i < count; i++)
		{
			Color32 c = pixels[i];
			XYZ xYZ = DecodeXYZ(DecodeInt(c));
			c = pixels2[xYZ.x + xYZ.z * worldTex.width];
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			if (xYZ.y >= c.r && xYZ.y < c.g)
			{
				if (xYZ.y >= c.r + 1 && xYZ.y < c.g - 1)
				{
					num3 = num2;
					num4 = 5;
					num5 = Mathf.Min(xYZ.y + 1 - c.r, c.g - xYZ.y);
				}
				else
				{
					num3 = ((xYZ.y < c.g - 1) ? num : c.b);
					num4 = Mathf.Clamp(255 - c.a, 0, 3) + 1;
					num5 = 1;
				}
			}
			orAllocPixels[i].r = (byte)num3;
			orAllocPixels2[i].r = (byte)num4;
			orAllocPixels3[i].r = (byte)num5;
		}
		typeTex.SetPixels32(orAllocPixels, offsetTex.width, offsetTex.height);
		volumeTex.SetPixels32(orAllocPixels2, offsetTex.width, offsetTex.height);
		depthTex.SetPixels32(orAllocPixels3, offsetTex.width, offsetTex.height);
	}

	public static void Retarget(Tex resultTex, Tex dataTex, Vector2 outputStart, Vector2 outputSize)
	{
		dataTex.Clean2D();
		Retarget(resultTex, dataTex.tex2D, outputStart, outputSize);
	}

	public static void Retarget(Tex resultTex, Texture2D dataTex, Vector2 outputStart, Vector2 outputSize)
	{
		XYZ start = XYZ.FromVector3(outputStart);
		XYZ size = XYZ.FromVector3(outputSize);
		if (start.x < 0 || start.x + size.x > dataTex.width || start.y < 0 || start.y + size.y > dataTex.height)
		{
			Retarget_OutOfBounds(resultTex, dataTex, start, size);
			return;
		}
		Color[] pixels = dataTex.GetPixels(start.x, start.y, size.x, size.y);
		resultTex.SetPixels(pixels, size.x, size.y);
	}

	public static void Retarget_OutOfBounds(Tex resultTex, Tex dataTex, Vector2 outputStart, Vector2 outputSize)
	{
		XYZ start = XYZ.FromVector3(outputStart);
		XYZ size = XYZ.FromVector3(outputSize);
		Retarget_OutOfBounds(resultTex, dataTex, start, size);
	}

	private static void Retarget_OutOfBounds(Tex resultTex, Tex dataTex, XYZ start, XYZ size)
	{
		Color32[] pixels = dataTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, size.x, size.y);
		XYZ xYZ = new XYZ(dataTex.width, dataTex.height, 0);
		XYZ xYZ2 = XYZ.Max(start, XYZ.zero);
		XYZ xYZ3 = xYZ2 - start;
		XYZ xYZ4 = XYZ.Min(size, xYZ - xYZ2, size - xYZ3);
		Blit32(pixels, dataTex.width, orAllocPixels, size.x, xYZ2.x, xYZ2.y, xYZ3.x, xYZ3.y, xYZ4.x, xYZ4.y);
		resultTex.SetPixels32(orAllocPixels, size.x, size.y);
	}

	public static void Remap(Tex resultTex, Tex previousTex, Tex dataTex, Vector4 rMask, Vector4 gMask, Vector4 bMask, Vector4 aMask)
	{
		Color32[] array = previousTex?.GetPixels32();
		Color32[] pixels = dataTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, dataTex, pixels);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			Color32 color = pixels[i];
			Color32 color2 = ((array == null) ? default(Color32) : array[i]);
			Vector4 a = new Vector4((int)color.r, (int)color.g, (int)color.b, (int)color.a);
			Vector4 vector = new Vector4((int)color2.r, (int)color2.g, (int)color2.b, (int)color2.a);
			Vector4 vector2 = new Vector4(Vector4.Dot(a, rMask), Vector4.Dot(a, gMask), Vector4.Dot(a, bMask), Vector4.Dot(a, aMask)) + vector;
			orAllocPixels[i] = new Color32((byte)Mathf.RoundToInt(Mathf.Clamp(vector2.x, 0f, 255f)), (byte)Mathf.RoundToInt(Mathf.Clamp(vector2.y, 0f, 255f)), (byte)Mathf.RoundToInt(Mathf.Clamp(vector2.z, 0f, 255f)), (byte)Mathf.RoundToInt(Mathf.Clamp(vector2.w, 0f, 255f)));
		}
		resultTex.SetPixels32(orAllocPixels, dataTex.width, dataTex.height);
	}

	public static void XZInit(Tex resultTex, Tex columnTex)
	{
		Color32[] pixels = columnTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, columnTex, pixels);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			Color32 color = pixels[i];
			int num = Mathf.Max(0, color.b - color.r) + Mathf.Max(0, color.g - color.a);
			orAllocPixels[i] = new Color32((byte)num, 0, 0, 0);
		}
		resultTex.SetPixels32(orAllocPixels, columnTex.width, columnTex.height);
	}

	public static void XZEnd(Tex resultTex, Tex previousTex, Tex columnTex)
	{
		Color32[] pixels = columnTex.GetPixels32();
		Color32[] pixels2 = previousTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, columnTex, pixels);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			Color32 color = pixels[i];
			int num = Mathf.Max(0, color.b - color.r) + Mathf.Max(0, color.g - color.a);
			color = pixels2[i];
			orAllocPixels[i] = EncodeInt(DecodeInt(color) - num + 1);
		}
		resultTex.SetPixels32(orAllocPixels, columnTex.width, columnTex.height);
	}

	public static void XZSumCol(Tex resultTex, Tex previousTex, int step)
	{
		Color32[] pixels = previousTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, previousTex, pixels);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			Color32 color = pixels[i];
			int num = i % (2 * step);
			if (num >= step)
			{
				int num2 = i - num + step - 1;
				Color32 c = pixels[num2];
				color = EncodeInt(DecodeInt(color) + DecodeInt(c));
			}
			orAllocPixels[i] = color;
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.width, previousTex.height);
	}

	public static void XZSumRow(Tex resultTex, Tex previousTex, int step)
	{
		Color32[] pixels = previousTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, previousTex, pixels);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			Color32 color = pixels[i];
			int num = i / previousTex.width;
			int num2 = num % (2 * step);
			if (num2 >= step)
			{
				int num3 = (num - num2 + step) * previousTex.width - 1;
				Color32 c = pixels[num3];
				color = EncodeInt(DecodeInt(color) + DecodeInt(c));
			}
			orAllocPixels[i] = color;
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.width, previousTex.height);
	}

	public static int XZ(ITexColors xzTex, ITexColors columnTex)
	{
		Color32[] pixels = columnTex.GetPixels32();
		int num = 2;
		Color32[] orAllocPixels = GetOrAllocPixels32(xzTex, columnTex, pixels);
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			int r = color.r;
			int g = color.g;
			int b = color.b;
			int a = color.a;
			orAllocPixels[i] = EncodeInt(num);
			if (r < g && (r < b || a < g))
			{
				num += b - r + (g - a);
			}
		}
		xzTex.SetPixels32(orAllocPixels, columnTex.Width, columnTex.Height);
		return num;
	}

	public static void Offset(ITexColors offsetTex, ITexColors xzTex, ITexColors columnTex, int size)
	{
		Color32[] pixels = xzTex.GetPixels32();
		Color32[] pixels2 = columnTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(offsetTex, size, size);
		int num = DecodeInt(pixels[0]);
		int i = 0;
		XYZ zero = XYZ.zero;
		zero.z = 0;
		while (zero.z < columnTex.Height)
		{
			zero.x = 0;
			for (; zero.x < columnTex.Width; zero.x++, i++)
			{
				int num2 = num;
				if (i + 1 < pixels2.Length)
				{
					num = DecodeInt(pixels[i + 1]);
					if (num2 == num)
					{
						continue;
					}
				}
				Color32 color = pixels2[i];
				Color32 color2 = EncodeInt(EncodeXYZ(zero));
				color2.a = color.r;
				color2.a = color.r;
				while (color2.a < color.b)
				{
					orAllocPixels[num2++] = color2;
					color2.a++;
				}
				color2.a = color.a;
				while (color2.a < color.g)
				{
					orAllocPixels[num2++] = color2;
					color2.a++;
				}
				while (num2 < num)
				{
					orAllocPixels[num2++] = color2;
				}
			}
			zero.z++;
		}
		offsetTex.SetPixels32(orAllocPixels, size, size);
	}

	public static void Smooths(ITexColors resultTex, ITexColors typeTex, ITexColors volumeTex, ITexColors offsetTex, int lod, int count)
	{
		Color32[] pixels = typeTex.GetPixels32();
		Color32[] pixels2 = volumeTex.GetPixels32();
		Color32[] pixels3 = offsetTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, typeTex, pixels);
		MondeAtlas atlas = MondeAtlas.GetAtlas("Terrain_LOD0");
		Color32 color = new Color32(byte.MaxValue, 0, 0, 0);
		for (int i = 0; i < count; i++)
		{
			if (pixels[i].r != 0)
			{
				bool dessous = i > 0 && (pixels[i - 1].r == 0 || DecodeXYZ(DecodeInt(pixels3[i - 1])) + XYZ.up != DecodeXYZ(DecodeInt(pixels3[i])));
				byte r = pixels[i].r;
				if (atlas.IsCroute(r, dessous, lod))
				{
					orAllocPixels[i] = color;
				}
				else
				{
					pixels2[i].r = 5;
				}
			}
		}
		volumeTex.SetPixels32(pixels2, volumeTex.Width, volumeTex.Height);
		resultTex.SetPixels32(orAllocPixels, typeTex.Width, typeTex.Height);
	}

	public static void CrustLevel(Tex resultTex, Tex cornerVolumeTex, Tex volumeTex, Tex smoothTex)
	{
		Color32[] pixels = cornerVolumeTex.GetPixels32();
		Color32[] pixels2 = smoothTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, cornerVolumeTex, pixels);
		Color32 color = new Color32(byte.MaxValue, 0, 0, 0);
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			if (pixels2[i].r == 0)
			{
				orAllocPixels[i] = color;
				continue;
			}
			Color32 color2 = pixels[i];
			if (color2.r > 4 || color2.g > 0 || color2.b > 0 || color2.a > 4)
			{
				orAllocPixels[i] = color;
			}
			else if (color2.r != 0 && color2.g != 0 && color2.b != 0 && color2.a != 0)
			{
				int num = color2.r + color2.g + color2.b + color2.a;
				int num2 = (num + 2) / 4;
				byte r = (byte)Mathf.Clamp((num2 - 1) * 85, 0, 255);
				orAllocPixels[i] = new Color32(r, 0, 0, 0);
			}
		}
		resultTex.SetPixels32(orAllocPixels, cornerVolumeTex.width, cornerVolumeTex.height);
	}

	public static void BranchingLevels(Tex resultTex, Tex columnTex, Tex xzTex, Tex offsetTex, Tex levelTex, Tex volumeTex, int count)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = levelTex.GetPixels32();
		Color32[] pixels3 = volumeTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, levelTex, pixels2);
		for (int i = 1; i < count - 1; i++)
		{
			Color32 color = pixels3[i];
			if (color.r >= 5)
			{
				if (pixels[i + 1].a == pixels[i].a + 1)
				{
					Color32 color2 = pixels2[i + 1];
					orAllocPixels[i] = new Color32((byte)((color2.r == 0) ? 4u : 255u), (byte)((color2.g == 0) ? 4u : 255u), (byte)((color2.b == 0) ? 4u : 255u), (byte)((color2.a == 0) ? 4u : 255u));
				}
				else
				{
					orAllocPixels[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				}
			}
			else if (color.r == 0)
			{
				if (pixels[i - 1].a == pixels[i].a - 1)
				{
					Color32 color3 = pixels2[i - 1];
					orAllocPixels[i] = new Color32((color3.r == byte.MaxValue) ? ((byte)1) : ((byte)0), (color3.g == byte.MaxValue) ? ((byte)1) : ((byte)0), (color3.b == byte.MaxValue) ? ((byte)1) : ((byte)0), (color3.a == byte.MaxValue) ? ((byte)1) : ((byte)0));
				}
				else
				{
					orAllocPixels[i] = new Color32(0, 0, 0, 0);
				}
			}
			else
			{
				Color32 color4 = pixels2[i];
				orAllocPixels[i] = new Color32((byte)((color4.r + 42) / 85 + 1), (byte)((color4.g + 42) / 85 + 1), (byte)((color4.b + 42) / 85 + 1), (byte)((color4.a + 42) / 85 + 1));
			}
		}
		resultTex.SetPixels32(orAllocPixels, levelTex.width, levelTex.height);
	}

	public static void BranchingsHackedFromConfig(Tex resultTex, Tex configTex, int count)
	{
		Color32[] pixels = configTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, configTex, pixels);
		for (int i = 0; i < count; i++)
		{
			int r = pixels[i].r;
			orAllocPixels[i] = new Color32((byte)(((r & 2) == 0) ? 255u : 0u), (byte)(((r & 4) == 0) ? 255u : 0u), (byte)(((r & 8) == 0) ? 255u : 0u), (byte)(((r & 0x10) == 0) ? 255u : 0u));
		}
		resultTex.SetPixels32(orAllocPixels, configTex.width, configTex.height);
	}

	public static void LevelConfig(ITexColors levelTex, ITexColors configTex, ITexColors columnTex, ITexColors xzTex, ITexColors offsetTex, ITexColors volumeTex, ITexColors smoothTex, int count)
	{
		_levelConfigNormalUvs.LevelConfig(levelTex, configTex, columnTex, xzTex, offsetTex, volumeTex, smoothTex, count);
	}

	public static void NormalUvs(ITexColors normalTex, ITexColors uvBaseTex, ITexColors uvTransTex, ITexColors uvSpecialTex, ITexColors columnTex, ITexColors xzTex, ITexColors offsetTex, ITexColors typeTex, ITexColors volumeTex, ITexColors levelTex, ITexColors noiseTex, XYZ worldStart, int count)
	{
		_levelConfigNormalUvs.NormalUvs(normalTex, uvBaseTex, uvTransTex, uvSpecialTex, columnTex, xzTex, offsetTex, typeTex, volumeTex, levelTex, noiseTex, worldStart, count);
	}

	public static void Stitch(Tex resultTex, Tex tex1, Tex tex2, Tex tex3, Tex tex4, int border, int inputSize)
	{
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, inputSize * 2, inputSize * 2);
		Color32[] pixels = tex1.GetPixels32();
		Color32[] pixels2 = tex2.GetPixels32();
		Color32[] pixels3 = tex3.GetPixels32();
		Color32[] pixels4 = tex4.GetPixels32();
		int num = 0;
		int num2 = inputSize - border;
		if (pixels != null)
		{
			Blit32(pixels, inputSize, orAllocPixels, inputSize * 2, num, num, border, border, num2, num2);
		}
		if (pixels2 != null)
		{
			Blit32(pixels2, inputSize, orAllocPixels, inputSize * 2, border, num, inputSize, border, num2, num2);
		}
		if (pixels3 != null)
		{
			Blit32(pixels3, inputSize, orAllocPixels, inputSize * 2, num, border, border, inputSize, num2, num2);
		}
		if (pixels4 != null)
		{
			Blit32(pixels4, inputSize, orAllocPixels, inputSize * 2, border, border, inputSize, inputSize, num2, num2);
		}
		resultTex.SetPixels32(orAllocPixels, inputSize * 2, inputSize * 2);
	}

	public static void StitchData(Tex resultTex, Tex offsetTex, Tex xzStitchedTex, Tex columnStitchedTex, Tex tex1, Tex tex2, Tex tex3, Tex tex4, Color32 emptyColor, Color32 internalColor)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = xzStitchedTex.GetPixels32();
		Color32[] pixels3 = columnStitchedTex.GetPixels32();
		Color32[][] array = new Color32[4][]
		{
			tex1.GetPixels32(),
			tex2.GetPixels32(),
			tex3.GetPixels32(),
			tex4.GetPixels32()
		};
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, offsetTex, pixels);
		int num = columnStitchedTex.width >> 1;
		for (int i = 0; i < orAllocPixels.Length; i++)
		{
			XYZ p = DecodeXYZ(DecodeInt(pixels[i]));
			int offset = GetOffset(p, pixels2, pixels3, columnStitchedTex.width, columnStitchedTex.height);
			if (offset >= 0)
			{
				Color32[] array2 = array[((p.x >= num) ? 1 : 0) + ((p.z >= num) ? 2 : 0)];
				orAllocPixels[i] = ((array2 != null) ? array2[offset] : emptyColor);
			}
			else
			{
				orAllocPixels[i] = ((offset == -2) ? emptyColor : internalColor);
			}
		}
		resultTex.SetPixels32(orAllocPixels, offsetTex.width, offsetTex.height);
	}

	public static void MergeColumns(Tex resultTex, Tex supTex)
	{
		int num = supTex.width >> 1;
		int num2 = supTex.height >> 1;
		Color32[] pixels = supTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, num, num2);
		Color32[] array = new Color32[4];
		int num3 = 0;
		for (int i = 0; i < num2; i++)
		{
			int num4 = i * 2 * supTex.width;
			int num5 = 0;
			while (num5 < num)
			{
				array[0] = pixels[num4];
				array[1] = pixels[num4 + 1];
				array[2] = pixels[num4 + supTex.width];
				array[3] = pixels[num4 + supTex.width + 1];
				int num6 = 255;
				int num7 = 0;
				int num8 = 255;
				int num9 = 0;
				for (int j = 0; j < 4; j++)
				{
					Color32 color = array[j];
					if (num6 >= num7 || num6 == 0 || num7 == 255)
					{
						num6 = color.r;
						num7 = color.g;
						num8 = color.b;
						num9 = color.a;
					}
					else if (color.r < color.g && color.r != 0 && color.g != byte.MaxValue)
					{
						num6 = Mathf.Min(num6, color.r);
						num7 = Mathf.Max(num7, color.g);
						if (num8 <= color.a)
						{
							num8 = Mathf.Min(num8, color.b);
						}
						if (num9 >= color.b)
						{
							num9 = Mathf.Max(num9, color.a);
						}
					}
				}
				if (num6 == 0 || num7 == 255)
				{
					num6 = (num7 = (num8 = (num9 = 0)));
				}
				num6 >>= 1;
				num9 >>= 1;
				num7 = num7 + 1 >> 1;
				num8 = num8 + 1 >> 1;
				if (num6 > num7)
				{
					num6 = (num7 = (num6 + num7) / 2);
				}
				if (num8 > num9)
				{
					num8 = (num9 = (num8 + num9) / 2);
				}
				if (num7 - num6 <= 0)
				{
					num6 = (num7 = (num8 = (num9 = 0)));
				}
				orAllocPixels[num3] = new Color32((byte)num6, (byte)num7, (byte)num8, (byte)num9);
				num5++;
				num3++;
				num4 += 2;
			}
		}
		resultTex.SetPixels32(orAllocPixels, num, num2);
	}

	public static void MergeTypesVolumesDepths(Tex typeTex, Tex volumeTex, Tex depthTex, Tex offsetTex, Tex xzSupTex, Tex columnSupTex, Tex typeSupTex, Tex volumeSupTex)
	{
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = xzSupTex.GetPixels32();
		Color32[] pixels3 = columnSupTex.GetPixels32();
		Color32[] pixels4 = typeSupTex.GetPixels32();
		Color32[] pixels5 = volumeSupTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(typeTex, offsetTex, pixels);
		Color32[] orAllocPixels2 = GetOrAllocPixels32(volumeTex, offsetTex, pixels);
		Color32[] orAllocPixels3 = GetOrAllocPixels32(depthTex, offsetTex, pixels);
		int[] array = new int[8];
		for (int i = 0; i < pixels.Length; i++)
		{
			XYZ xYZ = DecodeXYZ(DecodeInt(pixels[i]));
			GetOffsetCube(array, xYZ * 2, XYZ.one * 2, pixels2, pixels3, xzSupTex.width, xzSupTex.height);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			for (int j = 0; j < 8; j++)
			{
				int num8 = array[j];
				if (num8 >= 0)
				{
					num7 += Mathf.Min(4, pixels5[num8].r);
					byte r = pixels4[num8].r;
					if (r != 0 && r != byte.MaxValue)
					{
						if (num == 0 || num == r)
						{
							num = r;
							num4++;
						}
						else if (num2 == 0 || num2 == r)
						{
							num2 = r;
							num5++;
						}
						else if (num3 == 0 || num3 == r)
						{
							num3 = r;
							num6++;
						}
					}
				}
				else
				{
					num7 += ((num8 == -1) ? 4 : 0);
				}
			}
			int num9 = ((num4 >= num5 && num4 >= num6) ? num : ((num5 >= num6) ? num2 : ((num3 > 0) ? num3 : 8)));
			num7 = num7 + 7 >> 3;
			if (num7 > 0)
			{
				orAllocPixels[i] = new Color32((byte)num9, 0, 0, 0);
				orAllocPixels2[i] = new Color32((byte)num7, 0, 0, 0);
				orAllocPixels3[i] = new Color32(byte.MaxValue, 0, 0, 0);
			}
		}
		typeTex.SetPixels32(orAllocPixels, offsetTex.width, offsetTex.height);
		volumeTex.SetPixels32(orAllocPixels2, offsetTex.width, offsetTex.height);
		depthTex.SetPixels32(orAllocPixels3, offsetTex.width, offsetTex.height);
	}

	public static Dictionary<XYZ, int> GetLowResConfigs(Tex offsetTex, Tex configTex, Tex levelTex, XYZ mvSize)
	{
		Dictionary<XYZ, int> dictionary = new Dictionary<XYZ, int>();
		Color32[] pixels = offsetTex.GetPixels32();
		Color32[] pixels2 = configTex.GetPixels32();
		Color32[] pixels3 = levelTex.GetPixels32();
		for (int i = 0; i < pixels.Length; i++)
		{
			byte b = pixels2[i].r;
			if (b == 0)
			{
				continue;
			}
			XYZ a = DecodeXYZ(DecodeInt(pixels[i]));
			XYZ key = XYZ.Div(a, mvSize);
			int valueOrDefault = dictionary.GetValueOrDefault(key);
			if ((b & 0x21) != 0)
			{
				Color32 color = pixels3[i];
				if (color.r < color.g || color.b < color.a)
				{
					b |= 0x10;
				}
				if (color.r > color.g || color.b > color.a)
				{
					b |= 8;
				}
				if (color.r < color.b || color.g < color.a)
				{
					b |= 2;
				}
				if (color.r > color.b || color.g > color.a)
				{
					b |= 4;
				}
			}
			if ((valueOrDefault | b) != valueOrDefault)
			{
				valueOrDefault |= b;
				dictionary[key] = valueOrDefault;
			}
		}
		return dictionary;
	}

	public static void PatchLocalConfig(Color32[] resultColors, int xzWidth, Color32[] xzColors, Color32[] columnColors, Color32[] configColors, XYZ sampleStart, XYZ[] mvEdges, XYZ border, XYZ mvSize)
	{
		int num = sampleStart.x + xzWidth * sampleStart.z;
		for (int i = 0; i < mvEdges.Length; i++)
		{
			XYZ xYZ = mvEdges[i];
			XYZ xYZ2 = xYZ + new XYZ(0, sampleStart.y + 0, 0) + border * 0;
			int num2 = num + xYZ2.x + xzWidth * xYZ2.z;
			if (num2 < 0 || num2 >= columnColors.Length)
			{
				continue;
			}
			Color32 color = columnColors[num2];
			if (xYZ2.y >= color.r && xYZ2.y < color.g && (xYZ2.y < color.b || xYZ2.y >= color.a))
			{
				int num3 = DecodeInt(xzColors[num2]) + (xYZ2.y - color.r);
				if (xYZ2.y >= color.a)
				{
					num3 -= color.a - color.b;
				}
				byte r = configColors[num3].r;
				int num4 = 0;
				if (xYZ.x == 0)
				{
					num4 |= r & 0x10;
				}
				if (xYZ.x == mvSize.x - 1)
				{
					num4 |= r & 8;
				}
				if (xYZ.y == 0)
				{
					num4 |= r & 0x20;
				}
				if (xYZ.y == mvSize.y - 1)
				{
					num4 |= r & 1;
				}
				if (xYZ.z == 0)
				{
					num4 |= r & 4;
				}
				if (xYZ.z == mvSize.z - 1)
				{
					num4 |= r & 2;
				}
				resultColors[num3] = new Color32((byte)num4, 0, 0, 0);
			}
		}
	}

	public static void PatchConfigTex(Tex resultTex, Tex xzTex, Tex columnTex, Tex configTex, IEnumerable<XYZ> sampleStarts, XYZ[] mvEdges, XYZ border, XYZ mvSize)
	{
		Color32[] pixels = xzTex.GetPixels32();
		Color32[] pixels2 = columnTex.GetPixels32();
		Color32[] pixels3 = configTex.GetPixels32();
		Clear(resultTex, configTex);
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, configTex, pixels3);
		foreach (XYZ sampleStart in sampleStarts)
		{
			PatchLocalConfig(orAllocPixels, xzTex.width, pixels, pixels2, pixels3, sampleStart, mvEdges, border, mvSize);
		}
		resultTex.SetPixels32(orAllocPixels, configTex.width, configTex.height);
	}

	public static void TranslateWorld(Tex resultTex, Tex previousTex, int translateX, int translateZ, int translateBottom, int translateTop)
	{
		Color32[] pixels = previousTex.GetPixels32();
		Color32[] orAllocPixels = GetOrAllocPixels32(resultTex, previousTex, pixels);
		for (int i = 0; i < previousTex.height; i++)
		{
			for (int j = 0; j < previousTex.width; j++)
			{
				int num = i * previousTex.width + j;
				int num2 = j - translateX;
				int num3 = i - translateZ;
				Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				if (num2 >= 0 && num3 >= 0 && num2 < previousTex.width && num3 < previousTex.height)
				{
					int num4 = num3 * previousTex.width + num2;
					color = pixels[num4];
					color.r = (byte)Mathf.Clamp(color.r - translateBottom, 0, 255);
					color.g = (byte)Mathf.Clamp(color.g + translateTop, 0, 255);
					if (color.r >= color.g)
					{
						color.r = (color.g = byte.MaxValue);
					}
				}
				orAllocPixels[num] = color;
			}
		}
		resultTex.SetPixels32(orAllocPixels, previousTex.width, previousTex.height);
	}

	public static void ThreadWork(Action<int, int> work, int count, int nThreads)
	{
		while (nThreads > _threads.Count)
		{
			_threads.Add(new ThreadData());
		}
		for (int i = 0; i <= nThreads; i++)
		{
			int num = count * i / (nThreads + 1);
			int num2 = count * (i + 1) / (nThreads + 1);
			if (i < nThreads)
			{
				_threads[i].Start(work, num, num2 - num);
			}
			else
			{
				work(num, num2 - num);
			}
		}
		for (int j = 0; j < nThreads; j++)
		{
			_threads[j].Join();
		}
	}

	public static void ThreadWork(Action<int, int, object> work, int count, int nThreads, object o)
	{
		while (nThreads > _threads.Count)
		{
			_threads.Add(new ThreadData());
		}
		for (int i = 0; i <= nThreads; i++)
		{
			int num = count * i / (nThreads + 1);
			int num2 = count * (i + 1) / (nThreads + 1);
			if (i < nThreads)
			{
				_threads[i].Start(work, num, num2 - num, o);
			}
			else
			{
				work(num, num2 - num, o);
			}
		}
		for (int j = 0; j < nThreads; j++)
		{
			_threads[j].Join();
		}
	}

	public static void ThreadWork(this ISoftAlgo algo, int count, int nThreads)
	{
		ThreadWork(delegate(int i, int n, object a)
		{
			((ISoftAlgo)a).Work(i, n);
		}, count, nThreads, algo);
	}
}
