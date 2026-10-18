using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LayoutElementComp : Comp
{
	public static string name = "Layout Element";

	private LayoutElement layoutElement => component as LayoutElement;

	public LayoutElementComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public LayoutElementComp(Comp otherComp)
		: base(otherComp)
	{
		InitProps();
	}

	public void InitProps()
	{
		props = new Prop[4]
		{
			new FloatProp("[Float] Min Width", GetMinWidth, SetMinWidth),
			new FloatProp("[Float] Min Height", GetMinHeight, SetMinHeight),
			new FloatProp("[Float] Preferred Width", GetPreferredWidth, SetPreferredWidth),
			new FloatProp("[Float] Preferred Height", GetPreferredHeight, SetPreferredHeight)
		};
		propsIdx = ((propsIdx < props.Length) ? propsIdx : 0);
	}

	private float GetMinWidth()
	{
		return layoutElement.minWidth;
	}

	private void SetMinWidth(float value)
	{
		layoutElement.minWidth = value;
	}

	private float GetMinHeight()
	{
		return layoutElement.minHeight;
	}

	private void SetMinHeight(float value)
	{
		layoutElement.minHeight = value;
	}

	private float GetPreferredWidth()
	{
		return layoutElement.preferredWidth;
	}

	private void SetPreferredWidth(float value)
	{
		layoutElement.preferredWidth = value;
	}

	private float GetPreferredHeight()
	{
		return layoutElement.preferredHeight;
	}

	private void SetPreferredHeight(float value)
	{
		layoutElement.preferredHeight = value;
	}
}
