using System.Collections.Generic;
using Brix.External.Signals;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public class LegacyBaseMenu : MonoBehaviour, ISignalConnector<LegacyBaseMenu.Life>
{
	protected enum Life
	{
		Enabled
	}

	public enum Operation
	{
		None,
		Apply,
		Reset,
		ToggleFullscreen,
		ShowWelcomeScreenAtGameStart,
		PauseGameWhenWindowLosesFocus,
		EnableSpecialAssets,
		MouseScrollFactor,
		InvertMouseScroll,
		MuteAllSounds,
		UserInterface,
		SoundEffects,
		Environment,
		Music,
		ServerIP,
		ServerPort,
		ServerName,
		TaskbarGroupEmpty,
		NextDebugPath,
		ResetKeysToDefault,
		ToggleScreenLock,
		ToggleScreenNudge,
		ShowUnsupportedGameModes,
		VolumeVoices,
		ReturnToGame,
		SaveGame,
		QuitGame,
		QuitApplication,
		ChangeLanguageInGame,
		ChangeGameMode,
		RootMenu,
		ChatOptions,
		FindLanServer,
		NetworkServersModalMenuOpen,
		NetworkServersModalMenuClose,
		NetworkCustomServerModalMenuOpen,
		NetworkCustomServerModalMenuClose,
		ChangeLanguageTitleScreen,
		FogOpacity,
		RestartGame,
		Cancel,
		Revert,
		TerrainAcceleration,
		OptionsGeneral,
		OptionsGraphics,
		Resolution,
		FramerateLimit,
		Lod,
		OptionsAudio,
		OptionsGameplay,
		OptionsKeybindings,
		MasterVolume,
		UserInterfaceVolume,
		SoundEffectsVolume,
		VoicesVolume,
		EnvironmentVolume,
		MusicVolume,
		LoadGame,
		SinglePlayer,
		Multiplayer,
		Tutorials,
		WorldEditor,
		Conquest,
		Invasion,
		Sandbox,
		Title,
		Official,
		Scenarios,
		CustomMaps,
		Downloaded,
		BreadcrumbTitle,
		MultiplayerJoin,
		MultiplayerHost,
		PublicServers,
		LocalServers,
		SetGameName,
		SetPlayerName,
		SetPlayerFaction,
		SetPlayerColor,
		LeaveLobby,
		MpNewGameLibrary,
		MpLoadGameLibrary,
		BackToLobby,
		Lobby,
		Workshop,
		OptionsBackTitle,
		OptionsBackInGame,
		GameName,
		GameNameEdit,
		PlayerName,
		PlayerNameEdit,
		PlayerFaction,
		PlayerFactionEdit,
		PlayerTeam,
		PlayerTeamEdit,
		MultiplayerPort,
		CreditIsland,
		SendToWorkshop,
		ShowHelpAtGameStart,
		Help,
		SSAOQuality,
		CanvasScale,
		Bloom,
		ColorCorrection,
		ShadowQuality,
		ShadowResolution,
		Keybindings_Global,
		Keybindings_TaskPanel,
		Keybindings_BlocksPanel,
		Keybindings_StructuresPanel,
		Keybindings_Tasks,
		Keybindings_Crafting,
		Keybindings_Blueprint,
		Keybindings_WorldEditor,
		Keybindings_WorldEditor_Terrain,
		Keybindings_WorldEditor_Markers
	}

	private readonly Signal _onClearSignal = new Signal();

	Signal ISignalConnector<Life>.OnClearSignal => _onClearSignal;

	Dictionary<Life, Tuple<List<LifeSignalConnectionBase>, Signal>> ISignalConnector<Life>.SignalMap { get; set; }

	protected virtual SignalHandle<Life> AddSignals()
	{
		return new SignalHandle<Life>(this);
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
		_onClearSignal.Invoke();
		_onClearSignal.Clear();
	}

	protected virtual void Init()
	{
	}

	public virtual void Execute(Operation executeAction)
	{
	}
}
