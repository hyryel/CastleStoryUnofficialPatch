using UnityEngine;

public class ComboBox
{
	private static bool forceToUnShow;

	private static int useControlID = -1;

	private bool isClickedComboButton;

	private int selectedItemIndex;

	public int List(Rect rect, string buttonText, GUIContent[] listContent, GUIStyle listStyle)
	{
		return List(rect, new GUIContent(buttonText), listContent, "button", "box", listStyle);
	}

	public int List(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle)
	{
		return List(rect, buttonContent, listContent, "button", "box", listStyle);
	}

	public int List(Rect rect, string buttonText, GUIContent[] listContent, GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle)
	{
		return List(rect, new GUIContent(buttonText), listContent, buttonStyle, boxStyle, listStyle);
	}

	public int List(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle)
	{
		if (forceToUnShow)
		{
			forceToUnShow = false;
			isClickedComboButton = false;
		}
		bool flag = false;
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		EventType typeForControl = Event.current.GetTypeForControl(controlID);
		if (typeForControl == EventType.MouseUp && isClickedComboButton)
		{
			flag = true;
		}
		if (GUI.Button(rect, buttonContent, buttonStyle))
		{
			if (useControlID == -1)
			{
				useControlID = controlID;
				isClickedComboButton = false;
			}
			if (useControlID != controlID)
			{
				forceToUnShow = true;
				useControlID = controlID;
			}
			isClickedComboButton = true;
		}
		if (isClickedComboButton)
		{
			Rect position = new Rect(rect.x, rect.y + listStyle.CalcHeight(listContent[0], 1f), rect.width, listStyle.CalcHeight(listContent[0], 1f) * (float)listContent.Length);
			GUI.Box(position, string.Empty, boxStyle);
			int num = GUI.SelectionGrid(position, selectedItemIndex, listContent, 1, listStyle);
			if (num != selectedItemIndex)
			{
				selectedItemIndex = num;
			}
		}
		if (flag)
		{
			isClickedComboButton = false;
		}
		return GetSelectedItemIndex();
	}

	public int GetSelectedItemIndex()
	{
		return selectedItemIndex;
	}

	public bool IsShowingList()
	{
		return isClickedComboButton;
	}
}
