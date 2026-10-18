using Brix.Audio;
using Brix.Engine;
using Brix.Input;
using Brix.Network;
using Brix.Rendering;
using Brix.ThirdParty;
using Brix.UI.Builder.Menu;
using Brix.Utils.Logging;
using I2.Loc;
using UnityEngine;

namespace Brix.NewUI.UserSettings_;

public static class UserSettingManager
{
	public static class TempSettings
	{
		public static BrixLogger logger = new BrixLogger(typeof(TempSettings).FullName);

		private static UserSettings _settings;

		public static GameSignals.GameSignal OnTempSettingsSetDirty = new GameSignals.GameSignal();

		private static bool _isDirty;

		public static UserSettings Settings => _settings ?? (_settings = UserSettingsReader.Read());

		public static bool IsDirty => _isDirty;

		public static void SetDirty(bool isDirty = true)
		{
			_isDirty = isDirty;
			GameSignals.Invoke(OnTempSettingsSetDirty);
		}

		public static void Revert()
		{
			_settings = UserSettingsReader.GetDefault();
			SetIntoGame();
			UserSettingsReader.Save(_settings);
			SetDirty(isDirty: false);
			GameSignals.Invoke(BindingsProvider.OperateSignal, LegacyBaseMenu.Operation.ResetKeysToDefault);
		}

		public static void RevertSettings()
		{
			_settings = UserSettingsReader.GetDefault();
			SetIntoGame();
			UserSettingsReader.Save(_settings);
			SetDirty(isDirty: false);
		}

		public static void RevertKeybindings()
		{
			SetDirty(isDirty: false);
			GameSignals.Invoke(BindingsProvider.OperateSignal, LegacyBaseMenu.Operation.ResetKeysToDefault);
		}

		public static void Cancel()
		{
			_settings = UserSettingsReader.Read();
			SetIntoGame();
			SetDirty(isDirty: false);
			GameSignals.Invoke(BindingsProvider.OperateSignal, LegacyBaseMenu.Operation.Reset);
		}

		public static void Apply()
		{
			UserSettingsReader.Save(_settings);
			SetDirty(isDirty: false);
			GameSignals.Invoke(BindingsProvider.OperateSignal, LegacyBaseMenu.Operation.Apply);
		}

		public static void SetIntoGame()
		{
			SetSettingIntoGame(_settings);
		}
	}

	public static GameSignals.GameSignal OnSetSettingIntoGameSignal = new GameSignals.GameSignal();

	public static UserSettings SetSettingIntoGame(UserSettings settings)
	{
		if (Screen.currentResolution.width != settings.graphic.screenResolutionX || Screen.currentResolution.height != settings.graphic.screenResolutionY || Screen.fullScreen != settings.graphic.fullscreen)
		{
			if (settings.graphic.screenResolutionX != 0 && settings.graphic.screenResolutionY != 0)
			{
				Screen.SetResolution(settings.graphic.screenResolutionX, settings.graphic.screenResolutionY, settings.graphic.fullscreen);
			}
			else
			{
				Screen.fullScreen = settings.graphic.fullscreen;
			}
		}
		Application.targetFrameRate = Neo.GetActualDesiredFPS(settings.graphic.targetFPS);
		QualitySettings.SetQualityLevel(settings.graphic.qualitySettings);
		CameraEffectsManager.SSAOQuality = settings.graphic.ssao;
		CameraEffectsManager.ColorCorrectionEnabled = settings.graphic.colorCorrection;
		CameraEffectsManager.BloomEnabled = settings.graphic.bloom;
		CameraEffectsManager.ShadowQuality = settings.graphic.shadowQuality;
		CameraEffectsManager.ShadowResolution = settings.graphic.shadowResolution;
		Neo.terrainLodQuality = (int)settings.graphic.terrainLodQuality;
		Neo.canvasScale = settings.graphic.canvasScale;
		Neo.overrideTerrainAlgos = settings.graphic.overrideTerrainAlgos;
		Neo.overrideTerrainLodMax = settings.graphic.overrideTerrainLodMax;
		Neo.overrideTerrainLateAlgos = settings.graphic.overrideTerrainLateAlgos;
		Neo.terrainAcceleration = settings.graphic.terrainAcceleration;
		Neo.TerrainThreading = settings.graphic.terrainThreading;
		Architecte.heightmapMode = settings.graphic.editHeightmap;
		FogOfWarRenderer.Opacity = settings.graphic.fogOpacity;
		SoundEngine.mute = settings.audio.volumeMute;
		SoundEngine.volumeMaster = settings.audio.volumeMaster * 100f;
		SoundEngine.volumeUI = settings.audio.volumeUI * 100f;
		SoundEngine.volumeSFX = settings.audio.volumeSon * 100f;
		SoundEngine.volumeMusic = settings.audio.volumeMusique * 100f;
		SoundEngine.volumeAmbience = settings.audio.volumeAmbiance * 100f;
		SoundEngine.volumeVoice = settings.audio.volumeVoice * 100f;
		Application.runInBackground = Neo.multiplayer || !settings.game.pauseOnFocusLost;
		Neo.disableSpecialAssets = settings.game.disableSpecialAssets;
		Neo.skipWelcomeDisclaimers = settings.game.welcomeDisclaimers;
		Neo.showHelpAtGameStart = settings.game.showHelpAtGameStart;
		Neo.showUnsupportedGameModes = settings.game.showUnsupportedGameModes;
		CameraScript.ScrollFactor = settings.input.scrollwheelFactor * (float)((!settings.input.scrollwheelInvert) ? 1 : (-1));
		Curseur.instance.disableLockBySettings = !settings.input.enableScreenLock;
		Curseur.instance.disableNudgeBySettings = !settings.input.enableScreenNudge;
		Neo.multiplayerIP = (string.IsNullOrEmpty(settings.network.IP) ? "0.0.0.0" : settings.network.IP);
		Neo.multiplayerPort = (string.IsNullOrEmpty(settings.network.Port) ? "3000" : settings.network.Port);
		Neo.multiplayerName = settings.network.Name;
		UNetManager.SetHostingPort(string.IsNullOrEmpty(settings.network.Port) ? 3000 : int.Parse(settings.network.Port));
		LocalizationController.UpdateLanguages();
		LocalizationManager.CurrentLanguage = settings.localization.language;
		GameSignals.Invoke(OnSetSettingIntoGameSignal);
		return GetSettingsFromGame();
	}

	public static UserSettings GetSettingsFromGame()
	{
		UserSettings userSettings = new UserSettings();
		userSettings.graphic.fullscreen = Screen.fullScreen;
		userSettings.graphic.screenResolutionX = Screen.width;
		userSettings.graphic.screenResolutionY = Screen.height;
		userSettings.graphic.qualitySettings = QualitySettings.GetQualityLevel();
		userSettings.graphic.targetFPS = Application.targetFrameRate;
		userSettings.graphic.ssao = CameraEffectsManager.SSAOQuality;
		userSettings.graphic.colorCorrection = CameraEffectsManager.ColorCorrectionEnabled;
		userSettings.graphic.bloom = CameraEffectsManager.BloomEnabled;
		userSettings.graphic.shadowQuality = CameraEffectsManager.ShadowQuality;
		userSettings.graphic.shadowResolution = CameraEffectsManager.ShadowResolution;
		userSettings.graphic.terrainLodQuality = (UserSettings.LODLevels)Neo.terrainLodQuality;
		userSettings.graphic.canvasScale = Neo.canvasScale;
		userSettings.graphic.overrideTerrainAlgos = Neo.overrideTerrainAlgos;
		userSettings.graphic.overrideTerrainLodMax = Neo.overrideTerrainLodMax;
		userSettings.graphic.overrideTerrainLateAlgos = Neo.overrideTerrainLateAlgos;
		userSettings.graphic.terrainAcceleration = Neo.terrainAcceleration;
		userSettings.graphic.terrainThreading = Neo.TerrainThreading;
		userSettings.graphic.editHeightmap = Architecte.heightmapMode;
		userSettings.graphic.fogOpacity = FogOfWarRenderer.Opacity;
		userSettings.graphic._version = 1;
		userSettings.audio.volumeMute = SoundEngine.mute;
		userSettings.audio.volumeMaster = SoundEngine.volumeMaster / 100f;
		userSettings.audio.volumeUI = SoundEngine.volumeUI / 100f;
		userSettings.audio.volumeSon = SoundEngine.volumeSFX / 100f;
		userSettings.audio.volumeMusique = SoundEngine.volumeMusic / 100f;
		userSettings.audio.volumeAmbiance = SoundEngine.volumeAmbience / 100f;
		userSettings.audio.volumeVoice = SoundEngine.volumeVoice / 100f;
		userSettings.game.pauseOnFocusLost = !Application.runInBackground;
		userSettings.game.disableSpecialAssets = Neo.disableSpecialAssets;
		userSettings.game.welcomeDisclaimers = Neo.skipWelcomeDisclaimers;
		userSettings.game.showHelpAtGameStart = Neo.showHelpAtGameStart;
		userSettings.game.showUnsupportedGameModes = Neo.showUnsupportedGameModes;
		userSettings.input.scrollwheelFactor = Mathf.Abs(CameraScript.ScrollFactor);
		userSettings.input.scrollwheelInvert = CameraScript.ScrollFactor < 0f;
		userSettings.input.enableScreenLock = !Curseur.instance.disableLockBySettings;
		userSettings.input.enableScreenNudge = !Curseur.instance.disableNudgeBySettings;
		userSettings.input._configsVersion = 1;
		userSettings.network.IP = Neo.multiplayerIP;
		userSettings.network.Port = Neo.multiplayerPort;
		userSettings.network.Name = Neo.multiplayerName;
		return userSettings;
	}

	public static UserSettings OverwriteWithPreGameSettings(UserSettings settings)
	{
		settings.graphic.fullscreen = Screen.fullScreen;
		settings.graphic.screenResolutionX = Screen.width;
		settings.graphic.screenResolutionY = Screen.height;
		settings.graphic.qualitySettings = QualitySettings.GetQualityLevel();
		return settings;
	}
}
