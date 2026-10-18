using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class VerticalLayoutGroupWidgetComponent : BaseLayoutGroupWidgetComponent
{
	public LuiLayoutGroupWidget LuiLayoutGroupWidget = new LuiLayoutGroupWidget();

	[SerializeField]
	protected VerticalLayoutGroup _layoutGroup;

	[SerializeField]
	protected ContentSizeFitter _csf;

	public void Refresh_NEW()
	{
		if (_layoutGroup.spacing != LuiLayoutGroupWidget.Spacing.Value)
		{
			_layoutGroup.spacing = LuiLayoutGroupWidget.Spacing.Value;
		}
	}

	public override bool Refresh(IWidget w)
	{
		if (!base.Refresh(w))
		{
			return false;
		}
		if (w.Is<IGetSpacing>())
		{
			float spacing = (w as IGetSpacing).GetSpacing();
			if (_layoutGroup.spacing != spacing)
			{
				_layoutGroup.spacing = spacing;
			}
		}
		if (w.Is<IGetPadding>())
		{
			RectOffset padding = (w as IGetPadding).GetPadding();
			if (_layoutGroup.padding != padding)
			{
				_layoutGroup.padding = padding;
			}
		}
		if (w.Is<IGetAlignment>())
		{
			TextAnchor alignment = (w as IGetAlignment).GetAlignment();
			if (_layoutGroup.childAlignment != alignment)
			{
				_layoutGroup.childAlignment = alignment;
			}
		}
		if (w.Is<IGetChildForceExpandWidth>())
		{
			bool childForceExpandWidth = (w as IGetChildForceExpandWidth).GetChildForceExpandWidth();
			if (_layoutGroup.childForceExpandWidth != childForceExpandWidth)
			{
				_layoutGroup.childForceExpandWidth = childForceExpandWidth;
			}
		}
		if (w.Is<IGetChildForceExpandHeight>())
		{
			bool childForceExpandHeight = (w as IGetChildForceExpandHeight).GetChildForceExpandHeight();
			if (_layoutGroup.childForceExpandHeight != childForceExpandHeight)
			{
				_layoutGroup.childForceExpandHeight = childForceExpandHeight;
			}
		}
		return true;
	}
}
