using System;
using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater6to7 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater6to7).FullName);

	public SaveUpdater6to7(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(AssemblyUpgrade);
		fileUpdater.AddStep(MagieUpgrade);
		fileUpdater.AddStep(BlueprintUpgrade);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
		logger.Info("Save updater 6to7 succesfully loaded for: " + saveFile.path);
	}

	private JContainer AssemblyUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		Action<string, string> action = delegate(string oldType, string newType)
		{
			JObjectFetcher fetcher = JObjectFetcher.New().Type(oldType).Make();
			JObjectModifier jObjectModifier = JObjectModifier.New().Type(newType).Make();
			inputjson = jObjectModifier.Apply(fetcher, inputjson);
		};
		Action<string> action2 = delegate(string oldType)
		{
			inputjson = JObjectFetcher.New().Type(oldType).Make()
				.Delete(inputjson);
		};
		action("SpawnerCristal, Assembly-UnityScript", "Brix.Legacy.SpawnerCristal, Assembly-CSharp-firstpass");
		action("PaletteScript, Assembly-UnityScript", "Brix.Legacy.PaletteScript, Assembly-CSharp-firstpass");
		action("Torche, Assembly-UnityScript", "Brix.Legacy.Torche, Assembly-CSharp-firstpass");
		action("Lanterne, Assembly-UnityScript", "Brix.Legacy.Lanterne, Assembly-CSharp-firstpass");
		action("Convertisseur, Assembly-UnityScript", "Brix.Game.Components.ResourceConverter, Assembly-CSharp-firstpass");
		action2("ObstacleGenerique, Assembly-UnityScript");
		action2("BaliseCustom, Assembly-UnityScript");
		action("Brix.Game.AI.PathMarker, Assembly-CSharp-firstpass", "Brix.Game.AI.Pathfinding.PathMarker, Assembly-CSharp-firstpass");
		return inputjson;
	}

	private JContainer MagieUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Type("Magie, Assembly-UnityScript").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().Type("IsMagical, Assembly-CSharp-firstpass").ChangeFieldName("_pm", "magicPoints")
			.Make();
		return jObjectModifier.Apply(fetcher, inputjson);
	}

	private JContainer BlueprintUpgrade(JContainer inputjson, IdGenerator idgen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Type("Brix.Legacy.CubeFantome, Assembly-CSharp-firstpass").Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().Type("Brix.Legacy.Blueprint, Assembly-CSharp-firstpass").RemoveField("aConfig")
			.RemoveField("bConfig")
			.Make();
		return jObjectModifier.Apply(fetcher, inputjson);
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["version"].Value<int>();
		return num == 6;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", "7").Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}
}
