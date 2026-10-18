using System;
using Brix.Engine;
using Brix.Utils.Factories;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.UI.Builder.Menu.Component;

public class MenuComponent : BaseMenuComponent
{
	public enum MenuType
	{
		WeMenu,
		_unused0,
		NetworkPlayerListMenu,
		NetworkServersMenu,
		_unused342532,
		NetworkCustomServerModalMenu,
		NetworkWaitingModalMenu,
		LobbyMenu,
		NetworkChatMenu,
		MpLoadGameLibraryMenu,
		_unused1,
		NetworkMessageModalMenu,
		NetworkPlayerProfileMenu,
		SpLoadGameStarterMenu,
		MpLoadGameStarterMenu,
		SpLoadGameLibraryMenu,
		SpSaveGameLibraryMenu,
		MpSaveGameLibraryMenu,
		SpNewGameLibraryMenu,
		MpNewGameLibraryMenu,
		WorldEditorLibraryMenu,
		_unused2,
		MapSaverMenu,
		CustomMessageModalMenu,
		NewsMenu,
		MapWorkshopWaitingModalMenu,
		MapWorkshopReleaseNotesModalMenu,
		WorkshopItemStatusMenu,
		SpNewGameStarterMenu,
		LuaMenu,
		WorldEditorStarterMenu,
		MpNewGameStarterMenu,
		ServerInspectorMenu,
		NetworkWaitingQuitModalMenu,
		CreditIslandLibraryMenu,
		CreditIslandStarterMenu,
		LanguageWorkshopWaitingModalMenu,
		LanguageWorkshopReleaseNotesModalMenu
	}

	protected static BrixLogger logger = new BrixLogger(typeof(MenuComponent).FullName);

	private Func<RectTransform> _getParent;

	protected IMenu _menu;

	public MenuType _menuType;

	protected static readonly Action<MenuComponent> OnFactoriesReady = delegate(MenuComponent menuComponent)
	{
		if (!(menuComponent == null))
		{
			menuComponent.FactoriesReady = true;
			menuComponent.TryBegin();
		}
	};

	protected static readonly Action<MenuComponent> OnLevelReady = delegate(MenuComponent menuComponent)
	{
		if (!(menuComponent == null))
		{
			menuComponent.LevelReady = true;
			menuComponent.TryBegin();
		}
	};

	protected static readonly Action<MenuComponent> OnObserverReady = delegate(MenuComponent menuComponent)
	{
		if (!(menuComponent == null))
		{
			menuComponent.ObserverReady = true;
			menuComponent.TryBegin();
		}
	};

	protected static readonly Action<GameSignals.loadingContext, MenuComponent> OnGameReady = delegate(GameSignals.loadingContext lc, MenuComponent menuComponent)
	{
		if (lc == GameSignals.loadingContext.ingame && !(menuComponent == null))
		{
			menuComponent.GameReady = true;
			menuComponent.TryBegin();
		}
	};

	protected static readonly Action<MenuComponent> OnRelease = delegate(MenuComponent menuComponent)
	{
		if (!(menuComponent == null))
		{
			menuComponent.Release();
		}
	};

	protected bool GameReady;

	protected bool LevelReady;

	protected bool ObserverReady;

	protected bool FactoriesReady;

	protected bool GoReady;

	private Action<IMenu> _onOpen;

	private Action<IMenu> _onClose;

	private static readonly NotImplementedException TmpNotImplementedException = new NotImplementedException();

	protected Func<RectTransform> GetParent => () => base.transform as RectTransform;

	public IMenu Menu => _menu;

	protected virtual MenuType Type => _menuType;

	protected virtual bool Ready => FactoriesReady && GoReady;

	public static bool IsPlaying => Application.isPlaying;

	public bool CanOpen => IsPlaying && _menu != null && _menu.CanOpen;

	public bool CanClose => IsPlaying && _menu != null && _menu.CanClose;

	public bool CanToggle => IsPlaying && _menu != null && _menu.CanToggle;

	protected override void Start()
	{
		base.Start();
		ObserverReady = false;
		FactoriesReady = false;
		LevelReady = false;
		GameReady = true;
		FactorySignalInvoker.OnFactoriesReadySig.LateConnectOnce(OnFactoriesReady, this).DisconnectWhen(base.OnReleaseSignal);
		GameSignals.LateConnect(UIGameObserver.IsReady, OnObserverReady, this).DisconnectWhen(base.OnReleaseSignal);
		GameSignals.LateConnectOnce(GameSignals.LevelReady, OnLevelReady, this).DisconnectWhen(base.OnReleaseSignal);
		GameSignals.Connect(GameSignals.LoadingGame, OnGameReady, this).DisconnectWhen(base.OnReleaseSignal);
		base.OnReleaseSignal.ConnectOnce(OnRelease, this);
	}

	protected virtual void OnEnable()
	{
		GoReady = true;
		TryBegin();
	}

	protected virtual void OnDisable()
	{
		End();
		GoReady = false;
	}

	public void TryAutoOpen()
	{
		if (CanOpen && _onOpen != null)
		{
			_onOpen(Menu);
		}
	}

	public void TryAutoClose()
	{
		if (CanClose && _onClose != null)
		{
			_onClose(Menu);
		}
	}

	protected virtual void TryBegin()
	{
		if (Ready)
		{
			Build();
			TryAutoOpen();
		}
	}

	private void End()
	{
		TryAutoClose();
	}

	protected override void OnDestroy()
	{
		Release();
		base.OnDestroy();
	}

	protected virtual void Release()
	{
		if (_menu != null)
		{
			_menu.Close();
			_menu.Unload();
			_menu.Dispose();
			_menu = null;
			_onOpen = null;
			_onClose = null;
		}
	}

	protected void Build()
	{
		if (_menu != null)
		{
			return;
		}
		switch (Type)
		{
		case MenuType.WeMenu:
			break;
		case MenuType.NetworkPlayerListMenu:
			_menu = NetworkPlayersMenu.Build(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.NetworkServersMenu:
			_menu = NetworkServersMenu.Build(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType._unused342532:
			break;
		case MenuType.NetworkCustomServerModalMenu:
			_menu = NetworkCustomServerModalMenu.Build(GetParent);
			break;
		case MenuType.NetworkWaitingModalMenu:
			_menu = NetworkWaitingModalMenu.Build(GetParent, NetworkWaitingModalMenu.PlayerType.Client);
			break;
		case MenuType.LobbyMenu:
			_menu = LobbyMenu.Build(GetParent);
			break;
		case MenuType.NetworkChatMenu:
			_menu = NetworkChatMenu.Build(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.SpNewGameLibraryMenu:
			_menu = SpNewGameLibraryMenu.Create(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.MpNewGameLibraryMenu:
			_menu = MpNewGameLibraryMenu.Build(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.SpLoadGameLibraryMenu:
			_menu = SpLoadGameLibraryMenu.Build(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.MpLoadGameLibraryMenu:
			_menu = MpLoadGameLibraryMenu.Build(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.SpSaveGameLibraryMenu:
			_menu = SpSaveGameLibraryMenu.Build(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.MpSaveGameLibraryMenu:
			_menu = MpSaveGameLibraryMenu.Build(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType._unused1:
			break;
		case MenuType.NetworkMessageModalMenu:
			_menu = NetworkMessageModalMenu.Build(GetParent);
			break;
		case MenuType.NetworkPlayerProfileMenu:
			break;
		case MenuType.SpLoadGameStarterMenu:
			_menu = SpLoadGameStarterMenu.New(GetParent);
			break;
		case MenuType.MpLoadGameStarterMenu:
			_menu = MpLoadGameStarterMenu.New(GetParent);
			break;
		case MenuType.WorldEditorLibraryMenu:
			_menu = WorldEditorLibraryMenu.New(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType._unused2:
			break;
		case MenuType.MapSaverMenu:
			_menu = BaseMapSaverMenu.Build(GetParent);
			break;
		case MenuType.CustomMessageModalMenu:
			_menu = CustomMessageModalMenu.Build(GetParent);
			break;
		case MenuType.NewsMenu:
			_menu = NewsMenu.New(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.MapWorkshopWaitingModalMenu:
			_menu = MapWorkshopWaitingModalMenu.Build(GetParent);
			break;
		case MenuType.MapWorkshopReleaseNotesModalMenu:
			_menu = MapWorkshopReleaseNotesModalMenu.Build(GetParent);
			break;
		case MenuType.WorkshopItemStatusMenu:
			_menu = WorkshopItemStatusMenu.New(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.SpNewGameStarterMenu:
			_menu = SpNewGameStarterMenu.New(GetParent);
			break;
		case MenuType.LuaMenu:
			break;
		case MenuType.WorldEditorStarterMenu:
			_menu = WorldEditorStarterMenu.New(GetParent);
			break;
		case MenuType.MpNewGameStarterMenu:
			_menu = MpNewGameStarterMenu.New(GetParent);
			break;
		case MenuType.ServerInspectorMenu:
			_menu = ServerInspectorMenu.New(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.NetworkWaitingQuitModalMenu:
			_menu = NetworkWaitingQuitModalMenu.Build(GetParent);
			break;
		case MenuType.CreditIslandLibraryMenu:
			_menu = CreditIslandLibraryMenu.New(GetParent);
			_onOpen = delegate(IMenu menu)
			{
				menu.Open();
			};
			_onClose = delegate(IMenu menu)
			{
				menu.Close();
			};
			break;
		case MenuType.CreditIslandStarterMenu:
			_menu = CreditIslandStarterMenu.New(GetParent);
			break;
		case MenuType.LanguageWorkshopWaitingModalMenu:
			_menu = LanguageWorkshopWaitingModalMenu.Build(GetParent);
			break;
		case MenuType.LanguageWorkshopReleaseNotesModalMenu:
			_menu = LanguageWorkshopReleaseNotesModalMenu.Build(GetParent);
			break;
		default:
			throw TmpNotImplementedException;
		}
	}
}
