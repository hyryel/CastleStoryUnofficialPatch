using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Locomotion;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class CombatNodes
{
	public static Node BreakObject(this Labor labor, GameComponent gameComponent)
	{
		if (labor.recepteur.RequiresBothHands())
		{
			return Node.FailWork<Exceptions.CharacterCantDig>();
		}
		if (gameComponent.IsNullOrReleased() || gameComponent.damageReceiver.IsNullOrReleased() || gameComponent.damageReceiver.IsInvincible)
		{
			return Node.FailWork<Exceptions.CharacterCantDig>();
		}
		WorkNodeGenerator<GameComponent> workNodeGenerator = new WorkNodeGenerator<GameComponent>();
		workNodeGenerator.WorkAction = DoDammage;
		workNodeGenerator.WorkDoneCondition = (GameComponent component, Labor l) => component.IsNullOrReleased();
		workNodeGenerator.FailOnTargetReleased = false;
		workNodeGenerator.CustomAnimation = AttackLoop;
		workNodeGenerator.UseCustomAnimation = true;
		workNodeGenerator.NodeWrappers.Add(WorkNodesWrappers.HighlightTargetRed);
		workNodeGenerator.ReleaseWhen = ReleaseNodeWhen.Ended;
		return workNodeGenerator.BuildNode(gameComponent, labor);
	}

	private static Node DoDammage(GameComponent component, Labor labor)
	{
		return (StaticNode)Node.Sequence.Do((GameComponent _component, Labor _labor) => _component.damageReceiver.ReceiveAttack(_labor.Occupation.Profession.SiegeDamage, DamageType.Siege, DamageReaction.Ignored, _labor.transform.position, _component.transform.position - _labor.transform.position, 0f, _labor.gameObject), component, labor);
	}

	public static Node AttackLoop(Transform component, Labor labor)
	{
		if (labor.Occupation.Profession.GetMeleeWeapon() != null)
		{
			return labor.AttackLoop(Location.OfObject(component) as ObjectLocation);
		}
		return WorkerAttack(component, labor);
	}

	public static Node WorkerAttack(Transform target, Labor labor)
	{
		return AnimationNodes.Once(Node.Empty, Working.WorkStates.Hammer, target, labor);
	}

	public static Node AttackLoop(this Labor labor, ObjectLocation location)
	{
		return (StaticNode)Node.Trier.Label("AttackNode trier").Do(Node.Sequence.Do((Labor _labor, ObjectLocation _location) => _labor.Occupation.Profession.Strike(_location), labor, location).Label("Profession.Strike()").Do((Labor _labor) => Node.WaitForTicks(_labor.Occupation.Profession.RandomizedAttackCooldown), labor)
			.Loop());
	}
}
