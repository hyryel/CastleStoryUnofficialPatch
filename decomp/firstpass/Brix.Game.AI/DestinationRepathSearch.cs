using System;
using Brix.Engine;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components.Volumes;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.AI;

public class DestinationRepathSearch : IPoolable, IResetable, IDisposable
{
	private readonly SearchPathRequest.BranchEvaluator _pathEvaluator;

	private readonly PathPositionAccepter _pathAccepter;

	private readonly PathDirectBuilder _pathBuilder;

	private readonly PathLink _pathStart;

	private readonly PathSearch _pathSearch;

	private GameObject _gameObject;

	private Func<XYZ, bool> _presenceTest;

	private Func<XYZ, bool> _canCross;

	public PathLink Result => _pathAccepter.Result;

	public XYZ Destination => _pathAccepter.RepresentativePosition;

	public DestinationRepathSearch()
	{
		_canCross = CanCross;
		_pathEvaluator = new SearchPathRequest.BranchEvaluator(_canCross);
		_pathAccepter = new PathPositionAccepter(XYZ.zero);
		_pathStart = new PathLink();
		_pathBuilder = new PathDirectBuilder(TestPathfinding.Instance.terrainLod, _pathEvaluator);
		_pathSearch = new PathSearch(new PathLightFirstQueue(), _pathBuilder, _pathAccepter);
	}

	public DestinationRepathSearch Init(Locomotion4 locomotion, XYZ peekDestination, bool tryPassThrough, bool canClimb)
	{
		_gameObject = locomotion.gameObject;
		_presenceTest = locomotion.presence.BuildPresenceTest(tryPassThrough);
		_pathEvaluator.Reset(_canCross, canClimb);
		_pathAccepter.Reset(peekDestination);
		_pathStart.Reset(PathNodeRegistry.WalkableAt(locomotion.presence.PositionOccupied));
		_pathBuilder.Reset(TestPathfinding.Instance.terrainLod, _pathEvaluator);
		_pathSearch.Clear();
		return this;
	}

	public void ResetObject()
	{
		_pathSearch.Clear();
	}

	public void Dispose()
	{
		ResetObject();
	}

	private bool CanCross(XYZ p)
	{
		if (VolumeRegistry.HasVolumesWithPropertyAt(p, VProperty.obstacle, (IVolume v, GameObject _gameObject) => !v.PassThrough.CanPassThrough(_gameObject), _gameObject))
		{
			return false;
		}
		return _presenceTest(p);
	}

	public bool Search()
	{
		_pathSearch.Start(_pathStart);
		for (int i = 0; i < 100; i++)
		{
			if (!_pathSearch.MoveNext())
			{
				break;
			}
		}
		if (_pathAccepter.Result == null)
		{
			return false;
		}
		return true;
	}
}
