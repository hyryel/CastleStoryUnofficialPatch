using Brix.Steam;

namespace Brix.Assets;

public static class AssetDispensary
{
	public static readonly string BackupsPath = "Info/Backups/";

	public static Asset_Map.MapHandler maps = new Asset_Map.MapHandler
	{
		officialPath = "Info/Maps/",
		ugcPath = "Info/UGC/Maps/",
		downloadPath = "Info/Downloads/Maps/",
		workshopType = WorkshopItemType.Map
	};

	public static Asset_Map.SaveHandler saves = new Asset_Map.SaveHandler
	{
		officialPath = "Info/Saves/",
		ugcPath = "Info/UGC/Saves/",
		downloadPath = "Info/Downloads/Saves/",
		workshopType = WorkshopItemType.Save
	};

	public static AssetHandler<Asset_Gamemode> gamemodes = new AssetHandler<Asset_Gamemode>
	{
		officialPath = "Info/Lua/Gamemodes/",
		ugcPath = "Info/UGC/Gamemodes/",
		downloadPath = "Info/Downloads/Gamemodes/",
		workshopType = WorkshopItemType.Gamemode
	};

	public static Asset_GamemodePreset.PresetHandler presets = new Asset_GamemodePreset.PresetHandler
	{
		officialPath = "Info/GMPresets/",
		ugcPath = "Info/UGC/GMPresets/",
		downloadPath = "Info/Downloads/GMPresets/",
		workshopType = WorkshopItemType.Preset
	};

	public static AssetHandler<Asset_Blueprints> blueprints = new AssetHandler<Asset_Blueprints>
	{
		officialPath = "Info/Blueprints/",
		ugcPath = "Info/UGC/Blueprints/",
		downloadPath = "Info/Downloads/Blueprints/",
		workshopType = WorkshopItemType.Blueprint
	};

	public static AssetHandler<Asset_Mod> mods = new AssetHandler<Asset_Mod>
	{
		officialPath = "Info/Lua/Mods/",
		ugcPath = "Info/UGC/Mods/",
		downloadPath = "Info/Downloads/Mods/",
		workshopType = WorkshopItemType.Mod
	};

	public static AssetHandler<Asset_Language> languages = new AssetHandler<Asset_Language>
	{
		officialPath = "Info/Languages/",
		ugcPath = "Info/UGC/Languages/",
		downloadPath = "Info/Downloads/Languages/",
		workshopType = WorkshopItemType.Language
	};

	public static Asset_Stats stats = new Asset_Stats();
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
