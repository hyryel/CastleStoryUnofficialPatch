using UnityEngine;

namespace Brix.Skybox;

public class AmbientLight : MonoBehaviour
{
	public Color color;

	private void Update()
	{
		RenderSettings.ambientLight = color;
	}
}
