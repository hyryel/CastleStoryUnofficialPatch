using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Common.Collections;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Components;
using Brix.Lua;
using Brix.Network;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

public class CaptureClock : NetworkGameComponent
{
	[SerializeField]
	private CaptureProgress capture;

	[SerializeField]
	public float IncrementTime;

	public static BrixLogger logger = new BrixLogger(typeof(CaptureClock).FullName);

	public Signal<Faction> CaptureStarted = new Signal<Faction>();

	public Signal CaptureEnded = new Signal();

	public Signal<float> CaptureRatioChanged = new Signal<float>();

	public Signal<Faction> CapturerChanged = new Signal<Faction>();

	[LuaDrivable("hk_OnSetCaptureRatio")]
	private LuaHook<float> lua_CaptureRatioChanged;

	[LuaDrivable("hk_OnSetCaptureFaction")]
	private LuaHook<Faction> lua_CapturerChanged;

	private CaptureInfo defense;

	private CaptureInfo attack;

	[SyncVar]
	private GameObject _currentCapturers;

	private bool _attackTrueFactionIsAI;

	private float timeAtLastIncrement;

	private bool wasBeingCaptured;

	private NetworkInstanceId ____currentCapturersNetId;

	[LuaDrivable("captureRatio")]
	public float CaptureRatio => capture.Ratio();

	public Faction CaptureDefault => Faction.DefaultNeutral;

	private bool NonNeutral => faction != CaptureDefault;

	private bool Decays => !faction.isAI && !GetComponent<FireflyNest>().isHome;

	[LuaDrivable("captureFaction")]
	public Faction CurrentCapturers
	{
		get
		{
			if (_currentCapturers == null)
			{
				return null;
			}
			return _currentCapturers.GetComponent<Faction>();
		}
		set
		{
			if (!(_currentCapturers == value.gameObject))
			{
				Network_currentCapturers = value.gameObject;
			}
		}
	}

	[LuaDrivable("capturerIsEnemy")]
	public bool CapturerIsEnemy => (bool)_currentCapturers && Affiliation.BelongsToEnnemy(_currentCapturers.gameObject);

	public GameObject Network_currentCapturers
	{
		get
		{
			return _currentCapturers;
		}
		[param: In]
		set
		{
			SetSyncVarGameObject(value, ref _currentCapturers, 1u, ref ____currentCapturersNetId);
		}
	}

	public override void OnRequested()
	{
		base.OnRequested();
		Network_currentCapturers = CaptureDefault.gameObject;
		lua_CapturerChanged = new LuaHook<Faction>(CapturerChanged);
		lua_CaptureRatioChanged = new LuaHook<float>(CaptureRatioChanged);
	}

	public void SetUnitsInCaptureZone(HashSet<GameObject> unitSet)
	{
		if (NonNeutral)
		{
			attack.Reset(CaptureDefault, Decays ? 1 : 0);
			defense.Reset(faction, 1);
			ComputeAttackAndDefense(unitSet);
			Network_currentCapturers = CaptureDefault.gameObject;
			return;
		}
		attack.Reset(null);
		defense.Reset(CaptureDefault, 1);
		ComputeAttackAndDefense(unitSet);
		if (!attack.ThereIs())
		{
			Network_currentCapturers = CaptureDefault.gameObject;
		}
		else
		{
			Network_currentCapturers = attack.Faction.gameObject;
		}
		if (attack.IsMixed || defense.IsMixed)
		{
			attack.CapturePower = 0;
		}
	}

	private void ComputeAttackAndDefense(HashSet<GameObject> unitSet)
	{
		_attackTrueFactionIsAI = false;
		foreach (GameObject item in unitSet.Slinq().NoLeak())
		{
			try
			{
				CaptureInfo c = CaptureInfo.From(item);
				if (c.Defends(this))
				{
					defense = defense.Plus(c);
				}
				else if (c.Attacks(this))
				{
					attack = attack.Plus(c);
					if (c.Faction.isAI)
					{
						_attackTrueFactionIsAI = true;
					}
				}
			}
			catch (Exception e)
			{
				logger.WarnFormat(e, "Encountered invalid capture contestant while computing capture value in {0}", base.name);
			}
		}
	}

	private void FixedUpdate()
	{
		if (Time.time >= timeAtLastIncrement + IncrementTime)
		{
			timeAtLastIncrement = Time.time;
			OnTimeIncrement();
		}
	}

	private void OnTimeIncrement()
	{
		if (NetworkServer.active)
		{
			int inc = ((!attack.ThereIs()) ? (-1) : ((defense.CapturePower <= 1 && (!_attackTrueFactionIsAI || !NonNeutral || Decays)) ? 1 : 0));
			if (!defense.ThereIs())
			{
				Network_currentCapturers = CaptureDefault.gameObject;
			}
			capture.Add(inc);
			if (capture.Completed() || capture.Prevented())
			{
				if (capture.Completed())
				{
					faction = CurrentCapturers;
				}
				Network_currentCapturers = CaptureDefault.gameObject;
			}
		}
		RefreshCaptureState();
		if (capture.BeingCaptured() && capture.Changed())
		{
			CaptureRatioChanged.Invoke(CaptureRatio);
		}
	}

	private void RefreshCaptureState()
	{
		if (capture.BeingCaptured() && !wasBeingCaptured)
		{
			CaptureStarted.Invoke(CurrentCapturers);
		}
		else if (!capture.BeingCaptured() && wasBeingCaptured)
		{
			CaptureEnded.Invoke();
			capture.Reset();
		}
		wasBeingCaptured = capture.BeingCaptured();
	}

	public int IsOccupiedByLeader(Faction faction)
	{
		return (defense.hasLeader || attack.hasLeader) ? 1 : 0;
	}

	public bool IsUnderAttack()
	{
		return attack.ThereIs();
	}

	[LuaDrivable("IsOccupied")]
	private static DynValue _IsOccupiedByLeader(ScriptExecutionContext context, CallbackArguments args)
	{
		CaptureClock captureClock = args.AsComponent<CaptureClock>(0, "IsOccupied");
		Faction faction = args.AsComponent<Faction>(1, "IsOccupied");
		return DynValue.NewNumber(captureClock.IsOccupiedByLeader(faction));
	}

	[LuaDrivable("IsContested")]
	private static DynValue _IsUnderAttack(ScriptExecutionContext context, CallbackArguments args)
	{
		CaptureClock captureClock = args.AsComponent<CaptureClock>(0, "IsContested");
		return DynValue.NewBoolean(captureClock.IsUnderAttack());
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_currentCapturers);
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
			writer.Write(_currentCapturers);
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
			____currentCapturersNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			_currentCapturers = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		if (!____currentCapturersNetId.IsEmpty())
		{
			Network_currentCapturers = ClientScene.FindLocalObject(____currentCapturersNetId);
		}
	}
}
