using UnityEngine;

namespace Brix.Skybox;

public class FogColor : MonoBehaviour
{
	public Color color;

	private void Update()
	{
		RenderSettings.fogColor = color;
	}
}
