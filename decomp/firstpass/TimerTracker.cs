using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

public class TimerTracker : MonoBehaviour
{
	private class EventTime
	{
		public float timeAtStart;

		public float actionTime;

		public EventTime(float timeAtStart, float actionTime)
		{
			this.timeAtStart = timeAtStart;
			this.actionTime = actionTime;
		}
	}

	public Signal<GameObject> TimerReached = new Signal<GameObject>();

	private Dictionary<GameObject, EventTime> timers = new Dictionary<GameObject, EventTime>();

	public void StartTimer(GameObject go, float timeLimit)
	{
		if (!timers.ContainsKey(go))
		{
			timers.Add(go, new EventTime(Time.time, Time.time + timeLimit));
		}
	}

	public void ResetTimer(GameObject go)
	{
		if (timers.ContainsKey(go))
		{
			timers[go].actionTime = Time.time + (timers[go].actionTime - timers[go].timeAtStart);
			timers[go].timeAtStart = Time.time;
		}
	}

	public void StopTimer(GameObject go)
	{
		if (timers.ContainsKey(go))
		{
			timers.Remove(go);
		}
	}

	private void Update()
	{
		List<GameObject> list = CollectionPool.RequestList<GameObject>();
		foreach (KeyValuePair<GameObject, EventTime> timer in timers)
		{
			if (Time.time >= timer.Value.actionTime)
			{
				if (timer.Key != null)
				{
					TimerReached.Invoke(timer.Key);
				}
				list.Add(timer.Key);
			}
		}
		list.Slinq().ForEach(delegate(GameObject go, TimerTracker self)
		{
			self.timers.Remove(go);
		}, this);
		CollectionPool.Release(ref list);
	}
}
