using System;
using System.Collections.Generic;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Game.Network;

[Serializable]
public struct TrackingPair
{
	private static Dictionary<int, Func<GameObject, IObjectTracker>> _getTrackerFuncs = new Dictionary<int, Func<GameObject, IObjectTracker>>
	{
		{ 1, GetTracker<IGripTracker> },
		{ 2, GetTracker<IResourceTracker> },
		{ 3, GetTracker<ICharacterTracker> },
		{ 4, GetTracker<IBlueprintTracker> }
	};

	private static Dictionary<int, Func<Transform, IObjectTracker>> _getTrackerInParentFuncs = new Dictionary<int, Func<Transform, IObjectTracker>>
	{
		{ 1, GetTrackerInParent<IGripTracker> },
		{ 2, GetTrackerInParent<IResourceTracker> },
		{ 3, GetTrackerInParent<ICharacterTracker> },
		{ 4, GetTrackerInParent<IBlueprintTracker> }
	};

	public static TrackingPair None = default(TrackingPair);

	[SerializeField]
	public readonly GameObject go;

	[SerializeField]
	public readonly int type;

	public bool IsValid => !go.IsNullOrReleased() && Tracker != null;

	private IObjectTracker Tracker => _getTrackerFuncs[type](go);

	public TrackingPair(GameObject go, TrackingTypes type)
	{
		this.go = go;
		this.type = (int)type;
	}

	private static IObjectTracker GetTracker<T>(GameObject go) where T : IObjectTracker
	{
		return go.GetComponent<T>();
	}

	private static T GetTrackerInParent<T>(Transform inParents) where T : class, IObjectTracker
	{
		return inParents.GetComponentInParentExcludingSelf<T>();
	}

	public static TrackingPair FindInParent(Transform inParents, TrackingTypes type)
	{
		if (type == TrackingTypes.none)
		{
			return None;
		}
		IObjectTracker objectTracker = _getTrackerInParentFuncs[(int)type](inParents);
		if (objectTracker == null || objectTracker.gameObject.IsNullOrReleased())
		{
			return None;
		}
		return new TrackingPair(objectTracker.gameObject, type);
	}

	public void Add(GameObject item)
	{
		if (IsValid)
		{
			Tracker.AddObject(item);
		}
	}

	public void UnTrack(GameObject item)
	{
		if (IsValid)
		{
			Tracker.ObjectUntracked(item);
		}
	}

	public void Track(GameObject item)
	{
		if (IsValid)
		{
			Tracker.ObjectTracked(item);
		}
	}

	public bool IsSame(TrackingPair p)
	{
		return go == p.go && type == p.type;
	}
}
