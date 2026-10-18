using System;
using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater18to19 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater18to19).FullName);

	public SaveUpdater18to19(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(AssemblyUpgrade);
		fileUpdater.AddStep(RemoveLanterns);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	private JContainer RemoveLanterns(JContainer inputJson, IdGenerator idGen)
	{
		inputJson = JGameObjectFetcher.New().FieldEqualsTo("$assetKey", "ObjetsDynamiques.Lanterne").Make()
			.Delete(inputJson);
		inputJson = JGameObjectFetcher.New().FieldEqualsTo("$assetKey", "Blueprints.Lantern").Make()
			.Delete(inputJson);
		return inputJson;
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 18;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 18).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 19).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		Action<string> action = delegate(string oldType)
		{
			inputjson = JObjectFetcher.New().Type(oldType).Make()
				.Delete(inputjson);
		};
		action("Brix.Legacy.SpawnerCristal, Assembly-CSharp-firstpass");
		return inputjson;
	}
}
