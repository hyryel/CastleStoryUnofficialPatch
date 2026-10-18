using UnityEngine.UI;

namespace Brix.RuntimeShell.Out;

public class UIDrawer : RichTextDrawer
{
	public InputField uiInputField;

	public Text uiText;

	public Scrollbar uiScrollbar;

	public override void Refresh()
	{
		uiInputField.text = output;
		uiText.text = richOutput;
		if (uiScrollbar != null)
		{
			uiScrollbar.value = 0f;
			uiScrollbar.onValueChanged.Invoke(0f);
		}
	}
}
