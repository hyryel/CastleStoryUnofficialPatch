using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseLoadGameLibraryMenu : BaseMapLibraryMenu
{
	protected override bool ReloadOnOpen => true;

	protected override LayoutVariant SelectedLayoutVariant => LayoutVariant.List;

	protected static Section GeneralSection => new Section((List<Asset_Map> assets) => assets, (List<Asset_Map> assets) => BaseAssetLibraryMenu<Asset_Map>.ToSortedByDateThenTime(assets));

	protected override Asset_Map.MapHandler AssetHandler => AssetDispensary.saves;

	protected override bool ShowBadges => false;

	protected BaseLoadGameLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override SectionHandle OnAddSections()
	{
		return base.OnAddSections().AddSection(GeneralSection);
	}

	protected override string GetAssetLabel(Asset_Map asset)
	{
		return asset.customName;
	}

	protected override string GetAssetSubLabel(Asset_Map asset)
	{
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
	}
}
