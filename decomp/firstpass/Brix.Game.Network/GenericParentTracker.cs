using System;
using System.Runtime.InteropServices;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Network;
using Brix.UI.GameSelector;
using Brix.Utils;
using Brix.Utils.Logging;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public abstract class GenericParentTracker : NetworkGameComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(GenericParentTracker).FullName);

	[SyncVar(hook = "CurrentTrackerChanged")]
	protected TrackingPair currentTracker = TrackingPair.None;

	protected TrackingPair previousTracker = TrackingPair.None;

	public Signal<GameObject> ObjectNewlyTrackedSignal = new Signal<GameObject>();

	public Signal<GameObject> ObjectTrackedByNewParentSignal = new Signal<GameObject>();

	public Signal<GameObject> ObjectNewlyUntrackedSignal = new Signal<GameObject>();

	private NetworkIdentity networkIdentity;

	public TrackingPair NetworkcurrentTracker
	{
		get
		{
			return currentTracker;
		}
		[param: In]
		set
		{
			ref TrackingPair fieldValue = ref currentTracker;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				CurrentTrackerChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public abstract TrackingTypes TrackingType();

	public override void Initialize()
	{
		base.Initialize();
		networkIdentity = GetComponent<NetworkIdentity>();
	}

	private void CurrentTrackerChanged(TrackingPair pair)
	{
		previousTracker = currentTracker;
		NetworkcurrentTracker = pair;
		if (!base.Released)
		{
			UpdateTrackers();
		}
	}

	public override void OnStartPureClient()
	{
		UpdateTrackers();
		previousTracker = currentTracker;
	}

	public override void OnRequested()
	{
		base.OnReleaseOrDestroySignal.ConnectOnce(delegate(GameObject go, GenericParentTracker self)
		{
			self.currentTracker.UnTrack(go);
		}, this);
	}

	public bool IsTracked()
	{
		return currentTracker.IsValid;
	}

	public GameObject TrackingParent()
	{
		return currentTracker.go;
	}

	protected void ForceRefresh()
	{
		base.enabled = true;
	}

	protected virtual void OnTransformParentChanged()
	{
		base.enabled = false;
		if (!NetworkServer.active || base.gameObject.IsNullOrReleased())
		{
			return;
		}
		NetworkcurrentTracker = TrackingPair.FindInParent(base.transform, TrackingType());
		if (!(networkIdentity != null))
		{
			return;
		}
		try
		{
			networkIdentity.RebuildObservers(initialize: false);
		}
		catch
		{
		}
	}

	private void UpdateTrackers()
	{
		if (currentTracker.IsSame(previousTracker))
		{
			return;
		}
		if (previousTracker.IsValid)
		{
			UntrackOldParent(base.gameObject);
			if (!currentTracker.IsValid)
			{
				ObjectNewlyUntrackedSignal.SafeInvoke(base.gameObject);
			}
		}
		if (currentTracker.IsValid)
		{
			TrackNewParent(base.gameObject);
			if (!previousTracker.IsValid)
			{
				ObjectNewlyTrackedSignal.SafeInvoke(base.gameObject);
			}
		}
		else if (!NetworkServer.active)
		{
			ClasseMoi.Move(base.gameObject);
		}
	}

	protected virtual void OnTrackNewParent(GameObject o)
	{
		if (!UIGameSelector.HasSelectableParentGameObject(o))
		{
			UIGameSelector.Unhover(o);
			UIGameSelector.Unselect(o);
		}
	}

	private void TrackNewParent(GameObject o)
	{
		OnTrackNewParent(o);
		try
		{
			currentTracker.Track(o);
			ObjectTrackedByNewParentSignal.Invoke(currentTracker.go);
		}
		catch (ArgumentException message)
		{
			logger.Error(message);
			ClasseMoi.Move(base.gameObject);
		}
	}

	private void UntrackOldParent(GameObject o)
	{
		try
		{
			previousTracker.UnTrack(o);
			previousTracker = TrackingPair.None;
		}
		catch (ArgumentException message)
		{
			logger.Error(message);
			ClasseMoi.Move(base.gameObject);
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		previousTracker = TrackingPair.None;
		NetworkcurrentTracker = TrackingPair.None;
		ObjectNewlyTrackedSignal.Clear();
		ObjectTrackedByNewParentSignal.Clear();
		ObjectNewlyUntrackedSignal.Clear();
	}

	private void Update()
	{
		OnTransformParentChanged();
	}

	public bool IsOnGround()
	{
		return !IsTracked() && GetComponent<CatapultAmmoComponent>() == null && GetComponent<Firefly>() == null;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._WriteTrackingPair_None(writer, currentTracker);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			GeneratedNetworkCode._WriteTrackingPair_None(writer, currentTracker);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			currentTracker = GeneratedNetworkCode._ReadTrackingPair_None(reader);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			CurrentTrackerChanged(GeneratedNetworkCode._ReadTrackingPair_None(reader));
		}
	}
}
