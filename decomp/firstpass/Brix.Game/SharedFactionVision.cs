using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Lifecycle;
using Brix.Game.Components;
using Brix.Game.Components.Tracking;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game;

public class SharedFactionVision : UpdatableGameComponent<SharedFactionVision>
{
	private class Common
	{
		public readonly List<SharedFactionVision> SharedVisions;

		public RefreshAllActiveCollisionsEnumerator Enumerator;

		public Common()
		{
			SharedVisions = new List<SharedFactionVision>();
			Enumerator = new RefreshAllActiveCollisionsEnumerator(SharedVisions);
		}
	}

	public struct RefreshAllActiveCollisionsEnumerator
	{
		private readonly List<SharedFactionVision> _sharedVisions;

		private int _iSharedVision;

		private RefreshActiveTrackingGroupsEnumerator _refreshGroupsEnumerator;

		private RefreshActiveCollisionsEnumerator _refreshCollisionsEnumerator;

		public RefreshAllActiveCollisionsEnumerator(List<SharedFactionVision> sharedVisions)
		{
			this = default(RefreshAllActiveCollisionsEnumerator);
			_sharedVisions = sharedVisions;
		}

		public void Start()
		{
			_iSharedVision = 0;
			_refreshGroupsEnumerator = default(RefreshActiveTrackingGroupsEnumerator);
			_refreshCollisionsEnumerator = default(RefreshActiveCollisionsEnumerator);
		}

		public bool MoveNext()
		{
			while (true)
			{
				if (_refreshGroupsEnumerator.MoveNext())
				{
					return true;
				}
				if (_refreshCollisionsEnumerator.MoveNext())
				{
					return true;
				}
				if (_iSharedVision >= _sharedVisions.Count)
				{
					break;
				}
				_refreshGroupsEnumerator.Start(_sharedVisions[_iSharedVision]);
				_refreshCollisionsEnumerator.Start(_sharedVisions[_iSharedVision]);
				_iSharedVision++;
			}
			return false;
		}
	}

	private struct RefreshActiveTrackingGroupsEnumerator
	{
		private SharedFactionVision _sharedVision;

		private int _step;

		private int _nPreviousGroups;

		public void Start(SharedFactionVision sharedVision)
		{
			_sharedVision = sharedVision;
			_step = 1;
			_nPreviousGroups = _sharedVision._trackingGroups.Count;
		}

		public bool MoveNext()
		{
			switch (_step)
			{
			case 1:
				FlagAllPreviousTrackingGroups(_nPreviousGroups);
				_step++;
				break;
			case 2:
				AssignDetectionStatusFromAllVisions();
				_step++;
				break;
			case 3:
				RemoveNoLongerDetectedTrackingGroups(_nPreviousGroups);
				_step++;
				break;
			case 4:
				RemoveNoLongerDetectedFullGroups();
				_step++;
				break;
			case 5:
				RemoveNoLongerDetectedPartialGroups();
				_step++;
				break;
			case 6:
				UnflagStillDetectedGroups();
				_step++;
				break;
			case 7:
				AddNewlyDetectedFullAndPartialGroups();
				_step++;
				break;
			default:
				return false;
			}
			return true;
		}

		private void FlagAllPreviousTrackingGroups(int nGroups)
		{
			for (int i = 0; i < nGroups; i++)
			{
				TrackingRegistry.TrackingGroup trackingGroup = _sharedVision._trackingGroups[i];
				trackingGroup.visionScanId = 1;
			}
		}

		private void AssignDetectionStatusFromAllVisions()
		{
			foreach (LocalFactionVision allVision in _sharedVision.AllVisions)
			{
				XYZ xYZ = XYZ.FromVector3(allVision.transform.position);
				int lineOfSightRange = allVision.LineOfSightRange;
				using TrackingRegistry.RelevantGroupsInRangeEnumerator relevantGroupsInRangeEnumerator = new TrackingRegistry.RelevantGroupsInRangeEnumerator(xYZ, lineOfSightRange);
				XYZ xYZ2 = xYZ - (TrackingRegistry.MvSize - XYZ.one);
				while (relevantGroupsInRangeEnumerator.MoveNext())
				{
					TrackingRegistry.TrackingGroup current2 = relevantGroupsInRangeEnumerator.Current;
					if (current2.visionScanId == -1)
					{
						continue;
					}
					XYZ xYZ3 = XYZ.Mul(current2.Position, TrackingRegistry.MvSize);
					XYZ a = xYZ3 - xYZ;
					XYZ b = xYZ2 - xYZ3;
					XYZ a2 = XYZ.Max(a, b, XYZ.zero);
					if (XYZ.SqrMagnitude(a2) <= lineOfSightRange * lineOfSightRange)
					{
						int visionScanId = -2;
						XYZ a3 = XYZ.Min(a, b);
						if (XYZ.SqrMagnitude(a3) <= lineOfSightRange * lineOfSightRange)
						{
							visionScanId = -1;
						}
						if (current2.visionScanId == 0)
						{
							_sharedVision._trackingGroups.Add(current2);
						}
						current2.visionScanId = visionScanId;
					}
				}
			}
		}

		private void RemoveNoLongerDetectedTrackingGroups(int nPreviousGroups)
		{
			if (nPreviousGroups <= 0)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < nPreviousGroups; i++)
			{
				TrackingRegistry.TrackingGroup trackingGroup = _sharedVision._trackingGroups[i];
				if (trackingGroup.visionScanId >= 1)
				{
					trackingGroup.visionScanId = 0;
				}
				else
				{
					_sharedVision._trackingGroups[num++] = trackingGroup;
				}
			}
			if (num < nPreviousGroups)
			{
				if (nPreviousGroups - num < _sharedVision._trackingGroups.Count)
				{
					_sharedVision._trackingGroups.RemoveRange(num, nPreviousGroups - num);
				}
				else
				{
					_sharedVision._trackingGroups.Clear();
				}
			}
		}

		private void RemoveNoLongerDetectedFullGroups()
		{
			int count = _sharedVision._fullGroups.Count;
			if (count <= 0)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				TrackingRegistry.TrackingGroup trackingGroup = _sharedVision._fullGroups[i];
				if (trackingGroup.visionScanId == -1)
				{
					_sharedVision._fullGroups[num++] = trackingGroup;
				}
			}
			if (num < count)
			{
				if (count - num < _sharedVision._fullGroups.Count)
				{
					_sharedVision._fullGroups.RemoveRange(num, count - num);
				}
				else
				{
					_sharedVision._fullGroups.Clear();
				}
			}
		}

		private void RemoveNoLongerDetectedPartialGroups()
		{
			int count = _sharedVision._partialGroups.Count;
			if (count <= 0)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				TrackingRegistry.TrackingGroup trackingGroup = _sharedVision._partialGroups[i];
				if (trackingGroup.visionScanId == -2)
				{
					_sharedVision._partialGroups[num++] = trackingGroup;
				}
			}
			if (num < count)
			{
				if (count - num < _sharedVision._partialGroups.Count)
				{
					_sharedVision._partialGroups.RemoveRange(num, count - num);
				}
				else
				{
					_sharedVision._partialGroups.Clear();
				}
			}
		}

		private void UnflagStillDetectedGroups()
		{
			int count = _sharedVision._fullGroups.Count;
			for (int i = 0; i < count; i++)
			{
				_sharedVision._fullGroups[i].visionScanId = 0;
			}
			int count2 = _sharedVision._partialGroups.Count;
			for (int j = 0; j < count2; j++)
			{
				_sharedVision._partialGroups[j].visionScanId = 0;
			}
		}

		private void AddNewlyDetectedFullAndPartialGroups()
		{
			int count = _sharedVision._trackingGroups.Count;
			for (int i = 0; i < count; i++)
			{
				TrackingRegistry.TrackingGroup trackingGroup = _sharedVision._trackingGroups[i];
				if (trackingGroup.visionScanId == -1)
				{
					_sharedVision._fullGroups.Add(trackingGroup);
				}
				else if (trackingGroup.visionScanId == -2)
				{
					_sharedVision._partialGroups.Add(trackingGroup);
				}
				trackingGroup.visionScanId = 0;
			}
		}
	}

	public struct RefreshActiveCollisionsEnumerator
	{
		private const int PartialGroupsPerFrame = 10;

		private const int FullGroupsPerFrame = 10;

		private SharedFactionVision _sharedVision;

		private int _step;

		private int _iGroup;

		private int _nGroups;

		private int _nPreviousCollisions;

		public void Start(SharedFactionVision sharedVision)
		{
			_sharedVision = sharedVision;
			_step = 1;
			_iGroup = 0;
			_nGroups = 0;
			_nPreviousCollisions = _sharedVision._trackingCollisions.Count;
		}

		public bool MoveNext()
		{
			switch (_step)
			{
			case 1:
				MarkOldCollisions(_nPreviousCollisions);
				_step++;
				_nGroups = _sharedVision._partialGroups.Count;
				break;
			case 2:
			{
				for (int j = 0; j < 10; j++)
				{
					if (_iGroup < _nGroups)
					{
						AssignCollisionsFromPartialGroup(_sharedVision._partialGroups[_iGroup++]);
						continue;
					}
					_iGroup = 0;
					_nGroups = _sharedVision._fullGroups.Count;
					_step++;
					break;
				}
				break;
			}
			case 3:
			{
				for (int i = 0; i < 10; i++)
				{
					if (_iGroup < _nGroups)
					{
						AssignCollisionsFromFullGroup(_sharedVision._fullGroups[_iGroup++]);
						continue;
					}
					_iGroup = 0;
					_step++;
					break;
				}
				break;
			}
			case 4:
				RemoveNoLongerActiveCollisions(ref _nPreviousCollisions);
				_step++;
				break;
			case 5:
				AddNewCollisions(_nPreviousCollisions);
				_step++;
				break;
			default:
				return false;
			}
			return true;
		}

		private void MarkOldCollisions(int nCollisions)
		{
			for (int i = 0; i < nCollisions; i++)
			{
				LocalFactionVision localFactionVision = _sharedVision._trackingCollisions[i];
				if (!localFactionVision.IsNullOrReleased())
				{
					localFactionVision.CollisionTrackingId = 1 + i;
				}
			}
		}

		private void AssignCollisionsFromPartialGroup(TrackingRegistry.TrackingGroup partialGroup)
		{
			List<LocalFactionVision> list = _sharedVision.AllVisions;
			if (partialGroup.count >= 10)
			{
				foreach (LocalFactionVision allVision in _sharedVision.AllVisions)
				{
					if (TrackingRegistry.IsInAtLeastPartialRange(partialGroup, XYZ.FromVector3(allVision.transform.position), allVision.LineOfSightRange))
					{
						_sharedVision._edgeVisions.Add(allVision);
					}
				}
				list = _sharedVision._edgeVisions;
			}
			if ((partialGroup.trackedFactions & _sharedVision.EnemyOrNeutralFactions) != 0)
			{
				List<TrackingRegistry.TrackingEntry> entries = partialGroup.Entries;
				foreach (TrackingRegistry.TrackingEntry item in entries)
				{
					if (((1 << item.faction.ID) & _sharedVision.EnemyOrNeutralFactions) == 0)
					{
						continue;
					}
					LocalFactionVision vision = item.Vision;
					if (vision.IsNullOrReleased() || vision.CollisionTrackingId == -1)
					{
						continue;
					}
					if (vision.CollisionTrackingId >= 1 + _sharedVision._whoSeesMe.Count)
					{
						logger.ErrorFormat("CollisionTrackingId={0}, _whoSeesMe.Count={1}, _collisionSnapshot.count={2}", vision.CollisionTrackingId, _sharedVision._whoSeesMe.Count, _sharedVision._trackingCollisions.Count);
					}
					else
					{
						if (!LocalFactionVision.IsResponsibleForVision(vision))
						{
							continue;
						}
						if (vision.CollisionTrackingId >= 1)
						{
							LocalFactionVision whoSeesMe = _sharedVision._whoSeesMe[vision.CollisionTrackingId - 1];
							if (!whoSeesMe.IsNullOrReleased() && IsInRange(item.GameObject.transform.position, whoSeesMe))
							{
								vision.CollisionTrackingId = -1;
							}
							else if (IsInRange(item.GameObject.transform, list, ref whoSeesMe))
							{
								_sharedVision._whoSeesMe[vision.CollisionTrackingId - 1] = whoSeesMe;
								vision.CollisionTrackingId = -1;
							}
						}
						else
						{
							LocalFactionVision whoSeesMe2 = null;
							if (IsInRange(item.GameObject.transform, list, ref whoSeesMe2))
							{
								_sharedVision._whoSeesMe.Add(whoSeesMe2);
								_sharedVision._trackingCollisions.Add(vision);
							}
						}
					}
				}
			}
			if (list == _sharedVision._edgeVisions)
			{
				_sharedVision._edgeVisions.Clear();
			}
		}

		private void AssignCollisionsFromFullGroup(TrackingRegistry.TrackingGroup fullGroup)
		{
			if ((fullGroup.trackedFactions & _sharedVision.EnemyOrNeutralFactions) == 0)
			{
				return;
			}
			List<TrackingRegistry.TrackingEntry> entries = fullGroup.Entries;
			foreach (TrackingRegistry.TrackingEntry item in entries)
			{
				if (((1 << item.faction.ID) & _sharedVision.EnemyOrNeutralFactions) == 0)
				{
					continue;
				}
				LocalFactionVision vision = item.Vision;
				if (vision.IsNullOrReleased() || vision.CollisionTrackingId == -1)
				{
					continue;
				}
				if (vision.CollisionTrackingId >= 1 + _sharedVision._whoSeesMe.Count)
				{
					logger.ErrorFormat("CollisionTrackingId={0}, _whoSeesMe.Count={1}, _collisionSnapshot.count={2}", vision.CollisionTrackingId, _sharedVision._whoSeesMe.Count, _sharedVision._trackingCollisions.Count);
				}
				else if (LocalFactionVision.IsResponsibleForVision(vision))
				{
					if (vision.CollisionTrackingId >= 1)
					{
						_sharedVision._whoSeesMe[vision.CollisionTrackingId - 1] = null;
						vision.CollisionTrackingId = -1;
					}
					else
					{
						_sharedVision._whoSeesMe.Add(null);
						_sharedVision._trackingCollisions.Add(vision);
					}
				}
			}
		}

		private void RemoveNoLongerActiveCollisions(ref int nPreviousCollisions)
		{
			if (nPreviousCollisions <= 0)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < nPreviousCollisions; i++)
			{
				LocalFactionVision localFactionVision = _sharedVision._trackingCollisions[i];
				if (!localFactionVision.IsNullOrReleased())
				{
					if (localFactionVision.CollisionTrackingId >= 1)
					{
						_sharedVision.RemoveFromRegularTracking(localFactionVision);
					}
					else
					{
						_sharedVision._whoSeesMe[num] = _sharedVision._whoSeesMe[i];
						_sharedVision._trackingCollisions[num] = localFactionVision;
						num++;
					}
					localFactionVision.CollisionTrackingId = 0;
				}
			}
			if (num < nPreviousCollisions)
			{
				if (nPreviousCollisions - num < _sharedVision._trackingCollisions.Count)
				{
					_sharedVision._whoSeesMe.RemoveRange(num, nPreviousCollisions - num);
					_sharedVision._trackingCollisions.RemoveRange(num, nPreviousCollisions - num);
				}
				else
				{
					_sharedVision._whoSeesMe.Clear();
					_sharedVision._trackingCollisions.Clear();
				}
				nPreviousCollisions = num;
			}
		}

		private void AddNewCollisions(int nPreviousCollisions)
		{
			int count = _sharedVision._trackingCollisions.Count;
			for (int i = nPreviousCollisions; i < count; i++)
			{
				LocalFactionVision localFactionVision = _sharedVision._trackingCollisions[i];
				localFactionVision.CollisionTrackingId = 0;
				_sharedVision.AddToRegularTracking(localFactionVision);
			}
		}

		private bool IsInRange(Vector3 target, LocalFactionVision v)
		{
			Vector3 position = v.transform.position;
			int lineOfSightRange = v.LineOfSightRange;
			return (position - target).sqrMagnitude <= (float)(lineOfSightRange * lineOfSightRange);
		}

		private bool IsInRange(Transform t, List<LocalFactionVision> visions, ref LocalFactionVision whoSeesMe)
		{
			Vector3 position = t.position;
			int count = visions.Count;
			for (int i = 0; i < count; i++)
			{
				if (IsInRange(position, visions[i]))
				{
					whoSeesMe = visions[i];
					return true;
				}
			}
			return false;
		}
	}

	private Common _common = SingletonProvider.Get<Common>();

	private static BrixLogger logger = new BrixLogger(typeof(SharedFactionVision).FullName);

	private readonly List<TrackingRegistry.TrackingGroup> _trackingGroups = new List<TrackingRegistry.TrackingGroup>();

	private readonly List<TrackingRegistry.TrackingGroup> _partialGroups = new List<TrackingRegistry.TrackingGroup>();

	private readonly List<TrackingRegistry.TrackingGroup> _fullGroups = new List<TrackingRegistry.TrackingGroup>();

	private readonly List<LocalFactionVision> _trackingCollisions = new List<LocalFactionVision>();

	private readonly List<LocalFactionVision> _whoSeesMe = new List<LocalFactionVision>();

	private readonly List<LocalFactionVision> _edgeVisions = new List<LocalFactionVision>();

	public readonly List<LocalFactionVision> AllVisions = new List<LocalFactionVision>();

	public Faction Faction = Faction.DefaultAllied;

	public int EnemyOrNeutralFactions;

	private const int FogT = 511;

	private const int FogT2 = 426;

	private const int FogT4 = 324;

	private const int FogT8 = 264;

	private const int FogLayers = 9;

	private const int UninitializedId = 0;

	private const int FullDetectionId = -1;

	private const int PartialDetectionId = -2;

	private const int PrevDetectionId = 1;

	protected override void Awake()
	{
		base.Awake();
		_common.SharedVisions.Add(this);
		GameSignals.Connect(AffiliationComponent.AnyObjectChangedFaction, delegate(GameObject g, Faction oldF, Faction newF)
		{
			if (oldF == Faction && g.GetComponent<LocalFactionVision>() != null)
			{
				Remove(g.GetComponent<LocalFactionVision>());
			}
			if (newF == Faction && g.GetComponent<LocalFactionVision>() != null)
			{
				Add(g.GetComponent<LocalFactionVision>());
			}
		});
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate(SharedFactionVision self)
		{
			self.RefreshAlliances();
		}, this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_common.SharedVisions.Remove(this);
	}

	protected override void BrixUpdate()
	{
		if (!_common.Enumerator.MoveNext())
		{
			_common.Enumerator.Start();
		}
	}

	public void Add(LocalFactionVision vision)
	{
		if (vision.LineOfSightRange != 0)
		{
			vision.CollisionTrackingId = 0;
			AllVisions.Add(vision);
		}
	}

	public void Remove(LocalFactionVision vision)
	{
		AllVisions.Remove(vision);
	}

	private void AddToRegularTracking(LocalFactionVision v)
	{
		if (v.FactionsSeeingMe.Add(Faction.ID))
		{
			LocalFactionVision.OnUnitEnterSightRange(Faction, v);
		}
	}

	private void RemoveFromRegularTracking(LocalFactionVision v)
	{
		if (v.FactionsSeeingMe.Remove(Faction.ID))
		{
			LocalFactionVision.OnUnitLeaveSightRange(Faction, v);
		}
	}

	private void RefreshAlliances()
	{
		EnemyOrNeutralFactions = 0;
		foreach (Faction enemyOrNeutralFaction in Faction.EnemyOrNeutralFactions)
		{
			EnemyOrNeutralFactions |= 1 << enemyOrNeutralFaction.ID;
		}
	}
}
