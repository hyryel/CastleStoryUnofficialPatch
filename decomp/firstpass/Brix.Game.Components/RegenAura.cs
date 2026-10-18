using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public class RegenAura : UpdatableGameComponent<RegenAura>
{
	[SerializeField]
	private int RegenConfig;

	private int _currentRegen;

	[SerializeField]
	private bool alwaysHeal;

	private Trackable trackUnits;

	private List<DamageReceiver> tracked = new List<DamageReceiver>();

	public int RegenPerSecond => _currentRegen;

	protected override float SlowUpdateRate => 1f;

	public override void OnRequested()
	{
		base.OnRequested();
		DefaultHeal();
	}

	private void OnValidate()
	{
		DefaultHeal();
	}

	private void DefaultHeal()
	{
		if (alwaysHeal)
		{
			StarHealing();
		}
	}

	public void StarHealing()
	{
		_currentRegen = RegenConfig;
	}

	public void StopHealing()
	{
		_currentRegen = 0;
	}

	public void UnitsEnter(HashSet<Trackable> units)
	{
		units.Slinq().ForEach(delegate(Trackable u, RegenAura self)
		{
			self.StartTracking(u.gameObject);
		}, this);
	}

	public void UnitsExit(HashSet<Trackable> units)
	{
		units.Slinq().ForEach(delegate(Trackable u, RegenAura self)
		{
			self.StopTracking(u.gameObject);
		}, this);
	}

	private void StartTracking(GameObject unit)
	{
		Labor component = unit.GetComponent<Labor>();
		DamageReceiver component2 = unit.GetComponent<DamageReceiver>();
		if (component != null && component2 != null)
		{
			tracked.Add(component2);
		}
	}

	private void StopTracking(GameObject unit)
	{
		Labor component = unit.GetComponent<Labor>();
		DamageReceiver component2 = unit.GetComponent<DamageReceiver>();
		if (component != null && component2 != null)
		{
			tracked.Remove(component2);
			if (component2.ExternalRegenerator == this)
			{
				component2.ExternalRegenerator = null;
			}
		}
	}

	protected override void BrixSlowUpdate()
	{
		HealTick();
	}

	private void HealTick()
	{
		foreach (DamageReceiver item in tracked)
		{
			if (item.ExternalRegenerator == null || item.ExternalRegenerator.RegenPerSecond < _currentRegen)
			{
				item.ExternalRegenerator = this;
			}
		}
	}
}
