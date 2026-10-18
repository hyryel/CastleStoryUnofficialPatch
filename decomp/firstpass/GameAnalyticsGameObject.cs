using System.Collections.Generic;
using ThirdParty.GameAnalytics;
using UnityEngine;

public class GameAnalyticsGameObject : MonoBehaviour
{
	public bool testPushToServer;

	public List<string> pusheds;

	private float autoSaveTimer;

	private GameAnalyticsManager manager;

	private void Init()
	{
		manager = new GameAnalyticsManager("Info/game_analytics_events.json", "Info/timestamp.txt");
		autoSaveTimer = Time.time;
		testPushToServer = false;
		TestSignals();
	}

	private void TestSignals()
	{
	}

	private void OnLevelWasLoaded(int level)
	{
		TestSignals();
		if (manager != null)
		{
			manager.ReconnectToSignals();
		}
	}

	private void Awake()
	{
		Init();
	}

	private void Update()
	{
		if (Time.time - autoSaveTimer > 10f)
		{
			autoSaveTimer = Time.time;
			manager.SaveStore();
		}
		if (testPushToServer)
		{
			pusheds = manager.PushToServer();
			testPushToServer = false;
		}
	}

	private void OnLoadGame()
	{
	}

	private void OnDestroy()
	{
		if (manager != null)
		{
			manager.PushToServer();
			GA_Queue.ForceSubmit();
		}
	}
}
