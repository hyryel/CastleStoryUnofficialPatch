using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class ScrollableLayoutPanelComponent : BaseLayoutPanelComponent
{
	public RectTransform _bin;

	public LayoutElement _layoutElement;

	public ContentSizeFitter _fitter;

	public ContentSizeFitterPlus _fitterPlus;

	public ScrollRect _scrollRect;

	public Scrollbar _scrollbar;
}
