using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public class LegacyPlayGamemodeMenu : LegacyBaseMenu
{
	public Container TabsContainer;

	public Container SelectionContainer;

	public Container ContentContainer;

	public Texture2D ConquestTexture;

	public Texture2D InvasionTexture;

	public Texture2D SandboxTexture;

	public static GameSignals.GameSignal OnSelectOperationSignal = new GameSignals.GameSignal();

	private static readonly Asset_Gamemode ConquestGamemode = AssetDispensary.gamemodes.SendCodenameQuery("conquest");

	private static readonly Asset_Gamemode InvasionGamemode = AssetDispensary.gamemodes.SendCodenameQuery("invasion");

	private static readonly Asset_Gamemode SandboxGamemode = AssetDispensary.gamemodes.SendCodenameQuery("sandbox");

	private static readonly List<Operation> IncludedOperations = new List<Operation>
	{
		Operation.Conquest,
		Operation.Invasion,
		Operation.Sandbox
	};

	public static Operation DefaultSelectedOperation = Operation.Conquest;

	private static Operation _selectedOperation = DefaultSelectedOperation;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, LegacyPlayGamemodeMenu> OnItemClick = delegate(int idx, LegacyPlayGamemodeMenu menu)
	{
		Container.Item item = menu.SelectionContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		SelectedOperation = containerPopulatorOperation;
	};

	private static readonly Action<LegacyPlayGamemodeMenu> OnSelectOperation = delegate(LegacyPlayGamemodeMenu menu)
	{
		menu.RefreshTabs();
		menu.RefreshContent();
	};

	private static readonly Action<LegacyPlayGamemodeMenu> OnPopulateTabs = delegate(LegacyPlayGamemodeMenu menu)
	{
		menu.RefreshTabs();
		menu.RefreshContent();
	};

	private static readonly Action<LegacyPlayGamemodeMenu> OnPopulateSelection = delegate(LegacyPlayGamemodeMenu menu)
	{
		menu.RefreshSelection();
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

	public static Asset_Gamemode SelectedOperationGamemode => GetOperationGamemode(SelectedOperation);

	public static string GetGamemodeCodename(Asset_Gamemode mode)
	{
		return (mode != null) ? mode.codename : string.Empty;
	}

	public static string GetGamemodeDisplayname(Asset_Gamemode mode)
	{
		return (mode != null) ? mode.displayName : string.Empty;
	}

	public static string GetGamemodeDescription(Asset_Gamemode mode)
	{
		return (mode != null) ? mode.description : string.Empty;
	}

	private Texture2D GetGamemodeTexture(int idx)
	{
		return idx switch
		{
			0 => SandboxTexture, 
			1 => InvasionTexture, 
			2 => ConquestTexture, 
			_ => null, 
		};
	}

	private static Asset_Gamemode GetOperationGamemode(Operation operation)
	{
		return operation switch
		{
			Operation.Conquest => ConquestGamemode, 
			Operation.Invasion => InvasionGamemode, 
			Operation.Sandbox => SandboxGamemode, 
			_ => null, 
		};
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
		RefreshSelection();
	}

	private void Connect()
	{
		if (SelectionContainer != null)
		{
			_sgs.Add(SelectionContainer.OnItemClickSignal.Connect(OnItemClick, this));
		}
		_sgs.Add(GameSignals.Connect(OnSelectOperationSignal, OnSelectOperation, this));
		if (TabsContainer != null)
		{
			_sgs.Add(TabsContainer.OnPopulateSignal.Connect(OnPopulateTabs, this));
		}
		if (SelectionContainer != null)
		{
			_sgs.Add(SelectionContainer.OnPopulateSignal.Connect(OnPopulateSelection, this));
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
		Asset_Gamemode operationGamemode = GetOperationGamemode(SelectedOperation);
		for (int i = 0; i < TabsContainer.items.Count; i++)
		{
			Container.Item item = TabsContainer.items[i];
			if (item.containerPopulatorOperation != Operation.Title)
			{
				continue;
			}
			item.titleText = GetGamemodeDisplayname(operationGamemode).ToUpper();
			item.subtitleText = GetGamemodeDescription(operationGamemode);
			item.subsubtitleText = I2Helper.TryGet("##mainmenu_select").ToUpper();
			if (!(item.instance == null))
			{
				if (item.instance.title != null)
				{
					item.instance.title.text = item.titleText;
				}
				if (item.instance.subtitle != null)
				{
					item.instance.subtitle.text = item.subtitleText;
				}
				if (item.instance.subsubtitle != null)
				{
					item.instance.subsubtitle.text = item.subsubtitleText;
				}
			}
		}
	}

	private void RefreshContent()
	{
		Asset_Gamemode operationGamemode = GetOperationGamemode(SelectedOperation);
		for (int i = 0; i < ContentContainer.items.Count; i++)
		{
			Container.Item item = ContentContainer.items[i];
			if (item.containerPopulatorOperation == Operation.Title)
			{
				item.titleText = I2Helper.TryGet("##mainmenu_selectmap");
				if (item.instance == null)
				{
					continue;
				}
				if (item.instance.title != null)
				{
					item.instance.title.text = item.titleText.ToUpper();
				}
			}
			if (item.containerPopulatorOperation == Operation.BreadcrumbTitle)
			{
				item.titleText = I2Helper.TryGet("##mainmenu_play") + "  ➜  " + GetGamemodeDisplayname(operationGamemode).ToUpper() + "  ➜  " + I2Helper.TryGet("##mainmenu_selectmap");
				if (!(item.instance == null) && item.instance.title != null)
				{
					item.instance.title.text = item.titleText.ToUpper();
				}
			}
		}
	}

	private void RefreshSelection()
	{
		for (int i = 0; i < SelectionContainer.items.Count; i++)
		{
			Container.Item item = SelectionContainer.items[i];
			if (!IncludedOperations.Contains(item.containerPopulatorOperation))
			{
				break;
			}
			Texture2D texture = (item.imageTexture = GetGamemodeTexture(i));
			item.titleText = GetGamemodeDisplayname(GetOperationGamemode(item.containerPopulatorOperation)).ToUpper();
			item.subtitleText = GetGamemodeDescription(GetOperationGamemode(item.containerPopulatorOperation));
			item.subsubtitleText = I2Helper.TryGet("##mainmenu_select").ToUpper();
			if (!(item.instance == null))
			{
				if (item.instance.image != null)
				{
					item.instance.image.texture = texture;
				}
				if (item.instance.title != null)
				{
					item.instance.title.text = item.titleText;
				}
				if (item.instance.subtitle != null)
				{
					item.instance.subtitle.text = item.subtitleText;
				}
				if (item.instance.subsubtitle != null)
				{
					item.instance.subsubtitle.text = item.subsubtitleText;
				}
			}
		}
	}
}
