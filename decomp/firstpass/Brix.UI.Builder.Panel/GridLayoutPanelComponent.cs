using Brix.UI.Builder.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class GridLayoutPanelComponent : BaseLayoutPanelComponent
{
	public RectTransform _bin;

	public GridLayoutGroup _layoutGroup;

	public Image _image;

	public LayoutElement _layoutElement;

	public ContentSizeFitter _fitter;

	public ContentSizeFitterPlus _fitterPlus;

	public LayoutConstraintB _layoutConstraint;

	public GridLayoutWidgetComponent _layoutGroupWidget;

	public LayoutElementWidgetComponent _layoutElementWidget;

	public ImageWidgetComponent _imageWidget;
}
