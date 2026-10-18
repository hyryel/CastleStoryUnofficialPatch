using UnityEngine;

namespace Brix.RuntimeShell.Out;

public class GUIDrawer : RichTextDrawer
{
	[HideInInspector]
	public Vector2 scrollResults;

	public override void Refresh()
	{
	}

	public void DrawGUI(params GUILayoutOption[] options)
	{
		GUILayout.BeginVertical(options);
		scrollResults = GUILayout.BeginScrollView(scrollResults);
		string text = output;
		if (GUI.GetNameOfFocusedControl() != "RuntimeShell.Results")
		{
			GUI.skin.textArea.richText = true;
			text = richOutput;
		}
		GUI.SetNextControlName("RuntimeShell.Results");
		GUILayout.TextArea(text, GUILayout.ExpandHeight(expand: true));
		GUILayout.EndScrollView();
		if (GUILayout.Button("Clear"))
		{
			output = (richOutput = string.Empty);
		}
		GUILayout.EndVertical();
	}
}
