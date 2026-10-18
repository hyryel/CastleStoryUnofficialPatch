using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Widget;

public class SuffixLabelWidgetComponent : LabelWidgetComponent
{
	public RectTransform _rectTransform;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (w.Is<IGetSuffixLabelText>())
		{
			_label.text = (w as IGetSuffixLabelText).GetSuffixLabelText();
		}
		if (w.Is<IGetSuffixLabelColor>())
		{
			_label.color = (w as IGetSuffixLabelColor).GetSuffixLabelColor();
		}
		if (w.Is<IGetSuffixLabelWidth>())
		{
			_rectTransform.offsetMax = new Vector2((w as IGetSuffixLabelWidth).GetSuffixLabelWidth(_label), _rectTransform.offsetMax.y);
		}
		return true;
	}
}
