using System;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Network;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class NetworkCustomServerActionsMenu : BaseMenu
{
	private static class OkButton
	{
		private static readonly Action<LegacyButtonPanelComponent> OnReset = delegate(LegacyButtonPanelComponent panel)
		{
			panel._layoutElement.minHeight = 48f;
			panel._layoutElement.preferredHeight = 48f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._label.text = I2Helper.TryGet("##mainmenu_multiplayer_join").ToUpper();
			panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				if (CanJoin)
				{
					OnJoin();
				}
			});
		};

		private static readonly Action<LegacyButtonPanelComponent> OnRefresh = delegate(LegacyButtonPanelComponent panel)
		{
			panel._button.interactable = CanJoin;
			panel._label.color = ((!CanJoin) ? Colors.Brown : Colors.Black);
		};

		public static IPanel Panel
		{
			get
			{
				LegacyButtonPanelComponent.BasePanel basePanel = LegacyButtonPanelComponent.Build(OnReset, OnRefresh);
				basePanel.As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon);
				basePanel.IgnoreWidgets();
				return basePanel;
			}
		}
	}

	private static class CancelButton
	{
		private static readonly Action<LegacyButtonPanelComponent> OnReset = delegate(LegacyButtonPanelComponent panel)
		{
			panel._layoutElement.minHeight = 48f;
			panel._layoutElement.preferredHeight = 48f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._label.text = I2Helper.TryGet("##mainmenu_multiplayer_cancel").ToUpper();
			panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				OnCancel();
			});
		};

		public static IPanel Panel
		{
			get
			{
				LegacyButtonPanelComponent.BasePanel basePanel = LegacyButtonPanelComponent.Build(OnReset);
				basePanel.As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon);
				basePanel.IgnoreWidgets();
				return basePanel;
			}
		}
	}

	public static bool CanJoin => NetworkCustomServerModalMenu.IsAddressValid(NetworkCustomServerModalMenu.TmpAddress) && NetworkCustomServerModalMenu.IsPortValid(NetworkCustomServerModalMenu.TmpPort);

	private NetworkCustomServerActionsMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkCustomServerActionsMenu Build(Func<RectTransform> getParent)
	{
		return new NetworkCustomServerActionsMenu(getParent);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanel(OkButton.Panel).AddPanel(CancelButton.Panel);
	}

	private static void OnJoin()
	{
		NetworkServersMenu.SelectedMatchData = default(MatchList.MatchData);
		NetworkCustomServerModalMenu.CloseSignal.Invoke();
		NetworkWaitingModalMenu.OpenAsClientSignal.Invoke();
	}

	private static void OnCancel()
	{
		NetworkCustomServerModalMenu.CloseSignal.Invoke();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, NetworkCustomServerModalMenu.OnSetTmpInfoSignal, BaseMenu.OnRefresh, this);
	}
}
