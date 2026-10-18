using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Smooth.Slinq;
using UnityEngine;

public class RecipeDisplay : GameComponent
{
	[SerializeField]
	private GameObject Sword;

	[SerializeField]
	private GameObject Shield;

	[SerializeField]
	private GameObject Helmet;

	[SerializeField]
	private GameObject Bow;

	[SerializeField]
	private GameObject Quiver;

	[SerializeField]
	private GameObject Cap;

	[SerializeField]
	private GameObject Crossbow;

	[SerializeField]
	private GameObject Pouch;

	[SerializeField]
	private GameObject Kepi;

	[SerializeField]
	private GameObject Backpack;

	[SerializeField]
	private GameObject Bonnet;

	[SerializeField]
	private GameObject Staff;

	[SerializeField]
	private GameObject Sachel;

	[SerializeField]
	private GameObject ArtificerHat;

	[SerializeField]
	private GameObject Halbard;

	[SerializeField]
	private GameObject KettleHat;

	[SerializeField]
	private GameObject Barrel;

	[SerializeField]
	private GameObject Ward;

	[SerializeField]
	private GameObject BearTrap;

	[SerializeField]
	private GameObject Bomb;

	[SerializeField]
	private GameObject Cog;

	[SerializeField]
	private GameObject Fabric;

	[SerializeField]
	private GameObject Rope;

	[SerializeField]
	private GameObject Sentinel;

	[SerializeField]
	private GameObject Capture;

	[SerializeField]
	private GameObject Healing;

	[SerializeField]
	private GameObject Lantern;

	[SerializeField]
	private GameObject Pylon;

	private static Dictionary<Factory.AssetKey, Func<RecipeDisplay, GameObject>> displays = new Dictionary<Factory.AssetKey, Func<RecipeDisplay, GameObject>>
	{
		{
			Accessories.Sword,
			(RecipeDisplay d) => d.Sword
		},
		{
			Accessories.Shield,
			(RecipeDisplay d) => d.Shield
		},
		{
			Accessories.Helmet,
			(RecipeDisplay d) => d.Helmet
		},
		{
			Accessories.Bow,
			(RecipeDisplay d) => d.Bow
		},
		{
			Accessories.Quiver,
			(RecipeDisplay d) => d.Quiver
		},
		{
			Accessories.Cap,
			(RecipeDisplay d) => d.Cap
		},
		{
			Accessories.Crossbow,
			(RecipeDisplay d) => d.Crossbow
		},
		{
			Accessories.Pouch,
			(RecipeDisplay d) => d.Pouch
		},
		{
			Accessories.Kepi,
			(RecipeDisplay d) => d.Kepi
		},
		{
			Accessories.Backpack,
			(RecipeDisplay d) => d.Backpack
		},
		{
			Accessories.Bonnet,
			(RecipeDisplay d) => d.Bonnet
		},
		{
			Accessories.Staff,
			(RecipeDisplay d) => d.Staff
		},
		{
			Accessories.Sachel,
			(RecipeDisplay d) => d.Sachel
		},
		{
			Accessories.ArtificerHat,
			(RecipeDisplay d) => d.ArtificerHat
		},
		{
			Accessories.Poleaxe,
			(RecipeDisplay d) => d.Halbard
		},
		{
			Accessories.KettleHat,
			(RecipeDisplay d) => d.KettleHat
		},
		{
			ObjetsDynamiques.Baril,
			(RecipeDisplay d) => d.Barrel
		},
		{
			ObjetsDynamiques.BanishingWard,
			(RecipeDisplay d) => d.Ward
		},
		{
			ObjetsDynamiques.SoulWard,
			(RecipeDisplay d) => d.Ward
		},
		{
			ObjetsDynamiques.CaptureWard,
			(RecipeDisplay d) => d.Capture
		},
		{
			ObjetsDynamiques.HealingWard,
			(RecipeDisplay d) => d.Healing
		},
		{
			ObjetsDynamiques.LanternWard,
			(RecipeDisplay d) => d.Lantern
		},
		{
			ObjetsDynamiques.PylonGround,
			(RecipeDisplay d) => d.Pylon
		},
		{
			ObjetsDynamiques.SentinelWard,
			(RecipeDisplay d) => d.Sentinel
		},
		{
			ObjetsDynamiques.BearTrap,
			(RecipeDisplay d) => d.BearTrap
		},
		{
			BuildResources.Bomb,
			(RecipeDisplay d) => d.Bomb
		},
		{
			BuildResources.Cog,
			(RecipeDisplay d) => d.Cog
		},
		{
			BuildResources.Fabric,
			(RecipeDisplay d) => d.Fabric
		},
		{
			BuildResources.Rope,
			(RecipeDisplay d) => d.Rope
		}
	};

	private Factory.AssetKey key;

	public override void OnRequested()
	{
		base.OnRequested();
		DisplayObject(key);
	}

	private void DisplayObject(Factory.AssetKey nextKey)
	{
		if (!Affiliation.IsAllied(User.LocalUser.faction, base.gameObject))
		{
			return;
		}
		DisableAll();
		if (nextKey != null)
		{
			GameObject gameObject = DisplayOf(nextKey);
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
			}
		}
		key = nextKey;
	}

	private void DisableAll()
	{
		displays.Slinq().ForEach(delegate(KeyValuePair<Factory.AssetKey, Func<RecipeDisplay, GameObject>> kvp)
		{
			Disable(kvp.Value(this));
		});
	}

	private static void Disable(GameObject go)
	{
		if (go != null)
		{
			go.SetActive(value: false);
		}
	}

	private GameObject DisplayOf(Factory.AssetKey key)
	{
		return displays[key](this);
	}

	public void ShowRecipe(RecipeInfo recipe)
	{
		key = recipe?.Creates;
		DisplayObject(key);
	}

	public void FromStation(CraftingStation station)
	{
		GameObject go;
		RecipeInfo recipe = ((!station.ProductionToPickup(out go)) ? station.FirstRecipe : null);
		ShowRecipe(recipe);
	}
}
