using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Semantique;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.Game.Components;

public class InventoryTracker : MonoBehaviour
{
	private Recepteur recepteur;

	public Signal<string, Sprite, int> InventoryChanged = new Signal<string, Sprite, int>();

	private Dictionary<AdjectiveKey, int> currentCount = new Dictionary<AdjectiveKey, int>(8);

	private Dictionary<AdjectiveKey, int> previousCount = new Dictionary<AdjectiveKey, int>(8);

	protected Signal Destroyed = new Signal();

	private void Start()
	{
		recepteur = GetComponent<GameComponent>().recepteur;
		recepteur.Changed.Connect(delegate
		{
			Refresh();
		}).DisconnectWhen(Destroyed);
		Refresh();
	}

	private void Destroy()
	{
		Destroyed.Invoke();
	}

	public void CountInventory()
	{
		previousCount = currentCount;
		currentCount = new Dictionary<AdjectiveKey, int>(previousCount.Count);
		foreach (IDescriptor storedItem in recepteur.StoredItems)
		{
			IEnumerator<Adjectif> enumerator2 = storedItem.Description.GetUIRelevant().GetEnumerator();
			while (enumerator2.MoveNext())
			{
				if (!currentCount.ContainsKey(enumerator2.Current.GetKey()))
				{
					currentCount.Add(enumerator2.Current.GetKey(), 1);
				}
				else
				{
					currentCount[enumerator2.Current.GetKey()]++;
				}
			}
		}
	}

	public void Refresh(bool doItNextFrame = true)
	{
		if (doItNextFrame)
		{
			Neo.Relais(delegate
			{
				Refresh(doItNextFrame: false);
			}, "refresh InventoryTracker");
			return;
		}
		CountInventory();
		foreach (KeyValuePair<AdjectiveKey, int> item in previousCount)
		{
			AdjectiveInfo info = item.Key.GetInfo();
			if (!currentCount.TryGetValue(item.Key, out var value))
			{
				InventoryChanged.Invoke(info.descriptiveName, info.icon, 0);
			}
			else if (value != item.Value)
			{
				InventoryChanged.Invoke(info.descriptiveName, info.icon, value);
			}
		}
		foreach (KeyValuePair<AdjectiveKey, int> item2 in currentCount)
		{
			AdjectiveInfo info2 = item2.Key.GetInfo();
			if (!previousCount.ContainsKey(item2.Key))
			{
				InventoryChanged.Invoke(info2.descriptiveName, info2.icon, item2.Value);
			}
		}
	}
}
