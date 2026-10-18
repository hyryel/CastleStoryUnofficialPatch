using Brix.UI.Builder.Widget;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class SlidableLayoutPanelComponent : BaseLayoutPanelComponent
{
	public RectTransform _socket;

	public LayoutElementWidgetComponent _layoutElementWidget;

	public SlidableWidgetComponent _slidableWidget;

	public LayoutElementWidgetComponent _slidableLayoutElementWidget;

	public VerticalLayoutGroupWidgetComponent _slidableLayoutGroupWidget;

	public ImageWidgetComponent _slidableImageWidget;
}
