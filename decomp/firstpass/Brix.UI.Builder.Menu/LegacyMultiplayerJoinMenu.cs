using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyMultiplayerJoinMenu : LegacyBaseMenu
{
	public Container TabsContainer;

	public Text Title;

	public Text Bread;

	public Color DefaultNormalColor;

	public Color DefaultHighlightColor;

	public Color SelectedNormalColor;

	public Color SelectedHighlightColor;

	public static GameSignals.GameSignal OnSelectOperationSignal = new GameSignals.GameSignal();

	public Container.Item.Group ContentGroup = Container.Item.Group.Alpha;

	private static readonly List<Operation> IncludedOperations = new List<Operation>
	{
		Operation.PublicServers,
		Operation.LocalServers
	};

	public Operation DefaultOperation = Operation.LocalServers;

	private static Operation _selectedOperation = Operation.LocalServers;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, LegacyMultiplayerJoinMenu> OnItemClick = delegate(int idx, LegacyMultiplayerJoinMenu menu)
	{
		Container.Item item = menu.TabsContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		SelectedOperation = containerPopulatorOperation;
	};

	private static readonly Action<LegacyMultiplayerJoinMenu> OnSelectOperation = delegate(LegacyMultiplayerJoinMenu menu)
	{
		menu.RefreshTabs();
		menu.RefreshTitle();
		menu.RefreshBread();
	};

	private static readonly Action<LegacyMultiplayerJoinMenu> OnPopulateTabs = delegate(LegacyMultiplayerJoinMenu menu)
	{
		menu.RefreshTabs();
	};

	private static Coroutine _coroutineWaitOneSecond;

	private static bool _waitingOneSecond;

	private static WaitForSecondsRealtime _tmpWaitForOneSecond;

	public static GameSignals.GameSignal OnRefreshTimerSignal = new GameSignals.GameSignal();

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
		StartTimer();
	}

	private void OnDisable()
	{
		Disconnect();
		StopTimer();
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

	private void RefreshTitle()
	{
		if (!(Title == null))
		{
			switch (SelectedOperation)
			{
			case Operation.LocalServers:
				Title.text = I2Helper.TryGet("##mainmenu_multiplayer_localservers").ToUpper();
				break;
			case Operation.PublicServers:
				Title.text = I2Helper.TryGet("##mainmenu_multiplayer_publicservers").ToUpper();
				break;
			}
		}
	}

	private void RefreshBread()
	{
		if (!(Bread == null))
		{
			switch (SelectedOperation)
			{
			case Operation.LocalServers:
				Bread.text = string.Empty;
				Bread.text = string.Format("{0}{1}", Bread.text, I2Helper.TryGet("##mainmenu_play").ToUpper());
				Bread.text = string.Format("{0}{1}", Bread.text, "  ➜  ");
				Bread.text = string.Format("{0}{1}", Bread.text, I2Helper.TryGet("##mainmenu_multiplayer").ToUpper());
				Bread.text = string.Format("{0}{1}", Bread.text, "  ➜  ");
				Bread.text = string.Format("{0}{1}", Bread.text, I2Helper.TryGet("##mainmenu_multiplayer_localservers").ToUpper());
				break;
			case Operation.PublicServers:
				Bread.text = string.Empty;
				Bread.text = string.Format("{0}{1}", Bread.text, I2Helper.TryGet("##mainmenu_play").ToUpper());
				Bread.text = string.Format("{0}{1}", Bread.text, "  ➜  ");
				Bread.text = string.Format("{0}{1}", Bread.text, I2Helper.TryGet("##mainmenu_multiplayer").ToUpper());
				Bread.text = string.Format("{0}{1}", Bread.text, "  ➜  ");
				Bread.text = string.Format("{0}{1}", Bread.text, I2Helper.TryGet("##mainmenu_multiplayer_publicservers").ToUpper());
				break;
			}
		}
	}

	private static void StartTimer()
	{
		_coroutineWaitOneSecond = _coroutineWaitOneSecond ?? PersistentCoroutineRunner.Instance.StartCoroutine(WaitOneSecond());
		_waitingOneSecond = true;
	}

	private static void StopTimer()
	{
		_waitingOneSecond = false;
	}

	private static IEnumerator WaitOneSecond()
	{
		while (_waitingOneSecond)
		{
			GameSignals.Invoke(OnRefreshTimerSignal);
			if (_tmpWaitForOneSecond == null)
			{
				_tmpWaitForOneSecond = new WaitForSecondsRealtime(1f);
			}
			yield return _tmpWaitForOneSecond;
		}
	}
}
