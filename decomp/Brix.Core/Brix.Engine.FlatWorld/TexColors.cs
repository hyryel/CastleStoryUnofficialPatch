using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class TexColors : ITexColors
{
	private static readonly Color32[] EmptyColors = new Color32[0];

	[NotNull]
	public Color32[] Colors = EmptyColors;

	public int Width { get; private set; }

	public int Height => (Width != 0) ? (Colors.Length / Width) : 0;

	public bool Empty => Colors.Length == 0;

	public Color32[] GetPixels32()
	{
		return Colors;
	}

	public void SetPixels32(Color32[] colors, int width, int height)
	{
		Colors = colors;
		Width = width;
	}

	public Color32 GetPixel(int x, int z)
	{
		return Colors[x + Width * z];
	}

	public void SetPixel(int x, int z, Color32 c)
	{
		Colors[x + Width * z] = c;
	}

	public void Destroy()
	{
		Colors = EmptyColors;
		Width = 0;
	}

	public void Clear(int width, int height)
	{
		if (width == 0)
		{
			Destroy();
			return;
		}
		if (width != Width || height != Height)
		{
			if (width * height != Width * Height)
			{
				Colors = new Color32[width * height];
			}
			Width = width;
		}
		Array.Clear(Colors, 0, Colors.Length);
	}
}
