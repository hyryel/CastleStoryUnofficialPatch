using System.Collections.Generic;
using Brix.Components;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.Game.Storage;

public class StorageUI : BrixComponent
{
	public enum Mode
	{
		blueprint,
		storage
	}

	[SerializeField]
	private GameObject Visuals;

	[SerializeField]
	public Mode mode;

	public bool ShowItemsAlways;

	private RecepteurReader reader;

	[SerializeField]
	private InventoryItemUI cloneableItem;

	[SerializeField]
	private RectTransform itemHolder;

	[SerializeField]
	private int _nbActive;

	private Dictionary<Adjectif, InventoryItemUI> items = new Dictionary<Adjectif, InventoryItemUI>();

	public bool NoButtonBackground { get; set; }

	private int NbActive
	{
		get
		{
			return _nbActive;
		}
		set
		{
			_nbActive = value;
			ShowIfNeeded();
		}
	}

	public void SetBlueprintRecepteur(IRecepteur recepteur)
	{
		reader = base.gameObject.GetOrAddComponent<RecepteurReader>();
		reader.BlueprintIRecepteur = recepteur;
		reader.Refresh();
		PopulateCounts(reader);
	}

	public void SetStorageRecepteur(IRecepteur recepteur)
	{
		reader = base.gameObject.GetOrAddComponent<RecepteurReader>();
		reader.StorageIRecepteur = recepteur;
		reader.Refresh();
		PopulateCounts(reader);
	}

	private void OnValidate()
	{
		if (reader != null)
		{
			PopulateCounts(reader);
		}
	}

	private void PopulateCounts(RecepteurReader _reader)
	{
		List<InventoryItemCount> list = ((mode != Mode.blueprint) ? reader.StoredItems : reader.RequiredItems);
		foreach (InventoryItemCount item in list)
		{
			SetItem(item);
		}
		ShowIfNeeded();
	}

	private void Reset()
	{
		foreach (KeyValuePair<Adjectif, InventoryItemUI> item in items)
		{
			item.Value.Release();
		}
		items.Clear();
	}

	private InventoryItemUI GetOrCreate(InventoryItemCount count)
	{
		if (items.ContainsKey(count.Adjectif))
		{
			return items[count.Adjectif];
		}
		InventoryItemUI inventoryItemUI = RequestItem(count);
		items.Add(count.Adjectif, inventoryItemUI);
		return inventoryItemUI;
	}

	private void SetItem(InventoryItemCount count)
	{
		InventoryItemUI orCreate = GetOrCreate(count);
		orCreate.ResetComponent();
		orCreate.transform.SetParent(itemHolder);
		orCreate.name = count.Name;
		orCreate.ItemCount = count;
		orCreate.gameObject.SetActive(value: false);
		orCreate.VisibilityChanged.Connect(AdjustCount);
		orCreate.Button.GetComponent<Image>().enabled = !NoButtonBackground;
		Visuals.GetComponent<Image>().enabled = !NoButtonBackground;
		orCreate.EnabledWhen = ((!ShowItemsAlways) ? InventoryItemUI.EnableConditions.WhenContentNonZero : InventoryItemUI.EnableConditions.Always);
		orCreate.Refresh();
	}

	private InventoryItemUI RequestItem(InventoryItemCount count)
	{
		ObjectPoolSingleton.Request(out var go, Brix.External.Factories.UI.ItemUI);
		return go.GetComponent<InventoryItemUI>();
	}

	private void AdjustCount(InventoryItemUI item)
	{
		NbActive += (item.gameObject.activeSelf ? 1 : (-1));
	}

	private void ShowIfNeeded()
	{
		bool active = NbActive > 0;
		Visuals.SetActive(active);
	}

	public override void ResetComponent()
	{
		Reset();
	}
}
