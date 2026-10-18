using System;
using Brix.Audio;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Lifecycle.Pooling;
using Brix.NewUI.Tooltips;
using Brix.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UniversalButtonItem
{
	public enum Mode
	{
		Button,
		Counter,
		Label
	}

	public const string Side_Menu_Rollover = "Side_Menu_Rollover";

	public const string Side_Menu_Select = "Side_Menu_Select";

	public string name;

	public Sprite icon;

	public Action onClick;

	public Color color;

	public bool colorIcon;

	public bool greyWhenEmpty;

	public string count;

	public string tooltip;

	public string key;

	public Mode mode;

	public string hoverAudio = "Side_Menu_Rollover";

	public string pressAudio = "Side_Menu_Select";

	public Signal EnabledSignal = new Signal();

	public Signal DisabledSignal = new Signal();

	public bool showBackgroundByDefault;

	public TooltipCallDelegate tooltipCallback = delegate
	{
	};

	public Action destroyTooltipCallback = delegate
	{
	};

	public Transform parent;

	private UniversalButtonInterface _synchedButton;

	private SignalConnection _disabledSignalConnection = SignalConnection.Empty;

	public UniversalButtonInterface synchedButton
	{
		get
		{
			return _synchedButton;
		}
		private set
		{
			_disabledSignalConnection.Disconnect();
			_synchedButton = value;
			if (_synchedButton != null)
			{
				_disabledSignalConnection = _synchedButton.DisabledSignal.Connect(DisabledSignal.Invoke);
			}
		}
	}

	public UniversalButtonItem(Mode mode, string name, Sprite icon, Action onClick, Color color, string tooltip, string count, string key, bool greyWhenEmpty = false)
	{
		this.name = name;
		this.icon = icon;
		this.onClick = onClick;
		this.color = color;
		colorIcon = true;
		this.greyWhenEmpty = greyWhenEmpty;
		this.tooltip = tooltip;
		this.count = count;
		this.mode = mode;
		this.key = key;
	}

	public static UniversalButtonItem NewButton(string name, Sprite icon, Action onClick, Color color, bool colorIcon, string tooltip, string key = "")
	{
		UniversalButtonItem universalButtonItem = new UniversalButtonItem(Mode.Button, name, icon, onClick, color, tooltip, key, string.Empty);
		universalButtonItem.colorIcon = colorIcon;
		return universalButtonItem;
	}

	public static UniversalButtonItem NewLabel(string name, Sprite icon, Color color, bool colorIcon, string tooltip)
	{
		UniversalButtonItem universalButtonItem = new UniversalButtonItem(Mode.Label, name, icon, delegate
		{
		}, color, tooltip, string.Empty, string.Empty);
		universalButtonItem.colorIcon = colorIcon;
		return universalButtonItem;
	}

	public static UniversalButtonItem NewCounter(string name, Sprite icon, Color color, bool colorIcon, string tooltip, string count, bool greyWhenEmpty = true)
	{
		UniversalButtonItem universalButtonItem = new UniversalButtonItem(Mode.Counter, name, icon, delegate
		{
		}, color, tooltip, count, string.Empty, greyWhenEmpty);
		universalButtonItem.colorIcon = colorIcon;
		return universalButtonItem;
	}

	public void OverrideWith(UniversalButtonItem item)
	{
		name = item.name;
		icon = item.icon;
		onClick = item.onClick;
		color = item.color;
		colorIcon = item.colorIcon;
		greyWhenEmpty = item.greyWhenEmpty;
		tooltip = item.tooltip;
		count = item.count;
		mode = item.mode;
		key = item.key;
	}

	public UniversalButtonInterface ReplaceGameObjectWithButton(GameObject go)
	{
		synchedButton = _ReplaceGameObjectWithButton(go);
		return GenerateOrUpdateButton();
	}

	public UniversalButtonInterface LinkExistingButton(GameObject go)
	{
		UniversalButtonInterface component = go.GetComponent<UniversalButtonInterface>();
		if (component == null)
		{
			return null;
		}
		synchedButton = component;
		return GenerateOrUpdateButton();
	}

	public UniversalButtonInterface GenerateOrUpdateButton()
	{
		if (synchedButton == null)
		{
			synchedButton = RequestItem();
		}
		if (colorIcon)
		{
			synchedButton.SetIconColor(color);
			synchedButton.SetTextColor(color);
		}
		else
		{
			synchedButton.SetIconColor(Color.white);
			synchedButton.SetTextColor(color);
		}
		if (mode == Mode.Button)
		{
			synchedButton.SwitchContent(UniversalButtonInterface.ContentModes.PrimaryIcon);
			synchedButton.SetIconForContent(icon, UniversalButtonInterface.ContentModes.PrimaryIcon);
			synchedButton.EnableClick(value: true);
			synchedButton.EnableHover(value: true);
			synchedButton.EnableFlashOnHighlight(value: true);
		}
		if (mode == Mode.Label)
		{
			synchedButton.SetIconForContent(icon, UniversalButtonInterface.ContentModes.PrimaryIcon);
			synchedButton.SwitchContent(UniversalButtonInterface.ContentModes.PrimaryIcon);
			synchedButton.EnableClick(value: false);
			synchedButton.EnableHover(value: true);
			synchedButton.EnableFlashOnHighlight(value: false);
		}
		if (mode == Mode.Counter)
		{
			synchedButton.SetIconForContent(icon, UniversalButtonInterface.ContentModes.Counter);
			synchedButton.SwitchContent(UniversalButtonInterface.ContentModes.Counter);
			synchedButton.SetCountText(count);
			synchedButton.EnableClick(value: false);
			synchedButton.EnableHover(value: true);
			synchedButton.EnableFlashOnHighlight(value: false);
		}
		synchedButton.SetKey(key);
		synchedButton.name = name;
		synchedButton.SetText(I2Helper.TryGet(tooltip));
		synchedButton.ShowBackgroundByDefault(showBackgroundByDefault);
		Button.ButtonClickedEvent buttonClickedEvent = synchedButton.GetComponent<Button>().onClick;
		buttonClickedEvent.RemoveAllListeners();
		buttonClickedEvent.AddListener(delegate
		{
			onClick();
		});
		buttonClickedEvent.AddListener(delegate
		{
			SoundEngine.Play(pressAudio);
		});
		SetOnHover(delegate
		{
			SoundEngine.Play(hoverAudio);
		}, synchedButton.gameObject);
		EnabledSignal.Invoke();
		return synchedButton;
	}

	public void ReleaseButton()
	{
		ObjectPoolSingleton.Release(synchedButton.gameObject);
	}

	private UniversalButtonInterface RequestItem()
	{
		ObjectPoolSingleton.Request(out var go, UI.UniversalButton);
		go.transform.SetParent(parent);
		return go.GetComponent<UniversalButtonInterface>();
	}

	private UniversalButtonInterface _ReplaceGameObjectWithButton(GameObject go)
	{
		RectTransform component = go.GetComponent<RectTransform>();
		int siblingIndex = component.GetSiblingIndex();
		Transform transform = component.parent;
		Vector3 localPosition = component.localPosition;
		Vector3 vector = component.rect.size;
		if (go != null)
		{
			ObjectPoolSingleton.Release(go);
		}
		ObjectPoolSingleton.Request(out go, UI.UniversalButton);
		RectTransform component2 = go.GetComponent<RectTransform>();
		component2.SetParent(transform);
		component2.SetSiblingIndex(siblingIndex);
		component2.localPosition = localPosition;
		component2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vector.x);
		component2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vector.y);
		return component2.GetComponent<UniversalButtonInterface>();
	}

	private void SetOnHover(Action callback, GameObject target)
	{
		EventTrigger component = target.GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback = new EventTrigger.TriggerEvent();
		UnityAction<BaseEventData> call = delegate
		{
			callback();
		};
		entry.callback.AddListener(call);
		component.triggers.Add(entry);
	}

	private void SetOnHover(string tooltip, RectTransform target)
	{
		RectTransform rect = target;
		EventTrigger component = target.GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback = new EventTrigger.TriggerEvent();
		UnityAction<BaseEventData> call = delegate
		{
			tooltipCallback(I2Helper.TryGet(tooltip), rect);
		};
		entry.callback.AddListener(call);
		component.triggers.Add(entry);
	}

	private void SetOnEndHover(RectTransform target)
	{
		EventTrigger component = target.GetComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerExit;
		entry.callback = new EventTrigger.TriggerEvent();
		UnityAction<BaseEventData> call = delegate
		{
			destroyTooltipCallback();
		};
		entry.callback.AddListener(call);
		component.triggers.Add(entry);
	}
}
