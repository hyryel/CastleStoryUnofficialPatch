using System;
using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater17to18 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater17to18).FullName);

	public SaveUpdater17to18(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(AssemblyUpgrade);
		fileUpdater.AddStep(RemoveProjects);
		fileUpdater.AddStep(DeleteCorrupted);
		fileUpdater.AddStep(RemoveFireflies);
		fileUpdater.AddStep(MakeCrystalBrew);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
		fileUpdater2.AddStep(ConvertLegacyConquestToSandbox);
	}

	private JContainer MakeCrystalBrew(JContainer inputJson, IdGenerator idGen)
	{
		JObject componentContainer = JObject.Parse("{\r\n      \"$type\": \"Brix.Game.Components.FireflyNest, Assembly-CSharp-firstpass\",\r\n      \"isHome\": true,\r\n      \"doBrew\": true,\r\n      \"brewDuration\": 2.0,\r\n      \"fireflyFilling\": 20,\r\n      \"Operator\": null,\r\n      \"OperationMode\": null\r\n        }");
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().AssetKey("ObjetsDynamiques.Crystal").Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().InjectRawComponent(componentContainer).Make();
		return jGameObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer RemoveProjects(JContainer inputJson, IdGenerator idGen)
	{
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "DynamicObjectBlueprints.MagicMine").Make()
			.Delete(inputJson);
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Balises.SpawnPointBaliseStandalone").Make()
			.Delete(inputJson);
		inputJson = DeparentUnitsFromProject(inputJson, "ObjetsDynamiques.MagicMine", "/Jeu/ObjetsDynamiques/");
		inputJson = DeparentUnitsFromProject(inputJson, "Groupes.Mine", "/Jeu/Groupes/");
		inputJson = DeparentUnitsFromProject(inputJson, "Groupes.Landscape", "/Jeu/Groupes/");
		inputJson = DeparentUnitsFromProject(inputJson, "Groupes.Repair", "/Jeu/Groupes/");
		inputJson = JObjectFetcher.New().FieldEqualsTo("name", "Beacon -Magic Mine").Make()
			.Delete(inputJson);
		return inputJson;
	}

	private JContainer RemoveFireflies(JContainer inputJson, IdGenerator idGen)
	{
		return JObjectFetcher.New().FieldEqualsTo("$assetKey", "Environment.CrystalFirefly").Make()
			.Delete(inputJson);
	}

	private JContainer DeparentUnitsFromProject(JContainer inputJson, string assetKey, string unitsPath)
	{
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", assetKey).Make();
		foreach (JContainer item in jObjectFetcher.Fetch(inputJson))
		{
			JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().FieldEqualsTo("$path", string.Concat(unitsPath, item["$id"], "/Characters")).Make();
			foreach (JContainer item2 in jGameObjectFetcher.Fetch(inputJson))
			{
				JGameObjectFetcher fetcher = JGameObjectFetcher.New().FieldEqualsTo("$path", string.Concat(unitsPath, item["$id"], "/Characters/", item2["$id"])).Make();
				inputJson = JGameObjectModifier.New().ChangeFieldValue("$path", "/Jeu/" + item2["$id"]).Make()
					.Apply(fetcher, inputJson);
				JGameObjectFetcher fetcher2 = JGameObjectFetcher.New().FieldEqualsTo("$path", string.Concat(unitsPath, item["$id"], "/Characters/", item2["$id"], "/Stoque")).Make();
				inputJson = JGameObjectModifier.New().ChangeFieldValue("$path", string.Concat("/Jeu/", item2["$id"], "/Stoque")).Make()
					.Apply(fetcher2, inputJson);
			}
			inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.ProjectData").FieldEqualsTo("$parent", item["$id"].ToString())
				.Make()
				.Delete(inputJson);
			inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Balises.DefaultBalise").FieldEqualsTo("$parent", item["$id"].ToString())
				.Make()
				.Delete(inputJson);
			inputJson = JGameObjectModifier.New().ChangeFieldValue("$parent", "/Jeu/").Make()
				.Apply(jGameObjectFetcher, inputJson);
			inputJson = JGameObjectModifier.New().ChangeFieldValue("$path", "/Jeu/").Make()
				.Apply(jGameObjectFetcher, inputJson);
		}
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", assetKey).Make()
			.Delete(inputJson);
		return inputJson;
	}

	private JContainer ConvertLegacyConquestToSandbox(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().HasField("__LASTPLAYED_GAMEMODE").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__LASTPLAYED_GAMEMODE", "sandbox").Make();
		inputJson = jObjectModifier.Apply(fetcher, inputJson);
		return inputJson;
	}

	private JContainer DeleteCorrupted(JContainer inputJson, IdGenerator idGen)
	{
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.Biftron").Make()
			.Delete(inputJson);
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.BiftronFlailingCollider").Make()
			.Delete(inputJson);
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.Corruptron").Make()
			.Delete(inputJson);
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.CorruptronFlailingCollider").Make()
			.Delete(inputJson);
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.Minitron").Make()
			.Delete(inputJson);
		return inputJson;
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 17;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 17).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 18).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		Action<string> action = delegate(string oldType)
		{
			inputjson = JObjectFetcher.New().Type(oldType).Make()
				.Delete(inputjson);
		};
		action("Brix.Legacy.SpawnerCristal, Assembly-CSharp-firstpass");
		return inputjson;
	}
}
