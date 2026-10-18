using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Utils.Unity;
using Motus.Behavior;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

public class LaborInstinct : Behavior
{
	private enum EnemyProximity
	{
		Outnumbered,
		Close,
		Ranged,
		ExtendedRange
	}

	[SerializeField]
	private Labor labor;

	public static GameSignals.GameSignal<Labor> DangerStarted = new GameSignals.GameSignal<Labor>();

	public static GameSignals.GameSignal<Labor> DangerEnded = new GameSignals.GameSignal<Labor>();

	private Signal dangerStarted = new Signal();

	private Signal dangerEnded = new Signal();

	private GameObject _lastRangedEnemyWhoAttackedMe;

	private float _lastRangeEnemyFocusTime;

	private bool danger;

	[DisplayRules(DisplayFlags.ReadOnly)]
	[SerializeField]
	private GameObject validEnemy;

	[DisplayRules(DisplayFlags.ReadOnly)]
	[SerializeField]
	private EnemyProximity enemyIsClose;

	[DisplayRules(DisplayFlags.ReadOnly)]
	[SerializeField]
	private CombatPriorityType enemyPriority;

	public IReadOnlySignal OnDangerStarted => dangerStarted;

	public IReadOnlySignal OnDangerEnded => dangerEnded;

	public bool IsInDanger
	{
		get
		{
			return danger;
		}
		set
		{
			if (!danger && value)
			{
				dangerStarted.Invoke();
				GameSignals.Invoke(DangerStarted, labor);
			}
			if (danger && !value)
			{
				dangerEnded.Invoke();
				GameSignals.Invoke(DangerEnded, labor);
			}
			danger = value;
		}
	}

	public override void OnRequested()
	{
		base.OnRequested();
		base.enabled = NetworkServer.active;
	}

	protected override Node CreateTree()
	{
		return (StaticNode)Node.Sequence.Do(delegate
		{
			FightableEnemy(out validEnemy, out enemyIsClose, out enemyPriority);
		}).Do(() => IsInDanger = validEnemy != null).Do(() => Node.WaitForTicks(10))
			.Loop();
	}

	public bool HasWork()
	{
		return IsInDanger;
	}

	public IEnumerator<ComputationProgress<OrderPackage<Labor>>> GetInstruction(Labor labor)
	{
		OrderPackage<Labor> package = GetPackage(validEnemy, enemyIsClose, enemyPriority, IsOutnumbered());
		yield return package.AsNewComputation();
	}

	public void TemporaryFocusOnAttacker(GameObject enemy, float duration)
	{
		_lastRangedEnemyWhoAttackedMe = enemy;
		_lastRangeEnemyFocusTime = Time.time + duration;
	}

	private void FightableEnemy(out GameObject validEnemy, out EnemyProximity isClose, out CombatPriorityType priority)
	{
		validEnemy = null;
		isClose = EnemyProximity.Ranged;
		priority = CombatPriorityType.CompletelyIgnored;
		Slinq<GameObject, LinkedContext<float, GameObject>> closestEnemies = labor.GetClosestEnemies(labor.GetAggroDistance(applyInstinctConstraint: true));
		foreach (GameObject item in closestEnemies.NoLeak())
		{
			FightableEnemy(item, ref validEnemy, ref isClose, ref priority);
			if (validEnemy != null && priority >= CombatPriorityType.AttackOnSight)
			{
				break;
			}
		}
		if (!(validEnemy == null) || !(_lastRangedEnemyWhoAttackedMe != null))
		{
			return;
		}
		if (Time.time >= _lastRangeEnemyFocusTime)
		{
			_lastRangedEnemyWhoAttackedMe = null;
			return;
		}
		FightableEnemy(_lastRangedEnemyWhoAttackedMe, ref validEnemy, ref isClose, ref priority);
		if (validEnemy != null)
		{
			isClose = EnemyProximity.ExtendedRange;
		}
	}

	private void FightableEnemy(GameObject enemy, ref GameObject validEnemy, ref EnemyProximity isClose, ref CombatPriorityType priority)
	{
		IDamageReceiver damageReceiver = enemy.GetComponent<IDamageReceiver>() ?? NullDamageReceiver.Get;
		if (damageReceiver.IsInvincible || damageReceiver.CombatPriority < CombatPriorityType.AttackLowPriority)
		{
			return;
		}
		if (Location.Of(enemy).TargetReached(labor.gameObject, labor.transform.position))
		{
			if (labor.Autonomy.ListenToMeleeInstinct && labor.Profession.HasMeleeAttack())
			{
				validEnemy = enemy;
				isClose = EnemyProximity.Close;
				priority = damageReceiver.CombatPriority;
			}
		}
		else if (labor.Autonomy.ListenToRangeInstinct && (labor.Profession.ProjectileAttackAvailable() || labor.Profession.IsStrongInMelee()))
		{
			validEnemy = enemy;
			isClose = EnemyProximity.Ranged;
			priority = damageReceiver.CombatPriority;
		}
	}

	protected virtual bool IsOutnumbered()
	{
		return false;
	}

	protected virtual OrderPackage<Labor, GameObject> OnOutnumbered()
	{
		return null;
	}

	private OrderPackage<Labor> GetPackage(GameObject enemy, EnemyProximity isClose, CombatPriorityType priority, bool outnumbered)
	{
		OrderPackage<Labor> orderPackage = null;
		if (enemy == null)
		{
			return OrderPackage<Labor>.Empty;
		}
		if (outnumbered)
		{
			return OnOutnumbered();
		}
		switch (isClose)
		{
		case EnemyProximity.Close:
			return Order.Package.OfGameObject(Order.Combat.HoldPositionAndAttackWithPriority, enemy, Labor.DecisionSource.Character);
		case EnemyProximity.ExtendedRange:
			if (labor.Profession.IsStrongInMelee())
			{
				return Order.Package.OfGameObject(Order.Combat.GoAttack, enemy, Labor.DecisionSource.Character);
			}
			break;
		}
		if (labor.Profession.CanFireAt(Location.Of(enemy)))
		{
			return Order.Package.OfGameObject(Order.Combat.HoldPositionAndAttackWithPriority, enemy, Labor.DecisionSource.Character);
		}
		if (labor.Profession.ProjectileAttackAvailable() && labor.Profession.WillPursueWithRangeAttack())
		{
			return Order.Package.OfGameObject(Order.Combat.GoAttack, enemy, Labor.DecisionSource.Character);
		}
		if (labor.Profession.ProjectileAttackAvailable() && !labor.Profession.IsStrongInMelee())
		{
			return Order.Package.OfLocation(Order.Combat.HoldPositionAndAttack, Location.Empty, Labor.DecisionSource.Character);
		}
		if (priority == CombatPriorityType.AttackOnSight)
		{
			return Order.Package.OfGameObject(Order.Combat.GoAttackWithPriority, enemy, Labor.DecisionSource.Character);
		}
		return Order.Package.OfGameObject(Order.Combat.GoAttack, enemy, Labor.DecisionSource.Character);
	}
}
