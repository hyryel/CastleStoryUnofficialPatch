using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class BlueprintWorkshopReleaseNotesModalMenu : BaseWorkshopReleaseNotesModalMenu
{
	private static string _blueprintReleaseNote = string.Empty;

	public static string BlueprintReleaseNote
	{
		get
		{
			return _blueprintReleaseNote;
		}
		set
		{
			_blueprintReleaseNote = value;
		}
	}

	protected override string ReleaseNote
	{
		get
		{
			return BlueprintReleaseNote;
		}
		set
		{
			BlueprintReleaseNote = value;
		}
	}

	public static Signal OpenSignal => BaseWorkshopReleaseNotesModalMenu.BaseOpenSignal;

	public static Signal CloseSignal => BaseWorkshopReleaseNotesModalMenu.BaseCloseSignal;

	public static Signal RefreshSignal => BaseWorkshopReleaseNotesModalMenu.BaseRefreshSignal;

	private BlueprintWorkshopReleaseNotesModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static BlueprintWorkshopReleaseNotesModalMenu Build(Func<RectTransform> binGetter)
	{
		return new BlueprintWorkshopReleaseNotesModalMenu(binGetter);
	}

	protected override void Publish()
	{
		PublishBlueprints();
	}

	private static void PublishBlueprints()
	{
		BlueprintWorkshopWaitingModalMenu.OpenSignal.Invoke();
		BlueprintWorkshopPublishMenu.SendBlueprintToWorkshop();
		CloseSignal.Invoke();
	}
}
