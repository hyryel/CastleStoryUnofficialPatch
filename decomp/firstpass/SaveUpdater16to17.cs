using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater16to17 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater16to17).FullName);

	public SaveUpdater16to17(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(AssemblyUpgrade);
		fileUpdater.AddStep(RemoveWorkingUnits);
		fileUpdater.AddStep(RemovePhaseOut);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	private JContainer RemoveWorkingUnits(JContainer inputJson, IdGenerator idGen)
	{
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.ResourceDistributor, Assembly-CSharp-firstpass").Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().RemoveField("_workingUnits").Make();
		return jComponentModifier.Apply(compFetcher, inputJson);
	}

	private JContainer RemovePhaseOut(JContainer inputJson, IdGenerator idGen)
	{
		JComponentFetcher compFetcher = JComponentFetcher.New().Type("Brix.Game.AI.Labor, Assembly-CSharp-firstpass").Make();
		JComponentModifier jComponentModifier = JComponentModifier.New().RemoveField("_isPhaseOut").Make();
		return jComponentModifier.Apply(compFetcher, inputJson);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 16;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 16).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 17).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		return inputjson;
	}
}
