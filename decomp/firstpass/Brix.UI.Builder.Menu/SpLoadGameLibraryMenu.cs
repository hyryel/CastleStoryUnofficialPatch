using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class SpLoadGameLibraryMenu : BaseLoadGameLibraryMenu
{
	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Singleplayer;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSPSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Single;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.Existing;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	protected override bool AutoSelectFirstAsset => true;

	private SpLoadGameLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static SpLoadGameLibraryMenu Build(Func<RectTransform> binGetter)
	{
		return new SpLoadGameLibraryMenu(binGetter);
	}

	protected override void LoadSelf()
	{
		base.LoadSelf();
		if (!AutoSelectFirstAsset || BaseMapStarterMenu.IsPostUpdate)
		{
			return;
		}
		PersistentCoroutineRunner.Relais(delegate(SpLoadGameLibraryMenu self)
		{
			if (self != null)
			{
				List<Asset_Map> list = CollectionPool.RequestList<Asset_Map>();
				list.AddRange(self.Assets);
				list = BaseLoadGameLibraryMenu.GeneralSection.ToFiltered(list);
				list = BaseLoadGameLibraryMenu.GeneralSection.ToSorted(list);
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

	protected override void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters)
	{
		base.ExecuteMenuOperation(assetQueryParameters);
		MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
		SpLoadGameStarterMenu.CloseSignal.Invoke();
		SpLoadGameStarterMenu.OpenSignal.Invoke();
	}
}
