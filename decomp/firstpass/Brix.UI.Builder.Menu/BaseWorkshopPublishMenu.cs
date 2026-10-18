using System;
using System.Diagnostics;
using Brix.External.Signals;
using Brix.Steam;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Steamworks;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseWorkshopPublishMenu : BaseMenu
{
	protected enum State
	{
		WaitingForAsset,
		WaitingForAssetItem,
		Ready
	}

	private State _currentState;

	public static readonly Signal BaseRefreshSignal = new Signal();

	private Action<PublishedFileId_t, BaseWorkshopPublishMenu> OnItemUpdated = delegate
	{
	};

	public abstract bool IsAssetNull { get; }

	protected abstract bool CanPublishAsset { get; }

	public abstract WorkshopItem AssetItem { get; }

	protected bool AssetItemCreated => AssetItemId != 0;

	protected abstract ulong AssetItemId { get; }

	protected State CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			_currentState = value;
		}
	}

	private IPanel Content => VerticalLayoutPanel.New().AddPanels(SubContent);

	private PanelList SubContent
	{
		get
		{
			PanelList panelList = PanelList.Request();
			panelList.Add(ItemButton);
			panelList.Add(EulaButton);
			panelList.Add(PublishButton);
			return panelList;
		}
	}

	private IPanel ItemButton
	{
		get
		{
			Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
			{
				panel.gameObject.SetActive(AssetItemCreated);
			};
			Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
			{
				panel._label.text = I2Helper.TryGet("##mode_worldeditor_workshop_publish_info").ToUpper();
				panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					Process.Start("steam://url/CommunityFilePage/" + AssetItemId);
				});
			};
			return LegacyButtonPanelComponent.Build(onReset, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon).IgnoreWidgets();
		}
	}

	private IPanel EulaButton
	{
		get
		{
			Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
			{
				panel.gameObject.SetActive(!AssetItemCreated);
			};
			Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
			{
				panel._label.text = I2Helper.TryGet("##mode_worldeditor_workshop_error_eula_open").ToUpper();
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					Process.Start("steam://url/SSA");
				});
			};
			return LegacyButtonPanelComponent.Build(onReset, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon).IgnoreWidgets();
		}
	}

	private IPanel PublishButton
	{
		get
		{
			Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
			{
				Color yellow = Colors.Yellow;
				panel._label.color = yellow;
				panel._button.interactable = CanPublishAsset;
				panel._label.text = ((!AssetItemCreated) ? I2Helper.TryGet("##mode_worldeditor_workshop_savecreate").ToUpper() : I2Helper.TryGet("##mode_worldeditor_workshop_savepublish").ToUpper());
			};
			Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
			{
				panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					if (CanPublishAsset)
					{
						PublishAsset();
						BaseRefreshSignal.Invoke();
					}
				});
			};
			return LegacyButtonPanelComponent.Build(onReset, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon).IgnoreWidgets();
		}
	}

	protected override bool ReloadOnOpen => true;

	protected BaseWorkshopPublishMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public abstract void PublishAsset();

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanel(Content);
	}

	protected override void OpenSelf()
	{
		if (IsAssetNull)
		{
			CurrentState = State.WaitingForAsset;
		}
		else
		{
			WorkshopItem assetItem = AssetItem;
			if (AssetItemCreated && assetItem == null)
			{
				CurrentState = State.WaitingForAssetItem;
			}
			else
			{
				CurrentState = State.Ready;
			}
		}
		base.OpenSelf();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, BaseRefreshSignal, BaseMenu.OnRefresh, this).AddSignalConnection(MenuState.Opened, SteamWorkshopAgent.OnItemUpdated, OnItemUpdated, this);
	}
}
