using System.Collections.Generic;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyShadowResolutionMenu : LegacyBaseEnumMenu
{
	private static List<ShadowResolution> _options;

	private static Dictionary<ShadowResolution, string> _optionTitleMap;

	protected override Operation TargetOperation => Operation.ShadowResolution;

	protected override int OptionCount => _options.Count;

	protected override void Init()
	{
		List<ShadowResolution> list = new List<ShadowResolution>();
		list.Add(ShadowResolution.VeryHigh);
		list.Add(ShadowResolution.High);
		list.Add(ShadowResolution.Medium);
		list.Add(ShadowResolution.Low);
		_options = list;
		Dictionary<ShadowResolution, string> dictionary = new Dictionary<ShadowResolution, string>();
		dictionary.Add(ShadowResolution.VeryHigh, I2Helper.TryGet("##mainmenu_options_graphics_shadowresolution_veryhigh").ToUpper());
		dictionary.Add(ShadowResolution.High, I2Helper.TryGet("##mainmenu_options_graphics_shadowresolution_high").ToUpper());
		dictionary.Add(ShadowResolution.Medium, I2Helper.TryGet("##mainmenu_options_graphics_shadowresolution_medium").ToUpper());
		dictionary.Add(ShadowResolution.Low, I2Helper.TryGet("##mainmenu_options_graphics_shadowresolution_low").ToUpper());
		_optionTitleMap = dictionary;
		base.Init();
	}

	private int FindOptionIndex(ShadowResolution value)
	{
		return _options.IndexOf(value);
	}

	protected override string GetTitleLabel()
	{
		return I2Helper.TryGet("##mainmenu_options_graphics_shadowresolution").ToUpper();
	}

	protected override string GetOptionLabel(int optionIndex)
	{
		return _optionTitleMap[_options[optionIndex]].ToUpper();
	}

	protected override string GetOptionDescription(int optionIndex)
	{
		return string.Empty;
	}

	protected override bool CompareWithSettings(UserSettings settings, int optionIndex)
	{
		GetValueFromSettings(settings, out var optionIndex2);
		return optionIndex == optionIndex2;
	}

	protected override void GetValueFromSettings(UserSettings settings, out int optionIndex)
	{
		optionIndex = FindOptionIndex((ShadowResolution)Mathf.Clamp((int)settings.graphic.shadowResolution, 0, _options.Count - 1));
	}

	protected override void SetValueAtSettings(UserSettings settings, int optionIndex)
	{
		settings.graphic.shadowResolution = _options[optionIndex];
	}
}
