using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class InputFieldWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	private InputField _inputField;

	[SerializeField]
	private EventTrigger _eventTrigger;

	[SerializeField]
	private Text _valueText;

	[SerializeField]
	private Text _placeholderText;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (w.Is<IGetInteractable>())
		{
			_inputField.interactable = (w as IGetInteractable).GetInteractable();
		}
		if (w.Is<IGetContentType>())
		{
			_inputField.contentType = (w as IGetContentType).GetContentType();
		}
		if (w.Is<IGetLineType>())
		{
			_inputField.lineType = (w as IGetLineType).GetLineType();
		}
		if (w.Is<IGetCharacterLimit>())
		{
			_inputField.characterLimit = (w as IGetCharacterLimit).GetCharacterLimit();
		}
		if (w.Is<IGetControlColor>())
		{
			_inputField.selectionColor = (w as IGetControlColor).GetControlColor();
		}
		if (w.Is<IGetDoSelect>() && (w as IGetDoSelect).GetDoSelect())
		{
			_inputField.Select();
		}
		_inputField.onEndEdit.RemoveAllListeners();
		if (w.Is<IGetTextValue>())
		{
			_inputField.text = (w as IGetTextValue).GetTextValue();
		}
		if (w.Is<ISetTextValue>())
		{
			_inputField.onEndEdit.AddListener(delegate(string value)
			{
				(w as ISetTextValue).SetTextValue(value);
			});
		}
		_eventTrigger.triggers.Clear();
		if (w.Is<IOnSelect>())
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Select;
			entry.callback = new EventTrigger.TriggerEvent();
			entry.callback.AddListener(delegate
			{
				(w as IOnSelect).OnSelect();
			});
			_eventTrigger.triggers.Add(entry);
		}
		if (w.Is<IOnDeselect>())
		{
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.Deselect;
			entry2.callback = new EventTrigger.TriggerEvent();
			entry2.callback.AddListener(delegate
			{
				(w as IOnDeselect).OnDeselect();
			});
			_eventTrigger.triggers.Add(entry2);
		}
		if (w.Is<IOnPointerEnter>())
		{
			EventTrigger.Entry entry3 = new EventTrigger.Entry();
			entry3.eventID = EventTriggerType.PointerEnter;
			entry3.callback = new EventTrigger.TriggerEvent();
			entry3.callback.AddListener(delegate
			{
				(w as IOnPointerEnter).OnPointerEnter();
			});
			_eventTrigger.triggers.Add(entry3);
		}
		if (w.Is<IOnPointerExit>())
		{
			EventTrigger.Entry entry4 = new EventTrigger.Entry();
			entry4.eventID = EventTriggerType.PointerExit;
			entry4.callback = new EventTrigger.TriggerEvent();
			entry4.callback.AddListener(delegate
			{
				(w as IOnPointerExit).OnPointerExit();
			});
			_eventTrigger.triggers.Add(entry4);
		}
		return true;
	}
}
