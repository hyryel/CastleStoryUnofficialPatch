using System;
using System.Collections.Generic;
using Brix.External.Collections;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using UnityEngine;

public class ColliderTrackable
{
	public class Default
	{
		public string id;

		public Default(string id)
		{
			this.id = id;
		}
	}

	public static class Defaults
	{
		public static Default Bricktron = new Default("Bricktron");

		public static Default Corruptron = new Default("Corruptron");
	}

	public string id;

	public Predicate<GameObject> condition;

	private ReactiveCollection<GameObject> collection;

	private const string ID_corruptron = "Corruptron";

	private const string ID_bricktron = "Bricktron";

	public ICollection<GameObject> Items => collection;

	public IReadOnlySignal BecameEmpty => collection.BecameEmpty;

	public IReadOnlySignal NoLongerEmpty => collection.NoLongerEmpty;

	public IReadOnlySignal<GameObject> Added => collection.Added;

	public IReadOnlySignal<GameObject> Removed => collection.Removed;

	public ColliderTrackable(string id, Predicate<GameObject> condition)
	{
		this.id = id;
		this.condition = condition;
		collection = new ReactiveCollection<GameObject>(new HashSet<GameObject>());
	}

	public static ColliderTrackable DefaultTrackable(Default def)
	{
		if (def == Defaults.Bricktron)
		{
			return Bricktron();
		}
		if (def == Defaults.Corruptron)
		{
			return Corruptron();
		}
		throw new Exception("Invalid Default type");
	}

	private static ColliderTrackable Corruptron()
	{
		return new ColliderTrackable("Corruptron", IsCorruptron);
	}

	private static ColliderTrackable Bricktron()
	{
		return new ColliderTrackable("Bricktron", IsBricktron);
	}

	private static bool IsCorruptron(GameObject go)
	{
		if (go.GetComponent<Labor>() == null)
		{
			return false;
		}
		Faction faction = go.GetComponent<AffiliationComponent>().faction;
		if (faction == null)
		{
			return false;
		}
		if (faction == Faction.DefaultEnemy)
		{
			return true;
		}
		return false;
	}

	private static bool IsBricktron(GameObject go)
	{
		if (go.GetComponent<Labor>() == null)
		{
			return false;
		}
		Faction faction = go.GetComponent<AffiliationComponent>().faction;
		if (faction == null)
		{
			return false;
		}
		if (faction == Faction.DefaultAllied)
		{
			return true;
		}
		return false;
	}
}
