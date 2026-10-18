using Brix.UI.Builder.Menu;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class FillImageWidgetComponent : ImageWidgetComponent
{
	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (w.Is<IGetFill>())
		{
			_image.type = Image.Type.Filled;
			_image.fillAmount = (w as IGetFill).GetFill();
		}
		return true;
	}
}
