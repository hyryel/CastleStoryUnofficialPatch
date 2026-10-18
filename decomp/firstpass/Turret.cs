using Brix.Game.AI;
using UnityEngine;

public class Turret : MonoBehaviour
{
	public ProjectileWeapon Gun;

	private bool Fire;

	public Vector3 Target;

	private void Start()
	{
		Gun = GetComponent<ProjectileWeapon>();
	}

	private void Update()
	{
	}

	private void FixedUpdate()
	{
		if (Fire)
		{
			Gun.transform.LookAt(Target);
			Fire = false;
		}
	}
}
