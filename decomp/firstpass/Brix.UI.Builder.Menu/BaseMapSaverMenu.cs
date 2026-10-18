using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game;
using Brix.UI.Builder.Panel;
using Brix.UI.Icons;
using Brix.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public sealed class BaseMapSaverMenu : BaseMapInspectorMenu
{
	private enum PreviewSelectionOption
	{
		FromCamera,
		FromSave
	}

	private Dictionary<PreviewSelectionOption, string> _previewSelectionNameMap = new Dictionary<PreviewSelectionOption, string>(FastEnumIntEqualityComparer<PreviewSelectionOption>.Instance)
	{
		{
			PreviewSelectionOption.FromCamera,
			"##gamemenu_savegame_fromcamera"
		},
		{
			PreviewSelectionOption.FromSave,
			"##gamemenu_savegame_fromsave"
		}
	};

	private readonly List<PreviewSelectionOption> _targetPreviewSelectionOptions = new List<PreviewSelectionOption>
	{
		PreviewSelectionOption.FromCamera,
		PreviewSelectionOption.FromSave
	};

	private PreviewSelectionOption _currentPreviewSelectionOption;

	private static Texture2D _cameraThumbnail;

	private static bool _cameraThumbnailCompleted;

	private static string _userCustomName;

	private readonly We.KeyBindingCategoryUtil _cameraKeyBindings = new We.KeyBindingCategoryUtil("Camera");

	private readonly We.KeyBindingCategoryUtil _generalKeyBindings = new We.KeyBindingCategoryUtil("General");

	private readonly We.KeyBindingCategoryUtil _weMenuKeyBindings = new We.KeyBindingCategoryUtil("we_menu");

	private readonly We.KeyBindingCategoryUtil _debugBindings = new We.KeyBindingCategoryUtil("debug");

	public static Signal OpenSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	private static readonly Signal RefreshSignal = new Signal();

	protected override bool AsModal => false;

	protected override bool IsReskin => true;

	protected override bool ThumbnailIsHeightmap => false;

	protected override bool ThumbnailShowsLoadingIfNull => false;

	protected override bool ThumbnailStretches => true;

	protected override bool ThumbnailShowsBg => true;

	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Any;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Single;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.Existing;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	protected override PanelList Footer
	{
		get
		{
			PanelList panelList = PanelList.Request();
			Action<SpacerPanelComponent> onReset = delegate(SpacerPanelComponent panel)
			{
				panel._layoutElement.flexibleWidth = 1f;
				panel._layoutElement.flexibleHeight = 1f;
			};
			panelList.Add(SpacerPanelComponent.Build(onReset));
			Action<LegacyButtonPanelComponent> onReset2 = delegate(LegacyButtonPanelComponent panel)
			{
				panel._selectionImage.gameObject.SetActive(value: false);
			};
			Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
			{
				panel._keyLabel.text = I2Helper.TryGet("##gamemenu_savegame_name_info");
				panel._keyLabel.color = Colors.LightBlack;
				panel._label.text = I2Helper.TryGet("##gamemenu_savegame_name").ToUpper();
				panel._label.color = Colors.Black;
			};
			LegacyButtonPanelComponent.BasePanel basePanel = LegacyButtonPanelComponent.Build(onReset2, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_DblTxt_RightIcon);
			basePanel.IgnoreWidgets();
			panelList.Add(basePanel);
			Action<InputFieldPanelComponent> onRefresh2 = delegate(InputFieldPanelComponent panel)
			{
				panel._inputField.text = CustomName;
				panel._inputFieldPlaceholderText.text = CustomName;
			};
			Action<InputFieldPanelComponent> onReset3 = delegate(InputFieldPanelComponent panel)
			{
				panel.gameObject.SetActive(value: true);
				panel._inputField.onEndEdit.RemoveAllListeners();
				panel._inputField.onEndEdit.AddListener(delegate(string value)
				{
					CustomName = value;
					RefreshSignal.Invoke();
				});
				panel._inputField.Select();
			};
			panelList.Add(InputFieldPanelComponent.Build(onReset3, onRefresh2).As(InputFieldPanelComponent.BasePanel.Variant.ReskinLeft));
			Action<SpacerPanelComponent> onReset4 = delegate(SpacerPanelComponent panel)
			{
				panel._layoutElement.flexibleWidth = 1f;
				panel._layoutElement.flexibleHeight = 1f;
			};
			panelList.Add(SpacerPanelComponent.Build(onReset4));
			Action<LegacyButtonPanelComponent> onReset5 = delegate(LegacyButtonPanelComponent panel)
			{
				bool flag = true;
				if (SelectedAsset != null)
				{
					flag = !SelectedAsset.IsShell();
					if (SelectedAsset.mapType == Asset_Map.MapType.Map)
					{
						flag = flag && SelectedAsset.authorType != AssetAuthorType.Official && SelectedAsset.authorType != AssetAuthorType.Workshop;
					}
				}
				if (SelectedAsset == null)
				{
					flag = false;
				}
				panel.gameObject.SetActive(flag);
				panel._layoutElement.minHeight = 54f;
				panel._layoutElement.preferredHeight = 54f;
				panel._label.text = I2Helper.TryGet("##mapmenu_delete").ToUpper();
				panel._label.color = Colors.Black;
				panel._label.fontSize = 14;
				panel._selectionImage.sprite = IconKeys._UI_Trash.Get64();
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					OnAskDelete();
					MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Submit);
				});
			};
			panelList.Add(LegacyButtonPanelComponent.Build(onReset5).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon));
			Action<SpacerPanelComponent> onReset6 = delegate(SpacerPanelComponent panel)
			{
				panel._layoutElement.minHeight = 12f;
				panel._layoutElement.preferredHeight = 12f;
				panel._layoutElement.flexibleWidth = 1f;
				panel._layoutElement.flexibleHeight = -1f;
			};
			panelList.Add(SpacerPanelComponent.Build(onReset6));
			Action<LegacyButtonPanelComponent> onRefresh3 = delegate(LegacyButtonPanelComponent panel)
			{
				panel._button.interactable = _cameraThumbnailCompleted;
			};
			Action<LegacyButtonPanelComponent> onReset7 = delegate(LegacyButtonPanelComponent panel)
			{
				panel._label.text = ((!SelectedAsset.IsShell()) ? I2Helper.TryGet("##gamemenu_savegame_overwrite").ToUpper() : I2Helper.TryGet("##gamemenu_savegame_save").ToUpper());
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					OnSave();
					MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.GameStart);
				});
			};
			panelList.Add(LegacyButtonPanelComponent.Build(onReset7, onRefresh3).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyBigPlay));
			return panelList;
		}
	}

	protected override Action<VerticalLayoutPanelComponent> RootLayout_onReset => delegate(VerticalLayoutPanelComponent panel)
	{
		panel._fitter.enabled = false;
		panel._image.enabled = false;
		panel._layoutGroup.padding = new RectOffset(0, 0, 200, 32);
	};

	private PreviewSelectionOption CurrentPreviewSelectionOption
	{
		get
		{
			return _currentPreviewSelectionOption;
		}
		set
		{
			_currentPreviewSelectionOption = value;
			RefreshSignal.Invoke();
		}
	}

	private PanelList CustomNameFields => PanelList.Request();

	private IPanel PreviewField
	{
		get
		{
			Action<DropdownPanelComponent> action = delegate(DropdownPanelComponent panel)
			{
				List<PreviewSelectionOption> values = _targetPreviewSelectionOptions;
				panel._keyBgImage.color = Colors.HalfBlack;
				panel._arrowImage.color = Colors.Yellow;
				string text = I2Helper.TryGet("##gamemenu_savegame_preview");
				panel._keyLabel.text = text;
				panel._dropdown.onValueChanged.RemoveAllListeners();
				panel._dropdown.options = new List<Dropdown.OptionData>();
				foreach (PreviewSelectionOption item in values)
				{
					string text2 = I2Helper.TryGet(_previewSelectionNameMap[item]);
					panel._dropdown.options.Add(new Dropdown.OptionData(text2));
				}
				PreviewSelectionOption currentPreviewSelectionOption = CurrentPreviewSelectionOption;
				int value = values.IndexOf(currentPreviewSelectionOption);
				string text3 = I2Helper.TryGet(_previewSelectionNameMap[currentPreviewSelectionOption]);
				panel._dropdown.value = value;
				panel._valueLabel.text = text3;
				panel._dropdown.onValueChanged.AddListener(delegate(int dstIndex)
				{
					CurrentPreviewSelectionOption = values[dstIndex];
					MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
					RefreshSignal.Invoke();
				});
				panel._itemToggle.onValueChanged.RemoveAllListeners();
				panel._itemToggle.onValueChanged.AddListener(delegate
				{
					MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
				});
				panel._itemEventTrigger.triggers.Clear();
				EventTrigger.Entry entry = new EventTrigger.Entry
				{
					eventID = EventTriggerType.PointerEnter,
					callback = new EventTrigger.TriggerEvent()
				};
				entry.callback.AddListener(delegate
				{
					MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOn);
				});
				panel._itemEventTrigger.triggers.Add(entry);
				EventTrigger.Entry entry2 = new EventTrigger.Entry
				{
					eventID = EventTriggerType.PointerExit,
					callback = new EventTrigger.TriggerEvent()
				};
				entry2.callback.AddListener(delegate
				{
					MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOff);
				});
				panel._itemEventTrigger.triggers.Add(entry2);
				panel._keyLayoutElement.minWidth = 270f;
			};
			return DropdownPanel.New(action, action).As(DropdownPanel.Variant.StandardBig);
		}
	}

	public static Asset_Map SelectedAsset
	{
		get
		{
			return GameParam.SelectedMap;
		}
		set
		{
			GameParam.SelectedMap = value;
		}
	}

	private Asset_Map LoadedMap => GameParam.Map;

	private Asset_Gamemode LoadedGamemode => GameParam.Gamemode;

	protected override bool TitleHiddenIfEmpty => true;

	protected override int TitleFontSize
	{
		get
		{
			if (IsReskin)
			{
				return 22;
			}
			return 28;
		}
	}

	protected override float TitleHeight => 32f;

	protected override string TitleLabel
	{
		get
		{
			if (SelectedAsset == null)
			{
				return string.Empty;
			}
			if (SelectedAsset.IsShell())
			{
				return I2Helper.TryGet("##gamemenu_savegame_newsave");
			}
			return SelectedAsset.customName;
		}
	}

	protected override bool SubtitleHiddenIfEmpty => true;

	protected override string SubtitleLabel
	{
		get
		{
			if (SelectedAsset != null && !SelectedAsset.IsShell())
			{
				return I2Helper.DateTimeUtility.GetLongDateString(SelectedAsset.dateTimeCreated) + "\n" + SelectedAsset.dateTimeCreated.ToLongTimeString();
			}
			return string.Empty;
		}
	}

	protected override Texture2D ThumbnailTex => _cameraThumbnail;

	private string CustomName
	{
		get
		{
			Asset_Map selectedAsset = SelectedAsset;
			if (selectedAsset == null)
			{
				return string.Empty;
			}
			if (_userCustomName != null)
			{
				return _userCustomName;
			}
			if (selectedAsset.IsShell())
			{
				return DefaultCustomName;
			}
			return selectedAsset.customName;
		}
		set
		{
			if (!value.Contains(DefaultCustomNamePrefix) && _userCustomName != value)
			{
				_userCustomName = value;
				RefreshSignal.Invoke();
			}
		}
	}

	private static string DefaultCustomNamePrefix => I2Helper.TryGet("##gamemenu_savegame_untitled_filename") + "_";

	private static string DefaultCustomName => DefaultCustomNamePrefix + DateTime.Now.ToString().Replace(" ", "_").Replace("/", "-")
		.Replace(":", "-");

	protected override bool ReloadOnOpen => true;

	private BaseMapSaverMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static BaseMapSaverMenu Build(Func<RectTransform> binGetter)
	{
		return new BaseMapSaverMenu(binGetter);
	}

	private void OnAskDelete()
	{
		CustomMessageModalMenu.OpenWithContentSignal.Invoke(new CustomMessageModalMenu.CustomMessage(I2Helper.TryGet("##mapmenu_delete_confirmation"), I2Helper.TryGet("##mapmenu_delete_confirmation_message"), null, new List<KeyValuePair<string, Action>>
		{
			new KeyValuePair<string, Action>(I2Helper.TryGet("##mapmenu_delete_confirmation_confirm"), delegate
			{
				OnDelete();
				CustomMessageModalMenu.CloseSignal.Invoke();
			}),
			new KeyValuePair<string, Action>(I2Helper.TryGet("##mapmenu_delete_confirmation_cancel"), delegate
			{
				CustomMessageModalMenu.CloseSignal.Invoke();
			})
		}));
	}

	private void OnDelete()
	{
		SelectedAsset.Disk_Delete();
		SelectedAsset = null;
		BaseMapLibraryMenu.MapReopenSignal.Invoke();
		Close();
	}

	private void OnSave()
	{
		try
		{
			StatsTracker.SaveStats();
			if (SelectedAsset.IsShell())
			{
				SelectedAsset.FillFrom(LoadedMap, includeMapType: false);
				SelectedAsset.customName = CustomName;
				SelectedAsset.thumbnail = _cameraThumbnail;
				SelectedAsset.playableContext = PlayableContext;
				SelectedAsset.mapType = Asset_Map.MapType.Save;
				SelectedAsset.saveType = Asset_Map.SaveType.Manualsave;
				SelectedAsset.Disk_WriteAsNew(AssetAuthorType.Official);
			}
			else
			{
				SelectedAsset.isDebug = LoadedMap.isDebug;
				SelectedAsset.customName = CustomName;
				SelectedAsset.thumbnail = ThumbnailTex;
				SelectedAsset.playableContext = PlayableContext;
				SelectedAsset.mapType = Asset_Map.MapType.Save;
				SelectedAsset.saveType = Asset_Map.SaveType.Manualsave;
				SelectedAsset._lastPlayedGamemode = LoadedMap._lastPlayedGamemode;
				SelectedAsset._lastPlayedGamemodePreset = LoadedMap._lastPlayedGamemodePreset;
				SelectedAsset.Disk_OverwriteOrWriteAsNew(AssetAuthorType.Official);
			}
		}
		catch (Exception ex)
		{
			CustomMessageModalMenu.OpenWithContentSignal.Invoke(new CustomMessageModalMenu.CustomMessage(I2Helper.TryGet("##gamemenu_savegame_failure"), I2Helper.TryGet("##gamemenu_savegame_failure_message") + "\n\n\"" + ex.Message + "\"", null, new List<KeyValuePair<string, Action>>
			{
				new KeyValuePair<string, Action>(I2Helper.TryGet("##gamemenu_savegame_failure_ok"), delegate
				{
					CustomMessageModalMenu.CloseSignal.Invoke();
				})
			}));
		}
		SelectedAsset = null;
		Close();
		GameSignals.Invoke(Container.CloseSaveGameMenuSignal);
		GameSignals.Invoke(GameSignals.OnMenuToggleRequest);
	}

	private void OnCancel()
	{
		SelectedAsset = null;
		Close();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Loaded, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}

	protected override void OpenSelf()
	{
		_cameraThumbnailCompleted = false;
		ScreenShotTaker.CaptureScreenshot(hideMenus: true, createThumbnail: true, compoundMode: false, 1f, delegate(Texture2D dstTex, Texture2D dstThumbTex)
		{
			_cameraThumbnailCompleted = true;
			dstTex.CopyTo(ref _cameraThumbnail);
			RefreshSignal.Invoke();
		});
		CurrentPreviewSelectionOption = PreviewSelectionOption.FromCamera;
		base.OpenSelf();
		_cameraKeyBindings.Set(flag: false);
		_generalKeyBindings.Set(flag: false);
		_weMenuKeyBindings.Set(flag: false);
		_debugBindings.Set(flag: false);
	}

	protected override void CloseSelf()
	{
		_userCustomName = null;
		if (_cameraThumbnail != null)
		{
			UnityEngine.Object.Destroy(_cameraThumbnail);
			_cameraThumbnail = null;
		}
		_cameraKeyBindings.Reset();
		_generalKeyBindings.Reset();
		_weMenuKeyBindings.Reset();
		_debugBindings.Reset();
		base.CloseSelf();
	}
}
