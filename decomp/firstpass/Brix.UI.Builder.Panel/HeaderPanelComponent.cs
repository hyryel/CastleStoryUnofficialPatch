using Brix.UI.Builder.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class HeaderPanelComponent : BasePanelComponent
{
	public Text _label;

	public LayoutElement _layoutElement;

	public Button _button;

	public GameObject _imageGo;

	public Image _image;

	public LabelWidgetComponent _textWidget;

	public CollapsableWidgetComponent _collapsableWidget;

	public LayoutElementWidgetComponent _layoutElementWidget;
}
