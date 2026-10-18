using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Shell;

public class GameLayout : Layout
{
	public Rect position;

	private Dictionary<string, int> windowIDs = new Dictionary<string, int>();

	private List<Rect> windowRects = new List<Rect>();

	private Action windowDraw = null;

	private bool windowClose = false;

	public GameLayout()
	{
	}

	public GameLayout(Rect position)
	{
		this.position = position;
	}

	public override Rect GetPosition()
	{
		return position;
	}

	public override string CodeInput(string s)
	{
		GUI.skin.textArea.richText = false;
		float minHeight = GUI.skin.textArea.CalcHeight(new GUIContent(s), float.PositiveInfinity);
		return GUILayout.TextArea(s, GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: true), GUILayout.MinHeight(minHeight));
	}

	public override void CodeOutput(string s, bool richText)
	{
		if (s.Length > 16000)
		{
			s = s.Substring(s.Length - 16000, 16000);
		}
		GUI.skin.textArea.richText = richText;
		float minHeight = GUI.skin.textArea.CalcHeight(new GUIContent(s), float.PositiveInfinity);
		GUILayout.TextArea(s, GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: true), GUILayout.MinHeight(minHeight));
	}

	private void WindowFunction(int id)
	{
		if (GUI.Button(new Rect(windowRects[id].width - 20f, 0f, 20f, 20f), "X", "Label"))
		{
			windowClose = true;
		}
		windowDraw();
		GUI.DragWindow();
	}

	public override bool Window(string title, Action draw, params GUILayoutOption[] options)
	{
		if (!windowIDs.TryGetValue(title, out var value))
		{
			value = windowRects.Count;
			Vector2 vector = new Vector2(position.width / 2f, position.height / 2f);
			Vector2 vector2 = new Vector2(300f, 300f);
			windowRects.Add(new Rect(vector.x - vector2.x / 2f, vector.y - vector2.y / 2f, vector2.x, vector2.y));
			windowIDs.Add(title, value);
		}
		windowDraw = draw;
		windowRects[value] = GUILayout.Window(value, windowRects[value], WindowFunction, title, options);
		if (windowClose)
		{
			windowClose = false;
			return false;
		}
		return true;
	}

	public override GameObject ObjectField(GameObject o, params GUILayoutOption[] options)
	{
		return null;
	}
}
