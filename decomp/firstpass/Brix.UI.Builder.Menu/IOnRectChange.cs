using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface IOnRectChange : IWidgetHandle
{
	void OnRectChange(Vector2 pos, Vector4 lclRect);
}
