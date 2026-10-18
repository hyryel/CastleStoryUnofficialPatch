using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater13to14 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater13to14).FullName);

	public SaveUpdater13to14(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(AssemblyUpgrade);
		fileUpdater.AddStep(FixGoalsPathInBuildProject);
		fileUpdater.AddStep(FixAffiliation);
		fileUpdater.AddStep(RemoveGroupesRef);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
		fileUpdater2.AddStep(FixAssetMode);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 13;
	}

	private JContainer FixGoalsPathInBuildProject(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher filter = JGameObjectFetcher.New().AssetKey("Groupes.Construction").Make();
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().NestedIn(filter).Matches(IsABlueprint)
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().Modify(FixGoalsPath).Make();
		logger.Info("Updated Skybox id");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer RemoveGroupesRef(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().HasField("groupe").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().RemoveField("groupe").Make();
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private void FixGoalsPath(JObject jObject, JContainer json)
	{
		jObject["$path"] = jObject["$path"].Value<string>() + "/Goals";
	}

	private JContainer FixAssetMode(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__LASTPLAYED_GAMEMODE", "sandbox").Make();
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixAffiliation(JContainer inputJson, IdGenerator idGen)
	{
		JObject jObjectToInject = JObject.Parse("{\r\n          \"$component\": \"Brix.Game.Faction, Assembly-CSharp-firstpass\",\r\n          \"$id\": \"go:Blue\",\r\n          \"$type\": \"UnityEngine.GameObject, UnityEngine\",\r\n          \"$path\": \"/Jeu/Factions\",\r\n          \"name\": \"Blue\",\r\n          \"components\": [],\r\n          \"$assetKey\": \"Core.Faction\",\r\n          \"baliseMode\": \"2\"\r\n        }");
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass").HasField("faction")
			.FieldIsNull("faction")
			.Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("faction", jObjectToInject).Make();
		return jComponentModifier.Apply(compFetcher, inputJson);
	}

	private bool IsABlueprint(JContainer jobject)
	{
		return jobject["$assetKey"] != null && jobject["$assetKey"].Value<string>().StartsWith("Blueprints");
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 13).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 14).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		inputjson = JObjectFetcher.New().Type("Brix.Legacy.EmetteurDeLucioles, Assembly-CSharp-firstpass").Make()
			.Delete(inputjson);
		inputjson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Environment.CrystalFirefly").Make()
			.Delete(inputjson);
		inputjson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Blocks.Wood_SinglePlank").Make()
			.Delete(inputjson);
		return inputjson;
	}
}
