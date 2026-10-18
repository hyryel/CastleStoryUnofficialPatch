using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public interface IGetContentType : IWidgetHandle
{
	InputField.ContentType GetContentType();
}
