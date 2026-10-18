using System;
using Brix.Engine;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Rules;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Brix.Utils.Locomotion;
using UnityEngine;

namespace Brix.Game.AI;

public class ProjectileAiming
{
	private Transform _parent;

	private Location _target;

	private ProjectileWeapon _weapon;

	private float _powerMultiplier;

	private bool _avoidAllies = true;

	private Ray testRay = new Ray(Vector3.zero, Vector3.down);

	public Vector3 defaultStartPoint => transform.position;

	private Transform transform => _parent;

	private GameObject gameObject => _parent.gameObject;

	public Location Target => _target;

	public ProjectileWeapon Weapon => _weapon;

	public float PowerMultiplier => _powerMultiplier;

	public float Power => _weapon.Power * _powerMultiplier;

	public ProjectileAiming(Transform parent)
	{
		_parent = parent;
	}

	public void SetupFire(Location target, ProjectileWeapon weapon, float powerMultiplier, bool avoidAllies)
	{
		_weapon = weapon;
		_powerMultiplier = powerMultiplier;
		_avoidAllies = avoidAllies;
		_target = target;
	}

	public static Transform GetSignificantParent(Collider collider)
	{
		Labor componentInParent = collider.GetComponentInParent<Labor>();
		if (componentInParent != null)
		{
			return componentInParent.transform;
		}
		return collider.transform;
	}

	private Vector3 GetProjectileAimingPosition(Vector3 projectileAimingPosition, Vector3 positionAdjustment)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Target.Position - positionAdjustment - transform.position), Vector3.one);
		Vector4 vector = transform.InverseTransformPoint(projectileAimingPosition);
		vector.w = 1f;
		Vector3 vector2 = matrix4x * vector;
		return vector2 + positionAdjustment;
	}

	public bool CanReachWithProjectile(Vector3 projectileAimingPosition, LayerMask layermask, float maxDistance = float.PositiveInfinity, bool roundPosition = false)
	{
		if (!_weapon.IsInRangeToShoot(Target.Position))
		{
			return false;
		}
		Vector3 positionAdjustment = Vector3.zero;
		if (roundPosition)
		{
			WalkablePathNode walkablePathNode = PathNodeRegistry.At(XYZ.FromVector3(transform.position));
			Vector3 vector = transform.position.Rounded();
			if (walkablePathNode != null)
			{
				vector.y = walkablePathNode.GetSurfaceHeight(0f, 0f);
			}
			positionAdjustment = vector - transform.position;
		}
		Vector3 projectileAimingPosition2 = GetProjectileAimingPosition(projectileAimingPosition, positionAdjustment);
		bool flag = IsProjectileObstructed(projectileAimingPosition2, layermask);
		return !flag;
	}

	public bool IsProjectileObstructed(Vector3 projectileOrigin, LayerMask layerMask)
	{
		if (!ValidOrigin(projectileOrigin, layerMask))
		{
			return true;
		}
		GameObject gameObject = ((!(_target is ObjectLocation)) ? null : ((ObjectLocation)_target).Object);
		if (gameObject == null)
		{
			return false;
		}
		Vector3 delta = GetAnticipatedTargetPosition() - projectileOrigin;
		if (!IsInRange(delta))
		{
			return true;
		}
		ProjectilePhysics.CollisionTrajectory collisionTrajectory = SimulateArrow(projectileOrigin, delta, layerMask);
		if (collisionTrajectory.IsNull)
		{
			return false;
		}
		GameObject gameObject2 = SignificantParent.Of(collisionTrajectory.hit);
		if (gameObject2 == null)
		{
			return false;
		}
		if (!Affiliation.IsEnemy(this.gameObject, gameObject))
		{
			return false;
		}
		if (gameObject2 == gameObject)
		{
			return false;
		}
		if (Affiliation.IsEnemy(this.gameObject, gameObject2))
		{
			return false;
		}
		if (Vector3.Distance(projectileOrigin, gameObject.transform.position) < Vector3.Distance(projectileOrigin, collisionTrajectory.hit.point) && (!_avoidAllies || !Affiliation.IsAllied(this.gameObject, gameObject2)))
		{
			return false;
		}
		return true;
	}

	private bool ValidOrigin(Vector3 origin, LayerMask layerMask)
	{
		if (_avoidAllies && ProjectileWillSpawnInsideAlly(origin, layerMask))
		{
			return false;
		}
		if (ProjectileWillSpawnInsideTerrain(origin))
		{
			return false;
		}
		return true;
	}

	private bool IsInRange(Vector3 delta)
	{
		if (IsArcheryTargetFarEnough())
		{
			return true;
		}
		if (ProjectilePhysics.IsWithinReach(Power, delta))
		{
			return true;
		}
		return false;
	}

	private ProjectilePhysics.CollisionTrajectory SimulateArrow(Vector3 origin, Vector3 delta, LayerMask layerMask)
	{
		Vector3 initialVelocity = ProjectilePhysics.ComputeFiringVelocity(Power, delta);
		Func<RaycastHit, GameObject, bool> filterFunc = null;
		if (CombatRules.ArrowShootThroughAllies)
		{
			filterFunc = delegate(RaycastHit hit, GameObject gameObject)
			{
				GameObject gameObject2 = SignificantParent.Of(hit);
				bool flag = Affiliation.IsAllied(gameObject, gameObject2) && gameObject2.GetComponent<Labor>() != null;
				return !flag;
			};
		}
		return ProjectilePhysics.ComputeExpectedCollision(origin, initialVelocity, 0.2f, delta.magnitude * 2f, 5f, layerMask, filterFunc, gameObject);
	}

	public Vector3 GetAnticipatedTargetPosition()
	{
		return _target.RangeHitPosition;
	}

	public bool IsArcheryTargetFarEnough()
	{
		if (_weapon == null)
		{
			return false;
		}
		return _weapon.IsFarEnough(_target.Position);
	}

	public bool ProjectileWillSpawnInsideAlly(Vector3 projectileOrigin, LayerMask layerMask)
	{
		Labor labor = null;
		testRay.origin = projectileOrigin + Vector3.up;
		if (Physics.Raycast(testRay, out var hitInfo, 1f, layerMask, QueryTriggerInteraction.Ignore))
		{
			labor = hitInfo.collider.GetComponentInParent<Labor>();
		}
		return labor != null && !labor.gameObject.transform.IsChildOf(transform) && Affiliation.IsAllied(labor.gameObject, gameObject);
	}

	public bool ProjectileWillSpawnInsideTerrain(Vector3 projectileOrigin, float heightClearance = 0.05f)
	{
		XYZ position = XYZ.FromVector3(projectileOrigin);
		if (Voxel.IsEmpty(position))
		{
			return false;
		}
		if (!Voxel.IsTerrain(position))
		{
			return false;
		}
		if (!Voxel.IsCrust(position))
		{
			return true;
		}
		int niveaux = Voxel.GetNiveaux(position);
		float num = Voxel.ComputeSurfaceHeightFromNiveaux(niveaux, projectileOrigin.x - (float)position.x, projectileOrigin.z - (float)position.z);
		return projectileOrigin.y < num + heightClearance;
	}
}
