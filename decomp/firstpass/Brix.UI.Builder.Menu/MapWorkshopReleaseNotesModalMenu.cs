using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class MapWorkshopReleaseNotesModalMenu : BaseWorkshopReleaseNotesModalMenu
{
	private static string _mapReleaseNote = string.Empty;

	public static string MapReleaseNote
	{
		get
		{
			return _mapReleaseNote;
		}
		set
		{
			_mapReleaseNote = value;
		}
	}

	protected override string ReleaseNote
	{
		get
		{
			return MapReleaseNote;
		}
		set
		{
			MapReleaseNote = value;
		}
	}

	public static Signal OpenSignal => BaseWorkshopReleaseNotesModalMenu.BaseOpenSignal;

	public static Signal CloseSignal => BaseWorkshopReleaseNotesModalMenu.BaseCloseSignal;

	public static Signal RefreshSignal => BaseWorkshopReleaseNotesModalMenu.BaseRefreshSignal;

	private MapWorkshopReleaseNotesModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static MapWorkshopReleaseNotesModalMenu Build(Func<RectTransform> binGetter)
	{
		return new MapWorkshopReleaseNotesModalMenu(binGetter);
	}

	protected override void Publish()
	{
		PublishMap();
	}

	private static void PublishMap()
	{
		WeSaveActionsMenu.Save(delegate(bool success)
		{
			if (success)
			{
				MapWorkshopWaitingModalMenu.OpenSignal.Invoke();
				MapWorkshopPublishMenu.SendMapToWorkshop();
			}
			CloseSignal.Invoke();
		});
	}
}
