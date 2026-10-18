using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Utils;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Brix.Lua;

public static class LuaCrafting
{
	public enum LuaRecipe
	{
		None,
		Sword,
		Shield,
		Helmet,
		Bow,
		Quiver,
		Cap,
		CrossbowWeapon,
		CrossbowPouch,
		CrossbowMorion,
		AlchemistPouch,
		AlchemistHat,
		Staff,
		Sachel,
		ArtificerHat,
		Poleaxe,
		KettleHat,
		HalberdierKit,
		ArcherKit,
		KnightKit,
		ArbalistKit,
		ArtificerKit,
		AlchemistKit,
		RopeBallot,
		FabricPile,
		Planks,
		IronIngot,
		GlassIngot,
		Cog,
		Bomb,
		BearTrap,
		ExplosiveBarrel,
		SentinelWard,
		HealingWard,
		CaptureWard,
		LanternWard,
		PylonWard
	}

	public enum LuaResource
	{
		None,
		Empty,
		StoneBlock,
		WoodBlock,
		PlankBlock,
		Stones,
		Iron,
		BlueCrystal,
		TronStone,
		OrangeCrystal,
		SentinelWard,
		HealingWard,
		CaptureWard,
		LanternWard,
		PylonWard,
		Plant,
		Rope,
		Fabric,
		RawIron,
		Glass,
		Stick,
		Cog,
		Bomb,
		BearTrap,
		ExplosiveBarrel,
		Spring,
		Halbard,
		KettleHat,
		Sword,
		Shield,
		Helmet,
		Bow,
		Quiver,
		Cap,
		CrossbowWeapon,
		CrossbowProp,
		CrossbowHat,
		GeomancerWeapon,
		GeomancerProp,
		GeomancerHat,
		AlchemistPouch,
		AlchemistHelmet
	}

	private static HashSet<LuaRecipe> _kits = new HashSet<LuaRecipe>
	{
		LuaRecipe.HalberdierKit,
		LuaRecipe.ArcherKit,
		LuaRecipe.KnightKit,
		LuaRecipe.ArbalistKit,
		LuaRecipe.ArtificerKit,
		LuaRecipe.AlchemistKit
	};

	private static Dictionary<LuaResource, AdjectiveKey> _luaResourceToResource;

	private static Dictionary<AdjectiveKey, LuaResource> _adjectiveToLuaResource;

	private static Dictionary<LuaRecipe, List<RecipeInfo>> _luaRecipeToRecipeInfo;

	private static Dictionary<RecipeInfo, LuaRecipe> _recipeInfoToLuaRecipe;

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> GetRecipeInfo = delegate(ScriptExecutionContext sec, CallbackArguments arg)
	{
		LuaRecipe luaRecipe = arg.AsEnum<LuaRecipe>(0, "GetRecipeInfo");
		return (luaRecipe == LuaRecipe.None) ? ToRecipeInfo(LuaRecipe.Sword).ToDynValue(sec.OwnerScript) : ToRecipeInfo(luaRecipe).ToDynValue(sec.OwnerScript);
	};

	private static bool _isRegistered;

	private static readonly DynValue RecipeEnum = new LuaEnumDescriptor<LuaRecipe>().CreateDynValue();

	private static readonly DynValue ResourceEnum = new LuaEnumDescriptor<LuaResource>().CreateDynValue();

	private static readonly DynValue GetRecipeInfoCallback = DynValue.NewCallback(GetRecipeInfo);

	private static void MapResources(out Dictionary<LuaResource, AdjectiveKey> luaResourceToResource, out Dictionary<AdjectiveKey, LuaResource> adjectiveToLuaResource)
	{
		luaResourceToResource = new Dictionary<LuaResource, AdjectiveKey>(FastEnumIntEqualityComparer<LuaResource>.Instance)
		{
			{
				LuaResource.StoneBlock,
				Adjectif.stoneBlock.GetKey()
			},
			{
				LuaResource.WoodBlock,
				Adjectif.woodBlock.GetKey()
			},
			{
				LuaResource.PlankBlock,
				Adjectif.plankBlock.GetKey()
			},
			{
				LuaResource.Stones,
				Adjectif.stones.GetKey()
			},
			{
				LuaResource.Iron,
				Adjectif.iron.GetKey()
			},
			{
				LuaResource.BlueCrystal,
				Adjectif.blueCrystal.GetKey()
			},
			{
				LuaResource.TronStone,
				Adjectif.tronStone.GetKey()
			},
			{
				LuaResource.OrangeCrystal,
				Adjectif.orangeCrystal.GetKey()
			},
			{
				LuaResource.SentinelWard,
				Adjectif.sentinelWardAdj.GetKey()
			},
			{
				LuaResource.HealingWard,
				Adjectif.healingWardAdj.GetKey()
			},
			{
				LuaResource.CaptureWard,
				Adjectif.captureWardAdj.GetKey()
			},
			{
				LuaResource.LanternWard,
				Adjectif.lanternWardAdj.GetKey()
			},
			{
				LuaResource.PylonWard,
				Adjectif.pylonWardAdj.GetKey()
			},
			{
				LuaResource.Plant,
				Adjectif.plant.GetKey()
			},
			{
				LuaResource.Rope,
				Adjectif.rope.GetKey()
			},
			{
				LuaResource.Fabric,
				Adjectif.fabric.GetKey()
			},
			{
				LuaResource.RawIron,
				Adjectif.rawIron.GetKey()
			},
			{
				LuaResource.Glass,
				Adjectif.glass.GetKey()
			},
			{
				LuaResource.Stick,
				Adjectif.stick.GetKey()
			},
			{
				LuaResource.Cog,
				Adjectif.cog.GetKey()
			},
			{
				LuaResource.Bomb,
				Adjectif.bomb.GetKey()
			},
			{
				LuaResource.BearTrap,
				Adjectif.bearTrap.GetKey()
			},
			{
				LuaResource.ExplosiveBarrel,
				Adjectif.explosiveBarrel.GetKey()
			},
			{
				LuaResource.Spring,
				Adjectif.spring.GetKey()
			},
			{
				LuaResource.Halbard,
				Adjectif.halbard.GetKey()
			},
			{
				LuaResource.KettleHat,
				Adjectif.kettleHat.GetKey()
			},
			{
				LuaResource.Sword,
				Adjectif.sword.GetKey()
			},
			{
				LuaResource.Shield,
				Adjectif.shield.GetKey()
			},
			{
				LuaResource.Helmet,
				Adjectif.helmet.GetKey()
			},
			{
				LuaResource.Bow,
				Adjectif.bow.GetKey()
			},
			{
				LuaResource.Quiver,
				Adjectif.quiver.GetKey()
			},
			{
				LuaResource.Cap,
				Adjectif.cap.GetKey()
			},
			{
				LuaResource.CrossbowWeapon,
				Adjectif.crossbowWeapon.GetKey()
			},
			{
				LuaResource.CrossbowProp,
				Adjectif.crossbowProp.GetKey()
			},
			{
				LuaResource.CrossbowHat,
				Adjectif.crossbowHat.GetKey()
			},
			{
				LuaResource.GeomancerWeapon,
				Adjectif.geomancerWeapon.GetKey()
			},
			{
				LuaResource.GeomancerProp,
				Adjectif.geomancerProp.GetKey()
			},
			{
				LuaResource.GeomancerHat,
				Adjectif.geomancerHat.GetKey()
			},
			{
				LuaResource.AlchemistPouch,
				Adjectif.alchemistPouch.GetKey()
			},
			{
				LuaResource.AlchemistHelmet,
				Adjectif.alchemistHelmet.GetKey()
			}
		};
		adjectiveToLuaResource = luaResourceToResource.ToDictionary((KeyValuePair<LuaResource, AdjectiveKey> kvp) => kvp.Value, (KeyValuePair<LuaResource, AdjectiveKey> kvp) => kvp.Key);
	}

	private static void MapRecipes(out Dictionary<LuaRecipe, List<RecipeInfo>> luaRecipeToRecipeInfo, out Dictionary<RecipeInfo, LuaRecipe> recipeInfoToLuaRecipe)
	{
		Dictionary<LuaRecipe, RecipeInfo> dictionary = new Dictionary<LuaRecipe, RecipeInfo>(FastEnumIntEqualityComparer<LuaRecipe>.Instance);
		dictionary.Add(LuaRecipe.Sword, CookBook.Sword);
		dictionary.Add(LuaRecipe.Shield, CookBook.Shield);
		dictionary.Add(LuaRecipe.Helmet, CookBook.Helmet);
		dictionary.Add(LuaRecipe.Bow, CookBook.Bow);
		dictionary.Add(LuaRecipe.Quiver, CookBook.Quiver);
		dictionary.Add(LuaRecipe.Cap, CookBook.Cap);
		dictionary.Add(LuaRecipe.CrossbowWeapon, CookBook.CrossbowWeapon);
		dictionary.Add(LuaRecipe.CrossbowPouch, CookBook.CrossbowPouch);
		dictionary.Add(LuaRecipe.CrossbowMorion, CookBook.CrossbowMorion);
		dictionary.Add(LuaRecipe.AlchemistPouch, CookBook.AlchemistPouch);
		dictionary.Add(LuaRecipe.AlchemistHat, CookBook.AlchemistHat);
		dictionary.Add(LuaRecipe.Staff, CookBook.Staff);
		dictionary.Add(LuaRecipe.Sachel, CookBook.Sachel);
		dictionary.Add(LuaRecipe.ArtificerHat, CookBook.ArtificerHat);
		dictionary.Add(LuaRecipe.Poleaxe, CookBook.Poleaxe);
		dictionary.Add(LuaRecipe.KettleHat, CookBook.KettleHat);
		dictionary.Add(LuaRecipe.RopeBallot, CookBook.RopeBallot);
		dictionary.Add(LuaRecipe.FabricPile, CookBook.FabricPile);
		dictionary.Add(LuaRecipe.Planks, CookBook.Planks);
		dictionary.Add(LuaRecipe.IronIngot, CookBook.IronIngot);
		dictionary.Add(LuaRecipe.GlassIngot, CookBook.GlassIngot);
		dictionary.Add(LuaRecipe.Cog, CookBook.Cog);
		dictionary.Add(LuaRecipe.Bomb, CookBook.Bomb);
		dictionary.Add(LuaRecipe.BearTrap, CookBook.BearTrap);
		dictionary.Add(LuaRecipe.ExplosiveBarrel, CookBook.ExplosiveBarrel);
		dictionary.Add(LuaRecipe.SentinelWard, CookBook.SentinelWard);
		dictionary.Add(LuaRecipe.HealingWard, CookBook.HealingWard);
		dictionary.Add(LuaRecipe.CaptureWard, CookBook.CaptureWard);
		dictionary.Add(LuaRecipe.LanternWard, CookBook.LanternWard);
		dictionary.Add(LuaRecipe.PylonWard, CookBook.PylonWard);
		Dictionary<LuaRecipe, RecipeInfo> source = dictionary;
		recipeInfoToLuaRecipe = source.ToDictionary((KeyValuePair<LuaRecipe, RecipeInfo> kvp) => kvp.Value, (KeyValuePair<LuaRecipe, RecipeInfo> kvp) => kvp.Key);
		luaRecipeToRecipeInfo = source.ToDictionary((KeyValuePair<LuaRecipe, RecipeInfo> kvp) => kvp.Key, (KeyValuePair<LuaRecipe, RecipeInfo> kvp) => new List<RecipeInfo> { kvp.Value });
		luaRecipeToRecipeInfo.Add(LuaRecipe.HalberdierKit, new List<RecipeInfo>
		{
			CookBook.Poleaxe,
			CookBook.KettleHat
		});
		luaRecipeToRecipeInfo.Add(LuaRecipe.ArcherKit, new List<RecipeInfo>
		{
			CookBook.Bow,
			CookBook.Quiver,
			CookBook.Cap
		});
		luaRecipeToRecipeInfo.Add(LuaRecipe.KnightKit, new List<RecipeInfo>
		{
			CookBook.Sword,
			CookBook.Shield,
			CookBook.Helmet
		});
		luaRecipeToRecipeInfo.Add(LuaRecipe.ArbalistKit, new List<RecipeInfo>
		{
			CookBook.CrossbowWeapon,
			CookBook.CrossbowPouch,
			CookBook.CrossbowMorion
		});
		luaRecipeToRecipeInfo.Add(LuaRecipe.ArtificerKit, new List<RecipeInfo>
		{
			CookBook.Staff,
			CookBook.Sachel,
			CookBook.ArtificerHat
		});
		luaRecipeToRecipeInfo.Add(LuaRecipe.AlchemistKit, new List<RecipeInfo>
		{
			CookBook.AlchemistPouch,
			CookBook.AlchemistHat
		});
	}

	public static LuaRecipe ToLuaRecipe(RecipeInfo value)
	{
		return (value != null) ? _recipeInfoToLuaRecipe[value] : LuaRecipe.None;
	}

	public static List<RecipeInfo> ToRecipeInfo(LuaRecipe value)
	{
		return _luaRecipeToRecipeInfo[value];
	}

	public static LuaResource ToLuaResource(Ressource value)
	{
		return (value != null) ? _adjectiveToLuaResource[value.GetKey()] : LuaResource.None;
	}

	public static LuaResource ToLuaResource(AdjectiveKey value)
	{
		return (value != null) ? _adjectiveToLuaResource[value] : LuaResource.None;
	}

	public static bool IsKit(LuaRecipe r)
	{
		return _kits.Contains(r);
	}

	public static Ressource ToResource(LuaResource value)
	{
		return _luaResourceToResource[value].GetAdjectif() as Ressource;
	}

	public static void Register(Script script)
	{
		if (!_isRegistered)
		{
			MapRecipes(out _luaRecipeToRecipeInfo, out _recipeInfoToLuaRecipe);
			MapResources(out _luaResourceToResource, out _adjectiveToLuaResource);
			_isRegistered = true;
		}
		DynValue value = DynValue.NewTable(script);
		value.Table.Set("GetRecipeInfo", GetRecipeInfoCallback);
		script.Globals.Set("Crafting", value);
		script.Globals.Set("Recipe", RecipeEnum);
		script.Globals.Set("Resource", ResourceEnum);
	}
}
