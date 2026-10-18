using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

public class LowResSimple : LowResLayer
{
	public TexColors fillingTex = new TexColors();

	public TexColors columnTex = new TexColors();

	public override void Init(Layer parentLayer, XYZ scale)
	{
		base.Init(parentLayer, scale);
		TexColors texColors = (parentLayer as SimpleLayer).fillingTex;
		XYZ parentSize = new XYZ(texColors.Width, 256, texColors.Height);
		Layer.Filling_LowRes(fillingTex, texColors, XYZ.zero, parentSize, scale);
		TexColors texColors2 = new TexColors();
		Soft.Columns(texColors2, texColors, 0, 255);
		Layer.Filling_LowRes(columnTex, texColors2, XYZ.zero, parentSize, scale);
	}

	public override void Destroy()
	{
		fillingTex.Destroy();
		base.Destroy();
	}

	public override bool IsOccupiedOrUncertain(XYZ p)
	{
		if (p.x < 0 || p.x >= fillingTex.Width || p.z < 0 || p.z >= fillingTex.Height)
		{
			return false;
		}
		Color32 pixel = fillingTex.GetPixel(p.x, p.z);
		return p.y >= pixel.r && p.y < pixel.g;
	}

	public override bool IsFullyInternal(XYZ p)
	{
		if (p.x < 0 || p.x >= columnTex.Width || p.z < 0 || p.z >= columnTex.Height)
		{
			return false;
		}
		Color32 pixel = columnTex.GetPixel(p.x, p.z);
		return p.y >= pixel.b && p.y < pixel.a;
	}

	public override IEnumerable<XYZ> GetOccupiedOrUncertain()
	{
		Color32[] fillingColors = fillingTex.GetPixels32();
		for (int i = 0; i < fillingColors.Length; i++)
		{
			Color32 f = fillingColors[i];
			if (f.r < f.g)
			{
				XYZ p = new XYZ(i % fillingTex.Width, 0, i / fillingTex.Width)
				{
					y = f.r
				};
				while (p.y < f.g)
				{
					yield return p;
					p.y++;
				}
			}
		}
	}

	public override IEnumerable<XYZ> GetFullyOccupied()
	{
		Color32[] fillingColors = fillingTex.GetPixels32();
		for (int i = 0; i < fillingColors.Length; i++)
		{
			Color32 f = fillingColors[i];
			if (f.b < f.a)
			{
				XYZ p = new XYZ(i % fillingTex.Width, 0, i / fillingTex.Width)
				{
					y = f.b
				};
				while (p.y < f.a)
				{
					yield return p;
					p.y++;
				}
			}
		}
	}

	public override IEnumerable<XYZ> GetOccupiedOrUncertain(XYZ start, XYZ size)
	{
		Color32[] fillingColors = fillingTex.GetPixels32();
		XYZ end = start + size;
		start.x = Mathf.Max(0, start.x);
		start.z = Mathf.Max(0, start.z);
		end.x = Mathf.Min(fillingTex.Width, end.x);
		end.z = Mathf.Min(fillingTex.Height, end.z);
		for (int z = start.z; z < end.z; z++)
		{
			int i = start.x + fillingTex.Width * z;
			int x = start.x;
			while (x < end.x)
			{
				Color32 f = fillingColors[i];
				if (f.r < f.g)
				{
					int bottom = Mathf.Max(f.r, start.y);
					int top = Mathf.Min(f.g, end.y);
					XYZ p = new XYZ(x, 0, z)
					{
						y = bottom
					};
					while (p.y < top)
					{
						yield return p;
						p.y++;
					}
				}
				x++;
				i++;
			}
		}
	}

	public override XYZ GetOccupiedOrUncertainTotalSize(out XYZ start)
	{
		XYZ xYZ = new XYZ(fillingTex.Width, 256, fillingTex.Height);
		XYZ xYZ2 = new XYZ(0, 0, 0);
		Color32[] pixels = fillingTex.GetPixels32();
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			if (color.r < color.g)
			{
				XYZ xYZ3 = new XYZ(i % fillingTex.Width, 0, i / fillingTex.Width);
				xYZ = XYZ.Min(xYZ, new XYZ(xYZ3.x, color.r, xYZ3.z));
				xYZ2 = XYZ.Max(xYZ2, new XYZ(xYZ3.x + 1, color.g, xYZ3.z + 1));
			}
		}
		start = xYZ;
		return xYZ2 - xYZ;
	}
}
