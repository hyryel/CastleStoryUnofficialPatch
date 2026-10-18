using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class CreditIslandLibraryMenu : BaseLoadCreditLibraryMenu
{
	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Singleplayer;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.Official;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Both;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.New;

	protected override MenuGameContext GameContext => MenuGameContext.CreditIsland;

	protected override bool AutoSelectFirstAsset => true;

	private CreditIslandLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static CreditIslandLibraryMenu New(Func<RectTransform> binGetter)
	{
		return new CreditIslandLibraryMenu(binGetter);
	}

	protected override void LoadSelf()
	{
		base.LoadSelf();
		if (!AutoSelectFirstAsset || BaseMapStarterMenu.IsPostUpdate)
		{
			return;
		}
		PersistentCoroutineRunner.Relais(delegate(CreditIslandLibraryMenu self)
		{
			if (self != null)
			{
				List<Asset_Map> list = CollectionPool.RequestList<Asset_Map>();
				list.AddRange(self.Assets);
				list = BaseLoadCreditLibraryMenu.OfficialSection.ToFiltered(list);
				list = BaseLoadCreditLibraryMenu.OfficialSection.ToSorted(list);
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
		CreditIslandStarterMenu.CloseSignal.Invoke();
		CreditIslandStarterMenu.OpenSignal.Invoke();
	}
}
