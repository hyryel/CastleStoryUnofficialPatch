using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.AI;
using Smooth.Slinq;
using Smooth.Slinq.Context;

namespace Brix.Utils.Collections;

public class LayeredGoals
{
	private readonly HashSet<BuildGoal> _goals = new HashSet<BuildGoal>();

	private readonly Dictionary<Factory.AssetKey, int> _goalCountsPerAssetKey = new Dictionary<Factory.AssetKey, int>();

	public int Count => _goals.Count;

	public void Add(BuildGoal item)
	{
		if (_goals.Add(item) && !item.IsAnti)
		{
			IncreaseCount(item.AssetKey);
		}
	}

	public void Remove(BuildGoal item)
	{
		if (_goals.Remove(item) && !item.IsAnti)
		{
			DecreaseCount(item.AssetKey);
		}
	}

	private void IncreaseCount(Factory.AssetKey key)
	{
		int valueOrCreate = _goalCountsPerAssetKey.GetValueOrCreate(key);
		_goalCountsPerAssetKey[key] = valueOrCreate + 1;
	}

	private void DecreaseCount(Factory.AssetKey key)
	{
		int valueOrDefault = _goalCountsPerAssetKey.GetValueOrDefault(key);
		if (--valueOrDefault <= 0)
		{
			_goalCountsPerAssetKey.Remove(key);
		}
		else
		{
			_goalCountsPerAssetKey[key] = valueOrDefault;
		}
	}

	public HashSet<BuildGoal> All()
	{
		return _goals;
	}

	public Slinq<BuildGoal, HashSetSlinqContext<BuildGoal>> Slinq()
	{
		return _goals.Slinq();
	}

	public Slinq<BuildGoal, SelectContext<BuildGoal, KeyValuePair<Factory.AssetKey, int>, DictionaryContext<Factory.AssetKey, int>>> OneGoalPerAssetKey()
	{
		return from kv in _goalCountsPerAssetKey.Slinq()
			select Factory.Peek(kv.Key).GetComponent<BuildGoal>();
	}
}
