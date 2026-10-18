using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface IGetCellSize : IWidgetHandle
{
	Vector2 GetCellSize();
}
