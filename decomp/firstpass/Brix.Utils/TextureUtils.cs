using UnityEngine;

namespace Brix.Utils;

public static class TextureUtils
{
	public static void CopyTo(this Texture2D src, ref Texture2D dst)
	{
		if (dst == null)
		{
			dst = new Texture2D(src.width, src.height);
		}
		dst.SetPixels(src.GetPixels());
		dst.Apply();
	}
}
