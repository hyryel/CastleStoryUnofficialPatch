using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Input.WorldEditor;
using Brix.Steam;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Smooth.Slinq;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public class WeMapWorkshopEditOptionsMenu : BaseMenu
{
	private static ERemoteStoragePublishedFileVisibility _visibility;

	private static readonly Signal RefreshSignal = new Signal();

	public static Signal AfterChangeSignal = new Signal();

	public static WorkshopItem Item
	{
		get
		{
			if (GameParam.Map == null)
			{
				return null;
			}
			return GameParam.Map.GetWorkshopItem();
		}
	}

	public static WorkshopItemTags Tags
	{
		get
		{
			WorkshopItemTags workshopItemTags = (WorkshopItemTags)0;
			foreach (WorkshopItemTags gameModeWorkshopItemTag in WeModesValidationMenu.GameModeWorkshopItemTags)
			{
				workshopItemTags |= gameModeWorkshopItemTag;
			}
			return workshopItemTags;
		}
	}

	private PanelList Content
	{
		get
		{
			PanelList builders = PanelList.Request();
			AddVisibilityHeader(ref builders);
			AddVisibilityDropdown(ref builders);
			AddTagHeader(ref builders);
			WorkshopItemTags[] array = (WorkshopItemTags[])Enum.GetValues(typeof(WorkshopItemTags));
			for (int i = 0; i < array.Length; i++)
			{
				AddTagCheckbox(ref builders, array[i]);
			}
			Action<SpacerPanelComponent> onReset = delegate(SpacerPanelComponent panel)
			{
				panel._layoutElement.minHeight = 16f;
				panel._layoutElement.flexibleWidth = 1f;
				panel._layoutElement.flexibleHeight = -1f;
			};
			SpacerPanelComponent.BasePanel item = SpacerPanelComponent.Build(onReset);
			builders.Add(item);
			return builders;
		}
	}

	public static ERemoteStoragePublishedFileVisibility Visibility
	{
		get
		{
			return (Item != null) ? Item.visibility : _visibility;
		}
		set
		{
			if (Item == null)
			{
				_visibility = value;
			}
			else
			{
				Item.visibility = value;
			}
			AfterChangeSignal.Invoke();
			RefreshSignal.Invoke();
		}
	}

	private WeMapWorkshopEditOptionsMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static WeMapWorkshopEditOptionsMenu Build(Func<RectTransform> binGetter)
	{
		return new WeMapWorkshopEditOptionsMenu(binGetter);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanels(Content);
	}

	private void AddVisibilityHeader(ref PanelList builders)
	{
		Action<CompoundEntryPanelComponent> onReset = delegate(CompoundEntryPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("str_we_workshop_visibility");
			panel._selectImage.sprite = BaseMenu.IconCheck;
			panel._spaceGo.SetActive(value: false);
			panel._selectButton.gameObject.SetActive(value: false);
			panel._button.onClick.RemoveAllListeners();
			panel._selectButton.onClick.RemoveAllListeners();
		};
		CompoundEntryPanelComponent.BasePanel item = CompoundEntryPanelComponent.Build(onReset);
		builders.Add(item);
	}

	private void AddVisibilityDropdown(ref PanelList builders)
	{
		List<ERemoteStoragePublishedFileVisibility> visibilityEntries = ((ERemoteStoragePublishedFileVisibility[])Enum.GetValues(typeof(ERemoteStoragePublishedFileVisibility))).Slinq().ToList();
		Action<DropdownPanelComponent> onRefresh = delegate(DropdownPanelComponent panel)
		{
			ERemoteStoragePublishedFileVisibility visibility = Visibility;
			int value = visibilityEntries.IndexOf(visibility);
			string text = I2Helper.TryGet(Visibility.ToString());
			panel._valueLabel.text = text;
			panel._dropdown.interactable = true;
			Dropdown.DropdownEvent onValueChanged = panel._dropdown.onValueChanged;
			panel._dropdown.onValueChanged = new Dropdown.DropdownEvent();
			panel._dropdown.value = value;
			panel._dropdown.onValueChanged = onValueChanged;
		};
		Action<DropdownPanelComponent> onReset = delegate(DropdownPanelComponent panel)
		{
			panel._arrowImage.color = Colors.Yellow;
			panel._spacerGo.gameObject.SetActive(value: true);
			panel._keyLayoutElement.gameObject.SetActive(value: false);
			panel._keyLabel.text = I2Helper.TryGet("str_we_workshop_visibility");
			panel._dropdown.options = new List<Dropdown.OptionData>();
			foreach (ERemoteStoragePublishedFileVisibility item2 in visibilityEntries)
			{
				ERemoteStoragePublishedFileVisibility eRemoteStoragePublishedFileVisibility = item2;
				string text = I2Helper.TryGet(eRemoteStoragePublishedFileVisibility.ToString());
				panel._dropdown.options.Add(new Dropdown.OptionData(text));
			}
			panel._dropdown.onValueChanged.RemoveAllListeners();
			panel._dropdown.value = visibilityEntries.IndexOf(Visibility);
			panel._dropdown.onValueChanged.AddListener(delegate(int dstIndex)
			{
				Visibility = visibilityEntries[dstIndex];
			});
		};
		DropdownPanel item = DropdownPanel.New(onReset, onRefresh);
		builders.Add(item);
	}

	private void AddTagHeader(ref PanelList builders)
	{
		Action<CompoundEntryPanelComponent> onReset = delegate(CompoundEntryPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("str_we_workshop_tags");
			panel._selectImage.sprite = BaseMenu.IconCheck;
			panel._spaceGo.SetActive(value: false);
			panel._selectButton.gameObject.SetActive(value: false);
			panel._button.onClick.RemoveAllListeners();
			panel._selectButton.onClick.RemoveAllListeners();
		};
		CompoundEntryPanelComponent.BasePanel item = CompoundEntryPanelComponent.Build(onReset);
		builders.Add(item);
	}

	private void AddTagCheckbox(ref PanelList builders, WorkshopItemTags tagN)
	{
		Action<CompoundEntryPanelComponent> onRefresh = delegate(CompoundEntryPanelComponent panel)
		{
			if (GameParam.Map == null)
			{
				Debug.LogWarning(string.Concat(GetType(), "\nNull Map. Should not happen."));
				BaseMenu.logger.Warn(string.Concat(GetType(), "\nNull Map. Should not happen."));
			}
			else
			{
				panel._selectImage.gameObject.SetActive((Tags & tagN) != 0);
			}
		};
		Action<CompoundEntryPanelComponent> onReset = delegate(CompoundEntryPanelComponent panel)
		{
			panel._label.text = tagN.ToString();
			panel._label.color = Colors.White;
			panel._selectImage.sprite = BaseMenu.IconCheck;
			panel._selectImage.color = Colors.Gray;
			panel._spaceGo.SetActive(value: true);
			panel._button.onClick.RemoveAllListeners();
			panel._selectButton.onClick.RemoveAllListeners();
		};
		CompoundEntryPanelComponent.BasePanel item = CompoundEntryPanelComponent.Build(onReset, onRefresh);
		builders.Add(item);
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this).AddSignalConnection(MenuState.Opened, WorldGridController.RefreshSignal, BaseMenu.OnRefresh, this);
	}
}
