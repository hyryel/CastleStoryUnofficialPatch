using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public interface IGetAutoFitHorizontal : IWidgetHandle
{
	ContentSizeFitter.FitMode GetAutoFitHorizontal();
}
