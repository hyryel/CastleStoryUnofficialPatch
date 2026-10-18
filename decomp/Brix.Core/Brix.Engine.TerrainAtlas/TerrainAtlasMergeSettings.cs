using System;
using System.IO;
using UnityEngine;

namespace Brix.Engine.TerrainAtlas;

[Serializable]
public class TerrainAtlasMergeSettings : ScriptableObject
{
	private class TextureSection
	{
		private Vector4[] vPixelsOut;

		private float[] vWeightsOut;

		private int width;

		public TextureSection(int width, int height)
		{
			vPixelsOut = new Vector4[width * height];
			vWeightsOut = new float[width * height];
			this.width = width;
		}

		public void Clear()
		{
			Array.Clear(vPixelsOut, 0, vPixelsOut.Length);
			Array.Clear(vWeightsOut, 0, vWeightsOut.Length);
		}

		public void AddPixel(int x, int y, Vector4 v, float w)
		{
			vPixelsOut[x + width * y] += v * w;
			vWeightsOut[x + width * y] += w;
		}

		public Vector4 GetPixel(int x, int y)
		{
			float num = vWeightsOut[x + width * y];
			Vector4 result = vPixelsOut[x + width * y] / num;
			if (result.w < 0.001f)
			{
				result = Vector4.zero;
			}
			return result;
		}
	}

	public TerrainAtlasPaddingSettings padding;

	public TerrainAtlasVisualEntry[] tiles;

	public string splitTexturesPath;

	public string mergedTexturesPath;

	private bool refreshOnlyOneTile;

	private TerrainAtlasVisualEntry tileToRefresh;

	public TextureData originalTextures = new TextureData();

	public bool overwriteSplitTextures;

	[ContextMenu("Split Atlas")]
	private void SplitAtlas()
	{
		Debug.Log("Split Atlas");
		TerrainAtlasVisualEntry[] array = tiles;
		foreach (TerrainAtlasVisualEntry terrainAtlasVisualEntry in array)
		{
			string folderPath = Path.Combine(splitTexturesPath, terrainAtlasVisualEntry.name);
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
				Debug.Log("Created directory '" + folderPath + "'");
			}
			Action<Texture2D, string, Vector2> action = delegate(Texture2D src, string name, Vector2 uv)
			{
				string text = Path.Combine(folderPath, name + ".png");
				string text2 = Path.Combine(folderPath, name + ".jpg");
				if (overwriteSplitTextures || (!File.Exists(text) && !File.Exists(text2)))
				{
					string text3 = (File.Exists(text2) ? text2 : text);
					int num = src.width / 4;
					Texture2D texture2D = new Texture2D(num, num, TextureFormat.ARGB32, mipmap: true, linear: true);
					ReadTextureSection(src, Mathf.RoundToInt(uv.x * (float)num), Mathf.RoundToInt((3f - uv.y) * (float)num), num, num, texture2D);
					File.WriteAllBytes(text3, texture2D.EncodeToPNG());
					Debug.Log("Wrote file '" + text3 + "'");
					UnityEngine.Object.DestroyImmediate(texture2D);
				}
			};
			action(originalTextures.diffuse, terrainAtlasVisualEntry.name + "_Diffuse", terrainAtlasVisualEntry.uv);
			action(originalTextures.normals, terrainAtlasVisualEntry.name + "_Normals", terrainAtlasVisualEntry.uv);
			action(originalTextures.transitionsDiffuse, terrainAtlasVisualEntry.name + "Transitions_Diffuse", terrainAtlasVisualEntry.useSpecial ? terrainAtlasVisualEntry.uvSpecial : terrainAtlasVisualEntry.uvTransition);
			action(originalTextures.transitionsNormals, terrainAtlasVisualEntry.name + "Transitions_Normals", terrainAtlasVisualEntry.useSpecial ? terrainAtlasVisualEntry.uvSpecial : terrainAtlasVisualEntry.uvTransition);
			action(originalTextures.transitionsSpecular, terrainAtlasVisualEntry.name + "Transitions_Specular", terrainAtlasVisualEntry.uvSpecial);
		}
	}

	private void ReadTextureSection(Texture2D src, int startX, int startY, int sizeX, int sizeY, Texture2D dst)
	{
		Color[] pixels = src.GetPixels();
		Color[] array = new Color[sizeX * sizeY];
		for (int i = startY; i < startY + sizeY; i++)
		{
			for (int j = startX; j < startX + sizeX; j++)
			{
				Color color = pixels[j + i * src.width];
				if (color.a < 0.001f)
				{
					color = new Color(0f, 0f, 0f, 0f);
				}
				array[j - startX + (i - startY) * sizeX] = color;
			}
		}
		dst.SetPixels(array);
		dst.Apply();
	}

	[ContextMenu("Merge Atlas")]
	private void MergeAtlas()
	{
		Debug.Log("Merge Atlas");
		int w = padding.uvAtlasWidth / 4;
		Texture2D texture2D = CreateEmptyTexture(padding.uvAtlasWidth, padding.uvAtlasWidth);
		Texture2D texture2D2 = CreateEmptyNormals(padding.uvAtlasWidth, padding.uvAtlasWidth);
		Texture2D texture2D3 = CreateEmptyTexture(padding.uvAtlasWidth, padding.uvAtlasWidth);
		Texture2D texture2D4 = CreateEmptyNormals(padding.uvAtlasWidth, padding.uvAtlasWidth);
		Texture2D texture2D5 = CreateEmptyTexture(padding.uvAtlasWidth, padding.uvAtlasWidth);
		TextureSection section = new TextureSection(w / 4 - padding.uvInnerDecal * 2, w / 4 - padding.uvInnerDecal * 2);
		TerrainAtlasVisualEntry[] array = tiles;
		if (refreshOnlyOneTile)
		{
			LoadExistingTexture(texture2D, "Diffuse");
			LoadExistingTexture(texture2D2, "Normals");
			LoadExistingTexture(texture2D3, "Transitions_Diffuse");
			LoadExistingTexture(texture2D4, "Transitions_Normals");
			LoadExistingTexture(texture2D5, "Transitions_Specular");
			array = new TerrainAtlasVisualEntry[1] { tileToRefresh };
		}
		int num = 0;
		TerrainAtlasVisualEntry[] array2 = array;
		foreach (TerrainAtlasVisualEntry terrainAtlasVisualEntry in array2)
		{
			num++;
			string folderPath = Path.Combine(splitTexturesPath, terrainAtlasVisualEntry.name);
			if (!Directory.Exists(folderPath))
			{
				continue;
			}
			Action<Texture2D, string, Vector2> action = delegate(Texture2D dst, string name, Vector2 uv)
			{
				string path = Path.Combine(folderPath, name + ".png");
				if (!File.Exists(path))
				{
					path = Path.Combine(folderPath, name + ".jpg");
					if (!File.Exists(path))
					{
						return;
					}
				}
				Texture2D texture2D6 = new Texture2D(2, 2, TextureFormat.ARGB32, mipmap: true, linear: true);
				texture2D6.LoadImage(File.ReadAllBytes(path));
				texture2D6.Apply();
				Color[] pixels = dst.GetPixels();
				for (int j = 0; j < 16; j++)
				{
					WriteTextureSection(texture2D6, j % 4 * texture2D6.width / 4, j / 4 * texture2D6.height / 4, texture2D6.width / 4, texture2D6.height / 4, pixels, dst.width, (Mathf.RoundToInt(uv.x) * 4 + j % 4) * w / 4 + padding.uvInnerDecal, (Mathf.RoundToInt(3f - uv.y) * 4 + j / 4) * w / 4 + padding.uvInnerDecal, w / 4 - padding.uvInnerDecal * 2, w / 4 - padding.uvInnerDecal * 2, padding.uvInnerDecal, section);
				}
				UnityEngine.Object.DestroyImmediate(texture2D6);
				dst.SetPixels(pixels);
			};
			action(texture2D, terrainAtlasVisualEntry.name + "_Diffuse", terrainAtlasVisualEntry.uv);
			action(texture2D2, terrainAtlasVisualEntry.name + "_Normals", terrainAtlasVisualEntry.uv);
			action(texture2D3, terrainAtlasVisualEntry.name + "Transitions_Diffuse", terrainAtlasVisualEntry.useSpecial ? terrainAtlasVisualEntry.uvSpecial : terrainAtlasVisualEntry.uvTransition);
			action(texture2D4, terrainAtlasVisualEntry.name + "Transitions_Normals", terrainAtlasVisualEntry.useSpecial ? terrainAtlasVisualEntry.uvSpecial : terrainAtlasVisualEntry.uvTransition);
			action(texture2D5, terrainAtlasVisualEntry.name + "Transitions_Specular", terrainAtlasVisualEntry.uvSpecial);
		}
		if (!Directory.Exists(mergedTexturesPath))
		{
			Directory.CreateDirectory(mergedTexturesPath);
			Debug.Log("Created directory '" + mergedTexturesPath + "'");
		}
		Action<Texture2D, string> action2 = delegate(Texture2D dst, string name)
		{
			dst.Apply();
			File.WriteAllBytes(Path.Combine(mergedTexturesPath, name + ".png"), dst.EncodeToPNG());
			Debug.Log("Wrote file '" + Path.Combine(mergedTexturesPath, name + ".pgn") + "'");
			UnityEngine.Object.DestroyImmediate(dst);
		};
		action2(texture2D, "Diffuse");
		action2(texture2D2, "Normals");
		action2(texture2D3, "Transitions_Diffuse");
		action2(texture2D4, "Transitions_Normals");
		action2(texture2D5, "Transitions_Specular");
	}

	private Texture2D CreateEmptyTexture(int w, int h)
	{
		Texture2D texture2D = new Texture2D(w, h, TextureFormat.ARGB32, mipmap: true, linear: true);
		Color[] pixels = texture2D.GetPixels();
		Array.Clear(pixels, 0, pixels.Length);
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		return texture2D;
	}

	private Texture2D CreateEmptyNormals(int w, int h)
	{
		Texture2D texture2D = new Texture2D(w, h, TextureFormat.ARGB32, mipmap: true, linear: true);
		Color[] pixels = texture2D.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = new Color32(128, 128, byte.MaxValue, byte.MaxValue);
		}
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		return texture2D;
	}

	private void LoadExistingTexture(Texture2D tex, string name)
	{
		if (File.Exists(Path.Combine(mergedTexturesPath, name + ".png")))
		{
			tex.LoadImage(File.ReadAllBytes(Path.Combine(mergedTexturesPath, name)));
			tex.Apply();
		}
		else if (File.Exists(Path.Combine(mergedTexturesPath, name + ".jpg")))
		{
			tex.LoadImage(File.ReadAllBytes(Path.Combine(mergedTexturesPath, name)));
			tex.Apply();
		}
	}

	private void WriteTextureSection(Texture2D src, int startX, int startY, int sizeX, int sizeY, Color[] pixelsOut, int dstWidth, int dstStartX, int dstStartY, int dstSizeX, int dstSizeY, int dstPadding, TextureSection section)
	{
		Color[] pixels = src.GetPixels();
		float num = (float)dstSizeX / (float)sizeX;
		float num2 = (float)dstSizeY / (float)sizeY;
		for (int i = startY; i < startY + sizeY; i++)
		{
			for (int j = startX; j < startX + sizeX; j++)
			{
				Color color = pixels[j + i * src.width];
				Vector4 v = new Vector4(color.r, color.g, color.b, color.a);
				if (v.w < 0.001f)
				{
					continue;
				}
				float num3 = (float)(j - startX) * num;
				float num4 = (float)(i - startY) * num2;
				float num5 = num3 + num;
				float num6 = num4 + num2;
				if ((int)num5 != (int)num3 && (int)num5 < dstSizeX)
				{
					if ((int)num6 != (int)num4 && (int)num6 < dstSizeY)
					{
						section.AddPixel((int)num3, (int)num4, v, ((float)(int)num5 - num3) * ((float)(int)num6 - num4) * num * num2);
						section.AddPixel((int)num3, (int)num6, v, ((float)(int)num5 - num3) * (num6 - (float)(int)num6) * num * num2);
						section.AddPixel((int)num5, (int)num4, v, (num5 - (float)(int)num5) * ((float)(int)num6 - num4) * num * num2);
						section.AddPixel((int)num5, (int)num6, v, (num5 - (float)(int)num5) * (num6 - (float)(int)num6) * num * num2);
					}
					else
					{
						section.AddPixel((int)num3, (int)num4, v, ((float)(int)num5 - num3) * (num6 - num4) * num * num2);
						section.AddPixel((int)num5, (int)num4, v, (num5 - (float)(int)num5) * (num6 - num4) * num * num2);
					}
				}
				else if ((int)num6 != (int)num4 && (int)num6 < dstSizeY)
				{
					section.AddPixel((int)num3, (int)num4, v, (num5 - num3) * ((float)(int)num6 - num4) * num * num2);
					section.AddPixel((int)num3, (int)num6, v, (num5 - num3) * (num6 - (float)(int)num6) * num * num2);
				}
				else
				{
					section.AddPixel((int)num3, (int)num4, v, (num5 - num3) * (num6 - num4) * num * num2);
				}
			}
		}
		for (int k = dstStartY; k < dstStartY + dstSizeY; k++)
		{
			for (int l = dstStartX; l < dstStartX + dstSizeX; l++)
			{
				Vector4 pixel = section.GetPixel(l - dstStartX, k - dstStartY);
				pixelsOut[l + k * dstWidth] = new Color(pixel.x, pixel.y, pixel.z, pixel.w);
			}
		}
		section.Clear();
		for (int m = dstStartY - dstPadding; m < dstStartY + dstSizeY + dstPadding; m++)
		{
			for (int n = dstStartX - dstPadding; n < dstStartX + dstSizeX + dstPadding; n++)
			{
				int num7 = Mathf.Clamp(n, dstStartX, dstStartX + dstSizeX - 1);
				int num8 = Mathf.Clamp(m, dstStartY, dstStartY + dstSizeY - 1);
				if (num7 != n || num8 != m)
				{
					pixelsOut[n + m * dstWidth] = pixelsOut[num7 + num8 * dstWidth];
				}
			}
		}
	}
}
