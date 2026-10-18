using UnityEngine;

namespace Brix.Game.Components;

public class Spin : MonoBehaviour
{
	[SerializeField]
	private float rotationPerSecond;

	private void Update()
	{
		base.transform.Rotate(base.transform.up, rotationPerSecond * 360f * Time.deltaTime);
	}
}
