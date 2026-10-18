using System;
using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater11to12 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater11to12).FullName);

	public SaveUpdater11to12(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(RemoveTunnels);
		fileUpdater.AddStep(RemovedCorrupted);
		fileUpdater.AddStep(AssemblyUpgrade);
		fileUpdater.AddStep(FixSkyBox);
		fileUpdater.AddStep(FixFaction);
		fileUpdater.AddStep(ClearModeData);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["version"].Value<int>();
		return num == 11;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", "12").ChangeFieldValue("gamemode", "Sandbox")
			.Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer RemovedCorrupted(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$id", "go:CorruptronSpawnGroup").Make();
		inputJson = jObjectFetcher.Delete(inputJson);
		logger.Info("Deleted Corrupted spawn group");
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.CorruptronSpawnPoint").Make();
		inputJson = jObjectFetcher.Delete(inputJson);
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.Corruptron").Make()
			.Delete(inputJson);
		inputJson = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.Biftron").Make()
			.Delete(inputJson);
		return inputJson;
	}

	private JContainer RemoveTunnels(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Groupes.Tunnel").Make();
		foreach (JObject item in jObjectFetcher.Fetch(inputJson))
		{
			inputJson = JObjectFetcher.New().FieldEqualsTo("$path", "/Jeu/Groupes/" + item["$id"]).Make()
				.Delete(inputJson);
		}
		inputJson = jObjectFetcher.Delete(inputJson);
		return inputJson;
	}

	private JContainer ClearModeData(JContainer inputJson, IdGenerator idGen)
	{
		inputJson = JObjectFetcher.New().FieldEqualsTo("name", "Lua Gamemode Holder").Make()
			.Delete(inputJson);
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Test.RaidProject").Make();
		foreach (JObject item in jObjectFetcher.Fetch(inputJson))
		{
			inputJson = JObjectFetcher.New().FieldEqualsTo("$path", "/Jeu/ObjetsDynamiques/go:SiegeProject/" + item["$id"]).Make()
				.Delete(inputJson);
		}
		inputJson = jObjectFetcher.Delete(inputJson);
		return inputJson;
	}

	private JContainer FixSkyBox(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Environnement").Name("Skybox3d(Clone)")
			.Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().ChangeFieldValue("name", "Skybox3d (0)").Make();
		logger.Info("Updated Skybox id");
		return jGameObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixFaction(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$id", "go:Yellow").Make();
		inputJson = jObjectFetcher.Delete(inputJson);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$id", "go:Purple").Make();
		inputJson = jObjectFetcher.Delete(inputJson);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Core.Faction").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Jeu/go:Factions").ChangeFieldValue("$parent", "go:Factions")
			.Make();
		inputJson = jObjectModifier.Apply(jObjectFetcher, inputJson);
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass").Make();
		inputJson = JComponentModifier.New().ChangeFieldName("_faction", "faction").Make()
			.Apply(compFetcher, inputJson);
		logger.Info("Deleted factions");
		return inputJson;
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		Action<string> action = delegate(string oldType)
		{
			inputjson = JObjectFetcher.New().Type(oldType).Make()
				.Delete(inputjson);
		};
		action("Brix.Game.Bricktron.GlobalCharacterSpawner, Assembly-CSharp-firstpass");
		action("Brix.Game.Bricktron.CharacterSpawnPoint, Assembly-CSharp-firstpass");
		return inputjson;
	}
}
