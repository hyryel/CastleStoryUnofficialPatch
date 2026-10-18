using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class ToggleWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected Toggle _toggle;

	[SerializeField]
	protected EventTrigger _eventTrigger;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
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
		entry5.eventID = EventTriggerType.PointerClick;
		entry5.callback = new EventTrigger.TriggerEvent();
		entry5.callback.AddListener(delegate(BaseEventData e)
		{
			OnPointerClick(e, w);
		});
		_eventTrigger.triggers.Add(entry5);
		return true;
	}

	private void OnPointerClick(BaseEventData baseData, IWidget w)
	{
		PointerEventData pointerEventData = baseData as PointerEventData;
		if (pointerEventData.button == PointerEventData.InputButton.Left && pointerEventData.clickCount == 1)
		{
			if (w.Is<IOnAction>())
			{
				(w as IOnAction).OnAction();
			}
		}
		else if (pointerEventData.button == PointerEventData.InputButton.Right && pointerEventData.clickCount == 1 && w.Is<IOnAltAction>())
		{
			(w as IOnAltAction).OnAltAction();
		}
		if (pointerEventData.button == PointerEventData.InputButton.Left && pointerEventData.clickCount == 2)
		{
			if (w.Is<IOnDoubleAction>())
			{
				(w as IOnDoubleAction).OnDoubleAction();
			}
		}
		else if (pointerEventData.button == PointerEventData.InputButton.Right && pointerEventData.clickCount == 2 && w.Is<IOnAltDoubleAction>())
		{
			(w as IOnAltDoubleAction).OnAltDoubleAction();
		}
	}
}
