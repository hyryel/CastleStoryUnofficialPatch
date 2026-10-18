using System;
using System.Collections.Generic;
using System.Diagnostics;
using Brix.Assets;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class SpNewGameLibraryMenu : BaseNewGameLibraryMenu
{
	public static Signal ReopenSignal = new Signal();

	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Singleplayer;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSPSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Single;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.New;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	protected override bool AutoSelectFirstAsset => true;

	protected override bool LoadSubassets => true;

	private SpNewGameLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static SpNewGameLibraryMenu Create(Func<RectTransform> binGetter)
	{
		return new SpNewGameLibraryMenu(binGetter);
	}

	protected override void LoadSelf()
	{
		base.LoadSelf();
		if (!AutoSelectFirstAsset || BaseMapStarterMenu.IsPostUpdate)
		{
			return;
		}
		PersistentCoroutineRunner.Relais(delegate(SpNewGameLibraryMenu self)
		{
			if (self != null)
			{
				AssetAuthorType selectedAuthorType = LegacyPlaySourcesMenu.SelectedAuthorType;
				Section section = BaseNewGameLibraryMenu.OfficialSection;
				switch (selectedAuthorType)
				{
				case AssetAuthorType.Official:
					section = BaseNewGameLibraryMenu.OfficialSection;
					break;
				case AssetAuthorType.UGC:
					section = BaseNewGameLibraryMenu.UgcSection;
					break;
				case AssetAuthorType.Workshop:
					section = BaseNewGameLibraryMenu.WorkshopSection;
					break;
				}
				List<Asset_Map> list = CollectionPool.RequestList<Asset_Map>();
				list.AddRange(self.Assets);
				list = section.ToFiltered(list);
				list = section.ToSorted(list);
				list = self.ToSpecialFiltered(list);
				if (list.Count > 0)
				{
					foreach (Asset_Map item in list)
					{
						if (item.IsCurrentVersion())
						{
							self.SelectedAsset = item;
							break;
						}
					}
				}
				else
				{
					self.SelectedAsset = null;
				}
				self.ExecuteMenuOperation(self.QueryParameters);
				CollectionPool.Release(ref list);
			}
		}, this);
	}

	protected override SectionHandle OnAddSections()
	{
		SectionHandle sectionHandle = base.OnAddSections().ClearSections();
		switch (LegacyPlaySourcesMenu.SelectedOperation)
		{
		case LegacyBaseMenu.Operation.Official:
		{
			Section officialSection = BaseNewGameLibraryMenu.OfficialSection;
			officialSection.HasHeader = false;
			sectionHandle.AddSection(officialSection);
			break;
		}
		case LegacyBaseMenu.Operation.CustomMaps:
		{
			Section ugcSection = BaseNewGameLibraryMenu.UgcSection;
			ugcSection.HasHeader = false;
			sectionHandle.AddSection(ugcSection);
			break;
		}
		case LegacyBaseMenu.Operation.Workshop:
		{
			Section workshopSection = BaseNewGameLibraryMenu.WorkshopSection;
			workshopSection.HasHeader = false;
			sectionHandle.AddSection(workshopSection);
			break;
		}
		}
		return sectionHandle;
	}

	protected override List<Asset_Map> ToSpecialFiltered(List<Asset_Map> assets)
	{
		string gamemodeCodename = LegacyPlayGamemodeMenu.GetGamemodeCodename(LegacyPlayGamemodeMenu.SelectedOperationGamemode);
		List<Asset_Map> list = CollectionPool.RequestList<Asset_Map>();
		foreach (Asset_Map asset in assets)
		{
			if (asset.IsShell())
			{
				list.Add(asset);
				continue;
			}
			List<Asset_Gamemode> gamemodes = asset.GetGamemodes();
			foreach (Asset_Gamemode item in gamemodes)
			{
				if (item != null && !(item.codename != gamemodeCodename) && !list.Contains(asset))
				{
					list.Add(asset);
				}
			}
		}
		assets.Clear();
		assets.AddRange(list);
		CollectionPool.Release(ref list);
		return assets;
	}

	protected override void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters)
	{
		base.ExecuteMenuOperation(assetQueryParameters);
		if (SelectedAsset == BaseNewGameLibraryMenu.SteamAddMap)
		{
			SelectedAsset = null;
			Process.Start("steam://url/SteamWorkshopPage/227860");
		}
		else
		{
			MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
			SpNewGameStarterMenu.CloseSignal.Invoke();
			SpNewGameStarterMenu.OpenSignal.Invoke();
		}
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, ReopenSignal, BaseMenu.OnReopen, this);
	}
}
