using UnityEngine;

public class Vector3PropertyBlend : PropertyBlend
{
	public enum interpolationType
	{
		LERP,
		SLERP
	}

	public new static string outputTypeDesc = "Vector3";

	public new static string interpDesc = "Range + Curve";

	public Vector3 min;

	public Vector3 max;

	public AnimationCurve curve;

	public interpolationType interpolation;

	public override string GetOutputTypeDesc()
	{
		return outputTypeDesc;
	}

	public override string GetInterpDesc()
	{
		return interpDesc;
	}

	public override PropertyBlend Init()
	{
		min = default(Vector3);
		max = default(Vector3);
		interpolation = interpolationType.LERP;
		curve = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));
		return this;
	}

	public Vector3 Apply(float t)
	{
		Vector3 zero = Vector3.zero;
		if (curve == null)
		{
			return zero;
		}
		float t2 = curve.Evaluate(t);
		zero = ((interpolation != interpolationType.LERP) ? zero : Vector3.Lerp(min, max, t2));
		return (interpolation != interpolationType.SLERP) ? zero : Vector3.Slerp(min, max, t2);
	}
}
