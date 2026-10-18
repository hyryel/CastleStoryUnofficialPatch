using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.AI;
using UnityEngine;

namespace Brix.Game.Semantique;

public static class Props
{
	private static Func<Recepteur, Factory.AssetKey> NoneFunc = (Recepteur r) => Accessories.Bag;

	private static Dictionary<Ressource, Func<Recepteur, Factory.AssetKey>> props = new Dictionary<Ressource, Func<Recepteur, Factory.AssetKey>>
	{
		{
			Adjectif.stones,
			(Recepteur r) => Accessories.Bag
		},
		{
			Adjectif.rawIron,
			(Recepteur r) => Accessories.Bag
		},
		{
			Adjectif.orangeCrystal,
			(Recepteur r) => Accessories.Bag
		},
		{
			Adjectif.rawGlass,
			(Recepteur r) => Accessories.Bag
		},
		{
			Adjectif.plant,
			(Recepteur r) => Accessories.Bag
		},
		{
			Adjectif.bomb,
			(Recepteur r) => Accessories.Bag
		},
		{
			Adjectif.purifiedBlueCrystal,
			(Recepteur r) => Accessories.Bag
		},
		{
			Adjectif.plankBlock,
			CreatePropsFunc(Adjectif.plankBlock, Accessories.HeldPlank, Accessories.HeldPlank2, Accessories.HeldPlank3)
		},
		{
			Adjectif.iron,
			CreatePropsFunc(Adjectif.iron, Accessories.HeldIron1, Accessories.HeldIron2, Accessories.HeldIron3)
		},
		{
			Adjectif.glass,
			CreatePropsFunc(Adjectif.glass, Accessories.HeldGlass1, Accessories.HeldGlass2, Accessories.HeldGlass3)
		},
		{
			Adjectif.fabric,
			(Recepteur r) => Accessories.HeldFabric
		},
		{
			Adjectif.rope,
			(Recepteur r) => Accessories.HeldRope
		},
		{
			Adjectif.woodBlock,
			(Recepteur r) => Accessories.HeldLog
		},
		{
			Adjectif.stoneBlock,
			(Recepteur r) => Accessories.HeldBrick
		},
		{
			Adjectif.explosiveBarrel,
			(Recepteur r) => Accessories.HeldBarrel
		},
		{
			Adjectif.cog,
			(Recepteur r) => Accessories.HeldCog
		},
		{
			Adjectif.pumpkin,
			(Recepteur r) => Accessories.HeldPumpkin
		},
		{
			Adjectif.crystalObjective,
			(Recepteur r) => Accessories.HeldCrystal
		},
		{
			Adjectif.bearTrap,
			(Recepteur r) => Accessories.HeldBearTrap
		},
		{
			Adjectif.sentinelWardAdj,
			(Recepteur r) => Accessories.HeldSentinelWard
		},
		{
			Adjectif.healingWardAdj,
			(Recepteur r) => Accessories.HeldHealingWard
		},
		{
			Adjectif.captureWardAdj,
			(Recepteur r) => Accessories.HeldCaptureWard
		},
		{
			Adjectif.lanternWardAdj,
			(Recepteur r) => Accessories.HeldLanternWard
		},
		{
			Adjectif.pylonWardAdj,
			(Recepteur r) => Accessories.HeldPylonWard
		},
		{
			Adjectif.sword,
			(Recepteur r) => Accessories.HeldSword
		},
		{
			Adjectif.shield,
			(Recepteur r) => Accessories.HeldShield
		},
		{
			Adjectif.helmet,
			(Recepteur r) => Accessories.HeldHelmet
		},
		{
			Adjectif.bow,
			(Recepteur r) => Accessories.HeldBow
		},
		{
			Adjectif.quiver,
			(Recepteur r) => Accessories.HeldQuiver
		},
		{
			Adjectif.cap,
			(Recepteur r) => Accessories.HeldCap
		},
		{
			Adjectif.crossbowWeapon,
			(Recepteur r) => Accessories.HeldCrossbow
		},
		{
			Adjectif.crossbowProp,
			(Recepteur r) => Accessories.HeldPouch
		},
		{
			Adjectif.crossbowHat,
			(Recepteur r) => Accessories.HeldKepi
		},
		{
			Adjectif.alchemistPouch,
			(Recepteur r) => Accessories.HeldBackpack
		},
		{
			Adjectif.alchemistHelmet,
			(Recepteur r) => Accessories.HeldBonnet
		},
		{
			Adjectif.geomancerWeapon,
			(Recepteur r) => Accessories.HeldStaff
		},
		{
			Adjectif.geomancerProp,
			(Recepteur r) => Accessories.HeldSachel
		},
		{
			Adjectif.geomancerHat,
			(Recepteur r) => Accessories.HeldArtificerHat
		},
		{
			Adjectif.halbard,
			(Recepteur r) => Accessories.HeldPoleaxe
		},
		{
			Adjectif.kettleHat,
			(Recepteur r) => Accessories.HeldKettleHat
		},
		{
			Adjectif.blueCrystalChunck,
			(Recepteur r) => Accessories.HeldBlueCrystalChunck
		}
	};

	public static Factory.AssetKey GetProp(this Recepteur r, Ressource adj)
	{
		return props.GetValueOrDefault(adj, NoneFunc)(r);
	}

	private static Func<Recepteur, Factory.AssetKey> CreatePropsFunc(Adjectif adj, params Factory.AssetKey[] keys)
	{
		return (Recepteur r) => keys[Mathf.Clamp(r.ContentDescription.Value(adj) - 1, 0, keys.Length)];
	}
}
