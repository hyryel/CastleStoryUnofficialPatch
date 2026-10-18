using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Assets;
using Brix.External.Factories;
using Brix.IO.Saves;
using Brix.IO.Serialization.Json;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;
using Smooth.Algebraics;

public class SaveUpdater22to23 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater22to23).FullName);

	public static Factory.AssetKey Idle = new Factory.AssetKey("Groupes", "Idle");

	public static Factory.AssetKey Construction = new Factory.AssetKey("Groupes", "Construction");

	public static Factory.AssetKey Mine = new Factory.AssetKey("Groupes", "Mine");

	public static Factory.AssetKey Cleanup = new Factory.AssetKey("Groupes", "Cleanup");

	public static Factory.AssetKey Repair = new Factory.AssetKey("Groupes", "Repair");

	public static Factory.AssetKey Landscape = new Factory.AssetKey("Groupes", "Landscape");

	public static Factory.AssetKey Harvest = new Factory.AssetKey("Groupes", "Harvest");

	public static Factory.AssetKey Character = new Factory.AssetKey("Test", "Bricktron");

	public static string affiliationType = "Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass";

	public SaveUpdater22to23(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(MoveGroupPositionToNetworkPosition);
		fileUpdater.AddStep(RemoveRepairProject);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	private JContainer MoveGroupPositionToNetworkPosition(JContainer inputJson, IdGenerator idGen)
	{
		List<Factory.AssetKey> list = new List<Factory.AssetKey>();
		list.Add(Idle);
		list.Add(Construction);
		list.Add(Mine);
		list.Add(Cleanup);
		list.Add(Landscape);
		list.Add(Harvest);
		List<Tuple<string, JObject>> list2 = new List<Tuple<string, JObject>>();
		JComponentFetcher.JComponentFetcherBuilder jComponentFetcherBuilder = JComponentFetcher.New().Type("UnityEngine.Transform, UnityEngine");
		foreach (Factory.AssetKey item in list)
		{
			JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().AssetKey(item.ToString()).Make();
			foreach (JObject item2 in jGameObjectFetcher.Fetch(inputJson).OfType<JObject>())
			{
				string text = item2.Id();
				JObject jObject = jComponentFetcherBuilder.NestedIn(JGameObjectFetcher.New().Id(text).Make()).Make().Fetch(inputJson)
					.OfType<JObject>()
					.FirstOrDefault();
				if (jObject != null)
				{
					list2.Add(Tuple.Create(text, jObject));
				}
			}
		}
		foreach (Tuple<string, JObject> item3 in list2)
		{
			JGameObjectFetcher jGameObjectFetcher2 = JGameObjectFetcher.New().Id(item3._1).Make();
			JGameObjectFetcher goFetcher = JGameObjectFetcher.New().ParentIs(jGameObjectFetcher2).HasComponentOfType("Brix.Legacy.Beacon, Assembly-CSharp-firstpass")
				.HasNoComponentOfType("UnityEngine.Transform, UnityEngine")
				.Make();
			inputJson = RemoveJTransform(inputJson, jGameObjectFetcher2);
			inputJson = AddJTransform(inputJson, goFetcher, item3._2);
		}
		return inputJson;
	}

	private JContainer RemoveRepairProject(JContainer inputJson, IdGenerator idGen)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		List<JObject> list = JGameObjectFetcher.New().AssetKey(Idle.ToString()).Make()
			.Fetch(inputJson)
			.OfType<JObject>()
			.ToList();
		foreach (JObject item in list)
		{
			JGameObjectFetcher filter = JGameObjectFetcher.New().Id(item.Id()).Make();
			JObject jObject = JComponentFetcher.New().Type(affiliationType).NestedIn(filter)
				.Make()
				.Fetch(inputJson)
				.OfType<JObject>()
				.FirstOrDefault();
			JObject jobject = jObject["faction"] as JObject;
			string key = jobject.Ref() ?? jobject.Id();
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, item.Id());
			}
		}
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().AssetKey(Repair.ToString()).Make();
		List<JObject> list2 = jGameObjectFetcher.Fetch(inputJson).OfType<JObject>().ToList();
		foreach (JObject item2 in list2)
		{
			JGameObjectFetcher jGameObjectFetcher2 = JGameObjectFetcher.New().Id(item2.Id()).Make();
			List<JObject> list3 = JGameObjectFetcher.New().AssetKey(Character.ToString()).NestedIn(jGameObjectFetcher2)
				.Make()
				.Fetch(inputJson)
				.OfType<JObject>()
				.ToList();
			if (list3.Count == 0)
			{
				continue;
			}
			string text = FindMatchingIdleProjectID(inputJson, jGameObjectFetcher2, dictionary);
			JGameObjectFetcher injectionFetcher = JGameObjectFetcher.New().Id(text).Make();
			string tokenToFind = $"/Jeu/Groupes/{item2.Id()}";
			string text2 = $"/Jeu/Groupes/{text}/Characters";
			JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().InjectJGameObject("$parent", injectionFetcher).Path(text2)
				.Make();
			foreach (JObject item3 in list3)
			{
				inputJson = jGameObjectModifier.Apply(JGameObjectFetcher.New().Id(item3.Id()).Make(), inputJson);
			}
			inputJson = JGameObjectModifier.New().ReplacePath(tokenToFind, text2).Make()
				.Apply(JGameObjectFetcher.New().Make(), inputJson);
		}
		return jGameObjectFetcher.Delete(inputJson);
	}

	private static string FindMatchingIdleProjectID(JContainer inputJson, JGameObjectFetcher projectFetcher, Dictionary<string, string> factionToIdleProject)
	{
		JObject jObject = JComponentFetcher.New().Type(affiliationType).NestedIn(projectFetcher)
			.Make()
			.Fetch(inputJson)
			.OfType<JObject>()
			.FirstOrDefault();
		JObject jobject = jObject["faction"] as JObject;
		return factionToIdleProject[jobject.Id() ?? jobject.Ref()];
	}

	private static JContainer RemoveJTransform(JContainer inputJson, JGameObjectFetcher goFetcher)
	{
		JComponentFetcher jComponentFetcher = JComponentFetcher.New().Type("UnityEngine.Transform, UnityEngine").NestedIn(goFetcher)
			.Make();
		inputJson = jComponentFetcher.Delete(inputJson);
		return inputJson;
	}

	private static JContainer AddJTransform(JContainer inputJson, JGameObjectFetcher goFetcher, JObject transform)
	{
		return JGameObjectModifier.New().InjectRawComponent(transform).Make()
			.Apply(goFetcher, inputJson);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 21;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 22).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 23).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}
}
