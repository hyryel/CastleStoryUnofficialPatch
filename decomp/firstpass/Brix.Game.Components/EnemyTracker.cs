using System;
using System.Collections.Generic;
using Brix.Game.AI.Damage;
using Brix.Game.Components.Tracking;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;
using Smooth.Slinq.Collections;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.Components;

public class EnemyTracker : GameComponent
{
	public Slinq<GameObject, PredicateContext<GameObject, SelectContext<GameObject, TrackingRegistry.TrackingEntry, CollisionEnumerationRequestSlinqContext>, KeyValuePair<EnemyTracker, Faction>>> AllVisibleFor(TrackingLayer layers, int distance)
	{
		return (from t in TrackingRegistry.GetCollisions(base.transform.position, distance, layers, base.faction.EnemyFactions).Slinq()
			select t.GameObject).Where((GameObject c, KeyValuePair<EnemyTracker, Faction> kvp) => kvp.Key.IsAttackableEnemy(c) && kvp.Key.IsVisibleFor(c, kvp.Value), new KeyValuePair<EnemyTracker, Faction>(this, base.faction));
	}

	public Slinq<GameObject, LinkedContext<float, GameObject>> AllVisibleForByDistance(TrackingLayer layers, int distance, bool sortedByDistance = false)
	{
		return (from t in TrackingRegistry.GetCollisions(base.transform.position, distance, layers, base.faction.EnemyFactions).Slinq()
			select t.GameObject).Where((GameObject c, KeyValuePair<EnemyTracker, Faction> kvp) => kvp.Key.IsAttackableEnemy(c) && kvp.Key.IsVisibleFor(c, kvp.Value), new KeyValuePair<EnemyTracker, Faction>(this, base.faction)).OrderBy((GameObject c, Transform _transform) => Vector3.Distance(_transform.position, c.transform.position), base.transform);
	}

	public bool IsAttackableEnemy(GameObject o)
	{
		return IsAttackableEnemy(o.GetComponent<IGameComponent>());
	}

	public bool IsAttackableEnemy(IGameComponent o)
	{
		if (!Affiliation.IsEnemy(base.faction, o.faction))
		{
			return false;
		}
		IDamageReceiver damageReceiver = o.damageReceiver;
		return damageReceiver != null && !damageReceiver.IsInvincible;
	}

	public bool IsVisibleFor(GameObject o, Faction faction)
	{
		return !o.IsNullOrReleased() && LocalFactionVision.ObjectIsVisibleForFaction(o, faction);
	}

	public IGameComponent GetClosestEnemy<TArg>(float maxRange, Func<IGameComponent, TArg, bool> predicate, TArg arg, TrackingLayer trackingLayers)
	{
		IGameComponent result = null;
		float num = float.PositiveInfinity;
		Vector3 position = base.transform.position;
		foreach (TrackingRegistry.TrackingEntry collision in TrackingRegistry.GetCollisions(position, maxRange, trackingLayers, base.faction.EnemyFactions))
		{
			IGameComponent gameComponent = collision.gameComponent;
			if (IsAttackableEnemy(gameComponent) && predicate(gameComponent, arg))
			{
				float num2 = Vector3.Distance(gameComponent.GameObject.transform.position, position);
				if (num2 < num)
				{
					num = num2;
					result = gameComponent;
				}
			}
		}
		return result;
	}

	public bool EnemyInRadius(float radius)
	{
		Slinq<GameObject, PredicateContext<GameObject, SelectContext<GameObject, TrackingRegistry.TrackingEntry, CollisionEnumerationRequestSlinqContext>, KeyValuePair<EnemyTracker, Faction>>> slinq = AllVisibleFor(TrackingLayer.Character, Mathf.RoundToInt(radius));
		SlinqEnumerator<GameObject, PredicateContext<GameObject, SelectContext<GameObject, TrackingRegistry.TrackingEntry, CollisionEnumerationRequestSlinqContext>, KeyValuePair<EnemyTracker, Faction>>> enumerator = slinq.GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (!current.IsNullOrReleased() && !Affiliation.IsAllied(current, base.gameObject))
			{
				float num = Vector3.Distance(current.transform.position, base.transform.position);
				if (num < radius)
				{
					return true;
				}
			}
		}
		return false;
	}
}
