using UnityEngine;

namespace Brix.Legacy;

public class TextureScrolling : MonoBehaviour
{
	private float scrollSpeed = 0.25f;

	private void FixedUpdate()
	{
		float num = Time.time * scrollSpeed;
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0f - num, 0f - num));
		GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2(num, num));
	}
}
