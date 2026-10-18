using UnityEngine;

namespace Brix.Game.Components;

public class TogglableGlow : MonoBehaviour
{
	public GameObject pointLight;

	public float range;

	public float intensity;

	private Light _light;

	private Renderer _renderer;

	private void Awake()
	{
		_light = ((!(pointLight != null)) ? null : pointLight.GetComponent<Light>());
		_renderer = GetComponent<Renderer>();
		ToggleGlow(value: false);
	}

	private void OnEnable()
	{
		ToggleGlow(value: true);
	}

	private void OnDisable()
	{
		ToggleGlow(value: false);
	}

	public void ToggleGlow(bool value)
	{
		_light.enabled = value;
		_light.intensity = intensity;
		_light.range = range;
		if (_renderer != null)
		{
			_renderer.material.SetFloat("_Gain", (!value) ? 0f : intensity);
		}
	}
}
