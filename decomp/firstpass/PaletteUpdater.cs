using System.Collections.Generic;
using System.Linq;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PaletteUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(PaletteUpdater).FullName);

	private const string resourcePath = "SaveUpdater/3to4/3to4Content";

	private static IdGenerator idGen;

	public static List<PaletteUpdater> all;

	public string id;

	public string path;

	public Dictionary<string, int> content;

	public PaletteUpdater()
	{
		content = new Dictionary<string, int>();
	}

	public static void SetIdGen(IdGenerator idGen)
	{
		PaletteUpdater.idGen = idGen;
	}

	public static void PopulateAll(JContainer inputJson)
	{
		all = new List<PaletteUpdater>();
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "ObjetsDynamiques.Palette").Make();
		IEnumerable<JContainer> first = jObjectFetcher.Fetch(inputJson);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "ObjetsDynamiques.Baril").Make();
		IEnumerable<JContainer> second = jObjectFetcher.Fetch(inputJson);
		IEnumerable<JContainer> enumerable = first.Concat(second);
		foreach (JObject item in enumerable)
		{
			PaletteUpdater paletteUpdater = new PaletteUpdater();
			paletteUpdater.id = item["$id"].Value<string>();
			paletteUpdater.path = item["$path"].Value<string>();
			JObjectFetcher jObjectFetcher2 = JObjectFetcher.New().FieldEqualsTo("$type", "System.Collections.Hashtable, mscorlib").Make();
			JObject jhastable = jObjectFetcher2.Fetch(item).First() as JObject;
			paletteUpdater.FillContent(jhastable);
			all.Add(paletteUpdater);
		}
	}

	public static List<string> GetAllContaineeStrings()
	{
		string text = (Resources.Load("SaveUpdater/3to4/3to4Content") as TextAsset).text;
		JContainer defaultContent = JToken.Parse(text) as JContainer;
		List<string> list = new List<string>();
		foreach (PaletteUpdater item in all)
		{
			List<string> contentList = item.GetContentList(defaultContent);
			logger.Info("GetAllContaineeString received " + contentList.Count + " elements from onePalette");
			list.AddRange(contentList);
			logger.Info("GetAllContaineeString now has " + list.Count + " elements in its containee");
		}
		return list;
	}

	private List<string> GetContentList(JContainer DefaultContent)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("StoneBlock", "Blocks.Stone_DoubleBrick");
		dictionary.Add("WoodenBloc", "Blocks.Wood_DoubleLog_AA");
		dictionary.Add("Stones", "Garnottes.Stone1");
		dictionary.Add("Iron", string.Empty);
		dictionary.Add("BlueCrystal", "Garnottes.BlueCrystal1");
		dictionary.Add("Barrels", string.Empty);
		dictionary.Add("TronStone", string.Empty);
		dictionary.Add("OrangeCrystal", "Garnottes.OrangeCrystal1");
		List<string> list = new List<string>();
		foreach (string key in content.Keys)
		{
			if (content[key] > 0)
			{
				JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().AssetKey(dictionary[key]).Make();
				JObject defaultObject = jGameObjectFetcher.Fetch(DefaultContent).First() as JObject;
				List<string> manyTextObjects = GetManyTextObjects(defaultObject, key, content[key]);
				logger.Info("Content List received " + manyTextObjects.Count + "elements from GetMany..");
				list.AddRange(manyTextObjects);
				logger.Info("Conten List now has " + list.Count + "elements");
			}
		}
		return list;
	}

	private List<string> GetManyTextObjects(JObject defaultObject, string type, int qty)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < qty; i++)
		{
			string textObject = defaultObject.ToString();
			textObject = ReplaceParameters(type, textObject);
			list.Add(textObject);
			logger.Info("Added the Following Containee of type:" + type + "\n" + textObject);
		}
		logger.Info("GetManyTextObjects created " + qty + "objects");
		return list;
	}

	private string ReplaceParameters(string type, string textObject)
	{
		string text = textObject;
		text = text.Replace("[" + type + ".Parent]", id);
		text = text.Replace("[" + type + ".Path]", path + "/" + id);
		for (int i = 0; i < 10; i++)
		{
			text = text.Replace("[" + type + ".Id" + i + "]", idGen.GetNewId(isGameObject: false));
		}
		return text;
	}

	private void FillContent(JObject Jhastable)
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		dictionary.Add(1, "StoneBlock");
		dictionary.Add(2, "WoodenBloc");
		dictionary.Add(4, "Stones");
		dictionary.Add(8, "Iron");
		dictionary.Add(16, "BlueCrystal");
		dictionary.Add(32, "Barrels");
		dictionary.Add(64, "TronStone");
		dictionary.Add(128, "OrangeCrystal");
		logger.Info(Jhastable.ToString());
		foreach (int key in dictionary.Keys)
		{
			if (Jhastable.TryGetValue(key.ToString(), out var value))
			{
				int value2 = value.Value<int>();
				logger.Info(dictionary[key] + ":" + value2);
				content.Add(dictionary[key], value2);
			}
		}
	}

	public override string ToString()
	{
		string text = "id: " + id.ToString();
		text += "content:";
		foreach (string key in content.Keys)
		{
			text = text + key + ":" + content[key] + "\n";
		}
		return text;
	}
}
