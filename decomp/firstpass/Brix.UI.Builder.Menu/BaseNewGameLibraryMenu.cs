using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Assets;
using Brix.UI.Icons;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseNewGameLibraryMenu : BaseMapLibraryMenu
{
	private static Asset_Map _steamAddMap;

	private static Section TutorialSection => new Section((List<Asset_Map> assets) => (from i in assets.Slinq()
		where i.codename == "TutorialIsland_ad7e7180-1316-4cbc-b66c-b1c055f0068c"
		select i).ToList());

	protected static Section OfficialSection => new Section(() => I2Helper.TryGet("##mainmenu_newgame_header_official"), (List<Asset_Map> assets) => (from i in assets.Slinq()
		where i.codename != "TutorialIsland_ad7e7180-1316-4cbc-b66c-b1c055f0068c"
		where i.authorType == AssetAuthorType.Official
		select i).ToList(), (List<Asset_Map> assets) => assets.OrderByDescending((Asset_Map x) => x.Rating).ToList());

	protected static Section UgcSection => new Section(() => I2Helper.TryGet("##mainmenu_newgame_header_usergenerated"), (List<Asset_Map> assets) => (from i in assets.Slinq()
		where i.authorType == AssetAuthorType.UGC
		select i).ToList(), (List<Asset_Map> assets) => BaseAssetLibraryMenu<Asset_Map>.ToSortedByDateThenTime(assets));

	public static Asset_Map SteamAddMap => _steamAddMap ?? (_steamAddMap = Asset_Map.CreateShell(Asset_Map.MapType.Map));

	protected static Section WorkshopSection => new Section(() => I2Helper.TryGet("##mainmenu_newgame_header_steamworkshop"), delegate(List<Asset_Map> assets)
	{
		List<Asset_Map> list = (from i in assets.Slinq()
			where i.authorType == AssetAuthorType.Workshop
			select i).ToList();
		list.Add(SteamAddMap);
		return list;
	}, delegate(List<Asset_Map> assets)
	{
		List<Asset_Map> list = BaseAssetLibraryMenu<Asset_Map>.ToSortedByDateThenTime(assets);
		list.Remove(SteamAddMap);
		list.Insert(0, SteamAddMap);
		return list;
	});

	protected override Asset_Map.MapHandler AssetHandler => AssetDispensary.maps;

	protected BaseNewGameLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override SectionHandle OnAddSections()
	{
		return base.OnAddSections().AddSection(TutorialSection).AddSection(OfficialSection)
			.AddSection(UgcSection)
			.AddSection(WorkshopSection);
	}

	protected override string GetAssetLabel(Asset_Map asset)
	{
		if (asset == SteamAddMap)
		{
			return I2Helper.TryGet("##mainmenu_newgame_steambtn_title");
		}
		if (asset.authorType == AssetAuthorType.Official)
		{
			return I2Helper.TryGet(asset.displayName);
		}
		return asset.displayName;
	}

	protected override string GetAssetSubLabel(Asset_Map asset)
	{
		if (asset == SteamAddMap)
		{
			return I2Helper.TryGet("##mainmenu_newgame_steambtn_description");
		}
		string empty = string.Empty;
		if (SelectedLayoutVariant == LayoutVariant.List)
		{
			empty += asset.dateTimeCreated.ToShortDateString();
			empty += "\n";
			return empty + asset.dateTimeCreated.ToShortTimeString();
		}
		return empty + I2Helper.TryGet("##mainmenu_select");
	}

	protected override string GetAssetInfoLabel(Asset_Map asset)
	{
		if (asset == SteamAddMap)
		{
			return I2Helper.TryGet("##mainmenu_newgame_steambtn_info");
		}
		if (asset.authorType == AssetAuthorType.Official)
		{
			return I2Helper.TryGet(asset.description);
		}
		return asset.description;
	}

	protected override Sprite GetAssetDefaultImage(Asset_Map asset)
	{
		if (asset == SteamAddMap)
		{
			return IconKeys._UI_Plus.Get32();
		}
		return base.GetAssetDefaultImage(asset);
	}

	protected override string GetAssetDefaultText(Asset_Map asset)
	{
		if (asset == SteamAddMap)
		{
			return string.Empty;
		}
		return base.GetAssetDefaultText(asset);
	}

	protected override void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters)
	{
	}
}
