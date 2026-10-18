using UnityEngine;

public class Example : MonoBehaviour
{
	private void Start()
	{
		MonoBehaviour.print("Global position: " + ProMouse.Instance.GetGlobalMousePosition());
		MonoBehaviour.print("Local  position: " + ProMouse.Instance.GetLocalMousePosition());
		MonoBehaviour.print("Main screen size: " + ProMouse.Instance.GetMainScreenSize());
		MonoBehaviour.print("Set mouse to center of the unity screen.");
		ProMouse.Instance.SetCursorPosition(Screen.width / 2, Screen.height / 2);
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(0f, 0f, 100f, 20f), "Go Down"))
		{
			ProMouse.Instance.SetCursorPosition(50, 10);
		}
		if (GUI.Button(new Rect(0f, Screen.height - 20, 100f, 20f), "Go Right"))
		{
			ProMouse.Instance.SetCursorPosition(Screen.width - 50, 10);
		}
		if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 20, 100f, 20f), "Go Up"))
		{
			ProMouse.Instance.SetCursorPosition(Screen.width - 50, Screen.height - 10);
		}
		if (GUI.Button(new Rect(Screen.width - 100, 0f, 100f, 20f), "Go Left"))
		{
			ProMouse.Instance.SetCursorPosition(50, Screen.height - 10);
		}
		GUILayout.BeginArea(new Rect(Screen.width / 2 - 125, Screen.height / 2 - 75, 250f, 200f));
		GUILayout.Label(string.Concat("<b>Window Position: ", GetWindowPosition(), "</b>"));
		GUILayout.Space(10f);
		GUILayout.Label("Mouse Coord  Unity: " + ProMouse.Instance.GetLocalMousePosition());
		GUILayout.Label("Mouse Coord Global: " + ProMouse.Instance.GetGlobalMousePosition());
		GUILayout.Label("Main Display  Size: " + ProMouse.Instance.GetMainScreenSize().x + "x" + ProMouse.Instance.GetMainScreenSize().y);
		GUILayout.Label("Player Screen Size: " + Screen.width + "x" + Screen.height);
		GUILayout.Label("Global mouse coordinates:");
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("To 0,0 global"))
		{
			ProMouse.Instance.SetGlobalCursorPosition(0, 0);
		}
		if (GUILayout.Button("To Scr.w,Scr.h"))
		{
			ProMouse.Instance.SetGlobalCursorPosition((int)ProMouse.Instance.GetMainScreenSize().x, (int)ProMouse.Instance.GetMainScreenSize().y);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	private Vector2 GetWindowPosition()
	{
		Vector2 vector = ProMouse.Instance.GetGlobalMousePosition() - ProMouse.Instance.GetLocalMousePosition();
		return new Vector2((int)vector.x, (int)(vector.y + (float)Screen.height));
	}
}
