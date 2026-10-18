using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyPlaySourcesMenu : LegacyBaseMenu
{
	public Container TabsContainer;

	public Color DefaultNormalColor;

	public Color DefaultHighlightColor;

	public Color SelectedNormalColor;

	public Color SelectedHighlightColor;

	public static GameSignals.GameSignal OnSelectOperationSignal = new GameSignals.GameSignal();

	private static readonly List<Operation> IncludedOperations = new List<Operation>
	{
		Operation.Official,
		Operation.Scenarios,
		Operation.CustomMaps,
		Operation.Downloaded,
		Operation.Workshop
	};

	public static Operation DefaultSelectedOperation = Operation.Official;

	private static Operation _selectedOperation = DefaultSelectedOperation;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, LegacyPlaySourcesMenu> OnItemClick = delegate(int idx, LegacyPlaySourcesMenu menu)
	{
		Container.Item item = menu.TabsContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		SelectedOperation = containerPopulatorOperation;
	};

	private static readonly Action<LegacyPlaySourcesMenu> OnSelectOperation = delegate(LegacyPlaySourcesMenu menu)
	{
		CreditIslandStarterMenu.CloseSignal.Invoke();
		WorldEditorStarterMenu.CloseSignal.Invoke();
		SpLoadGameStarterMenu.CloseSignal.Invoke();
		SpNewGameStarterMenu.CloseSignal.Invoke();
		SpNewGameLibraryMenu.ReopenSignal.Invoke();
		menu.RefreshTabs();
	};

	private static readonly Action<LegacyPlaySourcesMenu> OnPopulateTabs = delegate(LegacyPlaySourcesMenu menu)
	{
		menu.RefreshTabs();
	};

	public static AssetAuthorType SelectedAuthorType => SelectedOperation switch
	{
		Operation.Official => AssetAuthorType.Official, 
		Operation.Scenarios => AssetAuthorType.UGC, 
		Operation.CustomMaps => AssetAuthorType.UGC, 
		Operation.Downloaded => AssetAuthorType.UGC, 
		Operation.Workshop => AssetAuthorType.Workshop, 
		_ => AssetAuthorType.Official, 
	};

	public static Operation SelectedOperation
	{
		get
		{
			return _selectedOperation;
		}
		set
		{
			if (IncludedOperations.Contains(value))
			{
				_selectedOperation = value;
				GameSignals.Invoke(OnSelectOperationSignal);
			}
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
			_sgs.Add(TabsContainer.OnPopulateSignal.Connect(OnPopulateTabs, this));
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

	private void RefreshTabs()
	{
		for (int i = 0; i < TabsContainer.items.Count; i++)
		{
			Container.Item item = TabsContainer.items[i];
			if (!(item.instance == null) && !(item.instance.button == null))
			{
				bool flag = item.containerPopulatorOperation == SelectedOperation && SelectedOperation != Operation.None;
				ColorBlock colors = item.instance.button.colors;
				colors.normalColor = ((!flag) ? DefaultNormalColor : SelectedNormalColor);
				colors.highlightedColor = ((!flag) ? DefaultHighlightColor : SelectedHighlightColor);
				item.instance.button.colors = colors;
			}
		}
	}
}
