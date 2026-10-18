using System;
using System.IO;
using System.Threading;
using Brix.Engine;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Legacy;

public class Vidangeur : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(Vidangeur).FullName);

	public float delaiMaximum;

	public float delaiMinimum;

	public bool ramasseUnity = true;

	public bool ramassePool = true;

	public bool ramasseDotNet;

	public bool ramasseOutputLog = true;

	public bool isThreadedGC;

	public bool collecteOnStart = true;

	public bool collecteOnDestroy;

	public bool existe = true;

	public bool forceDefaultValues = true;

	public float tempsDepuis;

	public long memoireALaDerniereAllocation;

	public long memoireActuelleEstimee;

	public bool collecte;

	public AsyncOperation asyncOp;

	public Thread threadGC;

	private ThreadStart _collecteGCAction = delegate
	{
	};

	public float combienActif;

	public float facteurDeSommeil;

	public static Vidangeur instance;

	public long memoryPressure;

	private Vector3 vieilleCameraPosition = Vector3.zero;

	private Vector3 vieilleCameraForward = Vector3.forward;

	private void Start()
	{
		instance = this;
		if (delaiMaximum <= 0f || forceDefaultValues)
		{
			delaiMaximum = 900f;
		}
		if (delaiMinimum <= 0f || forceDefaultValues)
		{
			delaiMinimum = 300f;
		}
		if (facteurDeSommeil <= 0f || forceDefaultValues)
		{
			facteurDeSommeil = Mathf.Pow(0.5f, Time.fixedDeltaTime);
		}
		if (forceDefaultValues)
		{
			ramasseUnity = false;
			ramasseDotNet = false;
			ramasseOutputLog = true;
			ramassePool = true;
			isThreadedGC = false;
			collecteOnStart = true;
			collecteOnDestroy = false;
			memoryPressure = 2000000000L;
		}
		_collecteGCAction = CollecteGC_Thread;
		GameSignals.DisableUntil(GameSignals.LevelReady, this);
		if (collecteOnStart)
		{
			GameSignals.LateConnectOnce(GameSignals.LevelReady, LancerLaCollecte);
		}
	}

	private void OnDestroy()
	{
		if (collecteOnDestroy)
		{
			LancerLaCollecte();
		}
	}

	public static float MemBase(long mem)
	{
		if (mem < 512)
		{
			return mem;
		}
		if (mem < 524288)
		{
			return Mathf.Round((float)mem / 1024f * 10f) / 10f;
		}
		return Mathf.Round((float)mem / 1024f / 1024f * 10f) / 10f;
	}

	public static string MemUnit(long mem)
	{
		if (mem < 512)
		{
			return "bytes";
		}
		if (mem < 524288)
		{
			return "K";
		}
		return "M";
	}

	public void CollecteGC()
	{
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		GC.AddMemoryPressure(memoryPressure);
		GC.Collect();
		GC.RemoveMemoryPressure(memoryPressure);
		long totalMemory2 = GC.GetTotalMemory(forceFullCollection: false);
		long mem = totalMemory - totalMemory2;
		logger.InfoFormat("Vidangeur ramasse {0} {1} inutiles (total: {2} {3}).", MemBase(mem), MemUnit(mem), MemBase(totalMemory2), MemUnit(totalMemory2));
		memoireALaDerniereAllocation = memoireActuelleEstimee;
		memoireActuelleEstimee = totalMemory2;
	}

	private void CollecteGC_Thread()
	{
		Thread.Sleep(0);
		CollecteGC();
		threadGC = null;
	}

	public void LancerLaCollecte()
	{
		LancerLaCollecte(ramasseUnity, ramassePool, ramasseDotNet, ramasseOutputLog);
	}

	public void LancerLaCollecte(bool ramasseUnity, bool ramassePool, bool ramasseDotNet, bool ramasseOutputLog)
	{
		if (ramasseUnity)
		{
			asyncOp = Resources.UnloadUnusedAssets();
		}
		if (ramassePool)
		{
			CollectionPool.ClearAllCollections();
			ObjectPool.ClearAllPools();
			ObjectPool2.ClearAllPools();
		}
		if (ramasseDotNet)
		{
			if (isThreadedGC)
			{
				threadGC = new Thread(_collecteGCAction);
				threadGC.Start();
			}
			else
			{
				CollecteGC();
			}
		}
		if (ramasseOutputLog)
		{
			try
			{
				string path = "Castle Story Prototype_Data/output_log.txt";
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch (Exception message)
			{
				logger.Warn(message);
				this.ramasseOutputLog = false;
			}
		}
		tempsDepuis = 0f;
	}

	private void FixedUpdate()
	{
		if (base.enabled)
		{
			combienActif *= facteurDeSommeil;
			if (Camera.main.transform.position != vieilleCameraPosition || Camera.main.transform.forward != vieilleCameraForward)
			{
				combienActif += Time.fixedDeltaTime;
				vieilleCameraPosition = Camera.main.transform.position;
				vieilleCameraForward = Camera.main.transform.forward;
			}
		}
	}

	private void Update()
	{
		if (!base.enabled)
		{
			return;
		}
		if (collecte)
		{
			if (asyncOp != null && asyncOp.isDone)
			{
				asyncOp = null;
			}
			if (asyncOp == null && threadGC == null)
			{
				collecte = false;
			}
			return;
		}
		tempsDepuis += Time.unscaledDeltaTime;
		if (tempsDepuis >= delaiMinimum)
		{
			if (tempsDepuis >= delaiMaximum)
			{
				collecte = true;
			}
			else if (combienActif < 0.1f)
			{
				collecte = true;
			}
			if (collecte)
			{
				LancerLaCollecte(ramasseUnity, ramassePool, ramasseDotNet, ramasseOutputLog);
			}
		}
	}

	public static void CollecteManuelle(bool ramasseUnity, bool ramassePool, bool ramasseDotNet, bool ramasseOutputLog)
	{
		if ((bool)instance)
		{
			instance.LancerLaCollecte(ramasseUnity, ramassePool, ramasseDotNet, ramasseOutputLog);
		}
	}
}
