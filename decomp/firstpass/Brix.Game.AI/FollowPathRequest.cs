using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Logging;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

[Serializable]
public class FollowPathRequest : IPoolable, IResetable, IDisposable
{
	private static BrixLogger logger = new BrixLogger(typeof(FollowPathRequest).FullName);

	public Navigation Navigation;

	public IPath Path;

	public bool Completed;

	public Labor Blocker;

	public bool Blocked;

	public bool Stopped;

	public bool Started;

	public bool Ended;

	public SignalConnection BlockedByInvalidTerrainConnection = SignalConnection.Empty;

	public SignalConnection BlockedByDynamicObstacleConnection = SignalConnection.Empty;

	public SignalConnection PathInterruptedManuallyConnection = SignalConnection.Empty;

	public SignalConnection PathBlockedConnection = SignalConnection.Empty;

	private readonly Action EndFollowAction;

	private readonly Action PathInterruptedManuallyAction;

	private readonly Action<Vector3> LocomotionPathBlockedAction;

	public bool IsActive => Started && !Ended;

	public FollowPathRequest()
	{
		EndFollowAction = EndFollow;
		PathInterruptedManuallyAction = OnPathInterruptedManually;
		LocomotionPathBlockedAction = OnLocomotionPathBlocked;
	}

	public static FollowPathRequest Make(Navigation navigation, IPath p)
	{
		FollowPathRequest followPathRequest = ObjectPool<FollowPathRequest>.Reserve();
		followPathRequest.Init(navigation, p);
		return followPathRequest;
	}

	public void Dispose()
	{
		ObjectPool<FollowPathRequest>.Release(this);
	}

	public void ResetObject()
	{
		Disconnect();
		Navigation = null;
		Path = null;
		Completed = false;
		Blocker = null;
		Blocked = false;
		Stopped = false;
		Started = false;
		Ended = false;
		PathInterruptedManuallyConnection = SignalConnection.Empty;
		PathBlockedConnection = SignalConnection.Empty;
	}

	private void Disconnect()
	{
		BlockedByInvalidTerrainConnection.Disconnect();
		BlockedByDynamicObstacleConnection.Disconnect();
		PathInterruptedManuallyConnection.Disconnect();
		PathBlockedConnection.Disconnect();
	}

	private void Init(Navigation navigation, IPath p)
	{
		Navigation = navigation;
		Path = p;
		PathInterruptedManuallyConnection = Navigation.PathInterruptedManually.Connect(PathInterruptedManuallyAction);
		PathBlockedConnection = Navigation.Locomotion.PathBlocked.Connect(LocomotionPathBlockedAction);
	}

	public StaticNode FollowNode()
	{
		return Node.Sequence.Do(delegate(FollowPathRequest self)
		{
			self.StartFollow();
		}, this).Do((FollowPathRequest self) => self.FollowPath(), this).Finally(EndFollowAction);
	}

	private void StartFollow()
	{
		Navigation.DebugPath = Path;
		Started = true;
	}

	private void EndFollow()
	{
		Ended = true;
		if ((bool)Navigation)
		{
			Navigation.DebugPath = null;
		}
	}

	public IEnumerator<ActivityProgress> FollowPath()
	{
		if (Path.IsEmpty)
		{
			logger.InfoFormat("Following empty path ({0})", Navigation.name);
			yield return ActivityProgress.Fail("FollowPathRequest: Following empty path");
		}
		else
		{
			if (Blocked || Stopped)
			{
				yield break;
			}
			if (!Navigation.Locomotion.CanMove())
			{
				Navigation.Locomotion.GetReadyToMove();
				yield return ActivityProgress.Wait();
				int i = 0;
				while (!Navigation.Locomotion.CanMove())
				{
					yield return ActivityProgress.Wait();
					int num;
					i = (num = i + 1);
					if (num >= 100)
					{
						logger.InfoFormat("Failed to become able to move ({0})", Navigation.name);
						yield return ActivityProgress.Fail("FollowPathRequest: Failed to move");
					}
				}
			}
			Navigation.Locomotion.DoFollowPath(Path);
			if (!Navigation.Locomotion.PathIterator.Active)
			{
				logger.InfoFormat("Transitioned to an empty path ({0})", Navigation.name);
				yield return ActivityProgress.Fail("FollowPathRequest: Transitioned to an empty path");
			}
			int hasArrivedCount = Navigation.Locomotion.HasArrived.InvokedCount;
			Vector3 lastObservedPosition = Navigation.transform.position;
			float lastTimeMoved = Time.time;
			while (!Blocked && !Stopped && hasArrivedCount == Navigation.Locomotion.HasArrived.InvokedCount)
			{
				yield return ActivityProgress.Wait();
				if ((lastObservedPosition - Navigation.transform.position).sqrMagnitude >= 0.010000001f)
				{
					lastObservedPosition = Navigation.transform.position;
					lastTimeMoved = Time.time;
				}
				else if (Time.time >= lastTimeMoved + 5f && !Navigation.Labor.Profession.CanMakeWay())
				{
					logger.InfoFormat("FollowPath seems stuck at {0} for {1}", Navigation.transform.position, Navigation.name);
					Blocked = true;
				}
			}
			if (Blocked || Stopped)
			{
				if (Navigation.Locomotion.PathIterator.Active)
				{
					Navigation.Locomotion.DoHalt();
				}
				yield return ActivityProgress.Fail("FollowPathRequest Failed : Blocked or Stopped");
			}
			logger.DebugFormat("Bricktron '{0}' has arrived successfully at {1}.", Navigation.name, Navigation.transform.position);
			Completed = true;
		}
	}

	private void OnPathInterruptedManually()
	{
		logger.InfoFormat("Bricktron '{0}' at '{1}' was interrupted manually", Navigation.name, Navigation.transform.position);
		Stopped = true;
	}

	private void OnLocomotionPathBlocked(Vector3 position)
	{
		logger.InfoFormat("Bricktron '{0}' at '{1}' was interrupted by locomotion at '{2}'", Navigation.name, Navigation.transform.position, position);
		Stopped = true;
	}
}
