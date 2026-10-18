using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public interface IGetFillMethod : IWidgetHandle
{
	Image.FillMethod GetFillMethod();
}
