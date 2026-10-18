using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Wards;
using Brix.Input;
using Brix.UI.GameSelector;
using Brix.Utils.Factories;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;

public class ObjectsHighlightDriver : GameComponent
{
	protected enum ShowStatus
	{
		show,
		remove,
		linger
	}

	private static Dictionary<Factory.AssetKey, float> ObjectRadius;

	[SerializeField]
	private Color BlockSelected;

	[SerializeField]
	private Color BlockHovered;

	private const float ToolRadius = 1.25f;

	private const float BuildRessourceRadius = 1.25f;

	private const float RessourceRadius = 1.5f;

	private Dictionary<GameObject, int> highlightsByIds = new Dictionary<GameObject, int>();

	private Dictionary<Factory.AssetKey, float> BuildObjectDynamiquesRadius()
	{
		Dictionary<Factory.AssetKey, float> dictionary = new Dictionary<Factory.AssetKey, float>();
		dictionary.Add(ObjetsDynamiques.Palette, 2f);
		dictionary.Add(ObjetsDynamiques.Catapulte, 4f);
		dictionary.Add(ObjetsDynamiques.BearTrap, 1.5f);
		dictionary.Add(ObjetsDynamiques.Baril, 1.5f);
		dictionary.Add(ObjetsDynamiques.Porte, 2f);
		dictionary.Add(ObjetsDynamiques.HeavyDoor, 2f);
		dictionary.Add(ObjetsDynamiques.ShakyDoor, 2f);
		dictionary.Add(ObjetsDynamiques.Crystal, 3f);
		dictionary.Add(ObjetsDynamiques.Toolrack, 2f);
		dictionary.Add(ObjetsDynamiques.Workbench, 2f);
		dictionary.Add(ObjetsDynamiques.ChoppingBlock, 1.5f);
		dictionary.Add(ObjetsDynamiques.Loom, 2f);
		dictionary.Add(ObjetsDynamiques.Furnace, 2.8f);
		dictionary.Add(ObjetsDynamiques.Forge, 2f);
		dictionary.Add(ObjetsDynamiques.Lab, 2f);
		dictionary.Add(ObjetsDynamiques.Altar, 2f);
		dictionary.Add(ObjetsDynamiques.MachineShop, 2f);
		return dictionary;
	}

	protected override void Start()
	{
		base.Start();
		ObjectRadius = CacheCirclesKeys();
		base.OnReleaseOrDestroySignal.ConnectOnce(delegate
		{
			ObjectRadius = null;
		});
		GameSignals.LateConnect(UIGameObserver.IsReady, ConnectToSignals);
	}

	private void ConnectToSignals()
	{
		UIGameObserver.dynamicObjects.Hovered.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.dynamicObjects.Unhovered.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.ennemyDynamicObjects.Hovered.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.ennemyDynamicObjects.Unhovered.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.ennemyDynamicObjects.Selected.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.ennemyDynamicObjects.Unselected.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.dynamicObjects.Selected.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.dynamicObjects.Unselected.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.dynamicObjects.Added.Connect(ConnectObject).DisconnectWhen(base.OnReleaseOrDestroySignal);
		GameSignals.Connect(InputModeController.InputModeChanged, delegate
		{
			OnInputModeChanged();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.placedBlocks.Hovered.Connect(OnBlockHovered).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.placedBlocks.Unhovered.Connect(OnBlockUnhovered).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.placedBlocks.Selected.Connect(OnBlockSelected).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.placedBlocks.Unselected.Connect(OnBlockUnselected).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private Dictionary<Factory.AssetKey, float> CacheCirclesKeys()
	{
		Dictionary<Factory.AssetKey, float> dictionary = BuildObjectDynamiquesRadius();
		CacheSimilar(dictionary, Accessories.Sword, 1.25f);
		CacheSimilar(dictionary, RawResources.RawStone, 1.5f);
		CacheSimilar(dictionary, BuildResources.IronIngot, 1.25f);
		return dictionary;
	}

	private void CacheSimilar(Dictionary<Factory.AssetKey, float> into, Factory.AssetKey key, float radius)
	{
		(from t in Factory.GetFactory(key).Templates.Slinq()
			select t.AssetKey).ForEach(delegate(Factory.AssetKey k, float r)
		{
			into.Add(k, r);
		}, radius);
	}

	private void ConnectObject(Transform dynObject)
	{
		DamageReceiver component = dynObject.GetComponent<DamageReceiver>();
		if ((bool)component)
		{
			component.HPChangedSignal.Connect(delegate(float previous, float next, Transform obj)
			{
				OnHpChanged(obj, previous > next);
			}, dynObject).DisconnectWhen(component.OnReleaseOrDestroySignal);
		}
		WardAttack component2 = dynObject.GetComponent<WardAttack>();
		if ((bool)component2)
		{
			component2.HeatChangedSignal.Connect(delegate(ObjectsHighlightDriver self, Transform obj)
			{
				RefreshCircle(obj);
			}, this, dynObject).DisconnectWhen(component2.OnReleaseOrDestroySignal);
		}
	}

	private void ClearHighlight(GameObject gameObject)
	{
		if (highlightsByIds.TryGetValue(gameObject, out var value))
		{
			Highlighter.remove.Request(value);
		}
	}

	private void OnBlockHovered(Transform block)
	{
		if (!UIGameSelector.IsSelected(block.gameObject))
		{
			VoxelTransform component = block.GetComponent<VoxelTransform>();
			ClearHighlight(block.gameObject);
			highlightsByIds[block.gameObject] = Highlighter.highlight.BlockAndTerrain(component.position, BlockHovered, (Signal<GameObject>)block.GetComponent<GameComponent>().OnReleaseOrDestroySignal);
		}
	}

	private void OnBlockUnhovered(Transform block)
	{
		if (!UIGameSelector.IsSelected(block.gameObject))
		{
			ClearHighlight(block.gameObject);
		}
	}

	private void OnBlockSelected(Transform block)
	{
		VoxelTransform component = block.GetComponent<VoxelTransform>();
		ClearHighlight(block.gameObject);
		highlightsByIds[block.gameObject] = Highlighter.highlight.BlockAndTerrain(component.position, BlockSelected, (Signal<GameObject>)block.GetComponent<GameComponent>().OnReleaseOrDestroySignal);
	}

	private void OnBlockUnselected(Transform block)
	{
		ClearHighlight(block.gameObject);
	}

	private static void OnHpChanged(Transform dynObject, bool damage)
	{
		RefreshCircle(dynObject, damage);
	}

	private static void OnHeatChanged(Transform dynObject, bool damage)
	{
		RefreshCircle(dynObject, damage);
	}

	private void OnInputModeChanged()
	{
		UIGameObserver.dynamicObjects.AllSelected.Slinq().ForEach(delegate(Transform t)
		{
			RefreshCircle(t);
		});
	}

	private static ShowStatus ShouldShow(Transform t)
	{
		if (InputModeController.GetCurrentMode() != InputMode.command)
		{
			return ShowStatus.remove;
		}
		LocalFactionVision component = t.GetComponent<LocalFactionVision>();
		if (!component.IsNullOrReleased() && !component.IsVisibleForMe())
		{
			return ShowStatus.remove;
		}
		if (UIGameSelector.IsFocused(t.gameObject))
		{
			return ShowStatus.show;
		}
		if (t.WithComponent((DamageReceiver dr) => dr.IsDamaged, _default: false))
		{
			return ShowStatus.show;
		}
		if (t.WithComponent((WardAttack w) => w.IsHeated, _default: false))
		{
			return ShowStatus.show;
		}
		return ShowStatus.remove;
	}

	private static void RefreshCircle(Transform dynObject)
	{
		RefreshCircle(dynObject, damage: false);
	}

	private static void RefreshCircle(Transform dynObject, bool damage)
	{
		switch (ShouldShow(dynObject))
		{
		case ShowStatus.show:
			GetCircle(dynObject.gameObject, damage);
			break;
		case ShowStatus.remove:
			SelectionCircleDispenser.Instance.Remove(dynObject.gameObject);
			break;
		case ShowStatus.linger:
			SelectionCircleDispenser.Instance.Remove(dynObject.gameObject, 1f);
			break;
		}
	}

	private static SelectionCircle.MainColors GetColor(GameObject go)
	{
		return (!Affiliation.BelongsToMe(go)) ? (Affiliation.BelongsToEnnemy(go) ? SelectionCircle.MainColors.Enemies : SelectionCircle.MainColors.Local) : SelectionCircle.MainColors.Local;
	}

	private static void GetCircle(GameObject go, bool damage)
	{
		SelectionCircle selectionCircle = SelectionCircleDispenser.Instance.AddOrGet(go);
		selectionCircle.Selected(UIGameSelector.IsFocused(go));
		selectionCircle.ShowHp(go.WithComponent((DamageReceiver dr) => dr.IsDamaged, _default: false));
		selectionCircle.ShowStamina(go.WithComponent((WardAttack dr) => dr.IsHeated, _default: false));
		selectionCircle.Hp(go.WithComponent((DamageReceiver dr) => dr.CurrentHP / dr.MaxHP, 1f));
		selectionCircle.Stamina(go.WithComponent((WardAttack dr) => dr.CurrentHeat, 1f));
		selectionCircle.Radius(go.WithComponent((FactoryImprint f) => ObjectRadius.GetValueOrDefault(f.AssetKey, 1f), 1f));
		selectionCircle.MainColor(GetColor(go));
		selectionCircle.SecondaryColor(SelectionCircle.SecondaryColors.Heat);
		selectionCircle.StamFlicker(go.WithComponent((WardAttack dr) => dr.IsOverHeated, _default: false));
		selectionCircle.FlashHp(damage);
	}
}
