using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using Motus.Behavior;
using Smooth.Delegates;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

public class HarvestGoalProvider : Behavior, IGoalProvider
{
	private BrixLogger logger = new BrixLogger(typeof(HarvestGoalProvider).FullName);

	[SerializeField]
	private Beacon _balise;

	[SerializeField]
	private NatureType _acceptedTypes;

	private readonly Dictionary<XYZ, NatureHarvestGoal> _goals = new Dictionary<XYZ, NatureHarvestGoal>();

	public Beacon Balise
	{
		get
		{
			if (_balise == null)
			{
				_balise = base.transform.parent.GetComponentInChildren<Beacon>();
			}
			return _balise;
		}
		set
		{
			_balise = value;
		}
	}

	public void SetAcceptedTypes(NatureType natureType)
	{
		_acceptedTypes = natureType;
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
		foreach (KeyValuePair<XYZ, NatureHarvestGoal> goal in _goals)
		{
			NatureHarvestGoal value = goal.Value;
			if (!value.IsNullOrReleased() && _acceptedTypes.HasFlag(value.NatureType))
			{
				value.UniqueItems(ref keys);
			}
		}
	}

	public void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys)
	{
	}

	protected override Node CreateTree()
	{
		return (StaticNode)Node.Sequence.Do((HarvestGoalProvider self) => self.RefreshGoals(), this).Loop().WithDelay(10L);
	}

	public Slinq<NatureHarvestGoal, SelectContext<NatureHarvestGoal, KeyValuePair<XYZ, NatureHarvestGoal>, PredicateContext<KeyValuePair<XYZ, NatureHarvestGoal>, DictionaryContext<XYZ, NatureHarvestGoal>, NatureType>>> Goals()
	{
		return from kvp in _goals.Slinq().Where((KeyValuePair<XYZ, NatureHarvestGoal> kvp, NatureType type) => type.HasFlag(kvp.Value.NatureType), _acceptedTypes)
			select kvp.Value;
	}

	public int GoalCountOfType(NatureType type)
	{
		return _goals.Slinq().Where((KeyValuePair<XYZ, NatureHarvestGoal> kvp, NatureType _type) => _type.HasFlag(kvp.Value.NatureType), type).Count();
	}

	public int GoalCount()
	{
		return GoalCountOfType(_acceptedTypes);
	}

	public Slinq<Goal, SelectContext<Goal, NatureHarvestGoal, SelectContext<NatureHarvestGoal, KeyValuePair<XYZ, NatureHarvestGoal>, PredicateContext<KeyValuePair<XYZ, NatureHarvestGoal>, DictionaryContext<XYZ, NatureHarvestGoal>, NatureType>>>> All()
	{
		return Goals().Select((DelegateFunc<NatureHarvestGoal, Goal>)((NatureHarvestGoal g) => g));
	}

	public void GetAll(ICollection<Goal> results)
	{
		All().AddTo(results);
	}

	protected IEnumerator<ActivityProgress> RefreshGoals()
	{
		while (Balise == null)
		{
			yield return ActivityProgress.Wait();
		}
		yield return ActivityProgress.Sub(ReleaseOutDatedGoals(_goals));
		HashSet<NatureData> natureToSpawn = CollectionPool.RequestHashSet<NatureData>();
		yield return ActivityProgress.Sub(RegisterGoals(natureToSpawn));
		yield return ActivityProgress.Sub(SpawnGoals(natureToSpawn));
		natureToSpawn.Dispose();
	}

	private bool NeedsAGoal(XYZ pos)
	{
		if (NatureRegistry.NatureAt(pos).IsValid != NatureRegistry.HasNatureAt(pos))
		{
			logger.Warn("NatureRegistry.HasNatureAt lies compared to NatureRegistry.NatureAt(pos).IsValid");
		}
		return NatureRegistry.NatureAt(pos).IsValid && IsInsideProjectRange(Balise.position.ToVector3(), pos.ToVector3(), (int)Balise.radius);
	}

	private IEnumerator<ActivityProgress> ReleaseOutDatedGoals(Dictionary<XYZ, NatureHarvestGoal> registeredGoals)
	{
		List<XYZ> posToRemove = CollectionPool.RequestList<XYZ>();
		foreach (KeyValuePair<XYZ, NatureHarvestGoal> registeredGoal in registeredGoals)
		{
			if (!NeedsAGoal(registeredGoal.Key))
			{
				posToRemove.Add(registeredGoal.Key);
			}
		}
		foreach (XYZ item in posToRemove)
		{
			NatureHarvestGoal natureHarvestGoal = registeredGoals[item];
			if (!natureHarvestGoal.IsNullOrReleased())
			{
				ObjectPoolSingleton.Release(natureHarvestGoal.gameObject);
			}
			registeredGoals.Remove(item);
		}
		posToRemove.Dispose();
		yield return ActivityProgress.Wait();
	}

	private IEnumerator<ActivityProgress> RegisterGoals(HashSet<NatureData> toSpawn)
	{
		int radius = (int)Balise.radius;
		XYZ center = Balise.position;
		for (int i = -radius; i <= radius; i++)
		{
			for (int j = -radius; j <= radius; j++)
			{
				if (IsInsideProjectRange(position: new Vector3(center.x + i, center.y, center.z + j), center: center.ToVector3(), radius: radius))
				{
					NatureData natureData = NatureRegistry.NatureAt(center.x + i, center.z + j);
					if (natureData.IsValid)
					{
						toSpawn.Add(natureData);
					}
				}
			}
		}
		yield return ActivityProgress.Wait();
	}

	private IEnumerator<ActivityProgress> SpawnGoals(HashSet<NatureData> natures)
	{
		foreach (NatureData nature in natures)
		{
			XYZ key = XYZ.FromVector3(nature.GoalPosition);
			NatureHarvestGoal value = null;
			if (!_goals.TryGetValue(key, out value) || value.IsNullOrReleased())
			{
				_goals[key] = NatureHarvestGoal.Request(nature, base.transform);
			}
		}
		yield return ActivityProgress.Wait();
	}

	protected bool IsInsideProjectRange(Vector3 center, Vector3 position, int radius)
	{
		return (position - center).Flatten().sqrMagnitude <= (float)(radius * radius);
	}

	public bool HasNonStorable()
	{
		return false;
	}
}
