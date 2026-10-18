using System;
using Brix.External.Signals;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public abstract class BaseWorkshopReleaseNotesModalMenu : BaseModalMenu
{
	private BaseMenu _titleMenu;

	protected readonly We.KeyBindingCategoryUtil CameraKeyBindings = new We.KeyBindingCategoryUtil("Camera");

	protected readonly We.KeyBindingCategoryUtil GeneralKeyBindings = new We.KeyBindingCategoryUtil("General");

	protected readonly We.KeyBindingCategoryUtil WeMenuKeyBindings = new We.KeyBindingCategoryUtil("we_menu");

	protected readonly We.KeyBindingCategoryUtil WeTerrainMenuKeyBindings = new We.KeyBindingCategoryUtil("we_terrain_menu");

	protected readonly We.KeyBindingCategoryUtil WeMarkersMenuKeyBindings = new We.KeyBindingCategoryUtil("we_markers_menu");

	protected readonly We.KeyBindingCategoryUtil DebugBindings = new We.KeyBindingCategoryUtil("debug");

	public static readonly Signal BaseOpenSignal = new Signal();

	public static readonly Signal BaseCloseSignal = new Signal();

	public static readonly Signal BaseRefreshSignal = new Signal();

	protected override Action<ModalPanelComponent> OnResetModal => delegate(ModalPanelComponent panel)
	{
		panel._contentFitterPlus._sizeOffset = new Vector2(0f, 32f);
		panel._contentFitterPlus._useBounds = false;
		panel._contentFitterPlus._useAltBounds = false;
		panel._contentFitterPlus._useAllDescendants = false;
	};

	protected abstract string ReleaseNote { get; set; }

	protected static string Title => I2Helper.TryGet("##mode_worldeditor_workshop_notes").ToUpper();

	protected static string Message => I2Helper.TryGet("##mode_worldeditor_workshop_notes_info");

	protected BaseWorkshopReleaseNotesModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override MenuHandle AddMenus()
	{
		return base.AddMenus().AddMenu(_titleMenu);
	}

	protected override PanelList GetHeaderBuilders()
	{
		PanelList headerBuilders = base.GetHeaderBuilders();
		Action<HorizontalLayoutPanelComponent> onReset = delegate(HorizontalLayoutPanelComponent panel)
		{
			panel._layoutGroup.padding = new RectOffset();
			panel._layoutGroup.spacing = 0f;
			panel._image.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
		};
		HorizontalLayoutPanel panelN = HorizontalLayoutPanel.New(onReset);
		_titleMenu = SidebarTitleMenu.Build(() => panelN.GetSocket_2(), Title, hasCloseButton: false);
		headerBuilders.Add(panelN);
		return headerBuilders;
	}

	protected override PanelList GetContentBuilders()
	{
		PanelList builders = base.GetContentBuilders();
		Action<TitleTextPanelComponent> onReset = delegate(TitleTextPanelComponent panel)
		{
			panel._label.text = Message;
			panel._label.color = Colors.Black;
			panel._label.alignment = TextAnchor.MiddleCenter;
			panel._layoutElement.minHeight = panel._label.preferredHeight;
			panel._layoutElement.preferredHeight = panel._label.preferredHeight;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
			panel._fitter.enabled = false;
		};
		TitleTextPanelComponent.BasePanel item = TitleTextPanelComponent.Build(onReset).As(TitleTextPanelComponent.BasePanel.Variant.SidebarHelpText);
		builders.Add(item);
		AddSpacer(ref builders);
		AddReleaseNoteInputField(ref builders, this, () => ReleaseNote, delegate(string value)
		{
			ReleaseNote = value;
		}, delegate
		{
		});
		return builders;
	}

	protected override PanelList GetFooterBuilders()
	{
		PanelList footerBuilders = base.GetFooterBuilders();
		Action<VerticalLayoutPanelComponent> onReset = delegate(VerticalLayoutPanelComponent panel)
		{
			panel._layoutGroup.padding = new RectOffset();
			panel._layoutGroup.spacing = 2f;
			panel._image.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
			panel._fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		};
		PanelList builders = PanelList.Request();
		AddOkButton(ref builders, () => true, delegate
		{
			Publish();
		});
		AddCancelButton(ref builders, () => true, delegate
		{
			Cancel();
		});
		AddSpacerSmall(ref builders);
		ILayoutPanel item = VerticalLayoutPanel.New(onReset).AddPanels(builders);
		footerBuilders.Add(item);
		return footerBuilders;
	}

	private void AddSpacer(ref PanelList builders)
	{
		Action<SpacerPanelComponent> onReset = delegate(SpacerPanelComponent panel)
		{
			panel._layoutElement.minHeight = 6f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
		};
		SpacerPanelComponent.BasePanel item = SpacerPanelComponent.Build(onReset);
		builders.Add(item);
	}

	private void AddSpacerSmall(ref PanelList builders)
	{
		Action<SpacerPanelComponent> onReset = delegate(SpacerPanelComponent panel)
		{
			panel._layoutElement.minHeight = 4f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
		};
		SpacerPanelComponent.BasePanel item = SpacerPanelComponent.Build(onReset);
		builders.Add(item);
	}

	private void AddReleaseNoteInputField(ref PanelList builders, BaseWorkshopReleaseNotesModalMenu menu, Func<string> getValue, Action<string> setValue, Action<bool> setReadyFlag)
	{
		Action<InputFieldPanelComponent> onRefresh = delegate(InputFieldPanelComponent panel)
		{
			string text = getValue();
			panel._inputField.text = text;
			panel._inputFieldPlaceholderText.text = ((!string.IsNullOrEmpty(text)) ? text : ("<" + I2Helper.TryGet("##mode_worldeditor_workshop_none") + ">"));
		};
		Action<InputFieldPanelComponent> onReset = delegate(InputFieldPanelComponent panel)
		{
			panel._inputFieldPlaceholderText.alignment = TextAnchor.UpperLeft;
			panel._inputField.textComponent.alignment = TextAnchor.UpperLeft;
			panel._inputField.lineType = InputField.LineType.MultiLineNewline;
			panel._layoutElement.minHeight = 126f;
			panel._inputField.onEndEdit.RemoveAllListeners();
			panel._inputField.onEndEdit.AddListener(delegate(string dstValue)
			{
				setValue(dstValue);
			});
			panel._inputFieldEventTrigger.triggers.Clear();
			EventTrigger.Entry entry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.Select,
				callback = new EventTrigger.TriggerEvent()
			};
			entry.callback.AddListener(delegate
			{
				menu.CameraKeyBindings.Set(flag: false);
				menu.GeneralKeyBindings.Set(flag: false);
				menu.WeMenuKeyBindings.Set(flag: false);
				menu.WeTerrainMenuKeyBindings.Set(flag: false);
				menu.WeMarkersMenuKeyBindings.Set(flag: false);
				menu.DebugBindings.Set(flag: false);
				setReadyFlag(obj: false);
				Refresh();
			});
			panel._inputFieldEventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry
			{
				eventID = EventTriggerType.Deselect,
				callback = new EventTrigger.TriggerEvent()
			};
			entry2.callback.AddListener(delegate
			{
				setValue(panel._inputField.text);
				menu.CameraKeyBindings.Reset();
				menu.GeneralKeyBindings.Reset();
				menu.WeMenuKeyBindings.Reset();
				menu.WeTerrainMenuKeyBindings.Reset();
				menu.WeMarkersMenuKeyBindings.Reset();
				menu.DebugBindings.Reset();
				setReadyFlag(obj: true);
				Refresh();
			});
			panel._inputFieldEventTrigger.triggers.Add(entry2);
		};
		IPanel item = InputFieldPanelComponent.Build(onReset, onRefresh).As(InputFieldPanelComponent.BasePanel.Variant.Reskin).IgnoreWidgets();
		builders.Add(item);
	}

	private void AddOkButton(ref PanelList builders, Func<bool> getReadyFlag, Action onPress)
	{
		Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
		{
			bool flag = getReadyFlag();
			Color color = ((!flag) ? Colors.Brown : Colors.Black);
			panel._button.interactable = flag;
			panel._label.color = color;
		};
		Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##mode_worldeditor_workshop_publishmenu_publish").ToUpper();
			panel._layoutElement.flexibleWidth = 1f;
			panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				if (getReadyFlag())
				{
					onPress();
				}
			});
		};
		IPanel item = LegacyButtonPanelComponent.Build(onReset, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon).IgnoreWidgets();
		builders.Add(item);
	}

	private void AddCancelButton(ref PanelList builders, Func<bool> getReadyFlag, Action onPress)
	{
		Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
		{
			bool flag = getReadyFlag();
			Color color = ((!flag) ? Colors.Brown : Colors.Black);
			panel._button.interactable = flag;
			panel._label.color = color;
		};
		Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##mode_worldeditor_workshop_publishmenu_cancel").ToUpper();
			panel._layoutElement.flexibleWidth = 1f;
			panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				if (getReadyFlag())
				{
					onPress();
				}
			});
		};
		LegacyButtonPanelComponent.BasePanel basePanel = LegacyButtonPanelComponent.Build(onReset, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon);
		basePanel.IgnoreWidgets();
		builders.Add(basePanel);
	}

	protected abstract void Publish();

	protected void Cancel()
	{
		Close();
	}

	protected override void OpenSelf()
	{
		ReleaseNote = string.Empty;
		base.OpenSelf();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, BaseOpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Loaded, BaseCloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, BaseRefreshSignal, BaseMenu.OnRefresh, this);
	}
}
