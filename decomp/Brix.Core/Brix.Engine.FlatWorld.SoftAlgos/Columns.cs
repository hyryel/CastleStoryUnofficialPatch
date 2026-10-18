using Brix.Utils;
using UnityEngine;

namespace Brix.Engine.FlatWorld.SoftAlgos;

public class Columns : Poolable, ISoftAlgo
{
	public ITexColors ResultTex;

	public ITexColors FillingTex;

	private Color32[] fillingColors;

	private Color32[] resultColors;

	private int width;

	private int height;

	private int yMin;

	private int yMax;

	public int Count => resultColors.Length;

	public void Process(ITexColors resultTex, ITexColors fillingTex, int yMin, int yMax)
	{
		Init(resultTex, fillingTex, yMin, yMax);
		Work(0, Count);
		Conclude();
	}

	public void Init(ITexColors resultTex, ITexColors fillingTex, int yMin, int yMax)
	{
		ResultTex = resultTex;
		FillingTex = fillingTex;
		fillingColors = fillingTex.GetPixels32();
		resultColors = Soft.GetOrAllocPixels32(resultTex, fillingTex, fillingColors);
		width = fillingTex.Width;
		height = fillingTex.Height;
		this.yMin = yMin;
		this.yMax = yMax;
	}

	public void Conclude()
	{
		ResultTex.SetPixels32(resultColors, FillingTex.Width, FillingTex.Height);
	}

	public Color32 Pixel(int i)
	{
		Color32 color = fillingColors[i];
		if (color.r >= color.g)
		{
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		byte r = color.r;
		byte g = color.g;
		int a = r + 1;
		int a2 = g - 1;
		a = Mathf.Max(a, color.b + 1);
		a2 = Mathf.Min(a2, color.a - 1);
		if (r <= yMin && a == r + 1)
		{
			a = r;
		}
		if (g >= yMax && a2 == g - 1)
		{
			a2 = g;
		}
		int num = i % width;
		int num2 = i / width;
		if (num >= 1)
		{
			CheckNeighbor(fillingColors[i - 1], ref a, ref a2);
		}
		if (num2 >= 1)
		{
			CheckNeighbor(fillingColors[i - width], ref a, ref a2);
		}
		if (num < width - 1)
		{
			CheckNeighbor(fillingColors[i + 1], ref a, ref a2);
		}
		if (num2 < height - 1)
		{
			CheckNeighbor(fillingColors[i + width], ref a, ref a2);
		}
		if (a2 <= a || a >= g || a2 <= r)
		{
			a = (a2 = (g + r) / 2);
		}
		return new Color32(r, g, (byte)a, (byte)a2);
	}

	public void Work(int start, int count)
	{
		while (--count >= 0)
		{
			resultColors[start] = Pixel(start);
			start++;
		}
	}

	private static void CheckNeighbor(Color32 d, ref int stop, ref int restart)
	{
		if (d.b < d.a)
		{
			stop = Mathf.Max(stop, d.b);
			restart = Mathf.Min(restart, d.a);
		}
		else
		{
			stop = 255;
			restart = 0;
		}
	}
}
