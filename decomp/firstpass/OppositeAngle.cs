using UnityEngine;

public class OppositeAngle : MonoBehaviour
{
	public RectTransform opposite;

	public RectTransform myRect;

	private void Start()
	{
	}

	private void Update()
	{
		myRect.rotation = Quaternion.Euler(myRect.rotation.x, myRect.rotation.y, 0f - opposite.rotation.z);
	}
}
