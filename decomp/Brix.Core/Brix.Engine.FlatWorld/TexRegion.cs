using System.Collections.Generic;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public struct TexRegion
{
	public Tex tex;

	public int x0;

	public int y0;

	public int w;

	public int h;

	public IEnumerator<Color32> Colors32()
	{
		return new ArrayEnumerator.ValueRange2D<Color32>(tex.GetPixels32(), x0, y0, w, h, tex.width);
	}

	public IEnumerator<int> Offsets()
	{
		return new ArrayEnumerator.OffsetRange2D(x0, y0, w, h, tex.width);
	}

	public IEnumerator<ArrayEnumerator.XY> Coords()
	{
		return new ArrayEnumerator.CoordRange2D(x0, y0, w, h, tex.width);
	}
}
