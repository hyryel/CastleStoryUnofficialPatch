using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Lua;
using Brix.NewUI.UserSettings_;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

[RequireComponent(typeof(Container))]
public class ContainerGenericPopulator : LegacyBaseMenu
{
	public Container container;

	public Texture2D twitchTextureIcon;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, ContainerGenericPopulator> OnItemClick = delegate(int idx, ContainerGenericPopulator menu)
	{
		Container.Item item = menu.container.items[idx];
		switch (item.containerPopulatorOperation)
		{
		case Operation.Help:
			GameSignals.Invoke(GameSignals.OnMenuToggleRequest);
			LuaHookBase.InvokeFromCLR(UIGameObserver.lua_ShowHelp);
			break;
		case Operation.ReturnToGame:
			GameSignals.Invoke(GameSignals.OnMenuToggleRequest);
			break;
		case Operation.QuitGame:
			GameSignals.Invoke(GameSignals.OnDisplayGenericDialog, new OkDialogContainerPopulator.GenericDialog(I2Helper.TryGet("##gamemenu_quit_game").ToUpper(), I2Helper.TryGet("##gamemenu_quit_message"), new OkDialogContainerPopulator.GenericDialog.Btn[3]
			{
				new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##gamemenu_quit_exitdesktop").ToUpper(), delegate
				{
					GameSignals.Invoke(GameSignals.QuitThroughMenu);
					ApplicationQuitter.Quit();
				}, string.Empty),
				new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##gamemenu_quit_leavegame").ToUpper(), delegate
				{
					GameSignals.Invoke(GameSignals.OnCloseDialog);
					Architecte.QuitToMainMenu();
				}, string.Empty),
				new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##gamemenu_cancel").ToUpper(), delegate
				{
					GameSignals.Invoke(GameSignals.OnCloseDialog);
				}, string.Empty)
			}));
			break;
		case Operation.QuitApplication:
			GameSignals.Invoke(GameSignals.OnDisplayGenericDialog, new OkDialogContainerPopulator.GenericDialog(I2Helper.TryGet("##mainmenu_exit").ToUpper(), I2Helper.TryGet("##mainmenu_exit_message"), new OkDialogContainerPopulator.GenericDialog.Btn[2]
			{
				new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##mainmenu_exit_confirmexit").ToUpper(), delegate
				{
					GameSignals.Invoke(GameSignals.QuitThroughMenu);
					ApplicationQuitter.Quit();
				}, string.Empty),
				new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##mainmenu_exit_cancelexit").ToUpper(), delegate
				{
					GameSignals.Invoke(GameSignals.OnCloseDialog);
				}, string.Empty)
			}));
			break;
		case Operation.ChangeGameMode:
		{
			List<Asset_Gamemode> modeNames = GameParam.Map.GetGamemodes(AssetPlayableContext.Singleplayer);
			int srcModeIndex = modeNames.IndexOf(GameParam.Gamemode);
			GameSignals.Invoke(GameSignals.OnDisplayGenericDialog, new OkDialogContainerPopulator.RadioGenericDialog("Game Mode", "Select a game mode below.", new OkDialogContainerPopulator.GenericDialog.Btn[2]
			{
				new OkDialogContainerPopulator.GenericDialog.Btn("OK", delegate
				{
					if (srcModeIndex >= 0 && srcModeIndex < modeNames.Count)
					{
						GameParam.Gamemode = modeNames[srcModeIndex];
					}
					else
					{
						GameParam.Gamemode = GameParam.Map.GetGamemodes().Slinq().FirstOrDefault();
					}
					GameSignals.Invoke(GameSignals.OnCloseDialog);
				}, string.Empty),
				new OkDialogContainerPopulator.GenericDialog.Btn("Cancel", delegate
				{
					GameSignals.Invoke(GameSignals.OnCloseDialog);
				}, string.Empty)
			}, (from i in modeNames.Slinq()
				select i.PrettyDisplayName(supported: true)).ToArray(), idx, delegate(int dstModeIndex)
			{
				srcModeIndex = dstModeIndex;
				GameSignals.Invoke(GameSignals.OnDisplayDialogRadioSelect, srcModeIndex);
			}));
			GameSignals.Invoke(GameSignals.OnDisplayDialogRadioSelect, srcModeIndex);
			break;
		}
		case Operation.ChatOptions:
		{
			UserSettings settings = UserSettingsReader.Read();
			bool dstTwitchFlag;
			bool defaultFlag = (dstTwitchFlag = settings.chat != null && settings.chat.twitch.autoConnect);
			string dstTwitchChanName;
			string defaultChanName = (dstTwitchChanName = ((settings.chat != null && settings.chat.twitch.chanName != null) ? settings.chat.twitch.chanName : string.Empty));
			string dstTwitchUserName;
			string defaultUserName = (dstTwitchUserName = ((settings.chat != null && settings.chat.twitch.userName != null) ? settings.chat.twitch.userName : string.Empty));
			string dstTwitchPassName;
			string defaultPassName = (dstTwitchPassName = ((settings.chat != null && settings.chat.twitch.passName != null) ? settings.chat.twitch.passName : string.Empty));
			GameSignals.Invoke(GameSignals.OnDisplayGenericDialog, new OkDialogContainerPopulator.LoginGenericDialog("Twitch Chat Options", "Enter your Twitch credentials below.\n<color=red>These will be saved as plain text in your settings file.</color>", new OkDialogContainerPopulator.GenericDialog.Btn[2]
			{
				new OkDialogContainerPopulator.GenericDialog.Btn("OK", delegate
				{
					settings.chat.twitch.autoConnect = dstTwitchFlag;
					settings.chat.twitch.chanName = dstTwitchChanName;
					settings.chat.twitch.userName = dstTwitchUserName;
					settings.chat.twitch.passName = dstTwitchPassName;
					UserSettingsReader.Save(settings);
					GameSignals.Invoke(GameSignals.OnCloseDialog);
				}, string.Empty),
				new OkDialogContainerPopulator.GenericDialog.Btn("Cancel", delegate
				{
					GameSignals.Invoke(GameSignals.OnCloseDialog);
				}, string.Empty)
			}, menu.twitchTextureIcon, defaultFlag, defaultChanName, defaultUserName, defaultPassName, delegate(bool argTwitchFlag)
			{
				dstTwitchFlag = argTwitchFlag;
			}, delegate(string argTwitchChanName)
			{
				dstTwitchChanName = argTwitchChanName;
			}, delegate(string argTwitchUserName)
			{
				dstTwitchUserName = argTwitchUserName;
			}, delegate(string argTwitchPassName)
			{
				dstTwitchPassName = argTwitchPassName;
			}));
			break;
		}
		case Operation.NetworkServersModalMenuOpen:
			NetworkCustomServerModalMenu.CloseSignal.Invoke();
			break;
		case Operation.NetworkCustomServerModalMenuOpen:
			NetworkCustomServerModalMenu.OpenSignal.Invoke();
			break;
		case Operation.NetworkCustomServerModalMenuClose:
			NetworkCustomServerModalMenu.CloseSignal.Invoke();
			break;
		}
	};

	[ContextMenu("Init")]
	protected override void Init()
	{
		container = container ?? base.gameObject.GetComponent<Container>();
	}

	private void OnEnable()
	{
		Connect();
		Init();
	}

	private void OnDisable()
	{
		Disconnect();
	}

	private void Connect()
	{
		if (container != null)
		{
			_sgs.Add(container.OnItemClickSignal.Connect(OnItemClick, this));
		}
	}

	private void Disconnect()
	{
		foreach (SignalConnection sg in _sgs)
		{
			sg.Disconnect();
		}
		_sgs.Clear();
	}
}
