using System;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Motus.Behavior;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class PickUpNode
{
	public static StaticNode GoPickUp(this Labor labor, GameComponent ressource, int upTo = 1)
	{
		return Node.Sequence.Do((Func<Labor, GameComponent, int, Node>)_GoPickUp, labor, ressource, upTo);
	}

	private static IReadOnlySignal GetConvertedSignal(GameComponent component)
	{
		if (component.ParentRecepteur.IsNullOrReleased())
		{
			return NullSignal.Get;
		}
		return component.ParentRecepteur.ConvertedResources;
	}

	private static Node _GoPickUp(this Labor labor, GameComponent itemToPickUp, int upTo = 1)
	{
		if (itemToPickUp == null)
		{
			return Node.FailWork<Exceptions.CannotFindWhatINeedToPickUp>();
		}
		if (labor.recepteur.Carries(itemToPickUp))
		{
			return Node.Empty;
		}
		Recepteur parentRecepteur = itemToPickUp.ParentRecepteur;
		GameComponent p = parentRecepteur ?? itemToPickUp;
		Node n = (StaticNode)Node.Sequence.Do(delegate(Labor _labor, GameComponent _target)
		{
			Knowledge.Instance.ReserveUntil(_target.gameObject, _labor.signal.WorkEnded);
		}, labor, p).Label("Reserve Until", labor).Do((Labor _labor, GameComponent t) => _labor.RunToExactly(Location.Of(t)), labor, p)
			.Do((Func<Labor, GameComponent, int, Node>)TakeSome, labor, itemToPickUp, upTo)
			.Label("Pick up " + itemToPickUp.name);
		return n.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get).Condition(GameComponentExtender.IsNullOrReleased, itemToPickUp).FailWhen(SharedWorkException<Exceptions.ObjectDoesNotFitInDestination>.Get)
			.Condition((Tuple<Labor, GameComponent> t) => !t._1.recepteur.HasRoomFor(t._2), Tuple.Create(labor, itemToPickUp))
			.FailWhen(SharedWorkException<Exceptions.ObjectHasBeenMovedFromRecepteur>.Get)
			.Condition((Tuple<GameComponent, Recepteur> t) => t._1.ParentRecepteur != t._2, Tuple.Create(itemToPickUp, parentRecepteur))
			.FailWhen(SharedWorkException<Exceptions.ObjectIsInSomeoneElseBag>.Get)
			.Condition((Recepteur r) => (bool)r && r.Fonction == FonctionDeContenant.personnage, parentRecepteur)
			.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get)
			.Signal(GetConvertedSignal(itemToPickUp))
			.FailWhen(SharedWorkException<Exceptions.ObjectHasBeenConverted>.Get)
			.Condition(GameComponentExtender.IsNullOrReleased, itemToPickUp);
	}

	public static Node FindAndPickUp(this Labor labor, Profile targetProfile, int upTo = 1)
	{
		return labor.NodeFindAndPickUp(upTo, labor.Find(targetProfile));
	}

	public static Node FindAndPickUp(this Labor labor, Recepteur toFill, int upTo = 20)
	{
		return labor.NodeFindAndPickUp(upTo, labor.Find(toFill));
	}

	public static Node FindAndPickUp(this Labor labor, Description toFill, int upTo = 20)
	{
		return labor.NodeFindAndPickUp(upTo, labor.Find(toFill));
	}

	private static Node NodeFindAndPickUp(this Labor labor, int upTo, GameObject item)
	{
		if (item.IsNullOrReleased())
		{
			return Node.FailWork<Exceptions.CannotFindWhatINeedToPickUp>();
		}
		return labor.GoPickUp(item.GetComponent<GameComponent>(), upTo);
	}

	public static Node TakeSome(this Labor labor, GameComponent itemToTake, int upTo = -1)
	{
		if ((bool)itemToTake.GetComponentInParent<Blueprint>())
		{
			return Node.Fail(SharedWorkException<Exceptions.CannotFindWhatINeedToPickUp>.Get);
		}
		labor.transaction.StoreEverything(itemToTake.gameObject, labor.gameObject, upTo);
		return Node.Empty;
	}

	public static Node GoPickUpAmmo(this Labor labor, GameComponent target)
	{
		if (target.IsNullOrReleased())
		{
			return Node.FailWork<Exceptions.CannotFindWhatINeedToPickUp>();
		}
		GameObject tool = labor.toolbag.GetTool(Accessories.Backpack);
		if (tool == null)
		{
			return Node.FailWork<Exceptions.ObjectDoesNotFitInDestination>();
		}
		Node n = (StaticNode)Node.Sequence.Do((Labor _labor, GameComponent t) => _labor.RunToExactly(Location.Of(t)), labor, target).Do(delegate(Labor _labor, GameComponent _item, GameObject _wepon)
		{
			_labor.transaction.ConvertToAmmo(_wepon.GetComponent<ProjectileWeapon>(), _item.recepteur);
		}, labor, target, tool).Label("Pick up ammo" + target.name);
		return n.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get).Condition(GameComponentExtender.IsNullOrReleased, target).FailWhen(SharedWorkException<Exceptions.ObjectHasBeenMovedFromRecepteur>.Get)
			.Condition((Tuple<GameComponent> t) => t._1.recepteur.IsEmpty(), Tuple.Create(target));
	}
}
