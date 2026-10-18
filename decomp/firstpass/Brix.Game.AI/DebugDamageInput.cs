using Brix.Game.AI.Damage;
using UnityEngine;

namespace Brix.Game.AI;

public class DebugDamageInput : MonoBehaviour
{
	private enum EDebugInputPhase
	{
		Inactive,
		OriginPhase,
		DirectionPhase,
		ForcePhase
	}

	public bool EnableDebugDamageInput;

	private EDebugInputPhase DebugInputPhase;

	private float Height = 1f;

	private Vector3 DamageImpactPoint = Vector3.zero;

	private Vector3 DamageSlingshot = Vector3.zero;

	public int DamageValue = 1;

	public DamageType DamageType;

	public DamageReaction Reaction = DamageReaction.Stun;

	public float Lift = 1f;

	private bool Fire;

	private DamageReceiver Receiver;

	public float ForceMultiplier = 1f;

	private void Start()
	{
		Receiver = GetComponent<DamageReceiver>();
	}

	private void Update()
	{
		if (EnableDebugDamageInput)
		{
			DebugInput();
		}
	}

	private void DebugInput()
	{
	}

	private Vector3 GetCenter()
	{
		return base.transform.position + Vector3.up * Height;
	}

	private bool RaycastOnCenterPlane(out Vector3 picked)
	{
		Plane plane = new Plane(Vector3.up, GetCenter());
		Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
		if (plane.Raycast(ray, out var enter))
		{
			picked = ray.GetPoint(enter);
			return true;
		}
		picked = Vector3.zero;
		return false;
	}

	private void FixedUpdate()
	{
		if (Fire)
		{
			Receiver.ReceiveAttack(DamageValue, DamageType, Reaction, DamageImpactPoint, (DamageImpactPoint - DamageSlingshot) * ForceMultiplier, Lift);
			DamageImpactPoint = Vector3.zero;
			DamageSlingshot = Vector3.zero;
			Height = 1f;
			Fire = false;
		}
	}

	private void OnDrawGizmos()
	{
		if (DamageImpactPoint != Vector3.zero)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(DamageImpactPoint, 0.2f);
		}
		if (DamageSlingshot != Vector3.zero)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(DamageSlingshot, 0.2f);
			Gizmos.DrawLine(DamageImpactPoint, DamageSlingshot);
		}
	}
}
