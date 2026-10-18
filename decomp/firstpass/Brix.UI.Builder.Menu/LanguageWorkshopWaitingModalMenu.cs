using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class LanguageWorkshopWaitingModalMenu : BaseWorkshopWaitingModalMenu
{
	protected override string ItemName => LanguageWorkshopPublishMenu.Language.language_name;

	protected override ulong ItemId => LanguageWorkshopPublishMenu.LanguageItemId;

	public static Signal OpenSignal => BaseWorkshopWaitingModalMenu.BaseOpenSignal;

	public static Signal CloseSignal => BaseWorkshopWaitingModalMenu.BaseCloseSignal;

	public static Signal RefreshSignal => BaseWorkshopWaitingModalMenu.BaseRefreshSignal;

	private LanguageWorkshopWaitingModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static LanguageWorkshopWaitingModalMenu Build(Func<RectTransform> binGetter)
	{
		return new LanguageWorkshopWaitingModalMenu(binGetter);
	}

	protected override void OnSuccess()
	{
		LanguageWorkshopPublishMenu.RefreshSignal.Invoke();
	}
}
