using System;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.UI;

public class SphereHighlight : MonoBehaviour
{
	[Serializable]
	public class Spawner
	{
		[DisplayRules(DisplayFlags.ReadOnly)]
		[SerializeField]
		private SphereHighlight instance;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private Transform parent;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private string name;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private Material brushMaterial;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private Vector3 _scale;

		public Vector3 position
		{
			set
			{
				if ((bool)instance)
				{
					instance.position = value;
				}
			}
		}

		public float Radius
		{
			get
			{
				return (!instance) ? 0f : instance.Radius;
			}
			set
			{
				if ((bool)instance)
				{
					instance.Radius = value;
				}
			}
		}

		public Color BrushColor
		{
			get
			{
				return (!instance) ? Color.black : instance.BrushColor;
			}
			set
			{
				if ((bool)instance)
				{
					instance.BrushColor = value;
				}
			}
		}

		public bool active
		{
			get
			{
				return (bool)instance && instance.active;
			}
			set
			{
				if ((bool)instance)
				{
					instance.active = value;
				}
			}
		}

		public Vector3 scale
		{
			get
			{
				return (!instance) ? Vector3.zero : instance.scale;
			}
			set
			{
				if ((bool)instance)
				{
					instance.scale = value;
				}
			}
		}

		public void Spawn()
		{
			ObjectPoolSingleton.RequestComponent<SphereHighlight>(out instance, VolumesUI.SphereHighlight);
			instance.transform.SetParent(parent);
			instance.transform.localPosition = Vector3.zero;
			instance.name = name;
			instance.scale = _scale;
			if (brushMaterial != null)
			{
				instance.BrushMaterial = brushMaterial;
			}
		}

		public void Release()
		{
			if ((bool)instance && !instance.gameObject.IsNullOrReleased())
			{
				ObjectPoolSingleton.Release(instance.gameObject);
			}
		}
	}

	public Color DefaultBrushColor;

	public Color RemoverColor = new Color(1f, 0.3f, 0.1f, 0.1f);

	[SerializeField]
	private Renderer _renderer;

	public Vector3 position
	{
		set
		{
			base.transform.position = value;
		}
	}

	public float Radius
	{
		get
		{
			return base.transform.localScale.x / 2f;
		}
		set
		{
			Vector3 localScale = new Vector3(value * 2f, value * 2f, value * 2f);
			base.transform.localScale = localScale;
		}
	}

	public Material BrushMaterial
	{
		get
		{
			return _renderer.sharedMaterial;
		}
		set
		{
			_renderer.sharedMaterial = value;
		}
	}

	public Color BrushColor
	{
		get
		{
			return _renderer.material.GetColor("_Color");
		}
		set
		{
			_renderer.material.SetColor("_Color", value);
		}
	}

	public bool active
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

	public Vector3 scale
	{
		get
		{
			return base.transform.localScale;
		}
		set
		{
			base.transform.localScale = value;
		}
	}

	private void Start()
	{
		active = false;
		_renderer = GetComponent<Renderer>();
		DefaultBrushColor = BrushColor;
	}
}
