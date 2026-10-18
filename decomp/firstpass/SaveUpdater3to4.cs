using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater3to4 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater3to4).FullName);

	public SaveUpdater3to4(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(CreateBriktronsV2);
		fileUpdater.AddStep(RemoveAllGarnotte);
		fileUpdater.AddStep(CreateContent);
		fileUpdater.AddStep(DeleteOldBriktrons);
		fileUpdater.AddStep(FixGroups);
		fileUpdater.AddStep(DeleteAccessories);
		fileUpdater.AddStep(DeleteLadders);
		fileUpdater.AddStep(DeleteAllDoors);
		fileUpdater.AddStep(FixCamera);
		fileUpdater.AddStep(FixAllNom);
		fileUpdater.AddStep(FixPlayer);
		fileUpdater.AddStep(FixTunnels);
		fileUpdater.AddStep(FixMineGO);
		fileUpdater.AddStep(FixMineEscalier);
		fileUpdater.AddStep(FixMine);
		fileUpdater.AddStep(FixBlueprints);
		FileUpdater fileUpdater2 = AddFile("gamemode.json", isArray: false);
		fileUpdater2.AddStep(FixUseBriktronV2);
		fileUpdater2.AddStep(FixEnergyBiftron);
		FileUpdater fileUpdater3 = AddFile("meta.json", isArray: false);
		fileUpdater3.AddStep(FixVersion);
		logger.Info("Save updater 3to4 succesfully loaded for: " + saveFile.path);
	}

	public override bool IsUpdatable()
	{
		return !IsSurvival() && !IsUsingBriktronV2();
	}

	private bool IsUsingBriktronV2()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "gamemode.json"));
		JObject jObject = JObject.Parse(json);
		string text = jObject.Value<string>("useBricktronV2");
		if (text == "False" || text == "false")
		{
			return false;
		}
		return true;
	}

	private bool IsSurvival()
	{
		return File.Exists(Path.Combine(saveFile.path, "survival.gamemode.json"));
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", "4").Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixEnergyBiftron(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("$type", "GameMode, Assembly-UnityScript").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("energyRefundForBiftronSoul", 1.0).Make();
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixUseBriktronV2(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("$type", "GameMode, Assembly-UnityScript").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("useBricktronV2", fieldValue: true).Make();
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixGroups(JContainer inputJson, IdGenerator idGen)
	{
		logger.Info("Group Updater");
		GroupeUpdater.SetIdGen(idGen);
		GroupeUpdater.PopulateAll(inputJson);
		JContainer json = DeleteGroups(inputJson, idGen);
		List<string> groupStrings = GroupeUpdater.GetGroupStrings();
		string text = string.Join(",", groupStrings.ToArray());
		JContainer json2 = JArray.Parse("[" + text + "]");
		return UpdateUtils.ConcatJson(json2, json);
	}

	private JContainer DeleteAllDoors(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "ObjetsDynamiques.Porte").Make();
		JContainer jContainer = jObjectFetcher.Delete(inputJson);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Blueprints.Door").Make();
		logger.Info("Deleted all doors and door blueprints");
		return jObjectFetcher.Delete(jContainer);
	}

	private JContainer ReplaceGroupeVoulu(JContainer inputJson, IdGenerator idGen)
	{
		string text = inputJson.ToString();
		foreach (GroupeUpdater item in GroupeUpdater.all)
		{
			string oldValue = "\"groupeVoulu\": " + item.id;
			string newValue = " \"groupe\": {\r\n            \"$component\": \"Brix.Legacy.Groupe, Assembly-CSharp-firstpass\",\r\n            \"$ref\": \"go:" + item.newId + " \" \r\n            }";
			text = text.Replace(oldValue, newValue);
		}
		return JArray.Parse(text);
	}

	private JContainer CreateBriktronsV2(JContainer inputJson, IdGenerator idGen)
	{
		logger.Info("All Briktron Updaters");
		BriktronUpdater.SetIdGen(idGen);
		BriktronUpdater.PopulateAll(inputJson);
		List<string> allBriktronStrings = BriktronUpdater.GetAllBriktronStrings();
		string text = string.Join(",", allBriktronStrings.ToArray());
		JContainer json = JArray.Parse("[" + text + "]");
		return UpdateUtils.ConcatJson(json, inputJson);
	}

	private JContainer CreateContent(JContainer inputJson, IdGenerator idGen)
	{
		logger.Info("All palette Updaters");
		PaletteUpdater.SetIdGen(idGen);
		PaletteUpdater.PopulateAll(inputJson);
		List<string> allContaineeStrings = PaletteUpdater.GetAllContaineeStrings();
		string text = string.Join(",", allContaineeStrings.ToArray());
		JContainer json = JArray.Parse("[" + text + "]");
		return UpdateUtils.ConcatJson(inputJson, json);
	}

	private JContainer DeleteGroups(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$type", "Groupe, Assembly-UnityScript").Make();
		JContainer jContainer = jObjectFetcher.Delete(inputJson);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("name", "Groupes").Make();
		logger.Info("Groups deleted from save file");
		return jObjectFetcher.Delete(jContainer);
	}

	private JContainer DeleteOldBriktrons(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Personnages.Bricktron").Make();
		JContainer jContainer = jObjectFetcher.Delete(inputJson);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Accessories.Hammer").Make();
		jContainer = jObjectFetcher.Delete(jContainer);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Accessories.Pick").Make();
		logger.Info("Briktrons deleted from save file");
		return jObjectFetcher.Delete(jContainer);
	}

	private JContainer DeleteAccessories(JContainer inputJson, IdGenerator idGen)
	{
		JContainer jContainer = inputJson.DeepClone() as JContainer;
		string[] array = new string[7] { "Hammer", "Pick", "Bag", "Axe", "Pick", "HeldDoubleBrick", "HeldDoubleLog" };
		string[] array2 = array;
		foreach (string text in array2)
		{
			JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Accessories." + text).Make();
			jContainer = jObjectFetcher.Delete(jContainer);
		}
		logger.Info("Accessories deleted from save file");
		return jContainer;
	}

	private JContainer DeleteLadders(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Blueprints.Ladder").Make();
		JContainer jContainer = jObjectFetcher.Delete(inputJson);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "ObjetsDynamiques.Echelle").Make();
		jContainer = jObjectFetcher.Delete(jContainer);
		jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$type", "Echelle, Assembly-UnityScript").Make();
		logger.Info("Deleted all ladders");
		return jObjectFetcher.Delete(jContainer);
	}

	private JContainer FixPlayer(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().Path("/Jeu/Players").Name("Player(Clone)")
			.Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().GOName("Player 0").Make();
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.Bricktron.Nom, Assembly-CSharp-firstpass").NestedIn(jGameObjectFetcher)
			.Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().ChangeFieldValue("nom", "Player 0").Make();
		JComponentFetcher compFetcher2 = JComponentFetcher.New().Type("Brix.Legacy.Joueur, Assembly-CSharp-firstpass").NestedIn(jGameObjectFetcher)
			.Make();
		JComponentModifier jComponentModifier2 = JComponentModifier.New().ChangeFieldValue("_nom", "Player 0").Make();
		JContainer jContainer = jComponentModifier.Apply(compFetcher, inputJson);
		jContainer = jComponentModifier2.Apply(compFetcher2, jContainer);
		jContainer = jGameObjectModifier.Apply(jGameObjectFetcher, jContainer);
		logger.Info("Fixed player in save file");
		return jContainer;
	}

	private JContainer FixCamera(JContainer inputJSon, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Type("CameraScript, Assembly-UnityScript").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().Type("Brix.Input.CameraScript, Assembly-CSharp-firstpass").Make();
		logger.Info("Fixed Camera in save file");
		return jObjectModifier.Apply(fetcher, inputJSon);
	}

	private JContainer FixAllNom(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Type("Joueur, Assembly-UnityScript").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().Type("Brix.Legacy.Joueur, Assembly-CSharp-firstpass").Make();
		logger.Info("Fixed Nom in save file");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer RemoveAllGarnotte(JContainer inputJson, IdGenerator idGen)
	{
		JContainer jContainer = inputJson.DeepClone() as JContainer;
		for (int i = 1; i < 5; i++)
		{
			JObjectFetcher jObjectFetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "ObjetsDynamiques.Garnotte" + i).Make();
			jContainer = jObjectFetcher.Delete(jContainer);
		}
		logger.Info("Deleted all garnotte from save file");
		return jContainer;
	}

	private JContainer FixTunnels(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().AssetKey("ObjetsDynamiques.Tunnel").Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().Path("/Jeu/Groupes").Make();
		JGameObjectModifier jGameObjectModifier2 = JGameObjectModifier.New().AssetKey("Groupes.Tunnel").Make();
		JGameObjectModifier jGameObjectModifier3 = JGameObjectModifier.New().RemoveField("$parent").Make();
		JGameObjectModifier jGameObjectModifier4 = JGameObjectModifier.New().RemoveField("activeSelf").Make();
		JContainer jContainer = jGameObjectModifier4.Apply(fetcher, inputJson);
		jContainer = jGameObjectModifier3.Apply(fetcher, jContainer);
		jContainer = jGameObjectModifier.Apply(fetcher, jContainer);
		logger.Info("Fixed Tunnels in save file");
		return jGameObjectModifier2.Apply(fetcher, jContainer);
	}

	private JContainer FixMineEscalier(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Type("MineEscalier, Assembly-UnityScript").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().Type("Brix.Legacy.MineEscalier, Assembly-CSharp-firstpass").Make();
		logger.Info("Fixed MineEscalier in save file");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixMine(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Type("Mine, Assembly-UnityScript").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().Type("Brix.Legacy.Mine, Assembly-CSharp-firstpass").Make();
		logger.Info("Fixed Mine in save file");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixMineGO(JContainer inputJson, IdGenerator idGen)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().AssetKey("ObjetsDynamiques.Mine").Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().AssetKey("Groupes.Mine").Make();
		JGameObjectModifier jGameObjectModifier2 = JGameObjectModifier.New().Path("/Jeu/Groupes").Make();
		JContainer jContainer = jGameObjectModifier2.Apply(fetcher, inputJson);
		logger.Info("Fixed Mine GameoObjects in save file");
		return jGameObjectModifier.Apply(fetcher, jContainer);
	}

	private JContainer FixBlueprints(JContainer inputJson, IdGenerator idGen)
	{
		JContainer jContainer = inputJson.DeepClone() as JContainer;
		string[] array = new string[6] { "Pallet", "Barrel", "Catapult", "Torch", "Door", "Lantern" };
		string[] array2 = new string[11]
		{
			"Stone_DoubleBrick", "Plank", "Wood_DoubleLog_VAA", "Wood_DoubleLog_AA", "Wood_DoublePlank", "Stone_Arch", "Stone_Column_Base", "Stone_Column_Middle", "Stone_Column_Top", "Stone_Staircase",
			"Stone_Brick"
		};
		GroupeUpdater groupeUpdater = GroupeUpdater.all.Find((GroupeUpdater x) => x.name == "Structures");
		if (groupeUpdater != null)
		{
			string[] array3 = array;
			foreach (string structureName in array3)
			{
				JGameObjectFetcher fetcherParent = JGameObjectFetcher.New().AssetKey("Groupes.Structures").Make();
				jContainer = FixOneBlueprint(jContainer, structureName, fetcherParent, "/Jeu/Groupes/go:" + groupeUpdater.newId);
			}
		}
		GroupeUpdater groupeUpdater2 = GroupeUpdater.all.Find((GroupeUpdater x) => x.name == "Blocs");
		if (groupeUpdater2 != null)
		{
			string[] array4 = array2;
			foreach (string structureName2 in array4)
			{
				JGameObjectFetcher fetcherParent2 = JGameObjectFetcher.New().AssetKey("Groupes.Blocks").Make();
				jContainer = FixOneBlueprint(jContainer, structureName2, fetcherParent2, "/Jeu/Groupes/go:" + groupeUpdater2.newId);
			}
		}
		logger.Info("Fixed Blueprints in save file");
		return jContainer;
	}

	private JContainer FixOneBlueprint(JContainer inputJson, string structureName, JGameObjectFetcher fetcherParent, string groupPath)
	{
		JGameObjectFetcher fetcher = JGameObjectFetcher.New().AssetKey("Blueprints." + structureName).Make();
		JComponentFetcher.New().FieldEqualsTo("$type", "Brix.Legacy.CubeFantome, Assembly-CSharp-firstpass").Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().ChangeFieldValue("$path", groupPath).Make();
		JGameObjectModifier jGameObjectModifier2 = JGameObjectModifier.New().RemoveField("DoNotReparentOnDeserialized").Make();
		JGameObjectModifier jGameObjectModifier3 = JGameObjectModifier.New().InjectJGameObject("$parent", fetcherParent).Make();
		JObjectFetcher.New().Make();
		JContainer jContainer = jGameObjectModifier.Apply(fetcher, inputJson);
		jContainer = jGameObjectModifier3.Apply(fetcher, jContainer);
		return jGameObjectModifier2.Apply(fetcher, jContainer);
	}
}
