using UnityEngine;

namespace Brix.Game.Components;

public class LanterneScript : MonoBehaviour
{
	public GameObject pointLight;

	public float range;

	public float intensity;

	public Gradient LanterneGradient;

	private Light _light;

	private Renderer _renderer;

	private void Awake()
	{
		_light = ((!(pointLight != null)) ? null : pointLight.GetComponent<Light>());
		_renderer = GetComponent<Renderer>();
	}

	private void Update()
	{
		float heure = Journee.instance.Heure;
		float time = heure / 24f;
		Color color = LanterneGradient.Evaluate(time);
		if (color.a > 0f)
		{
			if (!_light.enabled)
			{
				_light.enabled = true;
			}
			_light.intensity = color.a * intensity;
			_light.range = color.a * range;
			_light.color = color;
			if (_renderer != null)
			{
				_renderer.material.SetFloat("_Gain", color.a);
			}
		}
		else if (_light.enabled)
		{
			_light.enabled = false;
		}
	}
}
