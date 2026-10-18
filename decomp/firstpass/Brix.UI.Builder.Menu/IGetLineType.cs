using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public interface IGetLineType : IWidgetHandle
{
	InputField.LineType GetLineType();
}
