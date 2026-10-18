using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Shell;

public abstract class Layout
{
	public abstract Rect GetPosition();

	public abstract string CodeInput(string s);

	public abstract void CodeOutput(string s, bool richText);

	public abstract bool Window(string title, Action draw, params GUILayoutOption[] options);

	public abstract GameObject ObjectField(GameObject o, params GUILayoutOption[] options);

	public virtual void Repaint()
	{
	}

	public virtual void DrawTooltip(string tooltip, Vector2 mousePosition)
	{
	}

	public virtual void FocusControl(string name)
	{
		GUI.FocusControl(name);
	}

	public virtual bool Foldout(bool b, string s, params GUILayoutOption[] options)
	{
		return Foldout(b, s, GUI.skin.textField, options);
	}

	public virtual bool Foldout(bool b, string s, GUIStyle style, params GUILayoutOption[] options)
	{
		Rect rect = GUILayoutUtility.GetRect(new GUIContent(s), style, options);
		return Foldout(rect, b, s, style);
	}

	public virtual bool Foldout(Rect rect, bool b, string s, GUIStyle style)
	{
		string text = (b ? "▲" : "▼");
		Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = Color.Lerp(Color.black, backgroundColor, 0.7f);
		if (!GUI.Toggle(rect, value: true, text + " " + s, style))
		{
			b = !b;
		}
		GUI.backgroundColor = backgroundColor;
		return b;
	}

	public virtual void BeginIndentedSection()
	{
		Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = Color.Lerp(Color.black, backgroundColor, 0.7f);
		GUILayout.BeginHorizontal("TextArea");
		GUI.backgroundColor = backgroundColor;
		GUILayout.Space(3f);
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}

	public virtual void EndIndentedSection()
	{
		GUILayout.Space(2f);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	public virtual string DynamicList(string newElem, IList<string> list)
	{
		int num = -1;
		for (int i = 0; i < list.Count; i++)
		{
			if (GUILayout.Button(list[i]))
			{
				num = i;
			}
		}
		GUILayout.BeginHorizontal();
		newElem = GUILayout.TextField(newElem, GUILayout.ExpandWidth(expand: true), GUILayout.MinHeight(20f));
		if (GUILayout.Button("+", GUILayout.ExpandWidth(expand: false)))
		{
			list.Add(newElem);
			newElem = "";
		}
		GUILayout.EndHorizontal();
		if (num >= 0)
		{
			list.RemoveAt(num);
		}
		return newElem;
	}
}
