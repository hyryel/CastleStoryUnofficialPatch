using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface IGetPadding : IWidgetHandle
{
	RectOffset GetPadding();
}
