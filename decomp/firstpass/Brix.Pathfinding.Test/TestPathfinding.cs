using System.Collections;
using System.Collections.Generic;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Engine.Blocks;
using Brix.Engine.Nature;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components.Volumes;
using Brix.Legacy;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Pathfinding.Test;

public class TestPathfinding : NetworkBehaviour
{
	public class ObstacleSet
	{
	}

	private static class DicoWrapper<TKey, TValue>
	{
		public static void DicoRemove(Dictionary<TKey, HashSet<TValue>> dico, TKey key, TValue value)
		{
			dico.GetValueOrDefaultThenRemove(key, value, delegate(HashSet<TValue> h)
			{
				CollectionPool.Release(ref h);
			});
		}

		public static void DicoAdd(Dictionary<TKey, HashSet<TValue>> dico, TKey key, TValue value)
		{
			dico.GetValueOrCreateThenAdd(key, value, CollectionPool.RequestHashSet<TValue>);
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(TestPathfinding).FullName);

	public static TestPathfinding Instance;

	public static GameSignals.GameSignal InitializedSignal = GameSignals.CreateStaticSignal();

	public Pathfinding pathfinding;

	public PathDrawer pathDrawer;

	public bool read;

	public bool autoInit;

	public Dictionary<XYZ, TerrainPathNode> TerrainNodes = new Dictionary<XYZ, TerrainPathNode>();

	public HashSet<XYZ> DirtyVoxels = new HashSet<XYZ>();

	public HashSet<XYZ> WalkableVolumeVoxels = new HashSet<XYZ>();

	public HashSet<XYZ> QueuedVoxels = new HashSet<XYZ>();

	public float StartedWaitingForVoxelEngine;

	public float TimeLimitBeforeForceEngineUpdate = 5f;

	public Dictionary<PathNode, ObstacleSet> PathObstacles = new Dictionary<PathNode, ObstacleSet>();

	public Dictionary<IVolume, HashSet<XYZ>> Obstacles = new Dictionary<IVolume, HashSet<XYZ>>();

	public Dictionary<XYZ, HashSet<IVolume>> ObstaclePositions = new Dictionary<XYZ, HashSet<IVolume>>();

	public Dictionary<VoxelPresence, XYZ> VoxelPresences = new Dictionary<VoxelPresence, XYZ>();

	public Dictionary<XYZ, HashSet<VoxelPresence>> VoxelPresencePositions = new Dictionary<XYZ, HashSet<VoxelPresence>>();

	public Dictionary<XYZ, HashSet<VoxelPresence>> EarlyVoxelPresencePositions = new Dictionary<XYZ, HashSet<VoxelPresence>>();

	public Dictionary<IVolume, HashSet<XYZ>> Accesses = new Dictionary<IVolume, HashSet<XYZ>>();

	public Dictionary<XYZ, HashSet<IVolume>> AccessPositions = new Dictionary<XYZ, HashSet<IVolume>>();

	public Dictionary<IVolume, int> AllParticipantVolumes = new Dictionary<IVolume, int>();

	protected Signal Disconnected = new Signal();

	public TestTerrainLod terrainLod;

	public const int StepDefaultWeight = 10;

	public const int StepDownPenalty = 160;

	public const int ClimbUpPenalty = 240;

	public const int BreakBlockPenalty = 0;

	public const int BreakBlockPenaltyPer100Hp = 70;

	public const int LadderDownPenalty = 100;

	public const int MinClimbUpLevelDifference = 4;

	public const int MaxClimbUpLevelDifference = 10;

	public const int MinStepDownLevelDifference = 4;

	public const int MaxStepDownLevelDifference = 10;

	public const int InitPointsPerStep = 10000;

	private bool _initialized;

	private static readonly VPropertyMask ObstacleProperties = VPropertyMask.obstacle | VPropertyMask.character;

	private static readonly HashSet<IVolume> NoVolume = new HashSet<IVolume>();

	public bool Initialized => _initialized;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		ConnectToEarlySignals();
		GameSignals.LateConnectOnce(GameSignals.TerrainReady, delegate
		{
			read = autoInit;
		});
	}

	private void OnDestroy()
	{
		Disconnected.Invoke();
		Instance = null;
	}

	private void Update()
	{
		if (read)
		{
			read = false;
			Read();
		}
		if (!_initialized || (DirtyVoxels.Count <= 0 && QueuedVoxels.Count <= 0))
		{
			return;
		}
		bool flag = Brix.Engine.Voxel.Engine.IsWorking();
		if (flag)
		{
			if (StartedWaitingForVoxelEngine <= 0f)
			{
				StartedWaitingForVoxelEngine = Time.time;
			}
			if (Time.time >= StartedWaitingForVoxelEngine + TimeLimitBeforeForceEngineUpdate)
			{
				StartedWaitingForVoxelEngine = 0f;
				Brix.Engine.Voxel.Engine.FinishWork();
				flag = false;
			}
		}
		else
		{
			StartedWaitingForVoxelEngine = 0f;
		}
		if (flag)
		{
			if (DirtyVoxels.Count > 0)
			{
				DirtyVoxels.Slinq().ForEach(delegate(XYZ p, TestPathfinding self)
				{
					TerrainPathNode valueOrDefault = self.TerrainNodes.GetValueOrDefault(p);
					if (valueOrDefault != null && !valueOrDefault.IsEmpty && !VoxelPresencePositions.ContainsKey(valueOrDefault.RepresentativePosition))
					{
						valueOrDefault.Clear();
					}
				}, this);
				DirtyVoxels.Slinq().AddTo(QueuedVoxels);
			}
		}
		else
		{
			if (QueuedVoxels.Count > 0)
			{
				QueuedVoxels.Slinq().AddTo(DirtyVoxels);
				QueuedVoxels.Clear();
			}
			Rebuild(DirtyVoxels);
		}
		DirtyVoxels.Clear();
	}

	public void Read()
	{
		Brix.Engine.Abstract.Neo.DoAll(Read_Coroutine());
	}

	public IEnumerator Read_Coroutine()
	{
		yield return ReadAllPathNodes();
		yield return InitPathfinding();
		InitVolumePositions();
		ConnectSignals();
		_initialized = true;
		GameSignals.Invoke(InitializedSignal);
		yield return GameClock.WaitForEndOfFrame;
	}

	public int EstimateLoadingSteps(int visibleVoxelCount)
	{
		return visibleVoxelCount / 10000 * 2;
	}

	private void ConnectToEarlySignals()
	{
		Disconnected.Invoke();
		VoxelPresence.VoxelBecameOccupied.Connect(EarlyAddVoxelPresence).DisconnectWhen(Disconnected);
		VoxelPresence.VoxelBecameFree.Connect(EarlyRemoveVoxelPresence).DisconnectWhen(Disconnected);
		VolumeRegistrator.Instance.RegisterVolumeSignal.Connect(EarlyRegisterVolume).DisconnectWhen(Disconnected);
		VolumeRegistrator.Instance.UnregisterVolumeSignal.Connect(EarlyUnregisterVolume).DisconnectWhen(Disconnected);
	}

	private void ConnectSignals()
	{
		Disconnected.Invoke();
		GameSignals.Connect(GameSignals.BeforeBlockRemoved, delegate(BlockData b)
		{
			DirtyVoxels.UnionWith(GetBlockOccupiedPoints(b));
		}).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.TerrainRemoved, delegate(XYZ p)
		{
			DirtyVoxels.Add(p);
		}).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.BlockTypeChanged, delegate(XYZ p)
		{
			DirtyVoxels.Add(p);
		}).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.TerrainChanged, delegate(XYZ p)
		{
			DirtyVoxels.Add(p);
		}).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.VoxelChanged, delegate(XYZ p)
		{
			DirtyVoxels.Add(p);
		}).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.TreeChanged, NatureBlockerChanged).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.BouldersChanged, NatureBlockerChanged).DisconnectWhen(Disconnected);
		GameSignals.Connect(GameSignals.MineralsChanged, NatureBlockerChanged).DisconnectWhen(Disconnected);
		VolumeRegistrator.Instance.VolumeDataAddedSignal.Connect(AddToVoxelNeedingRevalidation).DisconnectWhen(Disconnected);
		VolumeRegistrator.Instance.VolumeDataRemovedSignal.Connect(RemoveVoxelNeedingRevalidation).DisconnectWhen(Disconnected);
		VolumeRegistrator.Instance.RegisterVolumeSignal.Connect(RegisterVolume).DisconnectWhen(Disconnected);
		VolumeRegistrator.Instance.UnregisterVolumeSignal.Connect(UnregisterVolume).DisconnectWhen(Disconnected);
		VoxelPresence.VoxelBecameOccupied.Connect(AddVoxelPresence).DisconnectWhen(Disconnected);
		VoxelPresence.VoxelBecameFree.Connect(RemoveVoxelPresence).DisconnectWhen(Disconnected);
	}

	private bool IsRelevantAccessVolume(IVolume volume)
	{
		return volume is Volume;
	}

	private void RegisterVolume(IVolume volume)
	{
		if (volume == null)
		{
			return;
		}
		HashSet<XYZ> hashSet = Obstacles.GetValueOrDefault(volume);
		if (hashSet != null)
		{
			logger.WarnFormat("Registring volume twice at {0}", volume.Position);
			hashSet.Clear();
		}
		else
		{
			Obstacles.Add(volume, hashSet = CollectionPool.RequestHashSet<XYZ>());
		}
		foreach (XYZ @case in volume.GetCases(ObstacleProperties))
		{
			XYZ item = @case;
			hashSet.Add(item);
		}
		foreach (XYZ item3 in hashSet)
		{
			AddObstacle(item3, volume, ObstaclePositions);
		}
		if (!IsRelevantAccessVolume(volume))
		{
			return;
		}
		hashSet = Accesses.GetValueOrDefault(volume);
		if (hashSet != null)
		{
			logger.WarnFormat("Registring volume twice at {0}", volume.Position);
			hashSet.Clear();
		}
		else
		{
			Accesses.Add(volume, hashSet = CollectionPool.RequestHashSet<XYZ>());
		}
		foreach (XYZ case2 in volume.GetCases(VProperty.access))
		{
			XYZ item2 = case2;
			hashSet.Add(item2);
		}
		foreach (XYZ item4 in hashSet)
		{
			AddAccess(item4, volume, AccessPositions);
		}
	}

	private void UnregisterVolume(IVolume volume)
	{
		if (volume == null)
		{
			return;
		}
		HashSet<XYZ> value = Obstacles.GetAndRemove(volume).Value;
		if (value != null)
		{
			foreach (XYZ item in value)
			{
				RemoveObstacle(item, volume, ObstaclePositions);
			}
			value.Dispose();
		}
		else
		{
			logger.WarnFormat("Unregistring unknown volume {0} at {1}", volume.Name, volume.Position);
		}
		if (!IsRelevantAccessVolume(volume))
		{
			return;
		}
		value = Accesses.GetAndRemove(volume).Value;
		if (value != null)
		{
			foreach (XYZ item2 in value)
			{
				RemoveAccess(item2, volume, AccessPositions);
			}
			value.Dispose();
		}
		else
		{
			logger.WarnFormat("Unregistring unknown volume {0} at {1}", volume.Name, volume.Position);
		}
	}

	private void EarlyRegisterVolume(IVolume volume)
	{
		if (volume == null)
		{
			return;
		}
		HashSet<XYZ> hashSet = Obstacles.GetValueOrDefault(volume);
		if (hashSet != null)
		{
			logger.ErrorFormat("Registring volume twice at {0}", volume.Position);
			hashSet.Clear();
		}
		else
		{
			Obstacles.Add(volume, hashSet = CollectionPool.RequestHashSet<XYZ>());
		}
		foreach (XYZ @case in volume.GetCases(ObstacleProperties))
		{
			XYZ item = @case;
			hashSet.Add(item);
		}
		if (!IsRelevantAccessVolume(volume))
		{
			return;
		}
		hashSet = Accesses.GetValueOrDefault(volume);
		if (hashSet != null)
		{
			logger.WarnFormat("Registring volume twice at {0}", volume.Position);
			hashSet.Clear();
		}
		else
		{
			Accesses.Add(volume, hashSet = CollectionPool.RequestHashSet<XYZ>());
		}
		foreach (XYZ case2 in volume.GetCases(VProperty.access))
		{
			XYZ item2 = case2;
			hashSet.Add(item2);
		}
	}

	private void EarlyUnregisterVolume(IVolume volume)
	{
		if (volume == null)
		{
			return;
		}
		HashSet<XYZ> value = Obstacles.GetAndRemove(volume).Value;
		if (value != null)
		{
			value.Dispose();
		}
		else
		{
			logger.ErrorFormat("Unregistring unknown volume {0} at {1}", volume.Name, volume.Position);
		}
		if (IsRelevantAccessVolume(volume))
		{
			value = Accesses.GetAndRemove(volume).Value;
			if (value != null)
			{
				value.Dispose();
			}
			else
			{
				logger.WarnFormat("Unregistring unknown volume {0} at {1}", volume.Name, volume.Position);
			}
		}
	}

	private void AddToVoxelNeedingRevalidation(BaseVolume vol)
	{
		foreach (XYZ @case in vol.GetCases(VPropertyMask.walkable))
		{
			if (WalkableVolumeVoxels.Add(@case))
			{
				DirtyVoxels.Add(@case);
			}
		}
	}

	private void RemoveVoxelNeedingRevalidation(BaseVolume vol)
	{
		foreach (XYZ @case in vol.GetCases(VPropertyMask.walkable))
		{
			if (WalkableVolumeVoxels.Remove(@case))
			{
				DirtyVoxels.Add(@case);
			}
		}
	}

	private void NatureBlockerChanged(XYZ position)
	{
		DirtyVoxels.UnionWith(Calcule.CubeConcentrique(position, 1));
	}

	private void InitVolumePositions()
	{
		ObstaclePositions.Clear();
		foreach (KeyValuePair<IVolume, HashSet<XYZ>> obstacle in Obstacles)
		{
			foreach (XYZ item in obstacle.Value)
			{
				AddObstacle(item, obstacle.Key, ObstaclePositions);
			}
		}
		VoxelPresences.Clear();
		VoxelPresencePositions.Clear();
		foreach (KeyValuePair<XYZ, HashSet<VoxelPresence>> earlyVoxelPresencePosition in EarlyVoxelPresencePositions)
		{
			foreach (VoxelPresence item2 in earlyVoxelPresencePosition.Value)
			{
				AddVoxelPresence(earlyVoxelPresencePosition.Key, item2);
			}
		}
		AccessPositions.Clear();
		foreach (KeyValuePair<IVolume, HashSet<XYZ>> access in Accesses)
		{
			foreach (XYZ item3 in access.Value)
			{
				AddAccess(item3, access.Key, AccessPositions);
			}
		}
	}

	public HashSet<IVolume> GetObstaclesAt(XYZ p)
	{
		return ObstaclePositions.GetValueOrDefault(p) ?? NoVolume;
	}

	private int CountObstaclesAt(XYZ p)
	{
		return DicoCount(ObstaclePositions, p) + DicoCount(VoxelPresencePositions, p);
	}

	private int CountAccessesAt(XYZ p)
	{
		return DicoCount(AccessPositions, p);
	}

	private void EarlyAddVoxelPresence(XYZ p, VoxelPresence presence)
	{
		DicoAdd(EarlyVoxelPresencePositions, p, presence);
	}

	private void EarlyRemoveVoxelPresence(XYZ p, VoxelPresence presence)
	{
		DicoRemove(EarlyVoxelPresencePositions, p, presence);
	}

	private void AddVoxelPresence(XYZ p, VoxelPresence presence)
	{
		AddObstacle(p, presence, VoxelPresencePositions);
	}

	private void RemoveVoxelPresence(XYZ p, VoxelPresence presence)
	{
		RemoveObstacle(p, presence, VoxelPresencePositions);
	}

	private void AddObstacle<TValue>(XYZ p, TValue value, Dictionary<XYZ, HashSet<TValue>> dico)
	{
		terrainLod.GetPathNode(p).ObstacleCount++;
		DicoAdd(dico, p, value);
	}

	private void RemoveObstacle<TValue>(XYZ p, TValue value, Dictionary<XYZ, HashSet<TValue>> dico)
	{
		terrainLod.GetPathNode(p).ObstacleCount--;
		DicoRemove(dico, p, value);
	}

	private void AddAccess(XYZ p, IVolume volume, Dictionary<XYZ, HashSet<IVolume>> dico)
	{
		TerrainPathNode valueOrDefault = TerrainNodes.GetValueOrDefault(p);
		if (valueOrDefault != null)
		{
			IncreaseAccessCount(volume);
			valueOrDefault.AccessCount++;
		}
		DicoAdd(dico, p, volume);
	}

	private void RemoveAccess(XYZ p, IVolume volume, Dictionary<XYZ, HashSet<IVolume>> dico)
	{
		TerrainPathNode valueOrDefault = TerrainNodes.GetValueOrDefault(p);
		if (valueOrDefault != null)
		{
			DecreaseAccessCount(volume);
			valueOrDefault.AccessCount--;
		}
		DicoRemove(dico, p, volume);
	}

	private void IncreaseAccessCount(XYZ p)
	{
		HashSet<IVolume> valueOrDefault = AccessPositions.GetValueOrDefault(p);
		if (valueOrDefault == null)
		{
			return;
		}
		foreach (IVolume item in valueOrDefault)
		{
			IncreaseAccessCount(item);
		}
	}

	private void DecreaseAccessCount(XYZ p)
	{
		HashSet<IVolume> valueOrDefault = AccessPositions.GetValueOrDefault(p);
		if (valueOrDefault == null)
		{
			return;
		}
		foreach (IVolume item in valueOrDefault)
		{
			DecreaseAccessCount(item);
		}
	}

	private void DecreaseAccessCount(IVolume volume)
	{
		int num = AllParticipantVolumes.GetValueOrDefault(volume) - 1;
		if (num > 0)
		{
			AllParticipantVolumes[volume] = num;
		}
		else
		{
			AllParticipantVolumes.Remove(volume);
		}
	}

	private void IncreaseAccessCount(IVolume volume)
	{
		AllParticipantVolumes[volume] = AllParticipantVolumes.GetValueOrDefault(volume) + 1;
	}

	private void DicoAdd<TKey, TValue>(Dictionary<TKey, HashSet<TValue>> dico, TKey key, TValue value)
	{
		DicoWrapper<TKey, TValue>.DicoAdd(dico, key, value);
	}

	private void DicoRemove<TKey, TValue>(Dictionary<TKey, HashSet<TValue>> dico, TKey key, TValue value)
	{
		DicoWrapper<TKey, TValue>.DicoRemove(dico, key, value);
	}

	private int DicoCount<TKey, TValue>(Dictionary<TKey, HashSet<TValue>> dico, TKey key)
	{
		return dico.GetValueOrDefault(key)?.Count ?? 0;
	}

	private static VolumePropertyRequest GetBlockOccupiedPoints(IBlockData b)
	{
		if (b.BlockInfo.Type == BlockType.Block)
		{
			return b.Volume.GetCases(VProperty.occupy);
		}
		return VolumePropertyRequest.Empty;
	}

	private IEnumerator ReadAllPathNodes()
	{
		TerrainNodes.Clear();
		DirtyVoxels.Clear();
		Slinq<XYZ, IEnumerableContext<XYZ>> terrainVoxels = Brix.Engine.Voxel.Engine.GetVisibleVoxelPositions().Slinq();
		Slinq<XYZ, SelectSlinqContext<XYZ, SelectContext<XYZ, XYZ, IListContext<XYZ>, XYZ>, RootBlockData, HashSetSlinqContext<RootBlockData>>> blockVoxels = BrixSingleton<BlockEngine>.Instance.RootBlocks.Slinq().SelectMany((RootBlockData rb) => GetBlockOccupiedPoints(rb).Slinq());
		IEnumerable<XYZ> trees = TreeRegistry.Instance.ListAllTreePosition();
		List<XYZ> natures = CollectionPool.RequestList<XYZ>();
		NatureRegistry.GetAllBlockerPositions(ref natures);
		XYZ[] obstacles = new XYZ[0];
		HashSet<XYZ> positives = terrainVoxels.Concat(blockVoxels).Concat(WalkableVolumeVoxels.Slinq()).ToHashSet();
		HashSet<XYZ> negatives = trees.Slinq().Concat(obstacles.Slinq()).Concat(natures.Slinq())
			.ToPooledHashSet();
		natures.Dispose();
		positives.ExceptWith(negatives);
		negatives.Dispose();
		HashSet<XYZ> points = positives;
		yield return GameClock.WaitForEndOfFrame;
		int steps = 0;
		foreach (XYZ p in points)
		{
			int num;
			steps = (num = steps + 1);
			if (num == 10000)
			{
				GameSignals.Invoke(GameSignals.LoadingProgress);
				yield return GameClock.WaitForEndOfFrame;
				steps = 0;
			}
			int n = Brix.Engine.Voxel.GetNiveaux(p);
			if ((n & 0x3333) != 13107)
			{
				if (Brix.Engine.Voxel.IsEmpty(p + XYZ.up) && !IsAWalkableObstacle(p + XYZ.up))
				{
					TerrainNodes[p] = new TerrainPathNode(p, n);
					IncreaseAccessCount(p);
					continue;
				}
			}
			else
			{
				if (Brix.Engine.Voxel.IsBlock(p) && !IsAWalkableObstacle(p))
				{
					TerrainNodes[p] = new TerrainPathNode(p, n);
				}
				XYZ xYZ = p;
				xYZ.y++;
				if (!TerrainNodes.ContainsKey(xYZ))
				{
					n = Brix.Engine.Voxel.GetNiveaux(xYZ);
					if ((n & 0x3333) != 13107 && Brix.Engine.Voxel.IsEmpty(xYZ + XYZ.up) && !IsAWalkableObstacle(xYZ + XYZ.up))
					{
						TerrainNodes[xYZ] = new TerrainPathNode(xYZ, n);
						IncreaseAccessCount(p);
						continue;
					}
				}
			}
			if (IsAWalkableObstacle(p))
			{
				n = VolumeRegistry.VolumesWithPropertyAt(p, VProperty.walkable).Slinq().First()
					.PassThrough.GetNiveauxAt(p);
				TerrainNodes[p] = new TerrainPathNode(p, n);
				IncreaseAccessCount(p);
			}
		}
	}

	public static bool IsSurfaceVoxel(XYZ p)
	{
		return Brix.Engine.Voxel.IsOpenAndWalkable(p);
	}

	public static bool IsAPermanentObstacle(XYZ p)
	{
		return VolumeRegistry.HasVolumesWithPropertyAt(p, VProperty.obstacle);
	}

	public bool IsAWalkableObstacle(XYZ p)
	{
		return WalkableVolumeVoxels.Contains(p);
	}

	public static bool IsNatureBlocker(XYZ p)
	{
		return TreeRegistry.Instance.HasTree(p) || NatureRegistry.HasNatureBlockerAt(p);
	}

	public static bool IsSemiWalkableNatureBlocker(XYZ p)
	{
		return TreeRegistry.Instance.IsNearATree(p);
	}

	public static bool IsAlmostNearATree(XYZ p)
	{
		return true;
	}

	private IEnumerator InitPathfinding()
	{
		pathfinding.CreateLods();
		terrainLod = new TestTerrainLod();
		pathfinding.pathLods[0] = terrainLod;
		terrainLod.coarserLod = pathfinding.GetPathLod(1);
		terrainLod.pathNodes = TerrainNodes;
		yield return GameClock.WaitForEndOfFrame;
		int steps = 0;
		foreach (TerrainPathNode terrainNode in TerrainNodes.Values)
		{
			int num;
			steps = (num = steps + 1);
			if (num == 10000)
			{
				GameSignals.Invoke(GameSignals.LoadingProgress);
				yield return GameClock.WaitForEndOfFrame;
				steps = 0;
			}
			RebuildLod0(terrainNode);
		}
		yield return pathfinding.InitHigherLods_Coroutine();
	}

	private void Rebuild(HashSet<XYZ> dirtyVoxels)
	{
		RefreshLod(dirtyVoxels);
	}

	private void RefreshLod(HashSet<XYZ> dirtyVoxels)
	{
		HashSet<XYZ> hashSet = CollectionPool.RequestHashSet<XYZ>();
		foreach (XYZ dirtyVoxel in dirtyVoxels)
		{
			foreach (XYZ item in Neighbors(dirtyVoxel))
			{
				hashSet.Add(item);
			}
		}
		RebuildLod0(hashSet);
		if (pathDrawer != null)
		{
			pathDrawer.forceDirty = true;
		}
		PathLod pathLod = pathfinding.GetPathLod0();
		while (pathLod.CoarserLod != PathLod.Empty)
		{
			pathLod = pathLod.CoarserLod;
			HashSet<XYZ> hashSet2 = (from q in hashSet.Slinq()
				select q >> 1).ToPooledHashSet();
			hashSet.Dispose();
			hashSet = hashSet2;
			RebuildHigherLod((PathHigherLod)pathLod, hashSet);
		}
		hashSet.Dispose();
	}

	private void RebuildLod0(HashSet<XYZ> points)
	{
		foreach (XYZ point in points)
		{
			TerrainPathNode valueOrDefault = TerrainNodes.GetValueOrDefault(point);
			if (valueOrDefault != null && !valueOrDefault.IsEmpty)
			{
				valueOrDefault.Clear();
			}
			if ((IsSurfaceVoxel(point) || Brix.Engine.Voxel.IsBlock(point)) && !IsNatureBlocker(point) && !IsAWalkableObstacle(point + XYZ.up))
			{
				int niveaux = Brix.Engine.Voxel.GetNiveaux(point);
				TerrainNodes[point] = new TerrainPathNode(point, niveaux);
				if (valueOrDefault == null)
				{
					IncreaseAccessCount(point);
				}
			}
			else if (WalkableVolumeVoxels.Contains(point))
			{
				int niveauxAt = VolumeRegistry.VolumesWithPropertyAt(point, VProperty.walkable).Slinq().First()
					.PassThrough.GetNiveauxAt(point);
				TerrainNodes[point] = new TerrainPathNode(point, niveauxAt);
				if (valueOrDefault == null)
				{
					IncreaseAccessCount(point);
				}
			}
			else
			{
				TerrainNodes.Remove(point);
				if (valueOrDefault != null)
				{
					DecreaseAccessCount(point);
				}
			}
		}
		HashSet<XYZ> hashSet = CollectionPool.RequestHashSet<XYZ>();
		foreach (XYZ point2 in points)
		{
			foreach (XYZ item in Neighbors(point2))
			{
				hashSet.Add(item);
			}
		}
		foreach (XYZ item2 in hashSet)
		{
			TerrainPathNode valueOrDefault2 = TerrainNodes.GetValueOrDefault(item2);
			if (valueOrDefault2 != null)
			{
				RebuildLod0(valueOrDefault2);
				valueOrDefault2.ObstacleCount = CountObstaclesAt(valueOrDefault2.position);
				valueOrDefault2.AccessCount = CountAccessesAt(valueOrDefault2.position);
			}
		}
		hashSet.Dispose();
	}

	private void RebuildLod0(TerrainPathNode terrainNode)
	{
		terrainNode.Init(terrainLod);
	}

	private void RebuildHigherLod(PathHigherLod pathLod, HashSet<XYZ> points)
	{
		foreach (XYZ point in points)
		{
			pathLod.RefreshAt(point);
		}
		PathBranchIterator iterator = pathLod.GetIterator();
		PathLod finerLod = pathLod.FinerLod;
		PathBranchIterator iterator2 = finerLod.GetIterator();
		HashSet<XYZ> hashSet = CollectionPool.RequestHashSet<XYZ>();
		foreach (XYZ point2 in points)
		{
			foreach (XYZ item in Neighbors(point2))
			{
				hashSet.Add(item);
			}
		}
		foreach (XYZ item2 in hashSet)
		{
			foreach (PathLodNode item3 in pathLod.GetAllPathLodNodesAt(item2))
			{
				item3.InitChildren(finerLod, iterator2);
				item3.ObstacleCount = (from child in iterator.GetChildren(item3).Slinq()
					select child.Node.ObstacleCount).Sum();
				item3.AccessCount = (from child in iterator.GetChildren(item3).Slinq()
					select child.Node.AccessCount).Sum();
			}
		}
		hashSet.Dispose();
	}

	private static Calcule.VolumeEnumerable Neighbors(XYZ p)
	{
		return Calcule.VolumeBetween(p - XYZ.one, p + XYZ.one);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
