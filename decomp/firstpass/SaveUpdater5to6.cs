using System.Collections.Generic;
using System.IO;
using Brix.Assets;
using Brix.Engine.FlatWorld;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater5to6 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater5to6).FullName);

	public SaveUpdater5to6(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("meta.json", isArray: false);
		fileUpdater.AddStep(ConvertHeightmap);
		fileUpdater.AddStep(FixVersion);
		logger.Info("Save updater 5to6 succesfully loaded for: " + saveFile.path);
	}

	private JContainer ConvertHeightmap(JContainer inputJson, IdGenerator idGen)
	{
		GridToLayersConverter.Convert(saveFile.path);
		return inputJson;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("version", "6").Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}

	public override bool IsUpdatable()
	{
		return Directory.Exists(Path.Combine(saveFile.path, "Lod0")) || File.Exists(Path.Combine(saveFile.path, "Heightmap.png"));
	}
}
