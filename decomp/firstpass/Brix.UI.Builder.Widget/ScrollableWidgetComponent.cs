using Brix.Game.Utils;
using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class ScrollableWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected ScrollRect _scrollRect;

	[SerializeField]
	protected Scrollbar _scrollbar;

	[SerializeField]
	protected ContentSizeFitter _contentSizeFitter;

	[SerializeField]
	protected VerticalLayoutGroup _layoutGroup;

	[SerializeField]
	protected LayoutElementConstraint _layoutElementConstraint;

	public RectTransform _scrollableRectTransform;

	public RectTransform _contentRectTransform;

	public override bool Refresh(IWidget w)
	{
		if (!base.Refresh(w))
		{
			return false;
		}
		Mute();
		return true;
	}

	private void OnEnable()
	{
		Mute();
	}

	private void Mute()
	{
		LayoutPanelManager.Add(_contentSizeFitter);
		LayoutPanelManager.Add(_layoutGroup);
	}
}
