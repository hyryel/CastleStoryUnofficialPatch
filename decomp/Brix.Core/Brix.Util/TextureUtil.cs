using UnityEngine;

namespace Brix.Util;

public static class TextureUtil
{
	private static Material defaultMaterial = new Material(Shader.Find("Sprites/Default"));

	public static Texture2D GetReadableTexture(Texture tex, Material mat = null)
	{
		if (tex == null)
		{
			return null;
		}
		if (mat == null)
		{
			mat = defaultMaterial;
		}
		RenderTexture active = RenderTexture.active;
		RenderTexture temporary = RenderTexture.GetTemporary(tex.width, tex.height);
		temporary.filterMode = tex.filterMode;
		RenderTexture.active = temporary;
		mat.SetPass(0);
		Graphics.Blit(tex, temporary, mat, 0);
		Texture2D texture2D = new Texture2D(temporary.width, temporary.height);
		texture2D.filterMode = tex.filterMode;
		texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		return texture2D;
	}
}
