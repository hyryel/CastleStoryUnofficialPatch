using System;
using UnityEngine;

[Serializable]
public class RadialLayoutGroupComp : Comp
{
	public static string name = "Radial Layout Group";

	private RadialLayoutGroup layoutGroup => component as RadialLayoutGroup;

	public RadialLayoutGroupComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public RadialLayoutGroupComp(Comp otherComp)
		: base(otherComp)
	{
		InitProps();
	}

	public void InitProps()
	{
		props = new Prop[2]
		{
			new FloatProp("[Float] Base Angle Offset AO", GetBaseAngleOffset, SetBaseAngleOffset),
			new FloatProp("[Float] Base Size Factor AO", GetBaseSizeFactor, SetBaseSizeFactor)
		};
		propsIdx = ((propsIdx < props.Length) ? propsIdx : 0);
	}

	private float GetBaseAngleOffset()
	{
		return layoutGroup.baseAngleOffsetAO;
	}

	private void SetBaseAngleOffset(float value)
	{
		layoutGroup.baseAngleOffsetAO = value;
	}

	private float GetBaseSizeFactor()
	{
		return layoutGroup.baseSizeFactorAO;
	}

	private void SetBaseSizeFactor(float value)
	{
		layoutGroup.baseSizeFactorAO = value;
	}
}
