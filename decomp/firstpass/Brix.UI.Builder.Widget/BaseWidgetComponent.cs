using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Widget;

public abstract class BaseWidgetComponent : MonoBehaviour, IWidgetComponent
{
	public virtual bool Refresh(IWidget w)
	{
		return true;
	}

	public bool Refresh(IWidget w, int phase)
	{
		return Refresh(w);
	}
}
