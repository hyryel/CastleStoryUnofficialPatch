using System;
using UnityEngine;

[Serializable]
public class LightComp : Comp
{
	public static string name = "Light";

	public LightComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public LightComp(Comp otherComp)
		: base(otherComp)
	{
		InitProps();
	}

	public void InitProps()
	{
		props = new Prop[2]
		{
			new FloatProp("[Float] Intensity", GetIntensity, SetIntensity),
			new ColorProp("[Color] Color", GetColor, SetColor)
		};
		propsIdx = ((propsIdx < props.Length) ? propsIdx : 0);
	}

	private float GetIntensity()
	{
		return (component as Light).intensity;
	}

	private void SetIntensity(float value)
	{
		(component as Light).intensity = ((!(value < 0.05f)) ? value : 0f);
	}

	private Color GetColor()
	{
		return (component as Light).color;
	}

	private void SetColor(Color value)
	{
		(component as Light).color = value;
	}
}
