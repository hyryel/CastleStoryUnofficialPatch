using System;
using Brix.Steam;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Steamworks;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class WorkshopItemStatusMenu : BaseMenu
{
	private const float MinProgress = 0.05f;

	private string _itemTitle;

	private PublishedFileId_t _itemId;

	private float _progress;

	private int _downloadState;

	private static readonly Action<PublishedFileId_t, EItemState, EItemState, WorkshopItemStatusMenu> OnStateChanged = delegate(PublishedFileId_t itemId, EItemState oldState, EItemState newState, WorkshopItemStatusMenu menu)
	{
		BaseMenu.logger.InfoFormat("OnStateChanged: {0}\n{1}:{2}", itemId, oldState, newState);
		if ((oldState & EItemState.k_EItemStateDownloading) == 0 && (newState & EItemState.k_EItemStateDownloading) == EItemState.k_EItemStateDownloading)
		{
			menu._itemId = itemId;
			WorkshopItem itemFromID = WorkshopItem.GetItemFromID(menu._itemId);
			menu._itemTitle = itemFromID.title;
			menu.Progress = 0.05f;
		}
		else if ((newState & EItemState.k_EItemStateDownloading) == 0 && (oldState & EItemState.k_EItemStateDownloading) == EItemState.k_EItemStateDownloading && menu._itemId.m_PublishedFileId == itemId.m_PublishedFileId)
		{
			menu._itemId = default(PublishedFileId_t);
		}
		if ((oldState & EItemState.k_EItemStateNeedsUpdate) == 0 && (newState & EItemState.k_EItemStateNeedsUpdate) == EItemState.k_EItemStateNeedsUpdate)
		{
			menu.DownloadState++;
		}
		else if ((newState & EItemState.k_EItemStateNeedsUpdate) == 0 && (oldState & EItemState.k_EItemStateNeedsUpdate) == EItemState.k_EItemStateNeedsUpdate)
		{
			menu.DownloadState--;
		}
	};

	private static readonly Action<WorkshopItemStatusMenu> OnUpdate = delegate(WorkshopItemStatusMenu menu)
	{
		if (menu._itemId.m_PublishedFileId != 0)
		{
			if (SteamUGC.GetItemDownloadInfo(menu._itemId, out var punBytesDownloaded, out var punBytesTotal) && punBytesTotal != 0)
			{
				menu.Progress = (float)punBytesDownloaded / (float)punBytesTotal;
			}
			else
			{
				menu.Progress = 0.05f;
			}
		}
	};

	private BasePanel ItemStatus
	{
		get
		{
			Action<WorkshopItemStatusPanelComponent> onRefresh = delegate(WorkshopItemStatusPanelComponent panel)
			{
				panel._contentGo.SetActive(IsDownloading);
				panel._progressImage.fillAmount = Progress;
				panel._infoLabel.text = _itemTitle;
			};
			Action<WorkshopItemStatusPanelComponent> onReset = delegate(WorkshopItemStatusPanelComponent panel)
			{
				panel._titleLabel.text = "Downloading Workshop Item...";
			};
			return WorkshopItemStatusPanelComponent.Add(onReset, onRefresh);
		}
	}

	private float Progress
	{
		get
		{
			return _progress;
		}
		set
		{
			_progress = value;
			Refresh();
		}
	}

	private int DownloadState
	{
		get
		{
			return _downloadState;
		}
		set
		{
			_downloadState = ((value >= 0) ? value : 0);
			Refresh();
		}
	}

	private bool IsDownloading => _downloadState != 0;

	public WorkshopItemStatusMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override void CloseSelf()
	{
		base.CloseSelf();
		_itemId = default(PublishedFileId_t);
		_downloadState = 0;
		_itemTitle = string.Empty;
		_progress = 0.05f;
	}

	public static WorkshopItemStatusMenu New(Func<RectTransform> binGetter)
	{
		return new WorkshopItemStatusMenu(binGetter);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanel(ItemStatus);
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, SteamWorkshopAgent.OnStateChanged, OnStateChanged, this).AddSignalConnection(MenuState.Opened, PersistentCoroutineRunner.UpdateSignal, OnUpdate, this);
	}
}
