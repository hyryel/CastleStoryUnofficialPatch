using System;
using Brix.External.Signals;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public sealed class ServerInspectorMenu : BaseMenu
{
	private static class VerticalLayout
	{
		public static ILayoutPanel Panel => VerticalLayoutPanel.New(OnReset);

		private static Action<VerticalLayoutPanelComponent> OnReset => delegate(VerticalLayoutPanelComponent panel)
		{
			panel._fitter.enabled = false;
			panel._image.enabled = false;
		};
	}

	private static class Spacer
	{
		public static IPanel Panel => SpacerPanelComponent.Build(OnReset);

		private static Action<SpacerPanelComponent> OnReset => delegate(SpacerPanelComponent panel)
		{
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = 1f;
		};
	}

	private static class Spacer12px
	{
		public static IPanel Panel => SpacerPanelComponent.Build(OnReset);

		private static Action<SpacerPanelComponent> OnReset => delegate(SpacerPanelComponent panel)
		{
			panel._layoutElement.minHeight = -1f;
			panel._layoutElement.preferredHeight = 12f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
		};
	}

	private static class Button_Play
	{
		public static IPanel Panel => LegacyButtonPanelComponent.Build(OnReset, OnRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyBigPlay);

		private static Action<LegacyButtonPanelComponent> OnReset => delegate(LegacyButtonPanelComponent panel)
		{
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				NetworkWaitingModalMenu.OpenAsClientSignal.Invoke();
			});
		};

		private static Action<LegacyButtonPanelComponent> OnRefresh => delegate(LegacyButtonPanelComponent panel)
		{
			bool canJoin = NetworkServersMenu.CanJoin;
			panel._label.text = I2Helper.TryGet((!canJoin) ? "##mainmenu_multiplayer_selectgame" : "##mainmenu_multiplayer_enterlobby").ToUpper();
			panel._label.color = ((!canJoin) ? Colors.PlayButtonLabel_Disabled : Colors.PlayButtonLabel_Enabled);
			panel._selectionImage.color = ((!canJoin) ? Colors.PlayButtonBg_Disabled : Colors.PlayButtonBg_Enabled);
			panel._button.colors = ((!canJoin) ? ColorBlocks.PlayButtonBar_Disabled : ColorBlocks.PlayButtonBar_Enabled);
			panel._button.interactable = canJoin;
		};
	}

	private static class Checkbox_ShowFull
	{
		private static ColorBlock _colorBlock = new ColorBlock
		{
			colorMultiplier = 1f,
			disabledColor = Colors.LightYellow,
			normalColor = Colors.LightYellow,
			highlightedColor = Colors.LighterYellow,
			pressedColor = Colors.White
		};

		public static IPanel Panel => LegacyButtonPanelComponent.Build(OnReset, OnRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskinCheckbox);

		private static Action<LegacyButtonPanelComponent> OnReset => delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##mainmenu_multiplayer_showfull").ToUpper();
			panel._label.color = Colors.Black;
			panel._keyLabel.text = I2Helper.TryGet("##mainmenu_multiplayer_showfull_info");
			panel._keyLabel.color = Colors.Black;
			panel._selectionImage.color = Colors.Black;
			panel._button.colors = _colorBlock;
			panel._button.interactable = true;
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				NetworkServersMenu.IsShowingFull = !NetworkServersMenu.IsShowingFull;
				RefreshSignal.Invoke();
			});
		};

		private static Action<LegacyButtonPanelComponent> OnRefresh => delegate(LegacyButtonPanelComponent panel)
		{
			bool flag = NetworkServersMenu.SelectedSource == NetworkServersMenu.Source.Public;
			panel.gameObject.SetActive(flag);
			if (flag)
			{
				panel._selectionImage.gameObject.SetActive(NetworkServersMenu.IsShowingFull);
			}
		};
	}

	private static readonly Signal RefreshSignal = new Signal();

	public ServerInspectorMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static ServerInspectorMenu New(Func<RectTransform> binGetter)
	{
		return new ServerInspectorMenu(binGetter);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanel(VerticalLayout.Panel.AddPanel(Spacer.Panel).AddPanel(Button_Play.Panel));
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this).AddSignalConnection(MenuState.Opened, LegacyMultiplayerJoinMenu.OnSelectOperationSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, NetworkServersMenu.OnSelectServerSignal, BaseMenu.OnRefresh, this);
	}
}
