using System;
using UnityEngine;

[Serializable]
public class Comp
{
	public Component component;

	public Prop[] props;

	public int propsIdx;

	public PropertyBlend propertyBlend;

	public ShaderPropertyInfo[] shaderPropertyInfo;

	public Comp()
	{
	}

	public Comp(Component component)
	{
		this.component = component;
	}

	public Comp(Comp otherComp)
	{
		component = otherComp.component;
		props = otherComp.props;
		propsIdx = otherComp.propsIdx;
		propertyBlend = otherComp.propertyBlend;
		shaderPropertyInfo = otherComp.shaderPropertyInfo;
	}
}
