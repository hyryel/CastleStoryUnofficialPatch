using System;
using System.IO;
using Brix.Engine;
using Brix.Lua;
using Brix.ThirdParty.GA;
using Brix.Utils.Logging;
using UnityEngine;

public class ScreenShotTaker : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(ScreenShotTaker).FullName);

	public const bool ScreenshotHideMenu = false;

	public const bool ScreenshotKeepFull = false;

	public TextureFormat Format = TextureFormat.RGBA32;

	public bool Mipmap;

	public bool Linear;

	public bool KeepAlive;

	private Action<Texture2D> callback;

	private Texture2D _tex;

	private static ScreenShotTaker instance;

	private readonly Vector2 _thumbSize = new Vector2(240f, 135f);

	private Texture2D _tmpThumbTex;

	private static CameraClearFlags _cClearFlags;

	private static int _cCullingMask;

	private static Color _cColor;

	private static bool _busy;

	public Texture2D CreateTexture(int width, int height)
	{
		return new Texture2D(width, height, Format, Mipmap, Linear);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination);
		if (callback == null)
		{
			return;
		}
		try
		{
			callback(CopyScreen(ref _tex));
		}
		catch (Exception message)
		{
			logger.Warn(message);
		}
		callback = null;
		if (!KeepAlive)
		{
			Neo.Relais(delegate
			{
				UnityEngine.Object.Destroy(this);
			}, 0.1f, "Remove ScreenShotTaker");
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(_tex);
		UnityEngine.Object.Destroy(_tmpThumbTex);
	}

	private Texture2D CopyScreen(ref Texture2D srcTex)
	{
		if (srcTex == null || srcTex.width != Screen.width || srcTex.height != Screen.height)
		{
			if (srcTex != null)
			{
				UnityEngine.Object.Destroy(srcTex);
			}
			srcTex = CreateTexture(Screen.width, Screen.height);
		}
		srcTex.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		srcTex.Apply();
		GameAnalytics.NewEvent("Game:Menu:HotCorner:Screenshot");
		return srcTex;
	}

	private static void AsyncCapture(Action<Texture2D> callback, Texture2D srcTex = null)
	{
		if (!instance)
		{
			instance = Camera.main.gameObject.AddComponent<ScreenShotTaker>();
		}
		instance._tex = srcTex ?? instance._tex;
		instance.callback = callback;
	}

	public static void PrintScreen(string path)
	{
		AsyncCapture(delegate(Texture2D screenshot)
		{
			File.WriteAllBytes(path, screenshot.EncodeToPNG());
			logger.Info("PrintScreen! " + path);
			UnityEngine.Object.Destroy(screenshot);
		});
	}

	public Texture2D CaptureSyncScreenshot(bool hideMenus)
	{
		if (hideMenus)
		{
			Neo.menusOff = true;
			if (Neo.hideVersionWithMenus)
			{
				Neo.versionOff = true;
			}
		}
		Texture2D result = instance.CopyScreen(ref _tex);
		if (hideMenus)
		{
			Neo.Relais(delegate
			{
				Neo.menusOff = false;
				Neo.versionOff = false;
			}, "hide menu");
		}
		return result;
	}

	public void CreateThumbnail(string pathScreenshot, string pathThumbnail, Vector2 thumbsize, bool keepFull)
	{
		Texture2D texture2D = CreateTexture(Screen.width, Screen.height);
		try
		{
			texture2D.LoadImage(File.ReadAllBytes(pathScreenshot));
		}
		catch (Exception message)
		{
			logger.Warn(message);
			if (!File.Exists(pathScreenshot))
			{
				logger.Warn("CreateThumbnail failed : '" + pathScreenshot + "'.");
				return;
			}
		}
		float num = Mathf.Min((float)texture2D.width / thumbsize.x, (float)texture2D.height / thumbsize.y);
		Texture2D texture2D2 = CreateTexture(Mathf.RoundToInt(thumbsize.x * num), Mathf.RoundToInt(thumbsize.y * num));
		int num2 = (texture2D.width - texture2D2.width) / 2;
		int num3 = (texture2D.height - texture2D2.height) / 2;
		for (int i = 0; i < texture2D2.height; i++)
		{
			for (int j = 0; j < texture2D2.width; j++)
			{
				texture2D2.SetPixel(j, i, texture2D.GetPixel(j + num2, i + num3));
			}
		}
		texture2D2.Apply();
		Texture2D texture2D3 = CreateTexture(Mathf.RoundToInt(thumbsize.x), Mathf.RoundToInt(thumbsize.y));
		for (int k = 0; k < texture2D3.height; k++)
		{
			for (int l = 0; l < texture2D3.width; l++)
			{
				texture2D3.SetPixel(l, k, texture2D2.GetPixelBilinear((float)l / (float)texture2D3.width, (float)k / (float)texture2D3.height));
			}
		}
		texture2D3.Apply();
		File.WriteAllBytes(pathThumbnail, texture2D3.EncodeToPNG());
		if (!keepFull)
		{
			File.Delete(pathScreenshot);
		}
		UnityEngine.Object.Destroy(texture2D);
		UnityEngine.Object.Destroy(texture2D2);
		UnityEngine.Object.Destroy(texture2D3);
	}

	public void CreateThumbnail(Texture2D tex, float scale = 2f, Texture2D thumbTex = null, bool destroyTmpOnComplete = true, Action<Texture2D> onCompleteAction = null)
	{
		if (tex == null)
		{
			return;
		}
		Texture2D texture2D = thumbTex;
		if ((object)texture2D == null)
		{
			Vector2 thumbSize = _thumbSize;
			int width = Mathf.RoundToInt(thumbSize.x);
			Vector2 thumbSize2 = _thumbSize;
			texture2D = CreateTexture(width, Mathf.RoundToInt(thumbSize2.y));
		}
		thumbTex = texture2D;
		if ((bool)_tmpThumbTex)
		{
			UnityEngine.Object.Destroy(_tmpThumbTex);
		}
		_tmpThumbTex = CreateTexture(Mathf.RoundToInt((float)thumbTex.width * scale), Mathf.RoundToInt((float)thumbTex.height * scale));
		int num = (tex.width - _tmpThumbTex.width) / 2;
		int num2 = (tex.height - _tmpThumbTex.height) / 2;
		for (int i = 0; i < _tmpThumbTex.height; i++)
		{
			for (int j = 0; j < _tmpThumbTex.width; j++)
			{
				_tmpThumbTex.SetPixel(j, i, tex.GetPixel(j + num, i + num2));
			}
		}
		_tmpThumbTex.Apply();
		for (int k = 0; k < thumbTex.height; k++)
		{
			for (int l = 0; l < thumbTex.width; l++)
			{
				thumbTex.SetPixel(l, k, _tmpThumbTex.GetPixelBilinear((float)l / (float)thumbTex.width, (float)k / (float)thumbTex.height));
			}
		}
		thumbTex.Apply();
		onCompleteAction?.Invoke(thumbTex);
	}

	public static void CaptureScreenshot(bool hideMenus, bool createThumbnail, bool compoundMode = false, float thumbScale = 2f, Action<Texture2D, Texture2D> onCompleteAction = null, Texture2D srcTex = null, Texture2D srcTexThumb = null)
	{
		if (_busy)
		{
			throw new Exception("ScreenShotTaker is busy!");
		}
		thumbScale = 1f;
		_cClearFlags = Camera.main.clearFlags;
		_cCullingMask = Camera.main.cullingMask;
		_cColor = Camera.main.backgroundColor;
		_busy = true;
		Camera.main.cullingMask = _cCullingMask & -32769;
		if (hideMenus)
		{
			HideMenus(flag: true);
		}
		Neo.Relais(delegate
		{
			AsyncCapture(delegate(Texture2D tex)
			{
				if (createThumbnail)
				{
					instance.CreateThumbnail(tex, thumbScale, srcTexThumb, destroyTmpOnComplete: true, delegate(Texture2D texThumb)
					{
						if (hideMenus)
						{
							HideMenus(flag: false);
						}
						if (onCompleteAction != null)
						{
							onCompleteAction(tex, texThumb);
						}
					});
				}
				else
				{
					if (hideMenus)
					{
						HideMenus(flag: false);
					}
					if (onCompleteAction != null)
					{
						onCompleteAction(tex, null);
					}
				}
				Camera.main.clearFlags = _cClearFlags;
				Camera.main.cullingMask = _cCullingMask;
				Camera.main.backgroundColor = _cColor;
				_busy = false;
				if (compoundMode)
				{
					LuaHookBase.InvokeFromCLR(UIGameObserver.lua_OnBlueprintScreenshotTaken);
				}
			}, srcTex);
		}, "async capture");
	}

	private static void HideMenus(bool flag)
	{
		Neo.menusOff = flag;
		if (Neo.hideVersionWithMenus)
		{
			Neo.versionOff = flag;
		}
	}
}
