using System;
using System.Diagnostics;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Steam;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public abstract class BaseWorkshopWaitingModalMenu : BaseModalMenu
{
	private enum ItemState
	{
		Creating,
		Publishing,
		Published
	}

	private BaseMenu _titleMenu;

	private const float TimeoutSpan = 20f;

	private bool _timeoutFlag;

	private bool _itemPublished;

	private static readonly Action<BaseWorkshopWaitingModalMenu> OnUpdate = delegate(BaseWorkshopWaitingModalMenu menu)
	{
		menu.TryEnd();
		menu.Refresh();
	};

	private static readonly Action<PublishedFileId_t, BaseWorkshopWaitingModalMenu> OnItemUpdated = delegate(PublishedFileId_t fileId, BaseWorkshopWaitingModalMenu menu)
	{
		if (menu.ItemId == fileId.m_PublishedFileId)
		{
			menu._itemPublished = true;
		}
	};

	public static readonly Signal BaseOpenSignal = new Signal();

	public static readonly Signal BaseCloseSignal = new Signal();

	public static readonly Signal BaseRefreshSignal = new Signal();

	protected override Action<ModalPanelComponent> OnResetModal => delegate(ModalPanelComponent panel)
	{
		panel._contentFitterPlus._sizeOffset = new Vector2(0f, 32f);
		panel._contentFitterPlus._useAltBounds = true;
		panel._contentFitterPlus._useAllDescendants = false;
	};

	protected abstract string ItemName { get; }

	protected static string Title => I2Helper.TryGet("##mode_worldeditor_workshop_steam").ToUpper();

	protected string Message => I2Helper.TryGet("##mode_worldeditor_workshop_publish_info");

	protected override bool ReloadOnOpen => true;

	private ItemState CurrentItemState
	{
		get
		{
			if (!ItemCreated)
			{
				return ItemState.Creating;
			}
			if (!ItemPublished)
			{
				return ItemState.Publishing;
			}
			return ItemState.Published;
		}
	}

	protected abstract ulong ItemId { get; }

	private bool ItemCreated => ItemId != 0;

	private bool ItemPublished => _itemPublished;

	protected BaseWorkshopWaitingModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
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
		Action<TitleTextPanelComponent> onRefresh = delegate(TitleTextPanelComponent panel)
		{
			panel.gameObject.SetActive(!ItemCreated || !ItemPublished);
		};
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
		TitleTextPanelComponent.BasePanel item = TitleTextPanelComponent.Build(onReset, onRefresh).As(TitleTextPanelComponent.BasePanel.Variant.SidebarHelpText);
		builders.Add(item);
		AddItemHeader(ref builders);
		AddSaveItemCheckbox(ref builders);
		AddCreateItemCheckbox(ref builders);
		AddPublishItemCheckbox(ref builders);
		return builders;
	}

	protected override PanelList GetFooterBuilders()
	{
		PanelList builders = base.GetFooterBuilders();
		Action<VerticalLayoutPanelComponent> onReset = delegate(VerticalLayoutPanelComponent panel)
		{
			panel._layoutGroup.childAlignment = TextAnchor.MiddleCenter;
			panel._layoutGroup.padding = new RectOffset();
			panel._layoutGroup.spacing = 2f;
			panel._image.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
			panel._fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		};
		PanelList builders2 = PanelList.Request();
		AddItemLinkButton(ref builders2);
		AddOkButton(ref builders2);
		AddLoadingWheel(ref builders);
		AddSpacerSmall(ref builders2);
		ILayoutPanel item = VerticalLayoutPanel.New(onReset).AddPanels(builders2);
		builders.Add(item);
		return builders;
	}

	private void AddSpacerSmall(ref PanelList builders)
	{
		Action<SpacerPanelComponent> onRefresh = delegate(SpacerPanelComponent panel)
		{
			panel.gameObject.SetActive(!ItemCreated || !ItemPublished);
		};
		Action<SpacerPanelComponent> onReset = delegate(SpacerPanelComponent panel)
		{
			panel._layoutElement.minHeight = 4f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
		};
		SpacerPanelComponent.BasePanel item = SpacerPanelComponent.Build(onReset, onRefresh);
		builders.Add(item);
	}

	private void AddLoadingWheel(ref PanelList builders)
	{
		Action<TexturePanelComponent> onRefresh = delegate(TexturePanelComponent panel)
		{
			panel.gameObject.SetActive(!ItemCreated || !ItemPublished);
		};
		Action<TexturePanelComponent> onReset = delegate(TexturePanelComponent panel)
		{
			panel._rawImage.gameObject.SetActive(value: false);
			panel._missingImage.gameObject.SetActive(value: false);
			panel._loadingImageGo.gameObject.SetActive(value: true);
			panel._bgImage.enabled = false;
			panel._layoutElement.minWidth = 60f;
			panel._layoutElement.minHeight = 60f;
		};
		TexturePanelComponent.BasePanel item = TexturePanelComponent.Build(onReset, onRefresh);
		builders.Add(item);
	}

	private void AddItemHeader(ref PanelList builders)
	{
		Action<CompoundEntryPanelComponent> onRefresh = delegate(CompoundEntryPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##mode_worldeditor_workshop_item") + " \"" + ItemName + "\"" + ((!ItemCreated) ? string.Empty : (" (ID #:" + ItemId + ")"));
		};
		Action<CompoundEntryPanelComponent> onReset = delegate(CompoundEntryPanelComponent panel)
		{
			panel._label.color = Colors.Black;
			panel._selectImage.sprite = BaseMenu.IconCheck;
			panel._spaceGo.SetActive(value: false);
			panel._selectButton.gameObject.SetActive(value: false);
			panel._button.interactable = false;
			panel._button.onClick.RemoveAllListeners();
			panel._selectButton.interactable = false;
			panel._selectButton.onClick.RemoveAllListeners();
		};
		IPanel item = CompoundEntryPanelComponent.Build(onReset, onRefresh).As(CompoundEntryPanelComponent.Variant.Reskin).IgnoreWidgets();
		builders.Add(item);
	}

	private void AddSaveItemCheckbox(ref PanelList builders)
	{
		Action<CompoundEntryPanelComponent> onRefresh = delegate(CompoundEntryPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##mode_worldeditor_workshop_saved");
		};
		Action<CompoundEntryPanelComponent> onReset = delegate(CompoundEntryPanelComponent panel)
		{
			panel._selectImage.gameObject.SetActive(value: true);
			panel._label.color = Colors.Black;
			panel._selectImage.color = Colors.Black;
			panel._selectImage.sprite = BaseMenu.IconCheck;
			panel._spaceGo.SetActive(value: true);
			panel._button.interactable = false;
			panel._button.onClick.RemoveAllListeners();
			panel._selectButton.interactable = false;
			panel._selectButton.onClick.RemoveAllListeners();
		};
		IPanel item = CompoundEntryPanelComponent.Build(onReset, onRefresh).As(CompoundEntryPanelComponent.Variant.Reskin).IgnoreWidgets();
		builders.Add(item);
	}

	private void AddCreateItemCheckbox(ref PanelList builders)
	{
		Action<CompoundEntryPanelComponent> onRefresh = delegate(CompoundEntryPanelComponent panel)
		{
			panel._selectImage.gameObject.SetActive(ItemCreated);
			panel._label.text = I2Helper.TryGet((!ItemCreated) ? "##mode_worldeditor_workshop_creating" : "##mode_worldeditor_workshop_created");
		};
		Action<CompoundEntryPanelComponent> onReset = delegate(CompoundEntryPanelComponent panel)
		{
			panel._label.color = Colors.Black;
			panel._selectImage.color = Colors.Black;
			panel._selectImage.sprite = BaseMenu.IconCheck;
			panel._spaceGo.SetActive(value: true);
			panel._button.interactable = false;
			panel._button.onClick.RemoveAllListeners();
			panel._selectButton.interactable = false;
			panel._selectButton.onClick.RemoveAllListeners();
		};
		IPanel item = CompoundEntryPanelComponent.Build(onReset, onRefresh).As(CompoundEntryPanelComponent.Variant.Reskin).IgnoreWidgets();
		builders.Add(item);
	}

	private void AddPublishItemCheckbox(ref PanelList builders)
	{
		Action<CompoundEntryPanelComponent> onRefresh = delegate(CompoundEntryPanelComponent panel)
		{
			panel._selectImage.gameObject.SetActive(ItemPublished);
			panel._label.text = I2Helper.TryGet((!ItemPublished) ? "##mode_worldeditor_workshop_publishing" : "##mode_worldeditor_workshop_published");
		};
		Action<CompoundEntryPanelComponent> onReset = delegate(CompoundEntryPanelComponent panel)
		{
			panel._label.color = Colors.Black;
			panel._selectImage.color = Colors.Black;
			panel._selectImage.sprite = BaseMenu.IconCheck;
			panel._spaceGo.SetActive(value: true);
			panel._button.interactable = false;
			panel._button.onClick.RemoveAllListeners();
			panel._selectButton.interactable = false;
			panel._selectButton.onClick.RemoveAllListeners();
		};
		IPanel item = CompoundEntryPanelComponent.Build(onReset, onRefresh).As(CompoundEntryPanelComponent.Variant.Reskin).IgnoreWidgets();
		builders.Add(item);
	}

	private void AddItemLinkButton(ref PanelList builders)
	{
		Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
		{
			panel.gameObject.SetActive(ItemCreated && ItemPublished);
			panel._button.interactable = ItemCreated && ItemPublished;
		};
		Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##mode_worldeditor_workshop_item_open").ToUpper();
			panel._label.color = Colors.Black;
			panel._layoutElement.flexibleWidth = 1f;
			panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				Process.Start("steam://url/CommunityFilePage/" + ItemId);
			});
		};
		IPanel item = LegacyButtonPanelComponent.Build(onReset, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon).IgnoreWidgets();
		builders.Add(item);
	}

	private void AddOkButton(ref PanelList builders)
	{
		Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
		{
			panel.gameObject.SetActive(ItemCreated && ItemPublished);
			panel._button.interactable = ItemCreated && ItemPublished;
		};
		Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##mode_worldeditor_workshop_steamworkshop_done").ToUpper();
			panel._label.color = Colors.Black;
			panel._layoutElement.flexibleWidth = 1f;
			panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				Close();
			});
		};
		IPanel item = LegacyButtonPanelComponent.Build(onReset, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon).IgnoreWidgets();
		builders.Add(item);
	}

	protected override MenuHandle AddMenus()
	{
		return base.AddMenus().AddMenu(_titleMenu);
	}

	protected override void OpenSelf()
	{
		base.OpenSelf();
		_itemPublished = false;
		_timeoutFlag = false;
		PersistentCoroutineRunner.Instance.Relais((Action)delegate
		{
			if (this != null && base.Opened && CurrentItemState != ItemState.Published)
			{
				_timeoutFlag = true;
			}
		}, 20f);
	}

	protected abstract void OnSuccess();

	private void OnFailure(string title, string info)
	{
		GameSignals.Invoke(GameSignals.OnDisplayDialog, title, info, new string[1] { I2Helper.TryGet("##mode_worldeditor_workshop_error_ok").ToUpper() }, new Action[1]
		{
			delegate
			{
				GameSignals.Invoke(GameSignals.OnCloseDialog);
			}
		});
	}

	private void TryEnd()
	{
		if (_timeoutFlag)
		{
			if (CurrentItemState == ItemState.Creating)
			{
				OnFailure(I2Helper.TryGet("##mode_worldeditor_workshop_error_timeoutcreate").ToUpper(), I2Helper.TryGet("##mode_worldeditor_workshop_error_timeoutcreate_info").Replace("#x", 20f.ToString()));
			}
			if (CurrentItemState == ItemState.Publishing)
			{
				OnFailure(I2Helper.TryGet("##mode_worldeditor_workshop_error_timeoutpublish").ToUpper(), I2Helper.TryGet("##mode_worldeditor_workshop_error_timeoutpublish_info").Replace("#x", 20f.ToString()));
			}
		}
		else if (CurrentItemState == ItemState.Published)
		{
			OnSuccess();
		}
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, BaseOpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Opened, BaseCloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, BaseRefreshSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, PersistentCoroutineRunner.UpdateSignal, OnUpdate, this)
			.AddSignalConnection(MenuState.Opened, SteamWorkshopAgent.OnItemUpdated, OnItemUpdated, this);
	}
}
