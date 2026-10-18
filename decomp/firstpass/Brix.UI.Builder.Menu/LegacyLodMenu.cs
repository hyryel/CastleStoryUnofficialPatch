using System.Collections.Generic;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyLodMenu : LegacyBaseEnumMenu
{
	private static List<UserSettings.LODLevels> _options;

	private static Dictionary<int, string> _optionTitleMap;

	protected override Operation TargetOperation => Operation.Lod;

	protected override int OptionCount => 3;

	protected override void Init()
	{
		List<UserSettings.LODLevels> list = new List<UserSettings.LODLevels>();
		list.Add(UserSettings.LODLevels.low);
		list.Add(UserSettings.LODLevels.medium);
		list.Add(UserSettings.LODLevels.high);
		_options = list;
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		dictionary.Add(0, I2Helper.TryGet("##mainmenu_options_graphics_levelofdetail_low").ToUpper());
		dictionary.Add(1, I2Helper.TryGet("##mainmenu_options_graphics_levelofdetail_medium").ToUpper());
		dictionary.Add(2, I2Helper.TryGet("##mainmenu_options_graphics_levelofdetail_high").ToUpper());
		_optionTitleMap = dictionary;
		base.Init();
	}

	private int FindOptionIndex(UserSettings.LODLevels lod)
	{
		return (int)lod;
	}

	protected override string GetTitleLabel()
	{
		return I2Helper.TryGet("##mainmenu_options_graphics_levelofdetail").ToUpper();
	}

	protected override string GetOptionLabel(int optionIndex)
	{
		return _optionTitleMap[optionIndex].ToUpper();
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
		optionIndex = FindOptionIndex((UserSettings.LODLevels)Mathf.Clamp((int)settings.graphic.terrainLodQuality, 0, 2));
	}

	protected override void SetValueAtSettings(UserSettings settings, int optionIndex)
	{
		settings.graphic.terrainLodQuality = _options[optionIndex];
	}
}
