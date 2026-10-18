using System.Collections.Generic;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyResolutionMenu : LegacyBaseEnumMenu
{
	private readonly List<string> _options = new List<string>();

	private readonly Dictionary<string, Resolution> _optionMap = new Dictionary<string, Resolution>();

	protected override int OptionCount => _options.Count;

	protected override Operation TargetOperation => Operation.Resolution;

	protected override void Init()
	{
		InitOptions();
		base.Init();
	}

	private void InitOptions()
	{
		_options.Clear();
		_optionMap.Clear();
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution value = resolutions[i];
			string text = value.ToString();
			if (!_options.Contains(text))
			{
				_options.Add(text);
				_optionMap.Add(text, value);
			}
		}
	}

	private int FindResolution(int w, int h, bool defaultToCurrent = true)
	{
		for (int i = 0; i < _options.Count; i++)
		{
			Resolution resolution = _optionMap[_options[i]];
			if (resolution.width == w && resolution.height == h)
			{
				return i;
			}
		}
		if (defaultToCurrent)
		{
			InitOptions();
			Resolution currentResolution = Screen.currentResolution;
			return FindResolution(currentResolution.width, currentResolution.height, defaultToCurrent: false);
		}
		return 0;
	}

	protected override bool CompareWithSettings(UserSettings settings, int optionIndex)
	{
		GetValueFromSettings(settings, out var optionIndex2);
		return optionIndex == optionIndex2;
	}

	protected override void GetValueFromSettings(UserSettings settings, out int optionIndex)
	{
		int screenResolutionX = settings.graphic.screenResolutionX;
		int screenResolutionY = settings.graphic.screenResolutionY;
		optionIndex = FindResolution(screenResolutionX, screenResolutionY);
	}

	protected override void SetValueAtSettings(UserSettings settings, int optionIndex)
	{
		settings.graphic.screenResolutionX = _optionMap[_options[optionIndex]].width;
		settings.graphic.screenResolutionY = _optionMap[_options[optionIndex]].height;
	}

	protected override string GetTitleLabel()
	{
		return I2Helper.TryGet("##mainmenu_options_graphics_screenresolution").ToUpper();
	}

	protected override string GetOptionLabel(int optionIndex)
	{
		return _options[optionIndex];
	}

	protected override string GetOptionDescription(int optionIndex)
	{
		return string.Empty;
	}
}
