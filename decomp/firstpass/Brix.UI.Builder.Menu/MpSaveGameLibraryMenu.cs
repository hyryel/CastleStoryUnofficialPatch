using System;
using Brix.Assets;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class MpSaveGameLibraryMenu : BaseSaveMapAssetLibraryMenu
{
	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Multiplayer;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Multi;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.Existing;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	private MpSaveGameLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static MpSaveGameLibraryMenu Build(Func<RectTransform> binGetter)
	{
		return new MpSaveGameLibraryMenu(binGetter);
	}
}
