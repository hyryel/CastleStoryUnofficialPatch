using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater10to11 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater10to11).FullName);

	public SaveUpdater10to11(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(FixFaction);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["version"].Value<int>();
		return num == 10;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", "11").Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixFaction(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Core.Faction").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("DoNotCreateOnDeserialized", fieldValue: true).Make();
		inputJson = jObjectModifier.Apply(fetcher, inputJson);
		logger.Info("Updated Factions");
		return inputJson;
	}
}
