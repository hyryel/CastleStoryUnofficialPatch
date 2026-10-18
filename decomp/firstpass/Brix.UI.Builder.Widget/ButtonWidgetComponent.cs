using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class ButtonWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected Button _button;

	[SerializeField]
	protected EventTrigger _eventTrigger;

	private bool _isInit;

	private IWidget _w;

	private bool _hovering;

	private bool _pressing;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (!_isInit)
		{
			_isInit = true;
			_eventTrigger.triggers.Clear();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback = new EventTrigger.TriggerEvent();
			entry.callback.AddListener(delegate
			{
				_hovering = true;
				_w = ((!_pressing && !_hovering) ? null : w);
				if (w.Is<IOnPointerEnter>())
				{
					(w as IOnPointerEnter).OnPointerEnter();
				}
			});
			_eventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.PointerExit;
			entry2.callback = new EventTrigger.TriggerEvent();
			entry2.callback.AddListener(delegate
			{
				_hovering = false;
				_w = ((!_pressing && !_hovering) ? null : w);
				if (w.Is<IOnPointerExit>())
				{
					(w as IOnPointerExit).OnPointerExit();
				}
			});
			_eventTrigger.triggers.Add(entry2);
			EventTrigger.Entry entry3 = new EventTrigger.Entry();
			entry3.eventID = EventTriggerType.PointerDown;
			entry3.callback = new EventTrigger.TriggerEvent();
			entry3.callback.AddListener(delegate
			{
				_pressing = true;
				_w = ((!_pressing && !_hovering) ? null : w);
				if (w.Is<IOnPointerDown>())
				{
					(w as IOnPointerDown).OnPointerDown();
				}
			});
			_eventTrigger.triggers.Add(entry3);
			EventTrigger.Entry entry4 = new EventTrigger.Entry();
			entry4.eventID = EventTriggerType.PointerUp;
			entry4.callback = new EventTrigger.TriggerEvent();
			entry4.callback.AddListener(delegate
			{
				_pressing = false;
				_w = ((!_pressing && !_hovering) ? null : w);
				if (w.Is<IOnPointerUp>())
				{
					(w as IOnPointerUp).OnPointerUp();
				}
			});
			_eventTrigger.triggers.Add(entry4);
			EventTrigger.Entry entry5 = new EventTrigger.Entry();
			entry5.eventID = EventTriggerType.PointerClick;
			entry5.callback = new EventTrigger.TriggerEvent();
			entry5.callback.AddListener(delegate(BaseEventData e)
			{
				OnPointerClick(e, w);
			});
			_eventTrigger.triggers.Add(entry5);
		}
		return true;
	}

	private bool IsShift()
	{
		return UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift) || UnityEngine.Input.GetKeyDown(KeyCode.LeftShift) || UnityEngine.Input.GetKeyDown(KeyCode.RightShift);
	}

	private bool IsCtrl()
	{
		return UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl) || UnityEngine.Input.GetKeyDown(KeyCode.LeftControl) || UnityEngine.Input.GetKeyDown(KeyCode.RightControl);
	}

	private void OnPointerClick(BaseEventData baseData, IWidget w)
	{
		PointerEventData pointerEventData = baseData as PointerEventData;
		if (pointerEventData.button == PointerEventData.InputButton.Left)
		{
			if (w.Is<IOnShiftAction>() && IsShift())
			{
				(w as IOnShiftAction).OnShiftAction();
			}
			else if (w.Is<IOnCtrlAction>() && IsCtrl())
			{
				(w as IOnCtrlAction).OnCtrlAction();
			}
			else if (w.Is<IOnAction>())
			{
				(w as IOnAction).OnAction();
			}
		}
		else if (pointerEventData.button == PointerEventData.InputButton.Right && w.Is<IOnAltAction>())
		{
			(w as IOnAltAction).OnAltAction();
		}
	}

	private void OnDisable()
	{
		if (_w != null)
		{
			if (_hovering && _w.Is<IOnPointerExit>())
			{
				(_w as IOnPointerExit).OnPointerExit();
			}
			if (_pressing && _w.Is<IOnPointerUp>())
			{
				(_w as IOnPointerUp).OnPointerUp();
			}
			_hovering = false;
			_pressing = false;
			_w = null;
		}
	}
}
