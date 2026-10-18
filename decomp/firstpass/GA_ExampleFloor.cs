using UnityEngine;

public class GA_ExampleFloor : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Renderer>().material.color = Color.red;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.name.Equals("Ball"))
		{
			GA_ExampleHighScore.GameOver(collision.transform.position);
			Object.Destroy(collision.gameObject);
		}
	}
}
