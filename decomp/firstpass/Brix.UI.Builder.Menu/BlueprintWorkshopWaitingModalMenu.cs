using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class BlueprintWorkshopWaitingModalMenu : BaseWorkshopWaitingModalMenu
{
	protected override string ItemName => "SomeBlueprint";

	protected override ulong ItemId => BlueprintWorkshopPublishMenu.BlueprintItemId;

	public static Signal OpenSignal => BaseWorkshopWaitingModalMenu.BaseOpenSignal;

	public static Signal CloseSignal => BaseWorkshopWaitingModalMenu.BaseCloseSignal;

	public static Signal RefreshSignal => BaseWorkshopWaitingModalMenu.BaseRefreshSignal;

	private BlueprintWorkshopWaitingModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static BlueprintWorkshopWaitingModalMenu Build(Func<RectTransform> binGetter)
	{
		return new BlueprintWorkshopWaitingModalMenu(binGetter);
	}

	protected override void OnSuccess()
	{
		MapWorkshopPublishMenu.RefreshSignal.Invoke();
	}
}
