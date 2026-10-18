using System.Collections.Generic;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public class LegacyFramerateMenu : LegacyBaseEnumMenu
{
	public class FrameCapItem
	{
		public string Label;

		public int Value;

		public FrameCapItem(string label, int value)
		{
			Label = label;
			Value = value;
		}
	}

	private readonly List<FrameCapItem> _options = new List<FrameCapItem>();

	protected override int OptionCount => _options.Count;

	protected override Operation TargetOperation => Operation.FramerateLimit;

	protected override void Init()
	{
		InitOptions();
		base.Init();
	}

	private void InitOptions()
	{
		_options.Clear();
		_options.Add(new FrameCapItem(I2Helper.TryGet("##mainmenu_options_graphics_framecap_unlimited").ToUpper(), -1));
		_options.Add(new FrameCapItem("25 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 25));
		_options.Add(new FrameCapItem("30 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 30));
		_options.Add(new FrameCapItem("50 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 50));
		_options.Add(new FrameCapItem("60 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 60));
		_options.Add(new FrameCapItem("80 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 80));
		_options.Add(new FrameCapItem("100 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 100));
		_options.Add(new FrameCapItem("120 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 120));
		_options.Add(new FrameCapItem("144 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 144));
		_options.Add(new FrameCapItem("165 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 165));
		_options.Add(new FrameCapItem("240 " + I2Helper.TryGet("##mainmenu_options_graphics_framecap_fps").ToUpper(), 240));
	}

	private int FindFrameCap(int frameCap)
	{
		for (int i = 0; i < _options.Count; i++)
		{
			FrameCapItem frameCapItem = _options[i];
			if (frameCapItem.Value == frameCap)
			{
				return i;
			}
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
		int targetFPS = settings.graphic.targetFPS;
		optionIndex = FindFrameCap(targetFPS);
	}

	protected override void SetValueAtSettings(UserSettings settings, int optionIndex)
	{
		settings.graphic.targetFPS = _options[optionIndex].Value;
	}

	protected override string GetTitleLabel()
	{
		return I2Helper.TryGet("##mainmenu_options_graphics_framecap").ToUpper();
	}

	protected override string GetOptionLabel(int optionIndex)
	{
		return _options[optionIndex].Label;
	}

	protected override string GetOptionDescription(int optionIndex)
	{
		return string.Empty;
	}
}
