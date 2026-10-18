using UnityEngine;

public class GA_ExampleMoveWithPlayer : MonoBehaviour
{
	public GameObject Player;

	private void Update()
	{
		if (Player != null)
		{
			base.transform.position = Player.transform.position + new Vector3(0f, 0.5f, 0f);
		}
	}
}
