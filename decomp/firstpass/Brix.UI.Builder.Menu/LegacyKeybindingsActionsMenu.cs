using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyKeybindingsActionsMenu : LegacyBaseMenu
{
	public Container Container;

	public Color DefaultColor;

	public Color DisabledColor;

	public List<Container.Item.ActionItem> TitleBackActions = new List<Container.Item.ActionItem>();

	public List<Container.Item.ActionItem> InGameBackActions = new List<Container.Item.ActionItem>();

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, LegacyKeybindingsActionsMenu> OnItemClick = delegate(int idx, LegacyKeybindingsActionsMenu menu)
	{
		Container.Item item = menu.Container.items[idx];
		switch (item.containerPopulatorOperation)
		{
		case Operation.Apply:
			UserSettingManager.TempSettings.Apply();
			break;
		case Operation.Revert:
			menu.OnRevert();
			break;
		case Operation.OptionsBackTitle:
			if (UserSettingManager.TempSettings.IsDirty)
			{
				menu.OnBack(menu.TitleBackActions);
			}
			else
			{
				Container.RunActions(menu.TitleBackActions);
			}
			break;
		case Operation.OptionsBackInGame:
			if (UserSettingManager.TempSettings.IsDirty)
			{
				menu.OnBack(menu.InGameBackActions);
			}
			else
			{
				Container.RunActions(menu.InGameBackActions);
			}
			break;
		}
	};

	private static readonly Action<LegacyKeybindingsActionsMenu> OnRefresh = delegate(LegacyKeybindingsActionsMenu menu)
	{
		for (int i = 0; i < menu.Container.items.Count; i++)
		{
			Container.Item item = menu.Container.items[i];
			Operation containerPopulatorOperation = item.containerPopulatorOperation;
			if (containerPopulatorOperation == Operation.Apply && item.instance != null)
			{
				item.instance.title.color = ((!UserSettingManager.TempSettings.IsDirty) ? menu.DisabledColor : menu.DefaultColor);
				item.instance.button.interactable = UserSettingManager.TempSettings.IsDirty;
			}
			if (item.instance != null)
			{
				if (item.instance.title != null)
				{
					item.instance.title.text = I2Helper.TryGet(item.titleText).ToUpper();
				}
				if (item.instance.subtitle != null)
				{
					item.instance.subtitle.text = I2Helper.TryGet(item.subtitleText);
				}
			}
		}
	};

	private void OnEnable()
	{
		Connect();
		Init();
	}

	private void OnDisable()
	{
		Disconnect();
	}

	protected override void Init()
	{
		UserSettingManager.TempSettings.Cancel();
		OnRefresh(this);
	}

	public void Connect()
	{
		if (Container != null)
		{
			_sgs.Add(Container.OnItemClickSignal.Connect(OnItemClick, this));
		}
		_sgs.Add(GameSignals.Connect(UserSettingManager.TempSettings.OnTempSettingsSetDirty, OnRefresh, this));
	}

	private void Disconnect()
	{
		foreach (SignalConnection sg in _sgs)
		{
			sg.Disconnect();
		}
		_sgs.Clear();
	}

	private void OnRevert()
	{
		GameSignals.Invoke(GameSignals.OnDisplayGenericDialog, new OkDialogContainerPopulator.GenericDialog(I2Helper.TryGet("##mainmenu_options_revert").ToUpper(), I2Helper.TryGet("##mainmenu_options_revert_message"), new OkDialogContainerPopulator.GenericDialog.Btn[2]
		{
			new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##mainmenu_options_revert_message_ok").ToUpper(), delegate
			{
				UserSettingManager.TempSettings.Revert();
				GameSignals.Invoke(GameSignals.OnCloseDialog);
			}, string.Empty),
			new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##mainmenu_options_revert_message_cancel").ToUpper(), delegate
			{
				GameSignals.Invoke(GameSignals.OnCloseDialog);
			}, string.Empty)
		}));
	}

	private void OnBack(List<Container.Item.ActionItem> actions)
	{
		GameSignals.Invoke(GameSignals.OnDisplayGenericDialog, new OkDialogContainerPopulator.GenericDialog(I2Helper.TryGet("##mainmenu_options_unsaved").ToUpper(), I2Helper.TryGet("##mainmenu_options_unsaved_message"), new OkDialogContainerPopulator.GenericDialog.Btn[3]
		{
			new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##mainmenu_options_unsaved_apply").ToUpper(), delegate
			{
				UserSettingManager.TempSettings.Apply();
				Container.RunActions(actions);
				GameSignals.Invoke(GameSignals.OnCloseDialog);
			}, string.Empty),
			new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##mainmenu_options_unsaved_discard").ToUpper(), delegate
			{
				UserSettingManager.TempSettings.Cancel();
				Container.RunActions(actions);
				GameSignals.Invoke(GameSignals.OnCloseDialog);
			}, string.Empty),
			new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##mainmenu_options_unsaved_cancel").ToUpper(), delegate
			{
				GameSignals.Invoke(GameSignals.OnCloseDialog);
			}, string.Empty)
		}));
	}
}
