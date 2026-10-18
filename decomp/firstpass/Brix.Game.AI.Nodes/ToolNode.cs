using System;
using System.Collections.Generic;
using Brix.Audio;
using Brix.External.Factories.Templates;
using Brix.Game.Components;
using Brix.Game.Rules;
using Brix.Game.Semantique;
using Brix.Utils;
using Brix.Utils.Factories;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class ToolNode
{
	public static Node GoEquipTool(this Labor labor, GameObject item, bool storeFirst)
	{
		Node n = (StaticNode)Node.Sequence.Do((Labor aLabor, GameObject aItem) => aLabor.RunToExactly(Location.Of(aItem)), labor, item).Do((Func<Labor, GameObject, Node>)UnequipToolsIfNeeded, labor, item).Do(delegate(Labor aLabor, GameObject aItem)
		{
			aLabor.Profession.toolbag.UnequipTool(aItem.GetComponent<Grip>().PropSlot, Tool.OnDropBehaviours.Drop);
		}, labor, item)
			.Do((Labor aLabor, GameObject aItem) => aLabor.GetComponent<Toolbag>().EquipTool(aItem.gameObject), labor, item)
			.Label("Equip");
		return n.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get).Condition(GameComponentExtender.IsNullOrReleased, item);
	}

	public static Node UnequipToolsIfNeeded(this Labor labor, GameObject item)
	{
		return (!GeneralRules.AllowMixedToolSets) ? labor.StoreOrDropUnrelatedTools(item, store: false) : Node.Empty;
	}

	public static Node UnequipTools(this Labor labor)
	{
		return (StaticNode)Node.Sequence.Do((Labor aLabor) => aLabor.HaltWhatever(), labor).Do(delegate(Labor aLabor)
		{
			aLabor.DropAllTools();
		}, labor);
	}

	public static Node UnequipHandTool(this Labor labor)
	{
		return (StaticNode)Node.Sequence.Do((Labor aLabor) => aLabor.HaltWhatever(), labor).Do(delegate(Labor aLabor)
		{
			aLabor.Profession.toolbag.UnequipTool(Slot.RightHand, Tool.OnDropBehaviours.Release);
		}, labor);
	}

	public static Node SpawnEquip(this Labor labor, FactoryTemplate template, bool storeFirst)
	{
		return (StaticNode)Node.Selector.When((FactoryTemplate aTemplate) => aTemplate == null, template).Label("Resource is null").Do(Node.FailWork<Exceptions.NullTarget>())
			.Label("Fail")
			.Else()
			.Do((Labor aLabor, FactoryTemplate aTemplate, bool aStoreFirst) => aLabor.StoreOrDropUnrelatedTools(aTemplate.gameObject, aStoreFirst), labor, template, storeFirst)
			.Do((FactoryTemplate aTemplate) => Toolbag.SpawnTool(aTemplate), template)
			.Process((GameObject tool, Labor aLabor) => aLabor.Profession.toolbag.EquipTool(tool), labor);
	}

	public static Node GoEquipBestToolOfType(this Labor labor, Accessoire type)
	{
		if (labor.Profession.toolbag.IsMagical)
		{
			FactoryTemplate factoryTemplate = Toolbag.SelectBestTool(type);
			if (!labor.Profession.toolbag.HasTool(factoryTemplate.AssetKey))
			{
				return Node.Do((Labor aLabor, FactoryTemplate aTool) => aLabor.SpawnEquip(aTool, storeFirst: true), labor, factoryTemplate);
			}
			return Node.Empty;
		}
		GameObject gameObject = labor.BestToolToPickup(type);
		if ((bool)gameObject && !labor.Profession.toolbag.HasTool(gameObject))
		{
			return Node.Do((Labor aLabor, GameObject aBestTool) => aLabor.GoEquipTool(aBestTool, storeFirst: true), labor, gameObject);
		}
		return Node.FailWork<Exceptions.ToolUnfindable>();
	}

	public static StaticNode GoEquipBestToolFromRack(this Labor labor, Accessoire type)
	{
		Func<Labor, bool> func = (Labor l) => l.Occupation.CurrentOccupation != Occupation.Type.Builder;
		if (func(labor))
		{
			return Node.Empty;
		}
		Sequencer.Builder n = Node.Sequence.Do((Labor aLabor, Accessoire aType) => aLabor.BestToolToPickup(aType), labor, type).Process((GameObject rackGo, Labor aLabor) => (!(rackGo == null)) ? aLabor.GoEquipRackContent(rackGo) : Node.FailWork<Exceptions.ToolUnfindable>(), labor);
		Catcher.Builder b = Node.Catcher.Do(n).Catch((Func<Exceptions.ToolUnfindable, Node>)Node.Fail).Catch((Exceptions.WorkFailedException e) => Node.Continue());
		return b.Loop().Until(func, labor).ToNode;
	}

	public static StaticNode GoStoreTools(this Labor labor, Accessoire type)
	{
		Func<Labor, bool> func = (Labor l) => l.Occupation.CurrentOccupation == Occupation.Type.Builder;
		if (func(labor))
		{
			return Node.Empty;
		}
		Sequencer.Builder n = Node.Sequence.Do((Labor _labor, Accessoire _type) => _labor.BestToolRackToStore(_type), labor, type).Process((GameObject toolRack, Labor _labor) => (!(toolRack == null)) ? _labor.GoStoreToolsInRack(toolRack) : Node.FailWork<Exceptions.CouldNotFindStorageForInventory>(), labor);
		Catcher.Builder b = Node.Catcher.Do(n).Catch((Func<Exceptions.CouldNotFindStorageForInventory, Node>)Node.Fail).Catch((Exceptions.WorkFailedException e) => Node.Continue());
		return b.Loop().Until(func, labor).ToNode;
	}

	public static Node GoEquipRackContent(this Labor labor, GameObject item)
	{
		Node n = (StaticNode)Node.Sequence.Do((Labor aLabor, GameObject aItem) => aLabor.RunToExactly(Location.Of(aItem)), labor, item).Do((Func<Labor, StaticNode>)DropNode.Drop, labor).Do((Func<Labor, Node>)FailIfNotEmpty, labor)
			.Do(Node.WaitForTicks(10))
			.Do(delegate(Labor aLabor, GameObject aItem)
			{
				aLabor.EquipRackContent(aItem);
			}, labor, item)
			.Label("Equip");
		return n.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get).Condition(GameComponentExtender.IsNullOrReleased, item).FailWhen(SharedWorkException<Exceptions.ObjectHasBeenMovedFromRecepteur>.Get)
			.Condition((Labor l, GameObject i) => !l.CanEquipRackContent(i), labor, item);
	}

	public static Node FailIfNotEmpty(Labor labor)
	{
		return (!labor.recepteur.IsEmpty()) ? Node.Fail(SharedWorkException<Exceptions.InventoryIsFull>.Get) : Node.Empty;
	}

	public static Node GoStoreToolsInRack(this Labor labor, GameObject item)
	{
		Node n = (StaticNode)Node.Sequence.Do((Labor aLabor, GameObject aItem) => aLabor.RunToExactly(Location.Of(aItem)), labor, item).Do((Labor aLabor, GameObject aItem) => UtilityNodes.FailOnFalse<Exceptions.StorageBecameFull, Labor, GameObject>(StoreToolsIn, aLabor, aItem), labor, item);
		return n.FailWhen(SharedWorkException<Exceptions.ToolUnfindable>.Get).Condition(GameComponentExtender.IsNullOrReleased, item);
	}

	private static bool StoreToolsIn(this Labor labor, GameObject rackGo)
	{
		ToolRack component = rackGo.GetComponent<ToolRack>();
		if (component == null)
		{
			return false;
		}
		if (!component.HasRoomForEquipmentOf(labor))
		{
			return false;
		}
		bool result = true;
		List<GameObject> result2 = CollectionPool.RequestList<GameObject>();
		labor.Profession.toolbag.Tools(ref result2);
		foreach (GameObject item in result2)
		{
			if (!component.StoreFromToolBag(item))
			{
				result = false;
			}
		}
		result2.Dispose();
		labor.Profession.toolbag.CompileToolStatistics();
		return result;
	}

	public static bool CanEquipRackContent(this Labor labor, GameObject rackGo)
	{
		if (!labor.faction.IsSame(rackGo))
		{
			return false;
		}
		return rackGo.WithComponent((ToolRack rack, Labor aLabor) => rack.HasEnoughTools() && rack.IsCompatibleWith(aLabor.Occupation.CurrentOccupation), _default: false, labor);
	}

	public static bool CanStoreToolOnRack(this Labor labor, GameObject _rack)
	{
		if (!labor.faction.IsSame(_rack))
		{
			return false;
		}
		return _rack.WithComponent((ToolRack r, Labor l) => r.recepteur.HasRoomForAllOf(l.Profession.toolbag) && r.HasRoomForKit(), _default: false, labor);
	}

	private static void EquipRackContent(this Labor labor, GameObject _rack)
	{
		ToolRack component = _rack.GetComponent<ToolRack>();
		if (component == null)
		{
			return;
		}
		SoundEngine.Play(labor.Sounds.Unequip, labor.gameObject);
		Toolbag toolbag = labor.Profession.toolbag;
		List<GameObject> gos = CollectionPool.RequestList<GameObject>();
		if (component.GetKit(ref gos))
		{
			foreach (GameObject item in gos)
			{
				if (toolbag.CanEquip(item))
				{
					toolbag.EquipTool(item.gameObject);
					toolbag.DropUnrelatedTools(item);
				}
			}
		}
		gos.Dispose();
		toolbag.CompileToolStatistics();
		SoundEngine.Play(labor.Sounds.Equip, labor.gameObject);
	}

	public static Node StoreOrDropUnrelatedTools(this Labor labor, GameObject dropItemsDifferentThanThis, bool store)
	{
		if (!store)
		{
			return Node.Do(delegate(Labor aLabor, GameObject aExcept)
			{
				aLabor.DropUnrelatedTools(aExcept);
			}, labor, dropItemsDifferentThanThis);
		}
		List<GameObject> results = CollectionPool.RequestList<GameObject>();
		if (labor.Profession.toolbag.UnrelatedTools(dropItemsDifferentThanThis, ref results))
		{
			GameObject toolToDrop = null;
			StaticNode b = Node.Sequence.Do((List<GameObject> aUnrelatedTools) => toolToDrop = aUnrelatedTools.Slinq().FirstOrDefault(), results).Do((Func<Labor, GameObject, StaticNode>)GoStoreToolOrDropIfFailed, labor, toolToDrop).Do((List<GameObject> aUnrelatedTools, GameObject aToolToDrop) => aUnrelatedTools.Remove(aToolToDrop), results, toolToDrop)
				.Finally(delegate(List<GameObject> aUnrelatedTools)
				{
					aUnrelatedTools.Dispose();
				}, results);
			return (StaticNode)b.Loop().While((List<GameObject> aUnrelatedTools) => aUnrelatedTools.Slinq().Any(), results);
		}
		results.Dispose();
		return Node.Empty;
	}

	public static StaticNode GoStoreToolOrDropIfFailed(this Labor labor, GameObject toolToStore)
	{
		return Node.Trier.Do(labor.GoStoreTool(toolToStore)).Do(delegate(Labor aLabor, GameObject tool)
		{
			aLabor.DropTool(tool);
		}, labor, toolToStore);
	}

	public static StaticNode GoStoreTool(this Labor labor, GameObject toolToStore)
	{
		return Node.Sequence.Do((Labor aLabor, GameObject aTools) => aLabor.BestRecepteurToStore(aTools), labor, toolToStore).Process((GameObject dstGo, Labor aLabor) => Node.Selector.If((GameObject aDstGo) => aDstGo == null, dstGo).Do(Node.FailWork<Exceptions.CouldNotFindStorageForInventory>()).Else()
			.Do((Func<Labor, GameObject, Node>)GoStoreToolsInRack, aLabor, dstGo), labor);
	}

	public static void DropTool(this Labor labor, GameObject toolToDrop)
	{
		if (labor.Profession.toolbag.HasTool(toolToDrop))
		{
			labor.Profession.toolbag.UnequipTool(toolToDrop, Tool.OnDropBehaviours.Drop);
		}
	}

	public static void DropUnrelatedTools(this Labor labor, GameObject item)
	{
		labor.Profession.toolbag.DropUnrelatedTools(item);
	}

	public static void DropAllTools(this Labor labor)
	{
		labor.Profession.toolbag.DropAllTools();
	}
}
