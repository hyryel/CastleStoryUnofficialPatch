using System;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Audio;
using Brix.Game.Components;
using Brix.Utils.Locomotion;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class LocomotionStateUpdater
{
	protected Locomotion4 Locomotion;

	protected Movement Movement => Locomotion.Movement;

	protected Fighting Fighting => Locomotion.Fighting;

	protected Archery Archery => Locomotion.Archery;

	protected Common Common => Locomotion.Common;

	protected TargetMatcher TargetMatcher => Locomotion.targetMatcher;

	protected Animator animator => Locomotion.animator;

	protected GridLocomotionControllerInterface controller => Locomotion.controller;

	protected Transform trf => Locomotion.trf;

	protected string name => Locomotion.name;

	protected float Time => Locomotion.targetMatcher.Time;

	protected Locomotion4.StateUpdaters States => Locomotion.States;

	protected DestinationIterator PathIterator => Locomotion.PathIterator;

	protected BaseVoxelTransform voxel => Locomotion.voxel;

	protected GameObject gameObject => Locomotion.gameObject;

	protected BricktronSoundEffects Sounds => Locomotion.Sounds;

	protected AnimatorLayerActivator layerActivator => Locomotion.layerActivator;

	protected Interaction Interaction => Locomotion.Interaction;

	public virtual void Init(Locomotion4 locomotion)
	{
		Locomotion = locomotion;
	}

	public virtual void Enter()
	{
	}

	public virtual void Exit()
	{
	}

	public virtual void Update()
	{
	}

	protected void OverrideMatchTarget(float time, Vector3 position, Quaternion orientation)
	{
		TargetMatcher.OverrideMatchTarget(time, position, orientation);
	}

	protected void OverrideMatchTarget(float time, float rotationTime, Vector3 position, Quaternion orientation)
	{
		TargetMatcher.OverrideMatchTarget(time, rotationTime, position, orientation);
	}

	protected void MatchTarget(AnimatorStateProgress state, Vector3 position, Quaternion orientation, float start, float end)
	{
		TargetMatcher.MatchTarget(state.StateHash, position, orientation, start, end);
	}

	protected void MatchTarget(AnimatorStateProgress state, Vector3 position, float start, float end)
	{
		TargetMatcher.MatchTarget(state.StateHash, position, start, end);
	}

	protected void MatchTarget(AnimatorStateProgress state, Quaternion orientation, float start, float end)
	{
		TargetMatcher.MatchTarget(state.StateHash, orientation, start, end);
	}

	protected void MatchTargetWithY(AnimatorStateProgress state, Vector3 position, float start, float end)
	{
		TargetMatcher.MatchTargetWithY(state.StateHash, position, start, end);
	}

	protected void MatchTargetWithY(AnimatorStateProgress state, Vector3 position, Quaternion orientation, float start, float end)
	{
		TargetMatcher.MatchTargetWithY(state.StateHash, position, orientation, start, end);
	}

	protected bool IsTowards(Location destination, float toleranceAngle = 4f)
	{
		return Vector3.Angle(trf.forward.Flatten(), (destination.Position - trf.position).Flatten()) < toleranceAngle;
	}

	protected bool IsTowards(WalkNode destination, float toleranceAngle = 4f)
	{
		return Vector3.Angle(trf.forward.Flatten(), (destination.Position - trf.position).Flatten()) < toleranceAngle;
	}

	protected bool IsAligned(WalkNode destination, float toleranceAngle = 10f)
	{
		return Vector3.Angle(trf.forward.Flatten(), (destination.Orientation * Vector3.forward).Flatten().normalized) < toleranceAngle;
	}

	protected bool IsOnSpot(WalkNode destination)
	{
		return IsSameVoxelOrAlmost(destination.Position, trf.position);
	}

	protected bool IsClose(WalkNode destination, float toleranceDistance = 1.5f)
	{
		return Vector3.Distance(trf.position.Flatten(), destination.Position.Flatten()) < toleranceDistance;
	}

	protected bool IsOnWayTo(WalkNode destination, WalkNode fromPrevious)
	{
		if (XYZ.FromVector3(fromPrevious.Position) != XYZ.FromVector3(trf.position))
		{
			return false;
		}
		Vector3 vector = fromPrevious.Position - trf.position;
		Vector3 to = destination.Position - trf.position;
		return Vector3.Angle(vector, to) >= 120f;
	}

	protected static bool IsSameVoxelOrAlmost(Vector3 a, Vector3 b)
	{
		return IsSameVoxelOrAlmost(XYZ.FromVector3(a), XYZ.FromVector3(b));
	}

	protected static bool IsSameVoxelOrAlmost(XYZ a, XYZ b)
	{
		return a.x == b.x && a.z == b.z && a.y >= b.y - 1 && a.y <= b.y + 1;
	}

	protected static bool IsAdjacent(Vector3 a, Vector3 b)
	{
		return IsAdjacent(XYZ.FromVector3(a), XYZ.FromVector3(b));
	}

	protected static bool IsAdjacent(XYZ a, XYZ b)
	{
		XYZ xYZ = a - b;
		return Mathf.Abs(xYZ.x) + Mathf.Abs(xYZ.z) == 1 && Mathf.Abs(xYZ.y) <= 1;
	}
}
