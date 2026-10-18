using Brix.UI.Builder.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class HorizontalLayoutPanelComponent : BaseLayoutPanelComponent
{
	public RectTransform _bin;

	public HorizontalLayoutGroup _layoutGroup;

	public Image _image;

	public LayoutElement _layoutElement;

	public ContentSizeFitter _fitter;

	public ContentSizeFitterPlus _fitterPlus;

	public HorizontalLayoutGroupWidgetComponent _layoutGroupWidget;

	public LayoutElementWidgetComponent _layoutElementWidget;

	public ImageWidgetComponent _imageWidget;
}
