using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyOptionCategoriesMenu : LegacyBaseMenu
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
		Operation.OptionsGeneral,
		Operation.OptionsGameplay,
		Operation.OptionsAudio,
		Operation.OptionsGraphics,
		Operation.OptionsKeybindings
	};

	public Operation DefaultOperation = Operation.OptionsGeneral;

	private static Operation _selectedOperation = Operation.OptionsGeneral;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, LegacyOptionCategoriesMenu> OnItemClick = delegate(int idx, LegacyOptionCategoriesMenu menu)
	{
		Container.Item item = menu.TabsContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		SelectedOperation = containerPopulatorOperation;
	};

	private static readonly Action<LegacyOptionCategoriesMenu> OnRefreshTabs = delegate(LegacyOptionCategoriesMenu menu)
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
		_sgs.Add(GameSignals.Connect(UserSettingManager.TempSettings.OnTempSettingsSetDirty, OnRefreshTabs, this));
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
