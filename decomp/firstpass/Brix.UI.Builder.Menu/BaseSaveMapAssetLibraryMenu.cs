using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseSaveMapAssetLibraryMenu : BaseMapLibraryMenu
{
	private static Asset_Map _newMap;

	protected override bool ReloadOnOpen => true;

	protected override bool ShowBadges => false;

	protected override LayoutVariant SelectedLayoutVariant => LayoutVariant.List;

	private static Section NewSection => new Section((List<Asset_Map> assets) => new List<Asset_Map> { NewMap });

	private static Section SimilarSection => new Section(() => string.Format("{0} ({1})", I2Helper.TryGet("##gamemenu_savegame_similar"), GameParam.Gamemode.displayName), (List<Asset_Map> assets) => assets.Slinq().Where(delegate(Asset_Map asset)
	{
		Asset_Gamemode lastPlayedGamemode = asset.GetLastPlayedGamemode();
		return lastPlayedGamemode != null && lastPlayedGamemode.codename == GameParam.Gamemode.codename;
	}).ToList(), (List<Asset_Map> assets) => BaseAssetLibraryMenu<Asset_Map>.ToSortedByDateThenTime(assets));

	private static Section OtherSection => new Section(() => I2Helper.TryGet("##gamemenu_savegame_other"), (List<Asset_Map> assets) => assets.Slinq().Where(delegate(Asset_Map asset)
	{
		Asset_Gamemode lastPlayedGamemode = asset.GetLastPlayedGamemode();
		return lastPlayedGamemode == null || lastPlayedGamemode.codename != GameParam.Gamemode.codename;
	}).ToList(), (List<Asset_Map> assets) => BaseAssetLibraryMenu<Asset_Map>.ToSortedByDateThenTime(assets));

	public static Asset_Map NewMap => _newMap = ((_newMap != null && _newMap.IsShell()) ? _newMap : Asset_Map.CreateShell(Asset_Map.MapType.Save));

	protected override Asset_Map.MapHandler AssetHandler => AssetDispensary.saves;

	protected BaseSaveMapAssetLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override SectionHandle OnAddSections()
	{
		return base.OnAddSections().AddSection(NewSection).AddSection(SimilarSection)
			.AddSection(OtherSection);
	}

	protected override string GetAssetLabel(Asset_Map asset)
	{
		if (asset.IsShell())
		{
			return I2Helper.TryGet("##gamemenu_savegame_newsave");
		}
		return asset.customName;
	}

	protected override string GetAssetSubLabel(Asset_Map asset)
	{
		if (asset.IsShell())
		{
			return string.Empty;
		}
		string empty = string.Empty;
		if (SelectedLayoutVariant == LayoutVariant.List)
		{
			string arg = asset.dateTimeCreated.ToShortDateString();
			string arg2 = asset.dateTimeCreated.ToShortTimeString();
			empty = $"{empty}{arg}";
			empty = $"{empty}\n";
			return $"{empty}{arg2}";
		}
		return string.Format("{0}{1}", empty, I2Helper.TryGet("##mainmenu_select"));
	}

	protected override string GetAssetInfoLabel(Asset_Map asset)
	{
		if (asset.IsShell())
		{
			return string.Empty;
		}
		string empty = string.Empty;
		string arg = asset.displayName.ToUpper();
		string arg2 = asset.dateTimeCreated.ToShortDateString();
		string arg3 = asset.dateTimeCreated.ToShortTimeString();
		Asset_Gamemode lastPlayedGamemode = asset.GetLastPlayedGamemode();
		string arg4 = ((lastPlayedGamemode == null) ? "???" : lastPlayedGamemode.displayName.ToUpper());
		Asset_GamemodePreset lastPlayedGamemodePreset = asset.GetLastPlayedGamemodePreset();
		string arg5 = ((lastPlayedGamemodePreset == null) ? "???" : lastPlayedGamemodePreset.displayName.ToUpper());
		if (SelectedLayoutVariant == LayoutVariant.List)
		{
			empty = $"{empty}<b>{arg4}|{arg5}</b>";
			empty = $"{empty} ";
			return $"{empty}{arg}";
		}
		empty = $"{empty}<b>{arg}</b>";
		empty = $"{empty}\n{arg4}|{arg5}\n";
		empty = $"{empty}\n{arg2}";
		return $"{empty}\n{arg3}";
	}

	protected override void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters)
	{
		MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Confirm);
		BaseMapSaverMenu.CloseSignal.Invoke();
		BaseMapSaverMenu.OpenSignal.Invoke();
	}

	protected override void CloseSelf()
	{
		base.CloseSelf();
		BaseMapSaverMenu.CloseSignal.Invoke();
	}
}
