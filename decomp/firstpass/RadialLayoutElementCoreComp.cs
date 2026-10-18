using System;
using UnityEngine;

[Serializable]
public class RadialLayoutElementCoreComp : Comp
{
	public static string name = "Radial Layout Element Core";

	private RadialLayoutElementCore layoutElement => component as RadialLayoutElementCore;

	public RadialLayoutElementCoreComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public RadialLayoutElementCoreComp(Comp otherComp)
		: base(otherComp)
	{
		InitProps();
	}

	public void InitProps()
	{
		props = new Prop[5]
		{
			new FloatProp("[Float] Radius", GetRadius, SetRadius),
			new FloatProp("[Float] Thickness", GetThickness, SetThickness),
			new FloatProp("[Float] Width", GetWidth, SetWidth),
			new ColorProp("[Color] Color", GetColor, SetColor),
			new FloatProp("[Float] Color Alpha Factor", GetColorAlphaFactor, SetColorAlphaFactor)
		};
		propsIdx = ((propsIdx < props.Length) ? propsIdx : 0);
	}

	private float GetRadius()
	{
		return layoutElement.coreRadiusAO;
	}

	private void SetRadius(float value)
	{
		layoutElement.coreRadiusAO = value;
	}

	private float GetThickness()
	{
		return layoutElement.coreThicknessAO;
	}

	private void SetThickness(float value)
	{
		layoutElement.coreThicknessAO = value;
	}

	private float GetWidth()
	{
		return layoutElement.coreWidthAO;
	}

	private void SetWidth(float value)
	{
		layoutElement.coreWidthAO = value;
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
