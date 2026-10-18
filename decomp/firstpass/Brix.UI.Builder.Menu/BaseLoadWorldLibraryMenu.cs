using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Brix.Assets;
using Brix.UI.Icons;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseLoadWorldLibraryMenu : BaseMapLibraryMenu
{
	private static Asset_Map _steamAddMap;

	protected override bool ReloadOnOpen => true;

	private static Section EmptySection => new Section((List<Asset_Map> assets) => (from i in assets.Slinq()
		where i.codename == "we_new_1c3b9622-d7d2-4b75-84c2-f487f81b599b" || i.codename == "we_newdesert_1c3b9622-d7d2-4b75-84c2-f487f81b599b" || i.codename == "we_newwinter_edf8b123-3f26-4582-ae33-aea5e488eb43"
		select i).ToList());

	protected static Section OfficialSection => new Section(() => I2Helper.TryGet("##mainmenu_newgame_header_official"), (List<Asset_Map> assets) => (from i in assets.Slinq()
		where i.codename != "we_new_1c3b9622-d7d2-4b75-84c2-f487f81b599b"
		where i.codename != "we_newdesert_1c3b9622-d7d2-4b75-84c2-f487f81b599b"
		where i.codename != "we_newwinter_edf8b123-3f26-4582-ae33-aea5e488eb43"
		where i.authorType == AssetAuthorType.Official
		select i).ToList(), (List<Asset_Map> assets) => (from x in assets
		orderby x.mapSize descending, x.displayName
		select x).ToList());

	private static Section UgcSection => new Section(() => I2Helper.TryGet("##mainmenu_newgame_header_usergenerated"), (List<Asset_Map> assets) => (from i in assets.Slinq()
		where i.authorType == AssetAuthorType.UGC
		select i).ToList(), (List<Asset_Map> assets) => BaseAssetLibraryMenu<Asset_Map>.ToSortedByDateThenTime(assets));

	protected override Asset_Map.MapHandler AssetHandler => AssetDispensary.maps;

	protected BaseLoadWorldLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override SectionHandle OnAddSections()
	{
		_steamAddMap = ((_steamAddMap == null) ? Asset_Map.CreateShell(Asset_Map.MapType.Map) : _steamAddMap);
		return base.OnAddSections().AddSection(EmptySection).AddSection(OfficialSection)
			.AddSection(UgcSection);
	}

	protected override string GetAssetLabel(Asset_Map asset)
	{
		if (asset == _steamAddMap)
		{
			return "##mode_worldeditor_workshop_csonsteam";
		}
		if (asset.authorType == AssetAuthorType.Official)
		{
			return I2Helper.TryGet(asset.displayName);
		}
		return asset.displayName;
	}

	protected override string GetAssetSubLabel(Asset_Map asset)
	{
		return I2Helper.TryGet("##mainmenu_select").ToUpper();
	}

	protected override string GetAssetInfoLabel(Asset_Map asset)
	{
		if (asset == _steamAddMap)
		{
			return string.Empty;
		}
		if (asset.authorType == AssetAuthorType.Official)
		{
			return I2Helper.TryGet(asset.description);
		}
		return asset.description;
	}

	protected override Sprite GetAssetDefaultImage(Asset_Map asset)
	{
		if (asset == _steamAddMap)
		{
			return IconKeys._UI_Plus.Get32();
		}
		return base.GetAssetDefaultImage(asset);
	}

	protected override string GetAssetDefaultText(Asset_Map asset)
	{
		if (asset == _steamAddMap)
		{
			return "Find more!";
		}
		return base.GetAssetDefaultText(asset);
	}

	protected override void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters)
	{
		if (SelectedAsset == _steamAddMap)
		{
			SelectedAsset = null;
			Process.Start("steam://url/SSA");
		}
		else
		{
			MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Confirm);
		}
	}
}
