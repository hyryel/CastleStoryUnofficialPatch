using System.Collections.Generic;
using System.Linq;
using Brix.Engine.Abstract;
using Brix.IO.Saves;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GroupeUpdater
{
	private static string[] GROUPS_IN = new string[2] { "Structures", "Blocs" };

	private static Dictionary<string, string> GROUPS_OUT = new Dictionary<string, string>
	{
		{ "Structures", "Groupes.Structures" },
		{ "Blocs", "Groupes.Blocks" }
	};

	private static IdGenerator idGen;

	private const string resourcePath = "SaveUpdater/3to4/3to4Groups";

	public string name;

	public string id;

	public Vector3 position;

	public string newId;

	public static List<GroupeUpdater> all;

	private JContainer defaultGroups;

	public GroupeUpdater(string _name, string _id, Vector3 _position)
	{
		name = _name;
		id = _id;
		position = _position;
	}

	public static void SetIdGen(IdGenerator _idGen)
	{
		idGen = _idGen;
	}

	public static void PopulateAll(JContainer inputJson)
	{
		all = new List<GroupeUpdater>();
		string[] gROUPS_IN = GROUPS_IN;
		foreach (string wantedValue in gROUPS_IN)
		{
			JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("description", "Construire").FieldEqualsTo("tag", wantedValue)
				.Make();
			JContainer firstJson = UpdateUtils.getFirstJson(fetcher, inputJson);
			if (firstJson != null)
			{
				string text = firstJson.Value<string>("id");
				JObjectFetcher fetcher2 = JObjectFetcher.New().FieldEqualsTo("$type", "Boo.Lang.Hash, Boo.Lang").Make();
				JContainer firstJson2 = UpdateUtils.getFirstJson(fetcher2, firstJson);
				if (firstJson2 != null)
				{
					int nombre = firstJson2.Value<int>("index");
					all.Add(new GroupeUpdater(wantedValue, text, Voxel.GetVecteur(nombre)));
				}
			}
		}
	}

	public static List<string> GetGroupStrings()
	{
		List<string> list = new List<string>();
		string text = (Resources.Load("SaveUpdater/3to4/3to4Groups") as TextAsset).text;
		JContainer jContainer = JToken.Parse(text) as JContainer;
		foreach (GroupeUpdater item in all)
		{
			string content = item.GetContent(jContainer);
			list.Add(content);
		}
		return list;
	}

	private string GetContent(JContainer defaultGroups)
	{
		string assetkey = GROUPS_OUT[name];
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().AssetKey(assetkey).Make();
		JObject jObject = jGameObjectFetcher.Fetch(defaultGroups).First() as JObject;
		string textJson = jObject.ToString();
		return ReplaceParameters(textJson);
	}

	private string ReplaceParameters(string textJson)
	{
		string text = textJson;
		text = text.Replace("\"[X]\"", position.x.ToString());
		text = text.Replace("\"[Y]\"", position.y.ToString());
		text = text.Replace("\"[Z]\"", position.z.ToString());
		text = text.Replace("\"[Index]\"", Voxel.GetScalaire(position).ToString());
		for (int i = 0; i < 15; i++)
		{
			string newValue = idGen.GetNewId(isGameObject: false);
			text = text.Replace("[Id" + i + "]", newValue);
			if (i == 0)
			{
				newId = newValue;
			}
		}
		return text;
	}
}
