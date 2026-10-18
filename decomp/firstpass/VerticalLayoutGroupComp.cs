using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class VerticalLayoutGroupComp : Comp
{
	public static string name = "Vertical Layout Group";

	private VerticalLayoutGroup layoutGroup => component as VerticalLayoutGroup;

	private RectOffset padding => layoutGroup.padding;

	public VerticalLayoutGroupComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public VerticalLayoutGroupComp(Comp otherComp)
		: base(otherComp)
	{
		InitProps();
	}

	public void InitProps()
	{
		props = new Prop[4]
		{
			new FloatProp("[Float] Padding Left", GetPaddingLeft, SetPaddingLeft),
			new FloatProp("[Float] Padding Right", GetPaddingRight, SetPaddingRight),
			new FloatProp("[Float] Padding Top", GetPaddingTop, SetPaddingTop),
			new FloatProp("[Float] Padding Bottom", GetPaddingBottom, SetPaddingBottom)
		};
		propsIdx = ((propsIdx < props.Length) ? propsIdx : 0);
	}

	private float GetPaddingLeft()
	{
		return layoutGroup.padding.left;
	}

	private void SetPaddingLeft(float value)
	{
		layoutGroup.padding = new RectOffset((int)value, padding.right, padding.top, padding.bottom);
	}

	private float GetPaddingRight()
	{
		return layoutGroup.padding.right;
	}

	private void SetPaddingRight(float value)
	{
		layoutGroup.padding = new RectOffset(padding.left, (int)value, padding.top, padding.bottom);
	}

	private float GetPaddingTop()
	{
		return layoutGroup.padding.top;
	}

	private void SetPaddingTop(float value)
	{
		layoutGroup.padding = new RectOffset(padding.left, padding.right, (int)value, padding.bottom);
	}

	private float GetPaddingBottom()
	{
		return layoutGroup.padding.bottom;
	}

	private void SetPaddingBottom(float value)
	{
		layoutGroup.padding = new RectOffset(padding.left, padding.right, padding.top, (int)value);
	}
}
