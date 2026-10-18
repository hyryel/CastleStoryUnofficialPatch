using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater24to25 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater24to25).FullName);

	public SaveUpdater24to25(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("meta.json", isArray: false);
		fileUpdater.AddStep(FixBiome);
		fileUpdater.AddStep(FixVersion);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 24;
	}

	private JContainer FixBiome(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 24).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__BIOMETYPE", "Green").Make();
		logger.Info("Updated biome sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 24).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 25).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}
}
