using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class WorldEditorLibraryMenu : BaseLoadWorldLibraryMenu
{
	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.WorldEditor;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.Official;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Both;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.New;

	protected override MenuGameContext GameContext => MenuGameContext.WorldEditor;

	protected override bool AutoSelectFirstAsset => true;

	private WorldEditorLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static WorldEditorLibraryMenu New(Func<RectTransform> binGetter)
	{
		return new WorldEditorLibraryMenu(binGetter);
	}

	protected override void LoadSelf()
	{
		base.LoadSelf();
		if (!AutoSelectFirstAsset || BaseMapStarterMenu.IsPostUpdate)
		{
			return;
		}
		PersistentCoroutineRunner.Relais(delegate(WorldEditorLibraryMenu self)
		{
			if (self != null)
			{
				List<Asset_Map> list = CollectionPool.RequestList<Asset_Map>();
				list.AddRange(self.Assets);
				list = BaseLoadWorldLibraryMenu.OfficialSection.ToFiltered(list);
				list = BaseLoadWorldLibraryMenu.OfficialSection.ToSorted(list);
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

	protected override void CloseSelf()
	{
		GameParam.SelectedMap = null;
		GameParam.SelectedGamemode = null;
		GameParam.SelectedPreset = null;
		base.CloseSelf();
	}

	protected override void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters)
	{
		base.ExecuteMenuOperation(assetQueryParameters);
		WorldEditorStarterMenu.CloseSignal.Invoke();
		WorldEditorStarterMenu.OpenSignal.Invoke();
	}
}
