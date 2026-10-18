using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.IO.Saves;
using Brix.Util;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SaveUpdater8to9 : SaveUpdater
{
	private enum blocksId
	{
		Stone_Arch = 18,
		Stone_Brick = 10,
		Stone_Brick_Round = 62,
		Stone_Column_Base = 48,
		Stone_Column_Middle = 50,
		Stone_Column_Top = 52,
		Stone_DoubleBrick = 14,
		Stone_DoubleBrick_RoundL = 58,
		Stone_DoubleBrick_RoundLR = 56,
		Stone_DoubleBrick_RoundR = 60,
		Stone_Staircase = 16
	}

	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater8to9).FullName);

	private List<XYZ> blocksToPostProcess = new List<XYZ>();

	public SaveUpdater8to9(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("meta.json", isArray: false);
		fileUpdater.AddStep(FixVersion);
		fileUpdater.AddStep(ConvertTrees);
		fileUpdater.AddStep(ConvertLegacyBlocks);
		FileUpdater fileUpdater2 = AddFile("gameobjects.json", isArray: true);
		fileUpdater2.AddStep(FixSiegeProjectId);
		fileUpdater2.AddStep(FixCorruptronSpawnGroupId);
		logger.Info("Save updater 8to9 succesfully loaded for: " + saveFile.path);
	}

	private JContainer FixCorruptronSpawnGroupId(JContainer inputJson, IdGenerator idgen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Name("CorruptronSpawnGroup").Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().Id("go:CorruptronSpawnGroup").Make();
		return jGameObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixSiegeProjectId(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Name("SiegeProject").Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().Id("go:SiegeProject").Make();
		return jGameObjectModifier.Apply(fetcher, inputJson);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["version"].Value<int>();
		return num == 8;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", "9").Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer ConvertTrees(JContainer inputjson, IdGenerator idgen)
	{
		Asset_Map.MapType mapType = (File.Exists(saveFile.path + "/meta/info.txt") ? Asset_Map.MapType.Map : Asset_Map.MapType.Save);
		string path = Path.Combine(saveFile.path, "Monde_Arbre");
		if (!File.Exists(path))
		{
			return inputjson;
		}
		int num = 0;
		byte[] array = File.ReadAllBytes(path);
		XYZ[] array2 = new XYZ[array.Length / 4];
		for (int i = 0; i < array.Length; i += 4)
		{
			int scalaire = array[i] + (array[i + 1] << 8) + (array[i + 2] << 16) + (array[i + 3] << 24);
			ref XYZ reference = ref array2[num++];
			reference = XYZ.FromScalaire(scalaire);
		}
		Asset_Map asset_Map = saveFile;
		Asset_Map.MapType meta = mapType;
		XYZ[] treePositions = array2;
		asset_Map.Disk_WriteSpecific(meta, voxelEngine: false, thumbnail: false, treePositions);
		File.Delete(path);
		return inputjson;
	}

	private JContainer ConvertLegacyBlocks(JContainer inputjson, IdGenerator idgen)
	{
		string path = Path.Combine(saveFile.path, "Monde_Voxels.json");
		if (!File.Exists(path))
		{
			return inputjson;
		}
		byte[] array = DeserializeLegacyBlocksData(path);
		Dictionary<XYZ, BlockDataStruct> dictionary = new Dictionary<XYZ, BlockDataStruct>(array.Length / 8);
		for (int i = 1; i < array.Length; i += 8)
		{
			int index = (array[i] << 24) + (array[i + 1] << 16) + (array[i + 2] << 8) + array[i + 3];
			int info = (array[i + 4] << 24) + (array[i + 5] << 16) + (array[i + 6] << 8) + array[i + 7];
			ConvertLegacyBlockToBlockDataStruct(index, info, dictionary);
		}
		WriteNewBlocksData(GetPostProcessedPlacedBlocks(blocksToPostProcess, dictionary));
		File.Delete(path);
		return inputjson;
	}

	private void WriteNewBlocksData(BlockDataStruct[] bds)
	{
		Asset_Map.MapType mapType = (File.Exists(saveFile.path + "/meta/info.txt") ? Asset_Map.MapType.Map : Asset_Map.MapType.Save);
		Asset_Map asset_Map = saveFile;
		Asset_Map.MapType meta = mapType;
		asset_Map.Disk_WriteSpecific(meta, voxelEngine: false, thumbnail: false, null, null, null, null, bds);
	}

	private byte[] DeserializeLegacyBlocksData(string path)
	{
		string value = File.ReadAllText(path);
		Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(value);
		JObject jObject = (JObject)hashtable["VoxelData"];
		return Convert.FromBase64String(string.Empty + (string)jObject["$value"]);
	}

	private void ConvertLegacyBlockToBlockDataStruct(int index, int info, Dictionary<XYZ, BlockDataStruct> bdsList)
	{
		int num = Brix.Engine.Abstract.Voxel._GetTypeBloc(info);
		if ((num & 1) == 1 || num == 0)
		{
			return;
		}
		int num2 = (info >> 11) & 3;
		Quaternion quaternion = Calcule.FaceIndexToRotationDeBase((byte)(num2 + 1));
		XYZ xYZ = XYZ.FromScalaire(index);
		if (IsLegacyWoodSystem(num))
		{
			ConvertLegacyWoodToBlockDataStruct(xYZ, quaternion, num, bdsList);
			return;
		}
		BlockInfo blockInfo = num.BlockInfoIDToBlockInfo();
		if (blockInfo != null && blockInfo.Type != BlockType.Terrain)
		{
			bdsList.Add(xYZ, new BlockDataStruct(xYZ, quaternion, 0f, num));
		}
	}

	private void ConvertLegacyWoodToBlockDataStruct(XYZ position, Quaternion rotation, int type, Dictionary<XYZ, BlockDataStruct> bdsList)
	{
		switch (type)
		{
		case 22:
		{
			bdsList.Add(position, new BlockDataStruct(position, rotation, 0f, 70));
			blocksToPostProcess.Add(position);
			XYZ xYZ2 = position + XYZ.FromVector3(rotation * Vector3.forward);
			bdsList.Add(xYZ2, new BlockDataStruct(xYZ2, rotation, 0f, 70));
			blocksToPostProcess.Add(xYZ2);
			break;
		}
		case 28:
			bdsList.Add(position, new BlockDataStruct(position, Quaternion.identity, 0f, 66));
			bdsList.Add(position + XYZ.up, new BlockDataStruct(position + XYZ.up, Quaternion.identity, 0f, 66));
			break;
		case 36:
		{
			bdsList.Add(position, new BlockDataStruct(position, rotation, 0f, 68));
			XYZ xYZ = position + XYZ.FromVector3(rotation * Vector3.forward);
			bdsList.Add(xYZ, new BlockDataStruct(xYZ, rotation, 0f, 68));
			break;
		}
		}
	}

	private BlockDataStruct[] GetPostProcessedPlacedBlocks(List<XYZ> toPostProcess, Dictionary<XYZ, BlockDataStruct> indexedBds)
	{
		BlockDataStruct[] array = new BlockDataStruct[indexedBds.Count + toPostProcess.Count];
		int num = 0;
		foreach (KeyValuePair<XYZ, BlockDataStruct> indexedBd in indexedBds)
		{
			ref BlockDataStruct reference = ref array[num++];
			reference = indexedBd.Value;
		}
		for (int i = 0; i < toPostProcess.Count; i++)
		{
			XYZ xYZ = toPostProcess[i];
			if (HasSupportingBlockUnder(indexedBds, xYZ))
			{
				ref BlockDataStruct reference2 = ref array[num];
				reference2 = new BlockDataStruct(xYZ, indexedBds[xYZ + XYZ.right].R, 0f, 22);
				indexedBds[xYZ] = array[num++];
			}
			else if (HasPlankFacingMeAt(indexedBds, xYZ, XYZ.right))
			{
				ref BlockDataStruct reference3 = ref array[num];
				reference3 = new BlockDataStruct(xYZ, indexedBds[xYZ + XYZ.right].R, 0f, 68);
				indexedBds[xYZ] = array[num++];
			}
			else if (HasPlankFacingMeAt(indexedBds, xYZ, -XYZ.right))
			{
				ref BlockDataStruct reference4 = ref array[num];
				reference4 = new BlockDataStruct(xYZ, indexedBds[xYZ - XYZ.right].R, 0f, 68);
				indexedBds[xYZ] = array[num++];
			}
			else if (HasPlankFacingMeAt(indexedBds, xYZ, XYZ.forward))
			{
				ref BlockDataStruct reference5 = ref array[num];
				reference5 = new BlockDataStruct(xYZ, indexedBds[xYZ + XYZ.forward].R, 0f, 68);
				indexedBds[xYZ] = array[num++];
			}
			else if (HasPlankFacingMeAt(indexedBds, xYZ, -XYZ.forward))
			{
				ref BlockDataStruct reference6 = ref array[num];
				reference6 = new BlockDataStruct(xYZ, indexedBds[xYZ - XYZ.forward].R, 0f, 68);
				indexedBds[xYZ] = array[num++];
			}
			else
			{
				ref BlockDataStruct reference7 = ref array[num];
				reference7 = new BlockDataStruct(xYZ, indexedBds[xYZ].R, 0f, 68);
				indexedBds[xYZ] = array[num++];
			}
		}
		return array;
	}

	private bool HasSupportingBlockUnder(Dictionary<XYZ, BlockDataStruct> indexedBds, XYZ position)
	{
		List<int> list = new List<int>();
		list.Add(18);
		list.Add(10);
		list.Add(62);
		list.Add(48);
		list.Add(50);
		list.Add(52);
		list.Add(14);
		list.Add(58);
		list.Add(56);
		list.Add(60);
		list.Add(16);
		List<int> list2 = list;
		foreach (int item in list2)
		{
			if (indexedBds.TryGetValue(position + -XYZ.up, out var value) && value.type == item)
			{
				XYZ xYZ = XYZ.FromVector3(value.R * Vector3.forward);
				return xYZ + -XYZ.up == XYZ.zero || xYZ - -XYZ.up == XYZ.zero;
			}
		}
		return false;
	}

	private static bool HasPlankFacingMeAt(Dictionary<XYZ, BlockDataStruct> indexedBds, XYZ newPlankPosition, XYZ neighborRelativePosition)
	{
		if (indexedBds.TryGetValue(newPlankPosition + neighborRelativePosition, out var value) && value.type == 68)
		{
			XYZ xYZ = XYZ.FromVector3(value.R * Vector3.forward);
			return xYZ + neighborRelativePosition == XYZ.zero || xYZ - neighborRelativePosition == XYZ.zero;
		}
		return false;
	}

	private bool IsLegacyWoodSystem(int i)
	{
		return i == 28 || i == 22 || i == 36;
	}
}
