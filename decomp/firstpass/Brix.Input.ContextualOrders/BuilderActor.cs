using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.CatapultSpace;
using Brix.Game.Rules;
using Brix.Game.Semantique;

namespace Brix.Input.ContextualOrders;

public class BuilderActor : ContextualActor
{
	public BuilderActor(Labor oe, ContextualActionGroupData groupData, ActionType type, bool toQueue)
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
		if (actionType == ActionType.Secondary)
		{
			if (!subject.recepteur.HasRoomInBag())
			{
				SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
				orderPackage = Order.Package.OfLocation(Order.Transfer.GoDropAt, Location.LeadingTo(voxel.ToVector3()));
			}
			else if (TerrainRules.Main.CanDigAt(voxel.ToVector3()))
			{
				SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
				orderPackage = Order.Package.OfLocation(Order.ForLabor.GoDig, new SpaceLocation(voxel.ToVector3(), walkable: false));
			}
		}
		else if (actionType == ActionType.Ternary)
		{
			SoundEngine.Play(subject.Sounds.ActionAttack, subject.gameObject);
			Location data2 = Location.Of(groupData.GetFormationDestination(voxel.ToVector3(), subject.gameObject));
			orderPackage = Order.Package.OfLocation(Order.Combat.MoveAttack, data2);
		}
	}

	protected override void ToCraftingStation(CraftingStation crafting)
	{
		SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
		orderPackage = crafting.GetContextualAction(subject);
	}

	protected override void ToCatapult(CatapultOperations catapult)
	{
		SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
		orderPackage = catapult.ContextualActionFor(subject);
	}

	protected override void ToGameComponent(GameComponent component)
	{
		if (ContextualActor.ShouldAttackByDefault(subject, component) || actionType == ActionType.Secondary)
		{
			SoundEngine.Play(subject.Sounds.ActionAttack, subject.gameObject);
			if (Affiliation.IsEnemy(subject.gameObject, component.gameObject))
			{
				orderPackage = Order.Package.OfGameObject(Order.Combat.GoAttackDirectly, component.gameObject);
			}
			else
			{
				orderPackage = Order.Package.OfGameComponent(Order.Combat.BreakObject, component);
			}
		}
		else
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
			orderPackage = Order.Package.OfGameObject(Order.Move.MoveToVolume, component.gameObject);
		}
	}

	protected override void ToTransferable(Recepteur recepteur)
	{
		SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
		if (actionType == ActionType.Primary)
		{
			orderPackage = Order.Package.OfGameObject(Order.Transfer.SmartTransfer, recepteur.gameObject);
		}
		else
		{
			orderPackage = Order.Package.OfGameObject(Order.Transfer.SmartTransfer_AlternateAction, recepteur.gameObject);
		}
	}

	protected void ToTransferable_alternate(Recepteur recepteur)
	{
		SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
		orderPackage = Order.Package.OfGameObject(Order.Transfer.SmartTransfer_AlternateAction, recepteur.gameObject);
	}

	protected override void ToRessource(GameComponent ressource)
	{
		SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
		orderPackage = Order.Package.OfGameObject(Order.Transfer.GoPickUpOne, ressource.gameObject);
	}

	protected override void ToTrap(BearTrapTrigger trap)
	{
		if (actionType == ActionType.Primary)
		{
			SoundEngine.Play(subject.Sounds.Go, subject.gameObject);
			orderPackage = Order.Package.OfGameObject(Order.Transfer.GoPickUpOne, trap.gameObject);
		}
		else
		{
			Var<OrderPackage<Labor>> var = new Var<OrderPackage<Labor>>();
			trap.GetComponent<ObjectActivator>().GetInstruction(subject, var);
			orderPackage = var.Get();
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

	protected override void ToTree(XYZ voxel)
	{
		SoundEngine.Play(subject.Sounds.ActionLumber, subject.gameObject);
		orderPackage = Order.Package.OfLocation(Order.ForLabor.GoCutNewTree, Location.Of(voxel));
	}

	protected override void ToNature(XYZ voxel)
	{
		NatureData natureData = NatureRegistry.NatureAt(voxel);
		if (!natureData.IsValid || (natureData is PlantData && actionType != ActionType.Secondary))
		{
			ToTerrain(voxel);
			return;
		}
		SoundEngine.Play(subject.Sounds.ActionLumber, subject.gameObject);
		orderPackage = Order.Package.OfVector3(Order.ForLabor.HarvestNatureAt, natureData.GoalPosition);
	}
}
