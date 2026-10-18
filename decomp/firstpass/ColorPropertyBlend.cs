using UnityEngine;

public class ColorPropertyBlend : PropertyBlend
{
	public enum interpType
	{
		GRADIENT,
		TEXTURE
	}

	public new static string outputTypeDesc = "Color";

	public new static string interpDesc = "Gradient";

	public interpType interp;

	public Texture2D gradientTexture;

	public Gradient gradient0;

	public Gradient gradient1;

	public Gradient gradient2;

	public Gradient gradient3;

	public Gradient gradient4;

	public Gradient gradient5;

	public Gradient gradient6;

	[SerializeField]
	private Gradient[] m_Gradients;

	public int gradientCount;

	public Gradient[] gradients
	{
		get
		{
			return m_Gradients;
		}
		set
		{
			m_Gradients = value;
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
		gradient0 = new Gradient();
		gradient1 = new Gradient();
		gradient2 = new Gradient();
		gradient3 = new Gradient();
		gradient4 = new Gradient();
		gradient5 = new Gradient();
		gradient6 = new Gradient();
		gradients = new Gradient[0];
		gradientCount = 1;
		return this;
	}

	public Color Apply(float t)
	{
		Color result = Color.black;
		if (interp == interpType.GRADIENT)
		{
			int num = gradients.Length;
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
			Gradient gradient = gradients[idx];
			if (gradient == null)
			{
				return result;
			}
			float time = (t - num4) / num5;
			Color color = gradient.Evaluate(time);
			outPartialTraverse = time;
			outIdx = idx;
			result = color;
		}
		else if (interp == interpType.TEXTURE)
		{
			if (gradientTexture == null)
			{
				return result;
			}
			float num6 = (float)gradientTexture.width * t;
			float num7 = Mathf.Round(num6);
			Color color2 = gradientTexture.GetPixel((int)num7, 0);
			if (num7 != num6)
			{
				float num8 = Mathf.Floor(num6);
				float num9 = Mathf.Ceil(num6);
				Color pixel = gradientTexture.GetPixel((int)num8, 0);
				Color pixel2 = gradientTexture.GetPixel((int)num9, 0);
				float t2 = num6 - num8;
				color2 = new Color(Mathf.Lerp(pixel.r, pixel2.r, t2), Mathf.Lerp(pixel.g, pixel2.g, t2), Mathf.Lerp(pixel.b, pixel2.b, t2), Mathf.Lerp(pixel.a, pixel2.a, t2));
			}
			result = color2;
		}
		return result;
	}
}
