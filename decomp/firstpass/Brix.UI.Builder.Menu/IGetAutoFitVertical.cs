using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public interface IGetAutoFitVertical : IWidgetHandle
{
	ContentSizeFitter.FitMode GetAutoFitVertical();
}
