using System;
using Brix.Assets;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseAssetMenu : BaseMenu
{
	public enum MenuPlayabilityContext
	{
		Single,
		Multi,
		Both
	}

	public enum MenuOperationContext
	{
		Load,
		Save
	}

	public enum MenuTypeContext
	{
		New,
		Existing
	}

	public enum MenuGameContext
	{
		Normal,
		WorldEditor,
		CreditIsland
	}

	protected abstract AssetPlayableContext PlayableContext { get; }

	protected abstract AssetQueryParameters QueryParameters { get; }

	protected abstract MenuPlayabilityContext PlayabilityContext { get; }

	protected abstract MenuOperationContext OperationContext { get; }

	protected abstract MenuTypeContext TypeContext { get; }

	protected abstract MenuGameContext GameContext { get; }

	protected BaseAssetMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}
}
