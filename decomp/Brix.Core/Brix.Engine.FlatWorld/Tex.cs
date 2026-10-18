using System;
using System.Collections;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class Tex : ITexColors
{
	private static BrixLogger logger = new BrixLogger();

	public const bool Mipmap = false;

	public const TextureFormat Format2D = TextureFormat.ARGB32;

	public const RenderTextureFormat FormatRT = RenderTextureFormat.ARGB32;

	public static bool Locked = false;

	private RenderTexture renderTex;

	public Texture2D tex2D;

	private Color32[] colors;

	private Texture activeTexture;

	private bool dirty;

	private bool dirtyColors;

	private static Texture2D pix2D;

	public static int MemUsageRT = 0;

	public static int MemUsage2D = 0;

	public static int MemMaxRT = 0;

	public static int MemMax2D = 0;

	public static bool Linear => QualitySettings.activeColorSpace == UnityEngine.ColorSpace.Linear;

	public static RenderTextureReadWrite ColorSpace => Linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB;

	public bool empty => activeTexture == null;

	public int width => activeTexture.width;

	public int height => activeTexture.height;

	bool ITexColors.Empty => empty;

	int ITexColors.Width => width;

	int ITexColors.Height => width;

	public static bool FlipYBug
	{
		get
		{
			RuntimePlatform platform = Application.platform;
			if ((uint)platform <= 1u || platform == RuntimePlatform.LinuxPlayer)
			{
				return false;
			}
			return true;
		}
	}

	void ITexColors.Destroy()
	{
		Destroy();
	}

	public void Render(Texture texIn, Material mat, int width = 0, int height = 0)
	{
		Render(ref renderTex, texIn, mat, width, height);
		activeTexture = renderTex;
		colors = null;
		dirtyColors = false;
		dirty = true;
	}

	public void Swap(Tex other)
	{
		RenderTexture renderTexture = other.renderTex;
		Texture2D texture2D = other.tex2D;
		Color32[] array = other.colors;
		Texture texture = other.activeTexture;
		bool flag = other.dirty;
		bool flag2 = other.dirtyColors;
		other.renderTex = renderTex;
		other.tex2D = tex2D;
		other.colors = colors;
		other.activeTexture = activeTexture;
		other.dirty = dirty;
		other.dirtyColors = dirtyColors;
		renderTex = renderTexture;
		tex2D = texture2D;
		colors = array;
		activeTexture = texture;
		dirty = flag;
		dirtyColors = flag2;
	}

	private static IEnumerator Done()
	{
		yield break;
	}

	private static IEnumerator Then(IEnumerator ie, Action act)
	{
		while (ie.MoveNext())
		{
			yield return ie.Current;
		}
		act();
	}

	public IEnumerator Clean2D_Coroutine(bool immediate = true, bool debug = false)
	{
		return _Clean2D(immediate, debug) ?? Done();
	}

	public void Clean2D(bool debug = false)
	{
		_Clean2D(immediate: true, debug);
	}

	private IEnumerator _Clean2D(bool immediate = true, bool debug = false)
	{
		if (dirty)
		{
			return ReadPixels(immediate, apply: true, debug);
		}
		CleanColors();
		return null;
	}

	public void CleanColors()
	{
		if (dirtyColors)
		{
			dirtyColors = false;
			tex2D.SetPixels32(colors);
			tex2D.Apply();
		}
	}

	public void FreeVRAM(bool cacheIn2D = true)
	{
		if (!(renderTex == null))
		{
			if (cacheIn2D)
			{
				Clean2D();
			}
			activeTexture = tex2D;
			if (RenderTexture.active == renderTex)
			{
				RenderTexture.active = null;
			}
			_ReleaseRT(renderTex);
			renderTex = null;
			dirty = false;
		}
	}

	public void CacheInRenderTexture()
	{
		Material material = Mat.Copy.GetMaterial();
		Render(ref renderTex, tex2D, material, tex2D.width, tex2D.height);
	}

	public void Free2D(bool cacheInRenderTexture = true)
	{
		if (!(tex2D == null))
		{
			if (cacheInRenderTexture)
			{
				CacheInRenderTexture();
			}
			activeTexture = renderTex;
			_Release2D(tex2D);
			tex2D = null;
			colors = null;
			dirtyColors = false;
			if (renderTex != null)
			{
				dirty = true;
			}
		}
	}

	private IEnumerator ReadPixels(bool immediate = true, bool apply = true, bool debug = false)
	{
		Rect finalRect = new Rect(0f, 0f, renderTex.width, renderTex.height);
		GetTex2D(ref tex2D, renderTex.width, renderTex.height);
		IEnumerator result = null;
		if (immediate)
		{
			ReadPixels_Immediate(renderTex, finalRect, debug);
			ReadPixels_Finalize(apply);
		}
		else
		{
			result = ReadPixels_Coroutine(renderTex, finalRect, debug);
			result = Then(result, delegate
			{
				ReadPixels_Finalize(apply);
			});
		}
		return result;
	}

	private void ReadPixels_Immediate(RenderTexture renderTex, Rect finalRect, bool debug = false)
	{
		RenderTexture.active = renderTex;
		_ReadPixels(tex2D, finalRect, 0, 0, recalculateMipMaps: false);
	}

	private IEnumerator ReadPixels_Coroutine(RenderTexture renderTex, Rect finalRect, bool debug = false)
	{
		yield return RenderCam.WaitForReadPixels(tex2D, renderTex, finalRect);
	}

	private void ReadPixels_Finalize(bool apply = true)
	{
		if (apply)
		{
			tex2D.Apply();
		}
		activeTexture = tex2D;
		dirty = false;
		colors = null;
		dirtyColors = false;
	}

	public Color32[] GetPixels32()
	{
		if (activeTexture == null)
		{
			return null;
		}
		Clean2D();
		if (colors == null)
		{
			colors = tex2D.GetPixels32();
		}
		return colors;
	}

	public void SetPixels32(Color32[] colors, int width = 0, int height = 0)
	{
		if (width != 0 && height != 0)
		{
			GetTex2D(ref tex2D, width, height);
		}
		tex2D.SetPixels32(colors);
		tex2D.Apply();
		this.colors = colors;
		activeTexture = tex2D;
		dirty = false;
	}

	public void SetPixels(Color[] colors, int width = 0, int height = 0)
	{
		if (width != 0 && height != 0)
		{
			GetTex2D(ref tex2D, width, height);
		}
		tex2D.SetPixels(colors);
		tex2D.Apply();
		this.colors = null;
		dirtyColors = false;
		activeTexture = tex2D;
		dirty = false;
	}

	public Color32 GetPixel(int x, int y)
	{
		Clean2D();
		if (colors != null)
		{
			return colors[x + tex2D.width * y];
		}
		return tex2D.GetPixel(x, y);
	}

	public Color32 GetPixel_RT(int x, int y)
	{
		if (colors != null)
		{
			return colors[x + tex2D.width * y];
		}
		if (activeTexture == tex2D)
		{
			return tex2D.GetPixel(x, y);
		}
		RenderTexture.active = renderTex;
		GetTex2D(ref pix2D, 1, 1);
		if (FlipYBug)
		{
			y = renderTex.height - 1 - y;
		}
		_ReadPixels(pix2D, new Rect(x, y, 1f, 1f), 0, 0, recalculateMipMaps: false);
		return pix2D.GetPixel(0, 0);
	}

	public void SetPixel(int x, int y, Color32 c)
	{
		SetPixel(x, y, c, apply: true);
	}

	public void SetPixel(int x, int y, Color32 c, bool apply)
	{
		Clean2D();
		activeTexture = tex2D;
		tex2D.SetPixel(x, y, c);
		if (apply)
		{
			tex2D.Apply();
		}
		if (colors != null)
		{
			colors[x + tex2D.width * y] = c;
		}
	}

	public byte[] EncodeToPNG()
	{
		Clean2D();
		return tex2D.EncodeToPNG();
	}

	public void LoadPNG(byte[] bytes)
	{
		if (tex2D == null)
		{
			GetTex2D(ref tex2D, 8, 8);
		}
		tex2D.LoadImage(bytes);
		tex2D.Apply();
		activeTexture = tex2D;
		dirty = false;
		colors = null;
		dirtyColors = false;
	}

	private static void _ReadPixels(Texture2D tex, Rect rect, int destX, int destY, bool recalculateMipMaps)
	{
		if (Locked)
		{
			logger.Error("'Tex' is locked, can't read pixels from RenderTexture!!");
		}
		else
		{
			tex.ReadPixels(rect, destX, destY, recalculateMipMaps);
		}
	}

	public static implicit operator Texture(Tex tex)
	{
		tex.CleanColors();
		return tex.activeTexture;
	}

	public static implicit operator Tex(Texture2D tex2D)
	{
		return new Tex
		{
			tex2D = tex2D,
			activeTexture = tex2D
		};
	}

	public static implicit operator Tex(RenderTexture renderTex)
	{
		return new Tex
		{
			renderTex = renderTex,
			activeTexture = renderTex,
			dirty = true
		};
	}

	public static explicit operator Tex(Texture tex)
	{
		Texture2D texture2D = tex as Texture2D;
		if (texture2D != null)
		{
			return texture2D;
		}
		return tex as RenderTexture;
	}

	private static void UnityDestroy(UnityEngine.Object o)
	{
		UnityEngine.Object.Destroy(o);
	}

	public static void Destroy(Tex a)
	{
		a?.Destroy();
	}

	public static void Destroy(Tex a, Tex b)
	{
		a?.Destroy();
		b?.Destroy();
	}

	public static void Destroy(Tex a, Tex b, Tex c)
	{
		Destroy(a, b);
		Destroy(c);
	}

	public static void Destroy(Tex a, Tex b, Tex c, Tex d)
	{
		Destroy(a, b);
		Destroy(c, d);
	}

	public static void Destroy(Tex a, Tex b, Tex c, Tex d, Tex e)
	{
		Destroy(a, b, c);
		Destroy(d, e);
	}

	public static void Destroy(Tex a, Tex b, Tex c, Tex d, Tex e, Tex f)
	{
		Destroy(a, b, c);
		Destroy(d, e, f);
	}

	public static void Destroy(params Tex[] a)
	{
		for (int i = 0; i < a.Length; i++)
		{
			a[i]?.Destroy();
		}
	}

	private void Destroy()
	{
		if (RenderTexture.active == renderTex)
		{
			RenderTexture.active = null;
		}
		if (renderTex != null)
		{
			_ReleaseRT(renderTex);
		}
		if (tex2D != null)
		{
			_Release2D(tex2D);
		}
		renderTex = null;
		tex2D = null;
		colors = null;
		dirtyColors = false;
		activeTexture = null;
		dirty = false;
	}

	private static void _AllocRT(ref RenderTexture tex, int w, int h)
	{
		tex = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, ColorSpace);
		MemUsageRT += w * h * 4;
		MemMaxRT = Mathf.Max(MemMaxRT, MemUsageRT);
	}

	private static void _Alloc2D(ref Texture2D tex, int w, int h)
	{
		tex = new Texture2D(w, h, TextureFormat.ARGB32, mipmap: false, Linear);
		MemUsage2D += w * h * 4;
		MemMax2D = Mathf.Max(MemMax2D, MemUsage2D);
	}

	private static void _ReleaseRT(RenderTexture tex)
	{
		MemUsageRT -= tex.width * tex.height * 4;
		RenderTexture.ReleaseTemporary(tex);
	}

	private static void _Release2D(Texture2D tex)
	{
		MemUsage2D -= tex.width * tex.height * 4;
		UnityDestroy(tex);
	}

	public static void ReadyTexture(Texture tex)
	{
		tex.anisoLevel = 1;
		tex.filterMode = FilterMode.Point;
		tex.wrapMode = TextureWrapMode.Clamp;
	}

	public static void GetRenderTex(ref RenderTexture renderTex, int inWidth, int inHeight, bool setAsActive = true)
	{
		if (renderTex == null || renderTex.width != inWidth || renderTex.height != inHeight)
		{
			if (renderTex != null)
			{
				_ReleaseRT(renderTex);
			}
			_AllocRT(ref renderTex, inWidth, inHeight);
			ReadyTexture(renderTex);
		}
		if (setAsActive)
		{
			RenderTexture.active = renderTex;
		}
	}

	public static void GetTex2D(ref Texture2D resultTex, int outWidth, int outHeight)
	{
		if (resultTex == null || resultTex.width != outWidth || resultTex.height != outHeight)
		{
			if (resultTex != null)
			{
				_Release2D(resultTex);
			}
			_Alloc2D(ref resultTex, outWidth, outHeight);
			ReadyTexture(resultTex);
		}
	}

	public static void FreeTex2D(ref Texture2D tex)
	{
		if (tex != null)
		{
			_Release2D(tex);
		}
		tex = null;
	}

	private static void ReadPixels(ref Texture2D resultTex, int outWidth = 0, int outHeight = 0, bool apply = true, bool debug = false)
	{
		if (outWidth == 0)
		{
			outWidth = RenderTexture.active.width;
		}
		if (outHeight == 0)
		{
			outHeight = RenderTexture.active.height;
		}
		GetTex2D(ref resultTex, outWidth, outHeight);
		int num = 0;
		if (FlipYBug)
		{
			num = RenderTexture.active.height - outHeight;
		}
		_ReadPixels(resultTex, new Rect(0f, num, outWidth, outHeight), 0, 0, recalculateMipMaps: false);
		if (apply)
		{
			resultTex.Apply();
		}
	}

	private static void Render(ref RenderTexture renderTex, Texture texIn, Material mat, int width = 0, int height = 0)
	{
		if (width == 0)
		{
			width = texIn.width;
		}
		if (height == 0)
		{
			height = texIn.height;
		}
		GetRenderTex(ref renderTex, width, height);
		if (texIn != null)
		{
			ReadyTexture(texIn);
		}
		mat.SetPass(0);
		Graphics.Blit(texIn, renderTex, mat, 0);
	}
}
