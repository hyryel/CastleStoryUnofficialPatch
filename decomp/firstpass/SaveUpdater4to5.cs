using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater4to5 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater4to5).FullName);

	public SaveUpdater4to5(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(AssetKeyJObjectUpgrade);
		fileUpdater.AddStep(ResourceDistributorUpgrade);
		fileUpdater.AddStep(WorkingUnitUpgrade);
		FileInfo[] files = new DirectoryInfo(saveFile.path).GetFiles("*gamemode.json");
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			AddFile(fileInfo.Name, isArray: false);
		}
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
		logger.Info("Save updater 4to5 succesfully loaded for: " + saveFile.path);
	}

	private JContainer WorkingUnitUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		JObject jObjectToInject = JObject.Parse("{\"$type\": \"Brix.Game.AI.ResourceConfig, Assembly-CSharp-firstpass\", \"_assetKey\": \"Garnottes.Stone1\", \"Rarity\": 1, \"Quantity\": 20, \"Resource\": { \"$type\": \"Brix.Game.AI.Stones, Assembly-CSharp-firstpass\", \"_valeur\": 1}}");
		JObjectFetcher fetcher = JObjectFetcher.New().Type("Brix.Game.AI.WorkingUnit, Assembly-CSharp-firstpass").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().RemoveField("_resource").InjectJObject("_rConfig", jObjectToInject)
			.Make();
		return jObjectModifier.Apply(fetcher, inputjson);
	}

	private JContainer ResourceDistributorUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Type("Brix.Game.AI.ResourceDistributor, Assembly-CSharp-firstpass").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().RemoveField("WantedResources").RemoveField("ResourcesAvailable")
			.Make();
		return jObjectModifier.Apply(fetcher, inputjson);
	}

	private JContainer AssetKeyJObjectUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Type("Brix.External.Factories.Factory+AssetKey, Brix.External").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ReplaceByValue(ConvertAssetKeyJObjectToString).Make();
		return jObjectModifier.Apply(fetcher, inputjson);
	}

	private string ConvertAssetKeyJObjectToString(JObject jobjectToBeConverted, JContainer allData)
	{
		return string.Concat(jobjectToBeConverted["Factory"], ".", jobjectToBeConverted["Name"]);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["version"].Value<int>();
		return num == 4;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", "5").Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}
}
