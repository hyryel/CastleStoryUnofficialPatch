using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Game.AI.KnowledgeSpace;

public class KnowledgeReserver : UpdatableGameComponent<KnowledgeReserver>
{
	private HashSet<GameObject> Reservations = new HashSet<GameObject>();

	private List<SignalConnection> connections = new List<SignalConnection>();

	public bool IsReserved(GameObject go)
	{
		return Reservations.Contains(go);
	}

	public bool IsAvailable(GameObject go)
	{
		return !Reservations.Contains(go);
	}

	public void ReserveUntil(GameObject go, Signal until)
	{
		if (!Reservations.Contains(go))
		{
			Reservations.Add(go);
			connections.Add(until.ConnectOnce(delegate
			{
				Reservations.Remove(go);
			}));
		}
	}

	public void ReserveUntil<T>(GameObject go, Signal<T> until)
	{
		if (!Reservations.Contains(go))
		{
			Reservations.Add(go);
			connections.Add(until.ConnectOnce(delegate
			{
				Reservations.Remove(go);
			}));
		}
	}

	private void Reset()
	{
		Reservations.Clear();
		connections.ForEach(delegate(SignalConnection c)
		{
			c.Disconnect();
		});
		connections.Clear();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Reset();
	}
}
