using Brix.Engine;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Bricktron;
using Brix.Game.Components;
using Brix.NewUI.OverheadDisplay;
using Brix.Utils;
using Brix.Utils.Signals;
using UnityEngine;
using UnityEngine.Networking;

public class BricktronOverheadDisplayDriver : OverheadDisplayDriver
{
	[SerializeField]
	private Labor labor;

	[SerializeField]
	private CharacterState state;

	private bool RelationAlliedOrNeutral => Affiliation.IsAlliedOrNeutral(base.gameObject, User.LocalUser.DefaultAlliedFactionGO);

	private bool OwnedbyMe => Affiliation.BelongsToMe(base.gameObject);

	private User Owner => GetComponent<AffiliationComponent>().faction.Owner;

	protected override void InitValues()
	{
		base.InitValues();
		overheadDisplay.BigIcon.Spawn();
		overheadDisplay.ItemCounts.Spawn();
		overheadDisplay.NameTag.Spawn();
		overheadDisplay.ThoughtBubble.Spawn();
		overheadDisplay.Issues.Spawn();
		overheadDisplay.Color = BackgroundColor;
		overheadDisplay.NameTag.Get.SetName(" " + base.gameObject.name, forceRefresh: true);
		overheadDisplay.NameTag.Get.Show = true;
		overheadDisplay.BigIcon.Get.NoIcon();
		overheadDisplay.BigIcon.Get.ForceShow = true;
		overheadDisplay.ItemCounts.Get.ClearAllResources();
		overheadDisplay.ThoughtBubble.Get.NoThought();
		overheadDisplay.NameTag.Get.Interactible = false;
		overheadDisplay.ItemCounts.Get.EnableButtons(enable: false);
		overheadDisplay.InFog = !base.vision.IsVisibleForMe();
		overheadDisplay.SortBasedOnCamera = true;
		overheadDisplay.hideIfFar = false;
		if (OwnedbyMe)
		{
			overheadDisplay.Color = OverheadDisplay.ownedColor;
			overheadDisplay.NameTag.Get.NoContext();
			overheadDisplay.NameTag.Get.TextColor(OverheadDisplay.ownedColor);
			return;
		}
		Color color = ((!RelationAlliedOrNeutral) ? OverheadDisplay.enemyColor : OverheadDisplay.allyColor);
		overheadDisplay.Color = color;
		if ((bool)Owner)
		{
			overheadDisplay.NameTag.Get.SetContext(Owner.ScreenName);
		}
		overheadDisplay.NameTag.Get.TextColor(color);
	}

	protected override void ConnectToSignals()
	{
		if (RelationAlliedOrNeutral)
		{
			if (NetworkServer.active)
			{
				state.NetworkUpdate.Connect(UpdateThought).DisconnectWhen(Disconnected);
			}
			GetComponent<InventoryTracker>().InventoryChanged.Connect(OnInventoryChanged).DisconnectWhen(Disconnected);
			labor.Profession.CapacityChanged.Connect(SetIcon).DisconnectWhen(Disconnected);
			state.NetworkUpdate.Connect(SetIcon).DisconnectWhen(Disconnected);
			GameSignals.Connect(Nom.Changed, OnNameChanged);
			labor.Occupation.OccupationChanged.Connect(delegate
			{
				SetIcon();
			}).DisconnectWhen(Disconnected);
		}
	}

	private void OnNameChanged(GameObject instance, string newName)
	{
		if (this != null && instance != null && instance == base.gameObject)
		{
			overheadDisplay.NameTag.Get.SetName(newName);
		}
	}

	private void OnInventoryChanged(string name, Sprite icon, int qty)
	{
		overheadDisplay.ItemCounts.Get.SetOrAddResource(icon, qty, name);
		overheadDisplay.ItemCounts.Get.Show = !labor.recepteur.IsEmpty();
	}

	private void UpdateThought()
	{
		if (state.Thought == Thought.None)
		{
			overheadDisplay.ThoughtBubble.Get.NoThought();
			return;
		}
		bool important = state.Thought.type == Thought.ThoughtType.Warning || state.Thought.type == Thought.ThoughtType.Error;
		overheadDisplay.ThoughtBubble.Get.SetThought(I2Helper.TryGet(state.Thought.GetLabel()), important);
		overheadDisplay.ShowOrHide();
	}

	private void SetIcon()
	{
		Sprite icon = labor.Autonomy.GetIcon(labor);
		if (icon != null)
		{
			overheadDisplay.NameTag.Get.WithIcon(icon);
		}
		else
		{
			overheadDisplay.NameTag.Get.NoIcon();
		}
		overheadDisplay.NameTag.Get.ForceShow = labor.Autonomy.IsDefensive;
		overheadDisplay.ShowOrHide();
	}
}
