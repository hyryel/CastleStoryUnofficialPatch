using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Network;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyMultiplayerMenu : LegacyBaseMenu
{
	private static BrixLogger logger = new BrixLogger(typeof(LegacyMultiplayerMenu).FullName);

	public Container panelsContainer;

	public Container hostModalContainer;

	public Container clientModalContainer;

	public List<Container.Item.ActionItem> OnHostSuccessActions = new List<Container.Item.ActionItem>();

	public List<Container.Item.ActionItem> OnJoinSuccessActions = new List<Container.Item.ActionItem>();

	public List<Container.Item.ActionItem> OnHostFailureActions = new List<Container.Item.ActionItem>();

	public List<Container.Item.ActionItem> OnJoinFailureActions = new List<Container.Item.ActionItem>();

	private static readonly Action<LegacyMultiplayerMenu> OnHostSuccess = delegate(LegacyMultiplayerMenu menu)
	{
		Container.RunActions(menu.OnHostSuccessActions);
	};

	private static readonly Action<LegacyMultiplayerMenu> OnJoinSuccess = delegate(LegacyMultiplayerMenu menu)
	{
		Container.RunActions(menu.OnJoinSuccessActions);
	};

	private static readonly Action<LegacyMultiplayerMenu> OnHostFailure = delegate(LegacyMultiplayerMenu menu)
	{
		Container.RunActions(menu.OnHostFailureActions);
	};

	private static readonly Action<LegacyMultiplayerMenu> OnJoinFailure = delegate(LegacyMultiplayerMenu menu)
	{
		Container.RunActions(menu.OnJoinFailureActions);
	};

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, LegacyMultiplayerMenu> OnItemClick_Panels = delegate(int idx, LegacyMultiplayerMenu menu)
	{
		Container.Item item = menu.panelsContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		if (containerPopulatorOperation == Operation.Reset)
		{
			menu.ManualAbort(string.Empty);
		}
	};

	private static readonly Action<int, LegacyMultiplayerMenu> OnItemClick_HostModal = delegate(int idx, LegacyMultiplayerMenu menu)
	{
		Container.Item item = menu.panelsContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		if (containerPopulatorOperation == Operation.Apply)
		{
			UNetManager.instance.LocalPlayerName = Neo.multiplayerName;
			NetworkWaitingModalMenu.OpenAsHostSignal.Invoke();
		}
	};

	private static readonly Action<int, LegacyMultiplayerMenu> OnItemClick_ClientModal = delegate(int idx, LegacyMultiplayerMenu menu)
	{
		Container.Item item = menu.panelsContainer.items[idx];
		switch (item.containerPopulatorOperation)
		{
		case Operation.Apply:
			UNetManager.instance.LocalPlayerName = Neo.multiplayerName;
			if (UNetManager.instance.networkDiscovery.running)
			{
				UNetManager.instance.networkDiscovery.StopBroadcast();
			}
			break;
		case Operation.FindLanServer:
			menu.GetLanServers();
			break;
		default:
			menu.ManualAbort(string.Empty);
			break;
		}
	};

	private void ManualAbort(string message = "")
	{
		logger.Info("Disconnecting.");
		UNetManager.instance.ResetPersistantNetworkSceneObjects();
		DisconnectFromServer();
		DisplayErrorDialog(I2Helper.TryGet(message));
	}

	public void DisplayErrorDialog(string message)
	{
		if (message != string.Empty)
		{
			GameSignals.Invoke(GameSignals.OnDisplayDialog, I2Helper.TryGet("##loading_title_error"), message, new string[1] { I2Helper.TryGet("##loading_ok") }, new Action[1]
			{
				delegate
				{
					GameSignals.Invoke(GameSignals.OnCloseDialog);
				}
			});
		}
	}

	private void DisconnectFromServer()
	{
		UNetManager.instance.Disconnect();
	}

	private void OnEnable()
	{
		Connect();
	}

	private void OnDisable()
	{
		Disconnect();
	}

	private void Connect()
	{
		if (panelsContainer != null)
		{
			_sgs.Add(panelsContainer.OnItemClickSignal.Connect(OnItemClick_Panels, this));
		}
		if (hostModalContainer != null)
		{
			_sgs.Add(hostModalContainer.OnItemClickSignal.Connect(OnItemClick_HostModal, this));
		}
		if (clientModalContainer != null)
		{
			_sgs.Add(clientModalContainer.OnItemClickSignal.Connect(OnItemClick_ClientModal, this));
		}
		_sgs.Add(NetworkWaitingModalMenu.OnHostSuccessSignal.Connect(OnHostSuccess, this));
		_sgs.Add(NetworkWaitingModalMenu.OnJoinSuccessSignal.Connect(OnJoinSuccess, this));
		_sgs.Add(NetworkWaitingModalMenu.OnHostFailureSignal.Connect(OnHostFailure, this));
		_sgs.Add(NetworkWaitingModalMenu.OnJoinFailureSignal.Connect(OnJoinFailure, this));
	}

	private void Disconnect()
	{
		foreach (SignalConnection sg in _sgs)
		{
			sg.Disconnect();
		}
		_sgs.Clear();
	}

	private void GetLanServers()
	{
		UNetManager.instance.networkDiscovery.FindLanGames();
	}
}
