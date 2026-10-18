using Brix.Audio;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.CatapultSpace;
using Brix.Game.Semantique;

namespace Brix.Input.ContextualOrders;

public class ArcherActor : ContextualActor
{
	public ArcherActor(Labor oe, ContextualActionGroupData groupData, ActionType type, bool toQueue)
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
		else if (actionType == ActionType.Secondary && !subject.toolbag.HasTool(Accessories.Staff))
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
			orderPackage = Order.Package.OfLocation(Order.Combat.ShootAt, new SpaceLocation(voxel.ToVector3(), walkable: false));
		}
		else if (actionType == ActionType.Ternary)
		{
			if (subject.toolbag.HasTool(Accessories.Staff))
			{
				subject.Autonomy = AutonomyStatus.InstinctOnly;
			}
			SoundEngine.Play(subject.Sounds.ActionAttack, subject.gameObject);
			Location data2 = Location.Of(groupData.GetFormationDestination(voxel.ToVector3(), subject.gameObject));
			orderPackage = Order.Package.OfLocation(Order.Combat.MoveAttack, data2);
		}
	}

	protected override void ToCatapult(CatapultOperations catapult)
	{
		ToTerrain(XYZ.FromVector3(catapult.transform.position));
	}

	protected override void ToGameComponent(GameComponent component)
	{
		if (ContextualActor.ShouldAttackByDefault(subject, component) || actionType == ActionType.Secondary)
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
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
		if (subject.toolbag.HasTool(Accessories.Staff))
		{
			subject.Autonomy = AutonomyStatus.InstinctOnly;
		}
		if (Affiliation.IsEnemy(subject.gameObject, unit.gameObject))
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
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

	protected override void ToTransferable(Recepteur recepteur)
	{
		if (recepteur.ContentDescription.HasPresenceOrValue(Adjectif.bomb) && subject.Profession.toolbag.HasTool(Accessories.Backpack))
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
			orderPackage = Order.Package.OfGameComponent(Order.Transfer.GoPickUpAmmo, recepteur);
		}
		else
		{
			base.ToTransferable(recepteur);
		}
	}
}
