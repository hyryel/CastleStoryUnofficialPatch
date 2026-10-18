using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ImageComp : Comp
{
	public static string name = "Image";

	public ImageComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public ImageComp(Comp otherComp)
		: base(otherComp)
	{
		InitProps();
	}

	public void InitProps()
	{
		props = new Prop[1]
		{
			new ColorProp("[Color] Color", GetColor, SetColor)
		};
		propsIdx = ((propsIdx < props.Length) ? propsIdx : 0);
	}

	private Color GetColor()
	{
		return (component as Image).color;
	}

	private void SetColor(Color value)
	{
		(component as Image).color = value;
	}
}
