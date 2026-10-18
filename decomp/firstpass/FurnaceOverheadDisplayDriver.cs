using System.Collections.Generic;
using Brix.Game;
using Brix.Game.Components;
using Brix.NewUI.OverheadDisplay;
using Brix.UI.Icons;
using Brix.Utils.Signals;
using Smooth.Algebraics;
using Smooth.Slinq;
using UnityEngine;

public class FurnaceOverheadDisplayDriver : OverheadDisplayDriver
{
	[SerializeField]
	private CraftingStation furnace;

	[SerializeField]
	private InventoryTracker inventory;

	private static Dictionary<Tuple<string, int, int>, string> nametags = new Dictionary<Tuple<string, int, int>, string>();

	protected override void Start()
	{
		if (Affiliation.IsAllied(User.LocalUser.faction, base.gameObject))
		{
			base.Start();
		}
	}

	protected override void InitValues()
	{
		base.InitValues();
		overheadDisplay.NameTag.Spawn();
		overheadDisplay.OrderQueue.Spawn();
		overheadDisplay.ItemCounts.Spawn();
		overheadDisplay.NameTag.Get.Show = true;
	}

	protected override void ConnectToSignals()
	{
		inventory.InventoryChanged.Connect(OnInventoryChanged).DisconnectWhen(base.OnReleaseOrDestroySignal);
		furnace.Changed.Connect(OnQueueChanged).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void OnQueueChanged()
	{
		RefreshQueue();
		RefreshName();
		overheadDisplay.ShowOrHide();
	}

	private void RefreshQueue()
	{
		if (furnace.CurrentMode == CraftingStation.Mode.queue)
		{
			overheadDisplay.OrderQueue.Get.ClearQueue();
			furnace.QueuedRecipes.Slinq().ForEach(delegate(RecipeInfo r, OverheadDisplay d)
			{
				d.OrderQueue.Get.AddToQueue(r.icon.Get32());
			}, overheadDisplay);
			if (furnace.Looping)
			{
				overheadDisplay.OrderQueue.Get.AddToQueue(IconKeys._UI_Wheel.Get32());
			}
			overheadDisplay.OrderQueue.Get.Show = true;
		}
		else
		{
			overheadDisplay.OrderQueue.Get.Show = false;
		}
	}

	private void RefreshName()
	{
		overheadDisplay.NameTag.Get.WithIcon(furnace.Icon.Get32(), forceRefresh: true);
		overheadDisplay.NameTag.Get.SetName(NameTag(furnace));
	}

	private void OnInventoryChanged(string name, Sprite icon, int qty)
	{
		overheadDisplay.ItemCounts.Get.SetOrAddResource(icon, qty, name);
		overheadDisplay.ItemCounts.Get.Show = !base.recepteur.IsEmpty();
	}

	private static string NameTag(CraftingStation station)
	{
		Tuple<string, int, int> key = Tuple.Create(station.PieMenuTag, station.CrewCount(), station.MaximumCrewSize);
		if (!nametags.ContainsKey(key))
		{
			string text = " x " + station.CrewCount() + "/" + station.MaximumCrewSize;
			nametags.Add(key, text);
			return text;
		}
		return nametags[key];
	}
}
