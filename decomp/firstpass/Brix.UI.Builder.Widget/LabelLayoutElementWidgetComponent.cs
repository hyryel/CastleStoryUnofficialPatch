using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Widget;

public class LabelLayoutElementWidgetComponent : LayoutElementWidgetComponent
{
	[SerializeField]
	protected TextLayoutConstraint _textLayoutConstraint;

	public override bool Refresh(IWidget w)
	{
		if (!base.Refresh(w))
		{
			return false;
		}
		if (w.Is<IGetAutoMinWidth>() && (w as IGetAutoMinWidth).GetAutoMinWidth())
		{
			_textLayoutConstraint.ConstrainMinWidth();
		}
		if (w.Is<IGetAutoMinHeight>() && (w as IGetAutoMinHeight).GetAutoMinHeight())
		{
			_textLayoutConstraint.ConstrainMinHeight();
		}
		if (w.Is<IGetAutoPreferredWidth>() && (w as IGetAutoPreferredWidth).GetAutoPreferredWidth())
		{
			_textLayoutConstraint.ConstrainPreferredWidth();
		}
		if (w.Is<IGetAutoPreferredHeight>() && (w as IGetAutoPreferredHeight).GetAutoPreferredHeight())
		{
			_textLayoutConstraint.ConstrainPreferredHeight();
		}
		return true;
	}
}
