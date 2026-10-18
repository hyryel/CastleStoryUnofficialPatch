using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class MapWorkshopWaitingModalMenu : BaseWorkshopWaitingModalMenu
{
	protected override string ItemName => WeSaveActionsMenu.DisplayName;

	protected override ulong ItemId => MapWorkshopPublishMenu.MapItemId;

	public static Signal OpenSignal => BaseWorkshopWaitingModalMenu.BaseOpenSignal;

	public static Signal CloseSignal => BaseWorkshopWaitingModalMenu.BaseCloseSignal;

	public static Signal RefreshSignal => BaseWorkshopWaitingModalMenu.BaseRefreshSignal;

	private MapWorkshopWaitingModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static MapWorkshopWaitingModalMenu Build(Func<RectTransform> binGetter)
	{
		return new MapWorkshopWaitingModalMenu(binGetter);
	}

	protected override void OnSuccess()
	{
		MapWorkshopPublishMenu.RefreshSignal.Invoke();
	}
}
