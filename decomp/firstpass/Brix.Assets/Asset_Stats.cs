using System;
using System.Collections;
using System.IO;
using Brix.Game;
using Brix.IO.Serialization;
using Brix.Utils.Logging;
using Newtonsoft.Json;

namespace Brix.Assets;

public class Asset_Stats
{
	private static readonly string __F_META = "stats.json";

	private static readonly string path = "Info/";

	private static BrixLogger logger = new BrixLogger(typeof(Asset_Stats).FullName);

	public Asset_Stats()
	{
		Disk_ReadMeta();
	}

	public void Disk_ReadMeta()
	{
		if (File.Exists(path + __F_META))
		{
			Hashtable table = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(path + __F_META));
			StatsTracker.LoadStats(table);
		}
		else
		{
			StatsTracker.LoadStats(null);
		}
	}

	protected void Disk_WriteMeta()
	{
		if (!StatsTracker.IsLegit())
		{
			return;
		}
		try
		{
			Hashtable value = StatsTracker.SaveStats();
			using StreamWriter textWriter = File.CreateText(path + __F_META);
			using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
			jsonWriter.Formatting = Formatting.Indented;
			Io.GenericSerializer.Serialize(jsonWriter, value);
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
	}

	public void Disk_Overwrite()
	{
		Disk_WriteMeta();
	}
}
