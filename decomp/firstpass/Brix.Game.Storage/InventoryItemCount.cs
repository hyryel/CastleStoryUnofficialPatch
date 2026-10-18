using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.AI;
using UnityEngine;

namespace Brix.Game.Storage;

public class InventoryItemCount
{
	public InventoryItemDescription item;

	public Signal ContentCountChanged = new Signal();

	private int _contentCount;

	public Signal CapacityCountChanged = new Signal();

	private int _capacityCount;

	public string Name => item.name;

	public Adjectif Adjectif => item.adjectif;

	public Func<Sprite> Icon => item.icon;

	public int ContentCount
	{
		get
		{
			return _contentCount;
		}
		set
		{
			int contentCount = _contentCount;
			_contentCount = value;
			if (contentCount != _contentCount)
			{
				ContentCountChanged.Invoke();
			}
		}
	}

	public int CapacityCount
	{
		get
		{
			return _capacityCount;
		}
		set
		{
			int capacityCount = _capacityCount;
			_capacityCount = value;
			if (capacityCount != _capacityCount)
			{
				CapacityCountChanged.Invoke();
			}
		}
	}

	public InventoryItemCount(string name, Adjectif adjectif, Func<Sprite> icon)
	{
		item = new InventoryItemDescription(name, adjectif, icon);
		_contentCount = 0;
		_capacityCount = 0;
	}

	public InventoryItemCount(InventoryItemDescription item)
	{
		this.item = item;
		_contentCount = 0;
		_capacityCount = 0;
	}

	public static void FromItems(ref List<InventoryItemCount> result, List<InventoryItemDescription> items)
	{
		foreach (InventoryItemDescription item in items)
		{
			result.Add(new InventoryItemCount(item));
		}
	}
}
