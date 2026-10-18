using System.Collections.Generic;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacySSAOQualityMenu : LegacyBaseEnumMenu
{
	private static List<UserSettings.SSAOQualityLevels> _options;

	private static Dictionary<UserSettings.SSAOQualityLevels, string> _optionTitleMap;

	protected override Operation TargetOperation => Operation.SSAOQuality;

	protected override int OptionCount => _options.Count;

	protected override void Init()
	{
		List<UserSettings.SSAOQualityLevels> list = new List<UserSettings.SSAOQualityLevels>();
		list.Add(UserSettings.SSAOQualityLevels.high);
		list.Add(UserSettings.SSAOQualityLevels.medium);
		list.Add(UserSettings.SSAOQualityLevels.low);
		list.Add(UserSettings.SSAOQualityLevels.off);
		_options = list;
		Dictionary<UserSettings.SSAOQualityLevels, string> dictionary = new Dictionary<UserSettings.SSAOQualityLevels, string>();
		dictionary.Add(UserSettings.SSAOQualityLevels.high, I2Helper.TryGet("##mainmenu_options_graphics_ssao_high").ToUpper());
		dictionary.Add(UserSettings.SSAOQualityLevels.medium, I2Helper.TryGet("##mainmenu_options_graphics_ssao_medium").ToUpper());
		dictionary.Add(UserSettings.SSAOQualityLevels.low, I2Helper.TryGet("##mainmenu_options_graphics_ssao_low").ToUpper());
		dictionary.Add(UserSettings.SSAOQualityLevels.off, I2Helper.TryGet("##mainmenu_options_graphics_ssao_off").ToUpper());
		_optionTitleMap = dictionary;
		base.Init();
	}

	private int FindOptionIndex(UserSettings.SSAOQualityLevels value)
	{
		return _options.IndexOf(value);
	}

	protected override string GetTitleLabel()
	{
		return I2Helper.TryGet("##mainmenu_options_graphics_ssao").ToUpper();
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
		optionIndex = FindOptionIndex((UserSettings.SSAOQualityLevels)Mathf.Clamp((int)settings.graphic.ssao, 0, _options.Count - 1));
	}

	protected override void SetValueAtSettings(UserSettings settings, int optionIndex)
	{
		settings.graphic.ssao = _options[optionIndex];
	}
}
