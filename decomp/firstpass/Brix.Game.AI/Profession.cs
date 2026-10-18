using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Game.AI.Nodes;
using Brix.Game.Audio;
using Brix.Game.Rules;
using Brix.Lua;
using Brix.Network;
using Brix.UI.Icons;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Motus.Behavior;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public abstract class Profession : NetworkGameComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(Profession).FullName);

	public Signal CapacityChanged = new Signal();

	protected Labor labor;

	[HideInInspector]
	public Toolbag toolbag;

	private ProjectileAiming _localAiming;

	[LuaDrivable("VigilanceRadius")]
	public float BaseAggroRange = 25f;

	public int AttackCooldown = 75;

	public int AttackCooldownRandomness;

	private int _characterLevel = 1;

	private ConflictResolver _conflictResolver;

	public AnimatorOverrideController Overrides;

	private RuntimeAnimatorController OriginalController;

	protected Locomotion4 Locomotion;

	public abstract ProfessionAudio Audio { get; }

	public abstract Sprite Icon32 { get; }

	public virtual Sprite Icon64 => Icon32;

	public BricktronSoundEffects Sounds => Audio.Sounds;

	protected ProjectileAiming LocalAiming => _localAiming ?? (_localAiming = new ProjectileAiming(base.transform));

	public Occupation.Type OccupationType => labor.Occupation.CurrentOccupation;

	public int RandomizedAttackCooldown => AttackCooldown + UnityEngine.Random.Range(-AttackCooldownRandomness, AttackCooldownRandomness);

	[JsonProperty]
	[LuaDrivable("CharacterLevel")]
	public int CharacterLevel
	{
		get
		{
			return _characterLevel;
		}
		set
		{
			_characterLevel = value;
			toolbag.CompileToolStatistics();
			if (value > 1 && GetComponent<LevelUpMaterials>() != null)
			{
				GetComponent<LevelUpMaterials>().ChangeMaterial(value - 2);
			}
			Locomotion4 component = Factory.Peek(AssetKey).GetComponent<Locomotion4>();
			Locomotion4 component2 = GetComponent<Locomotion4>();
			component2.WalkSpeedMperS = component.WalkSpeedMperS * MovementSpeedLevelMultiplier;
			component2.RunSpeedMperS = component.RunSpeedMperS * MovementSpeedLevelMultiplier;
			Profession component3 = Factory.Peek(AssetKey).GetComponent<Profession>();
			AttackCooldown = Mathf.RoundToInt((float)component3.AttackCooldown / AttackSpeedLevelMultiplier);
		}
	}

	public int JobSpeedBonus
	{
		get
		{
			if (this is Builder)
			{
				return labor.Occupation.CurrentJob switch
				{
					Occupation.Job.Builder => 1, 
					Occupation.Job.Halberdier => 1, 
					Occupation.Job.Archer => 1, 
					Occupation.Job.Knight => -1, 
					Occupation.Job.Arbalist => -1, 
					Occupation.Job.Artificer => -1, 
					Occupation.Job.Alchemist => 0, 
					_ => 0, 
				};
			}
			return 0;
		}
	}

	public float JobRangedCooldown
	{
		get
		{
			if (this is Builder)
			{
				return labor.Occupation.CurrentJob switch
				{
					Occupation.Job.Archer => 0.6f, 
					Occupation.Job.Arbalist => 1f, 
					Occupation.Job.Artificer => 0.8f, 
					Occupation.Job.Alchemist => 0.8f, 
					_ => 0.8f, 
				};
			}
			return 0.8f;
		}
	}

	public virtual float CombatAggro => InferedArchetype() switch
	{
		Occupation.Type.Knight => 0.33f, 
		Occupation.Type.Archer => 0.15f, 
		Occupation.Type.Builder => 0.04f, 
		_ => 0f, 
	};

	public virtual float RaidProjectAggro => 1f;

	public virtual CombatPriorityType CombatPriority => InferedArchetype() switch
	{
		Occupation.Type.Knight => CombatPriorityType.AttackOnSight, 
		Occupation.Type.Archer => CombatPriorityType.AttackOnSight, 
		Occupation.Type.Builder => CombatPriorityType.AttackHighPriority, 
		_ => CombatPriorityType.AttackOnSight, 
	};

	public virtual float BlockingAngle => BlockAngle;

	public virtual float Size => 1.25f;

	public virtual Vector3 AimAtOffset => Vector3.up;

	public virtual bool AvoidsFriendlyFire => true;

	public virtual int VoxelRepair => 120;

	public virtual float SiegeDamage => (float)BlockBreaking * CombatRules.UnitToObjectDamageMultiplier;

	public virtual float ProjectileSpeedMultiplier => 1f;

	public virtual bool UsesStamina => false;

	public int MeleeDamage => Mathf.RoundToInt((float)Mathf.Max(GetStatistic(Adjectif.meleeDamage), 12) * CombatRules.UnitToUnitDamageMultiplier * MeleeDamageLevelMultiplier);

	private float MeleeDamageLevelMultiplier => 1f + (float)Mathf.Max(CharacterLevel - 1, 0) * 0.2f;

	private float MovementSpeedLevelMultiplier => 1f + (float)Mathf.Max(CharacterLevel - 1, 0) * 0.3f;

	private float AttackSpeedLevelMultiplier => 1f + (float)Mathf.Max(CharacterLevel - 1, 0) * 0.3f;

	public int MeleeWeight => Mathf.Max(GetStatistic(Adjectif.meleeWeight), 1);

	public int Protection => GetStatistic(Adjectif.protection);

	public int BlockAngle => GetStatistic(Adjectif.blockAngle);

	public virtual int RangeDamage => Mathf.RoundToInt((float)GetStatistic(Adjectif.rangeDamage) * CombatRules.UnitToUnitDamageMultiplier);

	public int RangePrecision => GetStatistic(Adjectif.rangePrecision);

	public float AggroDistance => (float)GetStatistic(Adjectif.visionDistance) + BaseAggroRange;

	public int ToolSmithing => GetStatistic(Adjectif.toolSmithing);

	public int WoodCutting => GetStatistic(Adjectif.woodCutting);

	public int MineralHarvesting => GetStatistic(Adjectif.mineralHarvesting);

	public virtual int BlockBreaking => GetStatistic(Adjectif.blockBreaking);

	public int BlockRepair => GetStatistic(Adjectif.blockRepair);

	public virtual int TerrainDamage
	{
		get
		{
			if (OccupationType == Occupation.Type.Knight)
			{
				return 2;
			}
			return BlockBreaking;
		}
	}

	public Occupation.Type InferedArchetype()
	{
		if (this is Magitron)
		{
			return Occupation.Type.Magitron;
		}
		if (this is Biftron)
		{
			return Occupation.Type.Biftron;
		}
		if (this is Corruptron)
		{
			return Occupation.Type.Corruptron;
		}
		if (this is Minitron)
		{
			return Occupation.Type.Minitron;
		}
		return IsStrongInMelee() ? Occupation.Type.Knight : (ProjectileAttackAvailable() ? Occupation.Type.Archer : Occupation.Type.Builder);
	}

	public abstract void EquipGear();

	public virtual bool CanEquip(GameObject go)
	{
		Tool component = go.GetComponent<Tool>();
		return component != null && !component.Magical;
	}

	public virtual bool CanJumpOff()
	{
		return false;
	}

	public virtual bool CanClimbWall()
	{
		return false;
	}

	public virtual bool CanMakeWay()
	{
		return false;
	}

	public virtual bool IsWorker()
	{
		return false;
	}

	public virtual bool CanPassThrough()
	{
		return false;
	}

	public virtual bool CanTransferTo(Labor other)
	{
		return false;
	}

	public virtual bool CanCollectPlants()
	{
		return !labor.recepteur.RequiresBothHands();
	}

	public virtual bool CanHarvestMinerals()
	{
		return MineralHarvesting > 0 && !labor.recepteur.RequiresBothHands();
	}

	public virtual bool CanChopTrees()
	{
		return WoodCutting > 0 && !labor.recepteur.RequiresBothHands();
	}

	public virtual bool CanBreakVoxels()
	{
		return BlockBreaking > 0 && !labor.recepteur.RequiresBothHands();
	}

	public virtual bool CanSwitchTools()
	{
		return labor.Occupation.CurrentOccupation == Occupation.Type.Builder;
	}

	public virtual bool CanRun()
	{
		return true;
	}

	public virtual bool CanMoveAttack()
	{
		return true;
	}

	public virtual bool CanBlock()
	{
		return BlockAngle > 0;
	}

	public virtual bool CanRunWhileDefensive()
	{
		return true;
	}

	public virtual bool CanSack()
	{
		return false;
	}

	public virtual bool CanClimbWithLoad()
	{
		return true;
	}

	public virtual bool CanTranformLog()
	{
		return true;
	}

	public virtual bool CanWalkInForest()
	{
		return labor.presence.IsInForest();
	}

	public override void Initialize()
	{
		base.Initialize();
		labor = GetComponent<Labor>();
		toolbag = GetComponent<Toolbag>();
	}

	protected override void Start()
	{
		base.Start();
		Locomotion = GetComponent<Locomotion4>();
		Occupation occupation = GetComponent<Occupation>();
		CapacityChanged.Connect(delegate
		{
			occupation.CurrentOccupation = OccupationType;
		});
		if (NetworkServer.active)
		{
			Locomotion.ChangeArchetype(occupation.GetAnimationArchetypeFromToolbag());
			CapacityChanged.Connect(delegate
			{
				Locomotion.ChangeArchetype(occupation.GetAnimationArchetypeFromToolbag());
			});
		}
		_conflictResolver = new ConflictResolver(GetConflictResolutionModel);
		labor.navigation.RequestTransferSignal.Connect(_conflictResolver.OnLaborRequestTransfer).DisconnectWhen(labor.OnReleaseOrDestroySignal);
	}

	protected virtual ConflictResolver.ResolutionModel GetConflictResolutionModel()
	{
		return ConflictResolver.ResolutionModel.Warrior;
	}

	public IconKey HoldIcon()
	{
		if (OccupationType == Occupation.Type.Builder)
		{
			return IconKeys.Cancel;
		}
		if (toolbag.HasTool(Accessories.Bow))
		{
			return IconKeys._Hold_Fire;
		}
		if (toolbag.HasTool(Accessories.Crossbow))
		{
			return IconKeys._Hold_Fire_Crossbow;
		}
		if (toolbag.HasTool(Accessories.Staff))
		{
			return IconKeys._Healing_Ward;
		}
		if (toolbag.HasTool(Accessories.Backpack))
		{
			return IconKeys._Hold_Fire_Bomb;
		}
		if (toolbag.HasTool(Accessories.Sword))
		{
			return IconKeys._Shield;
		}
		if (toolbag.HasTool(Accessories.Poleaxe))
		{
			return IconKeys._Shield;
		}
		return IconKeys.Missing;
	}

	public IconKey InstinctIcon()
	{
		if (OccupationType == Occupation.Type.Builder)
		{
			return IconKeys.Hourglass;
		}
		if (toolbag.HasTool(Accessories.Bow))
		{
			return IconKeys._Bow;
		}
		if (toolbag.HasTool(Accessories.Crossbow))
		{
			return IconKeys._Crossbow;
		}
		if (toolbag.HasTool(Accessories.Staff))
		{
			return IconKeys._Sentinel;
		}
		if (toolbag.HasTool(Accessories.Backpack))
		{
			return IconKeys._Bomb;
		}
		if (toolbag.HasTool(Accessories.Sword))
		{
			return IconKeys._Sword;
		}
		if (toolbag.HasTool(Accessories.Poleaxe))
		{
			return IconKeys._Poleaxe;
		}
		return IconKeys.Missing;
	}

	public void EquipControllerOverrides()
	{
		if (Overrides != null)
		{
			Animator component = GetComponent<Animator>();
			OriginalController = component.runtimeAnimatorController;
			component.runtimeAnimatorController = Overrides;
		}
	}

	public void UnequipControllerOverrides()
	{
		if (OriginalController != null)
		{
			GetComponent<Animator>().runtimeAnimatorController = OriginalController;
		}
	}

	public virtual bool HasMeleeAttack()
	{
		return MeleeDamage != 0;
	}

	public bool ProjectileAttackAvailable()
	{
		ProjectileWeapon weapon;
		return ProjectileAttackAvailable(out weapon);
	}

	public virtual bool ProjectileAttackAvailable(out ProjectileWeapon weapon)
	{
		weapon = null;
		if (toolbag.HasToolFor(Adjectif.rangeDamage, out var tool) || toolbag.HasToolFor(Adjectif.rangePrecision, out tool))
		{
			weapon = tool.GetComponent<ProjectileWeapon>();
			return weapon.IsAvailableFor(labor);
		}
		return false;
	}

	public virtual bool WillPursueWithRangeAttack()
	{
		return false;
	}

	public bool IsStrongInMelee()
	{
		return OccupationType == Occupation.Type.Knight || OccupationType == Occupation.Type.Biftron || OccupationType == Occupation.Type.Corruptron || OccupationType == Occupation.Type.Minitron;
	}

	public virtual Tool GetMeleeWeapon()
	{
		toolbag.HasToolFor(Adjectif.meleeDamage, out var tool);
		return (!(tool == null)) ? tool.GetComponent<Tool>() : null;
	}

	public virtual bool CanWieldPick()
	{
		return false;
	}

	public virtual bool CanWieldAxe()
	{
		return false;
	}

	public virtual bool CanWieldHammer()
	{
		return false;
	}

	public virtual MeleeAttackVariation ChooseAttackVariation()
	{
		return MeleeAttackVariation.BricktronFist;
	}

	public Node Strike(ObjectLocation target)
	{
		return Strike(target, base.transform.position);
	}

	public Node Strike(ObjectLocation target, Vector3 endPosition)
	{
		return (StaticNode)Node.Selector.When(() => target == null || target.Object == null).Do(Node.FailWork<Exceptions.CombatTargetIsDead>()).When(() => true)
			.Do(() => Strike(target.Object.GetComponent<IDamageReceiver>(), endPosition));
	}

	public Node Strike(IDamageReceiver target, Vector3 endPosition)
	{
		if (!HasMeleeAttack())
		{
			logger.InfoFormat("Attack {0} on {1}: no target", base.name, target);
			return Node.FailWork<Exceptions.CombatTargetIsTooFarAway>();
		}
		return (StaticNode)Node.Sequence.Do(delegate
		{
			Locomotion.Wield(GetMeleeWeapon());
		}).Do(delegate
		{
			Locomotion.DoStopBlocking();
		}).Do(() => UtilityNodes.WaitForTimeOrCondition(() => Locomotion.IsReadyToStrike, 1f, failOnDelay: true))
			.Do(() => Locomotion.DoStrike(target, endPosition))
			.Process((MeleeAttack attack) => Node.Selector.If(() => attack == null).Do(Node.FailWork<Exceptions.CombatTargetIsTooFarAway>()).Else()
				.Do(() => Node.Selector.When(() => attack.CurrentPhase == MeleeAttack.Phase.Finished || attack.Target.IsNullOrReleased()).Do(Node.Empty).Else()
					.Do(() => Node.WaitForTicks(100))));
	}

	public Node ShootAt(Location location)
	{
		if (!ProjectileAttackAvailable())
		{
			logger.InfoFormat("ShootAt {0} to {1}: Can't fire", base.name, location);
			return Node.FailWork<Exceptions.CombatTargetIsTooFarAway>();
		}
		SignalConnection connection = SignalConnection.Empty;
		bool damaged = false;
		Action receivedDamage = delegate
		{
			damaged = true;
		};
		bool succeeded = false;
		return (StaticNode)Node.Sequence.Do(() => connection = GetComponent<DamageReceiver>().HasReceivedDamage.Connect(receivedDamage)).Do(Node.Selector.When(() => location is ObjectLocation && location.Object == null).Do(delegate
		{
			logger.InfoFormat("ShootAt {0} to {1}: no target", base.name, location);
		}).When(Locomotion.AttackFailed)
			.Do(delegate
			{
				logger.InfoFormat("ShootAt {0} to {1}: attack failed", base.name, location);
			})
			.When(() => damaged)
			.Do(delegate
			{
				logger.InfoFormat("ShootAt {0} to {1}: damaged", base.name, location);
			})
			.Else()
			.Do(delegate
			{
				GetComponent<SignalHandler>().ChangeTarget.Invoke(location.Object);
			})
			.Do(() => TurnToAimAt(location))
			.Do(() => TakeArcheryStance())
			.Do(delegate
			{
				LoadArrow();
			})
			.Do(() => FireAt(location))
			.Do(() => succeeded = true)).Do(Node.Selector.If(() => !succeeded).Do(Node.FailWork<Exceptions.CombatTargetIsTooFarAway>()))
			.Finally(delegate
			{
				connection.Disconnect();
				Locomotion.StopArcheryStance();
				GetComponent<SignalHandler>().ChangeTarget.Invoke(null);
			});
	}

	public virtual bool CanFireAt(Location target)
	{
		if (!TargetIsWeakToProjectileAttack(target))
		{
			return false;
		}
		if (!ProjectileAttackAvailable(out var weapon))
		{
			return false;
		}
		return CanReachWithProjectile(target, weapon);
	}

	protected virtual bool TargetIsWeakToProjectileAttack(Location target)
	{
		return true;
	}

	protected bool CanReachWithProjectile(Location target, ProjectileWeapon weapon)
	{
		LocalAiming.SetupFire(target, weapon, ProjectileSpeedMultiplier, AvoidsFriendlyFire);
		return LocalAiming.CanReachWithProjectile(Locomotion.ProjectileAimingPosition, weapon.AmmoCollisionLayer, AggroDistance, !Locomotion.IsInArcheryStance);
	}

	private IEnumerator<ActivityProgress> TurnToAimAt(Location target)
	{
		Vector3 targetPos = target.Position;
		Locomotion.StopArcheryStance();
		while (Locomotion.IsInArcheryStance)
		{
			yield return ActivityProgress.Wait();
		}
		if (!(Vector3.Angle(base.transform.forward, (targetPos - base.transform.position).Flatten()) < 40f))
		{
			yield return ActivityProgress.Sub(GetComponent<Navigation>().TurnTowards(target));
		}
	}

	private IEnumerator<ActivityProgress> TakeArcheryStance()
	{
		if (!ProjectileAttackAvailable(out var weapon))
		{
			yield return ActivityProgress.Fail();
		}
		Locomotion.WieldBow(weapon);
		while (!Locomotion.IsReadyToWield())
		{
			yield return ActivityProgress.Wait();
		}
		yield return ActivityProgress.Wait();
		while (!Locomotion.IsReadyToShoot)
		{
			yield return ActivityProgress.Wait();
		}
		yield return ActivityProgress.Wait(10);
	}

	private void LoadArrow()
	{
	}

	private IEnumerator<ActivityProgress> FireAt(Location target)
	{
		ProjectileWeapon weapon;
		if (!Locomotion.IsReadyToShoot)
		{
			logger.Warn("FireAt was called when character is not in Archery Idle state. Attack will fail.");
			yield return ActivityProgress.Fail("Invalid Archery State.");
		}
		else if (!CanFireAt(target) || !ProjectileAttackAvailable(out weapon))
		{
			yield return ActivityProgress.Fail(SharedWorkException<Exceptions.CombatTargetIsTooFarAway>.Get);
		}
		else
		{
			Locomotion.Aiming.SetupFire(target, weapon, ProjectileSpeedMultiplier, AvoidsFriendlyFire);
			Locomotion.ShootAt(target, CombatRules.ArrowAimTime, CombatRules.ArrowInaccuracy);
			yield return ActivityProgress.Wait(Locomotion.HasFiredArrow);
		}
	}

	public int GetStatistic(Accessoire adjectif)
	{
		return toolbag.ToolStatistics.Get(adjectif)?.valeur ?? 0;
	}

	public void SetHpBonus(int bonus)
	{
		BricktronDamageReceiver component = Factory.Peek(GetComponent<AssetKeyHolder>().AssetKey).GetComponent<BricktronDamageReceiver>();
		GetComponent<BricktronDamageReceiver>().MaxHP = ((float)bonus + component.MaxHP) * (float)CharacterLevel;
	}

	public void SetStaminaBonus(int bonus)
	{
		BricktronDamageReceiver component = Factory.Peek(GetComponent<AssetKeyHolder>().AssetKey).GetComponent<BricktronDamageReceiver>();
		GetComponent<BricktronDamageReceiver>().MaxStamina = ((float)bonus + component.MaxStamina) * (float)CharacterLevel;
	}

	public virtual Node GetDangerReactionNode(Labor subject, GameObject enemy)
	{
		return subject.GoAttack(enemy.gameObject);
	}

	public virtual bool IsCorruptron()
	{
		return false;
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
