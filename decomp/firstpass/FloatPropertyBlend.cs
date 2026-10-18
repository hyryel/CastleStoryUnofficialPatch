using UnityEngine;

public class FloatPropertyBlend : PropertyBlend
{
	public new static string outputTypeDesc = "Float";

	public new static string interpDesc = "Range + Curve";

	public float min;

	public float max;

	public AnimationCurve curve;

	[SerializeField]
	private AnimationCurve[] m_Curves;

	public AnimationCurve[] curves
	{
		get
		{
			return m_Curves;
		}
		set
		{
			m_Curves = value;
		}
	}

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
		min = 0f;
		max = 0f;
		curve = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));
		m_Curves = new AnimationCurve[0];
		return this;
	}

	public float Apply(float t)
	{
		float result = 0f;
		int num = m_Curves.Length;
		if (num == 0)
		{
			return result;
		}
		int idx = PropertyBlend.GetIdx(num, t, base.ranges);
		if (idx < 0)
		{
			return result;
		}
		if (idx >= num)
		{
			return result;
		}
		float num2 = base.ranges[idx];
		float num3 = base.ranges[idx + 1];
		float num4 = num2;
		float num5 = num3 - num2;
		AnimationCurve animationCurve = m_Curves[idx];
		if (animationCurve == null)
		{
			return result;
		}
		float time = (t - num4) / num5;
		float num6 = animationCurve.Evaluate(time);
		outPartialTraverse = time;
		outIdx = idx;
		return num6 * (max - min) + min;
	}
}
