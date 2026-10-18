using Brix.Audio;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.CatapultSpace;
using Brix.Game.Rules;

namespace Brix.Input.ContextualOrders;

public class KnightActor : ContextualActor
{
	public KnightActor(Labor oe, ContextualActionGroupData groupData, ActionType type, bool toQueue)
		: base(oe, groupData, type, toQueue)
	{
	}

	protected override void ToTerrain(XYZ voxel)
	{
		if (actionType == ActionType.Primary)
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
			Location data = Location.Of(groupData.GetFormationDestination(voxel.ToVector3(), subject.gameObject));
			orderPackage = Order.Package.OfLocation(Order.Move.RunToward, data);
		}
		else if (actionType == ActionType.Secondary && TerrainRules.Main.CanDigAt(voxel.ToVector3()))
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
			orderPackage = Order.Package.OfLocation(Order.ForLabor.GoDig, new SpaceLocation(voxel.ToVector3(), walkable: false));
		}
		else if (actionType == ActionType.Ternary)
		{
			SoundEngine.Play(subject.Sounds.ActionAttack, subject.gameObject);
			Location data2 = Location.Of(groupData.GetFormationDestination(voxel.ToVector3(), subject.gameObject));
			orderPackage = Order.Package.OfLocation(Order.Combat.MoveAttack, data2);
		}
	}

	protected override void ToCatapult(CatapultOperations gameobject)
	{
		ToTerrain(XYZ.FromVector3(gameobject.transform.position));
	}

	protected override void ToGameComponent(GameComponent component)
	{
		if (ContextualActor.ShouldAttackByDefault(subject, component) || actionType == ActionType.Secondary)
		{
			SoundEngine.Play(subject.Sounds.ActionAttack, subject.gameObject);
			orderPackage = Order.Package.OfGameObject(Order.Combat.GoAttackDirectly, component.gameObject);
		}
		else
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
			orderPackage = Order.Package.OfGameObject(Order.Move.MoveToVolume, component.gameObject);
		}
	}

	protected override void ToUnit(Labor unit)
	{
		if (ContextualActor.ShouldAttackByDefault(subject, unit.gameObject))
		{
			SoundEngine.Play(subject.Sounds.ActionAttack, subject.gameObject);
			orderPackage = Order.Package.OfGameObject(Order.Combat.GoAttackDirectly, unit.gameObject);
		}
		else
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
			orderPackage = Order.Package.OfGameObject(Order.Move.FollowCharacter, unit.gameObject);
		}
	}

	protected override void ToEquipment(Tool tool)
	{
	}

	protected override void ToTree(XYZ voxel)
	{
		ToTerrain(voxel);
	}

	protected override void ToNature(XYZ voxel)
	{
		ToTerrain(voxel);
	}
}
