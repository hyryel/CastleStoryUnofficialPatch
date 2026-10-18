using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Components.Tracking;
using Brix.Game.Network;
using Brix.Game.Rules;
using Brix.Lua;
using Brix.Network;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class LocalFactionVision : UpdatableNetworkGameComponent
{
	protected static BrixLogger logger = new BrixLogger(typeof(LocalFactionVision).FullName);

	private static readonly UpdatableNetworkProfiler<LocalFactionVision> UpdatableProfiler = new UpdatableNetworkProfiler<LocalFactionVision>();

	public static GameSignals.GameSignal<LocalFactionVision> AddedFogObject = new GameSignals.GameSignal<LocalFactionVision>();

	public static GameSignals.GameSignal<LocalFactionVision> RemovedFogObject = new GameSignals.GameSignal<LocalFactionVision>();

	private Signal _enteredFog;

	private Signal _leftFog;

	public static bool AllVisible;

	public bool AlwaysVisible;

	public bool AlwaysInvisibleForNonAllied;

	public int LineOfSightRange;

	public int MaxInvisibilityRange;

	public bool UseGameRuleLineOfSight;

	public bool IgnoreParentVision;

	public int CollisionTrackingId;

	public UnityEvent UpdateStoredGameObjectVision;

	public SmallSubset_8from8 FactionsSeeingMe = default(SmallSubset_8from8);

	private NetworkIdentity networkIdentity;

	public bool DrawVisionRange;

	private bool _lastHostVisibility;

	[LuaDrivable("hk_OnEnteredFog")]
	private static LuaHook<LuaDriver> lua_enteredFog = new LuaHook<LuaDriver>();

	[LuaDrivable("hk_OnLeftFog")]
	private static LuaHook<LuaDriver> lua_leftFog = new LuaHook<LuaDriver>();

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	public Signal OnEnterFogSignal => _enteredFog ?? (_enteredFog = new Signal());

	public Signal OnExitFogSignal => _leftFog ?? (_leftFog = new Signal());

	public IReadOnlySignal<GameObject> SelfRemovedFogObject => base.OnReleaseOrDestroySignal;

	public IReadOnlySignal EnteredFog => OnEnterFogSignal;

	public IReadOnlySignal LeftFog => OnExitFogSignal;

	protected override float SlowUpdateRate => 0.2f;

	public static void ToggleFogSystem(bool value)
	{
		AllVisible = !value;
		UIGameObserver.fogObjects.CheckCreated().Slinq().ForEach(delegate(LocalFactionVision self, bool enabled)
		{
			self.UpdateHostVisibility(enabled);
		}, value);
	}

	public bool IsVisibleForMe()
	{
		return IsVisibleForFaction(User.LocalUser.faction);
	}

	public static bool IsEnabled()
	{
		return !AllVisible;
	}

	public override void Initialize()
	{
		base.Initialize();
		networkIdentity = GetComponent<NetworkIdentity>();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		LineOfSightRange = ((!UseGameRuleLineOfSight) ? LineOfSightRange : GeneralRules.Main.FogOfWarDistance);
		base.enabled = IsEnabled();
		GameSignals.Invoke(AddedFogObject, this);
		faction.SharedVision.Add(this);
	}

	private void OnDrawGizmosSelected()
	{
		if (!DrawVisionRange)
		{
			return;
		}
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		TrackingRegistry.GetRelevantXYZsInRange(list, XYZ.FromVector3(base.transform.position), 50, includeEmpty: true);
		foreach (XYZ item in list)
		{
			Vector3 vector = (item * 16).ToVector3();
			Gizmos.DrawCube(vector + new Vector3(16f, 16f, 16f) / 2f, new Vector3(16f, 16f, 16f));
		}
		list.Dispose();
	}

	protected virtual void OnTransformParentChanged()
	{
		if (NetworkServer.active && base.Requested)
		{
			UpdateVisibility();
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (_enteredFog != null)
		{
			_enteredFog.Clear();
		}
		if (_leftFog != null)
		{
			_leftFog.Clear();
		}
		GameSignals.Invoke(RemovedFogObject, this);
		Faction faction = this.faction;
		if (faction != null && faction.SharedVision != null)
		{
			faction.SharedVision.Remove(this);
		}
	}

	public static bool ObjectIsVisibleForFaction(GameObject go, Faction faction, bool includeAllies = true)
	{
		if (go.IsNullOrReleased())
		{
			return false;
		}
		GameComponent component = go.GetComponent<GameComponent>();
		return component != null && ObjectIsVisibleForFaction(component, faction, includeAllies);
	}

	public static bool ObjectIsVisibleForFaction(IGameComponent gc, Faction faction, bool includeAllies = true)
	{
		LocalFactionVision localFactionVision = gc.vision;
		if (localFactionVision.IsNullOrReleased())
		{
			return true;
		}
		if (faction.IsNullOrReleased())
		{
			return false;
		}
		return localFactionVision.IsVisibleForFaction(faction, includeAllies);
	}

	public static bool IsResponsibleForVision(LocalFactionVision other)
	{
		if (other.IgnoreParentVision)
		{
			return true;
		}
		GenericParentTracker parentTracker = other.ParentTracker;
		if (parentTracker.IsNullOrReleased())
		{
			return true;
		}
		if (parentTracker is CharacterParentTracker)
		{
			return true;
		}
		return !parentTracker.IsTracked();
	}

	public static void OnUnitEnterSightRange(Faction faction, LocalFactionVision unit)
	{
		unit.UpdateVisibility();
	}

	public static void OnUnitLeaveSightRange(Faction faction, LocalFactionVision unit)
	{
		unit.UpdateVisibility();
	}

	public void TemporaryRevealForFaction(Faction unitFaction, float duration)
	{
		OnUnitEnterSightRange(unitFaction, this);
		Neo.Relais(delegate
		{
			if (unitFaction != null && !this.IsNullOrReleased())
			{
				OnUnitLeaveSightRange(unitFaction, this);
			}
		}, duration, "update visibility");
	}

	public bool IsVisibleForFaction(Faction faction, bool includeAllies = true)
	{
		if (this == null || faction == null)
		{
			logger.Warn("IsVisibleForFaction was called on null object or null faction");
			return false;
		}
		if (!NetworkServer.active && faction == User.LocalUser.faction)
		{
			return true;
		}
		if ((AllVisible || AlwaysVisible) && !AlwaysInvisibleForNonAllied)
		{
			return true;
		}
		if (AssetKey == BuildResources.Pumpkin || AssetKey == BuildResources.Crystal)
		{
			GenericParentTracker component = GetComponent<GenericParentTracker>();
			if (component == null || !component.IsTracked())
			{
				return true;
			}
		}
		if (faction.HasGlobalFogVision && !AlwaysInvisibleForNonAllied)
		{
			return true;
		}
		if (!IgnoreParentVision && IsTrackedParentVisibleFor(faction))
		{
			return true;
		}
		if (Affiliation.IsOwnedBy(base.gameObject, faction.gameObject) || (includeAllies && Affiliation.IsAllied(base.gameObject, faction.gameObject)))
		{
			return true;
		}
		if (AlwaysInvisibleForNonAllied)
		{
			return false;
		}
		if (FactionsSeeingMe.Contains(faction.ID))
		{
			return true;
		}
		return includeAllies && GetAlliedVision(faction);
	}

	private bool IsTrackedParentVisibleFor(Faction faction)
	{
		GenericParentTracker component = GetComponent<GenericParentTracker>();
		if (component != null && component.IsTracked())
		{
			LocalFactionVision component2 = component.TrackingParent().GetComponent<LocalFactionVision>();
			if (component2 != null && component2.IsVisibleForFaction(faction))
			{
				return true;
			}
		}
		return false;
	}

	private bool GetAlliedVision(Faction otherFaction)
	{
		return otherFaction.AllyFactions.Slinq().Any((Faction f, LocalFactionVision self) => self.FactionsSeeingMe.Contains(f.ID), this);
	}

	public void UpdateVisibility()
	{
		if (!(this == null))
		{
			UNetAnimator component = GetComponent<UNetAnimator>();
			if ((bool)component && (bool)component.animator)
			{
				component.NetworkserverAnimatorCarryModeOnSpawn = component.animator.GetFloat("CarryMode");
				component.NetworkserverAnimatorStateOnSpawn = component.animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
			}
			if ((bool)networkIdentity && !networkIdentity.netId.IsEmpty())
			{
				networkIdentity.RebuildObservers(initialize: false);
			}
			UpdateStoredGameObjectVision.Invoke();
		}
	}

	public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
	{
		if (UNetManager.instance.debugLoader)
		{
			UpdateHostVisibility(forced: false);
			return base.OnRebuildObservers(observers, initial);
		}
		foreach (User user in UNetManager.instance.UserList)
		{
			if (user.isLocalPlayer || IsVisibleForFaction(user.faction))
			{
				observers.Add(user.connectionToClient);
			}
		}
		UpdateHostVisibility(initial);
		return true;
	}

	private void UpdateHostVisibility(bool forced)
	{
		if (this == null)
		{
			return;
		}
		bool flag = IsVisibleForFaction(User.LocalUser.faction);
		if (_lastHostVisibility == flag && !forced)
		{
			return;
		}
		SignalHandler component = GetComponent<SignalHandler>();
		if ((bool)component)
		{
			if (!flag)
			{
				component.EnterFog.Invoke();
				OnEnterFogSignal.Invoke();
				LuaDriver component2 = GetComponent<LuaDriver>();
				if (component2 != null)
				{
					LuaHookBase.InvokeFromCLR(lua_enteredFog, component2);
				}
			}
			else
			{
				component.ExitFog.Invoke();
				OnExitFogSignal.Invoke();
				LuaDriver component3 = GetComponent<LuaDriver>();
				if (component3 != null)
				{
					LuaHookBase.InvokeFromCLR(lua_leftFog, component3);
				}
			}
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!(renderer is LineRenderer))
			{
				renderer.enabled = flag;
			}
		}
		_lastHostVisibility = flag;
	}

	public static void RefreshAllObservers()
	{
		TrackingRegistry.GetAllTrackedEntries().Slinq().ForEach(delegate(TrackingRegistry.TrackingEntry t)
		{
			t.Vision.GetComponent<NetworkIdentity>().RebuildObservers(initialize: false);
		});
	}

	[LuaDrivable("IsVisible")]
	private static DynValue lua_IsVisibleForMe(ScriptExecutionContext context, CallbackArguments args)
	{
		LocalFactionVision localFactionVision = args.AsComponent<LocalFactionVision>(0, "IsVisible");
		return DynValue.NewBoolean(localFactionVision.IsVisibleForFaction(User.LocalUser.faction));
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
