using System.Collections.Generic;
using System.Linq;
using Brix.IO.Saves;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BriktronUpdater
{
	private const string resourcePath = "SaveUpdater/3to4/3to4Briktron";

	private static IdGenerator idGen;

	public static List<BriktronUpdater> all;

	public string name;

	public float X;

	public float Y;

	public float Z;

	public static void SetIdGen(IdGenerator _idGen)
	{
		idGen = _idGen;
	}

	public static void PopulateAll(JContainer inputJson)
	{
		all = new List<BriktronUpdater>();
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Personnages.Bricktron").Make();
		IEnumerable<JContainer> enumerable = jObjectFetcher.Fetch(inputJson);
		foreach (JObject item in enumerable)
		{
			BriktronUpdater briktronUpdater = new BriktronUpdater();
			briktronUpdater.name = item["name"].Value<string>();
			JObjectFetcher jObjectFetcher2 = JObjectFetcher.New().FieldEqualsTo("$type", "UnityEngine.Transform, UnityEngine").Make();
			JObject position = jObjectFetcher2.Fetch(item).First() as JObject;
			briktronUpdater.SetPosition(position);
			all.Add(briktronUpdater);
		}
	}

	private void SetPosition(JObject transform)
	{
		JObject jObject = transform["position"].Value<JObject>();
		X = jObject["x"].Value<float>();
		Y = jObject["y"].Value<float>();
		Z = jObject["z"].Value<float>();
	}

	public static List<string> GetAllBriktronStrings()
	{
		string text = (Resources.Load("SaveUpdater/3to4/3to4Briktron") as TextAsset).text;
		JContainer defaultContent = JToken.Parse(text) as JContainer;
		List<string> list = new List<string>();
		foreach (BriktronUpdater item in all)
		{
			string content = item.GetContent(defaultContent);
			list.Add(content);
		}
		return list;
	}

	private string GetContent(JContainer DefaultContent)
	{
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().AssetKey("Test.Bricktron").Make();
		JObject jObject = jGameObjectFetcher.Fetch(DefaultContent).First() as JObject;
		string textObject = jObject.ToString();
		return ReplaceParameters(textObject);
	}

	private string ReplaceParameters(string textObject)
	{
		string text = textObject;
		text = text.Replace("[Name]", name);
		text = text.Replace("\"[X]\"", X.ToString());
		text = text.Replace("\"[Y]\"", Y.ToString());
		text = text.Replace("\"[Z]\"", Z.ToString());
		for (int i = 0; i < 15; i++)
		{
			text = text.Replace("[Id" + i + "]", idGen.GetNewId(isGameObject: false));
		}
		return text;
	}

	private string IdGen()
	{
		return "Oh Dear Favian Rescue Me!";
	}

	public override string ToString()
	{
		string text = "id: " + name;
		text = text + "X: " + X + "\n";
		text = text + "Y: " + Y + "\n";
		return text + "Z: " + Z + "\n";
	}
}
