using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Engine.FlatWorld.Layers;
using Brix.Engine.Volumes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

[Serializable]
public class GridToLayersConverter
{
	public struct GridSectionData
	{
		public string dir;

		public int i;

		public int j;
	}

	public class TempTex : IDisposable
	{
		private Tex _tex = new Tex();

		public int width => _tex.width;

		public int height => _tex.height;

		public Color32[] GetPixels32()
		{
			return _tex.GetPixels32();
		}

		public TempTex(string path)
		{
			_tex.LoadPNG(File.ReadAllBytes(path));
		}

		public void Dispose()
		{
			Tex.Destroy(_tex);
		}
	}

	private const int Grass = 2;

	private const int Sand = 4;

	private const int Earth = 6;

	public Tex heightmap = new Tex();

	public List<XYZ> removedVoxels = new List<XYZ>();

	public string folder;

	public XYZ gridMvSkip = new XYZ(8, 0, 8);

	public XYZ gridMvSize = new XYZ(128, 256, 128);

	public XYZ gridWorldStart = new XYZ(0, 0, 0);

	public XYZ gridWorldSize = new XYZ(1024, 256, 1024);

	public static void Convert(string folder, bool removeOldFiles = true)
	{
		if (Directory.Exists(Path.Combine(folder, "Lod0")))
		{
			GridToLayersConverter gridToLayersConverter = new GridToLayersConverter
			{
				folder = folder
			};
			gridToLayersConverter.Load();
			gridToLayersConverter.Save();
			if (removeOldFiles)
			{
				Directory.Delete(Path.Combine(folder, "Lod0"), recursive: true);
			}
		}
	}

	public IEnumerable<GridSectionData> GetGridSections()
	{
		string lodPath = Path.Combine(folder, "Lod0");
		DirectoryInfo[] directories = new DirectoryInfo(lodPath).GetDirectories();
		foreach (DirectoryInfo dirInfo in directories)
		{
			string dir = dirInfo.Name;
			string[] parts = dir.Split(' ');
			if (parts.Length != 4)
			{
				Debug.Log("GridToLayersConverter: Ignoring dir '" + dir + "'");
				continue;
			}
			int i2 = System.Convert.ToInt32(parts[1]);
			int j = System.Convert.ToInt32(parts[3]);
			yield return new GridSectionData
			{
				dir = dirInfo.FullName,
				i = i2,
				j = j
			};
		}
	}

	public XYZ WorldCoord(XYZ gridCoord)
	{
		return gridWorldStart + XYZ.Mul(gridMvSize - gridMvSkip * 2, gridCoord);
	}

	public void LoadSettings()
	{
		GridSectionData[] array = GetGridSections().ToArray();
		using (TempTex tempTex = new TempTex(Path.Combine(array[0].dir, "Filling.png")))
		{
			gridMvSize = new XYZ(tempTex.width, 256, tempTex.height);
		}
		int x = array.Select((GridSectionData s) => s.i).Max() + 1;
		int z = array.Select((GridSectionData s) => s.j).Max() + 1;
		gridWorldSize = WorldCoord(new XYZ(x, 1, z)) - gridWorldStart;
	}

	public void LoadColumns(Color32[] fillingColors, GridSectionData section)
	{
		int x = gridWorldSize.x;
		XYZ xYZ = new XYZ(section.i, 0, section.j);
		XYZ xYZ2 = WorldCoord(xYZ);
		XYZ xYZ3 = XYZ.Min(WorldCoord(xYZ + XYZ.one), gridWorldStart + gridWorldSize);
		XYZ xYZ4 = xYZ3 - xYZ2;
		string dir = section.dir;
		using TempTex tempTex = new TempTex(Path.Combine(dir, "Filling.png"));
		Color32[] pixels = tempTex.GetPixels32();
		Soft.Blit32(pixels, tempTex.width, fillingColors, x, gridMvSkip.x, gridMvSkip.z, xYZ2.x, xYZ2.z, xYZ4.x, xYZ4.z);
	}

	public void LoadTypeVolume(Color32[] fillingColors, GridSectionData section)
	{
		int x = gridWorldSize.x;
		XYZ xYZ = new XYZ(section.i, 0, section.j);
		XYZ xYZ2 = WorldCoord(xYZ);
		XYZ xYZ3 = XYZ.Min(WorldCoord(xYZ + XYZ.one), gridWorldStart + gridWorldSize);
		string dir = section.dir;
		using TempTex tempTex = new TempTex(Path.Combine(dir, "Offset.png"));
		using TempTex tempTex2 = new TempTex(Path.Combine(dir, "Type.png"));
		using TempTex tempTex3 = new TempTex(Path.Combine(dir, "Volume.png"));
		using TempTex tempTex4 = new TempTex(Path.Combine(dir, "Depth.png"));
		Color32[] pixels = tempTex.GetPixels32();
		Color32[] pixels2 = tempTex2.GetPixels32();
		Color32[] pixels3 = tempTex3.GetPixels32();
		Color32[] pixels4 = tempTex4.GetPixels32();
		for (int i = 0; i < pixels.Length; i++)
		{
			XYZ xYZ4 = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i])) + xYZ2 - gridMvSkip;
			if (xYZ4.x < xYZ2.x || xYZ4.x >= xYZ3.x || xYZ4.z < xYZ2.z || xYZ4.z >= xYZ3.z)
			{
				continue;
			}
			int num = xYZ4.x + xYZ4.z * x;
			if (xYZ4.y >= fillingColors[num].r && xYZ4.y < fillingColors[num].g && pixels4[i].r == 1)
			{
				fillingColors[num].b = pixels2[i].r;
				fillingColors[num].a = (byte)(256 - pixels3[i].r);
				if (fillingColors[num].b != 2 && fillingColors[num].b != 4 && fillingColors[num].b != 6)
				{
					fillingColors[num].b = 4;
				}
			}
		}
	}

	public void FindRemovedVoxels(Color32[] fillingColors, GridSectionData section, List<XYZ> removedVoxels)
	{
		int x = gridWorldSize.x;
		XYZ xYZ = new XYZ(section.i, 0, section.j);
		XYZ xYZ2 = WorldCoord(xYZ);
		XYZ xYZ3 = XYZ.Min(WorldCoord(xYZ + XYZ.one), gridWorldStart + gridWorldSize);
		string dir = section.dir;
		using TempTex tempTex = new TempTex(Path.Combine(dir, "Offset.png"));
		using TempTex tempTex2 = new TempTex(Path.Combine(dir, "Type.png"));
		Color32[] pixels = tempTex.GetPixels32();
		Color32[] pixels2 = tempTex2.GetPixels32();
		for (int i = 0; i < pixels.Length; i++)
		{
			XYZ item = Soft.DecodeXYZ(Soft.DecodeInt(pixels[i])) + xYZ2 - gridMvSkip;
			if (item.x >= xYZ2.x && item.x < xYZ3.x && item.z >= xYZ2.z && item.z < xYZ3.z)
			{
				int num = item.x + item.z * x;
				if (item.y >= fillingColors[num].r && item.y < fillingColors[num].g && pixels2[i].r == 0)
				{
					removedVoxels.Add(item);
				}
			}
		}
	}

	public void ApplyConstructionChange(Color32[] fillingColors, XYZ p, bool removed)
	{
		int x = gridWorldSize.x;
		int num = p.x + p.z * x;
		if (p.y == fillingColors[num].g)
		{
			fillingColors[num].b = 6;
		}
		if (p.y >= fillingColors[num].r && p.y < fillingColors[num].g)
		{
			removedVoxels.Add(p);
		}
	}

	public void ApplyConstructionChanges(Color32[] fillingColors, byte[] b)
	{
		for (int i = 1; i < b.Length; i += 8)
		{
			int scalaire = (b[i] << 24) + (b[i + 1] << 16) + (b[i + 2] << 8) + b[i + 3];
			int num = (b[i + 4] << 24) + (b[i + 5] << 16) + (b[i + 6] << 8) + b[i + 7];
			ApplyConstructionChange(fillingColors, XYZ.FromScalaire(scalaire), num == 0);
		}
	}

	private byte[] GetVoxelsDeltaV2(string path)
	{
		if (!File.Exists(path))
		{
			return new byte[0];
		}
		string value = File.ReadAllText(path);
		Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(value);
		JObject jObject = (JObject)hashtable["VoxelData"];
		return System.Convert.FromBase64String(((string)jObject["$value"]) ?? "");
	}

	public void Load()
	{
		string path = Path.Combine(folder, "Heightmap.png");
		bool flag = !File.Exists(path) && false;
		if (flag)
		{
			heightmap.LoadPNG(File.ReadAllBytes(path));
		}
		else
		{
			Soft.Clear(heightmap, gridWorldSize.x, gridWorldSize.z);
		}
		removedVoxels.Clear();
		Tex tex = heightmap;
		Color32[] pixels = tex.GetPixels32();
		GridSectionData[] array = GetGridSections().ToArray();
		GridSectionData[] array2 = array;
		foreach (GridSectionData section in array2)
		{
			if (!flag)
			{
				LoadColumns(pixels, section);
				LoadTypeVolume(pixels, section);
			}
			FindRemovedVoxels(pixels, section, removedVoxels);
			ApplyConstructionChanges(pixels, GetVoxelsDeltaV2(Path.Combine(folder, "Monde_Voxels.json")));
		}
		tex.SetPixels32(pixels, gridWorldSize.x, gridWorldSize.z);
	}

	public void Save()
	{
		string path = Path.Combine(folder, "Heightmap.png");
		File.WriteAllBytes(path, heightmap.EncodeToPNG());
		NegativeLayer negativeLayer = new NegativeLayer();
		negativeLayer.removedSet.AddCustomRegion(new VoxelSet(removedVoxels));
		negativeLayer.Save(folder);
	}
}
