using UnityEngine;

namespace Brix.Legacy;

public class MaterialToGlobalSettings : MonoBehaviour
{
	private Color Color;

	private bool colorOverride;

	public static MaterialToGlobalSettings instance;

	private void Awake()
	{
		instance = this;
	}

	public void OverrideColor(Color color)
	{
		colorOverride = true;
		Color = color;
	}

	private void Update()
	{
		RenderSettings.ambientLight = ((!colorOverride) ? GetComponent<Renderer>().sharedMaterial.color : Color);
		RenderSettings.fogColor = ((!colorOverride) ? GetComponent<Renderer>().sharedMaterial.color : Color);
	}
}
