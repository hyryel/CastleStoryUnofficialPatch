using Brix.External.Factories;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class RaidProject : RaidProject2
{
	[SerializeField]
	private RaidProjectData data;

	public bool StartSacking;

	public float PatrolRange = 10f;

	[ContextMenuItem("Register Corruptrons", "AddCorruptrons")]
	public int nbCorruptronsToAdd;

	private bool connected;

	[JsonProperty]
	[LuaDrivable("isDefensive")]
	public bool IsDefensive;

	[JsonProperty]
	public RaidType _raidType;

	[JsonProperty]
	[LuaDrivable("assignedCrystal")]
	public SpawnerCristal AssignedCrystal;

	public bool IsCorruptrons => isForAI;

	public int MaxCount => 10;

	protected override bool DoesPatrol => false;

	public RaidType RaidType
	{
		get
		{
			return _raidType;
		}
		set
		{
			_raidType = value;
			priorityWeights = ProjectPriority.WeightsFor(this);
		}
	}

	[LuaDrivable("minitronTotal")]
	private DynValue lua_minitronTotal => DynValue.NewNumber(data.minitronTotal);

	[LuaDrivable("corruptronTotal")]
	private DynValue lua_corruptronTotal => DynValue.NewNumber(data.corruptronTotal);

	[LuaDrivable("biftronTotal")]
	private DynValue lua_biftronTotal => DynValue.NewNumber(data.biftronTotal);

	[LuaDrivable("minitronCurrent")]
	private DynValue lua_minitronCurrent => DynValue.NewNumber(data.minitronCurrent);

	[LuaDrivable("corruptronCurrent")]
	private DynValue lua_corruptronCurrent => DynValue.NewNumber(data.corruptronCurrent);

	[LuaDrivable("biftronCurrent")]
	private DynValue lua_biftronCurrent => DynValue.NewNumber(data.biftronCurrent);

	[LuaDrivable("waveNumber")]
	private DynValue waveNumber
	{
		get
		{
			return DynValue.NewNumber(data.WaveNumber);
		}
		set
		{
			data.NetworkWaveNumber = (int)value.Number;
		}
	}

	[LuaDrivable("raidType")]
	private DynValue lua_raidType
	{
		set
		{
			RaidType = value.ToObject<LuaEnumProxy<RaidType>>().value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		CheckForGoals = false;
	}

	protected override void Start()
	{
		base.Start();
		if (!NetworkServer.active || data.minitronTotal != 0 || data.corruptronTotal != 0 || data.biftronTotal != 0)
		{
			return;
		}
		foreach (Labor item in Workforce)
		{
			CountCorruptronsTotal(item, 1);
		}
	}

	protected void Update()
	{
		if (base.DeathCount > 0f && Time.time - LastDeathCountChange > 60f)
		{
			base.DeathCount = Mathf.Max(base.DeathCount - 1f, 0f);
		}
	}

	public override void OnHire(Labor labor)
	{
		base.OnHire(labor);
		CountCorruptronsCurrent(labor, 1);
		if (NetworkServer.active)
		{
			labor.faction = base.faction;
		}
	}

	public override void OnFire(Labor labor)
	{
		base.OnFire(labor);
		CountCorruptronsCurrent(labor, -1);
	}

	public override void OnBecameEmpty()
	{
		base.OnBecameEmpty();
		if (NetworkServer.active)
		{
			SelfDestruct();
		}
	}

	public void OnHit(GameObject origin)
	{
	}

	private void CountCorruptronsCurrent(Labor labor, int amount)
	{
		switch (labor.Occupation.CurrentOccupation)
		{
		case Occupation.Type.Minitron:
		{
			RaidProjectData raidProjectData3 = data;
			raidProjectData3.NetworkminitronCurrent = raidProjectData3.minitronCurrent + amount;
			break;
		}
		case Occupation.Type.Corruptron:
		{
			RaidProjectData raidProjectData2 = data;
			raidProjectData2.NetworkcorruptronCurrent = raidProjectData2.corruptronCurrent + amount;
			break;
		}
		case Occupation.Type.Biftron:
		{
			RaidProjectData raidProjectData = data;
			raidProjectData.NetworkbiftronCurrent = raidProjectData.biftronCurrent + amount;
			break;
		}
		}
	}

	private void CountCorruptronsTotal(Labor labor, int amount)
	{
		switch (labor.Occupation.CurrentOccupation)
		{
		case Occupation.Type.Minitron:
		{
			RaidProjectData raidProjectData3 = data;
			raidProjectData3.NetworkminitronTotal = raidProjectData3.minitronTotal + amount;
			break;
		}
		case Occupation.Type.Corruptron:
		{
			RaidProjectData raidProjectData2 = data;
			raidProjectData2.NetworkcorruptronTotal = raidProjectData2.corruptronTotal + amount;
			break;
		}
		case Occupation.Type.Biftron:
		{
			RaidProjectData raidProjectData = data;
			raidProjectData.NetworkbiftronTotal = raidProjectData.biftronTotal + amount;
			break;
		}
		}
	}

	public void AddCorruptrons()
	{
		for (int i = 0; i < nbCorruptronsToAdd; i++)
		{
			ObjectPoolSingleton.Request(out var go, Test.Corruptron, base.transform.position, Quaternion.identity);
			Hire(go.GetComponent<Labor>());
		}
	}

	public override bool HasConnectedOrigins(Labor labor)
	{
		return labor != null && labor.OriginalSpawnCrystal != null && AssignedCrystal != null && labor.OriginalSpawnCrystal.HasPotentialConnection(AssignedCrystal);
	}
}
