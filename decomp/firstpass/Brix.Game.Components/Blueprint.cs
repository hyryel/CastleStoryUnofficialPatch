using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.IO.Serialization;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Network;
using Brix.Transactions;
using Brix.Utils;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

[JsonObject(MemberSerialization.OptIn)]
public class Blueprint : NetworkGameComponent
{
	public static GameSignals.GameSignal<Blueprint> BlueprintCreated = GameSignals.CreateStaticSignal<Blueprint>();

	public static GameSignals.GameSignal<Blueprint> BlueprintPlaced = GameSignals.CreateStaticSignal<Blueprint>();

	public static GameSignals.GameSignal<Blueprint> BlueprintDeleted = GameSignals.CreateStaticSignal<Blueprint>();

	public Factory.AssetKey BlueprintResult;

	public Signal<Blueprint, PlacementFlags, PlacementFlags> OnStateChangedSignal = new Signal<Blueprint, PlacementFlags, PlacementFlags>();

	public bool Stackable = true;

	[HideInInspector]
	[SyncVar(hook = "OnAntiChange")]
	[JsonProperty]
	public bool _isAnti;

	[SyncVar(hook = "OnCommit")]
	[JsonProperty]
	public bool commited;

	private StateRenderer _stateRenderer;

	private BlueprintProjectParentTracker _parentTracker;

	private Project _project;

	private PlacementFlags _state;

	public SignalConnection VisibilitySignalConnection;

	private ValidationData _pretendConcrete;

	private static HashSet<Factory.AssetKey> _plankPairKeys = new HashSet<Factory.AssetKey>
	{
		Blueprints.Wood_SinglePlank,
		Blueprints.Wood_Ramp_TopA,
		Blueprints.Wood_Ramp_TopB,
		Blueprints.Wood_Ramp_BotA,
		Blueprints.Wood_Ramp_BotB,
		Blueprints.Wood_Ramp_MidA,
		Blueprints.Wood_Ramp_MidB,
		Blueprints.Wood_Ramp_JunctionA,
		Blueprints.Wood_Ramp_JunctionB,
		Blueprints.Wood_Ramp_JunctionC,
		Blueprints.Wood_Ramp_JunctionD,
		Blueprints.Wood_Ramp_JunctionE,
		Blueprints.Wood_Ramp_JunctionF
	};

	public IReadOnlySignal<GameObject> SelfBlueprintDeleted => base.OnReleaseOrDestroySignal;

	public bool IsAnti => State.HasFlag(PlacementFlags.anti);

	[LuaDrivable("isPlaced")]
	public bool isPlaced => State.HasFlag(PlacementFlags.placed);

	public Project Project
	{
		get
		{
			if (_parentTracker == null || !_parentTracker.IsTracked())
			{
				_project = null;
			}
			else if (_project.IsNullOrReleased() || _parentTracker.TrackingParent() != _project.gameObject)
			{
				_project = _parentTracker.TrackingParent().GetComponent<Project>();
			}
			return _project;
		}
	}

	public PlacementFlags State
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				PlacementFlags state = _state;
				_state = value;
				StateChange(state, value);
			}
		}
	}

	public bool Network_isAnti
	{
		get
		{
			return _isAnti;
		}
		[param: In]
		set
		{
			ref bool isAnti = ref _isAnti;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnAntiChange(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref isAnti, 1u);
		}
	}

	public bool Networkcommited
	{
		get
		{
			return commited;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref commited;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnCommit(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	private void OnCommit(bool value)
	{
		Networkcommited = value;
		if (commited)
		{
			State |= PlacementFlags.commited;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		_stateRenderer = GetComponent<StateRenderer>();
		_parentTracker = GetComponent<BlueprintProjectParentTracker>();
		_state = PlacementFlags.valid;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		GameSignals.Invoke(BlueprintCreated, this);
		if (base.recepteur != null)
		{
			base.recepteur.InventoryChanged.Connect(OnReceivedNewItem);
		}
		if (!State.HasFlag(PlacementFlags.placed))
		{
			SerializationComponent component = GetComponent<SerializationComponent>();
			if (component != null)
			{
				component.Unsubscribe();
			}
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		GameSignals.Invoke(BlueprintDeleted, this);
		_state = PlacementFlags.valid;
	}

	public override void OnStartPureClient()
	{
		if (State.HasFlag(PlacementFlags.placed))
		{
			GameSignals.Invoke(BlueprintPlaced, this);
		}
	}

	public override void OnStartAnywhere()
	{
		if (_isAnti && base.damageReceiver == null)
		{
			TurnToAntiBlueprint();
		}
	}

	public void RefreshVisual()
	{
		_stateRenderer.RefreshVisual();
	}

	private void StateChange(PlacementFlags previousMode, PlacementFlags newMode)
	{
		if (((previousMode ^ newMode) & PlacementFlags.placed) != PlacementFlags.none && base.recepteur != null)
		{
			bool flag = newMode.HasFlag(PlacementFlags.placed);
			if (flag)
			{
				GameSignals.Invoke(BlueprintPlaced, this);
			}
			SerializationComponent component = GetComponent<SerializationComponent>();
			if (component != null)
			{
				if (flag)
				{
					component.Subscribe();
				}
				else
				{
					component.Unsubscribe();
				}
			}
		}
		Project project = Project;
		if (project != null)
		{
			BuildGoalProvider component2 = project.GetComponent<BuildGoalProvider>();
			component2.OnBlueprintChangedState(this, previousMode, newMode);
		}
		OnStateChangedSignal.Invoke(this, previousMode, newMode);
	}

	private void OnAntiChange(bool isAntiNewValue)
	{
		if (_isAnti != isAntiNewValue)
		{
			Network_isAnti = isAntiNewValue;
			if (isAntiNewValue)
			{
				TurnToAntiBlueprint();
			}
			else
			{
				TurnToNormalBlueprint();
			}
		}
	}

	private void TurnToNormalBlueprint()
	{
		Object.Destroy(base.gameObject.GetComponent<AdaptativeDamageReceiver>());
		State &= ~PlacementFlags.anti;
	}

	private void TurnToAntiBlueprint()
	{
		AdaptativeDamageReceiver orAddComponent = base.gameObject.GetOrAddComponent<AdaptativeDamageReceiver>();
		orAddComponent.SetProxiedDamageReceiver(GetTargetedDamageReceiver());
		State |= PlacementFlags.anti;
	}

	private IDamageReceiver GetTargetedDamageReceiver()
	{
		BaseVoxelTransform component = GetComponent<BaseVoxelTransform>();
		IDamageReceiver get = NullDamageReceiver.Get;
		Volume volume = VolumeRegistry.VolumesWithPropertyAt(component.position, VProperty.occupy).OfType<Volume>().FirstOrDefault();
		if (volume != null)
		{
			if (volume.damageReceiver != null)
			{
				return volume.damageReceiver;
			}
			return DieOnDamage_DamageReceiver.Make(volume.gameObject);
		}
		using IVoxel voxel = Voxel.GetVoxel(component.position);
		if (!voxel.IsNull)
		{
			return voxel.DamageReceiver;
		}
		return get;
	}

	public void ChangeProject(Project project)
	{
		base.transform.SetParent(project.transform);
	}

	public void OnReceivedNewItem()
	{
		if (NetworkServer.active && !base.Released && base.recepteur.IsFull())
		{
			Blueprint blueprint = FindPairedBlueprint();
			if (blueprint != null)
			{
				blueprint.MorphToObject();
			}
			MorphToObject();
		}
	}

	[ContextMenu("Morph To Object")]
	public GameObject MorphToObject()
	{
		GameObject gameObject = null;
		if (!IsAnti)
		{
			gameObject = Transactor.SpawnUNet(BlueprintResult, faction, base.transform.position, base.transform.rotation);
			RopeBridgeConfigurator component = gameObject.GetComponent<RopeBridgeConfigurator>();
			if (component != null)
			{
				RopeBridgeConfigurator component2 = GetComponent<RopeBridgeConfigurator>();
				component.SetEndPoints(component2.StartPoint, component2.EndPoint);
				component.CreateVolume(isBlueprint: false);
				component.InitRopes();
				component.CreatePlankColliders();
			}
		}
		else
		{
			IVolume component3 = GetComponent<IVolume>();
			bool flag = false;
			List<IVolume> list = VolumeRegistry.VolumesWithPropertyAt(component3.blueprint, VProperty.occupy).ToList();
			foreach (IVolume item in list)
			{
				flag = true;
				if (item.GameObject != null)
				{
					ObjectPoolSingleton.Release(item.GameObject);
				}
			}
			if (!flag)
			{
				(from i in component3.blueprint.Slinq()
					select Voxel.GetVoxel(i).RootOrSelf).Distinct().ForEach(delegate(IVoxel ibd)
				{
					ibd.DeBlock(ibd.BlockInfo.ID);
				});
			}
		}
		if ((bool)base.recepteur)
		{
		}
		base.gameObject.SafeNetworkDestroy();
		return gameObject;
	}

	private IEnumerable<ValidationData> PretendConcrete()
	{
		yield return _pretendConcrete;
	}

	private Blueprint FindPairedBlueprint()
	{
		Factory.AssetKey assetKey = GetComponent<AssetKeyHolder>().AssetKey;
		Blueprint result = null;
		List<IVolume> list = VUtil.ImpactedVolumes<IVolume>(base.volume);
		_pretendConcrete = FreeVolume.Make(BlueprintResult, base.voxel.position, base.voxel.RotationIndex).VData;
		VUtil.NewVID();
		if (assetKey == Blueprints.Wood_SingleLog || assetKey == Blueprints.Wood_VerticalBeam)
		{
			result = (from b in list.Slinq().Select(IVolumeExtender.GetComponent<Blueprint>)
				where b != null
				where b.GetComponent<ValidatorComponent>().GetCurrentState(PretendConcrete()) == ValidationResult.Buildable
				select b).FirstOrDefault((Blueprint b) => b.AssetKey == assetKey);
		}
		else if (assetKey == Blueprints.Wood_Beam)
		{
			result = (from b in list.Slinq().Where((IVolume v, Factory.AssetKey ak) => v.AssetKey == ak, assetKey).Where((IVolume v, IVolume vol) => VolumeMatch(vol, v), base.volume)
					.Select(IVolumeExtender.GetComponent<Blueprint>)
				where b != null
				where b.GetComponent<ValidatorComponent>().GetCurrentState(PretendConcrete()) == ValidationResult.Buildable
				select b).FirstOrDefault();
		}
		else if (_plankPairKeys.Contains(assetKey))
		{
			result = (from b in (from v in list.Slinq()
					where _plankPairKeys.Contains(v.AssetKey)
					select v).Where((IVolume v, IVolume vol) => VolumeMatch(vol, v), base.volume).Select(IVolumeExtender.GetComponent<Blueprint>)
				where b != null
				where b.GetComponent<ValidatorComponent>().GetCurrentState(PretendConcrete()) == ValidationResult.Buildable
				select b).FirstOrDefault();
		}
		list.Dispose();
		return result;
	}

	private bool VolumeMatch(IVolume current, IVolume other)
	{
		return current.HasPropertyInAny(VProperty.mainSupportReq, other.GetCases(VProperty.fantome)) && other.HasPropertyInAny(VProperty.mainSupportReq, current.GetCases(VProperty.fantome));
	}

	public void SetColliderActive(bool active)
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			collider.enabled = active;
		}
	}

	public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
	{
		foreach (User user in UNetManager.instance.UserList)
		{
			if (user.faction == faction)
			{
				observers.Add(user.connectionToClient);
			}
		}
		return true;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_isAnti);
			writer.Write(commited);
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
			writer.Write(_isAnti);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(commited);
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
			_isAnti = reader.ReadBoolean();
			commited = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			OnAntiChange(reader.ReadBoolean());
		}
		if ((num & 2) != 0)
		{
			OnCommit(reader.ReadBoolean());
		}
	}
}
