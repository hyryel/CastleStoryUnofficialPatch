using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Brix.Audio;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Bricktron;
using Brix.Game.Names;
using Brix.Game.Network;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Network;
using Brix.Transactions;
using Brix.Util;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class Firefly : NetworkGameComponent
{
	[JsonProperty]
	public Vector3 forward;

	[SyncVar]
	[JsonProperty]
	public Vector3 movementSource;

	[SyncVar]
	[JsonProperty]
	public Vector3 movementTarget;

	[SyncVar]
	[JsonProperty]
	public bool trackHotTarget;

	[SyncVar]
	[JsonProperty]
	public bool isBacker;

	[SyncVar]
	[JsonProperty]
	private FireflyMovementMode _movementMode;

	[JsonProperty]
	[SyncVar]
	public GameObject currentHotTarget;

	[JsonProperty]
	private int spawningStep;

	[JsonProperty]
	[SyncVar(hook = "LaborNameChanged")]
	public string laborName;

	[JsonProperty]
	[SyncVar]
	public Face.Expression laborFace;

	[JsonProperty]
	[SyncVar]
	public bool isOldUnit;

	[SyncVar(hook = "LaborOccupationChanged")]
	[JsonProperty]
	public Occupation.Type laborOccupation = Occupation.Type.Unassigned;

	[SyncVar(hook = "SyncPurifiedCrystalLevel_Hook")]
	private int _pureEnergy = 100;

	[JsonProperty]
	[SyncVar]
	public int storedEnergy;

	public bool queued;

	private static float _travelTopSpeed = 2f;

	private static float _dashTopSpeed = 15f;

	private float _terrainCollisionLastChecked;

	private float _collisionLastChecked;

	public Vector3 tendance;

	public float initiative;

	[LuaDrivable("timeToAbsorption")]
	private static float timeToAbsorption = 25f;

	public BrixTimer captureTimer = new BrixTimer(timeToAbsorption);

	public string sonSuffixe;

	public Material mat_blue;

	public Material mat_blueTrail;

	public Material mat_blueParticle;

	public Material mat_purple;

	public Material mat_purpleTrail;

	public Material mat_purpleParticle;

	public Material mat_yellow;

	public Material mat_yellowTrail;

	public Material mat_yellowParticle;

	public float yellowScale = 3f;

	public float queuedScale = 1f;

	public float blueScale = 3f;

	private static List<Firefly> _allFireflies = new List<Firefly>();

	private SpawnerCristal _spawnerCristal;

	private static RaycastHit[] hits = new RaycastHit[500];

	[LuaDrivable("hk_OnCapturedAndNamed")]
	private static LuaHook<LuaDriver> hk_OnCapturedAndNamed = new LuaHook<LuaDriver>();

	private NetworkInstanceId ___currentHotTargetNetId;

	public FireflyMovementMode movementMode => _movementMode;

	public bool IsSpawning => spawningStep > 0;

	[LuaDrivable("isNamed")]
	public bool IsNamed => !string.IsNullOrEmpty(laborName) && !isBacker;

	[LuaDrivable("laborHasOccupation")]
	public bool LaborHasOccupation => laborOccupation != Occupation.Type.Unassigned;

	[JsonProperty]
	public int pureEnergy
	{
		get
		{
			return _pureEnergy;
		}
		set
		{
			Network_pureEnergy = value;
		}
	}

	private Vector3 _effectiveTarget
	{
		get
		{
			if (currentHotTarget == null || !trackHotTarget)
			{
				return movementTarget;
			}
			return currentHotTarget.transform.position + new Vector3(0f, 0.5f, 0f);
		}
	}

	[LuaDrivable("laborName")]
	private string lua_laborName
	{
		get
		{
			return laborName;
		}
		set
		{
			NetworklaborName = value;
		}
	}

	[LuaDrivable("storedEnergy")]
	private int lua_storedEnergy
	{
		get
		{
			return storedEnergy;
		}
		set
		{
			NetworkstoredEnergy = value;
		}
	}

	[LuaDrivable("pureEnergy")]
	private int lua_pureEnergy
	{
		get
		{
			return _pureEnergy;
		}
		set
		{
			Network_pureEnergy = value;
		}
	}

	[LuaDrivable("isHome")]
	private bool lua_isHome => IsHome();

	[LuaDrivable("isBacker")]
	private bool lua_isBacker
	{
		get
		{
			return isBacker;
		}
		set
		{
			NetworkisBacker = value;
		}
	}

	public Vector3 NetworkmovementSource
	{
		get
		{
			return movementSource;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref movementSource, 1u);
		}
	}

	public Vector3 NetworkmovementTarget
	{
		get
		{
			return movementTarget;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref movementTarget, 2u);
		}
	}

	public bool NetworktrackHotTarget
	{
		get
		{
			return trackHotTarget;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref trackHotTarget, 4u);
		}
	}

	public bool NetworkisBacker
	{
		get
		{
			return isBacker;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isBacker, 8u);
		}
	}

	public FireflyMovementMode Network_movementMode
	{
		get
		{
			return _movementMode;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref _movementMode, 16u);
		}
	}

	public GameObject NetworkcurrentHotTarget
	{
		get
		{
			return currentHotTarget;
		}
		[param: In]
		set
		{
			SetSyncVarGameObject(value, ref currentHotTarget, 32u, ref ___currentHotTargetNetId);
		}
	}

	public string NetworklaborName
	{
		get
		{
			return laborName;
		}
		[param: In]
		set
		{
			ref string fieldValue = ref laborName;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				LaborNameChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 64u);
		}
	}

	public Face.Expression NetworklaborFace
	{
		get
		{
			return laborFace;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref laborFace, 128u);
		}
	}

	public bool NetworkisOldUnit
	{
		get
		{
			return isOldUnit;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isOldUnit, 256u);
		}
	}

	public Occupation.Type NetworklaborOccupation
	{
		get
		{
			return laborOccupation;
		}
		[param: In]
		set
		{
			ref Occupation.Type fieldValue = ref laborOccupation;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				LaborOccupationChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 512u);
		}
	}

	public int Network_pureEnergy
	{
		get
		{
			return _pureEnergy;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref _pureEnergy;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SyncPurifiedCrystalLevel_Hook(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1024u);
		}
	}

	public int NetworkstoredEnergy
	{
		get
		{
			return storedEnergy;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref storedEnergy, 2048u);
		}
	}

	private void SetMovementMode(FireflyMovementMode mode)
	{
		if (NetworkServer.active)
		{
			Network_movementMode = mode;
			tendance = Vector3.forward;
		}
	}

	private void LaborNameChanged(string newName)
	{
		NetworklaborName = newName;
		GetComponent<FireflyOverheadDisplayDriver>().SetName(newName);
		UpdateVisuals();
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		if (!string.IsNullOrEmpty(laborName))
		{
			GetComponent<FireflyOverheadDisplayDriver>().SetName(laborName);
		}
	}

	private void LaborOccupationChanged(Occupation.Type newOccupation)
	{
		NetworklaborOccupation = newOccupation;
		UpdateVisuals();
	}

	public override void Initialize()
	{
		base.Initialize();
		initiative = Calcule.De(4, 4, 0.5f);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		UpdateVisuals();
		GetComponent<AffiliationComponent>().OnFactionChanged.Connect(delegate(Faction a, Faction b, Firefly self)
		{
			self.OnFactionChanged();
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		GetComponent<GenericParentTracker>().ObjectNewlyUntrackedSignal.Connect(delegate(GameObject go, Firefly self)
		{
			self.OnUntracked();
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		if (faction == Faction.DefaultAllied && !LaborHasOccupation && IsNamed)
		{
			_spawnerCristal = GetComponentInParent<SpawnerCristal>();
			UIGameObserver.crystals.AddFirefly.Invoke(_spawnerCristal);
		}
		if (LaborHasOccupation && !captureTimer.running && !captureTimer.Check())
		{
			captureTimer.Start();
		}
	}

	public static int RespawningFirefliesForFaction(Faction faction)
	{
		return _allFireflies.Slinq().Where((Firefly ff, Faction f) => ff.IsNamed && ff.faction == f && ff.isOldUnit, faction).Count();
	}

	public static int SpawnableFirefliesForFaction(Faction faction)
	{
		return _allFireflies.Slinq().Where((Firefly ff, Faction f) => ff.IsNamed && ff.faction == f && !ff.isOldUnit, faction).Count();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		_allFireflies.Add(this);
	}

	public override void ResetComponent()
	{
		if (_spawnerCristal != null)
		{
			UIGameObserver.crystals.RemoveFirefly.Invoke(_spawnerCristal);
			_spawnerCristal = null;
		}
		_allFireflies.Remove(this);
		base.ResetComponent();
	}

	private void OnFactionChanged()
	{
		UpdateVisuals();
	}

	private void OnUntracked()
	{
		SetMovementMode(FireflyMovementMode.Undecided);
		captureTimer.Stop();
		captureTimer.Reset();
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		FireflyNest componentInParent = collider.GetComponentInParent<FireflyNest>();
		if (!(componentInParent != null) || componentInParent.faction.isAI || !componentInParent.isHome || spawningStep != 0 || IsHome())
		{
			return;
		}
		if (componentInParent.CaptureFirefly(this))
		{
			GoWanderAround(componentInParent.mainRecepteur.gameObject);
			if (!captureTimer.running && !captureTimer.Check())
			{
				captureTimer.Start();
			}
		}
		else
		{
			SetMovementMode(FireflyMovementMode.Undecided);
		}
	}

	private void Update()
	{
		if (NetworkServer.active)
		{
			if (captureTimer.running && captureTimer.Check())
			{
				Faction parentFaction = GetParentFaction();
				if (parentFaction != null)
				{
					Faction faction = GetComponent<AffiliationComponent>().faction;
					if (parentFaction != faction)
					{
						GetComponent<AffiliationComponent>().faction = parentFaction;
						NetworklaborName = string.Empty;
						NetworklaborOccupation = Occupation.Type.Unassigned;
					}
				}
				UpdateVisuals();
				captureTimer.Stop();
			}
			if (spawningStep == 1)
			{
				if (ReachedTargetPosition())
				{
					spawningStep = 2;
					Vector3 vector = Architecte.PigerUneCase(Architecte.SurfaceRaycast(base.transform.position, 10, downOnly: true), UnityEngine.Random.value * 4f + 2f).ToVector3();
					if (vector == Vector3.zero)
					{
						spawningStep = 0;
						SetMovementMode(FireflyMovementMode.Undecided);
					}
					else
					{
						DashTo(vector);
					}
				}
			}
			else if (spawningStep == 2 && ReachedTargetPosition())
			{
				spawningStep = 3;
				GoToHeaven();
				Factory.AssetKey key = OcupationToAssetkey(laborOccupation);
				GameObject gameObject = Transactor.SpawnUNet(key, GetComponent<AffiliationComponent>().faction, base.transform.position, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
				if (!string.IsNullOrEmpty(laborName))
				{
					gameObject.GetComponent<Nom>().SetNom(laborName);
					Face component = gameObject.GetComponent<Face>();
					if (!component.IsNullOrReleased())
					{
						component.CurrentExpression = laborFace;
					}
				}
				gameObject.GetComponent<Occupation>().SetOccupation(laborOccupation);
				SoundEngine.Play(gameObject.GetComponent<Occupation>().GetProfession().Sounds.Spawn, gameObject);
				ObjectPoolSingleton.Release(base.gameObject);
			}
		}
		if (NetworkServer.active && (movementMode == FireflyMovementMode.Undecided || (trackHotTarget && currentHotTarget == null) || (movementMode == FireflyMovementMode.WanderingAround && IsAlien())))
		{
			ReevaluatePriorities();
		}
		else if (movementMode == FireflyMovementMode.GoingToHeaven)
		{
			GoingToHeaven();
		}
		else if (movementMode == FireflyMovementMode.Travelling)
		{
			Travelling();
		}
		else if (movementMode == FireflyMovementMode.WanderingAround)
		{
			WanderAround();
			if (NetworkServer.active)
			{
				if (_collisionLastChecked + 1f > Time.time)
				{
					CrystalCollisionCheck();
				}
				else if (_terrainCollisionLastChecked + 1f > Time.time)
				{
					TerrainCollisionCheck();
				}
			}
		}
		else if (movementMode == FireflyMovementMode.Dashing)
		{
			Dashing();
		}
		else if (movementMode == FireflyMovementMode.DashCircling)
		{
			DashCircling();
		}
	}

	private Faction GetParentFaction()
	{
		if (base.transform.parent == null)
		{
			return null;
		}
		AffiliationComponent componentInParent = base.transform.parent.GetComponentInParent<AffiliationComponent>();
		if (componentInParent == null)
		{
			return null;
		}
		return componentInParent.faction;
	}

	private bool IsHome()
	{
		if (base.transform.parent != null)
		{
			AffiliationComponent componentInParent = base.transform.parent.GetComponentInParent<AffiliationComponent>();
			if (componentInParent != null && faction == componentInParent.faction)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsAlien()
	{
		if (base.transform.parent != null)
		{
			AffiliationComponent componentInParent = base.transform.parent.GetComponentInParent<AffiliationComponent>();
			if (componentInParent != null && faction != componentInParent.faction)
			{
				return true;
			}
		}
		return false;
	}

	private void ReevaluatePriorities()
	{
		GameObject nearestBestNest = GetNearestBestNest(GetComponent<AffiliationComponent>().faction);
		if (movementMode == FireflyMovementMode.WanderingAround && IsAlien())
		{
			GoToHeaven();
		}
		else if (!(nearestBestNest == null) && movementMode != FireflyMovementMode.Undecided)
		{
			TravelTo(nearestBestNest);
		}
	}

	public void GoToHeaven()
	{
		NetworkmovementSource = base.transform.position;
		NetworkmovementTarget = Vector3.zero;
		NetworktrackHotTarget = false;
		SetMovementMode(FireflyMovementMode.GoingToHeaven);
	}

	public void TravelTo(Vector3 target)
	{
		NetworkmovementSource = base.transform.position;
		NetworkmovementTarget = target;
		NetworktrackHotTarget = false;
		SetMovementMode(FireflyMovementMode.Travelling);
	}

	public void TravelTo(GameObject target)
	{
		NetworkmovementSource = base.transform.position;
		NetworkmovementTarget = Vector3.zero;
		NetworkcurrentHotTarget = target;
		NetworktrackHotTarget = true;
		SetMovementMode(FireflyMovementMode.Travelling);
	}

	public void GoWanderAround(Vector3 target)
	{
		NetworkmovementSource = Vector3.zero;
		NetworkmovementTarget = target;
		NetworktrackHotTarget = false;
		SetMovementMode(FireflyMovementMode.WanderingAround);
	}

	public void GoWanderAround(GameObject target)
	{
		NetworkmovementSource = Vector3.zero;
		NetworkmovementTarget = Vector3.zero;
		NetworkcurrentHotTarget = target;
		NetworktrackHotTarget = true;
		SetMovementMode(FireflyMovementMode.WanderingAround);
	}

	public void DashTo(Vector3 target)
	{
		NetworkmovementSource = base.transform.position;
		NetworkmovementTarget = target;
		NetworktrackHotTarget = false;
		SetMovementMode(FireflyMovementMode.Dashing);
	}

	public void DashTo(GameObject target)
	{
		NetworkmovementSource = base.transform.position;
		NetworkmovementTarget = Vector3.zero;
		NetworkcurrentHotTarget = target;
		NetworktrackHotTarget = true;
		SetMovementMode(FireflyMovementMode.Dashing);
	}

	private void GoingToHeaven()
	{
		base.transform.position = Vector3.MoveTowards(base.transform.position, base.transform.position + (Vector3.up + tendance).normalized, Time.deltaTime * _travelTopSpeed);
		tendance = Quaternion.Euler(0f, 1f, 0f) * tendance;
		if (base.transform.position.y >= movementSource.y + 64f)
		{
			Dissiper();
		}
	}

	private void Travelling()
	{
		base.transform.position = Vector3.MoveTowards(base.transform.position, _effectiveTarget, Time.deltaTime * _travelTopSpeed);
		if (ReachedTargetPosition())
		{
			SetMovementMode(FireflyMovementMode.Undecided);
		}
	}

	private void WanderAround()
	{
		if (tendance == Vector3.zero || Vector3.Angle(forward, tendance) < 10f)
		{
			Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
			tendance = (_effectiveTarget - base.transform.position + new Vector3(insideUnitCircle.x, UnityEngine.Random.value - 0.5f, insideUnitCircle.y).normalized * initiative).normalized;
		}
		forward = Vector3.RotateTowards(forward, tendance, _travelTopSpeed * Time.deltaTime, _travelTopSpeed * Time.deltaTime).normalized;
		base.transform.position += forward * Time.deltaTime * 2f;
		SoundEngine.SetParam("Luciole_Wander", (forward.x + 1f) / 2f, base.gameObject);
		if (base.transform.position.y < _effectiveTarget.y)
		{
			tendance = Vector3.up;
		}
	}

	private void Dashing()
	{
		base.transform.position = Vector3.MoveTowards(base.transform.position, _effectiveTarget, Time.deltaTime * _dashTopSpeed);
		if (ReachedTargetPosition())
		{
			SetMovementMode(FireflyMovementMode.Undecided);
		}
	}

	private void DashCircling()
	{
		base.transform.position = Vector3.MoveTowards(base.transform.position, base.transform.position + tendance, Time.deltaTime * _dashTopSpeed);
		tendance = Quaternion.Euler(0f, 18f, 0f) * tendance;
	}

	public void UpdateVisuals()
	{
		if (IsNamed)
		{
			bool laborHasOccupation = LaborHasOccupation;
			base.transform.localScale = ((!laborHasOccupation) ? (Vector3.one * yellowScale) : (Vector3.one * queuedScale));
			GetComponent<MeshRenderer>().sharedMaterial = mat_yellow;
			GetComponent<ParticleSystemRenderer>().sharedMaterial = mat_yellowParticle;
			GetComponent<TrailRenderer>().sharedMaterial = mat_yellowTrail;
		}
		else
		{
			bool isAI = GetComponent<AffiliationComponent>().faction.isAI;
			base.transform.localScale = Vector3.one * blueScale;
			GetComponent<MeshRenderer>().sharedMaterial = ((!isAI) ? mat_blue : mat_purple);
			GetComponent<ParticleSystemRenderer>().sharedMaterial = ((!isAI) ? mat_blueParticle : mat_purpleParticle);
			GetComponent<TrailRenderer>().sharedMaterial = ((!isAI) ? mat_blueTrail : mat_purpleTrail);
		}
	}

	public void SetFirefly(Faction faction, string name, Face.Expression face, Occupation.Type occupation, bool isOld)
	{
		GetComponent<AffiliationComponent>().faction = faction;
		NetworklaborName = name;
		NetworklaborFace = face;
		NetworklaborOccupation = occupation;
		NetworkisOldUnit = isOld;
	}

	public void StartSpawning(Vector3 stagingLocation)
	{
		StartSpawning(stagingLocation, laborOccupation);
	}

	public void StartSpawning(Vector3 stagingLocation, Occupation.Type occupation)
	{
		NetworklaborOccupation = occupation;
		spawningStep = 1;
		DashTo(stagingLocation);
	}

	public GameObject GetNearestBestNest(Faction faction)
	{
		GameObject gameObject = null;
		List<LuaDriver> allOfFaction = LuaDriverHQ.GetStaticDriver("fy_Crystal").GetAllOfFaction(faction);
		List<LuaDriver> allOfFaction2 = LuaDriverHQ.GetStaticDriver("fy_CookingPot").GetAllOfFaction(faction);
		Slinq<LuaDriver, ConcatContext<IListContext<LuaDriver>, LuaDriver, IListContext<LuaDriver>>> slinq = allOfFaction.Slinq().Concat(allOfFaction2.Slinq());
		foreach (LuaDriver item in slinq.NoLeak())
		{
			FireflyNest componentInChildren = item.GetComponentInChildren<FireflyNest>();
			if (componentInChildren.isHome && (gameObject == null || Vector3.Distance(componentInChildren.transform.position, base.transform.position) < Vector3.Distance(gameObject.transform.position, base.transform.position)))
			{
				gameObject = item.gameObject;
				SetMovementMode(FireflyMovementMode.Travelling);
			}
		}
		return gameObject;
	}

	private bool ReachedTargetPosition()
	{
		return Vector3.Distance(base.transform.position, movementTarget) <= 0.2f;
	}

	private Factory.AssetKey OcupationToAssetkey(Occupation.Type occupation)
	{
		return occupation switch
		{
			Occupation.Type.Minitron => Test.Minitron, 
			Occupation.Type.Corruptron => Test.Corruptron, 
			Occupation.Type.Biftron => Test.Biftron, 
			_ => Test.Bricktron, 
		};
	}

	public void AssignName()
	{
		NetworklaborName = NameDispenserGO.GetDispenserGO(NameDispenserGO.Dispensers.Bricktrons).GetName();
		LuaHookBase.InvokeFromCLR(hk_OnCapturedAndNamed, GetComponent<LuaDriver>());
	}

	private void SyncPurifiedCrystalLevel_Hook(int value)
	{
		Network_pureEnergy = value;
	}

	private void CrystalCollisionCheck()
	{
		if (Physics.Raycast(base.transform.position, forward, out var hitInfo, 2f, 524288, QueryTriggerInteraction.Collide))
		{
			int num = Physics.SphereCastNonAlloc(base.transform.position, 0.5f, forward, hits, 2f, 524288, QueryTriggerInteraction.Collide);
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < num; i++)
			{
				zero += hits[i].normal;
			}
			Array.Clear(hits, 0, num);
			tendance.y = hitInfo.normal.y;
			if (zero.magnitude > 0.5f)
			{
				tendance = (tendance + zero.normalized * 0.5f).normalized;
				base.transform.position -= zero * Vector3.Dot(forward * Time.deltaTime * 1f, zero / num);
			}
		}
		_collisionLastChecked = Time.time;
	}

	private void TerrainCollisionCheck()
	{
		if (Physics.Raycast(base.transform.position, -Vector3.up, out var hitInfo, 20f, 70656, QueryTriggerInteraction.Ignore))
		{
			if (hitInfo.distance > 2f && tendance.y >= 0f)
			{
				tendance.y = 0f - hitInfo.normal.y;
				tendance.Normalize();
				return;
			}
			if (hitInfo.distance < 1.6f && tendance.y <= 0f)
			{
				tendance.y = hitInfo.normal.y;
				tendance.Normalize();
				return;
			}
		}
		_terrainCollisionLastChecked = Time.time;
	}

	private void Dissiper()
	{
		if (NetworkServer.active)
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext sc)
	{
		SetDirtyBit(uint.MaxValue);
	}

	[LuaDrivable("Kill")]
	private static DynValue lua_Dissiper(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject go = args.AsGameObject(0, "Kill");
		if (NetworkServer.active)
		{
			ObjectPoolSingleton.Release(go);
		}
		return DynValue.Void;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(movementSource);
			writer.Write(movementTarget);
			writer.Write(trackHotTarget);
			writer.Write(isBacker);
			writer.Write((int)_movementMode);
			writer.Write(currentHotTarget);
			writer.Write(laborName);
			writer.Write((int)laborFace);
			writer.Write(isOldUnit);
			writer.Write((int)laborOccupation);
			writer.WritePackedUInt32((uint)_pureEnergy);
			writer.WritePackedUInt32((uint)storedEnergy);
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
			writer.Write(movementSource);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(movementTarget);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(trackHotTarget);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(isBacker);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write((int)_movementMode);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(currentHotTarget);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(laborName);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write((int)laborFace);
		}
		if ((base.syncVarDirtyBits & 0x100) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(isOldUnit);
		}
		if ((base.syncVarDirtyBits & 0x200) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write((int)laborOccupation);
		}
		if ((base.syncVarDirtyBits & 0x400) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)_pureEnergy);
		}
		if ((base.syncVarDirtyBits & 0x800) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)storedEnergy);
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
			movementSource = reader.ReadVector3();
			movementTarget = reader.ReadVector3();
			trackHotTarget = reader.ReadBoolean();
			isBacker = reader.ReadBoolean();
			_movementMode = (FireflyMovementMode)reader.ReadInt32();
			___currentHotTargetNetId = reader.ReadNetworkId();
			laborName = reader.ReadString();
			laborFace = (Face.Expression)reader.ReadInt32();
			isOldUnit = reader.ReadBoolean();
			laborOccupation = (Occupation.Type)reader.ReadInt32();
			_pureEnergy = (int)reader.ReadPackedUInt32();
			storedEnergy = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			movementSource = reader.ReadVector3();
		}
		if ((num & 2) != 0)
		{
			movementTarget = reader.ReadVector3();
		}
		if ((num & 4) != 0)
		{
			trackHotTarget = reader.ReadBoolean();
		}
		if ((num & 8) != 0)
		{
			isBacker = reader.ReadBoolean();
		}
		if ((num & 0x10) != 0)
		{
			_movementMode = (FireflyMovementMode)reader.ReadInt32();
		}
		if ((num & 0x20) != 0)
		{
			currentHotTarget = reader.ReadGameObject();
		}
		if ((num & 0x40) != 0)
		{
			LaborNameChanged(reader.ReadString());
		}
		if ((num & 0x80) != 0)
		{
			laborFace = (Face.Expression)reader.ReadInt32();
		}
		if ((num & 0x100) != 0)
		{
			isOldUnit = reader.ReadBoolean();
		}
		if ((num & 0x200) != 0)
		{
			LaborOccupationChanged((Occupation.Type)reader.ReadInt32());
		}
		if ((num & 0x400) != 0)
		{
			SyncPurifiedCrystalLevel_Hook((int)reader.ReadPackedUInt32());
		}
		if ((num & 0x800) != 0)
		{
			storedEnergy = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
		if (!___currentHotTargetNetId.IsEmpty())
		{
			NetworkcurrentHotTarget = ClientScene.FindLocalObject(___currentHotTargetNetId);
		}
	}
}
