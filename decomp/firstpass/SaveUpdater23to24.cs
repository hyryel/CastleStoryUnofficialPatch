using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater23to24 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater23to24).FullName);

	public static string affiliationType = "Brix.Game.Components.AffiliationComponent, Assembly-CSharp-firstpass";

	public SaveUpdater23to24(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(FixBricktronCap);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 23;
	}

	private JContainer FixBricktronCap(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldIsNotNull("startingWorkersCount").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("bricktronCap", 15).Make();
		JObjectFetcher fetcher2 = JObjectFetcher.New().FieldEqualsTo("startingWorkersCount", 12).Make();
		JObjectModifier jObjectModifier2 = JObjectModifier.New().ChangeFieldValue("bricktronCap", 12).Make();
		logger.Info("Updated bricktron cap sucessfully");
		JContainer jContainer = jObjectModifier.Apply(fetcher, inputJson);
		return jObjectModifier2.Apply(fetcher2, jContainer);
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 23).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 24).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}
}
