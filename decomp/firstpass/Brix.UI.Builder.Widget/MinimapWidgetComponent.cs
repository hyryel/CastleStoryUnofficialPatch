using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Widget;

public class MinimapWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected MiniMap _minimapController;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		return true;
	}
}
