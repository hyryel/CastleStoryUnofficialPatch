using System;
using Brix.Engine;
using UnityEngine;

namespace Brix.Game.AI;

[Serializable]
public class SpaceLocation : Location
{
	private bool _hasSpecificOrientation;

	protected Vector3 Pos;

	private Quaternion rotation;

	public override bool HasSpecificOrientation => _hasSpecificOrientation;

	public override Vector3 Position
	{
		get
		{
			return Pos;
		}
		set
		{
			Pos = value;
		}
	}

	public override Quaternion Orientation
	{
		get
		{
			return rotation;
		}
		set
		{
			rotation = value;
			_hasSpecificOrientation = true;
		}
	}

	public SpaceLocation(Vector3 pos, bool walkable = true)
	{
		SetPosition(pos, walkable);
		rotation = Quaternion.identity;
	}

	public SpaceLocation(Vector3 pos, Quaternion rot, bool specificOrientation = true)
	{
		Pos = pos;
		rotation = rot;
		_hasSpecificOrientation = specificOrientation;
	}

	public void SetPosition(Vector3 pos, bool walkable = true)
	{
		if (walkable)
		{
			pos = VoxelPresence.GetWalkableVoxel(XYZ.FromVector3(pos)).ToVector3();
		}
		Pos = pos;
	}

	public override bool TargetReached(GameObject source, Vector3 position)
	{
		return position == Position;
	}
}
