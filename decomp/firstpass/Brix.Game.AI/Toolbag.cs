using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Utils;
using Brix.Utils.Factories;
using MoonSharp.Interpreter;
using Smooth.Algebraics;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

[DisallowMultipleComponent]
public class Toolbag : MonoBehaviour, IGripTracker, IObjectTracker
{
	[Serializable]
	public class ToolLimbPair
	{
		public GripSocket Limb;

		public GameObject Tool;

		public bool Occupied => !Tool.IsNullOrReleased() && Tool.transform.parent == Limb.transform;
	}

	public ToolLimbPair Head = new ToolLimbPair();

	public ToolLimbPair Neck = new ToolLimbPair();

	public ToolLimbPair Front = new ToolLimbPair();

	public ToolLimbPair RightHand = new ToolLimbPair();

	public ToolLimbPair LeftHand = new ToolLimbPair();

	public ToolLimbPair Back = new ToolLimbPair();

	public ToolLimbPair RightFoot = new ToolLimbPair();

	public ToolLimbPair LeftFoot = new ToolLimbPair();

	public Signal<Description> OnToolStatiticChanged = new Signal<Description>();

	private Signal<GameObject> toolEquipedSignal = new Signal<GameObject>();

	private Signal<GameObject> toolUnequipedSignal = new Signal<GameObject>();

	public bool IsMagical;

	private Description _toolStatistics;

	public IReadOnlySignal<GameObject> OnToolEquiped => toolEquipedSignal;

	public IReadOnlySignal<GameObject> OnToolUnequiped => toolUnequipedSignal;

	public Description ToolStatistics
	{
		get
		{
			if (_toolStatistics == null)
			{
				CompileToolStatistics();
			}
			return _toolStatistics;
		}
	}

	public void Start()
	{
		CompileToolStatistics();
	}

	public GameObject ToolAt(Slot slot)
	{
		ToolLimbPair toolLimbPair = GetToolLimbPair(slot);
		return toolLimbPair.Tool;
	}

	public void EquipMagicalTool(Factory.AssetKey fromKey)
	{
		if (fromKey == null || fromKey.IsEmpty())
		{
			UnequipTool(Slot.RightHand, Tool.OnDropBehaviours.Drop);
		}
		else if (!HasTool(fromKey) && !HasRealToolAt(Tool.Slot(fromKey)))
		{
			EquipTool(SpawnTool(fromKey, magical: true).gameObject);
		}
	}

	public void EquipTool(Factory.AssetKey fromKey)
	{
		if (!HasTool(fromKey) && !HasRealToolAt(Tool.Slot(fromKey)))
		{
			EquipTool(SpawnTool(fromKey, magical: false).gameObject);
		}
	}

	public void EquipTools(List<Factory.AssetKey> fromKeys)
	{
		fromKeys.Slinq().ForEach(delegate(Factory.AssetKey k, Toolbag tb)
		{
			tb.EquipTool(k);
		}, this);
	}

	private Tool SpawnTool(Factory.AssetKey fromKey, bool magical)
	{
		ObjectPoolSingleton.RequestComponent<Tool>(out var t, fromKey);
		if (NetworkServer.active)
		{
			NetworkServer.Spawn(t.gameObject, fromKey.AssetId);
		}
		t.NetworkMagical = magical;
		return t;
	}

	public GameObject EquipTool(Factory.AssetKey fromKey, Slot atSlot)
	{
		GameObject go = null;
		if (fromKey != null)
		{
			ObjectPoolSingleton.Request(out go, fromKey, active: true);
			NetworkServer.Spawn(go, fromKey.AssetId);
			go.GetComponentInChildren<Grip>().PropSlot = atSlot;
		}
		return EquipTool(go);
	}

	public GameObject EquipTool(GameObject tool)
	{
		AddObject(tool);
		return tool;
	}

	private void OnEquipTool(GameObject item)
	{
		Slot slot = Grip.SlotOf(item);
		if (slot != Slot.Invalid)
		{
			ToolLimbPair toolLimbPair = GetToolLimbPair(slot);
			if (toolLimbPair.Tool != item)
			{
				UnequipTool(toolLimbPair.Tool, Tool.OnDropBehaviours.Release);
				toolLimbPair.Tool = item;
			}
			item.GetComponentInChildren<Grip>().AlignToSocket(toolLimbPair.Limb);
			EnablePhysics(item, value: false);
			item.SetActive(value: true);
			item.GetComponent<Tool>().CurrentUser = base.gameObject;
			CompileToolStatistics();
			toolEquipedSignal.Invoke(item);
			item.GetComponent<LocalFactionVision>().UpdateVisibility();
		}
	}

	public void UnequipTool(Slot slot, Tool.OnDropBehaviours onDrop)
	{
		UnequipTool(ToolAt(slot), onDrop);
	}

	public void UnequipTool(GameObject item, Tool.OnDropBehaviours onDrop)
	{
		if (!item.IsNullOrReleased())
		{
			item.GetComponent<Tool>().OnDropBehaviour = onDrop;
			ClasseMoi.Move(item);
		}
	}

	public void UnequipTool(Factory.AssetKey key)
	{
		List<GameObject> result = CollectionPool.RequestList<GameObject>();
		Tools(ref result);
		foreach (GameObject item in result)
		{
			Tool component = item.GetComponent<Tool>();
			if (component.AssetKey == key)
			{
				component.OnDropBehaviour = Tool.OnDropBehaviours.Release;
				ClasseMoi.Move(item);
			}
		}
		result.Dispose();
	}

	private void EnablePhysics(GameObject item, bool value)
	{
		Rigidbody component = item.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.detectCollisions = value;
			component.isKinematic = !value;
		}
	}

	private void OnUnequipTool(GameObject item)
	{
		Slot slot = Grip.SlotOf(item);
		if (slot != Slot.Invalid)
		{
			ToolLimbPair toolLimbPair = GetToolLimbPair(slot);
			toolLimbPair.Tool = null;
			toolLimbPair.Limb.Collapse(collapse: false);
			Tool component = item.GetComponent<Tool>();
			if (component.OnDropBehaviour == Tool.OnDropBehaviours.Drop)
			{
				EnablePhysics(item, value: true);
			}
			component.CurrentUser = null;
			component.OnDrop();
			CompileToolStatistics();
			toolUnequipedSignal.Invoke(item);
		}
	}

	public bool HasTool(Slot atSlot)
	{
		return GetToolLimbPair(atSlot).Occupied;
	}

	public bool HasRealToolAt(Slot atSlot)
	{
		ToolLimbPair toolLimbPair = GetToolLimbPair(atSlot);
		return toolLimbPair.Occupied && toolLimbPair.Tool.WithComponent((Tool t) => !t.Magical, _default: false);
	}

	public bool CanEquip(GameObject go)
	{
		Grip component = go.GetComponent<Grip>();
		ToolLimbPair toolLimbPair = GetToolLimbPair(component.PropSlot);
		return !toolLimbPair.Occupied || toolLimbPair.Tool.GetComponent<Tool>().Magical;
	}

	public bool HasTool(GameObject tool)
	{
		for (int i = 0; i <= 7; i++)
		{
			ToolLimbPair toolLimbPair = null;
			if (i == 0)
			{
				toolLimbPair = Head;
			}
			if (i == 1)
			{
				toolLimbPair = RightHand;
			}
			if (i == 2)
			{
				toolLimbPair = LeftHand;
			}
			if (i == 3)
			{
				toolLimbPair = Back;
			}
			if (i == 4)
			{
				toolLimbPair = RightFoot;
			}
			if (i == 5)
			{
				toolLimbPair = LeftFoot;
			}
			if (i == 6)
			{
				toolLimbPair = Neck;
			}
			if (i == 7)
			{
				toolLimbPair = Front;
			}
			if (toolLimbPair.Occupied && toolLimbPair.Tool == tool)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTool(Factory.AssetKey key)
	{
		for (int i = 0; i <= 7; i++)
		{
			ToolLimbPair toolLimbPair = null;
			if (i == 0)
			{
				toolLimbPair = Head;
			}
			if (i == 1)
			{
				toolLimbPair = RightHand;
			}
			if (i == 2)
			{
				toolLimbPair = LeftHand;
			}
			if (i == 3)
			{
				toolLimbPair = Back;
			}
			if (i == 4)
			{
				toolLimbPair = RightFoot;
			}
			if (i == 5)
			{
				toolLimbPair = LeftFoot;
			}
			if (i == 6)
			{
				toolLimbPair = Neck;
			}
			if (i == 7)
			{
				toolLimbPair = Front;
			}
			if (toolLimbPair.Occupied && toolLimbPair.Tool.GetComponent<FactoryImprint>().AssetKey == key)
			{
				return true;
			}
		}
		return false;
	}

	public GameObject GetTool(Factory.AssetKey key)
	{
		for (int i = 0; i <= 7; i++)
		{
			ToolLimbPair toolLimbPair = null;
			if (i == 0)
			{
				toolLimbPair = Head;
			}
			if (i == 1)
			{
				toolLimbPair = RightHand;
			}
			if (i == 2)
			{
				toolLimbPair = LeftHand;
			}
			if (i == 3)
			{
				toolLimbPair = Back;
			}
			if (i == 4)
			{
				toolLimbPair = RightFoot;
			}
			if (i == 5)
			{
				toolLimbPair = LeftFoot;
			}
			if (i == 6)
			{
				toolLimbPair = Neck;
			}
			if (i == 7)
			{
				toolLimbPair = Front;
			}
			if (toolLimbPair.Occupied && toolLimbPair.Tool.GetComponent<FactoryImprint>().AssetKey == key)
			{
				return toolLimbPair.Tool;
			}
		}
		return null;
	}

	public bool HasWepon()
	{
		return IsWepon(RightHand) || IsWepon(LeftHand);
	}

	public bool IsWepon(ToolLimbPair pair)
	{
		return pair.Occupied && pair.Tool.GetComponent<Tool>().IsWeapon();
	}

	public void CompileToolStatistics()
	{
		_toolStatistics = new Description();
		int num = 0;
		List<GameObject> result = CollectionPool.RequestList<GameObject>();
		Tools(ref result);
		foreach (GameObject item in result)
		{
			Apparence component = item.GetComponent<Apparence>();
			if (component != null)
			{
				_toolStatistics += component.Description;
			}
			num += item.WithComponent((ProjectileWeapon w) => w.AmmoCountGetter, 0);
		}
		GetComponent<CharacterState>().AmmoCount = num;
		result.Dispose();
		Profession profession = GetComponent<Labor>().Occupation.Profession;
		profession.SetHpBonus(_toolStatistics.Get(Adjectif.protection)?.valeur ?? 0);
		profession.SetStaminaBonus(_toolStatistics.Get(Adjectif.staminaBonus)?.valeur ?? 0);
		OnToolStatiticChanged.Invoke(_toolStatistics);
		profession.CapacityChanged.Invoke();
	}

	public bool HasToolFor(Adjectif adjectif, out GameObject tool)
	{
		tool = null;
		for (int i = 0; i <= 7; i++)
		{
			ToolLimbPair toolLimbPair = null;
			if (i == 0)
			{
				toolLimbPair = Head;
			}
			if (i == 1)
			{
				toolLimbPair = RightHand;
			}
			if (i == 2)
			{
				toolLimbPair = LeftHand;
			}
			if (i == 3)
			{
				toolLimbPair = Back;
			}
			if (i == 4)
			{
				toolLimbPair = RightFoot;
			}
			if (i == 5)
			{
				toolLimbPair = LeftFoot;
			}
			if (i == 6)
			{
				toolLimbPair = Neck;
			}
			if (i == 7)
			{
				toolLimbPair = Front;
			}
			if (toolLimbPair != null && toolLimbPair.Occupied && !toolLimbPair.Tool.IsNullOrReleased())
			{
				Apparence component = toolLimbPair.Tool.GetComponent<Apparence>();
				if (component != null && component.description.Has(adjectif))
				{
					tool = toolLimbPair.Tool;
					return true;
				}
			}
		}
		return false;
	}

	public void Tools(ref List<GameObject> result, bool includeNullTools = false, bool includeAesthetic = false)
	{
		for (int i = 0; i <= 7; i++)
		{
			ToolLimbPair toolLimbPair = null;
			if (i == 0)
			{
				toolLimbPair = Head;
			}
			if (i == 1)
			{
				toolLimbPair = RightHand;
			}
			if (i == 2)
			{
				toolLimbPair = LeftHand;
			}
			if (i == 3)
			{
				toolLimbPair = Back;
			}
			if (i == 4)
			{
				toolLimbPair = RightFoot;
			}
			if (i == 5)
			{
				toolLimbPair = LeftFoot;
			}
			if (i == 6)
			{
				toolLimbPair = Neck;
			}
			if (i == 7)
			{
				toolLimbPair = Front;
			}
			if (toolLimbPair.Occupied)
			{
				bool flag = toolLimbPair.Tool.WithComponent((Tool t) => t.Aesthetic, _default: false);
				if (includeAesthetic || !flag)
				{
					result.Add(toolLimbPair.Tool);
				}
			}
			else if (includeNullTools)
			{
				result.Add(null);
			}
		}
	}

	public ToolLimbPair GetToolLimbPair(Slot ofSlot)
	{
		return ofSlot switch
		{
			Slot.RightHand => RightHand, 
			Slot.LeftHand => LeftHand, 
			Slot.Head => Head, 
			Slot.Back => Back, 
			Slot.RightFoot => RightFoot, 
			Slot.LeftFoot => LeftFoot, 
			Slot.Neck => Neck, 
			Slot.Front => Front, 
			_ => null, 
		};
	}

	public Transform ContainerFor(GameObject itemGO)
	{
		Slot slot = Grip.SlotOf(itemGO);
		if (slot != Slot.Invalid)
		{
			return GetToolLimbPair(slot).Limb.transform;
		}
		return null;
	}

	public void ObjectUntracked(GameObject itemGO)
	{
		OnUnequipTool(itemGO);
	}

	public void OnObjectTracked(GameObject itemGO)
	{
		OnEquipTool(itemGO);
	}

	public void AddObject(GameObject itemGO)
	{
		ToolParentTracker component = itemGO.GetComponent<ToolParentTracker>();
		if ((bool)component)
		{
			itemGO.GetComponent<ToolParentTracker>().SetTrackerType(TrackingTypes.grip);
		}
		IObjectTrackerExtender.AddObject(this, itemGO);
	}

	public static GameObject SpawnTool(Adjectif accessoire, bool magical = true, bool onlyWorkerTools = true)
	{
		FactoryTemplate factoryTemplate = SelectBestTool(accessoire, onlyWorkerTools);
		return (!(factoryTemplate == null)) ? SpawnTool(factoryTemplate, magical) : null;
	}

	public static GameObject SpawnTool(FactoryTemplate template, bool magical = true)
	{
		ObjectPoolSingleton.Request(out var go, template.AssetKey);
		NetworkServer.Spawn(go, template.AssetKey.AssetId);
		go.GetComponent<Tool>().NetworkMagical = magical;
		return go;
	}

	public static FactoryTemplate SelectBestTool(Adjectif accessoire, bool onlyWorkerTools = true)
	{
		Factory factory = Factory.GetFactory("Accessories");
		Tuple<FactoryTemplate, int> tuple = default(Tuple<FactoryTemplate, int>);
		foreach (FactoryTemplate template in factory.Templates)
		{
			Apparence component = template.gameObject.GetComponent<Apparence>();
			if (!(component == null) && accessoire.PeutPrendre(component.Description) == ReponseContrainte.Positif && (!onlyWorkerTools || component.Description.Has(Adjectif.workerTool)))
			{
				int valeur = component.Description.Get(accessoire).quantifiable.valeur;
				if (valeur > tuple._2)
				{
					tuple = new Tuple<FactoryTemplate, int>(template, valeur);
				}
			}
		}
		return tuple._1;
	}

	public bool UnrelatedTools(GameObject tool, ref List<GameObject> results)
	{
		Tools(ref results);
		results.Slinq().Where(Tool.IsCompatible, tool).RemoveAll();
		return results.Count > 0;
	}

	public void DropUnrelatedTools(GameObject tool)
	{
		List<GameObject> results = CollectionPool.RequestList<GameObject>();
		UnrelatedTools(tool, ref results);
		results.Slinq().ForEach(delegate(GameObject t, Toolbag self)
		{
			self.UnequipTool(t, Tool.OnDropBehaviours.Drop);
		}, this);
		results.Dispose();
	}

	public void DropAllTools()
	{
		List<GameObject> result = CollectionPool.RequestList<GameObject>();
		Tools(ref result);
		result.Slinq().ForEach(delegate(GameObject t, Toolbag self)
		{
			self.UnequipTool(t, Tool.OnDropBehaviours.Drop);
		}, this);
		result.Dispose();
	}

	public void UpdateToolsVision()
	{
		List<GameObject> result = CollectionPool.RequestList<GameObject>();
		Tools(ref result);
		foreach (GameObject item in result)
		{
			NetworkIdentity component = item.GetComponent<NetworkIdentity>();
			if (!component.netId.IsEmpty())
			{
				component.RebuildObservers(initialize: false);
			}
		}
		result.Dispose();
	}

	[LuaDrivable("EquipTool")]
	private static DynValue lua_EquipTool(ScriptExecutionContext context, CallbackArguments args)
	{
		Toolbag toolbag = args.AsComponent<Toolbag>(0, "EquipTool");
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(1, "EquipTool");
		toolbag.EquipTool(luaStaticDriver.AssetKey);
		return DynValue.Void;
	}

	GameObject IObjectTracker.get_gameObject()
	{
		return base.gameObject;
	}
}
