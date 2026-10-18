using UnityEngine;

public class QuaternionPropertyBlend : PropertyBlend
{
	public enum interpolationType
	{
		LERP,
		SLERP
	}

	public new static string outputTypeDesc = "Quaternion";

	public new static string interpDesc = "Range + Curve";

	public Quaternion min;

	public Quaternion max;

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
		min = new Quaternion(Quaternion.identity.x, Quaternion.identity.y, Quaternion.identity.z, Quaternion.identity.w);
		max = new Quaternion(Quaternion.identity.x, Quaternion.identity.y, Quaternion.identity.z, Quaternion.identity.w);
		interpolation = interpolationType.LERP;
		curve = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));
		return this;
	}

	public Quaternion Apply(float t)
	{
		Quaternion identity = Quaternion.identity;
		if (min == Quaternion.identity && max == Quaternion.identity)
		{
			return identity;
		}
		if (min == max)
		{
			return identity;
		}
		if (curve == null)
		{
			return identity;
		}
		float t2 = curve.Evaluate(t);
		identity = ((interpolation != interpolationType.LERP) ? identity : Quaternion.Lerp(min, max, t2));
		return (interpolation != interpolationType.SLERP) ? identity : Quaternion.Slerp(min, max, t2);
	}
}
