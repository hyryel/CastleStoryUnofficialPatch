using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components;
using Brix.Legacy;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.AI;

public abstract class Location
{
	private class EmptyLocation : Location
	{
		public override Vector3 Position
		{
			get
			{
				return Vector3.zero;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override Quaternion Orientation
		{
			get
			{
				return Quaternion.identity;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override GameObject Object
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override bool IsEmpty => true;

		public override bool HasSpecificPosition => false;

		public override bool TargetReached(GameObject source, Vector3 position)
		{
			return false;
		}
	}

	private class EverywhereLocation : Location
	{
		public override Vector3 Position
		{
			get
			{
				return Vector3.zero;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override Quaternion Orientation
		{
			get
			{
				return Quaternion.identity;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override GameObject Object
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public override bool IsEmpty => false;

		public override bool HasSpecificPosition => false;

		public override bool TargetReached(GameObject source, Vector3 position)
		{
			return true;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(Location).FullName);

	public static readonly Location Null = Null;

	public static readonly Location Empty = new EmptyLocation();

	public static readonly Location Everywhere = new EverywhereLocation();

	private static TestTerrainLod.BranchIterator _branchIterator;

	public abstract Vector3 Position { get; set; }

	public abstract Quaternion Orientation { get; set; }

	public virtual GameObject Object { get; set; }

	public virtual bool IsEmpty => false;

	public virtual bool HasSpecificPosition => true;

	public virtual bool HasSpecificOrientation => false;

	public virtual Vector3 RangeHitPosition => Position;

	public static Location Of(Transform t)
	{
		return Of(t.position);
	}

	public static Location Of(int index)
	{
		return Of(Brix.Engine.Abstract.Voxel.GetVecteur(index));
	}

	public static Location Of(XYZ position)
	{
		return new SpaceLocation(position.ToVector3());
	}

	public static Location Of(Vector3 position)
	{
		return new SpaceLocation(position);
	}

	public static Location Of(Component component)
	{
		if (component == null)
		{
			return null;
		}
		return Of(component.gameObject);
	}

	public static Location Of(GameComponent component)
	{
		if (component == null)
		{
			return null;
		}
		return Of(component.gameObject);
	}

	public static Location OfObject(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		Labor component = go.GetComponent<Labor>();
		if (component != null)
		{
			return new CharacterLocation(component);
		}
		if ((bool)go.GetComponent<Garnotte>())
		{
			return LeadingTo(go.transform.position);
		}
		return new ObjectLocation(go);
	}

	public static Location Of(GameObject go)
	{
		return OfObject(go);
	}

	public static Location OfObject(Transform t)
	{
		return (!(t == null)) ? OfObject(t.gameObject) : null;
	}

	public static ReachSpaceLocation LeadingTo(Vector3 position, int reach = 1, bool excludeInside = true)
	{
		return new ReachSpaceLocation(position, reach, excludeInside);
	}

	public static ReachSpaceLocation LeadingTo(GameObject target, int reach = 1, bool excludeInside = true)
	{
		ReachSpaceLocation reachSpaceLocation = LeadingTo(target.transform.position, reach, excludeInside);
		reachSpaceLocation.Object = target;
		return reachSpaceLocation;
	}

	public static Location RandomAround(XYZ center, float radius, Labor navigator)
	{
		TerrainPathNode terrainPathNode = PathNodeRegistry.WalkableAt(center) as TerrainPathNode;
		if (terrainPathNode == null)
		{
			logger.Warn("RaidProject.SomewhereRandomInsideZone: center is not walkable!");
			return Of(center);
		}
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		Vector3 rhs = new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y);
		TestTerrainLod.BranchIterator branchIterator = _branchIterator ?? (_branchIterator = TestPathfinding.Instance.terrainLod.GetTerrainIterator());
		int num = Mathf.RoundToInt(radius) * 4;
		for (int i = 0; i <= num; i++)
		{
			float num2 = 0f;
			foreach (PathBranch sibling in branchIterator.GetSiblings(terrainPathNode))
			{
				if (sibling.Node is TerrainPathNode && sibling.Type.CompareTo(PathBranch.BranchType.Climbing) >= 0 && navigator.presence.CollideWithEveryone(sibling.Node.RepresentativePosition) && !SearchPathRequest.HasBlockerAt(navigator.gameObject, sibling.Node.RepresentativePosition))
				{
					num2 += Vector3.Dot((sibling.Node.RepresentativePosition - terrainPathNode.RepresentativePosition).ToVector3().normalized, rhs) + 1.5f;
				}
			}
			float num3 = UnityEngine.Random.Range(0f, num2);
			foreach (PathBranch sibling2 in branchIterator.GetSiblings(terrainPathNode))
			{
				if (sibling2.Node is TerrainPathNode && sibling2.Type.CompareTo(PathBranch.BranchType.Climbing) >= 0 && navigator.presence.CollideWithEveryone(sibling2.Node.RepresentativePosition) && !SearchPathRequest.HasBlockerAt(navigator.gameObject, sibling2.Node.RepresentativePosition))
				{
					num3 -= Vector3.Dot((sibling2.Node.RepresentativePosition - terrainPathNode.RepresentativePosition).ToVector3().normalized, rhs) + 1.5f;
					if (num3 <= 0f)
					{
						terrainPathNode = (TerrainPathNode)sibling2.Node;
						break;
					}
				}
			}
			if ((float)XYZ.SqrMagnitude(terrainPathNode.RepresentativePosition - center) >= radius * radius)
			{
				break;
			}
		}
		return Of(terrainPathNode.RepresentativePosition);
	}

	public static Location RandomAroundObjectWithPredicate(XYZ center, float radius, PathBranch.BranchType permission, Func<XYZ, bool> acceptFunc)
	{
		if (!(PathNodeRegistry.WalkableAt(center) is TerrainPathNode terrainPathNode))
		{
			logger.Warn("RaidProject.SomewhereRandomInsideZone: center is not walkable!");
			return Of(center);
		}
		TestTerrainLod.BranchIterator branchIterator = _branchIterator ?? (_branchIterator = TestPathfinding.Instance.terrainLod.GetTerrainIterator());
		HashSet<TerrainPathNode> hashSet = CollectionPool.RequestHashSet<TerrainPathNode>();
		Queue<TerrainPathNode> queue = new Queue<TerrainPathNode>();
		queue.Enqueue(terrainPathNode);
		while (queue.Count > 0)
		{
			TerrainPathNode terrainPathNode2 = queue.Dequeue();
			if (!hashSet.Add(terrainPathNode2))
			{
				continue;
			}
			if (acceptFunc(terrainPathNode2.RepresentativePosition))
			{
				return Of(terrainPathNode2.RepresentativePosition);
			}
			foreach (PathBranch sibling in branchIterator.GetSiblings(terrainPathNode))
			{
				if (sibling.Node is TerrainPathNode && sibling.Type.CompareTo(permission) >= 0)
				{
					queue.Enqueue(sibling.Node as TerrainPathNode);
				}
			}
		}
		return Of(terrainPathNode.RepresentativePosition);
	}

	public static ReachSpaceLocation LeadingTo(GameComponent target, int reach = 1, bool excludeInside = true)
	{
		ReachSpaceLocation reachSpaceLocation = LeadingTo(target.transform.position, reach, excludeInside);
		reachSpaceLocation.Object = target.gameObject;
		return reachSpaceLocation;
	}

	public static Location ToFollow(Labor labor)
	{
		return new CharacterLocation(labor);
	}

	public static CharacterLocation ToMeet(Labor labor)
	{
		return new CharacterLocation(labor, CharacterLocation.RdvType.Meet);
	}

	public static Location ToIntercept(Labor labor)
	{
		return new CharacterLocation(labor, CharacterLocation.RdvType.Intercept);
	}

	public static Location ToWorkOn(Vector3 position)
	{
		return ToWorkOn(XYZ.FromVector3(position));
	}

	public static Location ToWorkOn(XYZ position)
	{
		SpaceLocation spaceLocation = new SpaceLocation(Vector3.up * ((float)position.y - 0.5f));
		if (Brix.Engine.Voxel.IsOpenAndWalkable(position))
		{
			spaceLocation.Position -= Vector3.up;
		}
		return spaceLocation;
	}

	public static SpaceLocation ToMoveOn(Vector3 position)
	{
		return new SpaceLocation(position);
	}

	public float GetDistance(Vector3 origin)
	{
		return Vector3.Distance(origin.Flatten(), Position.Flatten());
	}

	public float GetAngleToOrientation(Quaternion origin)
	{
		return Vector3.Angle((origin * Vector3.forward).Flatten(), (Orientation * Vector3.forward).Flatten());
	}

	public bool IsToTheRight(Vector3 originPos, Quaternion originDir)
	{
		Vector3 normalized = (Position - originPos).normalized;
		Vector3 lhs = originDir * Vector3.right;
		lhs.y = 0f;
		lhs.Normalize();
		return Vector3.Dot(lhs, normalized) > 0f;
	}

	public float GetAngleToPosition(Vector3 originPos, Quaternion originDir)
	{
		Vector3 normalized = (Position - originPos).normalized;
		Vector3 vector = originDir * Vector3.forward;
		vector.y = 0f;
		vector.Normalize();
		normalized.y = 0f;
		normalized.Normalize();
		return Vector3.Angle(vector, normalized);
	}

	public float GetDirectionToPosition(Vector3 originPos, Quaternion originDir)
	{
		float num = GetAngleToPosition(originPos, originDir) / 180f;
		if (!IsToTheRight(originPos, originDir))
		{
			num = 0f - num;
		}
		return num;
	}

	public float GetDirectionToOrientation(Quaternion originDir)
	{
		float num = GetAngleToOrientation(originDir) / 180f;
		Quaternion a = Quaternion.LookRotation(originDir * Vector3.right);
		if (Quaternion.Angle(a, Orientation) > 90f)
		{
			num = 0f - num;
		}
		return num;
	}

	public virtual float DistanceFromTarget(Vector3 otherPosition)
	{
		return (Position - otherPosition).magnitude;
	}

	public abstract bool TargetReached(GameObject source, Vector3 position);

	public virtual bool TargetInCloseRange(Vector3 position, float range = 3f)
	{
		return DistanceFromTarget(position) <= range;
	}
}
