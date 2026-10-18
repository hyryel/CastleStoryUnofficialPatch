using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class SliderWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	private Slider _slider;

	[SerializeField]
	private EventTrigger _eventTrigger;

	[SerializeField]
	private Image _image;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (w.Is<IGetInteractable>())
		{
			_slider.interactable = (w as IGetInteractable).GetInteractable();
		}
		if (w.Is<IGetWholeNumbers>())
		{
			_slider.wholeNumbers = (w as IGetWholeNumbers).GetWholeNumbers();
		}
		if (w.Is<IGetColor>())
		{
			_image.color = (w as IGetColor).GetColor();
		}
		if (w.Is<IGetMinNumberValue>())
		{
			_slider.minValue = (w as IGetMinNumberValue).GetMinNumberValue();
		}
		if (w.Is<IGetMaxNumberValue>())
		{
			_slider.maxValue = (w as IGetMaxNumberValue).GetMaxNumberValue();
		}
		if (w.Is<IGetNumberValue>())
		{
			Slider.SliderEvent onValueChanged = _slider.onValueChanged;
			_slider.onValueChanged = new Slider.SliderEvent();
			_slider.value = (w as IGetNumberValue).GetNumberValue();
			_slider.onValueChanged = onValueChanged;
		}
		_slider.onValueChanged.RemoveAllListeners();
		_slider.onValueChanged.AddListener(delegate(float rawValue)
		{
			float numberValue = ((!w.Is<IGetValidatedNumberValue>()) ? rawValue : (w as IGetValidatedNumberValue).GetValidatedNumberValue(rawValue));
			if (w.Is<ISetNumberValue>())
			{
				(w as ISetNumberValue).SetNumberValue(numberValue);
			}
		});
		_eventTrigger.triggers.Clear();
		if (w.Is<IOnPointerEnter>())
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback = new EventTrigger.TriggerEvent();
			entry.callback.AddListener(delegate
			{
				(w as IOnPointerEnter).OnPointerEnter();
			});
			_eventTrigger.triggers.Add(entry);
		}
		if (w.Is<IOnPointerExit>())
		{
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.PointerExit;
			entry2.callback = new EventTrigger.TriggerEvent();
			entry2.callback.AddListener(delegate
			{
				(w as IOnPointerExit).OnPointerExit();
			});
			_eventTrigger.triggers.Add(entry2);
		}
		if (w.Is<IOnPointerUp>())
		{
			EventTrigger.Entry entry3 = new EventTrigger.Entry();
			entry3.eventID = EventTriggerType.PointerUp;
			entry3.callback = new EventTrigger.TriggerEvent();
			entry3.callback.AddListener(delegate
			{
				(w as IOnPointerUp).OnPointerUp();
				OnPointerUp(w);
			});
			_eventTrigger.triggers.Add(entry3);
		}
		if (w.Is<IOnPointerDown>())
		{
			EventTrigger.Entry entry4 = new EventTrigger.Entry();
			entry4.eventID = EventTriggerType.PointerDown;
			entry4.callback = new EventTrigger.TriggerEvent();
			entry4.callback.AddListener(delegate
			{
				(w as IOnPointerDown).OnPointerDown();
			});
			_eventTrigger.triggers.Add(entry4);
		}
		EventTrigger.Entry entry5 = new EventTrigger.Entry();
		entry5.eventID = EventTriggerType.Select;
		entry5.callback = new EventTrigger.TriggerEvent();
		entry5.callback.AddListener(delegate
		{
			OnSelect();
		});
		_eventTrigger.triggers.Add(entry5);
		return true;
	}

	private void OnSelect()
	{
	}

	private void OnPointerUp(IWidget w)
	{
	}
}
