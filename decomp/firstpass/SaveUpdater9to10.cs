using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater9to10 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater9to10).FullName);

	public SaveUpdater9to10(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(FixBalise);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["version"].Value<int>();
		return num == 9;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", "10").Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer FixBalise(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Balises.Balise2").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("$assetKey", "Balises.DefaultBalise").Make();
		inputJson = jObjectModifier.Apply(fetcher, inputJson);
		fetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Balises.Custom").Make();
		jObjectModifier = JObjectModifier.New().ChangeFieldValue("$assetKey", "Balises.DefaultBalise").Make();
		inputJson = jObjectModifier.Apply(fetcher, inputJson);
		fetcher = JObjectFetcher.New().FieldEqualsTo("$assetKey", "Balises.DefaultBalise").Make();
		jObjectModifier = JObjectModifier.New().ChangeFieldValue("baliseMode", 4).Make();
		inputJson = jObjectModifier.Apply(fetcher, inputJson);
		logger.Info("Updated Balises to default balise");
		return inputJson;
	}
}
