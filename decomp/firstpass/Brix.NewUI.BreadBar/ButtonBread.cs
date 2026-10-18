using System;
using Brix.Utils;
using UnityEngine;

namespace Brix.NewUI.BreadBar;

public class ButtonBread
{
	public string _label;

	public Color _labelColor;

	public bool _labelColorOverride;

	public Sprite _icon;

	public Color _iconColor;

	public bool _iconColorOverride;

	public Action _clickAction;

	public bool _clickActionEnabled = true;

	public ButtonBread(string label, Sprite icon, Action clickAction, bool clickActionEnabled)
	{
		_label = I2Helper.TryGet(label);
		_icon = icon;
		_clickAction = clickAction;
		_clickActionEnabled = clickActionEnabled;
	}

	public ButtonBread(string label, Sprite icon, Action clickAction)
	{
		_label = I2Helper.TryGet(label);
		_icon = icon;
		_clickAction = clickAction;
	}

	public ButtonBread(Sprite icon, Action clickAction)
	{
		_label = string.Empty;
		_icon = icon;
		_clickAction = clickAction;
	}

	public ButtonBread SetColor(Color color)
	{
		SetLabelColor(color);
		SetIconColor(color);
		return this;
	}

	public ButtonBread SetLabelColor(Color labelColor)
	{
		_labelColor = labelColor;
		_labelColorOverride = true;
		return this;
	}

	public ButtonBread SetIconColor(Color iconColor)
	{
		_iconColor = iconColor;
		_iconColorOverride = true;
		return this;
	}

	public ButtonBread ResetColor()
	{
		ResetLabelColor();
		ResetIconColor();
		return this;
	}

	public ButtonBread ResetLabelColor()
	{
		_labelColorOverride = false;
		return this;
	}

	public ButtonBread ResetIconColor()
	{
		_iconColorOverride = false;
		return this;
	}
}
