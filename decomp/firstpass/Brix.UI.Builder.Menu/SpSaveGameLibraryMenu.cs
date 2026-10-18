using System;
using Brix.Assets;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class SpSaveGameLibraryMenu : BaseSaveMapAssetLibraryMenu
{
	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Singleplayer;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSPSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Single;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.Existing;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	private SpSaveGameLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static SpSaveGameLibraryMenu Build(Func<RectTransform> binGetter)
	{
		return new SpSaveGameLibraryMenu(binGetter);
	}
}
