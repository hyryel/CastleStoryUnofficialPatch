using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TextComp : Comp
{
	public static string name = "Text";

	private Text textComponent => component as Text;

	public TextComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public TextComp(Comp otherComp)
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
		return textComponent.color;
	}

	private void SetColor(Color value)
	{
		textComponent.color = value;
	}
}
