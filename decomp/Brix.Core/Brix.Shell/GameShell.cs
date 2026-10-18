using System;
using System.IO;
using UnityEngine;

namespace Brix.Shell;

public class GameShell : MonoBehaviour
{
	public static bool AllowKeyboardActivation = false;

	public string SettingsPath = "Shell/settings.runtime.xml";

	public string DatabasePath = "Shell/Database";

	public bool visible = false;

	private Data data;

	private Drawer drawer;

	private GameLayout layout;

	private bool crashed = false;

	private void OnEnable()
	{
		if (crashed)
		{
			return;
		}
		try
		{
			data = new Data();
			drawer = new Drawer();
			layout = new GameLayout();
			if (File.Exists(SettingsPath))
			{
				data = Data.LoadFromXML(File.ReadAllText(SettingsPath));
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("GameShell initialization failed.\n" + ex);
			crashed = true;
		}
	}

	private void OnDisable()
	{
		if (crashed)
		{
			return;
		}
		try
		{
			FileInfo fileInfo = new FileInfo(SettingsPath);
			if (!fileInfo.Exists && !fileInfo.Directory.Exists)
			{
				Directory.CreateDirectory(fileInfo.DirectoryName);
			}
			File.WriteAllText(SettingsPath, data.ToXML());
		}
		catch (Exception ex)
		{
			Debug.LogWarning("GameShell cleanup failed.\n" + ex);
		}
	}

	private void OnGUI()
	{
		if (!crashed)
		{
			if (visible)
			{
				Rect rect = new Rect(50f, 50f, Screen.width - 100, Screen.height - 100);
				layout.position = rect;
				data.database.path = DatabasePath;
				drawer.data = data;
				drawer.layout = layout;
				GUILayout.BeginArea(rect);
				drawer.OnGUI();
				GUILayout.EndArea();
			}
			HandleEvents();
		}
	}

	private void HandleEvents()
	{
		if (!crashed && AllowKeyboardActivation)
		{
			Event current = Event.current;
			if (current != null && current.type == EventType.KeyUp && current.keyCode == KeyCode.F10 && current.shift)
			{
				visible = !visible;
			}
		}
	}
}
