using UnityEngine;

public class Vector2RangeAttribute : PropertyAttribute
{
	public enum GuiType
	{
		Slider,
		SliderMinMax
	}

	public GuiType guiType;

	public float min;

	public float max;

	public Vector2RangeAttribute(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public Vector2RangeAttribute(float min, float max, GuiType guiType)
	{
		this.min = min;
		this.max = max;
		this.guiType = guiType;
	}
}
