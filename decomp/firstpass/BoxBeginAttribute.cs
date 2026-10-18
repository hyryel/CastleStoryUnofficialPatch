using System;
using UnityEngine;

public class BoxBeginAttribute : Attribute
{
	public string title = string.Empty;

	public GUIStyle style;

	public float opacity = 0.5f;

	public Color contentColor = Color.white;

	public Color bgColor = Color.white;

	public BoxBeginAttribute()
	{
		InitStyle();
	}

	public BoxBeginAttribute(string title)
	{
		this.title = title;
		InitStyle();
	}

	public BoxBeginAttribute(string title, GUIStyle style)
	{
		this.title = title;
		this.style = style;
		InitStyle();
	}

	public BoxBeginAttribute(string title, GUIStyle style, Color contentColor, Color bgColor)
	{
		this.title = title;
		this.style = style;
		this.contentColor = contentColor;
		this.bgColor = bgColor;
		InitStyle();
	}

	private void InitStyle()
	{
		style = new GUIStyle("IN ThumbnailShadow");
		style.padding = new RectOffset(2, 2, 0, 4);
	}
}
