using System.Collections;
using UnityEngine;

public class GA_ExampleTarget : MonoBehaviour
{
	public float SpinSpeed = 0.5f;

	private bool _hit;

	private void Start()
	{
		GetComponent<Renderer>().material.color = Color.yellow;
	}

	private void Update()
	{
		if (!_hit)
		{
			GetComponent<Rigidbody>().AddTorque(Vector3.forward * SpinSpeed);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.name.Equals("Ball") && !_hit)
		{
			GA_ExampleHighScore.AddScore(10, base.gameObject.name, collision.transform.position);
			StartCoroutine(FlashColor(0.5f));
		}
	}

	private IEnumerator FlashColor(float duration)
	{
		_hit = true;
		GetComponent<Renderer>().material.color = Color.green;
		yield return new WaitForSeconds(duration);
		_hit = false;
		GetComponent<Renderer>().material.color = Color.yellow;
	}
}
