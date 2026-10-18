using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Network;
using Brix.UI.Builder.Panel;
using Brix.UI.Icons;
using Brix.Utils;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public abstract class BaseMapStarterMenu : BaseMapInspectorMenu
{
	private enum State
	{
		Undefined,
		Sp_SelectMap,
		Sp_UpdateMap,
		Sp_StartGame,
		Sp_StartWorldEditor,
		Sp_StartCreditIsland,
		MpHost_NoMap,
		MpHost_SelectMap,
		MpHost_UpdateMap,
		MpHost_NotReady,
		MpHost_WaitingForPlayers,
		MpHost_WaitingForDownloadingPlayers,
		MpHost_Start,
		MpClient_NotReady,
		MpClient_WaitingForHost,
		MpClient_WaitingForDownload
	}

	private enum DisplayedMapStatus
	{
		Ready,
		Outdated,
		Undefined
	}

	private const string WeGamemodeCodeName = "world_editor";

	private static bool _updateInProgress;

	private static int _updateId = -1;

	public static bool IsPostUpdate;

	private static readonly Action<BaseMapStarterMenu> OnSetMap = delegate(BaseMapStarterMenu menu)
	{
		menu.OnAfterSetMap();
	};

	private static readonly Action<BaseMapStarterMenu> OnSetMode = delegate(BaseMapStarterMenu menu)
	{
		menu.Refresh();
	};

	private static readonly Action<BaseMapStarterMenu> OnSetPreset = delegate(BaseMapStarterMenu menu)
	{
		menu.Refresh();
	};

	private static readonly Action<BaseMapStarterMenu> OnAnyToggleReady = delegate(BaseMapStarterMenu menu)
	{
		if (menu.PlayabilityContext == MenuPlayabilityContext.Multi)
		{
			menu.Refresh();
		}
	};

	protected override bool AsModal => true;

	protected override bool HasHeader => false;

	protected override AssetPlayableContext PlayableContext => AssetPlayableContext.Any;

	protected override AssetQueryParameters QueryParameters => AssetQueryParameters.AllSources;

	protected override MenuPlayabilityContext PlayabilityContext => MenuPlayabilityContext.Single;

	protected override MenuOperationContext OperationContext => MenuOperationContext.Load;

	protected override MenuTypeContext TypeContext => MenuTypeContext.Existing;

	protected override MenuGameContext GameContext => MenuGameContext.Normal;

	private bool ShowSelected => !IsMultiplayer || (LobbyMenu.IsHost && LobbyMenu.IsMapLibrary);

	protected Asset_Map DisplayedMap
	{
		get
		{
			if (PlayabilityContext == MenuPlayabilityContext.Multi && !LobbyMenu.IsHost && LobbyMenu.IsDownloading)
			{
				return null;
			}
			return (!ShowSelected) ? GameParam.Map : GameParam.SelectedMap;
		}
	}

	protected Asset_Gamemode DisplayedMode
	{
		get
		{
			return (!ShowSelected) ? GameParam.Gamemode : GameParam.SelectedGamemode;
		}
		set
		{
			if (ShowSelected)
			{
				BaseMapLibraryMenu.SetSelectedGamemode(DisplayedMap, value, PlayableContext);
			}
			else
			{
				BaseMapLibraryMenu.SetLoadedGamemode(DisplayedMap, value, PlayableContext);
			}
		}
	}

	protected Asset_GamemodePreset DisplayedPreset
	{
		get
		{
			return (!ShowSelected) ? GameParam.Preset : GameParam.SelectedPreset;
		}
		set
		{
			if (ShowSelected)
			{
				BaseMapLibraryMenu.SetSelectedPreset(value);
			}
			else
			{
				BaseMapLibraryMenu.SetLoadedPreset(value);
			}
		}
	}

	protected override string SubtitleLabel => TypeContext switch
	{
		MenuTypeContext.New => (!IsReskin || DisplayedMap == null) ? string.Empty : DisplayedMap.description, 
		MenuTypeContext.Existing => (!IsReskin || DisplayedMap == null) ? string.Empty : $"{I2Helper.DateTimeUtility.GetLongDateString(DisplayedMap.dateTimeCreated)}\n{DisplayedMap.dateTimeCreated.ToLongTimeString()}", 
		_ => string.Empty, 
	};

	protected override PanelList Footer
	{
		get
		{
			PanelList panelList = PanelList.Request();
			Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
			{
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					if (IsNotMultiplayerOrIsHost && !LobbyMenu.IsMapLibrary && PlayabilityContext == MenuPlayabilityContext.Multi)
					{
						LobbyMenu.ToggleMapLibrary();
					}
				});
			};
			Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
			{
				bool flag = IsNotMultiplayerOrIsHost && !LobbyMenu.IsMapLibrary && PlayabilityContext == MenuPlayabilityContext.Multi;
				panel._keyLabel.text = I2Helper.TryGet("##map");
				panel._keyLabel.color = Colors.Black;
				panel._label.text = ((DisplayedMap != null) ? DisplayedMap.displayName.ToUpper() : I2Helper.TryGet("##map_noneselected").ToUpper());
				panel._label.color = Colors.Black;
				panel._selectionImage.color = Colors.Black;
				panel._selectionImage.sprite = IconKeys._UI_Big_Arrow_Right.Get64();
				panel._selectionImage.gameObject.SetActive(flag);
				panel._button.interactable = flag;
			};
			LegacyButtonPanelComponent.BasePanel basePanel = LegacyButtonPanelComponent.Build(onReset, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_DblTxt_RightIcon);
			basePanel.IgnoreWidgets();
			panelList.Add(basePanel);
			Action<DropdownPanelComponent> action = delegate(DropdownPanelComponent panel)
			{
				panel._keyLabel.text = I2Helper.TryGet("##mode");
				if (DisplayedMap == null)
				{
					panel.gameObject.SetActive(value: false);
				}
				else
				{
					panel.gameObject.SetActive(value: true);
					List<Asset_Gamemode> mapModes = GetGamemodes(DisplayedMap);
					bool isEditable = !IsWorldEditorMenu && !IsExistingAsset;
					isEditable = isEditable && !IsCreditIslandMenu;
					isEditable = isEditable && mapModes.Count > 1;
					isEditable = isEditable && IsNotMultiplayerOrIsHost;
					isEditable = isEditable && IsNewGame;
					panel._dropdown.interactable = isEditable;
					panel._keyLabel.color = Colors.Black;
					panel._valueLabel.color = Colors.Black;
					panel._arrowImage.color = Colors.Black;
					panel._arrowImage.gameObject.SetActive(isEditable);
					if (mapModes.Count == 0)
					{
						panel._valueLabel.text = I2Helper.TryGet("##mode_nonesupported").ToUpper();
					}
					else
					{
						panel._dropdown.options = new List<Dropdown.OptionData>();
						foreach (Asset_Gamemode item in mapModes)
						{
							if (item == null)
							{
								BaseMenu.logger.Info("null mode detected..\ntell Kim to fix asap");
							}
							else
							{
								panel._dropdown.options.Add(new Dropdown.OptionData(item.displayName.ToUpper()));
							}
						}
						panel._dropdown.onValueChanged.RemoveAllListeners();
						panel._dropdown.value = ((DisplayedMode != null) ? mapModes.IndexOf(DisplayedMode) : (-1));
						panel._dropdown.onValueChanged.AddListener(delegate(int dstIndex)
						{
							if (isEditable)
							{
								DisplayedMode = mapModes[dstIndex];
								MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
							}
						});
						panel._itemToggle.onValueChanged.RemoveAllListeners();
						panel._itemToggle.onValueChanged.AddListener(delegate
						{
							if (isEditable)
							{
								MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
							}
						});
						panel._itemEventTrigger.triggers.Clear();
						EventTrigger.Entry entry = new EventTrigger.Entry
						{
							eventID = EventTriggerType.PointerEnter,
							callback = new EventTrigger.TriggerEvent()
						};
						entry.callback.AddListener(delegate
						{
							if (isEditable)
							{
								MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOn);
							}
						});
						panel._itemEventTrigger.triggers.Add(entry);
						EventTrigger.Entry entry2 = new EventTrigger.Entry
						{
							eventID = EventTriggerType.PointerExit,
							callback = new EventTrigger.TriggerEvent()
						};
						entry2.callback.AddListener(delegate
						{
							if (isEditable)
							{
								MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOff);
							}
						});
						panel._itemEventTrigger.triggers.Add(entry2);
						panel._valueLabel.text = (IsExistingAsset ? mapModes[0].displayName.ToUpper() : ((DisplayedMode != null) ? DisplayedMode.displayName.ToUpper() : I2Helper.TryGet("##mode_noneselected").ToUpper()));
					}
				}
			};
			panelList.Add(DropdownPanel.New(action, action).As((!IsReskin) ? DropdownPanel.Variant.StandardBig : DropdownPanel.Variant.StandardBigReskin));
			Action<DropdownPanelComponent> action2 = delegate(DropdownPanelComponent panel)
			{
				panel._keyLabel.text = I2Helper.TryGet("##mode_preset");
				List<Asset_Gamemode> gamemodes = GetGamemodes(DisplayedMap);
				if (gamemodes.Count == 0)
				{
					panel.gameObject.SetActive(value: false);
				}
				else
				{
					panel.gameObject.SetActive(value: true);
					List<Asset_GamemodePreset> presets = new List<Asset_GamemodePreset>();
					if (IsExistingAsset)
					{
						Asset_GamemodePreset lastPlayedGamemodePreset = DisplayedMap.GetLastPlayedGamemodePreset();
						if (lastPlayedGamemodePreset == null)
						{
							panel.gameObject.SetActive(value: false);
							return;
						}
						presets.Add(lastPlayedGamemodePreset);
					}
					else if (DisplayedMode != null)
					{
						presets.AddRange(DisplayedMode.GetPresets(QueryParameters, PlayableContext));
					}
					bool flag = presets.Count != 0;
					bool isEditable = !IsWorldEditorMenu;
					isEditable = isEditable && !IsCreditIslandMenu;
					isEditable = isEditable && flag;
					isEditable = isEditable && IsNotMultiplayerOrIsHost;
					isEditable = isEditable && IsNewGame;
					isEditable = isEditable && !IsExistingAsset;
					panel._dropdown.interactable = isEditable;
					panel._keyLabel.color = Colors.Black;
					panel._valueLabel.color = Colors.Black;
					panel._arrowImage.color = Colors.Black;
					panel._arrowImage.gameObject.SetActive(isEditable);
					panel._dropdown.options = new List<Dropdown.OptionData>();
					foreach (Asset_GamemodePreset item2 in presets)
					{
						panel._dropdown.options.Add(new Dropdown.OptionData(item2.displayName.ToUpper()));
					}
					panel._dropdown.onValueChanged.RemoveAllListeners();
					panel._dropdown.value = presets.IndexOf(DisplayedPreset);
					panel._dropdown.onValueChanged.AddListener(delegate(int dstIndex)
					{
						if (isEditable)
						{
							DisplayedPreset = presets[dstIndex];
							MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
						}
					});
					panel._itemToggle.onValueChanged.RemoveAllListeners();
					panel._itemToggle.onValueChanged.AddListener(delegate
					{
						if (isEditable)
						{
							MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
						}
					});
					panel._itemEventTrigger.triggers.Clear();
					EventTrigger.Entry entry = new EventTrigger.Entry
					{
						eventID = EventTriggerType.PointerEnter,
						callback = new EventTrigger.TriggerEvent()
					};
					entry.callback.AddListener(delegate
					{
						if (isEditable)
						{
							MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOn);
						}
					});
					panel._itemEventTrigger.triggers.Add(entry);
					EventTrigger.Entry entry2 = new EventTrigger.Entry
					{
						eventID = EventTriggerType.PointerExit,
						callback = new EventTrigger.TriggerEvent()
					};
					entry2.callback.AddListener(delegate
					{
						if (isEditable)
						{
							MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOff);
						}
					});
					panel._itemEventTrigger.triggers.Add(entry2);
					panel._valueLabel.text = ((DisplayedPreset != null) ? DisplayedPreset.displayName.ToUpper() : I2Helper.TryGet("##mode_preset_noneselected").ToUpper());
				}
			};
			panelList.Add(DropdownPanel.New(action2, action2).As((!IsReskin) ? DropdownPanel.Variant.StandardBig : DropdownPanel.Variant.StandardBigReskin));
			Action<SpacerPanelComponent> onReset2 = delegate(SpacerPanelComponent panel)
			{
				panel._layoutElement.flexibleWidth = 1f;
				panel._layoutElement.flexibleHeight = 1f;
			};
			panelList.Add(SpacerPanelComponent.Build(onReset2));
			if (PlayabilityContext == MenuPlayabilityContext.Multi && LobbyMenu.IsHost)
			{
				Action<DropdownPanelComponent> action3 = delegate(DropdownPanelComponent panel)
				{
					bool flag = !LobbyMenu.IsMapLibrary;
					panel.gameObject.SetActive(flag);
					if (flag)
					{
						panel._keyLabel.text = I2Helper.TryGet("##mainmenu_multiplayer_privacy");
						panel._valueLabel.text = I2Helper.TryGet(UNetManager.GameVisibilityNameMap[UNetManager.instance.GameVisibility]).ToUpper();
						panel._dropdown.interactable = true;
						panel._keyLabel.color = Colors.Black;
						panel._valueLabel.color = Colors.Black;
						panel._arrowImage.color = Colors.Black;
						panel._arrowImage.gameObject.SetActive(value: true);
						panel._dropdown.options = new List<Dropdown.OptionData>();
						foreach (ELobbyType gameVisibility in UNetManager.GameVisibilityList)
						{
							panel._dropdown.options.Add(new Dropdown.OptionData(I2Helper.TryGet(UNetManager.GameVisibilityNameMap[gameVisibility]).ToUpper()));
						}
						panel._dropdown.onValueChanged.RemoveAllListeners();
						panel._dropdown.value = UNetManager.GameVisibilityList.IndexOf(UNetManager.instance.GameVisibility);
						panel._dropdown.onValueChanged.AddListener(delegate(int dstIndex)
						{
							UNetManager.instance.GameVisibility = UNetManager.GameVisibilityList[dstIndex];
							panel._valueLabel.text = I2Helper.TryGet(UNetManager.GameVisibilityNameMap[UNetManager.instance.GameVisibility]).ToUpper();
							MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
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
					}
				};
				panelList.Add(DropdownPanel.New(action3, action3).As((!IsReskin) ? DropdownPanel.Variant.StandardBig : DropdownPanel.Variant.StandardBigReskin));
			}
			if (IsReskin && !IsLobby)
			{
				Action<LegacyButtonPanelComponent> action4 = delegate(LegacyButtonPanelComponent panel)
				{
					bool flag = true;
					if (DisplayedMap != null)
					{
						flag = !DisplayedMap.IsShell();
						if (DisplayedMap.mapType == Asset_Map.MapType.Map)
						{
							flag = flag && DisplayedMap.authorType != AssetAuthorType.Official && DisplayedMap.authorType != AssetAuthorType.Workshop && DisplayedMap.codename != "world_editor";
						}
					}
					if (DisplayedMap == null)
					{
						flag = false;
					}
					panel.gameObject.SetActive(flag);
					panel._label.text = I2Helper.TryGet("##mapmenu_delete").ToUpper();
					panel._label.color = Colors.Black;
					panel._keyLabel.text = I2Helper.TryGet("##mapmenu_delete_info");
					panel._keyLabel.color = Colors.Black;
					panel._selectionImage.sprite = IconKeys._UI_Big_Trash.Get64();
					panel._button.onClick.RemoveAllListeners();
					panel._button.onClick.AddListener(delegate
					{
						OnAskDelete();
						MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Submit);
					});
				};
				panelList.Add(LegacyButtonPanelComponent.Build(action4, action4).As((!IsReskin) ? LegacyButtonPanelComponent.BasePanel.Variant.LegacyBig : LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_DblTxt_RightIcon));
			}
			if (IsMultiplayer)
			{
				Action<LegacyButtonPanelComponent> onReset3 = delegate(LegacyButtonPanelComponent panel)
				{
					panel._label.text = I2Helper.TryGet("##mainmenu_lobby_ready").ToUpper();
					panel._button.onClick.RemoveAllListeners();
					panel._button.onClick.AddListener(delegate
					{
						if (LobbyMenu.CanSetReady)
						{
							LobbyMenu.SetIsReady(!LobbyMenu.GetIsReady());
							MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Submit);
							Refresh();
						}
					});
				};
				Action<LegacyButtonPanelComponent> onRefresh2 = delegate(LegacyButtonPanelComponent panel)
				{
					panel.gameObject.SetActive(!LobbyMenu.IsMapLibrary);
					panel._label.color = ((!LobbyMenu.CanSetReady) ? Colors.Brown : Colors.Black);
					panel._keyLabel.color = ((!LobbyMenu.CanSetReady) ? Colors.Brown : Colors.Black);
					panel._selectionImage.gameObject.SetActive(LobbyMenu.GetIsReady());
					panel._keyLabel.text = I2Helper.TryGet(LobbyMenu.IsHost ? "##mainmenu_lobby_ready_info_host" : ((!LobbyMenu.IsDownloading) ? "##mainmenu_lobby_ready_info_client" : "##mainmenu_lobby_waitingfordownload"));
				};
				panelList.Add(LegacyButtonPanelComponent.Build(onReset3, onRefresh2).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskinCheckbox));
			}
			if (IsReskin)
			{
				Action<SpacerPanelComponent> onReset4 = delegate(SpacerPanelComponent panel)
				{
					panel._layoutElement.minHeight = -1f;
					panel._layoutElement.preferredHeight = 12f;
					panel._layoutElement.flexibleWidth = 1f;
					panel._layoutElement.flexibleHeight = -1f;
				};
				panelList.Add(SpacerPanelComponent.Build(onReset4));
			}
			Action<LegacyButtonPanelComponent> action5 = delegate(LegacyButtonPanelComponent panel)
			{
				bool flag = true;
				string term = string.Empty;
				switch (CurrentState)
				{
				case State.Undefined:
					term = "???";
					flag = false;
					break;
				case State.Sp_SelectMap:
					term = "##mainmenu_selectamap";
					flag = false;
					break;
				case State.Sp_UpdateMap:
					term = "##mainmenu_update";
					break;
				case State.Sp_StartGame:
					term = "##mainmenu_play";
					break;
				case State.Sp_StartWorldEditor:
					term = "##mainmenu_edit";
					break;
				case State.Sp_StartCreditIsland:
					term = "##mainmenu_play";
					break;
				case State.MpHost_NoMap:
					term = "##mainmenu_nomap";
					flag = false;
					break;
				case State.MpHost_SelectMap:
					term = "##mainmenu_select";
					break;
				case State.MpHost_UpdateMap:
					term = "##mainmenu_update";
					break;
				case State.MpHost_NotReady:
					term = "##mainmenu_lobby_notready";
					flag = false;
					break;
				case State.MpHost_WaitingForPlayers:
					term = "##mainmenu_lobby_waitingforplayers";
					flag = false;
					break;
				case State.MpHost_WaitingForDownloadingPlayers:
					term = "##mainmenu_lobby_waitingforplayerdownloads";
					flag = false;
					break;
				case State.MpHost_Start:
					term = "##mainmenu_lobby_start";
					break;
				case State.MpClient_NotReady:
					term = "##mainmenu_lobby_notready";
					flag = false;
					break;
				case State.MpClient_WaitingForHost:
					term = "##mainmenu_lobby_waitingforhost";
					flag = false;
					break;
				case State.MpClient_WaitingForDownload:
					term = "##mainmenu_lobby_waitingfordownload";
					flag = false;
					break;
				}
				panel._label.text = I2Helper.TryGet(term).ToUpper();
				panel._label.color = ((!flag) ? Colors.PlayButtonLabel_Disabled : Colors.PlayButtonLabel_Enabled);
				panel._selectionImage.color = ((!flag) ? Colors.PlayButtonBg_Disabled : Colors.PlayButtonBg_Enabled);
				panel._button.interactable = flag;
				panel._button.colors = ((!flag) ? ColorBlocks.PlayButtonBar_Disabled : ColorBlocks.PlayButtonBar_Enabled);
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					switch (CurrentState)
					{
					case State.Sp_UpdateMap:
						OnBeginUpdate();
						break;
					case State.Sp_StartGame:
						OnOk();
						break;
					case State.Sp_StartWorldEditor:
						OnOk();
						break;
					case State.Sp_StartCreditIsland:
						OnOk();
						break;
					case State.MpHost_SelectMap:
						LobbyMenu.CloseMapLibrary();
						break;
					case State.MpHost_UpdateMap:
						OnBeginUpdate();
						break;
					case State.MpHost_Start:
						GameParam.instance.BeginLoadingScreen();
						break;
					case State.MpHost_NoMap:
					case State.MpHost_NotReady:
					case State.MpHost_WaitingForPlayers:
					case State.MpHost_WaitingForDownloadingPlayers:
						break;
					}
				});
			};
			panelList.Add(LegacyButtonPanelComponent.Build(action5, action5).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyBigPlay));
			return panelList;
		}
	}

	private bool IsMultiplayer => PlayabilityContext == MenuPlayabilityContext.Multi;

	private bool IsNotMultiplayerOrIsHost
	{
		get
		{
			if (!IsMultiplayer)
			{
				return true;
			}
			return LobbyMenu.IsHost;
		}
	}

	private bool IsNewGame
	{
		get
		{
			if (DisplayedMap == null)
			{
				return false;
			}
			if (!DisplayedMap.IsNewGame())
			{
				return false;
			}
			return true;
		}
	}

	private State CurrentState
	{
		get
		{
			if (PlayabilityContext == MenuPlayabilityContext.Multi)
			{
				if (!LobbyMenu.IsHost)
				{
					if (LobbyMenu.IsDownloading)
					{
						return State.MpClient_WaitingForDownload;
					}
					if (LobbyMenu.GetIsReady())
					{
						return State.MpClient_WaitingForHost;
					}
					return State.MpClient_NotReady;
				}
				if (LobbyMenu.IsMapLibrary)
				{
					switch (CurrentDisplayedMapStatus)
					{
					case DisplayedMapStatus.Ready:
						return State.MpHost_SelectMap;
					case DisplayedMapStatus.Outdated:
						return State.MpHost_UpdateMap;
					case DisplayedMapStatus.Undefined:
						return State.MpHost_NoMap;
					}
				}
				else
				{
					switch (CurrentDisplayedMapStatus)
					{
					case DisplayedMapStatus.Ready:
						if (LobbyMenu.IsAnyPlayerDownloading)
						{
							return State.MpHost_WaitingForDownloadingPlayers;
						}
						if (LobbyMenu.GetIsReady())
						{
							if (LobbyMenu.IsAnyClientNotReady)
							{
								return State.MpHost_WaitingForPlayers;
							}
							return State.MpHost_Start;
						}
						return State.MpHost_NotReady;
					case DisplayedMapStatus.Outdated:
						return State.MpHost_UpdateMap;
					case DisplayedMapStatus.Undefined:
						return State.MpHost_NoMap;
					}
				}
			}
			else if (IsWorldEditorMenu)
			{
				switch (CurrentDisplayedMapStatus)
				{
				case DisplayedMapStatus.Ready:
					return State.Sp_StartWorldEditor;
				case DisplayedMapStatus.Outdated:
					return State.Sp_UpdateMap;
				case DisplayedMapStatus.Undefined:
					return State.Sp_SelectMap;
				}
			}
			else if (IsCreditIslandMenu)
			{
				switch (CurrentDisplayedMapStatus)
				{
				case DisplayedMapStatus.Ready:
					return State.Sp_StartCreditIsland;
				case DisplayedMapStatus.Outdated:
					return State.Sp_UpdateMap;
				case DisplayedMapStatus.Undefined:
					return State.Sp_SelectMap;
				}
			}
			else
			{
				switch (CurrentDisplayedMapStatus)
				{
				case DisplayedMapStatus.Ready:
					return State.Sp_StartGame;
				case DisplayedMapStatus.Outdated:
					return State.Sp_UpdateMap;
				case DisplayedMapStatus.Undefined:
					return State.Sp_SelectMap;
				}
			}
			return State.Undefined;
		}
	}

	private DisplayedMapStatus CurrentDisplayedMapStatus
	{
		get
		{
			if (DisplayedMap == null)
			{
				return DisplayedMapStatus.Undefined;
			}
			if (!DisplayedMap.IsCurrentVersion())
			{
				return DisplayedMapStatus.Outdated;
			}
			return DisplayedMapStatus.Ready;
		}
	}

	protected override float HeaderHeight => 20f;

	protected override Action<VerticalLayoutPanelComponent> RootLayout_onReset => delegate(VerticalLayoutPanelComponent panel)
	{
		panel._fitter.enabled = false;
		panel._image.enabled = !IsReskin;
	};

	public static Asset_Gamemode WorldEditorGamemode => AssetDispensary.gamemodes.SendCodenameQuery("world_editor", AssetQueryParameters.AllSources, AssetPlayableContext.Any);

	private static PanelList LoadingWheel
	{
		get
		{
			PanelList panelList = PanelList.Request();
			Action<TexturePanelComponent> onReset = delegate(TexturePanelComponent panel)
			{
				panel._rawImage.gameObject.SetActive(value: false);
				panel._missingImage.gameObject.SetActive(value: false);
				panel._loadingImageGo.gameObject.SetActive(value: true);
				panel._bgImage.enabled = false;
				panel._layoutElement.minWidth = 60f;
				panel._layoutElement.minHeight = 60f;
			};
			TexturePanelComponent.BasePanel item = TexturePanelComponent.Build(onReset);
			panelList.Add(item);
			return panelList;
		}
	}

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
			Asset_Map displayedMap = DisplayedMap;
			if (displayedMap == null)
			{
				return string.Empty;
			}
			switch (GameContext)
			{
			case MenuGameContext.Normal:
				return TypeContext switch
				{
					MenuTypeContext.New => I2Helper.TryGet(displayedMap.displayName), 
					MenuTypeContext.Existing => displayedMap.customName, 
					_ => string.Empty, 
				};
			case MenuGameContext.WorldEditor:
				switch (displayedMap.authorType)
				{
				case AssetAuthorType.Official:
					return I2Helper.TryGet(displayedMap.displayName);
				case AssetAuthorType.UGC:
				case AssetAuthorType.Workshop:
					return displayedMap.displayName;
				default:
					return string.Empty;
				}
			default:
				return string.Empty;
			}
		}
	}

	protected override Texture2D ThumbnailTex => (DisplayedMap == null) ? null : ((!ThumbnailIsHeightmap) ? DisplayedMap.Disk_ReadThumbnail() : DisplayedMap.Disk_ReadHeightmap());

	protected override bool ThumbnailStretches => true;

	protected override bool ThumbnailShowsBg => false;

	private Asset_Map SelectedMap
	{
		get
		{
			return BaseMapLibraryMenu.GetSelectedMap();
		}
		set
		{
			BaseMapLibraryMenu.SetSelectedMap(value, PlayableContext, GameContext, TypeContext);
			if (IsLobby)
			{
				BaseMapLibraryMenu.SetSelectedAsLoaded();
			}
		}
	}

	private Asset_Gamemode SelectedGamemode
	{
		get
		{
			return BaseMapLibraryMenu.GetSelectedGamemode();
		}
		set
		{
			BaseMapLibraryMenu.SetSelectedGamemode(BaseMapLibraryMenu.GetSelectedMap(), value, PlayableContext);
			if (IsLobby)
			{
				BaseMapLibraryMenu.SetSelectedAsLoaded();
			}
		}
	}

	private Asset_GamemodePreset SelectedPreset
	{
		get
		{
			return BaseMapLibraryMenu.GetSelectedPreset();
		}
		set
		{
			BaseMapLibraryMenu.SetSelectedPreset(value);
			if (IsLobby)
			{
				BaseMapLibraryMenu.SetSelectedAsLoaded();
			}
		}
	}

	protected override bool ReloadOnOpen => true;

	protected BaseMapStarterMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	private List<Asset_Gamemode> GetGamemodes(Asset_Map tgtMap)
	{
		List<Asset_Gamemode> list = new List<Asset_Gamemode>();
		if (tgtMap != null)
		{
			if (IsWorldEditorMenu)
			{
				list.Add(WorldEditorGamemode);
			}
			else if (IsExistingAsset)
			{
				Asset_Gamemode lastPlayedGamemode = tgtMap.GetLastPlayedGamemode();
				if (lastPlayedGamemode != null)
				{
					list.Add(lastPlayedGamemode);
				}
			}
			else
			{
				list.AddRange(tgtMap.GetGamemodes(PlayableContext));
			}
		}
		return list;
	}

	private void OnAskDelete()
	{
		CustomMessageModalMenu.OpenWithContentSignal.Invoke(new CustomMessageModalMenu.CustomMessage(I2Helper.TryGet("##mapmenu_delete_confirmation").ToUpper(), I2Helper.TryGet("##mapmenu_delete_confirmation_message"), null, new List<KeyValuePair<string, Action>>
		{
			new KeyValuePair<string, Action>(I2Helper.TryGet("##mapmenu_delete_confirmation_confirm").ToUpper(), delegate
			{
				OnDelete();
				CustomMessageModalMenu.CloseSignal.Invoke();
			}),
			new KeyValuePair<string, Action>(I2Helper.TryGet("##mapmenu_delete_confirmation_cancel").ToUpper(), delegate
			{
				CustomMessageModalMenu.CloseSignal.Invoke();
			})
		}));
	}

	private void OnDelete()
	{
		SelectedMap.Disk_Delete();
		SelectedPreset = null;
		SelectedGamemode = null;
		SelectedMap = null;
		BaseMapLibraryMenu.MapReopenSignal.Invoke();
		Close();
	}

	private void OnOk()
	{
		switch (GameContext)
		{
		case MenuGameContext.Normal:
			switch (PlayabilityContext)
			{
			case MenuPlayabilityContext.Single:
				BaseMapLibraryMenu.SetSelectedAsLoaded();
				GameParam.StartGame(delegate
				{
					BaseMapLibraryMenu.EmptySelected();
				});
				MenuSound.PlaySound(MenuSound.Prefix.Title, MenuSound.Suffix.GameStart);
				break;
			case MenuPlayabilityContext.Multi:
				UNetManager.instance.LocalLobbyPlayer.SendReadyToBeginMessage();
				MenuSound.PlaySound(MenuSound.Prefix.Title, MenuSound.Suffix.GameStart);
				break;
			default:
				throw new NotImplementedException();
			}
			break;
		case MenuGameContext.WorldEditor:
		case MenuGameContext.CreditIsland:
			BaseMapLibraryMenu.SetSelectedAsLoaded();
			GameParam.StartGame(delegate
			{
				BaseMapLibraryMenu.EmptySelected();
			});
			MenuSound.PlaySound(MenuSound.Prefix.Title, MenuSound.Suffix.GameStart);
			break;
		default:
			throw new NotImplementedException();
		}
		Close();
	}

	private void OnCancel()
	{
		SelectedPreset = null;
		SelectedGamemode = null;
		SelectedMap = null;
		Close();
	}

	private void AskUpdate()
	{
		CustomMessageModalMenu.OpenWithFooterSignal.Invoke(new CustomMessageModalMenu.CustomMessage(I2Helper.TryGet("str_confirm_update_title"), I2Helper.TryGet("str_confirm_update_content"), null, new List<KeyValuePair<string, Action>>
		{
			new KeyValuePair<string, Action>(I2Helper.TryGet("str_confirm_update_yes"), delegate
			{
				OnBeginUpdate();
			}),
			new KeyValuePair<string, Action>(I2Helper.TryGet("str_confirm_update_no"), delegate
			{
				CustomMessageModalMenu.CloseSignal.Invoke();
			})
		}));
	}

	private void OnBeginUpdate()
	{
		if (!_updateInProgress)
		{
			_updateInProgress = true;
			MenuSound.PlaySound(MenuSound.Prefix.Title, MenuSound.Suffix.Open);
			CustomMessageModalMenu.CloseSignal.Invoke();
			CustomMessageModalMenu.OpenWithContentSignal.Invoke(new CustomMessageModalMenu.CustomMessage(I2Helper.TryGet("##mainmenu_update_waiting_title").ToUpper(), I2Helper.TryGet("##mainmenu_update_waiting_content"), LoadingWheel, new List<KeyValuePair<string, Action>>
			{
				new KeyValuePair<string, Action>(I2Helper.TryGet("##mainmenu_update_waiting_cancel").ToUpper(), delegate
				{
					OnCancelUpdate();
				})
			}));
			SaveUpdaterRunner.Update(SelectedMap, OnEndUpdate);
		}
	}

	private void OnEndUpdate(Asset_Map updatedMap, bool success)
	{
		if (_updateInProgress)
		{
			_updateInProgress = false;
			SelectedMap = updatedMap;
			IsPostUpdate = true;
			Reopen();
			BaseMapLibraryMenu.MapReopenSignal.Invoke();
			IsPostUpdate = false;
			if (success)
			{
				OnSuccessUpdate(SelectedMap);
			}
			else
			{
				OnFailureUpdate(SelectedMap);
			}
		}
	}

	private void OnSuccessUpdate(Asset_Map updatedMap)
	{
		CustomMessageModalMenu.CloseSignal.Invoke();
		CustomMessageModalMenu.OpenWithFooterSignal.Invoke(new CustomMessageModalMenu.CustomMessage(I2Helper.TryGet("##mainmenu_update_success_title").ToUpper(), I2Helper.TryGet("##mainmenu_update_success_content") + "\n" + AssetDispensary.BackupsPath, null, new List<KeyValuePair<string, Action>>
		{
			new KeyValuePair<string, Action>(I2Helper.TryGet("##mainmenu_update_success_ok").ToUpper(), delegate
			{
				CustomMessageModalMenu.CloseSignal.Invoke();
			})
		}));
	}

	private void OnFailureUpdate(Asset_Map updatedMap)
	{
		CustomMessageModalMenu.CloseSignal.Invoke();
		CustomMessageModalMenu.OpenWithFooterSignal.Invoke(new CustomMessageModalMenu.CustomMessage(I2Helper.TryGet("##mainmenu_update_failure_title").ToUpper(), I2Helper.TryGet("##mainmenu_update_failure_content"), null, new List<KeyValuePair<string, Action>>
		{
			new KeyValuePair<string, Action>(I2Helper.TryGet("##mainmenu_update_failure_ok").ToUpper(), delegate
			{
				CustomMessageModalMenu.CloseSignal.Invoke();
			})
		}));
	}

	private void OnTimeoutUpdate(int updateId)
	{
		if (updateId == _updateId && _updateInProgress)
		{
			_updateInProgress = false;
			CustomMessageModalMenu.CloseSignal.Invoke();
			CustomMessageModalMenu.OpenWithContentSignal.Invoke(new CustomMessageModalMenu.CustomMessage(I2Helper.TryGet("##mainmenu_update_timeout_title").ToUpper(), I2Helper.TryGet("##mainmenu_update_timeout_content"), null, new List<KeyValuePair<string, Action>>
			{
				new KeyValuePair<string, Action>(I2Helper.TryGet("##mainmenu_update_timeout_ok").ToUpper(), delegate
				{
					CustomMessageModalMenu.CloseSignal.Invoke();
				})
			}));
		}
	}

	private void OnCancelUpdate()
	{
		if (_updateInProgress)
		{
			_updateInProgress = false;
			CustomMessageModalMenu.CloseSignal.Invoke();
		}
	}

	protected virtual void OnAfterSetMap()
	{
		Refresh();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, GameParam.OnSetMapSignal, OnSetMap, this).AddSignalConnection(MenuState.Opened, GameParam.OnSetModeSignal, OnSetMode, this)
			.AddSignalConnection(MenuState.Opened, GameParam.OnSetPresetSignal, OnSetPreset, this)
			.AddSignalConnection(MenuState.Loaded, GameParam.OnSetSelectedMapSignal, OnSetMap, this)
			.AddSignalConnection(MenuState.Opened, GameParam.OnSetSelectedModeSignal, OnSetMode, this)
			.AddSignalConnection(MenuState.Opened, GameParam.OnSetSelectedPresetSignal, OnSetPreset, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.OnPlayerDownloadStart, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.OnPlayerDownloadEnd, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.OnAnyToggleReadySignal, OnAnyToggleReady, this);
	}
}
