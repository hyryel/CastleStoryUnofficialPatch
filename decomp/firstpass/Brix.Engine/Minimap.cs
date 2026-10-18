using System;
using UnityEngine;

namespace Brix.Engine;

public class Minimap
{
	public static Color earthColor;

	public static Color grassColor;

	public static Color thickGrassColor;

	public static Color cloverColor;

	public static Color snowColor;

	public static Color pavementColor;

	public static Color[] typeColors;

	static Minimap()
	{
		earthColor = new Color(0.4392157f, 0.36078432f, 0.21960784f, 1f);
		grassColor = new Color(14f / 85f, 0.5803922f, 0.101960786f, 1f);
		thickGrassColor = new Color(8f / 51f, 0.4f, 4f / 15f, 1f);
		cloverColor = new Color(0.22745098f, 42f / 85f, 12f / 85f, 1f);
		snowColor = new Color(0.76862746f, 74f / 85f, 13f / 15f, 1f);
		pavementColor = new Color(33f / 85f, 33f / 85f, 0.34901962f, 1f);
		typeColors = new Color[256];
		for (int i = 0; i < 256; i++)
		{
			ref Color reference = ref typeColors[i];
			reference = earthColor;
		}
		ref Color reference2 = ref typeColors[2];
		reference2 = grassColor;
		ref Color reference3 = ref typeColors[3];
		reference3 = snowColor;
		ref Color reference4 = ref typeColors[5];
		reference4 = pavementColor;
		ref Color reference5 = ref typeColors[9];
		reference5 = cloverColor;
		ref Color reference6 = ref typeColors[7];
		reference6 = thickGrassColor;
	}

	private static void DrawTerrainType(Color[] pixels, int width)
	{
		foreach (XYZ topVoxelPositionsFromLayer in Voxel.Engine.GetTopVoxelPositionsFromLayers())
		{
			int iD = Voxel.GetTerrainBlockData(topVoxelPositionsFromLayer).BlockInfo.ID;
			Color color = typeColors[iD];
			pixels[topVoxelPositionsFromLayer.x + topVoxelPositionsFromLayer.z * width] = color;
		}
	}

	private static void DrawTerrainHeight(Color[] pixels, int width, int yMin = 0, int yMax = 255)
	{
		foreach (XYZ topVoxelPositionsFromLayer in Voxel.Engine.GetTopVoxelPositionsFromLayers())
		{
			float num = (float)(topVoxelPositionsFromLayer.y - yMin) / (float)(yMax - yMin);
			ref Color reference = ref pixels[topVoxelPositionsFromLayer.x + topVoxelPositionsFromLayer.z * width];
			reference = new Color(num, num, 0f, 1f);
		}
	}

	private static void DrawTerrainSideView(Color[] pixels, int width)
	{
		foreach (XYZ visibleVoxelPosition in Voxel.Engine.GetVisibleVoxelPositions())
		{
			ref Color reference = ref pixels[visibleVoxelPosition.x + visibleVoxelPosition.z * width];
			reference = grassColor;
		}
	}

	public static Texture2D GetTerrainTypeMinimap(Texture2D resultBuffer = null)
	{
		Texture2D texture2D = resultBuffer ?? new Texture2D(1024, 1024);
		Color[] pixels = texture2D.GetPixels();
		Array.Clear(pixels, 0, pixels.Length);
		DrawTerrainType(pixels, texture2D.width);
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D GetTerrainSideViewMap(Texture2D resultBuffer = null)
	{
		Texture2D texture2D = resultBuffer ?? new Texture2D(1024, 256);
		Color[] pixels = texture2D.GetPixels();
		Array.Clear(pixels, 0, pixels.Length);
		DrawTerrainSideView(pixels, texture2D.width);
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		return texture2D;
	}

	public static Bounds GetTerrainBounds(Texture2D tex)
	{
		Color32[] pixels = tex.GetPixels32(3);
		int num = tex.width >> 3;
		XYZ a = new XYZ(1024, 256, 1024);
		XYZ xYZ = XYZ.zero;
		for (int i = 0; i < pixels.Length; i++)
		{
			Color32 color = pixels[i];
			if (color.g > color.r)
			{
				XZ xZ = new XZ(i % num, i / num) << 3;
				a = XYZ.Min(a, new XYZ(xZ.x, color.r, xZ.z));
				xYZ = XYZ.Max(xYZ, new XYZ(xZ.x, color.g - 1, xZ.z));
			}
		}
		xYZ.x += 7;
		xYZ.z += 7;
		return new VoxelBounds(a, xYZ).ToVectorBounds();
	}

	public static Rect GetScaleAndOffsetFromBounds(Bounds bounds, float padding = 0f)
	{
		Bounds bounds2 = bounds;
		bounds2.size = Vector3.one * (Mathf.Max(bounds2.size.x, bounds2.size.z) + padding);
		Vector2 vector = new Vector2(bounds2.size.x, bounds2.size.z) / 1024f;
		Vector2 vector2 = new Vector2(bounds2.min.x, bounds2.min.z) / 1024f;
		return new Rect(vector2.x, vector2.y, vector.x, vector.y);
	}
}
