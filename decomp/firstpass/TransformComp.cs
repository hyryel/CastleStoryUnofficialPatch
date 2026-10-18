using System;
using UnityEngine;

[Serializable]
public class TransformComp : Comp
{
	public static string name = "Transform";

	public TransformComp(Component component)
		: base(component)
	{
		InitProps();
	}

	public TransformComp(Comp otherComp)
		: base(otherComp)
	{
		InitProps();
	}

	public void InitProps()
	{
		props = new Prop[7]
		{
			new FloatProp("[Float] X", GetX, SetX),
			new FloatProp("[Float] Y", GetY, SetY),
			new FloatProp("[Float] Width", GetWidth, SetWidth),
			new FloatProp("[Float] Height", GetHeight, SetHeight),
			new Vector3Prop("[Vector3] Position (Lcl)", GetPosition, SetPosition),
			new QuaternionProp("[Quaternion] Rotation (Lcl)", GetRotation, SetRotation),
			new Vector3Prop("[Vector3] Scale (Lcl)", GetScale, SetScale)
		};
		propsIdx = ((propsIdx < props.Length) ? propsIdx : 0);
	}

	private float GetX()
	{
		return (component as RectTransform).rect.x;
	}

	private void SetX(float value)
	{
		float width = (component as RectTransform).rect.width;
		(component as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, value, width);
	}

	private float GetY()
	{
		return (component as RectTransform).rect.y;
	}

	private void SetY(float value)
	{
		float height = (component as RectTransform).rect.height;
		(component as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, value, height);
	}

	private float GetWidth()
	{
		return (component as RectTransform).rect.width;
	}

	private void SetWidth(float value)
	{
		(component as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
	}

	private float GetHeight()
	{
		return (component as RectTransform).rect.height;
	}

	private void SetHeight(float value)
	{
		(component as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
	}

	private Vector3 GetPosition()
	{
		return (component as Transform).localPosition;
	}

	private void SetPosition(Vector3 value)
	{
		(component as Transform).localPosition = value;
	}

	private Quaternion GetRotation()
	{
		return (component as Transform).localRotation;
	}

	private void SetRotation(Quaternion value)
	{
		(component as Transform).localRotation = value;
	}

	private Vector3 GetScale()
	{
		return (component as Transform).localScale;
	}

	private void SetScale(Vector3 value)
	{
		(component as Transform).localScale = value;
	}
}
