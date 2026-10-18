using Brix.External.Factories;
using Brix.Game.AI;
using UnityEngine;

namespace Brix.Input;

public class ArrowPicker : PickerComponent
{
	private float force = 400f;

	private Factory.AssetKey ammoAssetKey = Test.Arrow;

	private GameObject attacker;

	private void Start()
	{
		attacker = GameObject.FindWithTag("MainCamera");
	}

	public override void OnFirstActionAny(bool _whatever, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1")
		{
			Vector3 hitVector = _voxelRayHit.HitVector;
			Location target = Location.Of(hitVector);
			Debug.Log("Arrow was fired from location " + attacker.transform.position);
			KinematicProjectile.FireAt(ammoAssetKey, attacker, attacker.transform.position, attacker.transform.position, target, hitVector, force, 0f, 50);
		}
	}
}
