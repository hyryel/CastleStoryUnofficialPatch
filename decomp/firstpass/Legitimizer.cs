using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Brix.Assets;
using Newtonsoft.Json;

public static class Legitimizer
{
	public const string ChecksumFilePath = "Info/Checksum/gamemodes.json";

	public static string gameModePath = "Info/Lua/Gamemodes";

	public static bool GameModeIsLegit(string modeCodeName)
	{
		Legitimization item = GenerateOneHashForAllFilesIn(AssetDispensary.gamemodes.SendCodenameQuery(modeCodeName).path);
		List<Legitimization> list = LoadLegitFile();
		return list.Contains(item);
	}

	public static bool PresetIsLegit(string presetCodeName)
	{
		string path = AssetDispensary.presets.SendCodenameQuery(presetCodeName).path;
		Legitimization item = GenerateOneHashForAllFilesIn(path);
		List<Legitimization> list = LoadLegitFile();
		return list.Contains(item);
	}

	public static void SaveAllGamemodeAndPresetsHashes()
	{
		List<Legitimization> list = new List<Legitimization>();
		list.AddRange(GetGameModesHashes());
		list.AddRange(GetAllPresetHashes());
		string contents = JsonConvert.SerializeObject(list, Formatting.Indented);
		File.WriteAllText("Info/Checksum/gamemodes.json", contents);
	}

	private static List<Legitimization> LoadLegitFile()
	{
		return JsonConvert.DeserializeObject<List<Legitimization>>(File.ReadAllText("Info/Checksum/gamemodes.json"));
	}

	private static List<Legitimization> GetGameModesHashes()
	{
		return Directory.GetDirectories(gameModePath).Select(GenerateOneHashForAllFilesIn).ToList();
	}

	private static List<Legitimization> GetAllPresetHashes()
	{
		List<Legitimization> list = new List<Legitimization>();
		string[] directories = Directory.GetDirectories(gameModePath);
		foreach (string modepath in directories)
		{
			list.AddRange(GetPresetsHashes(modepath));
		}
		return list;
	}

	private static List<Legitimization> GetPresetsHashes(string modepath)
	{
		string path = modepath + "/Presets/";
		return GenerateAllHashesForFilesIn(path);
	}

	public static Legitimization GenerateOneHashForAllFilesIn(string path)
	{
		string[] files = Directory.GetFiles(path);
		Array.Sort(files, new CaseInsensitiveComparer());
		string input = files.Aggregate(string.Empty, (string total, string path_) => total + File.ReadAllText(path_));
		Legitimization legitimization = new Legitimization();
		legitimization.path = path;
		legitimization.hash = CalculateMD5Hash(input);
		return legitimization;
	}

	private static List<Legitimization> GenerateAllHashesForFilesIn(string path)
	{
		return Directory.GetFiles(path).Select(GenerateHashForFileAt).ToList();
	}

	private static Legitimization GenerateHashForFileAt(string path)
	{
		Legitimization legitimization = new Legitimization();
		legitimization.path = path;
		string input = File.ReadAllText(path);
		legitimization.hash = CalculateMD5Hash(input);
		return legitimization;
	}

	private static bool HashesAreSame(string official, string underTest)
	{
		string text = CalculateMD5Hash(official);
		string text2 = CalculateMD5Hash(underTest);
		return text == text2;
	}

	private static string CalculateMD5Hash(string input)
	{
		MD5 mD = MD5.Create();
		byte[] bytes = Encoding.ASCII.GetBytes(input);
		byte[] array = mD.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}
}
