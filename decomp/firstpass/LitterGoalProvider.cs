using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.Tracking;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Motus.Behavior;
using Smooth.Delegates;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

public class LitterGoalProvider : Behavior, IGoalProvider
{
	protected static BrixLogger logger = new BrixLogger(typeof(LitterGoalProvider).FullName);

	private const TrackingLayer CLEAN_LAYER = TrackingLayer.SmallResources | TrackingLayer.Accessories;

	private Dictionary<GameObject, LitterGoal> GoalsByGos = new Dictionary<GameObject, LitterGoal>();

	private List<LitterGoal> _goals = new List<LitterGoal>();

	public Beacon _balise;

	private List<LitterGoal> _toRemove = new List<LitterGoal>();

	private List<GameObject> _toRemoveGO = new List<GameObject>();

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

	public override void OnRequested()
	{
		base.OnRequested();
		base.enabled = NetworkServer.active;
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
		foreach (LitterGoal goal in _goals)
		{
			goal.UniqueItems(ref keys);
		}
	}

	public void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys)
	{
	}

	protected override Node CreateTree()
	{
		return (StaticNode)Node.Do(delegate(LitterGoalProvider self)
		{
			self.Refresh();
		}, this).Loop().WithDelay(30L);
	}

	public void GetAll(ICollection<Goal> goals)
	{
		All().AddTo(goals);
	}

	public Slinq<Goal, SelectContext<Goal, LitterGoal, IListContext<LitterGoal>>> All()
	{
		return _goals.Slinq().Select((DelegateFunc<LitterGoal, Goal>)((LitterGoal g) => g));
	}

	public Slinq<Goal, SelectContext<Goal, LitterGoal, IListContext<LitterGoal>>> All(bool yes)
	{
		return ((!yes) ? Empty.List<LitterGoal>() : _goals).Slinq().Select((DelegateFunc<LitterGoal, Goal>)((LitterGoal g) => g));
	}

	public bool Any()
	{
		return _goals.Count > 0;
	}

	public int GoalCount()
	{
		return _goals.Count;
	}

	public List<LitterGoal> Goals()
	{
		return _goals;
	}

	private void Refresh()
	{
		if (!Balise.IsNullOrReleased())
		{
			HashSet<GameObject> hashSet = (from t in TrackingRegistry.GetCollisions(Balise.transform.position, Balise.radius, TrackingLayer.SmallResources | TrackingLayer.Accessories, Faction.DefaultNeutral.AsList).Slinq()
				select t.GameObject).ToPooledHashSet();
			RemoveOldGoals(hashSet);
			hashSet.Slinq().Where(IsOnGround).ForEach(delegate(GameObject t, LitterGoalProvider self)
			{
				self.CreateGoalIfNoneNearby(t);
			}, this);
			hashSet.Dispose();
		}
	}

	private void RemoveOldGoals(HashSet<GameObject> items)
	{
		try
		{
			_toRemove.Clear();
			foreach (KeyValuePair<GameObject, LitterGoal> goalsByGo in GoalsByGos)
			{
				if (!items.Contains(goalsByGo.Key))
				{
					_toRemoveGO.Add(goalsByGo.Key);
					_toRemove.Add(goalsByGo.Value);
				}
			}
			for (int i = 0; i < _goals.Count; i++)
			{
				if (_goals[i].IsNullOrReleased())
				{
					_toRemove.Add(_goals[i]);
				}
			}
			_toRemoveGO.Slinq().ForEach(delegate(GameObject g, LitterGoalProvider self)
			{
				self.GoalsByGos.Remove(g);
			}, this);
			_toRemove.Slinq().ForEach(delegate(LitterGoal g, LitterGoalProvider self)
			{
				self._goals.Remove(g);
				if (!g.IsNullOrReleased())
				{
					ObjectPoolSingleton.Release(g.gameObject);
				}
			}, this);
		}
		catch (Exception t)
		{
			logger.ErrorFormat("Error removing old goals for {0} in LitterGoalProvider {1}", t, base.name);
		}
	}

	private void CreateGoalIfNoneNearby(GameObject go)
	{
		if (!Apparence.Is<Granulaire>(go))
		{
			return;
		}
		LitterGoal valueOrDefault = GoalsByGos.GetValueOrDefault(go);
		if (!(valueOrDefault != null))
		{
			valueOrDefault = PackListAndSearchNearbyGoal(go);
			if (valueOrDefault == null)
			{
				valueOrDefault = CreateLitterGoal(go);
				_goals.Add(valueOrDefault);
			}
			valueOrDefault.Garnottes.Add(go.transform);
			GoalsByGos[go] = valueOrDefault;
		}
	}

	private LitterGoal PackListAndSearchNearbyGoal(GameObject go)
	{
		LitterGoal litterGoal = null;
		Transform t = go.transform;
		for (int i = 0; i < _goals.Count; i++)
		{
			if (_goals[i] == null)
			{
				litterGoal = _goals.Pop();
				if (litterGoal != null)
				{
					_goals[i] = litterGoal;
				}
				i--;
			}
			else if (_goals[i].IsNearby(t))
			{
				litterGoal = _goals[i];
				break;
			}
		}
		return litterGoal;
	}

	private LitterGoal CreateLitterGoal(GameObject go)
	{
		ObjectPoolSingleton.RequestComponent<LitterGoal>(out var t, Test.LitterGoal, Calcule.RoundVecteur(go.transform.position), Quaternion.identity);
		t.transform.SetParent(base.transform);
		t.RejectGarnotteSignal.Connect(delegate(Transform transform)
		{
			if (transform != null)
			{
				GoalsByGos.Remove(transform.gameObject);
			}
		});
		return t;
	}

	public bool HasNonStorable()
	{
		return false;
	}

	public static bool IsOnGround(GameObject go)
	{
		if (!go.activeSelf)
		{
			return false;
		}
		GenericParentTracker component = go.GetComponent<GenericParentTracker>();
		return component.IsNullOrReleased() || component.IsOnGround();
	}
}
