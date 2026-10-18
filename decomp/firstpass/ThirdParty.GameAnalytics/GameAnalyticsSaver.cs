using System;
using System.IO;
using System.Threading;
using Brix.Assets;
using Newtonsoft.Json;

namespace ThirdParty.GameAnalytics;

public class GameAnalyticsSaver
{
	public readonly string dataPath;

	public readonly string timeStampPath;

	private Thread saveThread;

	private JsonSerializer serializer;

	public bool Saving => saveThread != null && saveThread.IsAlive;

	public GameAnalyticsSaver(string dataPath, string timeStampPath)
	{
		this.dataPath = dataPath;
		this.timeStampPath = timeStampPath;
	}

	public void WaitForSave()
	{
		if (Saving)
		{
			saveThread.Join();
		}
	}

	public void Save(GameAnalyticsStore store)
	{
		if (!Saving)
		{
			saveThread = new Thread((ThreadStart)delegate
			{
				_Save(store);
			});
			saveThread.Start();
		}
	}

	private void _Save(GameAnalyticsStore store)
	{
		if (File.Exists(dataPath))
		{
			File.Delete(dataPath);
		}
		Asset_Base<Asset_Map>.Disk_WriteJsonData(store, dataPath, humanReadable: true);
	}

	public void SaveTimeStamp(DateTime moment)
	{
		int num = 0;
		while (true)
		{
			try
			{
				num++;
				File.WriteAllText(timeStampPath, moment.ToString());
				break;
			}
			catch (Exception)
			{
				if (num > 3)
				{
					throw;
				}
				Thread.Sleep(100);
			}
		}
	}

	public GameAnalyticsStore Load()
	{
		if (File.Exists(dataPath))
		{
			string value = File.ReadAllText(dataPath);
			return JsonConvert.DeserializeObject<GameAnalyticsStore>(value);
		}
		return null;
	}

	public void DeleteStoreData()
	{
		if (File.Exists(dataPath))
		{
			File.Delete(dataPath);
		}
	}

	public DateTime LastTimePlayed()
	{
		if (File.Exists(timeStampPath) && DateTime.TryParse(File.ReadAllText(timeStampPath), out var result))
		{
			return result;
		}
		return DateTime.MaxValue;
	}
}
