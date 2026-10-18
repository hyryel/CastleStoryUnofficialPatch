using Brix.Components;
using UnityEngine;

public class CircleHighlight : BrixComponent
{
	private CompCache<Renderer> _renderer;

	private const float CIRCLE_SHADER_SCALE_ADJUSTMENT_TO_PREVENT_SHADOW_CLIP = 1.25f;

	private rotationLocker _rotationLocker;

	private MeshRenderer _meshRenderer;

	public bool Showing
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public float Radius
	{
		get
		{
			return base.transform.localScale.x / 5f;
		}
		set
		{
			base.transform.localScale = Vector3.one * 4f * (value * 1.25f);
		}
	}

	public Material Material
	{
		get
		{
			return _renderer.Get.material;
		}
		set
		{
			_renderer.Get.material = value;
		}
	}

	public Color Color
	{
		get
		{
			return Material.GetColor("_Color");
		}
		set
		{
			Material.SetColor("_Color", value);
		}
	}

	public float RadiusOffset
	{
		get
		{
			return Material.GetFloat("_Radius");
		}
		set
		{
			Material.SetFloat("_Radius", value);
		}
	}

	public float Thickness
	{
		get
		{
			return Material.GetFloat("_Thickness");
		}
		set
		{
			Material.SetFloat("_Thickness", value);
		}
	}

	public float Offset
	{
		get
		{
			return Material.GetFloat("_Offset");
		}
		set
		{
			Material.SetFloat("_Offset", value);
		}
	}

	public float ShadowOpacity
	{
		get
		{
			return Material.GetFloat("_ShadowOpacity");
		}
		set
		{
			Material.SetFloat("_ShadowOpacity", value);
		}
	}

	public float Squareness
	{
		get
		{
			return Material.GetFloat("_Squareness");
		}
		set
		{
			Material.SetFloat("_Squareness", value);
		}
	}

	public float OccludedOpacity
	{
		get
		{
			return Material.GetFloat("_OccludedOpacity");
		}
		set
		{
			Material.SetFloat("_OccludedOpacity", value);
		}
	}

	public float Angle
	{
		get
		{
			return Material.GetFloat("_Angle");
		}
		set
		{
			Material.SetFloat("_Angle", value);
		}
	}

	public float Width
	{
		get
		{
			return Material.GetFloat("_Width");
		}
		set
		{
			Material.SetFloat("_Width", value);
		}
	}

	public float Progress
	{
		get
		{
			return Material.GetFloat("_Progress");
		}
		set
		{
			Material.SetFloat("_Progress", value);
		}
	}

	public Color Color2
	{
		get
		{
			return Material.GetColor("_Color2");
		}
		set
		{
			Material.SetColor("_Color2", value);
		}
	}

	public void InvertColors()
	{
		Color color = Color2;
		Color2 = Color;
		Color = color;
	}

	public override void Initialize()
	{
		base.Initialize();
		_renderer = new CompCache<Renderer>(base.gameObject);
		_rotationLocker = GetComponent<rotationLocker>();
		_meshRenderer = GetComponent<MeshRenderer>();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		if ((bool)_rotationLocker)
		{
			_rotationLocker.enabled = true;
		}
		if ((bool)_meshRenderer)
		{
			_meshRenderer.enabled = true;
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if ((bool)_rotationLocker)
		{
			_rotationLocker.enabled = false;
		}
		if ((bool)_meshRenderer)
		{
			_meshRenderer.enabled = false;
		}
		if (Material != null)
		{
			Object.Destroy(Material);
		}
	}
}
