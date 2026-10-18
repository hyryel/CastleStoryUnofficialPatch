using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater15to16 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater15to16).FullName);

	public SaveUpdater15to16(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(AssemblyUpgrade);
		fileUpdater.AddStep(UpdateLuaStandalone);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 15;
	}

	private JContainer UpdateLuaStandalone(JContainer inputJson, IdGenerator idGen)
	{
		JComponentFetcher jComponentFetcher = JComponentFetcher.New().Type("Brix.Lua.LuaStandalone, Assembly-CSharp-firstpass").Make();
		if (jComponentFetcher.Fetch(inputJson).FirstOrDefault() is JObject jObject)
		{
			string text = jObject["Descriptor"]["Path"].ToString();
			JObject jObject2 = JObject.Parse("{\"$type\": \"System.Collections.Generic.Dictionary`2[[System.Object, mscorlib],[System.Object, mscorlib]], mscorlib\",}");
			jObject2["path"] = "Info/Lua/" + text;
			jObject2["pathRelative"] = text;
			JComponentModifier jComponentModifier = JComponentModifier.New().InjectJObject("_csdata", jObject2).Make();
			inputJson = jComponentModifier.Apply(jComponentFetcher, inputJson);
		}
		return inputJson;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 15).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 16).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		return inputjson;
	}
}
