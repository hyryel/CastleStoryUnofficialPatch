using Brix.UI.Builder.Widget;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class SquareButtonPanelComponent : BasePanelComponent
{
	public RectTransform _bin;

	public ButtonWidgetComponent _buttonWidget;

	public ImageLabelAnimatorWidgetComponent _spriteLabelWidget;

	public StateAnimatorWidgetComponent _stateAnimatorWidget;

	public FrameAnimatorWidgetComponent _frameAnimatorWidget;

	public LabelWidgetComponent _hotkeyWidget;

	public LayoutElementWidgetComponent _layoutElement;

	public ImageWidgetComponent _hoverBgImageWidget;

	public ImageWidgetComponent _hoverImageWidget;

	public LabelWidgetComponent _hoverLabelWidget;
}
