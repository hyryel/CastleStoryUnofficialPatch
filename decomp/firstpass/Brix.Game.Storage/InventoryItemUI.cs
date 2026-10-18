using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.Game.Storage;

public class InventoryItemUI : GameComponent
{
	public enum EnableConditions
	{
		Always,
		WhenContentNonZero
	}

	public UniversalButtonInterface Button;

	public Signal<InventoryItemUI> VisibilityChanged = new Signal<InventoryItemUI>();

	public EnableConditions EnabledWhen;

	private Signal needsDisconnect = new Signal();

	[SerializeField]
	private InventoryItemCount _itemCount;

	private string previousCount;

	public InventoryItemCount ItemCount
	{
		get
		{
			return _itemCount;
		}
		set
		{
			needsDisconnect.Invoke();
			_itemCount = value;
			if (_itemCount != null)
			{
				_itemCount.CapacityCountChanged.Connect(Refresh).DisconnectWhen(needsDisconnect);
				_itemCount.ContentCountChanged.Connect(Refresh).DisconnectWhen(needsDisconnect);
				if (Button == null)
				{
					Button = UniversalButtonItem.NewCounter(_itemCount.Name, _itemCount.Icon(), Color.yellow, colorIcon: false, _itemCount.Name, string.Empty).GenerateOrUpdateButton();
					Button.SetTextColor(Color.yellow);
					Button.SetIconColor(Color.white);
					ParentButton(Button.gameObject);
				}
			}
		}
	}

	private bool EvaluateEnablingCondition(EnableConditions condition)
	{
		return condition switch
		{
			EnableConditions.WhenContentNonZero => ContentNonZero(), 
			EnableConditions.Always => true, 
			_ => false, 
		};
	}

	private bool ContentNonZero()
	{
		return _itemCount != null && _itemCount.ContentCount > 0;
	}

	public void Refresh()
	{
		bool activeSelf = base.gameObject.activeSelf;
		bool flag = EvaluateEnablingCondition(EnabledWhen);
		base.gameObject.SetActive(flag);
		string countText = ItemCount.ContentCount.ToString();
		Button.SetCountText(countText);
		Button.SetIconColor(Color.white);
		Button.SwitchContent(UniversalButtonInterface.ContentModes.Counter);
		if (activeSelf != flag)
		{
			VisibilityChanged.Invoke(this);
		}
	}

	public void ParentButton(GameObject button)
	{
		RectTransform component = button.GetComponent<RectTransform>();
		component.SetParent(base.transform);
		component.anchorMin = new Vector2(0f, 0f);
		component.anchorMax = new Vector2(1f, 1f);
	}

	public void Release()
	{
		if (!Button.IsNullOrReleased())
		{
			ObjectPoolSingleton.Release(Button.gameObject);
		}
		ObjectPoolSingleton.Release(base.gameObject);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		VisibilityChanged.Clear();
		needsDisconnect.Invoke();
	}

	private void OnValidate()
	{
		ItemCount = _itemCount;
		if (ItemCount != null)
		{
			Refresh();
		}
	}
}
