using Brix.UI.Builder.Widget;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class DropdownPanelComponent : BasePanelComponent
{
	public Dropdown _dropdown;

	public Image _arrowImage;

	public LayoutElement _keyLayoutElement;

	public Text _keyLabel;

	public Image _keyBgImage;

	public Text _valueLabel;

	public Image _valueImage;

	public Toggle _itemToggle;

	public EventTrigger _eventTrigger;

	public EventTrigger _itemEventTrigger;

	public GameObject _spacerGo;

	public LayoutElementWidgetComponent _layoutElementWidget;

	public ImageWidgetComponent _arrowImageWidget;

	public ImageWidgetComponent _valueCheckImageWidget;

	public ImageWidgetComponent _valueImageWidget;

	public LabelWidgetComponent _valueLabelWidget;

	public ImageWidgetComponent _valueTemplateImageWidget;

	public LabelWidgetComponent _valueTemplateLabelWidget;

	public DropdownWidgetComponent _dropdownWidget;
}
