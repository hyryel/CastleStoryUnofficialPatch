using Brix.UI.Builder.Menu;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class DropdownWidgetComponent : BaseWidgetComponent
{
	public Dropdown _dropdown;

	public EventTrigger _eventTrigger;

	public RectTransform _contentRectTrs;

	public bool shown;

	public UnityEvent m_OnShow = new UnityEvent();

	public UnityEvent m_OnHide = new UnityEvent();

	public Dropdown.DropdownEvent m_OnPointerClickAt = new Dropdown.DropdownEvent();

	public void OnShow()
	{
		if (!shown)
		{
			shown = true;
			m_OnShow.Invoke();
		}
	}

	public void OnHide()
	{
		if (shown)
		{
			shown = false;
			m_OnHide.Invoke();
		}
	}

	public void OnPointerClickAt(int v)
	{
		m_OnPointerClickAt.Invoke(v);
	}

	private void OnDisable()
	{
		_dropdown.Hide();
	}

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		_dropdown.onValueChanged.RemoveAllListeners();
		if (GetOptions(w))
		{
			if (w.Is<IGetSelection>())
			{
				_dropdown.value = (w as IGetSelection).GetSelection();
			}
			if (w.Is<ISetSelection>())
			{
				m_OnPointerClickAt.RemoveAllListeners();
				if (w.Is<ISetSelection>())
				{
					m_OnPointerClickAt.AddListener(delegate(int i)
					{
						(w as ISetSelection).SetSelection(i);
					});
				}
				m_OnHide.RemoveAllListeners();
				if (w.Is<IOnHide>())
				{
					m_OnHide.AddListener(delegate
					{
						(w as IOnHide).OnHide();
					});
				}
				m_OnShow.RemoveAllListeners();
				if (w.Is<IOnShow>())
				{
					m_OnShow.AddListener(delegate
					{
						(w as IOnShow).OnShow();
					});
				}
			}
		}
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
		return true;
	}

	private bool GetOptions(IWidget w)
	{
		_dropdown.options.Clear();
		if (!w.Is<IGetOptions>())
		{
			return false;
		}
		DynValue options = (w as IGetOptions).GetOptions();
		if (options.IsNil())
		{
			return false;
		}
		Table table = options.Table;
		foreach (DynValue key in table.Keys)
		{
			Table table2 = table.Get(key).Table;
			DynValue dynValue = table2.Get("image");
			DynValue dynValue2 = table2.Get("text");
			Sprite sprite = ((!dynValue.IsNil()) ? dynValue.ToObject<Sprite>() : null);
			string text = ((!dynValue2.IsNil()) ? dynValue2.String : string.Empty);
			_dropdown.itemImage.enabled = sprite != null;
			_dropdown.options.Add(new Dropdown.OptionData(text, sprite));
		}
		if (w.Is<IGetAnchoredHeight>())
		{
			_contentRectTrs.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (w as IGetAnchoredHeight).GetAnchoredHeight());
		}
		return true;
	}
}
