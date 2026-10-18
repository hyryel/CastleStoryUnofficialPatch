using System.Collections.Generic;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Tracking;

public class TrackingController : UpdatableGameComponent<TrackingController>
{
	[SerializeField]
	protected Affiliation.Relation trackedRelations;

	[SerializeField]
	protected TrackingLayer trackedLayers;

	[SerializeField]
	protected float effectRange;

	[SerializeField]
	protected float refreshPerSecond = 0.5f;

	[SerializeField]
	protected TrackingEvent FullRefreshEvent;

	[SerializeField]
	protected TrackingEvent UnitsEnterEvent;

	[SerializeField]
	protected TrackingEvent UnitsExitEvent;

	private GameObject significantGO;

	private float lastRefresh;

	private readonly HashSet<Trackable> _objectsSnapshot = new HashSet<Trackable>();

	public GameObject SignificantGO
	{
		get
		{
			return significantGO ?? base.gameObject;
		}
		set
		{
			significantGO = value;
		}
	}

	public float EffectRange
	{
		get
		{
			return effectRange;
		}
		set
		{
			effectRange = value;
		}
	}

	protected override void BrixSlowUpdate()
	{
		base.BrixSlowUpdate();
		if (NetworkServer.active && Time.fixedTime > lastRefresh + refreshPerSecond)
		{
			lastRefresh = Time.fixedTime;
			FullRefresh();
		}
	}

	protected void FullRefresh()
	{
		if (!(this == null) && trackedRelations != Affiliation.Relation.None)
		{
			SetDirtyFlagForOldTrackedObjects();
			HashSet<Trackable> hashSet = CollectionPool.RequestHashSet<Trackable>();
			FindTrackedObjectsAndAddActiveFlag(hashSet);
			HashSet<Trackable> hashSet2 = CollectionPool.RequestHashSet<Trackable>();
			FindUntrackedObjectsAndRemoveFlags(hashSet2);
			SendTrackingEvents(hashSet, hashSet2);
			hashSet.Dispose();
			hashSet2.Dispose();
		}
	}

	private void SetDirtyFlagForOldTrackedObjects()
	{
		foreach (Trackable item in _objectsSnapshot)
		{
			item.TrackingBit = 1;
		}
	}

	private void FindTrackedObjectsAndAddActiveFlag(HashSet<Trackable> added)
	{
		List<Faction> list = CollectionPool.RequestList<Faction>();
		Faction.GetAllFactions().Slinq().Where((Faction f, TrackingController self) => self.trackedRelations.HasFlag(Affiliation.GetRelation(self.SignificantGO, f.gameObject)), this)
			.AddTo(list);
		foreach (TrackingRegistry.TrackingEntry collision in TrackingRegistry.GetCollisions(base.transform.position, effectRange, trackedLayers, list))
		{
			Trackable trackable = collision.gameComponent as Trackable;
			if (!trackable.IsNullOrReleased() && trackable.TrackingBit != 2)
			{
				if (trackable.TrackingBit == 0)
				{
					added.Add(trackable);
					_objectsSnapshot.Add(trackable);
				}
				trackable.TrackingBit = 2;
			}
		}
		list.Dispose();
	}

	private void FindUntrackedObjectsAndRemoveFlags(HashSet<Trackable> removed)
	{
		List<Trackable> list = CollectionPool.RequestList<Trackable>();
		foreach (Trackable item in _objectsSnapshot)
		{
			if (item.TrackingBit == 1)
			{
				list.Add(item);
			}
			item.TrackingBit = 0;
		}
		foreach (Trackable item2 in list)
		{
			_objectsSnapshot.Remove(item2);
			if (!item2.IsNullOrReleased())
			{
				removed.Add(item2);
			}
		}
		list.Dispose();
	}

	private void SendTrackingEvents(HashSet<Trackable> added, HashSet<Trackable> removed)
	{
		if (FullRefreshEvent != null)
		{
			FullRefreshEvent.Invoke(_objectsSnapshot);
		}
		if (UnitsEnterEvent != null && added.Count > 0)
		{
			UnitsEnterEvent.Invoke(added);
		}
		if (UnitsExitEvent != null && removed.Count > 0)
		{
			UnitsExitEvent.Invoke(removed);
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (UnitsExitEvent != null && _objectsSnapshot.Count > 0)
		{
			HashSet<Trackable> hashSet = (from o in _objectsSnapshot.Slinq()
				where !o.IsNullOrReleased()
				select o).ToPooledHashSet();
			_objectsSnapshot.Clear();
			UnitsExitEvent.Invoke(hashSet);
			hashSet.Dispose();
		}
		significantGO = null;
	}
}
