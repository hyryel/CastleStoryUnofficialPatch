using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Components;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.Tracking;

public class TrackingRegistry : BrixSingleton<TrackingRegistry>
{
	public class TrackingEntry : IPoolable, IResetable, IDisposable
	{
		public static TrackingEntry Empty = new TrackingEntry();

		public IGameComponent gameComponent;

		public Faction faction;

		public TrackingLayer Layers;

		public TrackingGroup TrackingGroup;

		public int TrackingGroupIndex = -1;

		public GameObject GameObject => (gameComponent == null) ? null : gameComponent.GameObject;

		public LocalFactionVision Vision => (gameComponent == null) ? null : gameComponent.vision;

		public static TrackingEntry Request(IGameComponent gameComponent)
		{
			TrackingEntry trackingEntry = ObjectPool<TrackingEntry>.Reserve();
			trackingEntry.gameComponent = gameComponent;
			return trackingEntry;
		}

		public void Dispose()
		{
			ObjectPool<TrackingEntry>.Release(this);
		}

		public void ResetObject()
		{
			gameComponent = null;
			faction = null;
			Layers = (TrackingLayer)0;
			TrackingGroup = null;
			TrackingGroupIndex = -1;
		}
	}

	public struct GroupPositionsInRangeEnumerator : IEnumerator<XYZ>, IDisposable, IEnumerator
	{
		private Calcule.VolumeEnumerator _volumeEnumerator;

		private readonly int mvRadius2;

		private readonly XYZ mvPosition;

		object IEnumerator.Current => Current;

		public XYZ Current => _volumeEnumerator.Current;

		public GroupPositionsInRangeEnumerator(XYZ position, int radius)
		{
			this = default(GroupPositionsInRangeEnumerator);
			int num = Calcule.Ceil(radius, 16);
			mvRadius2 = num * num;
			mvPosition = XYZ.Div(position, MvSize);
			_volumeEnumerator = new Calcule.VolumeEnumerator(mvPosition - XYZ.one * num, mvPosition + XYZ.one * num);
		}

		public bool MoveNext()
		{
			while (_volumeEnumerator.MoveNext())
			{
				if (XYZ.SqrMagnitude(_volumeEnumerator.Current - mvPosition) <= mvRadius2)
				{
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
			_volumeEnumerator.MoveNext();
		}

		public void Dispose()
		{
			_volumeEnumerator.Dispose();
		}
	}

	public struct GroupPositionsEnumerator<TEnumerator> : IEnumerator<XYZ>, IDisposable, IEnumerator where TEnumerator : struct, IEnumerator<TrackingGroup>
	{
		private TEnumerator _groupEnumerator;

		object IEnumerator.Current => Current;

		public XYZ Current => _groupEnumerator.Current.Position;

		public GroupPositionsEnumerator(TEnumerator groupEnumerator)
		{
			_groupEnumerator = groupEnumerator;
		}

		public bool MoveNext()
		{
			return _groupEnumerator.MoveNext();
		}

		public void Reset()
		{
			_groupEnumerator.MoveNext();
		}

		public void Dispose()
		{
			_groupEnumerator.Dispose();
		}
	}

	public struct GroupsInDenseRangeEnumerator : IEnumerator<TrackingGroup>, IDisposable, IEnumerator
	{
		private GroupPositionsInRangeEnumerator _positionsEnumerator;

		object IEnumerator.Current => Current;

		public TrackingGroup Current { get; set; }

		public GroupsInDenseRangeEnumerator(XYZ position, int radius)
		{
			this = default(GroupsInDenseRangeEnumerator);
			_positionsEnumerator = new GroupPositionsInRangeEnumerator(position, radius);
		}

		public bool MoveNext()
		{
			while (_positionsEnumerator.MoveNext())
			{
				XYZ current = _positionsEnumerator.Current;
				if (BrixSingleton<TrackingRegistry>.Instance._groups.TryGetValue(current, out var value))
				{
					Current = value;
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
			_positionsEnumerator.Reset();
		}

		public void Dispose()
		{
			_positionsEnumerator.Dispose();
		}
	}

	public struct GroupsInSparseRangeEnumerator : IEnumerator<TrackingGroup>, IDisposable, IEnumerator
	{
		private Dictionary<XYZ, TrackingGroup>.Enumerator _groupsEnumerator;

		private int _radius2;

		private readonly XYZ _position;

		private readonly XYZ _flippedPosition;

		object IEnumerator.Current => Current;

		public TrackingGroup Current { get; set; }

		public GroupsInSparseRangeEnumerator(XYZ position, int radius)
		{
			this = default(GroupsInSparseRangeEnumerator);
			_groupsEnumerator = BrixSingleton<TrackingRegistry>.Instance._groups.GetEnumerator();
			_radius2 = radius * radius;
			_position = position;
			_flippedPosition = position - (MvSize - XYZ.one);
		}

		public bool MoveNext()
		{
			while (_groupsEnumerator.MoveNext())
			{
				KeyValuePair<XYZ, TrackingGroup> current = _groupsEnumerator.Current;
				XYZ xYZ = XYZ.Mul(current.Key, MvSize);
				XYZ a = xYZ - _position;
				XYZ b = _flippedPosition - xYZ;
				XYZ a2 = XYZ.Max(a, b, XYZ.zero);
				if (XYZ.SqrMagnitude(a2) <= _radius2)
				{
					Current = current.Value;
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
			_groupsEnumerator.Dispose();
			_groupsEnumerator = BrixSingleton<TrackingRegistry>.Instance._groups.GetEnumerator();
		}

		public void Dispose()
		{
			_groupsEnumerator.Dispose();
		}
	}

	public struct RelevantXYZsInRangeEnumerator : IEnumerator<XYZ>, IDisposable, IEnumerator
	{
		private readonly IEnumerator<XYZ> _e;

		XYZ IEnumerator<XYZ>.Current => Current;

		object IEnumerator.Current => Current;

		public XYZ Current => _e.Current;

		public RelevantXYZsInRangeEnumerator(XYZ position, int radius, bool includeEmpty = false)
		{
			if (includeEmpty)
			{
				_e = PooledBoxedEnumerator<XYZ, GroupPositionsInRangeEnumerator>.Request();
			}
			else if (BrixSingleton<TrackingRegistry>.Instance._groups.Count >= radius * radius * radius * 20480)
			{
				_e = PooledBoxedEnumerator<XYZ, GroupPositionsEnumerator<GroupsInDenseRangeEnumerator>>.Request(new GroupPositionsEnumerator<GroupsInDenseRangeEnumerator>(new GroupsInDenseRangeEnumerator(position, radius)));
			}
			else
			{
				_e = PooledBoxedEnumerator<XYZ, GroupPositionsEnumerator<GroupsInSparseRangeEnumerator>>.Request(new GroupPositionsEnumerator<GroupsInSparseRangeEnumerator>(new GroupsInSparseRangeEnumerator(position, radius)));
			}
		}

		public bool MoveNext()
		{
			return _e.MoveNext();
		}

		public void Reset()
		{
			_e.MoveNext();
		}

		public void Dispose()
		{
			_e.Dispose();
		}
	}

	public struct RelevantGroupsInRangeEnumerator : IEnumerator<TrackingGroup>, IDisposable, IEnumerator
	{
		private readonly IEnumerator<TrackingGroup> _e;

		object IEnumerator.Current => Current;

		public TrackingGroup Current => _e.Current;

		public RelevantGroupsInRangeEnumerator(XYZ position, int radius)
		{
			if (BrixSingleton<TrackingRegistry>.Instance._groups.Count >= radius * radius * radius * 20480)
			{
				_e = PooledBoxedEnumerator<TrackingGroup, GroupsInDenseRangeEnumerator>.Request(new GroupsInDenseRangeEnumerator(position, radius));
			}
			else
			{
				_e = PooledBoxedEnumerator<TrackingGroup, GroupsInSparseRangeEnumerator>.Request(new GroupsInSparseRangeEnumerator(position, radius));
			}
		}

		public bool MoveNext()
		{
			return _e.MoveNext();
		}

		public void Reset()
		{
			_e.MoveNext();
		}

		public void Dispose()
		{
			_e.Dispose();
		}
	}

	public class TrackingGroup : IPoolable, IResetable, IDisposable
	{
		public List<TrackingEntry> Entries = new List<TrackingEntry>();

		public int[] LayerEntryCount = new int[9];

		public int[] FactionEntryCount = new int[8];

		public int visionScanId;

		private XYZ _position;

		public int trackedFactions;

		public TrackingLayer trackedLayers;

		public int count => Entries.Count;

		public bool IsEmpty => trackedLayers == (TrackingLayer)0;

		public XYZ Position => _position;

		public static TrackingGroup Request(XYZ position)
		{
			TrackingGroup trackingGroup = ObjectPool<TrackingGroup>.Reserve();
			trackingGroup._position = position;
			return trackingGroup;
		}

		public void AddEntry(TrackingEntry entry)
		{
			int iD = entry.faction.ID;
			trackedFactions |= 1 << iD;
			trackedLayers |= entry.Layers;
			int trackingGroupIndex = Entries.Count;
			Entries.Add(entry);
			entry.TrackingGroupIndex = trackingGroupIndex;
			foreach (int item in entry.Layers.Bits())
			{
				LayerEntryCount[item]++;
			}
			FactionEntryCount[iD]++;
		}

		public void Remove(TrackingEntry entry)
		{
			int iD = entry.faction.ID;
			TrackingEntry trackingEntry = Entries.Pop();
			if (trackingEntry != entry)
			{
				Entries[entry.TrackingGroupIndex] = trackingEntry;
				trackingEntry.TrackingGroupIndex = entry.TrackingGroupIndex;
			}
			entry.TrackingGroupIndex = -1;
			foreach (int item in entry.Layers.Bits())
			{
				LayerEntryCount[item]--;
				if (LayerEntryCount[item] == 0)
				{
					trackedLayers &= ~((TrackingLayerProperty)item).ToMask();
				}
			}
			FactionEntryCount[iD]--;
			if (FactionEntryCount[iD] == 0)
			{
				trackedFactions &= ~(1 << iD);
			}
		}

		public void Dispose()
		{
			ObjectPool<TrackingGroup>.Release(this);
		}

		public void ResetObject()
		{
			visionScanId = 0;
			Entries.Clear();
			Array.Clear(LayerEntryCount, 0, 9);
			Array.Clear(FactionEntryCount, 0, 8);
		}
	}

	public struct TrackingRequest : IEnumerable<TrackingEntry>, IEnumerable
	{
		public struct Enumerator : IEnumerator<TrackingEntry>, IDisposable, IEnumerator
		{
			private TrackingRegistry registry;

			private XYZ point;

			private float radius;

			private TrackingLayer layers;

			private IList<Faction> factions;

			private int factionBits;

			private RelevantXYZsInRangeEnumerator _groupEnumerator;

			private List<TrackingEntry>.Enumerator _entriesEnumerator;

			private TrackingGroup _currentGroup;

			private Vector3 pointV3;

			private TrackingEntry _current;

			object IEnumerator.Current => Current;

			public TrackingEntry Current => _current;

			public Enumerator(TrackingRequest request)
			{
				this = default(Enumerator);
				registry = request.registry;
				point = request.point;
				radius = request.radius;
				factions = request.factions;
				layers = request.layers;
				factionBits = GetFactionBits(factions);
				pointV3 = point.ToVector3();
				_groupEnumerator = GetRelevantGroupKeysEnumerator();
				_entriesEnumerator = Empty.List<TrackingEntry>().GetEnumerator();
			}

			public bool MoveNext()
			{
				do
				{
					if (NextEntry())
					{
						return true;
					}
				}
				while (GenerateEntryEnumerator());
				return false;
			}

			private bool GenerateEntryEnumerator()
			{
				if (!NextGroup())
				{
					return false;
				}
				_entriesEnumerator.Dispose();
				_entriesEnumerator = _currentGroup.Entries.GetEnumerator();
				return true;
			}

			private bool NextEntry()
			{
				while (_entriesEnumerator.MoveNext())
				{
					_current = _entriesEnumerator.Current;
					if (IsMatch(_current))
					{
						return true;
					}
				}
				return false;
			}

			private bool NextGroup()
			{
				while (_groupEnumerator.MoveNext())
				{
					_currentGroup = registry._groups[_groupEnumerator.Current];
					if ((_currentGroup.trackedLayers & layers) != 0 && (_currentGroup.trackedFactions & factionBits) != 0)
					{
						return true;
					}
				}
				return false;
			}

			private bool IsMatch(TrackingEntry e)
			{
				return (factionBits & (1 << e.faction.ID)) != 0 && (layers & e.Layers) != 0 && IsInRange(e.GameObject.transform.position, pointV3, radius);
			}

			public void Reset()
			{
			}

			public void Dispose()
			{
				_groupEnumerator.Dispose();
				_entriesEnumerator.Dispose();
			}

			private RelevantXYZsInRangeEnumerator GetRelevantGroupKeysEnumerator()
			{
				return new RelevantXYZsInRangeEnumerator(point, (int)radius);
			}
		}

		public XYZ point;

		public float radius;

		public TrackingLayer layers;

		public IList<Faction> factions;

		public TrackingRegistry registry;

		public TrackingRequest(XYZ point, float radius, TrackingLayer layers, TrackingRegistry registry, IList<Faction> factions)
		{
			this.point = point;
			this.radius = radius;
			this.layers = layers;
			this.registry = registry;
			this.factions = factions;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<TrackingEntry> IEnumerable<TrackingEntry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Slinq<TrackingEntry, CollisionEnumerationRequestSlinqContext> Slinq()
		{
			return CollisionEnumerationRequestSlinqContext.Slinq(this);
		}
	}

	private const int AllT = 511;

	private const int AllT2 = 426;

	private const int AllT4 = 324;

	private const int AllT8 = 264;

	public const int LayerCount = 9;

	private const int FactionCount = 8;

	protected static BrixLogger logger = new BrixLogger(typeof(TrackingRegistry).FullName);

	protected static HashSet<TrackingEntry> TrackedEntries = new HashSet<TrackingEntry>();

	public const int MvSideLength = 16;

	public static readonly XYZ MvSize = XYZ.one * 16;

	private readonly Dictionary<XYZ, TrackingGroup> _groups = new Dictionary<XYZ, TrackingGroup>();

	private readonly Dictionary<IGameComponent, TrackingEntry> _entries = new Dictionary<IGameComponent, TrackingEntry>();

	public static HashSet<TrackingEntry> GetAllTrackedEntries()
	{
		return TrackedEntries;
	}

	public static TrackingEntry GetEntry(IGameComponent gc)
	{
		return BrixSingleton<TrackingRegistry>.Instance._GetEntry(gc);
	}

	public static TrackingEntry Register(IGameComponent gc, TrackingLayer layer, Faction faction)
	{
		return BrixSingleton<TrackingRegistry>.Instance._Register(gc, layer, faction);
	}

	public static void Unregister(TrackingEntry entry)
	{
		if (BrixSingleton<TrackingRegistry>.Instance != null)
		{
			BrixSingleton<TrackingRegistry>.Instance._Unregister(entry);
		}
	}

	public static void Unregister(IGameComponent gc)
	{
		if (BrixSingleton<TrackingRegistry>.Instance != null)
		{
			BrixSingleton<TrackingRegistry>.Instance._Unregister(gc);
		}
	}

	public static void Refresh(TrackingEntry entry)
	{
		BrixSingleton<TrackingRegistry>.Instance._Refresh(entry);
	}

	public static void Refresh(IGameComponent gc)
	{
		BrixSingleton<TrackingRegistry>.Instance._Refresh(gc);
	}

	public static void GetRelevantXYZsInRange(List<XYZ> results, XYZ position, int radius, bool includeEmpty = false)
	{
		using RelevantXYZsInRangeEnumerator relevantXYZsInRangeEnumerator = new RelevantXYZsInRangeEnumerator(position, radius, includeEmpty);
		while (relevantXYZsInRangeEnumerator.MoveNext())
		{
			results.Add(relevantXYZsInRangeEnumerator.Current);
		}
	}

	public static bool IsInAtLeastPartialRange(TrackingGroup g, XYZ position, int radius)
	{
		XYZ xYZ = position - (MvSize - XYZ.one);
		XYZ xYZ2 = XYZ.Mul(g.Position, MvSize);
		XYZ a = xYZ2 - position;
		XYZ b = xYZ - xYZ2;
		XYZ a2 = XYZ.Max(a, b, XYZ.zero);
		return XYZ.SqrMagnitude(a2) <= radius * radius;
	}

	public static TrackingRequest GetCollisions(Vector3 point, float radius, TrackingLayer layer, IList<Faction> factions)
	{
		return GetCollisions(XYZ.FromVector3(point), radius, layer, factions);
	}

	public static TrackingRequest GetCollisions(XYZ point, float radius, TrackingLayer layer, IList<Faction> factions)
	{
		return BrixSingleton<TrackingRegistry>.Instance._GetCollisions(point, radius, layer, factions);
	}

	public static void GetCollisions(Vector3 point, float radius, TrackingLayer layer, IList<Faction> factions, ICollection<TrackingEntry> results)
	{
		GetCollisions(XYZ.FromVector3(point), radius, layer, factions, results);
	}

	public static void GetCollisions(XYZ point, float radius, TrackingLayer layer, IList<Faction> factions, ICollection<TrackingEntry> results)
	{
		BrixSingleton<TrackingRegistry>.Instance._GetCollisions(point, radius, layer, factions, results);
	}

	public static bool IsCollision(IGameComponent target, Vector3 point, float radius, TrackingLayer layer, IList<Faction> factions)
	{
		TrackingEntry valueOrDefault = BrixSingleton<TrackingRegistry>.Instance._entries.GetValueOrDefault(target);
		if ((valueOrDefault.Layers & layer) == 0)
		{
			return false;
		}
		if (!factions.Contains(valueOrDefault.faction))
		{
			return false;
		}
		return IsInRange(target.GameObject.transform.position, point, radius);
	}

	public static bool IsCollision(IGameComponent target, Vector3 point, float radius, TrackingLayer layer)
	{
		TrackingEntry valueOrDefault = BrixSingleton<TrackingRegistry>.Instance._entries.GetValueOrDefault(target);
		if ((valueOrDefault.Layers & layer) == 0)
		{
			return false;
		}
		return IsInRange(target.GameObject.transform.position, point, radius);
	}

	public static bool IsCollision(Vector3 targetPosition, Vector3 point, float radius)
	{
		return IsInRange(targetPosition, point, radius);
	}

	private TrackingEntry _GetEntry(IGameComponent gc)
	{
		return _entries.GetValueOrDefault(gc);
	}

	private TrackingEntry _Register(IGameComponent gc, TrackingLayer layer, Faction faction)
	{
		logger.InfoFormat("Registering tracking entry {0} on layers {1}", gc.Name, layer);
		XYZ key = XYZ.Div(XYZ.FromVector3(gc.GameObject.transform.position), MvSize);
		TrackingGroup valueOrCreate = _groups.GetValueOrCreate(key, TrackingGroup.Request);
		TrackingEntry valueOrCreate2 = _entries.GetValueOrCreate(gc, TrackingEntry.Request(gc));
		valueOrCreate2.Layers = layer;
		valueOrCreate2.faction = faction;
		valueOrCreate2.TrackingGroup = valueOrCreate;
		valueOrCreate.AddEntry(valueOrCreate2);
		TrackedEntries.Add(valueOrCreate2);
		return valueOrCreate2;
	}

	private void _Unregister(IGameComponent gc)
	{
		_Unregister(_entries[gc]);
	}

	private void _Unregister(TrackingEntry e)
	{
		TrackedEntries.Remove(e);
		if (e.TrackingGroup != null)
		{
			logger.InfoFormat("Unregistering tracking entry {0} on layers {1}", e.gameComponent.Name, e.Layers);
			IGameComponent gameComponent = e.gameComponent;
			TrackingGroup trackingGroup = e.TrackingGroup;
			e.TrackingGroup = null;
			XYZ position = trackingGroup.Position;
			trackingGroup.Remove(e);
			if (trackingGroup.IsEmpty)
			{
				_groups.Remove(position);
				trackingGroup.Dispose();
			}
			if (_entries.Remove(gameComponent))
			{
				e.Dispose();
			}
		}
	}

	private void _Refresh(IGameComponent gc)
	{
		_Refresh(_entries[gc]);
	}

	private void _Refresh(TrackingEntry e)
	{
		IGameComponent gameComponent = e.gameComponent;
		TrackingGroup trackingGroup = e.TrackingGroup;
		XYZ position = trackingGroup.Position;
		XYZ xYZ = XYZ.Div(XYZ.FromVector3(gameComponent.GameObject.transform.position), MvSize);
		if (!(position == xYZ))
		{
			trackingGroup.Remove(e);
			if (trackingGroup.IsEmpty)
			{
				_groups.Remove(position);
				trackingGroup.Dispose();
			}
			(e.TrackingGroup = _groups.GetValueOrCreate(xYZ, TrackingGroup.Request)).AddEntry(e);
		}
	}

	private TrackingRequest _GetCollisions(XYZ point, float radius, TrackingLayer c, IList<Faction> factions)
	{
		return new TrackingRequest(point, radius, c, this, factions);
	}

	private void _GetCollisions(XYZ point, float radius, TrackingLayer layers, IList<Faction> factions, ICollection<TrackingEntry> results)
	{
		int factionBits = GetFactionBits(factions);
		Vector3 b = point.ToVector3();
		using RelevantXYZsInRangeEnumerator relevantXYZsInRangeEnumerator = new RelevantXYZsInRangeEnumerator(point, (int)radius);
		while (relevantXYZsInRangeEnumerator.MoveNext())
		{
			TrackingGroup trackingGroup = _groups[relevantXYZsInRangeEnumerator.Current];
			if ((trackingGroup.trackedLayers & layers) == 0 || (trackingGroup.trackedFactions & factionBits) == 0)
			{
				continue;
			}
			foreach (TrackingEntry entry in trackingGroup.Entries)
			{
				if ((factionBits & (1 << entry.faction.ID)) != 0 && (layers & entry.Layers) != 0 && IsInRange(entry.GameObject.transform.position, b, radius))
				{
					results.Add(entry);
				}
			}
		}
	}

	private static int GetFactionBits(IList<Faction> factions)
	{
		int num = 0;
		int count = factions.Count;
		for (int i = 0; i < count; i++)
		{
			num |= 1 << factions[i].ID;
		}
		return num;
	}

	private static bool IsInRange(Vector3 a, Vector3 b, float radius)
	{
		return (a - b).sqrMagnitude <= radius * radius;
	}
}
