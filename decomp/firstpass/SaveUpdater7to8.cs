using System;
using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater7to8 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater7to8).FullName);

	public SaveUpdater7to8(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(AssemblyUpgrade);
		if (File.Exists(saveFile.path + "/survival.gamemode.json"))
		{
			fileUpdater.AddStep(InsertBlueFaction);
		}
		fileUpdater.AddStep(FixCamera);
		fileUpdater.AddStep(FixKnowledgeSystem);
		fileUpdater.AddStep(FixGameClock);
		fileUpdater.AddStep(FixCharacters);
		fileUpdater.AddStep(RemoveDefaultAlly);
		fileUpdater.AddStep(FixSkyBox);
		fileUpdater.AddStep(FixPlayers);
		fileUpdater.AddStep(FixBrickTronDamageReceiver);
		fileUpdater.AddStep(FixObjectsDamageReceiver);
		fileUpdater.AddStep(FixCatapults);
		fileUpdater.AddStep(FixGroupesConstruction);
		fileUpdater.AddStep(FixGroupeStructure);
		fileUpdater.AddStep(FixGroupeLumber);
		fileUpdater.AddStep(FixGroupeCleanup);
		fileUpdater.AddStep(FixGroupeMine);
		fileUpdater.AddStep(FixGroupeTunnel);
		fileUpdater.AddStep(FixCorruptronSpawnGroup);
		fileUpdater.AddStep(FixCorruptronSpawnPoint);
		fileUpdater.AddStep(FixAffiliation);
		fileUpdater.AddStep(FixSiegeProject);
		fileUpdater.AddStep(FixBarils);
		fileUpdater.AddStep(FixBaliseMode);
		fileUpdater.AddStep(RemoveWoodBluePrints);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
		logger.Info("Save updater 7to8 succesfully loaded for: " + saveFile.path);
	}

	private JContainer RemoveWoodBluePrints(JContainer inputJson, IdGenerator idGen)
	{
		string[] array = new string[4] { "Plank", "Wood_DoubleLog_VAA", "Wood_DoubleLog_AA", "Wood_DoublePlank" };
		string[] array2 = array;
		foreach (string text in array2)
		{
			JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Blueprints." + text).Make();
			inputJson = jObjectFetcher.Delete(inputJson);
		}
		logger.Info("Removed Wood blueprints");
		return inputJson;
	}

	private JContainer InsertBlueFaction(JContainer inputJson, IdGenerator idGen)
	{
		JObject content = JObject.Parse("{\t\t\r\n        \"$id\": \"go:Blue\",\t\t\r\n        \"$type\": \"UnityEngine.GameObject, UnityEngine\",\t\t\r\n        \"$assetKey\": \"Core.Faction\",\t\t\r\n        \"$path\": \"/Jeu/Factions\",\t\t\r\n         \"name\": \"Blue\",\t\t\r\n        }");
		inputJson.AddFirst(content);
		logger.Info("Inserted faction");
		return inputJson;
	}

	private JContainer FixBaliseMode(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Balises.Balise").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$assetKey", "Balises.Balise2").Make();
		logger.Info("Updated Balise Mode");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixAffiliation(JContainer inputJson, IdGenerator idGen)
	{
		JObject componentContainer = JObject.Parse("{\"$type\": \"Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass\"}");
		JObject jObjectToInject = JObject.Parse("{\r\n          \"$component\": \"Brix.Game.Faction, Assembly-CSharp-firstpass\",\r\n          \"$id\": \"go:Blue\",\r\n          \"$type\": \"UnityEngine.GameObject, UnityEngine\",\r\n          \"$path\": \"/Jeu/Factions\",\r\n          \"name\": \"Blue\",\r\n          \"components\": [],\r\n          \"$assetKey\": \"Core.Faction\",\r\n          \"baliseMode\": \"2\"\r\n        }");
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/Personnages/Bricktrons").AssetKey("Test.Bricktron")
			.Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass").NestedIn(filter)
			.Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("_faction", jObjectToInject).Make();
		inputJson = jComponentModifier.Apply(compFetcher, inputJson);
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().Path("/Jeu/ObjetsDynamiques").Name("Crystal(Clone)")
			.Make();
		foreach (JObject item in jGameObjectFetcher.Fetch(inputJson))
		{
			JGameObjectFetcher fetcher = JGameObjectFetcher.New().FieldEqualsTo("$id", item["$id"]).Make();
			JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().InjectRawComponent(componentContainer).Make();
			inputJson = jGameObjectModifier.Apply(fetcher, inputJson);
		}
		JComponentModifier jComponentModifier2 = JComponentModifier.New().InjectJObject("_faction", jObjectToInject).Make();
		JComponentFetcher compFetcher2 = JComponentFetcher.New().Type("Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass").NestedIn(jGameObjectFetcher)
			.Make();
		inputJson = jComponentModifier2.Apply(compFetcher2, inputJson);
		JObject jObjectToInject2 = JObject.Parse("{\r\n          \"$component\": \"Brix.Game.Faction, Assembly-CSharp-firstpass\",\r\n          \"$id\": \"go:Corrupted\",\r\n          \"$type\": \"UnityEngine.GameObject, UnityEngine\",\r\n          \"$path\": \"/Jeu/Factions\",\r\n          \"name\": \"Corrupted\",\r\n          \"components\": [],\r\n          \"$assetKey\": \"Core.Faction\",\r\n          \"baliseMode\": \"2\"\r\n        }");
		JGameObjectFetcher jGameObjectFetcher2 = JGameObjectFetcher.New().Path("/Jeu/ObjetsDynamiques").Name("SiegeProject")
			.Make();
		JGameObjectModifier jGameObjectModifier2 = JGameObjectModifier.New().InjectRawComponent(componentContainer).Make();
		inputJson = jGameObjectModifier2.Apply(jGameObjectFetcher2, inputJson);
		JComponentModifier jComponentModifier3 = JComponentModifier.New().InjectJObject("_faction", jObjectToInject2).Make();
		JComponentFetcher compFetcher3 = JComponentFetcher.New().Type("Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass").NestedIn(jGameObjectFetcher2)
			.Make();
		inputJson = jComponentModifier3.Apply(compFetcher3, inputJson);
		JGameObjectFetcher filter2 = JGameObjectFetcher.New().Path("/Jeu/Metadata").Name("CorruptronSpawnGroup")
			.Make();
		JComponentModifier jComponentModifier4 = JComponentModifier.New().InjectJObject("_faction", jObjectToInject2).Make();
		JComponentFetcher compFetcher4 = JComponentFetcher.New().Type("Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass").NestedIn(filter2)
			.Make();
		inputJson = jComponentModifier4.Apply(compFetcher4, inputJson);
		logger.Info("Updated Affiliations");
		return inputJson;
	}

	private JContainer FixSiegeProject(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("$path", "/Jeu/Factions").FieldEqualsTo("name", "DefaultEnemy")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().Id("go:Corrupted").Make();
		JContainer jContainer = jObjectModifier.Apply(fetcher, inputJson);
		fetcher = JObjectFetcher.New().FieldEqualsTo("name", "DefaultEnemy").Make();
		jObjectModifier = JObjectModifier.New().ChangeFieldValue("name", "Corrupted").Make();
		jContainer = jObjectModifier.Apply(fetcher, jContainer);
		logger.Info("Removed Default Enemy");
		return jContainer;
	}

	private JContainer FixGroupeLumber(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Groupes").Name("Lumber(Clone)")
			.Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().ChangeFieldValue("$path", "/Jeu/UnclassedObjects").Make();
		JContainer jContainer = jGameObjectModifier.Apply(fetcher, inputJson);
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/UnclassedObjects").Name("Lumber(Clone)")
			.Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.LumberProject, Assembly-CSharp-firstpass").NestedIn(filter)
			.Make();
		JObject jObjectToInject = JObject.Parse("{\"$type\": \"WorkForce, Assembly-CSharp-firstpass\"}");
		JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("workForce", jObjectToInject).Make();
		jContainer = jComponentModifier.Apply(compFetcher, jContainer);
		logger.Info("Updated Lumber Path");
		return jContainer;
	}

	private JContainer FixGroupeStructure(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Groupes").Name("Structures(Clone)")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Jeu/UnclassedObjects").Make();
		JContainer jContainer = jObjectModifier.Apply(fetcher, inputJson);
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/UnclassedObjects").Name("Structures(Clone)")
			.Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.LumberProject, Assembly-CSharp-firstpass").NestedIn(filter)
			.Make();
		JObject jObjectToInject = JObject.Parse("{\"$type\": \"WorkForce, Assembly-CSharp-firstpass\"}");
		JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("workForce", jObjectToInject).Make();
		jContainer = jComponentModifier.Apply(compFetcher, jContainer);
		logger.Info("Updated Structure Path");
		return jContainer;
	}

	private JContainer FixGroupeMine(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Groupes").Name("Mine(Clone)")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Jeu/UnclassedObjects").Make();
		JContainer jContainer = jObjectModifier.Apply(fetcher, inputJson);
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/UnclassedObjects").Name("Mine(Clone)")
			.Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.LumberProject, Assembly-CSharp-firstpass").NestedIn(filter)
			.Make();
		JObject jObjectToInject = JObject.Parse("{\"$type\": \"WorkForce, Assembly-CSharp-firstpass\"}");
		JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("workForce", jObjectToInject).Make();
		jContainer = jComponentModifier.Apply(compFetcher, jContainer);
		logger.Info("Updated mine Path");
		return jContainer;
	}

	private JContainer FixGroupeTunnel(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Groupes").Name("Tunnel(Clone)")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Jeu/UnclassedObjects").Make();
		JContainer jContainer = jObjectModifier.Apply(fetcher, inputJson);
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/UnclassedObjects").Name("Tunnel(Clone)")
			.Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.LumberProject, Assembly-CSharp-firstpass").NestedIn(filter)
			.Make();
		JObject jObjectToInject = JObject.Parse("{\"$type\": \"WorkForce, Assembly-CSharp-firstpass\"}");
		JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("workForce", jObjectToInject).Make();
		jContainer = jComponentModifier.Apply(compFetcher, jContainer);
		logger.Info("Updated tunnel Path");
		return jContainer;
	}

	private JContainer FixGroupeCleanup(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Groupes").Name("Cleanup(Clone)")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Jeu/UnclassedObjects").Make();
		JContainer jContainer = jObjectModifier.Apply(fetcher, inputJson);
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/UnclassedObjects").Name("Cleanup(Clone)")
			.Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.LumberProject, Assembly-CSharp-firstpass").NestedIn(filter)
			.Make();
		JObject jObjectToInject = JObject.Parse("{\"$type\": \"WorkForce, Assembly-CSharp-firstpass\"}");
		JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("workForce", jObjectToInject).Make();
		jContainer = jComponentModifier.Apply(compFetcher, jContainer);
		logger.Info("Updated cleanup Path");
		return jContainer;
	}

	private JContainer FixGroupesConstruction(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Groupes").Name("Blocks(Clone)")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Jeu/UnclassedObjects").Make();
		JContainer jContainer = jObjectModifier.Apply(fetcher, inputJson);
		fetcher = JGameObjectFetcher.New().Path("/Jeu/UnclassedObjects").Name("Blocks(Clone)")
			.Make();
		jObjectModifier = JObjectModifier.New().ChangeFieldValue("$assetKey", "Groupes.Construction").Make();
		jContainer = jObjectModifier.Apply(fetcher, jContainer);
		fetcher = JGameObjectFetcher.New().Path("/Jeu/UnclassedObjects").Name("Blocks(Clone)")
			.Make();
		jObjectModifier = JObjectModifier.New().ChangeFieldValue("name", "Construction(Clone)").Make();
		jContainer = jObjectModifier.Apply(fetcher, jContainer);
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/UnclassedObjects").Name("Construction(Clone)")
			.Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.LumberProject, Assembly-CSharp-firstpass").NestedIn(filter)
			.Make();
		JObject jObjectToInject = JObject.Parse("{\"$type\": \"WorkForce, Assembly-CSharp-firstpass\"}");
		JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("workForce", jObjectToInject).Make();
		jContainer = jComponentModifier.Apply(compFetcher, jContainer);
		logger.Info("Updated Construction Path");
		return jContainer;
	}

	private JContainer FixCorruptronSpawnGroup(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/ObjetsDynamiques").Name("CorruptronSpawnGroup")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Jeu/Metadata").Make();
		inputJson = jObjectModifier.Apply(fetcher, inputJson);
		fetcher = JGameObjectFetcher.New().Path("/Jeu/Metadata").Name("CorruptronSpawnGroup")
			.Make();
		JComponentFetcher jComponentFetcher = JComponentFetcher.New().Type("Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass").NestedIn(fetcher)
			.Make();
		inputJson = jComponentFetcher.Delete(inputJson);
		logger.Info("Updated Corruptron SpawnGroup");
		return inputJson;
	}

	private JContainer FixCorruptronSpawnPoint(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().AssetKey("Test.CorruptronSpawnPoint").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Jeu/UnclassedObjects").RemoveField("$parent")
			.Make();
		inputJson = jObjectModifier.Apply(fetcher, inputJson);
		logger.Info("Updated Corruptron SpawnGroup");
		return inputJson;
	}

	private JContainer FixCamera(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Cameras").Name("Camera")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Cameras").Make();
		logger.Info("Updated Camera Path");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixObjectsDamageReceiver(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/ObjetsDynamiques").AssetKey("ObjetsDynamiques.Catapulte")
			.Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.Damage.ObjectDamageReceiver, Assembly-CSharp-firstpass").NestedIn(filter)
			.Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().ChangeFieldName("HP", "_hp").Make();
		logger.Info("Updated Catapult Damage receiver");
		return jComponentModifier.Apply(compFetcher, inputJson);
	}

	private JContainer FixBrickTronDamageReceiver(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/Personnages/Bricktrons").AssetKey("Test.Bricktron")
			.Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.Damage.BricktronsDamageReceiver, Assembly-CSharp-firstpass").NestedIn(filter)
			.Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().ChangeFieldName("HP", "_hp").Make();
		logger.Info("Updated Damage receiver");
		return jComponentModifier.Apply(compFetcher, inputJson);
	}

	private JContainer FixPlayers(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("name", "Player 0").Make();
		JContainer jContainer = jObjectFetcher.Delete(inputJson);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("name", "enemy").Make();
		jContainer = jObjectFetcher.Delete(jContainer);
		logger.Info("Deleted players objects");
		return jContainer;
	}

	private JContainer FixCatapults(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/ObjetsDynamiques").AssetKey("ObjetsDynamiques.Catapulte")
			.Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().ChangeFieldValue("$component", "Brix.Legacy.Catapult.Catapult, Assembly-CSharp-firstpass").Make();
		JContainer result = jGameObjectModifier.Apply(fetcher, inputJson);
		logger.Info("Updated Catapults");
		return result;
	}

	private JContainer FixBarils(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("$component", "Catapulte, Assembly-UnityScript").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$component", "Brix.Legacy.Catapult.Catapult, Assembly-CSharp-firstpass").Make();
		JContainer result = jObjectModifier.Apply(fetcher, inputJson);
		logger.Info("Updated Barils");
		return result;
	}

	private JContainer FixKnowledgeSystem(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Config").Name("KnowledgeSystem")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Systems").Make();
		logger.Info("Updated KnowledgeSystem Path");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixGameClock(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Config").Name("GameClock")
			.Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$path", "/Systems").Make();
		logger.Info("Updated GameClock Path");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixSkyBox(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().Path("/Jeu/Environnement").Name("Skybox3d(Clone)")
			.Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().RemoveField("$id").Make();
		logger.Info("Updated Skybox id");
		return jGameObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixCharacters(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/Personnages/Bricktrons").AssetKey("Test.Bricktron")
			.Make();
		JComponentFetcher jComponentFetcher = JComponentFetcher.New().Type("UnityEngine.Rigidbody, UnityEngine").NestedIn(filter)
			.Make();
		logger.Info("Deleted ridigbody on characters");
		return jComponentFetcher.Delete(inputJson);
	}

	private JContainer RemoveDefaultAlly(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher filter = JGameObjectFetcher.New().Path("/Jeu/Factions").Name("DefaultAllied")
			.Make();
		JComponentFetcher jComponentFetcher = JComponentFetcher.New().FieldEqualsTo("name", "DefaultAllied").NestedIn(filter)
			.Make();
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("name", "DefaultAllied").Make();
		JContainer jContainer = jComponentFetcher.Delete(inputJson);
		jContainer = jObjectFetcher.Delete(jContainer);
		logger.Info("Deleted DefaultAllied");
		return jContainer;
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		Action<string, string> action = delegate(string oldType, string newType)
		{
			JObjectFetcher fetcher = JObjectFetcher.New().Type(oldType).Make();
			JObjectModifier jObjectModifier = JObjectModifier.New().Type(newType).Make();
			inputjson = jObjectModifier.Apply(fetcher, inputjson);
		};
		Action<string> action2 = delegate(string oldType)
		{
			inputjson = JObjectFetcher.New().Type(oldType).Make()
				.Delete(inputjson);
		};
		action("Brix.IO.Serialization.Json.Converters.Vector3DTO, Brix.Core", "UnityEngine.Vector3, UnityEngine");
		action("Brix.IO.Serialization.Json.Converters.QuaternionDTO, Brix.Core", "UnityEngine.Quaternion, UnityEngine");
		action("EmetteurDeLucioles, Assembly-UnityScript", "Brix.Legacy.EmetteurDeLucioles, Assembly-CSharp-firstpass");
		action("Brix.Game.AI.ObjectDamageReceiver, Assembly-CSharp-firstpass", "Brix.Game.AI.Damage.ObjectDamageReceiver, Assembly-CSharp-firstpass");
		action("Brix.Game.AI.BricktronDamageReceiver, Assembly-CSharp-firstpass", "Brix.Game.AI.Damage.BricktronDamageReceiver, Assembly-CSharp-firstpass");
		action("Occupation, Assembly-CSharp-firstpass", "Brix.Game.AI.Occupation, Assembly-CSharp-firstpass");
		action("Catapulte, Assembly-UnityScript", "Brix.Legacy.Catapult.Catapult, Assembly-CSharp-firstpass");
		action("Mine, Assembly-UnityScript", "Brix.Legacy.Mine, Assembly-CSharp-firstpass");
		action("TunnelSelection, Assembly-UnityScript", "Brix.Legacy.TunnelSelection, Assembly-CSharp-firstpass");
		action("Brix.Game.Components.RigidbodyExtended, Assembly-CSharp-firstpass", "Brix.Legacy.TunnelSelection, Assembly-CSharp-firstpass");
		action("PaletteScript, Assembly-UnityScript", "Brix.Legacy.PaletteScript, Assembly-CSharp-firstpass");
		action("Luciole, Assembly-UnityScript", "Brix.Legacy.Luciole, Assembly-CSharp-firstpass");
		action2("Brix.Game.Components.Volumes.VolumeCustom, Assembly-CSharp-firstpass");
		action2("Brix.Game.Components.Volumes.VolumeBlueprint, Assembly-CSharp-firstpass");
		action2("Brix.Game.Bricktron.Matricule, Assembly-CSharp-firstpass");
		action2("Vie, Assembly-UnityScript");
		action2("ContenantComponent, Assembly-UnityScript");
		action2("Brix.Game.Components.TypeDeBlocComponent, Assembly-CSharp-firstpass");
		action2("PrefabReseau, Assembly-UnityScript");
		action2("Brix.Game.Components.RigidbodyExtended, Assembly-CSharp-firstpass");
		action2("Passage, Assembly-UnityScript");
		action2("Brix.Legacy.BaliseRenderer, Assembly-CSharp-firstpass");
		return inputjson;
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["version"].Value<int>();
		return num == 7;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", "8").Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}
}
