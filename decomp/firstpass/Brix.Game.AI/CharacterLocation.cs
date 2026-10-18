using System;
using Brix.Engine;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components.Volumes;
using Brix.Pathfinding;
using Brix.Utils.Locomotion;
using UnityEngine;

namespace Brix.Game.AI;

public class CharacterLocation : ObjectLocation
{
	public enum RdvType
	{
		Follow,
		Intercept,
		Meet,
		Flock
	}

	public Transform Transform;

	public Labor Character;

	public Locomotion4 Locomotion;

	public RdvType RdvOption;

	private const float MinHdist = 0.9f;

	private const float MaxHdist = 2.1f;

	private const float MaxYdist = 1.1f;

	public override GameObject Object => (!(Character != null)) ? null : Character.gameObject;

	public override Vector3 Position
	{
		get
		{
			return ImmediatePosition;
		}
		set
		{
			throw new Exception("unsupported assignation in CharacterLocation");
		}
	}

	public override Vector3 RangeHitPosition
	{
		get
		{
			Transform rangeAttacksTargetPoint = Locomotion.RangeAttacksTargetPoint;
			return (!(rangeAttacksTargetPoint != null)) ? Position : rangeAttacksTargetPoint.position;
		}
	}

	public Vector3 ImmediatePosition => (!(Transform != null)) ? Vector3.zero : Transform.position;

	public XYZ VoxelPosition => (!(Transform != null)) ? XYZ.zero : XYZ.FromVector3(Transform.position);

	public CharacterLocation()
	{
		Transform = null;
		Character = null;
		Locomotion = null;
		RdvOption = RdvType.Follow;
	}

	public CharacterLocation(Labor character, RdvType rdvOption = RdvType.Follow)
		: base(character.gameObject)
	{
		Reset(character, rdvOption);
	}

	public static RdvType GetAppropriateRendezVous(Labor attacker, Labor defender)
	{
		if (attacker.Occupation.Profession.HasMeleeAttack() && defender.Occupation.Profession.HasMeleeAttack())
		{
			return RdvType.Meet;
		}
		if (attacker.Occupation.Profession.HasMeleeAttack() && !defender.Occupation.Profession.HasMeleeAttack())
		{
			return RdvType.Intercept;
		}
		return RdvType.Intercept;
	}

	public void Reset(Labor character, RdvType rdvOption = RdvType.Follow)
	{
		Character = character;
		Transform = character.transform;
		Locomotion = character.navigation.Locomotion;
		RdvOption = rdvOption;
	}

	public override bool TargetReached(GameObject source, Vector3 projectedPosition)
	{
		return TargetReached(source, XYZ.FromVector3(projectedPosition), VoxelPosition);
	}

	public bool TargetReachedOrAcceptable(GameObject source, XYZ projectedPosition, XYZ targetMeasuredPosition)
	{
		return IsInVeryCloseRange(projectedPosition, VoxelPosition) || TargetReached(source, projectedPosition, targetMeasuredPosition);
	}

	public bool TargetReached(GameObject source, XYZ projectedPosition, XYZ targetMeasuredPosition)
	{
		if (Character == null)
		{
			return false;
		}
		XYZ xYZ = targetMeasuredPosition - projectedPosition;
		int num = XYZ.SqrMagnitude(xYZ.Flatten());
		if ((float)num < 0.80999994f || (float)num > 4.4099994f || (float)Mathf.Abs(xYZ.y) >= 1.1f)
		{
			return false;
		}
		if ((float)num < 1.21f)
		{
			return true;
		}
		if (ObstaclePreventingAttack(source, projectedPosition, targetMeasuredPosition))
		{
			return false;
		}
		return true;
	}

	private bool IsInVeryCloseRange(XYZ projectedPosition, XYZ targetMeasuredPosition)
	{
		if ((projectedPosition - targetMeasuredPosition).Flatten().Manhattan() <= 1)
		{
			int num = projectedPosition.y - targetMeasuredPosition.y;
			return (float)num >= -1.1f && (float)num <= 1.1f;
		}
		return false;
	}

	private bool ObstaclePreventingAttack(GameObject source, XYZ projectedPosition, XYZ targetMeasuredPosition)
	{
		return PathNodeRegistry.CountMutualLibertes(projectedPosition, targetMeasuredPosition, PathBranch.BranchType.Climbing, (XYZ p, GameObject _source) => !VolumeRegistry.HasVolumesWithPropertyAt(p, VProperty.obstacle, (IVolume v) => !v.PassThrough.CanPassThrough(_source)), source) == 0;
	}

	public override float DistanceFromTarget(Vector3 otherPosition)
	{
		return (ImmediatePosition - otherPosition).Flatten().Manhattan();
	}
}
