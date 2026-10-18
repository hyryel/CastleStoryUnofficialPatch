using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Components.Tracking;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.Network;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Signals;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Wards;

public class WardLink : NetworkGameComponent
{
	public enum LinkDirection
	{
		None,
		In,
		Out,
		InOut
	}

	[HideInInspector]
	public LineRenderer LinkLineRenderer;

	public LinkDirection Direction;

	public Material LineMaterial;

	public const float LinkMaxRange = 20f;

	public bool DoNotAutoConnect;

	private RecepteurParentTracker parentTracker;

	[SyncVar(hook = "linkedParentChanged")]
	private GameObject linkedParent;

	private HashSet<WardLink> linkedChildrens = new HashSet<WardLink>();

	private WardController wardController;

	public bool Connected;

	private static bool linkVisibles;

	private const float UnfocusedWidth = 0.1f;

	private const float FocusedWidth = 0.2f;

	private HashSet<GameObject> wardLinksSnapshots = new HashSet<GameObject>();

	private static Signal LinkVisibilityChanged = new Signal();

	private NetworkInstanceId ___linkedParentNetId;

	public static bool LinkVisibles => linkVisibles;

	public GameObject NetworklinkedParent
	{
		get
		{
			return linkedParent;
		}
		[param: In]
		set
		{
			ref GameObject gameObjectField = ref linkedParent;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				linkedParentChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVarGameObject(value, ref gameObjectField, 1u, ref ___linkedParentNetId);
		}
	}

	private void linkedParentChanged(GameObject parent)
	{
		NetworklinkedParent = parent;
		if (parent == null)
		{
			_getDisconnect();
		}
		else
		{
			_getConnect(parent.GetComponent<WardLink>());
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		wardController = GetComponent<WardController>();
		parentTracker = GetComponent<RecepteurParentTracker>();
	}

	public override void OnStartPureClient()
	{
		base.OnStartPureClient();
		if (linkedParent != null)
		{
			linkedParentChanged(linkedParent);
		}
	}

	public override void OnRequested()
	{
		base.OnRequested();
		if ((bool)wardController)
		{
			wardController.enabled = false;
		}
		InstantiateLineRenderer();
		RequestFirstConnection();
		ConnectToSignals();
	}

	private void RequestFirstConnection()
	{
		Neo.Relais(delegate
		{
			FindAndTryToGetConnectedByNearbyParent();
			if ((bool)wardController)
			{
				wardController.ForceInitialActivationEvent(wardController.enabled);
			}
		}, "ward initial linking");
	}

	private void RequestReconnect()
	{
		if (!Connected)
		{
			Neo.Relais(FindAndTryToGetConnectedByNearbyParent, "ward linking");
		}
	}

	private void OnLeaveInventory(GameObject parent)
	{
		DoNotAutoConnect = false;
		RequestReconnect();
	}

	private void OnEnterInventory(GameObject parent)
	{
		DoNotAutoConnect = true;
		NetworklinkedParent = null;
	}

	private void ConnectToSignals()
	{
		if ((bool)parentTracker)
		{
			parentTracker.ObjectNewlyTrackedSignal.Connect(OnEnterInventory);
			parentTracker.ObjectNewlyUntrackedSignal.Connect(OnLeaveInventory);
		}
		base.affiliation.OnFactionChanged.Connect(delegate
		{
			AffiliationChanged();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		LinkVisibilityChanged.Connect(delegate
		{
			LineMaterial.SetFloat("_Gain", (!linkVisibles) ? 0.1f : 0.2f);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		DisconnectChildrens();
		if ((bool)linkedParent)
		{
			linkedParent.GetComponent<WardLink>().TryRemoveLink(this);
		}
	}

	private void AffiliationChanged()
	{
		DisconnectChildrens();
		Neo.Relais(FindAndTryToConnectWithNearbyChildrens, "ward linking");
	}

	public void FindAndTryToGetConnectedByNearbyParent()
	{
		if (!(this == null) && NetworkServer.active && !DoNotAutoConnect)
		{
			WardLink wardLink = FindClosestOutputLink();
			if (!wardLink.IsNullOrReleased())
			{
				wardLink.TryCreateLink(this);
			}
		}
	}

	public void FindAndTryToConnectWithNearbyChildrens()
	{
		if (!(this == null) && NetworkServer.active && !DoNotAutoConnect)
		{
			GetAllInputInRange().ForEach(TryCreateLink);
		}
	}

	public WardLink FindClosestOutputLink(bool includeBlueprints = false)
	{
		TrackingLayer layer = ((!includeBlueprints) ? TrackingLayer.WardLink : (TrackingLayer.WardLink | TrackingLayer.WardLinkBlueprint));
		return (from t in TrackingRegistry.GetCollisions(base.transform.position, 20f, layer, faction.AsList).Slinq()
			select t.GameObject into o
			where !o.IsNullOrReleased()
			select o.GetComponent<WardLink>() into o
			orderby Vector3.Distance(o.transform.position, base.transform.position)
			orderby !o.GetComponent<WardLink>().Connected
			select o).FirstOrDefault((WardLink o) => o.IsValidOutput(this));
	}

	public Slinq<GameObject, PredicateContext<GameObject, SelectContext<GameObject, TrackingRegistry.TrackingEntry, CollisionEnumerationRequestSlinqContext>>> GetAllInputInRange(bool includeBlueprints = false)
	{
		TrackingLayer layer = ((!includeBlueprints) ? TrackingLayer.WardLink : (TrackingLayer.WardLink | TrackingLayer.WardLinkBlueprint));
		return from t in TrackingRegistry.GetCollisions(base.transform.position, 20f, layer, faction.AsList).Slinq()
			select t.GameObject into o
			where !o.IsNullOrReleased() && IsValidInput(o.GetComponent<WardLink>())
			select o;
	}

	private bool IsValidInput(WardLink o)
	{
		return Vector3.Distance(base.transform.position, o.transform.position) <= 20f && o.Direction.HasFlag(LinkDirection.In) && !o.Connected;
	}

	private bool IsValidOutput(WardLink o)
	{
		return Vector3.Distance(base.transform.position, o.transform.position) <= 20f && Direction.HasFlag(LinkDirection.Out);
	}

	public void DisconnectChildrens()
	{
		linkedChildrens.Slinq().ForEach(delegate(WardLink linked)
		{
			linked._getDisconnect();
		});
		linkedChildrens.Clear();
	}

	public void TryCreateLink(GameObject o)
	{
		WardLink component = o.GetComponent<WardLink>();
		if ((bool)component)
		{
			TryCreateLink(component);
		}
	}

	public void TryCreateLink(WardLink o)
	{
		if (Connected && Vector3.Distance(base.transform.position, o.transform.position) <= 20f && Direction.HasFlag(LinkDirection.Out) && !o.Connected && o.Direction.HasFlag(LinkDirection.In))
		{
			_addChildren(o);
		}
	}

	public void TryRemoveLink(WardLink o)
	{
		if (linkedChildrens.Contains(o))
		{
			_removeChildren(o);
		}
	}

	private void _refreshConnections()
	{
		if (Connected)
		{
			Neo.Relais(FindAndTryToConnectWithNearbyChildrens, "ward linking");
			return;
		}
		DisconnectChildrens();
		Neo.Relais(FindAndTryToGetConnectedByNearbyParent, "ward linking");
	}

	private void _addChildren(WardLink o)
	{
		linkedChildrens.Add(o);
		o.NetworklinkedParent = base.gameObject;
	}

	private void _removeChildren(WardLink o)
	{
		linkedChildrens.Remove(o);
		o.NetworklinkedParent = null;
	}

	private void _getConnect(WardLink source)
	{
		Connected = true;
		_refreshConnections();
		NetworklinkedParent = source.gameObject;
		if ((bool)wardController)
		{
			wardController.enabled = true;
		}
		if ((bool)LinkLineRenderer && Affiliation.IsOwnedBy(base.gameObject, User.LocalUser))
		{
			LinkLineRenderer.useWorldSpace = true;
			LinkLineRenderer.startColor = Color.white;
			LinkLineRenderer.endColor = Color.white;
			LinkLineRenderer.SetPositions(new Vector3[2]
			{
				base.transform.position,
				source.transform.position
			});
			LineMaterial.SetFloat("_Gain", (!linkVisibles) ? 0.1f : 0.2f);
		}
	}

	private void _getDisconnect()
	{
		if (!Architecte.termine)
		{
			Connected = false;
			_refreshConnections();
			NetworklinkedParent = null;
			if ((bool)wardController)
			{
				wardController.enabled = false;
			}
			if ((bool)LinkLineRenderer && Affiliation.IsOwnedBy(base.gameObject, User.LocalUser))
			{
				LinkLineRenderer.SetPositions(new Vector3[2]
				{
					Vector3.zero,
					Vector3.zero
				});
			}
		}
	}

	private void InstantiateLineRenderer()
	{
		GameObject gameObject = new GameObject("WardLinkLineRenderer");
		gameObject.transform.SetParent(base.transform);
		LinkLineRenderer = gameObject.AddComponent<LineRenderer>();
		LinkLineRenderer.transform.SetParent(gameObject.transform);
		LinkLineRenderer.material = LineMaterial;
		LinkLineRenderer.startWidth = 0.1f;
		LinkLineRenderer.endWidth = 0.1f;
		LinkLineRenderer.SetPositions(new Vector3[2]
		{
			Vector3.zero,
			Vector3.zero
		});
	}

	public void PreviewConnections()
	{
		Color color = Color.yellow;
		WardLink wardLink = FindClosestOutputLink(includeBlueprints: true);
		if ((bool)wardLink)
		{
			color = ((!wardLink.Connected) ? Color.yellow : Color.blue);
			LinkLineRenderer.startColor = color;
			LinkLineRenderer.endColor = color;
			LinkLineRenderer.SetPositions(new Vector3[2]
			{
				base.transform.position,
				wardLink.transform.position
			});
		}
		else
		{
			LinkLineRenderer.SetPositions(new Vector3[2]
			{
				Vector3.zero,
				Vector3.zero
			});
		}
		if (Direction.HasFlag(LinkDirection.Out))
		{
			HashSet<GameObject> hashSet = GetAllInputInRange(includeBlueprints: true).ToPooledHashSet();
			StopConnectionsPreview();
			hashSet.Slinq().ForEach(delegate(GameObject o)
			{
				o.GetComponent<WardLink>().LinkLineRenderer.startColor = color;
				o.GetComponent<WardLink>().LinkLineRenderer.endColor = color;
				o.GetComponent<WardLink>().LinkLineRenderer.SetPositions(new Vector3[2]
				{
					base.transform.position,
					o.transform.position
				});
			});
			wardLinksSnapshots.Dispose();
			wardLinksSnapshots = hashSet;
		}
	}

	public void StopConnectionsPreview()
	{
		(from o in wardLinksSnapshots.Slinq()
			where !o.IsNullOrReleased() && !o.GetComponent<WardLink>().Connected
			select o).ForEach(delegate(GameObject o)
		{
			o.GetComponent<WardLink>().LinkLineRenderer.startColor = Color.blue;
			o.GetComponent<WardLink>().LinkLineRenderer.endColor = Color.blue;
			o.GetComponent<WardLink>().LinkLineRenderer.SetPositions(new Vector3[2]
			{
				Vector3.zero,
				Vector3.zero
			});
		});
	}

	public static void ShowLinks()
	{
		linkVisibles = true;
		LinkVisibilityChanged.Invoke();
	}

	public static void HideLinks()
	{
		linkVisibles = false;
		LinkVisibilityChanged.Invoke();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(linkedParent);
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
			writer.Write(linkedParent);
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
			___linkedParentNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			linkedParentChanged(reader.ReadGameObject());
		}
	}

	public override void PreStartClient()
	{
		if (!___linkedParentNetId.IsEmpty())
		{
			NetworklinkedParent = ClientScene.FindLocalObject(___linkedParentNetId);
		}
	}
}
