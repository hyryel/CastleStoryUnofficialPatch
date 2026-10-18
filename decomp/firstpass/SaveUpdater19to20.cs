using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.IO.Serialization.Json;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SaveUpdater19to20 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater19to20).FullName);

	public SaveUpdater19to20(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		if (!saveFile.IsNewGame())
		{
			fileUpdater.AddStep(UpdateFactions);
			fileUpdater.AddStep(UpdateBricktron);
			fileUpdater.AddStep(UpdateSv_Param);
			fileUpdater.AddStep(InjectIdleGroups);
			fileUpdater.AddStep(SetMaxpopulation);
			fileUpdater.AddStep(SetHomeCrystal);
			fileUpdater.AddStep(RemoveLuaStandalone);
			fileUpdater.AddStep(DeleteCorrupted);
			fileUpdater.AddStep(CommitProjects);
			fileUpdater.AddStep(UpdateSpawnerCrystal);
		}
		fileUpdater.AddStep(SetMarkersAsPlaced);
		fileUpdater.AddStep(AssemblyUpgrade);
		fileUpdater.AddStep(RemoveCrystalFireflies);
		fileUpdater.AddStep(RemoveTorch);
		fileUpdater.AddStep(ChangeGameClockTimestampFieldName);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
		fileUpdater2.AddStep(ConvertToSandbox);
	}

	private JContainer CommitProjects(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().Path("/Jeu/Groupes").Make();
		JObject componentContainer = JObject.Parse("\r\n        {\r\n            \"$type\": \"Brix.Game.AI.ProjectState, Assembly-CSharp-firstpass\",\r\n            \"updateId\": 20,\r\n            \"flags\": 59\r\n        }");
		foreach (JObject item in jGameObjectFetcher.Fetch(inputJson))
		{
			JGameObjectFetcher fetcher = JGameObjectFetcher.New().FieldEqualsTo("$id", item["$id"]).Make();
			inputJson = JGameObjectModifier.New().InjectRawComponent(componentContainer).Make()
				.Apply(fetcher, inputJson);
		}
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.Components.Blueprint, Assembly-CSharp-firstpass").Make();
		inputJson = JComponentModifier.New().ChangeFieldValue("commited", fieldValue: true).Make()
			.Apply(compFetcher, inputJson);
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

	private JContainer RemoveLuaStandalone(JContainer inputJson, IdGenerator idGen)
	{
		inputJson = JGameObjectFetcher.New().Path("/Jeu/LuaScripts").Make()
			.Delete(inputJson);
		JObject content = JObject.Parse("    \r\n                {\r\n                    \"$id\": \"go:LuaScripts\",\r\n                  \"$type\": \"UnityEngine.GameObject, UnityEngine\",\r\n                  \"$path\": \"/Jeu\",\r\n                  \"FactorylessGameObject\": true,\r\n                  \"DoNotCreateOnDeserialized\": true,\r\n                  \"name\": \"LuaScripts\",\r\n                  \"components\": [\r\n                            {\r\n                            \"$type\": \"Brix.Lua.LuaRegistry, Assembly-CSharp-firstpass\",\r\n                            \"_saveData\": {\r\n                                \"$type\": \"System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Object, mscorlib]], mscorlib]], mscorlib\",\r\n                                \"sandbox\": {\r\n                                    \"$type\": \"System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Object, mscorlib]], mscorlib\",\r\n                                    \"doneInit\": true,\r\n                                }\r\n                          }\r\n                        }\r\n                    ]\r\n                    }");
		inputJson.Add(content);
		return inputJson;
	}

	private JContainer RemoveTorch(JContainer inputJson, IdGenerator idGen)
	{
		inputJson = JGameObjectFetcher.New().AssetKey("Blueprints.Torch").Make()
			.Delete(inputJson);
		inputJson = JGameObjectFetcher.New().AssetKey("Blueprints.Lantern").Make()
			.Delete(inputJson);
		inputJson = JGameObjectFetcher.New().AssetKey("ObjetsDynamiques.Lanterne").Make()
			.Delete(inputJson);
		inputJson = JGameObjectFetcher.New().AssetKey("ObjetsDynamiques.Torche").Make()
			.Delete(inputJson);
		return inputJson;
	}

	private JContainer SetHomeCrystal(JContainer inputJson, IdGenerator idGen)
	{
		JObject componentContainer = JObject.Parse("{\r\n                      \"$type\": \"Brix.Game.Components.FireflyNest, Assembly-CSharp-firstpass\",\r\n                      \"isHome\": true\r\n                      }");
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().AssetKey("ObjetsDynamiques.Crystal").Make();
		foreach (JObject item in jGameObjectFetcher.Fetch(inputJson))
		{
			JGameObjectFetcher fetcher = JGameObjectFetcher.New().FieldEqualsTo("$id", item["$id"]).Make();
			inputJson = JGameObjectModifier.New().InjectRawComponent(componentContainer).Make()
				.Apply(fetcher, inputJson);
		}
		return inputJson;
	}

	private JContainer SetMaxpopulation(JContainer inputJson, IdGenerator idGen)
	{
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.WorkForceProvider, Assembly-CSharp-firstpass").Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().RemoveField("_maxPopulation").Make();
		return jComponentModifier.Apply(compFetcher, inputJson);
	}

	private JContainer RemoveCrystalFireflies(JContainer inputJson, IdGenerator idGgen)
	{
		inputJson = JGameObjectFetcher.New().AssetKey("Environment.CrystalFirefly").Make()
			.Delete(inputJson);
		return JGameObjectFetcher.New().AssetKey("Environment.Firefly").Make()
			.Delete(inputJson);
	}

	private JContainer InjectIdleGroups(JContainer inputjson, IdGenerator idGen)
	{
		string[] array = new string[6] { "Blue", "Red", "Green", "Orange", "Neutral", "Corruptrons" };
		for (int i = 0; i < 6; i++)
		{
			JGameObjectFetcher filter = JGameObjectFetcher.New().AssetKey("ObjetsDynamiques.Crystal").Make();
			JComponentFetcher jComponentFetcher = JComponentFetcher.New().Type("Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass").NestedIn(filter)
				.Make();
			Vector3 vector = Vector3.zero;
			foreach (JContainer item in jComponentFetcher.Fetch(inputjson))
			{
				if ((((JObject)item["faction"]).Id() ?? ((JObject)item["faction"]).Ref()) == "go:" + array[i])
				{
					string id = item.Parent.Parent.Parent.Id() ?? item.Parent.Parent.Parent.Ref();
					JGameObjectFetcher filter2 = JGameObjectFetcher.New().Id(id).Make();
					vector = JComponentFetcher.New().Type("UnityEngine.Transform, UnityEngine").NestedIn(filter2)
						.Make()
						.Fetch(inputjson)
						.FirstOrDefault()
						.ToObject<Vector3>();
				}
			}
			string text = string.Empty;
			if (vector.magnitude != 0f)
			{
				text = "\r\n              {\r\n                \"$type\": \"Brix.Legacy.Beacon, Assembly-CSharp-firstpass\",\r\n                \"VolumeComponent\": null,\r\n                \"radius\": 0.0,\r\n                \"position\": {\r\n                  \"$type\": \"Brix.Engine.XYZ, Brix.Core\",\r\n                  \"x\": " + vector.x + ",\r\n                  \"y\": " + vector.y + ",\r\n                  \"z\": " + vector.z + "\r\n                }\r\n              ";
			}
			JObject content = JObject.Parse("\r\n            {\r\n                \"$id\": \"go:Idle" + array[i] + "\",\r\n                \"$type\": \"UnityEngine.GameObject, UnityEngine\",\r\n                \"$assetKey\": \"Groupes.Idle\",\r\n                \"$path\": \"/Jeu/Groupes\",\r\n                \"name\": \"Idle (" + i + ")\",\r\n                \"components\": \r\n                    [ {\r\n                      \"$type\": \"Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass\",\r\n                      \"faction\": {\r\n                        \"$component\": \"Brix.Game.Faction, Assembly-CSharp-firstpass\",\r\n                        \"$ref\": \"go:" + array[i] + "\"\r\n                      }, \r\n                     " + text + "\r\n                    },]\r\n            }");
			inputjson.Add(content);
		}
		return inputjson;
	}

	private JContainer UpdateFactions(JContainer inputJson, IdGenerator idGen)
	{
		inputJson = JObjectFetcher.New().FieldEqualsTo("$id", "go:Spectator").Make()
			.Delete(inputJson);
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Id("go:Corrupted").Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().GOName("Corruptrons").Id("go:Corruptrons")
			.Make();
		return jGameObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer UpdateSpawnerCrystal(JContainer inputJson, IdGenerator idGen)
	{
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("SpawnerCrystal, Assembly-CSharp-firstpass").Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().RemoveField("firefliesSpawned").RemoveField("_magie")
			.RemoveField("unitBasePrice")
			.RemoveField("unitPriceMultiplier")
			.RemoveField("unitPossibleToSpawn")
			.Make();
		return jComponentModifier.Apply(compFetcher, inputJson);
	}

	private JContainer UpdateBricktron(JContainer inputJson, IdGenerator idGen)
	{
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("CharacterState, Assembly-CSharp-firstpass").Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().ChangeFieldValue("thoughtId", 5).ChangeFieldValue("updateId", 2)
			.ChangeFieldValue("autonomyID", 40)
			.RemoveField("holdingFire")
			.RemoveField("defendingPosition")
			.Make();
		return jComponentModifier.Apply(compFetcher, inputJson);
	}

	private JContainer UpdateSv_Param(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().HasField("_saveParams").Make();
		JObject jObjectToInject = JObject.Parse("\r\n            {\r\n                \"$type\": \"System.Collections.Generic.Dictionary`2[[System.Object, mscorlib],[System.Object, mscorlib]], mscorlib\",\r\n                \"gameTimer\": {\r\n                  \"$type\": \"Brix.Lua.LuaTimer, Assembly-CSharp-firstpass\",\r\n                  \"_elapsedTime\": 0,\r\n                  \"_running\": true,\r\n                  \"_initialized\": true,\r\n                  \"_delay\": 0.0,\r\n                  \"_useUnscaledTime\": false\r\n                },\r\n                \"gameTimerOffset\": 0\r\n            }\r\n        ");
		JObjectModifier jObjectModifier = JObjectModifier.New().InjectJObject("_saveParams", jObjectToInject).Make();
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer ChangeGameClockTimestampFieldName(JContainer inputJson, IdGenerator idGen)
	{
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.GameClock, Assembly-CSharp-firstpass").HasField("timestamp")
			.Make();
		return JComponentModifier.New().ChangeFieldName("timestamp", "gameModeTimestamp").Make()
			.Apply(compFetcher, inputJson);
	}

	private JContainer SetMarkersAsPlaced(JContainer inputJson, IdGenerator idGen)
	{
		JComponentFetcher compFetcher = JComponentFetcher.New().FieldEqualsTo("$type", "Brix.Lua.LuaMarker, Assembly-CSharp-firstpass").Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().ChangeFieldValue("placed", fieldValue: true).Make();
		return jComponentModifier.Apply(compFetcher, inputJson);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 19;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 19).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 20).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer ConvertToSandbox(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().HasField("__LASTPLAYED_GAMEMODE").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__LASTPLAYED_GAMEMODE", "sandbox").Make();
		inputJson = jObjectModifier.Apply(fetcher, inputJson);
		return inputJson;
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		Action<string> action = delegate(string oldType)
		{
			inputjson = JObjectFetcher.New().Type(oldType).Make()
				.Delete(inputjson);
		};
		action("Brix.Legacy.SpawnerCristal, Assembly-CSharp-firstpass");
		action("IsMagical, Assembly-CSharp-firstpass");
		action("Ame, Assembly-CSharp-firstpass");
		action("Luciole, Assembly-CSharp-firstpass");
		action("PrendreAme, Assembly-CSharp-firstpass");
		action("Spawner, Assembly-CSharp-firstpass");
		return inputjson;
	}
}
