using System.Collections.Generic;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyCanvasScaleMenu : LegacyBaseEnumMenu
{
	private static readonly List<float> Options = new List<float> { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f };

	protected override Operation TargetOperation => Operation.CanvasScale;

	protected override int OptionCount => Options.Count;

	private int FindOptionIndex(float value)
	{
		return Options.IndexOf(value);
	}

	protected override string GetTitleLabel()
	{
		return I2Helper.TryGet("##mainmenu_options_graphics_canvasscale").ToUpper();
	}

	protected override string GetOptionLabel(int optionIndex)
	{
		return $"{(Options[optionIndex] * 100f).ToString()}%";
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
		optionIndex = FindOptionIndex(settings.graphic.canvasScale);
	}

	protected override void SetValueAtSettings(UserSettings settings, int optionIndex)
	{
		settings.graphic.canvasScale = Options[optionIndex];
	}
}
