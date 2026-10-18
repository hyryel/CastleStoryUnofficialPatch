using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MeshRendererComp : Comp
{
	public static string name = "Mesh Renderer";

	private MeshRenderer meshRenderer => component as MeshRenderer;

	public MeshRendererComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public MeshRendererComp(Comp otherComp)
		: base(otherComp)
	{
		InitProps();
	}

	public void InitProps()
	{
		props = new Prop[1]
		{
			new ColorProp("[Color] Main Color", GetColor, SetColor)
		};
		if (base.shaderPropertyInfo == null || base.shaderPropertyInfo.Length == 0)
		{
			base.shaderPropertyInfo = (from i in ShaderPropertyInfoGenerator.Generate(component as MeshRenderer).ToList()
				where ShaderPropertyInfoGenerator.acceptedTypeNames.ToList().Contains(i.typeInfo.ToUpper())
				select i).ToArray();
		}
		List<Prop> list = new List<Prop>();
		ShaderPropertyInfo[] array = base.shaderPropertyInfo;
		foreach (ShaderPropertyInfo shaderPropertyInfo in array)
		{
			switch (shaderPropertyInfo.typeInfo.ToUpper())
			{
			case "COLOR":
				list.Add(new ColorProp(shaderPropertyInfo.nameInfo + " / [" + shaderPropertyInfo.typeInfo + "] " + shaderPropertyInfo.descInfo, GetColor, SetColor));
				break;
			case "RANGE":
				list.Add(new FloatProp(shaderPropertyInfo.nameInfo + " / [" + shaderPropertyInfo.typeInfo + "] " + shaderPropertyInfo.descInfo + " (" + shaderPropertyInfo.rangeInfo.y + ", " + shaderPropertyInfo.rangeInfo.z + ")", GetFloat, SetFloat));
				break;
			case "FLOAT":
				list.Add(new FloatProp(shaderPropertyInfo.nameInfo + " / [" + shaderPropertyInfo.typeInfo + "] " + shaderPropertyInfo.descInfo, GetFloat, SetFloat));
				break;
			case "VECTOR":
				list.Add(new QuaternionProp(shaderPropertyInfo.nameInfo + " / [" + shaderPropertyInfo.typeInfo + "] " + shaderPropertyInfo.descInfo, GetVectorAsQuat, SetVectorAsQuat));
				break;
			}
		}
		props = list.ToArray();
		propsIdx = ((propsIdx < props.Length) ? propsIdx : 0);
	}

	public void InitShaderInfos()
	{
	}

	private Color GetColor()
	{
		InitShaderInfos();
		ShaderPropertyInfo shaderPropertyInfo = base.shaderPropertyInfo[propsIdx];
		return ShaderPropertyInfo.GetMaterial(meshRenderer).GetColor(shaderPropertyInfo.nameInfo);
	}

	private void SetColor(Color value)
	{
		InitShaderInfos();
		ShaderPropertyInfo shaderPropertyInfo = base.shaderPropertyInfo[propsIdx];
		ShaderPropertyInfo.GetMaterial(meshRenderer).SetColor(shaderPropertyInfo.nameInfo, value);
	}

	private float GetFloat()
	{
		InitShaderInfos();
		ShaderPropertyInfo shaderPropertyInfo = base.shaderPropertyInfo[propsIdx];
		return ShaderPropertyInfo.GetMaterial(meshRenderer).GetFloat(shaderPropertyInfo.nameInfo);
	}

	private void SetFloat(float value)
	{
		InitShaderInfos();
		ShaderPropertyInfo shaderPropertyInfo = base.shaderPropertyInfo[propsIdx];
		ShaderPropertyInfo.GetMaterial(meshRenderer).SetFloat(shaderPropertyInfo.nameInfo, value);
	}

	private Quaternion GetVectorAsQuat()
	{
		InitShaderInfos();
		ShaderPropertyInfo shaderPropertyInfo = base.shaderPropertyInfo[propsIdx];
		Vector4 vector = ShaderPropertyInfo.GetMaterial(meshRenderer).GetVector(shaderPropertyInfo.nameInfo);
		return new Quaternion(vector.x, vector.y, vector.z, vector.w);
	}

	private void SetVectorAsQuat(Quaternion value)
	{
		InitShaderInfos();
		ShaderPropertyInfo shaderPropertyInfo = base.shaderPropertyInfo[propsIdx];
		Vector4 value2 = new Vector4(value.x, value.y, value.z, value.w);
		ShaderPropertyInfo.GetMaterial(meshRenderer).SetVector(shaderPropertyInfo.nameInfo, value2);
	}
}
