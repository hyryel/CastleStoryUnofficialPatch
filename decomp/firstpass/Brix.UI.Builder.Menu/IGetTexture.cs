using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface IGetTexture : IWidgetHandle
{
	Texture2D GetTexture();
}
