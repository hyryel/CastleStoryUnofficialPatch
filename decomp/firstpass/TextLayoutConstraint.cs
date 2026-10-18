using UnityEngine;
using UnityEngine.UI;

public class TextLayoutConstraint : MonoBehaviour
{
	public Text _text;

	public LayoutElement _layoutElement;

	[ContextMenu("ConstrainMinWidth")]
	public void ConstrainMinWidth()
	{
		float minWidth = _text.minWidth;
		if (_layoutElement.minWidth != minWidth)
		{
			_layoutElement.minWidth = minWidth;
		}
	}

	[ContextMenu("ConstrainMinHeight")]
	public void ConstrainMinHeight()
	{
		float minHeight = _text.minHeight;
		if (_layoutElement.minHeight != minHeight)
		{
			_layoutElement.minHeight = minHeight;
		}
	}

	[ContextMenu("ConstrainPreferredWidth")]
	public void ConstrainPreferredWidth()
	{
		float preferredWidth = _text.preferredWidth;
		if (_layoutElement.preferredWidth != preferredWidth)
		{
			_layoutElement.preferredWidth = preferredWidth;
		}
	}

	[ContextMenu("ConstrainPreferredHeight")]
	public void ConstrainPreferredHeight()
	{
		float preferredHeight = _text.preferredHeight;
		if (_layoutElement.preferredHeight != preferredHeight)
		{
			_layoutElement.preferredHeight = preferredHeight;
		}
	}
}
