using UnityEngine;

public class GA_ExampleController : MonoBehaviour
{
	public float Speed = 1f;

	public float MaxVelocityChange = 10f;

	private void Update()
	{
		Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
		vector *= Speed;
		Vector3 velocity = GetComponent<Rigidbody>().velocity;
		Vector3 force = vector - velocity;
		force.x = Mathf.Clamp(force.x, 0f - MaxVelocityChange, MaxVelocityChange);
		force.y = 0f;
		force.z = 0f;
		GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);
	}
}
