using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyKeybindingsCategoriesMenu : LegacyBaseMenu
{
	public Container TabsContainer;

	public Color DefaultNormalColor;

	public Color DefaultHighlightColor;

	public Color SelectedNormalColor;

	public Color SelectedHighlightColor;

	public static GameSignals.GameSignal OnSelectOperationSignal = new GameSignals.GameSignal();

	public Container.Item.Group ContentGroup = Container.Item.Group.Alpha;

	private static readonly List<Operation> IncludedOperations = new List<Operation>
	{
		Operation.Keybindings_Global,
		Operation.Keybindings_TaskPanel,
		Operation.Keybindings_BlocksPanel,
		Operation.Keybindings_StructuresPanel,
		Operation.Keybindings_Tasks,
		Operation.Keybindings_Crafting,
		Operation.Keybindings_Blueprint,
		Operation.Keybindings_WorldEditor,
		Operation.Keybindings_WorldEditor_Terrain,
		Operation.Keybindings_WorldEditor_Markers
	};

	public Operation DefaultOperation = Operation.Keybindings_Global;

	private static Operation _selectedOperation = Operation.Keybindings_Global;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, LegacyKeybindingsCategoriesMenu> OnItemClick = delegate(int idx, LegacyKeybindingsCategoriesMenu menu)
	{
		Container.Item item = menu.TabsContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		SelectedOperation = containerPopulatorOperation;
	};

	private static readonly Action<LegacyKeybindingsCategoriesMenu> OnRefreshTabs = delegate(LegacyKeybindingsCategoriesMenu menu)
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
		SelectedOperation = DefaultOperation;
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
		_sgs.Add(GameSignals.Connect(OnSelectOperationSignal, OnRefreshTabs, this));
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
