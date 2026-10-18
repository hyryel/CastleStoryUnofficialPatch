using System;
using System.Collections.Generic;
using System.Linq;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Rules;
using Brix.Legacy;
using Brix.Lua;
using Brix.Utils;
using Brix.Utils.Collections;
using MoonSharp.Interpreter;
using Smooth.Algebraics;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Semantique;

public class ToolRack : GameComponent, PieMenuable
{
	public struct KitCount
	{
		public int Weapon;

		public int Prop;

		public int Hat;

		public static KitCount FromDescription(Description desc)
		{
			return new KitCount
			{
				Weapon = Weapons.Slinq().Aggregate(0, (int sum, Ressource adj, Description aDesc) => sum + aDesc.Value(adj), desc),
				Prop = Props.Slinq().Aggregate(0, (int sum, Ressource adj, Description aDesc) => sum + aDesc.Value(adj), desc),
				Hat = Hats.Slinq().Aggregate(0, (int sum, Ressource adj, Description aDesc) => sum + aDesc.Value(adj), desc)
			};
		}

		public int CompleteCount()
		{
			return Mathf.Min(Mathf.Min(Weapon, Prop), Hat);
		}

		public float Fullness()
		{
			return 1 - (4 - CompleteCount()) / 4;
		}

		public bool IsExtra(Outil tool)
		{
			if (Weapon == Prop && Weapon == Hat)
			{
				return false;
			}
			if (Weapons.Contains(tool))
			{
				return Weapon >= Prop && Weapon >= Hat;
			}
			if (Props.Contains(tool))
			{
				return Prop >= Weapon && Prop >= Hat;
			}
			if (Hats.Contains(tool))
			{
				return Hat >= Weapon && Hat >= Prop;
			}
			throw new ArgumentOutOfRangeException();
		}

		public bool IsMissing(Outil tool)
		{
			if (Weapon == Prop && Weapon == Hat)
			{
				return false;
			}
			if (Weapons.Contains(tool))
			{
				return Weapon <= Prop && Weapon <= Hat;
			}
			if (Props.Contains(tool))
			{
				return Prop <= Weapon && Prop <= Hat;
			}
			if (Hats.Contains(tool))
			{
				return Hat <= Weapon && Hat <= Prop;
			}
			throw new ArgumentOutOfRangeException();
		}
	}

	private List<Ressource> _exclusivity = AllItems;

	private static readonly List<Ressource> Weapons = new List<Ressource>
	{
		Adjectif.sword,
		Adjectif.bow,
		Adjectif.crossbowWeapon,
		Adjectif.geomancerWeapon,
		Adjectif.lance,
		Adjectif.halbard
	};

	private static readonly List<Ressource> Hats = new List<Ressource>
	{
		Adjectif.helmet,
		Adjectif.cap,
		Adjectif.crossbowHat,
		Adjectif.alchemistHelmet,
		Adjectif.geomancerHat,
		Adjectif.kettleHat
	};

	private static readonly List<Ressource> Props = new List<Ressource>
	{
		Adjectif.shield,
		Adjectif.quiver,
		Adjectif.kokiriShield,
		Adjectif.slingerProp,
		Adjectif.crossbowProp,
		Adjectif.alchemistPouch,
		Adjectif.geomancerProp
	};

	private static readonly List<Ressource> Knight = new List<Ressource>
	{
		Adjectif.sword,
		Adjectif.shield,
		Adjectif.helmet
	};

	private static readonly List<Ressource> Archer = new List<Ressource>
	{
		Adjectif.bow,
		Adjectif.quiver,
		Adjectif.cap
	};

	private static readonly List<Ressource> Arbalist = new List<Ressource>
	{
		Adjectif.crossbowWeapon,
		Adjectif.crossbowProp,
		Adjectif.crossbowHat
	};

	private static readonly List<Ressource> Alchemist = new List<Ressource>
	{
		Adjectif.alchemistPouch,
		Adjectif.alchemistHelmet
	};

	private static readonly List<Ressource> Artificer = new List<Ressource>
	{
		Adjectif.geomancerWeapon,
		Adjectif.geomancerProp,
		Adjectif.geomancerHat
	};

	private static readonly List<Ressource> Halbardier = new List<Ressource>
	{
		Adjectif.halbard,
		Adjectif.kettleHat
	};

	private static readonly List<List<Ressource>> AllClasses = new List<List<Ressource>> { Knight, Archer, Arbalist, Alchemist, Artificer, Halbardier };

	public static readonly List<Ressource> AllItems = AllClasses.SelectMany((List<Ressource> r) => r).ToList();

	private static readonly Dictionary<CategorieOutil, List<Ressource>> Exclusivity = new Dictionary<CategorieOutil, List<Ressource>>
	{
		{
			Adjectif.knightTool,
			Knight
		},
		{
			Adjectif.archerTool,
			Archer
		},
		{
			Adjectif.crossbowTool,
			Arbalist
		},
		{
			Adjectif.alchemistTool,
			Alchemist
		},
		{
			Adjectif.geomancerTool,
			Artificer
		},
		{
			Adjectif.halbardTool,
			Halbardier
		}
	};

	public const int TOOLRACK_CAPACITY = 4;

	public bool IsForKnight => _exclusivity == Knight;

	public bool IsForArcher => _exclusivity == Archer;

	public bool IsForHalberdier => _exclusivity == Halbardier;

	public bool IsForArbalist => _exclusivity == Arbalist;

	public bool IsForAlchemist => _exclusivity == Alchemist;

	public bool IsForArtificer => _exclusivity == Artificer;

	protected override void Start()
	{
		base.Start();
		base.recepteur.OnResourceAdded.Connect(delegate(IDescriptor aDescriptor, ToolRack aToolRack)
		{
			aToolRack.RefreshExclusivity();
		}, this);
		base.recepteur.OnResourceRemoved.Connect(delegate(IDescriptor aDescriptor, ToolRack aToolRack)
		{
			aToolRack.RefreshExclusivity();
		}, this);
	}

	public OrderPackage<Labor> GetContextualActionFor(Labor labor)
	{
		if (base.recepteur.HasRoomForSomeOf(labor.recepteur))
		{
			return Order.Package.OfGameComponent(Order.Transfer.GoStoreInto, this, Labor.DecisionSource.User);
		}
		if (HasRoomForEquipmentOf(labor) && labor.Profession.OccupationType != Occupation.Type.Builder)
		{
			return Order.Package.OfGameObject(Order.AboutTools.GoStoreInRack, base.gameObject, Labor.DecisionSource.User);
		}
		if (HasFullKit())
		{
			return Order.Package.OfGameObject(Order.AboutTools.GoEquipRackContent, base.gameObject, Labor.DecisionSource.User);
		}
		if (labor.Profession.OccupationType == Occupation.Type.Builder)
		{
			return Order.Package.OfGameObject(Order.Transfer.GoPickUpFrom, base.gameObject, Labor.DecisionSource.User);
		}
		return Order.Package.OfLocation(Order.Move.RunToExactly, Location.Of(this), Labor.DecisionSource.User);
	}

	public static bool Accepts(Ressource adj)
	{
		return AllItems.Contains(adj);
	}

	public bool HasRoomForEquipmentOf(Labor labor)
	{
		return base.recepteur.HasRoomForAllOf(labor.Profession.toolbag) && HasRoomForKit();
	}

	public float PickUpBonusFor(Outil tool)
	{
		KitCount kitCount = KitCount.FromDescription(base.recepteur.ContentDescription);
		return PickupBonuses.ForPickingUp(tool, kitCount.IsExtra(tool), 1f - kitCount.Fullness());
	}

	public float StoreBonusFor(Outil tool)
	{
		KitCount kitCount = KitCount.FromDescription(base.recepteur.ContentDescription);
		return PickupBonuses.ForStoring(tool, kitCount.IsMissing(tool), kitCount.Fullness());
	}

	public bool HasFullKit()
	{
		return AllClasses.Slinq().Any((List<Ressource> items, ToolRack aToolRack) => items.Slinq().All((Ressource adj, Recepteur r) => r.ContentDescription.Value(adj) > 0, aToolRack.recepteur), this);
	}

	public bool HasRoomForKit()
	{
		return AllClasses.Slinq().Any((List<Ressource> items, ToolRack aToolRack) => items.Slinq().Any((Ressource adj, Recepteur r) => r.CurrentCapacity.Value(adj) > 0, aToolRack.recepteur), this);
	}

	public bool HasEnoughTools()
	{
		return HasFullKit() || GeneralRules.AllowPartialToolsets;
	}

	public bool IsCompatibleWith(Occupation.Type type)
	{
		return type == Occupation.Type.Builder || GeneralRules.AllowMixedToolSets;
	}

	public bool StoreFromToolBag(GameObject go)
	{
		Tool component = go.GetComponent<Tool>();
		if (component == null)
		{
			return false;
		}
		StoreFromToolBag(component);
		return true;
	}

	public bool StoreFromToolBag(Tool tool)
	{
		if (base.recepteur.HasRoomFor(tool.gameObject))
		{
			tool.OnDropBehaviour = Tool.OnDropBehaviours.Transfer;
			base.recepteur.AddObject(tool.gameObject);
			RefreshExclusivity();
			return true;
		}
		return true;
	}

	public override bool CheckTag(string tag)
	{
		return tag == "ToolRack";
	}

	public void RefreshExclusivity()
	{
		base.recepteur.FirstObject(out var go);
		List<Ressource> list = ExclusivityFor(go);
		if (list != _exclusivity)
		{
			base.recepteur.MakeExclusive(list);
			_exclusivity = list;
		}
	}

	private List<Ressource> ExclusivityFor(GameObject item)
	{
		if (item != null)
		{
			Option<KeyValuePair<CategorieOutil, List<Ressource>>> option = Exclusivity.Slinq().FirstOrNone((KeyValuePair<CategorieOutil, List<Ressource>> kvp, GameObject aItem) => Apparence.Is(aItem, kvp.Key), item);
			if (option.isSome)
			{
				return option.value.Value;
			}
		}
		return AllItems;
	}

	private bool Is(List<Ressource> itemTypes, GameObject tool)
	{
		return itemTypes.Slinq().Any((Ressource adj, GameObject aTool) => Apparence.Is(aTool, adj), tool);
	}

	public bool GetKit(ref List<GameObject> gos)
	{
		Apparence apparence = null;
		Apparence apparence2 = null;
		Apparence apparence3 = null;
		CategorieOutil categorieOutil = null;
		List<GameObject> list = base.recepteur.StoredGameObjectsSlinq.ToPooledList();
		foreach (GameObject item in list)
		{
			Apparence component = item.GetComponent<Apparence>();
			if (apparence == null && Is(Weapons, component.gameObject))
			{
				apparence = component;
				gos.Add(apparence.gameObject);
				categorieOutil = categorieOutil ?? component.Adjectifs.OfType(default(Tuple<CategorieOutil>)).FirstOrDefault();
			}
			else if (apparence2 == null && Is(Props, component.gameObject))
			{
				apparence2 = component;
				gos.Add(apparence2.gameObject);
				categorieOutil = categorieOutil ?? component.Adjectifs.OfType(default(Tuple<CategorieOutil>)).FirstOrDefault();
			}
			else if (apparence3 == null && Is(Hats, component.gameObject))
			{
				apparence3 = component;
				gos.Add(apparence3.gameObject);
				categorieOutil = categorieOutil ?? component.Adjectifs.OfType(default(Tuple<CategorieOutil>)).FirstOrDefault();
			}
		}
		list.Dispose();
		return categorieOutil != null && gos.Count == Exclusivity[categorieOutil].Count;
	}

	[ContextMenu("FillWithArcherKit")]
	public void FillWithArcherKit()
	{
		FillWithKit(Archer);
	}

	[ContextMenu("FillWithKnightKit")]
	public void FillWithKnightKit()
	{
		FillWithKit(Knight);
	}

	private void FillWithKit(IEnumerable<Ressource> ressources)
	{
		base.recepteur.Wipe();
		foreach (Ressource ressource in ressources)
		{
			FactoryTemplate factoryTemplate = Toolbag.SelectBestTool(ressource, onlyWorkerTools: false);
			while (base.recepteur.HasRoomFor(Factory.Peek(factoryTemplate.AssetKey)))
			{
				GameObject item = Toolbag.SpawnTool(factoryTemplate, magical: false);
				base.recepteur.AddObject(item);
			}
		}
	}

	[LuaDrivable("FillWithArcherKit")]
	private static DynValue lua_FillWithArcherKit(ScriptExecutionContext context, CallbackArguments args)
	{
		ToolRack toolRack = args.AsComponent<ToolRack>(0, "FillWithArcherKit");
		toolRack.FillWithKit(Archer);
		return DynValue.Void;
	}

	[LuaDrivable("FillWithKnightKit")]
	private static DynValue lua_FillWithKnightKit(ScriptExecutionContext context, CallbackArguments args)
	{
		ToolRack toolRack = args.AsComponent<ToolRack>(0, "FillWithKnightKit");
		toolRack.FillWithKit(Knight);
		return DynValue.Void;
	}

	[LuaDrivable("HasFullKit")]
	private static DynValue lua_HasFullKit(ScriptExecutionContext context, CallbackArguments args)
	{
		ToolRack toolRack = args.AsComponent<ToolRack>(0, "HasFullKit");
		return DynValue.NewBoolean(toolRack.HasFullKit());
	}
}
