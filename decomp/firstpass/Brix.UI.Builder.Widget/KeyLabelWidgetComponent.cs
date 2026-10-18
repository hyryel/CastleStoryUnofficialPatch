using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Widget;

public class KeyLabelWidgetComponent : LabelWidgetComponent
{
	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (w.Is<IGetKeyLabelText>())
		{
			_label.text = (w as IGetKeyLabelText).GetKeyLabelText();
		}
		if (w.Is<IGetKeyLabelColor>())
		{
			_label.color = (w as IGetKeyLabelColor).GetKeyLabelColor();
		}
		return true;
	}
}
