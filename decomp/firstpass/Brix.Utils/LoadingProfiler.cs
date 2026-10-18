using System;
using System.Collections.Generic;
using Brix.Engine;
using UnityEngine;

namespace Brix.Utils;

public class LoadingProfiler : MonoBehaviour
{
	[Serializable]
	public class Entry
	{
		public string name;

		public float time;

		public float deltaTime;
	}

	public bool pauseBetweenSteps;

	public float awakeTime;

	public float startTime;

	public List<Entry> steps = new List<Entry>();

	private void Awake()
	{
		awakeTime = Time.realtimeSinceStartup;
	}

	private void Start()
	{
		string[] array = new string[12]
		{
			"NetworkWelcomed", "NetworkStarted", "LoadingStarted", "GamemodeLoaded", "BeforeTerrainLoaded", "TerrainLoaded", "LevelLoaded", "TerrainReady", "LevelReady", "LevelSaveReady",
			"PhysicsReady", "WaitingForPlayers"
		};
		startTime = Time.realtimeSinceStartup;
		steps.Clear();
		string[] array2 = array;
		foreach (string text in array2)
		{
			GameSignals.GameSignal gs = (GameSignals.GameSignal)RearViewMirror.GetField(typeof(GameSignals), null, text);
			string name = text;
			GameSignals.Once(gs, delegate
			{
				AddEntry(name, Time.realtimeSinceStartup - startTime);
			});
		}
	}

	private void AddEntry(string name, float time)
	{
		Entry entry = new Entry();
		entry.name = name;
		entry.time = time;
		Entry entry2 = entry;
		if (steps.Count > 0)
		{
			entry2.deltaTime = time - steps[steps.Count - 1].time;
		}
		steps.Add(entry2);
	}
}
