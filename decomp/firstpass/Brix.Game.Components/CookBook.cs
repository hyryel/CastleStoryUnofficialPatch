using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.UI.Icons;

namespace Brix.Game.Components;

public static class CookBook
{
	private static int nextId = 1;

	private static Dictionary<int, RecipeInfo> RecipesById = new Dictionary<int, RecipeInfo>();

	public static RecipeInfo Sword = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Sword,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Sword,
		Drawing = IconKeys._Sword
	}.With(Adjectif.iron, 2).Make();

	public static RecipeInfo Shield = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Shield,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Shield,
		Drawing = IconKeys._Shield
	}.With(Adjectif.iron, 2).Make();

	public static RecipeInfo Helmet = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Helmet,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Helmet,
		Drawing = IconKeys._Helmet
	}.With(Adjectif.iron, 1).Make();

	public static RecipeInfo Bow = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Bow,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Bow,
		Drawing = IconKeys._Bow
	}.With(Adjectif.plankBlock, 1).With(Adjectif.rope, 1).Make();

	public static RecipeInfo Quiver = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Quiver,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Quiver,
		Drawing = IconKeys._Quiver
	}.With(Adjectif.plankBlock, 1).Make();

	public static RecipeInfo Cap = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Cap,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Cap,
		Drawing = IconKeys._Cap
	}.With(Adjectif.fabric, 1).Make();

	public static RecipeInfo CrossbowWeapon = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Crossbow,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Crossbow,
		Drawing = IconKeys._Crossbow
	}.With(Adjectif.cog, 2).With(Adjectif.rope, 1).With(Adjectif.iron, 1)
		.Make();

	public static RecipeInfo CrossbowPouch = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Pouch,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Pouch,
		Drawing = IconKeys._Pouch
	}.With(Adjectif.iron, 1).Make();

	public static RecipeInfo CrossbowMorion = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Kepi,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Kepi,
		Drawing = IconKeys._Kepi
	}.With(Adjectif.fabric, 1).Make();

	public static RecipeInfo AlchemistPouch = new RecipeMaker
	{
		Work = 21,
		Heat = 0,
		Creates = Accessories.Backpack,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Backpack,
		Drawing = IconKeys._Backpack
	}.With(Adjectif.fabric, 2).With(Adjectif.glass, 3).With(Adjectif.orangeCrystal, 30)
		.Make();

	public static RecipeInfo AlchemistHat = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Bonnet,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Bonnet,
		Drawing = IconKeys._Bonnet
	}.With(Adjectif.fabric, 1).Make();

	public static RecipeInfo Staff = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Staff,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Staff,
		Drawing = IconKeys._Staff
	}.With(Adjectif.orangeCrystal, 40).With(Adjectif.plankBlock, 1).With(Adjectif.iron, 1)
		.Make();

	public static RecipeInfo Sachel = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Sachel,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Satchel,
		Drawing = IconKeys._Satchel
	}.With(Adjectif.orangeCrystal, 40).With(Adjectif.fabric, 1).With(Adjectif.glass, 1)
		.Make();

	public static RecipeInfo ArtificerHat = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.ArtificerHat,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Artificer_Hat,
		Drawing = IconKeys._Artificer_Hat
	}.With(Adjectif.fabric, 1).Make();

	public static RecipeInfo Poleaxe = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.Poleaxe,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Poleaxe,
		Drawing = IconKeys._Poleaxe
	}.With(Adjectif.plankBlock, 1).Make();

	public static RecipeInfo KettleHat = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = Accessories.KettleHat,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Kettle_Hat,
		Drawing = IconKeys._Kettle_Hat,
		Sound = "Workbench_Halberd_Helmet_Done"
	}.With(Adjectif.iron, 1).Make();

	public static RecipeInfo RopeBallot = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = BuildResources.Rope,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Rope,
		Drawing = IconKeys._Rope
	}.With(Adjectif.plant, 10).Make();

	public static RecipeInfo FabricPile = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = BuildResources.Fabric,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Fabric,
		Drawing = IconKeys._Fabric
	}.With(Adjectif.plant, 10).Make();

	public static RecipeInfo Planks = new RecipeMaker
	{
		Work = 3,
		Heat = 0,
		Creates = Blocks.Wood_DoublePlank_AA,
		QteCreated = 3,
		DropOnCompletion = false,
		Icon = IconKeys._Plank,
		Drawing = IconKeys._Plank
	}.With(Adjectif.woodBlock, 1).Make();

	public static RecipeInfo IronIngot = new RecipeMaker
	{
		Work = 0,
		Heat = 2,
		Creates = BuildResources.IronIngot,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Iron,
		Drawing = IconKeys._Iron
	}.With(Adjectif.rawIron, 10).Make();

	public static RecipeInfo GlassIngot = new RecipeMaker
	{
		Work = 0,
		Heat = 5,
		Creates = BuildResources.GlassIngot,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Glass_Ingot,
		Drawing = IconKeys._Glass_Ingot
	}.With(Adjectif.stones, 10).Make();

	public static RecipeInfo Cog = new RecipeMaker
	{
		Work = 6,
		Heat = 0,
		Creates = BuildResources.Cog,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Cog,
		Drawing = IconKeys._Cog
	}.With(Adjectif.iron, 1).Make();

	public static RecipeInfo Bomb = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = BuildResources.Bomb,
		QteCreated = 3,
		DropOnCompletion = false,
		Icon = IconKeys._Bomb,
		Drawing = IconKeys._Bomb
	}.With(Adjectif.orangeCrystal, 15).Make();

	public static RecipeInfo BearTrap = new RecipeMaker
	{
		Work = 6,
		Heat = 0,
		Creates = ObjetsDynamiques.BearTrap,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Beartrap,
		Drawing = IconKeys._Beartrap
	}.With(Adjectif.iron, 1).Make();

	public static RecipeInfo ExplosiveBarrel = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = ObjetsDynamiques.Baril,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Explosive_Barrel,
		Drawing = IconKeys._Explosive_Barrel
	}.With(Adjectif.plankBlock, 1).With(Adjectif.orangeCrystal, 10).Make();

	public static RecipeInfo SentinelWard = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = ObjetsDynamiques.SentinelWard,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Sentinel,
		Drawing = IconKeys._Sentinel
	}.With(Adjectif.orangeCrystal, 40).With(Adjectif.iron, 1).Make();

	public static RecipeInfo HealingWard = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = ObjetsDynamiques.HealingWard,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Healing_Ward,
		Drawing = IconKeys._Healing_Ward
	}.With(Adjectif.orangeCrystal, 20).With(Adjectif.iron, 1).Make();

	public static RecipeInfo CaptureWard = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = ObjetsDynamiques.CaptureWard,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Capture_Stone,
		Drawing = IconKeys._Capture_Stone
	}.With(Adjectif.orangeCrystal, 40).With(Adjectif.iron, 1).Make();

	public static RecipeInfo LanternWard = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = ObjetsDynamiques.LanternWard,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Lantern,
		Drawing = IconKeys._Lantern
	}.With(Adjectif.orangeCrystal, 5).With(Adjectif.iron, 1).Make();

	public static RecipeInfo PylonWard = new RecipeMaker
	{
		Work = 10,
		Heat = 0,
		Creates = ObjetsDynamiques.PylonGround,
		QteCreated = 1,
		DropOnCompletion = false,
		Icon = IconKeys._Pylon,
		Drawing = IconKeys._Pylon
	}.With(Adjectif.orangeCrystal, 10).With(Adjectif.iron, 1).Make();

	public static RecipeInfo FireFly = new RecipeMaker
	{
		Work = 0,
		Heat = 0,
		Creates = Environment.Firefly,
		QteCreated = 1,
		DropOnCompletion = true,
		Icon = IconKeys._Wisp_Blue,
		Drawing = IconKeys._Wisp_Blue
	}.With(Adjectif.blueCrystal, 10).Make();

	public static int GetNextID()
	{
		return nextId++;
	}

	public static void Register(RecipeInfo info)
	{
		RecipesById.Add(info.id, info);
	}

	public static RecipeInfo GetById(int i)
	{
		return RecipesById[i];
	}
}
