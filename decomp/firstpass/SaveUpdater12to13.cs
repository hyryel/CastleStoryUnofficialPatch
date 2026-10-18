using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SaveUpdater12to13 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater12to13).FullName);

	public SaveUpdater12to13(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(RemoveLegacyBalises);
		fileUpdater.AddStep(AssemblyUpgrade);
		fileUpdater.AddStep(FixSkyBox);
		fileUpdater.AddStep(FixGarnotteAssetKey);
		fileUpdater.AddStep(FixSiegeProject);
		fileUpdater.AddStep(FixGizmoSurligneur);
		fileUpdater.AddStep(FixIdleGroup);
		fileUpdater.AddStep(FixStructure);
		fileUpdater.AddStep(FixGroupesAffilation);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
		fileUpdater2.AddStep(FixMetaToNewAssetDispensary);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["version"].Value<int>();
		return num == 12;
	}

	private JContainer FixSkyBox(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Environnement").Name("Skybox3d (0)")
			.Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().ChangeFieldValue("name", "Skybox3d").Make();
		logger.Info("Updated Skybox id");
		return jGameObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixSiegeProject(JContainer inputJson, IdGenerator idGen)
	{
		return JObjectFetcher.New().FieldEqualsTo("name", "SiegeProject").Make()
			.Delete(inputJson);
	}

	private JContainer FixStructure(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().AssetKey("Groupes.Structures").Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().ChangeFieldValue("$assetKey", "Groupes.Construction").Make();
		return jGameObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixGroupesAffilation(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().Path("/Jeu/UnclassedObjects").Make();
		JObject jObjectToInject = JObject.Parse("{\r\n          \"$component\": \"Brix.Game.Faction, Assembly-CSharp-firstpass\",\r\n          \"$id\": \"go:Blue\",\r\n          \"$type\": \"UnityEngine.GameObject, UnityEngine\",\r\n          \"$path\": \"/Jeu/Factions\",\r\n          \"name\": \"Blue\",\r\n          \"components\": [],\r\n          \"$assetKey\": \"Core.Faction\",\r\n          \"baliseMode\": \"2\"\r\n        }");
		JObject componentContainer = JObject.Parse("{\"$type\": \"Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass\"}");
		foreach (JObject item in jGameObjectFetcher.Fetch(inputJson.DeepClone() as JContainer))
		{
			logger.Info("id: " + item["$id"]);
			JGameObjectFetcher fetcher = JGameObjectFetcher.New().FieldEqualsTo("$id", item["$id"]).Make();
			JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().InjectRawComponent(componentContainer).Make();
			inputJson = jGameObjectModifier.Apply(fetcher, inputJson);
		}
		JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("faction", jObjectToInject).Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass").NestedIn(jGameObjectFetcher)
			.Make();
		inputJson = jComponentModifier.Apply(compFetcher, inputJson);
		return inputJson;
	}

	private JContainer FixIdleGroup(JContainer inputJson, IdGenerator idGen)
	{
		return JObjectFetcher.New().FieldEqualsTo("$assetKey", "Groupes.Idle").Make()
			.Delete(inputJson);
	}

	private JContainer FixGizmoSurligneur(JContainer inputJson, IdGenerator idGen)
	{
		return JObjectFetcher.New().FieldEqualsTo("$assetKey", "Gizmos.Surligneur").Make()
			.Delete(inputJson);
	}

	private JContainer RemoveLegacyBalises(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Balises.DefaultBalise").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("activeSelf", fieldValue: false).Make();
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", 13).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixMetaToNewAssetDispensary(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(saveFile.path + "meta.json"));
		inputJson = JObjectModifier.New().ChangeFieldName("type", "__MAPTYPE").Make()
			.Apply(fetcher, inputJson);
		if (File.Exists(saveFile.path + "/meta/info.txt"))
		{
			Hashtable hashtable2 = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(saveFile.path + "/meta/info.txt"));
			string[] fieldValue = (from mode in ((JArray)hashtable2["GameModes"]).ToObject<string[]>()
				select mode.ToLower()).ToArray();
			inputJson = JObjectModifier.New().ChangeFieldValue("__SUPPORTED_GAMEMODES", fieldValue).Make()
				.Apply(fetcher, inputJson);
			inputJson = JObjectModifier.New().ChangeFieldValue("__SAVETYPE", "Map").Make()
				.Apply(fetcher, inputJson);
			inputJson = JObjectModifier.New().ChangeFieldValue("__MAPTYPE", "Map").Make()
				.Apply(fetcher, inputJson);
		}
		else
		{
			inputJson = JObjectModifier.New().ChangeFieldValue("__MAPTYPE", "Save").Make()
				.Apply(fetcher, inputJson);
			inputJson = JObjectModifier.New().ChangeFieldValue("__SAVETYPE", "Manualsave").Make()
				.Apply(fetcher, inputJson);
		}
		inputJson = JObjectModifier.New().ChangeFieldValue("version", 13).Make()
			.Apply(fetcher, inputJson);
		inputJson = JObjectModifier.New().ChangeFieldName("version", "__ASSETVERSION").Make()
			.Apply(fetcher, inputJson);
		inputJson = JObjectModifier.New().ChangeFieldName("gamemode", "__LASTPLAYED_GAMEMODE").Make()
			.Apply(fetcher, inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("__CODENAME", saveFile.codename).Make()
			.Apply(fetcher, inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("__CUSTOMNAME", saveFile.path.Split('/').ElementAt(saveFile.path.Split('/').Length - 2)).Make()
			.Apply(fetcher, inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("__DISPLAYNAME", saveFile.path.Split('/').ElementAt(saveFile.path.Split('/').Length - 2)).Make()
			.Apply(fetcher, inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("__PLAYABLECONTEXT", 7).Make()
			.Apply(fetcher, inputJson);
		if (hashtable.ContainsKey("gamemode"))
		{
			inputJson = JObjectModifier.New().ChangeFieldValue("__LASTPLAYED_GAMEMODE", hashtable["gamemode"].ToString().ToLower()).Make()
				.Apply(fetcher, inputJson);
		}
		if (File.Exists(saveFile.path + "/thumb.png"))
		{
			File.Move(saveFile.path + "/thumb.png", saveFile.path + "/thumbnail.png");
		}
		if (File.Exists(saveFile.path + "meta/thumbnail.png"))
		{
			File.Move(saveFile.path + "meta/thumbnail.png", saveFile.path + "/thumbnail.png");
		}
		logger.Info("Updated version file sucessfully");
		return inputJson;
	}

	private JContainer FixGarnotteAssetKey(JContainer inputJson, IdGenerator idGen)
	{
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawStone").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.Stone1").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawStone").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.Stone2").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawStone").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.Stone3").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawStone").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.Stone4").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawIron").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.Iron1").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawIron").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.Iron2").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawIron").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.Iron3").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawIron").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.Iron4").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawOrangeCrystal").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.OrangeCrystal1").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawOrangeCrystal").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.OrangeCrystal2").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawOrangeCrystal").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.OrangeCrystal3").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawOrangeCrystal").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.OrangeCrystal4").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawBlueCrystal").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.BlueCrystal1").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawBlueCrystal").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.BlueCrystal2").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawBlueCrystal").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.BlueCrystal3").Make(), inputJson);
		inputJson = JObjectModifier.New().ChangeFieldValue("$assetKey", "RawResources.RawBlueCrystal").Make()
			.Apply(JObjectFetcher.New().FieldEqualsTo("$assetKey", "Garnottes.BlueCrystal4").Make(), inputJson);
		return inputJson;
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		Action<string> action = delegate(string oldType)
		{
			inputjson = JObjectFetcher.New().Type(oldType).Make()
				.Delete(inputjson);
		};
		Action<string, string> action2 = delegate(string oldType, string newType)
		{
			JObjectFetcher fetcher = JObjectFetcher.New().Type(oldType).Make();
			JObjectModifier jObjectModifier = JObjectModifier.New().Type(newType).Make();
			inputjson = jObjectModifier.Apply(fetcher, inputjson);
		};
		action2("Brix.Legacy.BaliseAI, Assembly-CSharp-firstpass", "Brix.Legacy.Beacon, Assembly-CSharp-firstpass");
		action2("Brix.Legacy.Blueprint, Assembly-CSharp-firstpass", "Brix.Game.Components.Blueprint, Assembly-CSharp-firstpass");
		action("Brix.Game.AI.Pathfinding.PathMarker, Assembly-CSharp-firstpass");
		action("Brix.Legacy.Groupe, Assembly-CSharp-firstpass");
		action("Brix.Legacy.SiegeProject, Assembly-CSharp-firstpass");
		action("Brix.Legacy.PaletteScript, Assembly-CSharp-firstpass");
		action("Brix.Legacy.TunnelSelection, Assembly-CSharp-firstpass");
		return inputjson;
	}
}
