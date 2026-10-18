using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components.Volumes;
using Brix.Lifecycle.Pooling;
using Brix.Pathfinding;
using Brix.Pathfinding.Collections;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Locomotion;
using Motus.Behavior;
using Motus.Behavior.Extensions;
using Smooth.Algebraics;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class NavigationNodes
{
	private class Tries : IPoolable, Brix.Utils.IResetable, IDisposable
	{
		private int _tries;

		public int ConsecutiveFailures => _tries;

		public bool Succeeded => _tries < 0;

		void Brix.Utils.IResetable.ResetObject()
		{
			_tries = 0;
		}

		public static Tries Get()
		{
			return ObjectPool<Tries>.Reserve();
		}

		public void Dispose()
		{
			ObjectPool<Tries>.Release(this);
		}

		public StaticNode Reset()
		{
			return Node.Do((Tries self) => self._tries = 0, this);
		}

		public StaticNode SucceedIf(StaticNode node)
		{
			return Node.Sequence.Do(node).Do((Tries self) => self._tries = -1, this);
		}

		public StaticNode CountFailureAndWait(int ticks)
		{
			return Node.Sequence.Do((Tries self) => self._tries++, this).Do(Node.WaitForTicks(ticks));
		}
	}

	[Flags]
	public enum FindObstacleMode
	{
		None = 0,
		JumpOff = 1,
		BreakVoxel = 2,
		ClimbWall = 4,
		JumpInVoid = 8,
		BreakObject = 0x10,
		JumpOnFriends = 0x20,
		All = -1
	}

	private class FindBestObstacleData : IPoolable, Brix.Utils.IResetable, IDisposable
	{
		public readonly HashSet<XYZ> Explored = new HashSet<XYZ>();

		public readonly Brix.Pathfinding.Collections.PriorityQueue<float, XYZ> Queue = new Brix.Pathfinding.Collections.PriorityQueue<float, XYZ>();

		public static FindBestObstacleData Get()
		{
			return ObjectPool<FindBestObstacleData>.Reserve();
		}

		public void Dispose()
		{
			ObjectPool<FindBestObstacleData>.Release(this);
		}

		public void ResetObject()
		{
			Explored.Clear();
			Queue.Clear();
		}
	}

	public struct ObstacleSearchResult
	{
		public readonly XYZ Direction;

		public readonly XYZ Prospect;

		public readonly FindObstacleMode SearchMode;

		public ObstacleSearchResult(XYZ direction, XYZ prospect, FindObstacleMode searchMode)
		{
			Direction = direction;
			Prospect = prospect;
			SearchMode = searchMode;
		}
	}

	public static bool DebugEveryoneCanDoEverything = false;

	private static readonly XYZ[] Directions = new XYZ[4]
	{
		XYZ.forward,
		-XYZ.forward,
		XYZ.right,
		-XYZ.right
	};

	public static Node Sit(this Labor labor)
	{
		return (StaticNode)Node.Sequence.Label("Sit Sequence").Do((Labor _labor) => _labor.HaltWhatever(), labor).Do(delegate(Labor _labor)
		{
			_labor.navigation.Locomotion.DoSit();
		}, labor);
	}

	public static StaticNode RunToExactly(this Labor labor, Location location)
	{
		return Node.Sequence.Label("RunToExactly Sequence").Do((Labor self) => self.navigation.Locomotion.NoRun = false, labor).Do((Labor self, Location _location) => self._NavigateExactlyTo(_location), labor, location);
	}

	public static StaticNode WalkToExactly(this Labor labor, Location location)
	{
		return Node.Sequence.Label("WalkToExactly Sequence").Do((Labor self) => self.navigation.Locomotion.NoRun = true, labor).Do((Labor self, Location _location) => self._NavigateExactlyTo(_location), labor, location);
	}

	public static StaticNode RunToward(this Labor labor, Location location, bool precise, bool aggressive)
	{
		return Node.Sequence.Label("RunToward Sequence").Do((Labor self) => self.navigation.Locomotion.NoRun = false, labor).Do(delegate(Labor self, Location _location, bool _precise, bool _aggressive)
		{
			bool precise2 = _precise;
			return self.NavigateOrBreakYourWayTo(_location, FindObstacleMode.None, precise2, _aggressive);
		}, labor, location, precise, aggressive);
	}

	public static StaticNode WalkToward(this Labor labor, Location location, bool precise, bool aggressive)
	{
		return Node.Sequence.Label("WalkToward Sequence").Do((Labor self) => self.navigation.Locomotion.NoRun = true, labor).Do(delegate(Labor self, Location _location, bool _precise, bool _aggressive)
		{
			bool precise2 = _precise;
			return self.NavigateOrBreakYourWayTo(_location, FindObstacleMode.None, precise2, _aggressive);
		}, labor, location, precise, aggressive);
	}

	public static StaticNode RunToUntil(this Labor labor, ObjectLocation location, Func<ObjectLocation, bool> test)
	{
		return Node.Selector.Label("RunToUntil Sequence").When((Func<ObjectLocation, bool> _test, ObjectLocation _location) => _test(_location), test, location).Do(Node.Empty)
			.Else()
			.Do((Func<Labor, ObjectLocation, StaticNode>)RunToExactly, labor, location);
	}

	public static StaticNode TurnAndJumpToward(this Labor labor, Location location)
	{
		return Node.Sequence.Label("TurnAndJumpToward Sequence").Do((Labor self) => self.navigation.Locomotion.NoRun = false, labor).Do((Labor self, Location _location) => self.GoJumpOff(self.presence.CurrentVoxel, XYZ.FromVector3(_location.Position)), labor, location);
	}

	public static Node CompleteStop(this Labor labor, float duration)
	{
		return (StaticNode)Node.Sequence.Label("CompleteStop Sequence").Do(labor.HaltWhatever()).Do(Node.WaitForTicks(Mathf.RoundToInt(duration / Time.fixedDeltaTime)));
	}

	private static StaticNode _NavigateTo(this Labor labor, Location location, bool exact = true, bool precise = true, bool aggressive = false)
	{
		return Node.Selector.When((Labor l, Location loc) => l == null || loc == null, labor, location).Do((Func<StaticNode>)Node.FailWork<Exceptions.NullTarget>).Label("Fail (Location or Labor Null)")
			.If((Location loc, GameObject go) => loc.Object == go, location, labor.gameObject)
			.Do((Labor l) => l.HaltWhatever(), labor)
			.Label("Halt because move to self")
			.Else()
			.Do((Labor _labor, Tuple<Location, bool, bool, bool> t) => _labor.navigation.NavigateTo(t._1, t._2, t._3, t._4), labor, Tuple.Create(location, exact, precise, aggressive))
			.Label("Navigate to " + location)
			.ToNode;
	}

	private static StaticNode _NavigateToAndFailIfNotReached(this Labor labor, Location location, bool precise = true)
	{
		Sequencer.Builder sequence = Node.Sequence;
		bool precise2 = precise;
		return sequence.Do(labor._NavigateTo(location, exact: true, precise2)).Do(Node.Selector.If(IsAt, labor, location).Do(Node.Empty).Else()
			.Do(Node.FailPathTo<Exceptions.PathExecutionFailed>(location)));
	}

	private static StaticNode _NavigateExactlyTo(this Labor labor, Location location, bool precise = true)
	{
		Tries tries = Tries.Get();
		return Node.Sequence.Do(Node.Trier.Do(tries.SucceedIf(labor._NavigateToAndFailIfNotReached(location, precise))).Label("NavigateTo Exactly").Do(tries.CountFailureAndWait(50))
			.Label("Count Failures")).Do(Node.Selector.If((Labor _labor) => _labor == null, labor).Do((Func<StaticNode>)Node.FailWork<Exceptions.NullLabor>).If((Tries t) => t.ConsecutiveFailures >= 3, tries)
			.Do(Node.FailPathTo<Exceptions.PathExecutionFailed>(location))).Loop()
			.Until((Tries t) => t.Succeeded, tries)
			.Finally(delegate(Tries t)
			{
				t.Dispose();
			}, tries);
	}

	private static Node NavigateOrBreakYourWayTo(this Labor labor, Location location, FindObstacleMode mode = FindObstacleMode.None, bool precise = true, bool aggressive = true)
	{
		Tries tries = Tries.Get();
		return (StaticNode)Node.Sequence.Do(labor._NavigateTo(location, exact: false, precise, aggressive)).Label("NavigateTo Approximately").Do(Node.Selector.If(IsAt, labor, location).Do(Node.Break()))
			.Do(Node.Trier.Do(tries.SucceedIf(labor.JumpOrMakeWayTo(location, mode))).Label("Jump or Make your way to").Do(tries.CountFailureAndWait(50))
				.Label("Count Failures"))
			.Do(Node.Selector.If((Labor _labor, Location _location) => _labor == null || _location == null, labor, location).Do(Node.FailWork<Exceptions.NullLabor>()).If((Tries t) => t.ConsecutiveFailures >= 3, tries)
				.Do(Node.FailPathTo<Exceptions.PathExecutionFailed>(location))
				.If((Tries t) => t.Succeeded, tries)
				.Do(tries.Reset()))
			.Loop()
			.Finally(delegate(Tries t)
			{
				t.Dispose();
			}, tries);
	}

	public static bool IsAt(this Labor labor, Location location)
	{
		return location.TargetReached(labor.gameObject, labor.transform.position.Rounded());
	}

	private static Node NavigateUntilSuccess(StaticNode navigateNode, StaticNode failNode, int retryDelay = 50)
	{
		Tries tries = Tries.Get();
		return (StaticNode)Node.Selector.When((Tries t) => t.Succeeded, tries).Do(Node.Empty).When((Tries t) => t.ConsecutiveFailures >= 3, tries)
			.Do(failNode)
			.Else()
			.Do(Node.Trier.Do(tries.SucceedIf(navigateNode)).Do(tries.CountFailureAndWait(retryDelay)).Label("Count Failures"))
			.Finally(delegate(Tries t)
			{
				t.Dispose();
			}, tries);
	}

	public static StaticNode JumpOrMakeWayTo(this Labor labor, Location location, FindObstacleMode _mode)
	{
		StaticNode n = Node.Sequence.Do((Labor l, Location loc, FindObstacleMode m) => FindBestObstacle(l, loc.Position, m), labor, location, _mode).Process((ObstacleSearchResult result, Labor l, Location loc) => Node.Selector.If((ObstacleSearchResult r) => r.SearchMode == FindObstacleMode.BreakVoxel, result).Do((ObstacleSearchResult r, Labor _l) => _l.MakeWay(r.Prospect), result, l).If((ObstacleSearchResult r) => r.SearchMode == FindObstacleMode.BreakObject, result)
			.Do((ObstacleSearchResult r, Labor _l) => _l.BreakObject(r.Prospect), result, l)
			.If((ObstacleSearchResult r) => r.SearchMode == FindObstacleMode.JumpOff, result)
			.Do((ObstacleSearchResult r, Labor _l) => _l.GoJump(r.Direction, r.Prospect), result, l)
			.If((ObstacleSearchResult r) => r.SearchMode == FindObstacleMode.ClimbWall, result)
			.Do((ObstacleSearchResult r, Labor _l) => _l.GoClimb(r.Direction, r.Prospect), result, l)
			.Else()
			.Do(Node.FailPathTo<Exceptions.PathExecutionFailed>(loc)), labor, location);
		return Node.Selector.If((Tuple<Labor, Location> t) => !t._1.IsAt(t._2), new Tuple<Labor, Location>(labor, location)).Do(n);
	}

	private static void SetupDigGoal(DigGoal goal)
	{
		goal.DestroyIfAbandonned = true;
		goal.EatGarnotteWhileWorking = false;
		goal.RebuildNode();
	}

	public static Node MakeWay(this Labor labor, XYZ voxel)
	{
		return (StaticNode)Node.Sequence.Label("MakeWay").Do((XYZ _voxel) => ObjectPoolSingleton.Request_Unsafe(Test.DigGoal, _voxel.ToVector3(), Quaternion.identity).GetComponent<DigGoal>(), voxel).Label("Request")
			.Process((DigGoal goal) => Wait.ForLateSignal(goal.GoalStarted))
			.Label("WaitForGoal")
			.Process(delegate(DigGoal goal)
			{
				SetupDigGoal(goal);
			})
			.Label("Setup")
			.Process((DigGoal goal, Labor _labor) => goal.GetWorkNode(_labor), labor)
			.Label("Work")
			.Do(Node.WaitForTicks(20))
			.Label("Wait");
	}

	public static Node BreakObject(this Labor labor, XYZ voxel)
	{
		return (StaticNode)Node.Sequence.Label("BreakObject").Do(delegate(Labor _labor, XYZ _voxel)
		{
			Order.Combat.GoAttackDirectly.Send(_labor, GetBreakableObjectAt(_voxel));
		}, labor, voxel);
	}

	public static Node GoJump(this Labor labor, XYZ from, XYZ to)
	{
		return (StaticNode)Node.Sequence.Label("GoJump").Do((Labor _labor, XYZ _from, XYZ _to) => FindBestObstacle(_labor, _to.ToVector3(), FindObstacleMode.JumpOff), labor, from, to).Process((ObstacleSearchResult result, Labor _labor) => Node.Sequence.Do((ObstacleSearchResult r, Labor l) => l._NavigateToAndFailIfNotReached(Location.Of(r.Direction)), result, _labor).Do((ObstacleSearchResult r, Labor l) => l.TurnToward(r.Prospect), result, _labor), labor)
			.Do((Func<Labor, Node>)JumpOff, labor);
	}

	public static Node GoJumpOff(this Labor labor, XYZ from, XYZ to)
	{
		FindObstacleMode findObstacleMode = FindObstacleMode.JumpOff;
		findObstacleMode |= FindObstacleMode.JumpInVoid;
		findObstacleMode |= FindObstacleMode.JumpOnFriends;
		Sequencer.Builder p = Node.Sequence.Do((Labor _labor, XYZ _to, FindObstacleMode _jumpMode) => FindBestObstacle(_labor, _to.ToVector3(), _jumpMode), labor, to, findObstacleMode).Process((ObstacleSearchResult result, Labor _labor) => Node.Sequence.Do((ObstacleSearchResult r, Labor l) => l._NavigateToAndFailIfNotReached(Location.Of(r.Direction)), result, _labor).Do((ObstacleSearchResult r, Labor l) => l.TurnToward(r.Prospect), result, _labor), labor).Do((Func<Labor, Node>)JumpOff, labor);
		StaticNode p2 = Node.Fail(SharedWorkException<Exceptions.UnknownError>.Get);
		return (StaticNode)Node.Trier.Do((Sequencer.Builder jump, StaticNode fail) => NavigateUntilSuccess(jump, fail), p, p2).Do((Labor _labor, XYZ _to) => _labor._NavigateTo(Location.Of(_to), exact: false), labor, to);
	}

	public static Node GoClimb(this Labor labor, XYZ from, XYZ to)
	{
		return (StaticNode)Node.Sequence.Label("GoClimb").Do((Labor _labor, XYZ _from) => _labor._NavigateTo(Location.Of(_from)), labor, from).Do((Func<Labor, XYZ, Node>)TurnToward, labor, to)
			.Do((Func<Labor, XYZ, Node>)ClimbWall, labor, to);
	}

	public static Node TurnToward(this Labor labor, Location location)
	{
		return (StaticNode)Node.Sequence.Do((Labor _labor, Location _location) => _labor.navigation.TurnTowards(_location), labor, location);
	}

	public static Node TurnToward(this Labor labor, XYZ position)
	{
		return (StaticNode)Node.Sequence.Do((Labor _labor, XYZ _position) => _labor.navigation.TurnTo(_position.ToVector3()), labor, position);
	}

	public static Node JumpOff(this Labor labor)
	{
		return (StaticNode)Node.Sequence.Do(delegate(Labor _labor)
		{
			_labor.navigation.Locomotion.JumpOff();
		}, labor).Do(Node.WaitForTicks(50));
	}

	public static Node ClimbWall(this Labor labor, XYZ position)
	{
		return (StaticNode)Node.Sequence.Do(delegate(Labor _labor, XYZ _position)
		{
			_labor.navigation.Locomotion.ClimbTo(Location.Of(_position));
		}, labor, position).Do(Node.WaitForTicks(50));
	}

	public static Node FreezeAt(this Labor labor, Vector3 position, int ticks)
	{
		return (StaticNode)Node.Sequence.Do(delegate(Labor _labor, Vector3 _position)
		{
			_labor.navigation.Freeze(_position);
		}, labor, position).Do(Node.WaitForTicks(ticks)).Do(delegate(Labor _labor)
		{
			_labor.navigation.Locomotion.GetReadyToMove();
		}, labor);
	}

	private static GameObject GetBreakableObjectAt(XYZ prospect)
	{
		foreach (IVolume item in VolumeRegistry.VolumesWithPropertyAt(prospect, VProperty.obstacle))
		{
			if (!(item.GameObject == null))
			{
				IDamageReceiver component = item.GameObject.GetComponent<IDamageReceiver>();
				if (component != null && !component.IsInvincible)
				{
					return item.GameObject;
				}
			}
		}
		return null;
	}

	private static IEnumerator<ComputationProgress<ObstacleSearchResult>> FindBestObstacle(Labor labor, Vector3 destination, FindObstacleMode _mode = FindObstacleMode.None, bool failIfPathFound = false)
	{
		if (_mode != FindObstacleMode.None)
		{
			return _FindBestObstacle(labor, destination, _mode, 150, failIfPathFound);
		}
		FindObstacleMode findObstacleMode = FindObstacleMode.None;
		if (labor.Profession.CanMakeWay() || DebugEveryoneCanDoEverything)
		{
			findObstacleMode |= FindObstacleMode.BreakVoxel | FindObstacleMode.BreakObject;
		}
		if (labor.Profession.CanJumpOff() || DebugEveryoneCanDoEverything)
		{
			findObstacleMode |= FindObstacleMode.JumpOff;
		}
		if (labor.Profession.CanClimbWall() || DebugEveryoneCanDoEverything)
		{
			findObstacleMode |= FindObstacleMode.ClimbWall;
		}
		return _FindBestObstacle(labor, destination, findObstacleMode, 150, failIfPathFound);
	}

	private static IEnumerator<ComputationProgress<ObstacleSearchResult>> _FindBestObstacle(Labor labor, Vector3 destination, FindObstacleMode mode, int qteMax, bool failIfPathFound)
	{
		if (mode == FindObstacleMode.None)
		{
			yield return ComputationProgress<ObstacleSearchResult>.Fail();
		}
		XYZ end = XYZ.FromVector3(destination);
		XYZ start = labor.presence.CurrentVoxel;
		PathBranch.BranchType branchPermission = ((!mode.HasFlag(FindObstacleMode.BreakVoxel)) ? PathBranch.BranchType.Climbing : PathBranch.BranchType.Breaking);
		ObstacleSearchResult bestResult = default(ObstacleSearchResult);
		float bestPriority = float.PositiveInfinity;
		bool endFound = false;
		using (FindBestObstacleData data = FindBestObstacleData.Get())
		{
			data.Explored.Add(start);
			data.Queue.Enqueue(0f, start);
			while (data.Queue.Count != 0 && qteMax > 0)
			{
				if (qteMax % 10 == 0)
				{
					yield return ComputationProgress<ObstacleSearchResult>.Wait();
				}
				qteMax--;
				XYZ next = data.Queue.Dequeue();
				if (next == end)
				{
					endFound = true;
					break;
				}
				SmallSubset_16from16 voxelPaths = default(SmallSubset_16from16);
				foreach (XYZ item in PathNodeRegistry.GetConnectedNodes(next, branchPermission).Where(CanCrossAt).NoLeak())
				{
					XYZ xYZ = item - next;
					voxelPaths.Add(xYZ.x + xYZ.z * 3 + 4);
					if (data.Explored.Add(item))
					{
						data.Queue.Enqueue(GetObstaclePriority(destination, item), item);
					}
				}
				if (voxelPaths.Count >= 4)
				{
					continue;
				}
				for (int i = 0; i < 4; i++)
				{
					XYZ xYZ2 = Directions[i];
					if (voxelPaths.Contains(xYZ2.x + xYZ2.z * 3 + 4))
					{
						continue;
					}
					ObstacleSearchResult obstacleSearchResult = FindObstacleInColumn(next + xYZ2, labor, mode, next);
					if (obstacleSearchResult.SearchMode != FindObstacleMode.None)
					{
						float obstaclePriority = GetObstaclePriority(destination, obstacleSearchResult.Prospect);
						if (obstaclePriority < bestPriority)
						{
							bestResult = obstacleSearchResult;
							bestPriority = obstaclePriority;
						}
					}
				}
			}
		}
		if (bestResult.SearchMode == FindObstacleMode.None || (failIfPathFound && endFound))
		{
			yield return ComputationProgress<ObstacleSearchResult>.Fail();
		}
		yield return new ComputationProgress<ObstacleSearchResult>.Result(bestResult);
	}

	private static float GetObstaclePriority(Vector3 destination, XYZ voxel)
	{
		return Vector3.Distance(destination, voxel.ToVector3());
	}

	private static bool CanCrossAt(XYZ p)
	{
		return !VolumeRegistry.HasVolumesWithPropertyAt(p, VPropertyMask.obstacle | VPropertyMask.character);
	}

	private static bool IsMineStaircaseBlock(IVoxel voxel)
	{
		Factory.AssetKey key = voxel.BlockInfo.Key;
		return key == Blocks.Mine_Staircase || key == Blocks.Mine_Staircase_Block;
	}

	private static ObstacleSearchResult FindObstacleInColumn(XYZ choice, Labor labor, FindObstacleMode mode, XYZ next)
	{
		bool flag = mode.HasFlag(FindObstacleMode.BreakVoxel);
		bool flag2 = mode.HasFlag(FindObstacleMode.JumpOff);
		bool flag3 = mode.HasFlag(FindObstacleMode.ClimbWall);
		bool flag4 = mode.HasFlag(FindObstacleMode.JumpInVoid);
		bool flag5 = mode.HasFlag(FindObstacleMode.JumpOnFriends);
		for (int num = choice.y + 2; num >= choice.y - 25; num--)
		{
			XYZ xYZ = new XYZ(choice.x, num, choice.z);
			if (flag)
			{
				GameObject breakableObjectAt = GetBreakableObjectAt(xYZ);
				if (breakableObjectAt != null && Location.Of(breakableObjectAt).TargetReached(labor.gameObject, next.ToVector3()))
				{
					return new ObstacleSearchResult(next, xYZ, FindObstacleMode.BreakObject);
				}
			}
			if (Voxel.IsNonEmpty(xYZ))
			{
				if (num >= choice.y - 1)
				{
					if (flag)
					{
						IVoxel voxel = Voxel.GetVoxel(xYZ);
						if (!voxel.IsCrust && !voxel.IsInvincible && !IsMineStaircaseBlock(voxel) && !IsMineStaircaseBlock(Voxel.GetVoxel(xYZ + XYZ.up)) && (num >= labor.voxel.position.y || !voxel.IsTerrain) && !VolumeRegistry.HasVolumesWithPropertyAt(xYZ, VProperty.invincible))
						{
							return new ObstacleSearchResult(next, xYZ, FindObstacleMode.BreakVoxel);
						}
					}
					flag2 = false;
				}
				else
				{
					if (flag2 && !NatureRegistry.HasNatureBlockerAt(xYZ) && (flag5 || !VolumeRegistry.HasVolumesWithPropertyAt(xYZ, VProperty.character, (IVolume v, Labor _labor) => Affiliation.IsAllied(_labor.faction, v.GameObject), labor)))
					{
						return new ObstacleSearchResult(next, xYZ, FindObstacleMode.JumpOff);
					}
					flag2 = false;
				}
				flag4 = false;
			}
			else if (num <= choice.y - 25)
			{
				flag3 = false;
				if (flag4)
				{
					return new ObstacleSearchResult(next, xYZ, FindObstacleMode.JumpOff);
				}
			}
			if (num <= choice.y - 4 && Voxel.IsTerrain(xYZ))
			{
				break;
			}
		}
		if (flag3)
		{
			for (int num2 = choice.y + 2; num2 < choice.y + 20; num2++)
			{
				XYZ xYZ2 = new XYZ(choice.x, num2, choice.z);
				if (Voxel.IsEmpty(xYZ2))
				{
					return new ObstacleSearchResult(next, xYZ2, FindObstacleMode.ClimbWall);
				}
			}
		}
		return new ObstacleSearchResult(XYZ.zero, XYZ.zero, FindObstacleMode.None);
	}
}
