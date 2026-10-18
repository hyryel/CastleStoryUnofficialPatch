using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyPlayCategoriesMenu : LegacyBaseMenu
{
	public Container TabsContainer;

	public Container ContentContainer;

	public Color DefaultNormalColor;

	public Color DefaultHighlightColor;

	public Color SelectedNormalColor;

	public Color SelectedHighlightColor;

	public static GameSignals.GameSignal OnSelectOperationSignal = new GameSignals.GameSignal();

	public static Operation DefaultSelectedOperation = Operation.SinglePlayer;

	private static Operation _selectedOperation = DefaultSelectedOperation;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, LegacyPlayCategoriesMenu> OnItemClick = delegate(int idx, LegacyPlayCategoriesMenu menu)
	{
		Container.Item item = menu.TabsContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		SelectedOperation = containerPopulatorOperation;
	};

	private static readonly Action<LegacyPlayCategoriesMenu> OnRefreshTabs = delegate(LegacyPlayCategoriesMenu menu)
	{
		for (int i = 0; i < menu.TabsContainer.items.Count; i++)
		{
			Container.Item item = menu.TabsContainer.items[i];
			if (!(item.instance == null) && !(item.instance.button == null))
			{
				bool flag = item.containerPopulatorOperation == SelectedOperation && SelectedOperation != Operation.None;
				ColorBlock colors = item.instance.button.colors;
				colors.normalColor = ((!flag) ? menu.DefaultNormalColor : menu.SelectedNormalColor);
				colors.highlightedColor = ((!flag) ? menu.DefaultHighlightColor : menu.SelectedHighlightColor);
				item.instance.button.colors = colors;
			}
		}
	};

	private static readonly Action<LegacyPlayCategoriesMenu> OnSelectOperation = delegate(LegacyPlayCategoriesMenu menu)
	{
		CreditIslandStarterMenu.CloseSignal.Invoke();
		WorldEditorStarterMenu.CloseSignal.Invoke();
		SpLoadGameStarterMenu.CloseSignal.Invoke();
		SpNewGameStarterMenu.CloseSignal.Invoke();
		OnRefreshTabs(menu);
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
		Connect();
		Init();
	}

	private void OnDisable()
	{
		Disconnect();
	}

	protected override void Init()
	{
		ResetSelectedOperation();
	}

	private void Connect()
	{
		_sgs.Add(GameSignals.Connect(OnSelectOperationSignal, OnSelectOperation, this));
		if (TabsContainer != null)
		{
			_sgs.Add(TabsContainer.OnItemClickSignal.Connect(OnItemClick, this));
		}
		if (TabsContainer != null)
		{
			_sgs.Add(TabsContainer.OnPopulateSignal.Connect(OnRefreshTabs, this));
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
