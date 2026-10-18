using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater20to21 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater20to21).FullName);

	public SaveUpdater20to21(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(DeleteIdleGroups_MapsOnly);
		fileUpdater.AddStep(DeleteGameClock_MapsOnly);
		fileUpdater.AddStep(FixCatapultNameSpace);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	private JContainer FixCatapultNameSpace(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Type("Brix.Legacy.Catapult.Catapult, Assembly-CSharp-firstpass").Make();
		inputJson = JObjectModifier.New().ChangeFieldValue("$type", "Brix.Game.Components.CatapultSpace.Catapult, Assembly-CSharp-firstpass").Make()
			.Apply(fetcher, inputJson);
		JObjectFetcher fetcher2 = JObjectFetcher.New().Type("Brix.Legacy.Catapult.CatapultBody, Assembly-CSharp-firstpass").Make();
		inputJson = JObjectModifier.New().ChangeFieldValue("$type", "Brix.Game.Components.CatapultSpace.CatapultBody, Assembly-CSharp-firstpass").Make()
			.Apply(fetcher2, inputJson);
		JObjectFetcher fetcher3 = JObjectFetcher.New().Type("Brix.Legacy.Catapult.CatapultControl, Assembly-CSharp-firstpass").Make();
		inputJson = JObjectModifier.New().ChangeFieldValue("$type", "Brix.Game.Components.CatapultSpace.CatapultControl, Assembly-CSharp-firstpass").Make()
			.Apply(fetcher3, inputJson);
		JObjectFetcher fetcher4 = JObjectFetcher.New().Type("Brix.Game.Components.CatapultOperations, Assembly-CSharp-firstpass").Make();
		inputJson = JObjectModifier.New().ChangeFieldValue("$type", "Brix.Game.Components.CatapultSpace.CatapultOperations, Assembly-CSharp-firstpass").Make()
			.Apply(fetcher4, inputJson);
		JObjectFetcher fetcher5 = JObjectFetcher.New().FieldEqualsTo("$component", "Brix.Legacy.Catapult.CatapultControl, Assembly-CSharp-firstpass").Make();
		inputJson = JObjectModifier.New().ChangeFieldValue("$component", "Brix.Game.Components.CatapultSpace.CatapultControl, Assembly-CSharp-firstpass").Make()
			.Apply(fetcher5, inputJson);
		JObjectFetcher fetcher6 = JObjectFetcher.New().FieldEqualsTo("$component", "Brix.Legacy.Catapult.CatapultBody, Assembly-CSharp-firstpass").Make();
		inputJson = JObjectModifier.New().ChangeFieldValue("$component", "Brix.Game.Components.CatapultSpace.CatapultBody, Assembly-CSharp-firstpass").Make()
			.Apply(fetcher6, inputJson);
		JObjectFetcher fetcher7 = JObjectFetcher.New().FieldEqualsTo("$component", "Brix.Legacy.Catapult.Catapult, Assembly-CSharp-firstpass").Make();
		inputJson = JObjectModifier.New().ChangeFieldValue("$component", "Brix.Game.Components.CatapultSpace.Catapult, Assembly-CSharp-firstpass").Make()
			.Apply(fetcher7, inputJson);
		return inputJson;
	}

	private JContainer DeleteIdleGroups_MapsOnly(JContainer inputJson, IdGenerator idGen)
	{
		if (saveFile.IsNewGame())
		{
			inputJson = JGameObjectFetcher.New().FieldEqualsTo("$assetKey", "Groupes.Idle").Make()
				.Delete(inputJson);
		}
		return inputJson;
	}

	private JContainer DeleteGameClock_MapsOnly(JContainer inputJson, IdGenerator idGen)
	{
		if (saveFile.IsNewGame())
		{
			inputJson = JGameObjectFetcher.New().Name("LuaScripts").Make()
				.Delete(inputJson);
		}
		return inputJson;
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 20;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 20).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 21).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}
}
