using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface IGetAnchoredPosition : IWidgetHandle
{
	Vector2 GetAnchoredPosition();
}
