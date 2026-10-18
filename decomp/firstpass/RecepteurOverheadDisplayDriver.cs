using Brix.Game.Components;
using Brix.UI.Icons;
using Brix.Utils.Signals;
using UnityEngine;

public class RecepteurOverheadDisplayDriver : OverheadDisplayDriver
{
	private InventoryTracker tracker;

	protected override void InitValues()
	{
		tracker = GetComponent<InventoryTracker>();
		base.InitValues();
		overheadDisplay.ItemCounts.Spawn();
		overheadDisplay.ItemCounts.Get.Show = false;
		overheadDisplay.hideIfFar = false;
		overheadDisplay.showOnHover = true;
		overheadDisplay.showOnSelect = true;
		overheadDisplay.BigIcon.Spawn();
		overheadDisplay.ShowBigIcon(value: false);
		overheadDisplay.SortBasedOnCamera = false;
		SetBigIcon();
	}

	protected override void ConnectToSignals()
	{
		tracker.InventoryChanged.Connect(OnInventoryChanged).DisconnectWhen(Disconnected);
		base.recepteur.ExclusionChanged.Connect(delegate
		{
			SetBigIcon();
		}).DisconnectWhen(Disconnected);
	}

	private void OnInventoryChanged(string name, Sprite icon, int qty)
	{
		overheadDisplay.ItemCounts.Get.SetOrAddResource(icon, qty, name);
		overheadDisplay.ShowOrHide();
	}

	private void SetBigIcon()
	{
		Sprite icon = (base.recepteur.ExcludingAll ? IconKeys.UI_X.Get64() : ((base.recepteur.ExclusiveRessource == null) ? IconKeys.UI_Check.Get32() : base.recepteur.ExclusiveRessource.GetKey().GetInfo().icon));
		overheadDisplay.BigIcon.Get.SetIcon(icon);
		bool flag = base.recepteur.HasExclusions();
		overheadDisplay.ShowBigIcon(flag);
		overheadDisplay.BigIcon.Get.Show = flag;
	}
}
