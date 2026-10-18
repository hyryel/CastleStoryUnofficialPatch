using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Assets;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseLoadCreditLibraryMenu : BaseMapLibraryMenu
{
	protected override bool ReloadOnOpen => true;

	protected static Section OfficialSection => new Section((List<Asset_Map> assets) => (from i in assets.Slinq()
		where i.codename == "CreditIsland_d19201d7-aec4-4079-9df3-bf02ab8d3f96"
		select i).ToList(), (List<Asset_Map> assets) => (from x in assets
		orderby x.mapSize descending, x.displayName
		select x).ToList());

	protected override Asset_Map.MapHandler AssetHandler => AssetDispensary.maps;

	protected BaseLoadCreditLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override SectionHandle OnAddSections()
	{
		return base.OnAddSections().AddSection(OfficialSection);
	}

	protected override string GetAssetLabel(Asset_Map asset)
	{
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
		if (asset.authorType == AssetAuthorType.Official)
		{
			return I2Helper.TryGet(asset.description);
		}
		return asset.description;
	}

	protected override void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters)
	{
		MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Confirm);
	}
}
