using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Input;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Network;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Beacons;

public class BeaconDriver : NetworkGameComponent
{
	public enum BaliseType
	{
		balise2,
		WorldEditorStandaloneBalise,
		MarkerBeacon
	}

	public enum BaliseMode
	{
		None,
		CreateOnStart,
		CreateOnFirstAction,
		CreateOnDebug,
		ShowInWorldEditor
	}

	public enum ShowCircleWhen
	{
		Never,
		EditingOrActive,
		Always
	}

	public struct NetworkBounds
	{
		public Vector3 center;

		public Vector3 size;

		public NetworkBounds(Vector3 c, Vector3 s)
		{
			center = c;
			size = s;
		}
	}

	public class BoundsSyncList : SyncListStruct<NetworkBounds>
	{
		public override void SerializeItem(NetworkWriter writer, NetworkBounds item)
		{
			writer.Write(item.center);
			writer.Write(item.size);
		}

		public override NetworkBounds DeserializeItem(NetworkReader reader)
		{
			return new NetworkBounds
			{
				center = reader.ReadVector3(),
				size = reader.ReadVector3()
			};
		}
	}

	public Signal<Beacon> BaliseCreated = new Signal<Beacon>();

	[HideInInspector]
	public BeaconMeta beaconMeta;

	[SerializeField]
	public Factory.AssetKey MetaAssetKey;

	[SerializeField]
	public ShowCircleWhen showCircleWhen;

	[SerializeField]
	public bool selectable;

	[SerializeField]
	private BaliseMode baliseMode;

	[SerializeField]
	private BaliseType _baliseType;

	[SerializeField]
	private GameObject baliseHolder;

	[SerializeField]
	private bool SpawnWithHandle;

	[JsonProperty]
	private string nameOverride;

	public bool SpawnOnAllClients;

	[SyncVar(hook = "NetworkPositionSet")]
	[SerializeField]
	public Vector3 NetworkPosition;

	[SerializeField]
	[SyncVar(hook = "NetworkRadiusSet")]
	public int NetworkRadius;

	public BoundsSyncList boundsSyncList = new BoundsSyncList();

	private static int kListboundsSyncList;

	public Project project => GetComponent<Project>();

	[JsonProperty]
	public Beacon Beacon { get; set; }

	public bool Showing
	{
		get
		{
			return Beacon.Visuals.gameObject.activeSelf && Beacon.GetComponent<OverheadDisplayDriver>().enabled;
		}
		set
		{
			Beacon.Visuals.gameObject.SetActive(value);
			Beacon.GetComponent<OverheadDisplayDriver>().enabled = value;
			Beacon.VisibilityChanged.Invoke(value);
		}
	}

	public Vector3 NetworkNetworkPosition
	{
		get
		{
			return NetworkPosition;
		}
		[param: In]
		set
		{
			ref Vector3 networkPosition = ref NetworkPosition;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				NetworkPositionSet(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref networkPosition, 1u);
		}
	}

	public int NetworkNetworkRadius
	{
		get
		{
			return NetworkRadius;
		}
		[param: In]
		set
		{
			ref int networkRadius = ref NetworkRadius;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				NetworkRadiusSet(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref networkRadius, 2u);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		beaconMeta = Factory.Peek(MetaAssetKey).GetComponent<BeaconMeta>();
	}

	public override void OnStartServer()
	{
		if (Beacon != null)
		{
			NetworkNetworkPosition = Beacon.transform.position;
			NetworkNetworkRadius = (int)Beacon.radius;
		}
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		PositionBalise();
		InstantiateOptionalsGO();
		InitBalise();
		if ((bool)project && base.affiliation.UserCreator == User.LocalUser.gameObject)
		{
			SoundEngine.Play(beaconMeta.sound.Created, base.gameObject);
		}
		UIGameObserver.worldEditorObjects.Selected.Connect(delegate(Transform t)
		{
			if (t == base.transform)
			{
				ShowBaliseIfNeeded();
			}
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.worldEditorObjects.Unselected.Connect(delegate(Transform t)
		{
			if (t == base.transform)
			{
				ShowBaliseIfNeeded();
			}
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	protected override void Awake()
	{
		base.Awake();
		if (baliseMode == BaliseMode.ShowInWorldEditor)
		{
			ChangeBaliseOnEditorMode(InputModeController.GetCurrentMode());
			GameSignals.Connect(InputModeController.InputModeChanged, ChangeBaliseOnEditorMode).DisconnectWhen(base.OnReleaseOrDestroySignal);
		}
		else if (baliseMode == BaliseMode.CreateOnFirstAction)
		{
			ConnectBaliseOnFirstAction();
		}
		boundsSyncList.InitializeBehaviour(this, kListboundsSyncList);
	}

	private void ConnectBaliseOnFirstAction()
	{
		GetComponentInChildren<IBlueprintTracker>()?.BlueprintAddedSignal.ConnectOnce(SpawnConstructionBalise);
	}

	private void SpawnConstructionBalise(Blueprint blueprint)
	{
		SetBaliseAt(blueprint.transform.position);
	}

	public void SetName(string name)
	{
		Beacon.SetName(name);
	}

	public string GetName()
	{
		return Beacon.name;
	}

	public bool SetMeta(Factory.AssetKey beaconMetaAssetKey)
	{
		if (MetaAssetKey == beaconMetaAssetKey)
		{
			return false;
		}
		MetaAssetKey = beaconMetaAssetKey;
		beaconMeta = Factory.Peek(MetaAssetKey).GetComponent<BeaconMeta>();
		if (!Beacon.IsNullOrReleased())
		{
			Beacon.Meta = beaconMeta;
			BaliseOverheadDisplayDriver component = Beacon.GetComponent<BaliseOverheadDisplayDriver>();
			if ((bool)component.overheadDisplay)
			{
				component.overheadDisplay.NameTag.Get.WithIcon(beaconMeta.sprite, forceRefresh: true);
			}
		}
		return true;
	}

	private void InitBalise()
	{
		if (!string.IsNullOrEmpty(nameOverride))
		{
			Beacon.name = nameOverride;
		}
		else if ((bool)beaconMeta)
		{
			Beacon.name = "Beacon -" + beaconMeta.displayName;
		}
		UIGameObserver.projects.Selected.Connect(OnProjectSelected).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.projects.Unselected.Connect(OnProjectUnselected).DisconnectWhen(base.OnReleaseOrDestroySignal);
		Beacon.Meta = beaconMeta;
		Beacon.RadiusChange.End.Connect(OnRadiusDoneChanging);
		Beacon.ClosenessChanged.Connect(ShowBaliseIfNeeded);
		Beacon.PositionChange.End.Connect(OnPositionDoneChanging);
		Beacon.BeaconNameChanged.Connect(delegate(string name)
		{
			nameOverride = name;
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		Beacon.Init();
		ShowBaliseIfNeeded();
	}

	private void OnRadiusDoneChanging(Beacon b)
	{
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAccepterRayon(base.gameObject, (int)b.radius);
		ShowBaliseIfNeeded();
	}

	private void OnPositionDoneChanging(Beacon b)
	{
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAccepterPosition(base.gameObject, b.position.x, b.position.y, b.position.z);
		ShowBaliseIfNeeded();
	}

	private void OnProjectUnselected(Project project)
	{
		ShowBaliseIfNeeded();
	}

	private void OnProjectSelected(Project project)
	{
		ShowBaliseIfNeeded();
	}

	private void InstantiateOptionalsGO()
	{
		if (SpawnWithHandle)
		{
			Beacon.InstantiateHandle();
		}
		if (showCircleWhen != ShowCircleWhen.Never)
		{
			Beacon.InstantiateCircleDrag();
		}
	}

	private void ShowBaliseIfNeeded()
	{
		if (this != null && (bool)Beacon && (bool)faction)
		{
			bool flag = false;
			flag = project == UIGameObserver.projects.CurrentSelected;
			Showing = true;
			Beacon.ShowingCircle = showCircleWhen == ShowCircleWhen.Always || (showCircleWhen == ShowCircleWhen.EditingOrActive && (Beacon.Editing || flag) && showCircleWhen != ShowCircleWhen.Never);
			Beacon.ShowingPositionDrag = (!project && InputModeController.GetCurrentMode() == InputMode.planning) || Beacon.Editing || flag;
			Showing = (InputModeController.GetCurrentMode() == InputMode.worldEditor || faction.IsOwnedByMe) && (flag || Beacon.isCloseEnough);
		}
	}

	private void CreateBaliseOnStart()
	{
		if (baliseMode == BaliseMode.CreateOnStart)
		{
			PositionBalise();
		}
	}

	private void ChangeBaliseOnEditorMode(InputMode mode)
	{
		if (mode == InputMode.worldEditor)
		{
			if (Beacon == null)
			{
				PositionBalise();
				InitBalise();
				InstantiateOptionalsGO();
			}
			if (!(Beacon != null))
			{
				return;
			}
			{
				foreach (Transform item in Beacon.transform)
				{
					item.gameObject.SetActive(value: true);
				}
				return;
			}
		}
		if (!(Beacon != null))
		{
			return;
		}
		foreach (Transform item2 in Beacon.transform)
		{
			item2.gameObject.SetActive(value: false);
		}
	}

	public void PositionBalise()
	{
		Transform parent = ((!baliseHolder) ? base.transform : baliseHolder.transform);
		SetBaliseAt(parent, NetworkPosition);
	}

	public void SetBaliseAt(Vector3 position)
	{
		Transform parent = ((!baliseHolder) ? base.transform : baliseHolder.transform);
		SetBaliseAt(parent, position);
	}

	public void SetBaliseAt(Transform parent, Vector3 position)
	{
		if (Beacon == null)
		{
			ObjectPoolSingleton.RequestComponent<Beacon>(out var t, GetBaliseAssetKey(_baliseType), position, Quaternion.identity);
			Beacon = t;
			Beacon.Meta = beaconMeta;
			Beacon.transform.SetParent(parent, worldPositionStays: true);
			Showing = faction == User.LocalUser.faction;
			BaliseCreated.Invoke(Beacon);
		}
		else
		{
			Beacon.transform.parent.SetParent(parent, worldPositionStays: true);
			if (position != Vector3.zero)
			{
				Beacon.position = XYZ.FromVector3(position);
			}
		}
	}

	private void NetworkPositionSet(Vector3 position)
	{
		NetworkNetworkPosition = position;
		SetBaliseAt(position);
	}

	private void NetworkRadiusSet(int radius)
	{
		NetworkNetworkRadius = radius;
		if (Beacon != null && !NetworkServer.active)
		{
			Beacon.radius = radius;
		}
	}

	public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
	{
		if (!SpawnOnAllClients && baliseMode == BaliseMode.ShowInWorldEditor)
		{
			return true;
		}
		foreach (User user in UNetManager.instance.UserList)
		{
			if (SpawnOnAllClients || user.faction == faction || faction == Faction.DefaultNeutral)
			{
				observers.Add(user.connectionToClient);
			}
		}
		return true;
	}

	private Factory.AssetKey GetBaliseAssetKey(BaliseType type)
	{
		return type switch
		{
			BaliseType.balise2 => Balises.DefaultBalise, 
			BaliseType.WorldEditorStandaloneBalise => Balises.SpawnPointBaliseStandalone, 
			BaliseType.MarkerBeacon => Balises.MarkerBeacon, 
			_ => Balises.DefaultBalise, 
		};
	}

	public override void ResetComponent()
	{
		if (Beacon != null)
		{
			ObjectPoolSingleton.Release(Beacon.gameObject);
		}
	}

	[LuaDrivable("SetBeaconPosition")]
	private static DynValue lua_SetBeaconPosition(ScriptExecutionContext context, CallbackArguments args)
	{
		BeaconDriver beaconDriver = args.AsComponent<BeaconDriver>(0, "SetBeaconPosition");
		Vector3 visualPosition = args.AsUserData<Vector3>(1, "SetBeaconPosition");
		beaconDriver.Beacon.SetVisualPosition(visualPosition);
		return DynValue.Void;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListboundsSyncList(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList boundsSyncList called on server.");
		}
		else
		{
			((BeaconDriver)obj).boundsSyncList.HandleMsg(reader);
		}
	}

	static BeaconDriver()
	{
		kListboundsSyncList = 194694940;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(BeaconDriver), kListboundsSyncList, InvokeSyncListboundsSyncList);
		NetworkCRC.RegisterBehaviour("BeaconDriver", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(NetworkPosition);
			writer.WritePackedUInt32((uint)NetworkRadius);
			GeneratedNetworkCode._WriteStructBoundsSyncList_BeaconDriver(writer, boundsSyncList);
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
			writer.Write(NetworkPosition);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)NetworkRadius);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			GeneratedNetworkCode._WriteStructBoundsSyncList_BeaconDriver(writer, boundsSyncList);
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
			NetworkPosition = reader.ReadVector3();
			NetworkRadius = (int)reader.ReadPackedUInt32();
			GeneratedNetworkCode._ReadStructBoundsSyncList_BeaconDriver(reader, boundsSyncList);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			NetworkPositionSet(reader.ReadVector3());
		}
		if ((num & 2) != 0)
		{
			NetworkRadiusSet((int)reader.ReadPackedUInt32());
		}
		if ((num & 4) != 0)
		{
			GeneratedNetworkCode._ReadStructBoundsSyncList_BeaconDriver(reader, boundsSyncList);
		}
	}
}
