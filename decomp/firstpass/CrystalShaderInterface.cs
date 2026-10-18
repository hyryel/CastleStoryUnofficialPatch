using UnityEngine;

public class CrystalShaderInterface : MonoBehaviour
{
	public Material material;

	[SerializeField]
	private bool OverrideColor;

	[SerializeField]
	private Color ColorDefault;

	[SerializeField]
	private bool OverrideSecondaryColor;

	[SerializeField]
	private Color SecondaryColorDefault;

	[SerializeField]
	private bool OverrideColor2;

	[SerializeField]
	private Color Color2Default;

	[SerializeField]
	private bool OverrideSecondaryColor2;

	[SerializeField]
	private Color SecondaryColor2Default;

	[SerializeField]
	private bool OverrideLinear;

	[SerializeField]
	private bool LinearDefault;

	[SerializeField]
	private bool OverrideProgress;

	[SerializeField]
	private float ProgressDefault;

	[SerializeField]
	private bool OverrideTransitionPoint;

	[SerializeField]
	private Vector3 TransitionPointDefault;

	[SerializeField]
	private bool OverrideLocalTransition;

	[SerializeField]
	private bool LocalTransitionDefault;

	[SerializeField]
	private bool OverrideTransitionFalloff;

	[SerializeField]
	private float TransitionFalloffDefault;

	[SerializeField]
	private bool OverrideGloss;

	[SerializeField]
	private float GlossDefault;

	[SerializeField]
	private bool OverrideGlow;

	[SerializeField]
	private bool GlowDefault;

	[SerializeField]
	private bool OverrideGlowPoint;

	[SerializeField]
	private Vector3 GlowPointDefault;

	[SerializeField]
	private bool OverrideLocalGlow;

	[SerializeField]
	private bool LocalGlowDefault;

	[SerializeField]
	private bool OverrideGlowPower;

	[SerializeField]
	private float GlowPowerDefault;

	[SerializeField]
	private bool OverrideGlowFalloff;

	[SerializeField]
	private float GlowFalloffDefault;

	public Color Color
	{
		get
		{
			return material.GetColor("_Color");
		}
		set
		{
			material.SetColor("_Color", value);
		}
	}

	public Color SecondaryColor
	{
		get
		{
			return material.GetColor("_SecondaryColor");
		}
		set
		{
			material.SetColor("_SecondaryColor", value);
		}
	}

	public Color Color2
	{
		get
		{
			return material.GetColor("_Color2");
		}
		set
		{
			material.SetColor("_Color2", value);
		}
	}

	public Color SecondaryColor2
	{
		get
		{
			return material.GetColor("_Secondary2");
		}
		set
		{
			material.SetColor("_Secondary2", value);
		}
	}

	public bool Linear
	{
		get
		{
			return material.GetFloat("_Linear") == 0f;
		}
		set
		{
			material.SetFloat("_Linear", (!value) ? 0f : 1f);
		}
	}

	public float Progress
	{
		get
		{
			return material.GetFloat("_Progress");
		}
		set
		{
			material.SetFloat("_Progress", value);
		}
	}

	public Vector3 TransitionPoint
	{
		get
		{
			return material.GetVector("_TransitionPoint");
		}
		set
		{
			material.SetVector("_TransitionPoint", value);
		}
	}

	public bool LocalTransition
	{
		get
		{
			return material.GetFloat("_LocalTransition") == 0f;
		}
		set
		{
			material.SetFloat("_LocalTransition", (!value) ? 0f : 1f);
		}
	}

	public float TransitionFalloff
	{
		get
		{
			return material.GetFloat("_TransitionFalloff");
		}
		set
		{
			material.SetFloat("_TransitionFalloff", value);
		}
	}

	public float Gloss
	{
		get
		{
			return material.GetFloat("_Gloss");
		}
		set
		{
			material.SetFloat("_Gloss", Mathf.Clamp(value, 0f, 1f));
		}
	}

	public bool Glow
	{
		get
		{
			return material.GetFloat("_Glow") == 0f;
		}
		set
		{
			material.SetFloat("_Glow", (!value) ? 0f : 1f);
		}
	}

	public Vector3 GlowPoint
	{
		get
		{
			return material.GetVector("_GlowPoint");
		}
		set
		{
			material.SetVector("_GlowPoint", value);
		}
	}

	public bool LocalGlow
	{
		get
		{
			return material.GetFloat("_LocalGlow") == 0f;
		}
		set
		{
			material.SetFloat("_LocalGlow", (!value) ? 0f : 1f);
		}
	}

	public float EmissivePower
	{
		get
		{
			return material.GetFloat("_EmissivePower");
		}
		set
		{
			material.SetFloat("_EmissivePower", Mathf.Clamp(value, 0f, 1f));
		}
	}

	public float GlowFalloff
	{
		get
		{
			return material.GetFloat("_GlowFalloff");
		}
		set
		{
			material.SetFloat("_GlowFalloff", value);
		}
	}

	public Color GlowColor
	{
		get
		{
			return material.GetColor("_GlowColor");
		}
		set
		{
			material.SetColor("_GlowColor", value);
		}
	}

	public Color GlowColor2
	{
		get
		{
			return material.GetColor("_GlowColor2");
		}
		set
		{
			material.SetColor("_GlowColor2", value);
		}
	}

	public Color LightColor
	{
		get
		{
			return material.GetColor("_LightColor");
		}
		set
		{
			material.SetColor("_LightColor", value);
		}
	}

	public Color LightColor2
	{
		get
		{
			return material.GetColor("_LightColor2");
		}
		set
		{
			material.SetColor("_LightColor2", value);
		}
	}

	private void Awake()
	{
		ApplyDefault();
	}

	private void ApplyDefault()
	{
		if (OverrideColor)
		{
			Color = ColorDefault;
		}
		if (OverrideSecondaryColor)
		{
			SecondaryColor = SecondaryColorDefault;
		}
		if (OverrideColor2)
		{
			Color2 = Color2Default;
		}
		if (OverrideSecondaryColor2)
		{
			SecondaryColor2 = SecondaryColor2Default;
		}
		if (OverrideLinear)
		{
			Linear = LinearDefault;
		}
		if (OverrideProgress)
		{
			Progress = ProgressDefault;
		}
		if (OverrideTransitionPoint)
		{
			TransitionPoint = TransitionPointDefault;
		}
		if (OverrideLocalTransition)
		{
			LocalTransition = LocalTransitionDefault;
		}
		if (OverrideTransitionFalloff)
		{
			TransitionFalloff = TransitionFalloffDefault;
		}
		if (OverrideGloss)
		{
			Gloss = GlossDefault;
		}
		if (OverrideGlow)
		{
			Glow = GlowDefault;
		}
		if (OverrideGlowPoint)
		{
			GlowPoint = GlowPointDefault;
		}
		if (OverrideLocalGlow)
		{
			LocalGlow = LocalGlowDefault;
		}
		if (OverrideGlowPower)
		{
			EmissivePower = GlowPowerDefault;
		}
		if (OverrideGlowFalloff)
		{
			GlowFalloff = GlowFalloffDefault;
		}
	}
}
