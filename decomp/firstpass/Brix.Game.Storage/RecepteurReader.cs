using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Semantique;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.Storage;

public class RecepteurReader : MonoBehaviour
{
	private SignalConnection storageSignal = SignalConnection.Empty;

	private IRecepteur _storageIRecepteur;

	private SignalConnection blueprintSignal = SignalConnection.Empty;

	private IRecepteur _blueprintIRecepteur;

	private List<InventoryItemCount> storedItems = new List<InventoryItemCount>();

	private List<InventoryItemCount> requiredItems = new List<InventoryItemCount>();

	public List<string> debug_stored_items = new List<string>();

	public List<string> debug_required_items = new List<string>();

	public IRecepteur StorageIRecepteur
	{
		get
		{
			return _storageIRecepteur;
		}
		set
		{
			_storageIRecepteur = value;
			storageSignal.Disconnect();
			storageSignal = value.Changed.Connect(OnRecepteurChanged);
			List<InventoryItemDescription> buffer = CollectionPool.RequestList<InventoryItemDescription>();
			InventoryItemDescription.ItemDescriptionsFromAdjectiveType(ref buffer, AdjectiveInfo.AdjectiveType.Ressource);
			InventoryItemCount.FromItems(ref storedItems, buffer);
			buffer.Dispose();
		}
	}

	public IRecepteur BlueprintIRecepteur
	{
		get
		{
			return _blueprintIRecepteur;
		}
		set
		{
			_blueprintIRecepteur = value;
			blueprintSignal.Disconnect();
			blueprintSignal = value.Changed.Connect(OnRecepteurChanged);
			requiredItems.Clear();
			List<InventoryItemDescription> buffer = CollectionPool.RequestList<InventoryItemDescription>();
			InventoryItemDescription.ItemDescriptionsFromAdjectiveType(ref buffer, AdjectiveInfo.AdjectiveType.Ressource);
			InventoryItemCount.FromItems(ref requiredItems, buffer);
			buffer.Dispose();
		}
	}

	public List<InventoryItemCount> StoredItems => storedItems;

	public List<InventoryItemCount> RequiredItems => requiredItems;

	private void OnDestroy()
	{
		Disconnect();
	}

	private void Disconnect()
	{
		storageSignal.Disconnect();
		blueprintSignal.Disconnect();
	}

	private void OnRecepteurChanged()
	{
		base.enabled = true;
	}

	public void Refresh()
	{
		ReadRecepteur(StorageIRecepteur, BlueprintIRecepteur);
	}

	private void ReadRecepteur(IRecepteur storage, IRecepteur requirement)
	{
		if (storedItems != null)
		{
			for (int i = 0; i < storedItems.Count; i++)
			{
				InventoryItemCount inventoryItemCount = storedItems[i];
				if (storage != null)
				{
					inventoryItemCount.ContentCount = storage.ContentDescription.Value(inventoryItemCount.Adjectif);
				}
			}
		}
		if (requiredItems == null)
		{
			return;
		}
		for (int j = 0; j < requiredItems.Count; j++)
		{
			InventoryItemCount inventoryItemCount2 = requiredItems[j];
			if (requirement != null)
			{
				inventoryItemCount2.ContentCount = requirement.BaseCapacity.Value(inventoryItemCount2.Adjectif) - requirement.ContentDescription.Value(inventoryItemCount2.Adjectif);
			}
		}
	}

	private void Update()
	{
		base.enabled = false;
		Refresh();
	}

	private void DebugDisplay(List<InventoryItemCount> counts, List<string> output)
	{
		for (int i = 0; i < counts.Count; i++)
		{
			string text = counts[i].Name + " : " + counts[i].ContentCount + " / " + counts[i].CapacityCount;
			if (i >= output.Count)
			{
				output.Add(text);
			}
			output[i] = text;
		}
	}
}
