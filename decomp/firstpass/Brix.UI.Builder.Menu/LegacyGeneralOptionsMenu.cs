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
public class LegacyGeneralOptionsMenu : LegacyBaseMenu
{
	public static bool isDebug = false;

	public Container container;

	public Color DefaultColor;

	public Color DisabledColor;

	private bool disableToggling;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<LegacyGeneralOptionsMenu> OnAfterContainerEnable = delegate(LegacyGeneralOptionsMenu menu)
	{
		UpdateMenuItemsFromSettings(menu);
	};

	private static readonly Action<int, bool, LegacyGeneralOptionsMenu> OnItemToggle = delegate(int idx, bool flag, LegacyGeneralOptionsMenu menu)
	{
		Container.Item item = menu.container.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		if (containerPopulatorOperation == Operation.MultiplayerPort && item.instance != null)
		{
			item.instance.spriteImage.enabled = flag;
			item.instance.spriteImage.color = BaseMenu.Colors.White;
		}
	};

	private static readonly Action<int, string, LegacyGeneralOptionsMenu> OnItemText = delegate(int idx, string value, LegacyGeneralOptionsMenu menu)
	{
		Container.Item item = menu.container.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		if (containerPopulatorOperation == Operation.MultiplayerPort)
		{
			string text = value;
			text = ((!string.IsNullOrEmpty(text)) ? text : "3000");
			int num = int.Parse(text);
			UserSettings settings = UserSettingManager.TempSettings.Settings;
			settings.network.Port = num.ToString();
			UserSettingManager.TempSettings.SetDirty();
			UserSettingManager.TempSettings.SetIntoGame();
		}
	};

	public static Action<int, float, LegacyGeneralOptionsMenu> OnItemScrollEnd = delegate(int idx, float value, LegacyGeneralOptionsMenu menu)
	{
		Container.Item item = menu.container.items[idx];
		switch (item.containerPopulatorOperation)
		{
		case Operation.MouseScrollFactor:
		{
			UserSettings settings8 = UserSettingManager.TempSettings.Settings;
			settings8.input.scrollwheelFactor = value;
			UserSettingManager.TempSettings.SetDirty();
			UserSettingManager.TempSettings.SetIntoGame();
			break;
		}
		case Operation.FogOpacity:
		{
			UserSettings settings7 = UserSettingManager.TempSettings.Settings;
			settings7.graphic.fogOpacity = value;
			UserSettingManager.TempSettings.SetDirty();
			UserSettingManager.TempSettings.SetIntoGame();
			break;
		}
		case Operation.MasterVolume:
		{
			UserSettings settings6 = UserSettingManager.TempSettings.Settings;
			settings6.audio.volumeMaster = value;
			UserSettingManager.TempSettings.SetDirty();
			UserSettingManager.TempSettings.SetIntoGame();
			break;
		}
		case Operation.UserInterfaceVolume:
		{
			UserSettings settings5 = UserSettingManager.TempSettings.Settings;
			settings5.audio.volumeUI = value;
			UserSettingManager.TempSettings.SetDirty();
			UserSettingManager.TempSettings.SetIntoGame();
			break;
		}
		case Operation.SoundEffectsVolume:
		{
			UserSettings settings4 = UserSettingManager.TempSettings.Settings;
			settings4.audio.volumeSon = value;
			UserSettingManager.TempSettings.SetDirty();
			UserSettingManager.TempSettings.SetIntoGame();
			break;
		}
		case Operation.VoicesVolume:
		{
			UserSettings settings3 = UserSettingManager.TempSettings.Settings;
			settings3.audio.volumeVoice = value;
			UserSettingManager.TempSettings.SetDirty();
			UserSettingManager.TempSettings.SetIntoGame();
			break;
		}
		case Operation.EnvironmentVolume:
		{
			UserSettings settings2 = UserSettingManager.TempSettings.Settings;
			settings2.audio.volumeAmbiance = value;
			UserSettingManager.TempSettings.SetDirty();
			UserSettingManager.TempSettings.SetIntoGame();
			break;
		}
		case Operation.MusicVolume:
		{
			UserSettings settings = UserSettingManager.TempSettings.Settings;
			settings.audio.volumeMusique = value;
			UserSettingManager.TempSettings.SetDirty();
			UserSettingManager.TempSettings.SetIntoGame();
			break;
		}
		}
		ApplyFromSettings(item, menu);
	};

	private static readonly Action<int, LegacyGeneralOptionsMenu> OnItemClick = delegate(int idx, LegacyGeneralOptionsMenu menu)
	{
		Container.Item item = menu.container.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		if (!menu.disableToggling)
		{
			UserSettings settings = UserSettingManager.TempSettings.Settings;
			bool flag = false;
			switch (containerPopulatorOperation)
			{
			case Operation.MuteAllSounds:
				settings.audio.volumeMute = !settings.audio.volumeMute;
				flag = true;
				break;
			case Operation.ShowHelpAtGameStart:
				settings.game.showHelpAtGameStart = !settings.game.showHelpAtGameStart;
				flag = true;
				break;
			case Operation.ShowWelcomeScreenAtGameStart:
				settings.game.welcomeDisclaimers = !settings.game.welcomeDisclaimers;
				flag = true;
				break;
			case Operation.PauseGameWhenWindowLosesFocus:
				settings.game.pauseOnFocusLost = !settings.game.pauseOnFocusLost;
				flag = true;
				break;
			case Operation.ShowUnsupportedGameModes:
				settings.game.showUnsupportedGameModes = !settings.game.showUnsupportedGameModes;
				flag = true;
				break;
			case Operation.EnableSpecialAssets:
				settings.game.disableSpecialAssets = !settings.game.disableSpecialAssets;
				flag = true;
				break;
			case Operation.InvertMouseScroll:
				settings.input.scrollwheelInvert = !settings.input.scrollwheelInvert;
				flag = true;
				break;
			case Operation.ToggleScreenLock:
				settings.input.enableScreenLock = !settings.input.enableScreenLock;
				flag = true;
				break;
			case Operation.ToggleScreenNudge:
				if (settings.input.enableScreenLock)
				{
					settings.input.enableScreenNudge = !settings.input.enableScreenNudge;
					flag = true;
				}
				break;
			case Operation.ToggleFullscreen:
				settings.graphic.fullscreen = !settings.graphic.fullscreen;
				flag = true;
				break;
			case Operation.Bloom:
				settings.graphic.bloom = !settings.graphic.bloom;
				flag = true;
				break;
			case Operation.ColorCorrection:
				settings.graphic.colorCorrection = !settings.graphic.colorCorrection;
				flag = true;
				break;
			}
			if (flag)
			{
				UserSettingManager.TempSettings.SetDirty();
				UserSettingManager.TempSettings.SetIntoGame();
			}
			UpdateMenuItemsFromSettings(menu);
		}
	};

	private static readonly Action<LegacyGeneralOptionsMenu> UpdateMenuItemsFromSettings = delegate(LegacyGeneralOptionsMenu menu)
	{
		for (int i = 0; i < menu.container.items.Count; i++)
		{
			ApplyFromSettings(menu.container.items[i], menu);
		}
	};

	private static readonly Action<Container.Item, LegacyGeneralOptionsMenu> ApplyFromSettings = delegate(Container.Item item, LegacyGeneralOptionsMenu menu)
	{
		UserSettings settings = UserSettingManager.TempSettings.Settings;
		switch (item.containerPopulatorOperation)
		{
		case Operation.MultiplayerPort:
		{
			string port = settings.network.Port;
			if (item.instance != null)
			{
				if (item.instance.actualInputField != null)
				{
					item.instance.actualInputField.contentType = InputField.ContentType.IntegerNumber;
					item.instance.actualInputField.textComponent.text = port;
					item.instance.actualInputField.text = port;
				}
				if (item.instance.inputField != null)
				{
					item.instance.inputField.text = port;
				}
			}
			break;
		}
		case Operation.MuteAllSounds:
		{
			bool volumeMute = settings.audio.volumeMute;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!volumeMute) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = volumeMute;
				}
			}
			break;
		}
		case Operation.MasterVolume:
		{
			float volumeMaster = settings.audio.volumeMaster;
			if (item.instance != null)
			{
				if (item.instance.slider != null)
				{
					item.instance.slider.value = volumeMaster;
				}
				if (item.instance.sliderValueTitle != null)
				{
					item.instance.sliderValueTitle.text = $"{Mathf.Round(Mathf.Abs(volumeMaster) * 100f).ToString()} %";
				}
			}
			break;
		}
		case Operation.UserInterfaceVolume:
		{
			float volumeUI = settings.audio.volumeUI;
			if (item.instance != null)
			{
				if (item.instance.slider != null)
				{
					item.instance.slider.value = volumeUI;
				}
				if (item.instance.sliderValueTitle != null)
				{
					item.instance.sliderValueTitle.text = $"{Mathf.Round(Mathf.Abs(volumeUI) * 100f).ToString()} %";
				}
			}
			break;
		}
		case Operation.SoundEffectsVolume:
		{
			float volumeSon = settings.audio.volumeSon;
			if (item.instance != null)
			{
				if (item.instance.slider != null)
				{
					item.instance.slider.value = volumeSon;
				}
				if (item.instance.sliderValueTitle != null)
				{
					item.instance.sliderValueTitle.text = $"{Mathf.Round(Mathf.Abs(volumeSon) * 100f).ToString()} %";
				}
			}
			break;
		}
		case Operation.VoicesVolume:
		{
			float volumeVoice = settings.audio.volumeVoice;
			if (item.instance != null)
			{
				if (item.instance.slider != null)
				{
					item.instance.slider.value = volumeVoice;
				}
				if (item.instance.sliderValueTitle != null)
				{
					item.instance.sliderValueTitle.text = $"{Mathf.Round(Mathf.Abs(volumeVoice) * 100f).ToString()} %";
				}
			}
			break;
		}
		case Operation.EnvironmentVolume:
		{
			float volumeAmbiance = settings.audio.volumeAmbiance;
			if (item.instance != null)
			{
				if (item.instance.slider != null)
				{
					item.instance.slider.value = volumeAmbiance;
				}
				if (item.instance.sliderValueTitle != null)
				{
					item.instance.sliderValueTitle.text = $"{Mathf.Round(Mathf.Abs(volumeAmbiance) * 100f).ToString()} %";
				}
			}
			break;
		}
		case Operation.MusicVolume:
		{
			float volumeMusique = settings.audio.volumeMusique;
			if (item.instance != null)
			{
				if (item.instance.slider != null)
				{
					item.instance.slider.value = volumeMusique;
				}
				if (item.instance.sliderValueTitle != null)
				{
					item.instance.sliderValueTitle.text = $"{Mathf.Round(Mathf.Abs(volumeMusique) * 100f).ToString()} %";
				}
			}
			break;
		}
		case Operation.FogOpacity:
		{
			float fogOpacity = settings.graphic.fogOpacity;
			if (item.instance != null)
			{
				if (item.instance.slider != null)
				{
					item.instance.slider.value = fogOpacity;
				}
				if (item.instance.sliderValueTitle != null)
				{
					item.instance.sliderValueTitle.text = $"{Mathf.Round(Mathf.Abs(fogOpacity) * 100f).ToString()} %";
				}
			}
			break;
		}
		case Operation.ShowWelcomeScreenAtGameStart:
		{
			bool welcomeDisclaimers = settings.game.welcomeDisclaimers;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!welcomeDisclaimers) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = welcomeDisclaimers;
				}
			}
			break;
		}
		case Operation.ShowHelpAtGameStart:
		{
			bool showHelpAtGameStart = settings.game.showHelpAtGameStart;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!showHelpAtGameStart) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = showHelpAtGameStart;
				}
			}
			break;
		}
		case Operation.PauseGameWhenWindowLosesFocus:
		{
			bool pauseOnFocusLost = settings.game.pauseOnFocusLost;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!pauseOnFocusLost) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = pauseOnFocusLost;
				}
			}
			break;
		}
		case Operation.EnableSpecialAssets:
		{
			bool disableSpecialAssets = settings.game.disableSpecialAssets;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!disableSpecialAssets) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = disableSpecialAssets;
				}
			}
			break;
		}
		case Operation.ShowUnsupportedGameModes:
		{
			bool showUnsupportedGameModes = settings.game.showUnsupportedGameModes;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!showUnsupportedGameModes) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = showUnsupportedGameModes;
				}
			}
			break;
		}
		case Operation.MouseScrollFactor:
			item.sliderValue = settings.input.scrollwheelFactor;
			if (item.instance != null)
			{
				item.instance.slider.value = item.sliderValue;
				item.instance.sliderValueTitle.text = $"{Mathf.Round(Mathf.Abs(item.sliderValue) * 100f).ToString()} %";
			}
			break;
		case Operation.InvertMouseScroll:
		{
			bool scrollwheelInvert = settings.input.scrollwheelInvert;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!scrollwheelInvert) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = scrollwheelInvert;
				}
			}
			break;
		}
		case Operation.ToggleScreenLock:
		{
			bool enableScreenLock = settings.input.enableScreenLock;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!enableScreenLock) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = enableScreenLock;
				}
			}
			break;
		}
		case Operation.ToggleScreenNudge:
		{
			bool flag = settings.input.enableScreenNudge;
			string text = string.Empty;
			if (flag && !settings.input.enableScreenLock)
			{
				text = "*";
				flag = false;
			}
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!flag) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper() + text;
					item.instance.toggleText.color = ((!settings.input.enableScreenLock) ? menu.DisabledColor : menu.DefaultColor);
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = flag;
					item.instance.checkmarkImage.color = ((!settings.input.enableScreenLock) ? menu.DisabledColor : menu.DefaultColor);
				}
				if (item.instance.checkboxImage != null)
				{
					item.instance.checkboxImage.color = ((!settings.input.enableScreenLock) ? menu.DisabledColor : menu.DefaultColor);
				}
			}
			break;
		}
		case Operation.ToggleFullscreen:
		{
			bool fullscreen = settings.graphic.fullscreen;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!fullscreen) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = fullscreen;
				}
			}
			break;
		}
		case Operation.Bloom:
		{
			bool bloom = settings.graphic.bloom;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!bloom) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = bloom;
				}
			}
			break;
		}
		case Operation.ColorCorrection:
		{
			bool colorCorrection = settings.graphic.colorCorrection;
			if (item.instance != null)
			{
				if (item.instance.toggleText != null)
				{
					item.instance.toggleText.text = I2Helper.TryGet((!colorCorrection) ? Container.Item.StrDisabled : Container.Item.StrEnabled).ToUpper();
				}
				if (item.instance.checkmarkImage != null)
				{
					item.instance.checkmarkImage.enabled = colorCorrection;
				}
			}
			break;
		}
		}
	};

	private void OnEnable()
	{
		Connect();
		UpdateMenuItemsFromSettings(this);
	}

	private void OnDisable()
	{
		Disconnect();
	}

	private void Connect()
	{
		if (container != null)
		{
			_sgs.Add(container.OnAfterEnableSignal.Connect(OnAfterContainerEnable, this));
		}
		if (container != null)
		{
			_sgs.Add(container.OnItemScrollEndSignal.Connect(OnItemScrollEnd, this));
		}
		if (container != null)
		{
			_sgs.Add(container.OnItemClickSignal.Connect(OnItemClick, this));
		}
		if (container != null)
		{
			_sgs.Add(container.OnItemTextSignal.Connect(OnItemText, this));
		}
		if (container != null)
		{
			_sgs.Add(container.OnItemToggleSignal.Connect(OnItemToggle, this));
		}
		_sgs.Add(GameSignals.Connect(UserSettingManager.TempSettings.OnTempSettingsSetDirty, UpdateMenuItemsFromSettings, this));
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
