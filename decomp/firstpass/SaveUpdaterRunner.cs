using System;
using System.Threading;
using Brix.Assets;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Logging;

public class SaveUpdaterRunner
{
	protected static BrixLogger logger = new BrixLogger(typeof(SaveUpdaterRunner).FullName);

	private static Signal UpdateWorkerEnded = new Signal();

	private static Thread SaveUpdateWorkerThread;

	private static Asset_Map _map;

	private static Action<Asset_Map, bool> _onAfterUpdateComplete;

	private static readonly Action EndUpdate = delegate
	{
		bool flag = _map.IsCurrentVersion();
		if (flag)
		{
			_map.ApplyStaging();
		}
		else
		{
			_map.CancelStaging();
		}
		if (_onAfterUpdateComplete != null)
		{
			_onAfterUpdateComplete(_map, flag);
		}
		Reset();
	};

	private static void Set(Asset_Map map, Action<Asset_Map, bool> onAfterUpdateComplete)
	{
		_map = map;
		_onAfterUpdateComplete = onAfterUpdateComplete;
	}

	private static void Reset()
	{
		_map = null;
		_onAfterUpdateComplete = null;
	}

	public static void Update(Asset_Map map, Action<Asset_Map, bool> onAfterUpdateComplete)
	{
		Set(map, onAfterUpdateComplete);
		BeginUpdate();
		SaveUpdateWorkerThread = new Thread(DoUpdate);
		UpdateWorkerEnded.ConnectOnce(EndUpdate);
		SaveUpdateWorkerThread.Start();
		PersistentCoroutineRunner.Instance.StartCoroutine(PersistentCoroutineRunner.CallbackWhenThreadEnd(SaveUpdateWorkerThread, UpdateWorkerEnded.Invoke));
	}

	private static void BeginUpdate()
	{
		_map.Disk_CreateBackup();
		_map.MoveToStaging();
	}

	private static void DoUpdate()
	{
		try
		{
			SaveUpdater instance = SaveUpdater.GetInstance(_map);
			instance.UpdateSave();
			instance.Write(_map.path);
			_map.Disk_ReadMeta();
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
	}
}
