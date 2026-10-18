using System;
using UnityEngine;

[Serializable]
public class RadialLayoutElementComp : Comp
{
	public static string name = "Radial Layout Element";

	private RadialLayoutElement layoutElement => component as RadialLayoutElement;

	public RadialLayoutElementComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public RadialLayoutElementComp(Comp otherComp)
		: base(otherComp)
	{
		InitProps();
	}

	public void InitProps()
	{
		props = new Prop[5]
		{
			new FloatProp("[Float] Angle Offset AO", GetAngleOffset, SetAngleOffset),
			new FloatProp("[Float] Radius Inner AO", GetRadiusInner, SetRadiusInner),
			new FloatProp("[Float] Radius Outer AO", GetRadiusOuter, SetRadiousOuter),
			new ColorProp("[Color] Color", GetColor, SetColor),
			new FloatProp("[Float] Color Alpha Factor", GetColorAlphaFactor, SetColorAlphaFactor)
		};
		propsIdx = ((propsIdx < props.Length) ? propsIdx : 0);
	}

	private float GetAngleOffset()
	{
		return layoutElement.angleOffsetAO;
	}

	private void SetAngleOffset(float value)
	{
		layoutElement.angleOffsetAO = value;
	}

	private float GetRadiusInner()
	{
		return layoutElement.radiusInnerAO;
	}

	private void SetRadiusInner(float value)
	{
		layoutElement.radiusInnerAO = value;
	}

	private float GetRadiusOuter()
	{
		return layoutElement.radiusOuterAO;
	}

	private void SetRadiousOuter(float value)
	{
		layoutElement.radiusOuterAO = value;
	}

	private Color GetColor()
	{
		return layoutElement.color;
	}

	private void SetColor(Color value)
	{
		layoutElement.color = value;
	}

	private float GetColorAlphaFactor()
	{
		return layoutElement.alphaFactor;
	}

	private void SetColorAlphaFactor(float value)
	{
		layoutElement.alphaFactor = value;
	}
}
