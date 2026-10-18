using System;
using System.Collections.Generic;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyTerrainAccelerationMenu : LegacyBaseEnumMenu
{
	public enum Mode
	{
		Never,
		Autodetect,
		Always
	}

	private readonly int _modeCount = Enum.GetValues(typeof(Mode)).Length;

	private static Dictionary<int, string> _optionTitleMap;

	private static Dictionary<int, string> _optionDescriptionMap;

	protected override Operation TargetOperation => Operation.TerrainAcceleration;

	protected override bool ShowHelperTitle => true;

	protected override bool ShowHelperDescription => true;

	protected override int OptionCount => _modeCount;

	protected override void Init()
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		dictionary.Add(0, I2Helper.TryGet("##mainmenu_options_graphics_terrain_never").ToUpper());
		dictionary.Add(1, I2Helper.TryGet("##mainmenu_options_graphics_terrain_autodetect").ToUpper());
		dictionary.Add(2, I2Helper.TryGet("##mainmenu_options_graphics_terrain_always").ToUpper());
		_optionTitleMap = dictionary;
		dictionary = new Dictionary<int, string>();
		dictionary.Add(0, I2Helper.TryGet("##mainmenu_options_graphics_terrain_never_info"));
		dictionary.Add(1, I2Helper.TryGet("##mainmenu_options_graphics_terrain_autodetect_info"));
		dictionary.Add(2, I2Helper.TryGet("##mainmenu_options_graphics_terrain_always_info"));
		_optionDescriptionMap = dictionary;
		base.Init();
	}

	protected override string GetTitleLabel()
	{
		return I2Helper.TryGet("##mainmenu_options_graphics_terrain").ToUpper();
	}

	protected override string GetOptionLabel(int optionIndex)
	{
		return _optionTitleMap[optionIndex].ToUpper();
	}

	protected override string GetOptionDescription(int optionIndex)
	{
		return _optionDescriptionMap[optionIndex];
	}

	protected override bool CompareWithSettings(UserSettings settings, int optionIndex)
	{
		return optionIndex == settings.graphic.terrainThreading;
	}

	protected override void GetValueFromSettings(UserSettings settings, out int optionIndex)
	{
		optionIndex = settings.graphic.terrainThreading;
	}

	protected override void SetValueAtSettings(UserSettings settings, int optionIndex)
	{
		settings.graphic.terrainThreading = optionIndex;
	}
}
