using System;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.CatapultSpace;
using Brix.Game.Semantique;
using Brix.Legacy;
using Brix.UI.GameSelector;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Input.ContextualOrders;

public abstract class ContextualActor
{
	protected Labor subject;

	protected OrderPackage<Labor> orderPackage;

	protected ActionType actionType;

	protected bool queue;

	protected IconKey iconKey;

	protected ContextualActionGroupData groupData;

	public ContextualActor(Labor oe, ContextualActionGroupData groupData, ActionType type, bool toQueue)
	{
		subject = oe;
		actionType = type;
		queue = toQueue;
		this.groupData = groupData;
	}

	public void SendOrder()
	{
		if (orderPackage != null)
		{
			orderPackage.DecisionSource = Labor.DecisionSource.User;
			subject.AssignToIdle();
			if (queue)
			{
				orderPackage.Queue(subject);
			}
			else
			{
				orderPackage.Send(subject);
			}
		}
	}

	public void Prepare(object target)
	{
		Vector3? vector = target as Vector3?;
		GameObject go;
		if (vector.HasValue)
		{
			XYZ xYZ = XYZ.FromVector3(vector.Value);
			if (TreeRegistry.Instance.HasTree(xYZ))
			{
				ToTree(xYZ);
			}
			else if (NatureRegistry.HasNatureAt(xYZ))
			{
				ToNature(xYZ);
			}
			else
			{
				ToTerrain(xYZ);
			}
		}
		else if (GetTarget(target, out go) && !CallOnComponent<Labor>(go, ToUnit))
		{
			if (go.GetComponent<GameComponent>() != null && Affiliation.IsEnemy(subject.gameObject, target as GameObject))
			{
				ToGameComponent(go.GetComponent<GameComponent>());
			}
			else if (!CallOnComponent<CatapultOperations>(go, ToCatapult) && !CallOnComponent<ToolRack>(go, ToToolRack) && !CallOnComponent<CraftingStation>(go, ToCraftingStation) && !CallOnComponent<BearTrapTrigger>(go, ToTrap) && !CallOnComponent<Tool>(go, ToEquipment) && !CallOnComponent<Recepteur>(go, ToTransferable) && !IfApparence<Ressource>(go, ToRessource) && !CallOnComponent<GameComponent>(go, ToGameComponent))
			{
			}
		}
	}

	private bool GetTarget(object target, out GameObject go)
	{
		go = (GameObject)target;
		if (go == null)
		{
			return false;
		}
		UISelectable uISelectable = UIGameSelector.FindSelectable(go);
		if (uISelectable != null)
		{
			go = uISelectable.gameObject;
		}
		return true;
	}

	private bool IfApparence<T>(GameObject go, Action<GameComponent> action) where T : Adjectif
	{
		if (Apparence.Is<T>(go))
		{
			action(go.GetComponent<GameComponent>());
			return true;
		}
		return false;
	}

	private bool CallOnComponent<T>(GameObject go, Action<T> action)
	{
		T component = go.GetComponent<T>();
		if (component != null)
		{
			action(component);
			return true;
		}
		return false;
	}

	private void IgnoreOrder()
	{
		SoundEngine.Play(subject.Sounds.Impossible, subject.gameObject);
	}

	protected virtual void ToCraftingStation(CraftingStation go)
	{
		IgnoreOrder();
	}

	protected virtual void ToTerrain(XYZ voxel)
	{
		IgnoreOrder();
	}

	protected virtual void ToUnit(Labor unit)
	{
		IgnoreOrder();
	}

	protected virtual void ToCatapult(CatapultOperations catapult)
	{
		IgnoreOrder();
	}

	protected virtual void ToGameComponent(GameComponent component)
	{
		IgnoreOrder();
	}

	protected virtual void ToTree(XYZ voxel)
	{
		IgnoreOrder();
	}

	protected virtual void ToNature(XYZ voxel)
	{
		IgnoreOrder();
	}

	protected virtual void ToTrap(BearTrapTrigger trap)
	{
		ToGameComponent(trap.GetComponent<GameComponent>());
	}

	protected virtual void ToTransferable(Recepteur recepteur)
	{
		ToGameComponent(recepteur);
	}

	protected virtual void ToRessource(GameComponent ressource)
	{
		ToGameComponent(ressource);
	}

	protected virtual void ToEquipment(Tool tool)
	{
		orderPackage = tool.GetContextualAction(subject);
	}

	protected virtual void ToToolRack(ToolRack rack)
	{
		orderPackage = rack.GetContextualActionFor(subject);
	}

	public static bool ShouldAttackByDefault(Labor labor, GameComponent gc)
	{
		return Affiliation.IsEnemy(labor.gameObject, gc.gameObject) && gc.damageReceiver != null && !gc.damageReceiver.IsInvincible;
	}

	public static bool ShouldAttackByDefault(Labor labor, GameObject go)
	{
		return Affiliation.IsEnemy(labor.gameObject, go) && (bool)go.GetComponent<DamageReceiver>() && !go.GetComponent<DamageReceiver>().IsInvincible;
	}
}
