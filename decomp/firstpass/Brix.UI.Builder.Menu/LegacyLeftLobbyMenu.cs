using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyLeftLobbyMenu : LegacyBaseMenu
{
	public Container LobbyMapLibraryLeftContainer;

	public Color DefaultNormalColor;

	public Color DefaultHighlightColor;

	public Color SelectedNormalColor;

	public Color SelectedHighlightColor;

	public List<Container.Item.ActionItem> ShowPlayerListActions = new List<Container.Item.ActionItem>();

	public List<Container.Item.ActionItem> ShowMapLibraryActions = new List<Container.Item.ActionItem>();

	private List<SignalConnection> _sgs = new List<SignalConnection>();

	public static GameSignals.GameSignal OnSelectOperationSignal = new GameSignals.GameSignal();

	private static readonly List<Operation> RelevantOperations = new List<Operation>
	{
		Operation.MpNewGameLibrary,
		Operation.MpLoadGameLibrary
	};

	public static Operation DefaultSelectedOperation = Operation.MpNewGameLibrary;

	private static Operation _selectedOperation = DefaultSelectedOperation;

	private static readonly Action<int, LegacyLeftLobbyMenu> OnItemClick = delegate(int idx, LegacyLeftLobbyMenu menu)
	{
		Container.Item item = menu.LobbyMapLibraryLeftContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		if (containerPopulatorOperation == Operation.BackToLobby)
		{
			LobbyMenu.CloseMapLibrary();
		}
		else if (RelevantOperations.Contains(containerPopulatorOperation))
		{
			SelectedOperation = containerPopulatorOperation;
		}
	};

	private static readonly Action<LegacyLeftLobbyMenu> OnRefreshTabs = delegate(LegacyLeftLobbyMenu menu)
	{
		for (int i = 0; i < menu.LobbyMapLibraryLeftContainer.items.Count; i++)
		{
			Container.Item item = menu.LobbyMapLibraryLeftContainer.items[i];
			if (!(item.instance == null) && !(item.instance.button == null) && RelevantOperations.Contains(item.containerPopulatorOperation))
			{
				bool flag = item.containerPopulatorOperation == SelectedOperation && SelectedOperation != Operation.None;
				ColorBlock colors = item.instance.button.colors;
				colors.normalColor = ((!flag) ? menu.DefaultNormalColor : menu.SelectedNormalColor);
				colors.highlightedColor = ((!flag) ? menu.DefaultHighlightColor : menu.SelectedHighlightColor);
				item.instance.button.colors = colors;
				if (flag && LobbyMenu.IsMapLibrary)
				{
					Container.RunActions(item.actionItems);
				}
			}
		}
	};

	private static readonly Action<LegacyLeftLobbyMenu> OnToggleMapLibrary = delegate(LegacyLeftLobbyMenu menu)
	{
		if (LobbyMenu.IsHost)
		{
			Container.RunActions((!LobbyMenu.IsMapLibrary) ? menu.ShowPlayerListActions : menu.ShowMapLibraryActions);
		}
	};

	public static Operation SelectedOperation
	{
		get
		{
			return _selectedOperation;
		}
		set
		{
			_selectedOperation = value;
			GameSignals.Invoke(OnSelectOperationSignal);
		}
	}

	private void ResetSelectedOperation()
	{
		SelectedOperation = DefaultSelectedOperation;
	}

	private void OnEnable()
	{
		Begin();
		Init();
	}

	private void OnDisable()
	{
		End();
	}

	protected override void Init()
	{
		ResetSelectedOperation();
	}

	private void Begin()
	{
		End();
		_sgs.Add(LobbyMenu.OnToggleMapLibrarySignal.Connect(OnToggleMapLibrary, this));
		_sgs.Add(GameSignals.Connect(OnSelectOperationSignal, OnRefreshTabs, this));
		_sgs.Add(LobbyMapLibraryLeftContainer.OnPopulateSignal.Connect(OnRefreshTabs, this));
		if (LobbyMapLibraryLeftContainer != null)
		{
			_sgs.Add(LobbyMapLibraryLeftContainer.OnItemClickSignal.Connect(OnItemClick, this));
		}
		_sgs.Add(GameSignals.Connect(GameParam.OnSetPresetSignal, OnRefreshTabs, this));
	}

	private void End()
	{
		foreach (SignalConnection sg in _sgs)
		{
			sg.Disconnect();
		}
		_sgs.Clear();
	}
}
