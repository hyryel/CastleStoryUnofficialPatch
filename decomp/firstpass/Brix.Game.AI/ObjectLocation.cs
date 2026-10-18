using System;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.AI;

public class ObjectLocation : Location
{
	private static BrixLogger logger = new BrixLogger(typeof(ObjectLocation).FullName);

	protected Func<IVolume, bool> IsTargetFunc;

	public override GameObject Object { get; set; }

	public override Vector3 Position
	{
		get
		{
			if (Object == null)
			{
				return Vector3.zero;
			}
			return Object.transform.position;
		}
		set
		{
			throw new Exception("unsupported assignation in ObjectLocation");
		}
	}

	public override Quaternion Orientation
	{
		get
		{
			return (!(Object == null)) ? Object.transform.rotation : Quaternion.identity;
		}
		set
		{
			throw new Exception("unsupported assignation in ObjectLocation");
		}
	}

	public override Vector3 RangeHitPosition
	{
		get
		{
			if (!Object.IsNullOrReleased())
			{
				VoxelTransform component = Object.GetComponent<VoxelTransform>();
				if (component != null)
				{
					return component.GetRangeOffsettedPosition();
				}
			}
			return Position;
		}
	}

	public ObjectLocation()
	{
		IsTargetFunc = IsTarget;
	}

	public ObjectLocation(GameObject obj)
	{
		if (obj == null)
		{
			logger.Warn("cannot create ObjectLocation from null GameObject");
		}
		Object = obj;
		IsTargetFunc = IsTarget;
	}

	private bool IsTarget(IVolume v)
	{
		Component component = v as Component;
		return component != null && component.gameObject == Object;
	}

	public override bool TargetReached(GameObject source, Vector3 position)
	{
		if (Object == null)
		{
			return false;
		}
		XYZ position2 = XYZ.FromVector3(position);
		return VolumeRegistry.HasVolumesWithPropertyAt(position2, VProperty.access, IsTargetFunc);
	}

	public bool IsValid()
	{
		if (Object.IsNullOrReleased())
		{
			return false;
		}
		return Object.GetInterface<IVolume>()?.Registered ?? true;
	}
}
