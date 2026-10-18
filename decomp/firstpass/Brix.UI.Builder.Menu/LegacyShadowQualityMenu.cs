using System.Collections.Generic;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyShadowQualityMenu : LegacyBaseEnumMenu
{
	private static List<ShadowQuality> _options;

	private static Dictionary<ShadowQuality, string> _optionTitleMap;

	protected override Operation TargetOperation => Operation.ShadowQuality;

	protected override int OptionCount => _options.Count;

	protected override void Init()
	{
		List<ShadowQuality> list = new List<ShadowQuality>();
		list.Add(ShadowQuality.All);
		list.Add(ShadowQuality.HardOnly);
		list.Add(ShadowQuality.Disable);
		_options = list;
		Dictionary<ShadowQuality, string> dictionary = new Dictionary<ShadowQuality, string>();
		dictionary.Add(ShadowQuality.All, I2Helper.TryGet("##mainmenu_options_graphics_shadowquality_all").ToUpper());
		dictionary.Add(ShadowQuality.HardOnly, I2Helper.TryGet("##mainmenu_options_graphics_shadowquality_hardonly").ToUpper());
		dictionary.Add(ShadowQuality.Disable, I2Helper.TryGet("##mainmenu_options_graphics_shadowquality_disable").ToUpper());
		_optionTitleMap = dictionary;
		base.Init();
	}

	private int FindOptionIndex(ShadowQuality value)
	{
		return _options.IndexOf(value);
	}

	protected override string GetTitleLabel()
	{
		return I2Helper.TryGet("##mainmenu_options_graphics_shadowquality").ToUpper();
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
		optionIndex = FindOptionIndex((ShadowQuality)Mathf.Clamp((int)settings.graphic.shadowQuality, 0, _options.Count - 1));
	}

	protected override void SetValueAtSettings(UserSettings settings, int optionIndex)
	{
		settings.graphic.shadowQuality = _options[optionIndex];
	}
}
