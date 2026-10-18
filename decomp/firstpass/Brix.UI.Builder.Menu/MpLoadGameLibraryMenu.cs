using System;
using Brix.Assets;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class MpLoadGameLibraryMenu : BaseLoadGameLibraryMenu
{
	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Multiplayer;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Multi;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.Existing;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	private MpLoadGameLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static MpLoadGameLibraryMenu Build(Func<RectTransform> binGetter)
	{
		return new MpLoadGameLibraryMenu(binGetter);
	}

	protected override void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters)
	{
		base.ExecuteMenuOperation(assetQueryParameters);
		MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Confirm);
	}
}
