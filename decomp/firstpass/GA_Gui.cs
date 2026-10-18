using System.Collections;
using UnityEngine;

public class GA_Gui : MonoBehaviour
{
	public enum WindowType
	{
		None,
		MessageTypeWindow,
		FeedbackWindow,
		BugWindow
	}

	[HideInInspector]
	public bool GuiEnabled;

	[HideInInspector]
	public bool GuiAllowScreenshot;

	private WindowType _windowType;

	private bool _popUpError;

	private bool _takeScreenshot;

	private Texture2D _screenshot;

	private Rect _messageTypeWindowRect;

	private Rect _feedbackWindowRect;

	private Rect _bugWindowRect;

	private string _topic = string.Empty;

	private string _details = string.Empty;

	private Texture2D keyTexture;

	private void Start()
	{
		_messageTypeWindowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 75, 400f, 150f);
		if (GuiAllowScreenshot)
		{
			_feedbackWindowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 170, 400f, 340f);
			_bugWindowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 170, 400f, 340f);
		}
		else
		{
			_feedbackWindowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 150, 400f, 300f);
			_bugWindowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 150, 400f, 300f);
		}
	}

	private void OnGUI()
	{
		Texture2D background = GUI.skin.window.onNormal.background;
		GUI.skin.window.onNormal.background = GUI.skin.window.normal.background;
		if (GuiEnabled || _windowType != WindowType.None)
		{
			switch (_windowType)
			{
			case WindowType.None:
				if (GUI.Button(new Rect(Screen.width - 55, Screen.height - 55, 50f, 50f), GA.SettingsGA.Logo))
				{
					_windowType = WindowType.MessageTypeWindow;
				}
				break;
			case WindowType.MessageTypeWindow:
				_messageTypeWindowRect = GUI.Window(0, _messageTypeWindowRect, MessageTypeWindow, string.Empty);
				break;
			case WindowType.FeedbackWindow:
				if (_takeScreenshot)
				{
					StartCoroutine(TakeScreenshot());
				}
				else
				{
					_feedbackWindowRect = GUI.Window(0, _feedbackWindowRect, FeedbackWindow, string.Empty);
				}
				break;
			case WindowType.BugWindow:
				if (_takeScreenshot)
				{
					StartCoroutine(TakeScreenshot());
				}
				else
				{
					_bugWindowRect = GUI.Window(0, _bugWindowRect, BugWindow, string.Empty);
				}
				break;
			}
		}
		GUI.skin.window.onNormal.background = background;
	}

	private void MessageTypeWindow(int windowID)
	{
		GUI.Label(new Rect(10f, 15f, 380f, 50f), "Help improve this game by submitting feedback/bug reports");
		if (GUI.Button(new Rect(10f, 50f, 185f, 90f), "Feedback"))
		{
			_windowType = WindowType.FeedbackWindow;
			GUI.FocusControl("TopicField");
		}
		if (GUI.Button(new Rect(205f, 50f, 185f, 90f), "Bug Report"))
		{
			_windowType = WindowType.BugWindow;
			GUI.FocusControl("TopicField");
		}
	}

	private void FeedbackWindow(int windowID)
	{
		int num = 0;
		GUI.Label(new Rect(10f, 15f, 380f, 50f), "Submit feedback");
		GUI.Label(new Rect(10f, 50f, 380f, 20f), "Topic*");
		GUI.SetNextControlName("TopicField");
		_topic = GUI.TextField(new Rect(10f, 70f, 380f, 20f), _topic, 50);
		GUI.Label(new Rect(10f, 100f, 380f, 20f), "Details*");
		_details = GUI.TextArea(new Rect(10f, 120f, 380f, 130f), _details, 400);
		if (GuiAllowScreenshot)
		{
			num = 40;
			if (GUI.Button(new Rect(10f, 260f, 130f, 25f), "Take Screenshot"))
			{
				_takeScreenshot = true;
			}
			if (_screenshot != null)
			{
				GUI.Label(new Rect(192f, 262f, 198f, 20f), "Screenshot added.");
				GUI.DrawTexture(new Rect(150f, 256f, 32f, 32f), _screenshot);
			}
			else
			{
				GUI.Label(new Rect(150f, 262f, 240f, 20f), "Screenshot not added.");
			}
		}
		if (GUI.Button(new Rect(10f, 260 + num, 185f, 30f), "Cancel"))
		{
			_topic = string.Empty;
			_details = string.Empty;
			_windowType = WindowType.None;
			_screenshot = null;
		}
		if (GUI.Button(new Rect(205f, 260 + num, 185f, 30f), "Submit") && _topic.Length > 0 && _details.Length > 0)
		{
			GA.API.Error.NewEvent(GA_Error.SeverityType.info, "Feedback [" + _topic + "]: " + _details);
			_topic = string.Empty;
			_details = string.Empty;
			_windowType = WindowType.None;
			_screenshot = null;
		}
	}

	private void BugWindow(int windowID)
	{
		int num = 0;
		if (_popUpError)
		{
			GUI.Label(new Rect(10f, 10f, 385f, 50f), "Oops! It looks like an error just occurred! Please fill out this form with as many details as possible (what you were doing, etc.).");
		}
		else
		{
			GUI.Label(new Rect(10f, 15f, 380f, 50f), "Submit bug report");
		}
		GUI.Label(new Rect(10f, 50f, 380f, 20f), "Topic*");
		GUI.SetNextControlName("TopicField");
		_topic = GUI.TextField(new Rect(10f, 70f, 380f, 20f), _topic, 50);
		GUI.Label(new Rect(10f, 100f, 380f, 20f), "Details*");
		_details = GUI.TextArea(new Rect(10f, 120f, 380f, 130f), _details, 400);
		if (GuiAllowScreenshot)
		{
			num = 40;
			if (GUI.Button(new Rect(10f, 260f, 130f, 25f), "Take Screenshot"))
			{
				_takeScreenshot = true;
			}
			if (_screenshot != null)
			{
				GUI.Label(new Rect(192f, 262f, 198f, 20f), "Screenshot added.");
				GUI.DrawTexture(new Rect(150f, 256f, 32f, 32f), _screenshot);
			}
			else
			{
				GUI.Label(new Rect(150f, 262f, 240f, 20f), "Screenshot not added.");
			}
		}
		if (GUI.Button(new Rect(10f, 260 + num, 185f, 30f), "Cancel"))
		{
			_topic = string.Empty;
			_details = string.Empty;
			_windowType = WindowType.None;
			_popUpError = false;
			_screenshot = null;
		}
		if (GUI.Button(new Rect(205f, 260 + num, 185f, 30f), "Submit") && _topic.Length > 0 && _details.Length > 0)
		{
			if (_popUpError)
			{
				GA.API.Error.NewEvent(GA_Error.SeverityType.info, "Crash Report [" + _topic + "]: " + _details);
			}
			else
			{
				GA.API.Error.NewEvent(GA_Error.SeverityType.info, "Bug Report [" + _topic + "]: " + _details);
			}
			_topic = string.Empty;
			_details = string.Empty;
			_windowType = WindowType.None;
			_popUpError = false;
			_screenshot = null;
		}
	}

	private IEnumerator TakeScreenshot()
	{
		yield return new WaitForEndOfFrame();
		_screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipmap: false);
		_screenshot.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		_screenshot.Apply();
		_takeScreenshot = false;
	}

	public void OpenBugGUI()
	{
		_windowType = WindowType.BugWindow;
		_popUpError = true;
		if (GuiAllowScreenshot)
		{
			_takeScreenshot = true;
		}
	}
}
