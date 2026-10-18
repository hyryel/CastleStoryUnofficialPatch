using UnityEngine;
using UnityEngine.EventSystems;

namespace Brix.UI.Builder.Panel;

public class LuaDropdownItem : MonoBehaviour
{
	public int _itemIndex;

	public LuaDropdownPanelComponent _dropdown;

	public EventTrigger _eventTrigger;

	public void OnEnable()
	{
		int siblingIndex = base.transform.GetSiblingIndex();
		RegisterEventTrigger();
		if (siblingIndex == _itemIndex)
		{
			_dropdown._dropdownWidget.OnShow();
		}
	}

	public void OnDisable()
	{
		int siblingIndex = base.transform.GetSiblingIndex();
		UnregisterEventTrigger();
		if (siblingIndex == _itemIndex)
		{
			_dropdown._dropdownWidget.OnHide();
		}
	}

	private void RegisterEventTrigger()
	{
		_eventTrigger.triggers.Clear();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerClick;
		entry.callback = new EventTrigger.TriggerEvent();
		entry.callback.AddListener(delegate
		{
			int siblingIndex = base.transform.GetSiblingIndex();
			_dropdown._dropdownWidget.OnPointerClickAt(siblingIndex);
		});
		_eventTrigger.triggers.Add(entry);
	}

	private void UnregisterEventTrigger()
	{
		_eventTrigger.triggers.Clear();
	}
}
