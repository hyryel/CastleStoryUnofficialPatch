using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public interface IGetSuffixLabelWidth : IWidgetHandle
{
	float GetSuffixLabelWidth(Text text);
}
