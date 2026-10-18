using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Demos;

[RequireComponent(typeof(GUITexture))]
[AddComponentMenu("")]
public class TouchJoystickExample : MonoBehaviour
{
	private class Boundary
	{
		public Vector2 min = Vector2.zero;

		public Vector2 max = Vector2.zero;
	}

	public bool allowMouseControl = true;

	public bool touchPad;

	public bool fadeGUI;

	public Vector2 deadZone = Vector2.zero;

	public bool normalize;

	public int tapCount = -1;

	private Rect touchZone;

	private int lastFingerId = -1;

	private float tapTimeWindow;

	private Vector2 fingerDownPos;

	private float firstDeltaTime;

	private GUITexture gui;

	private Rect defaultRect;

	private Boundary guiBoundary = new Boundary();

	private Vector2 guiTouchOffset;

	private Vector2 guiCenter;

	[NonSerialized]
	private bool initialized;

	private bool mouseActive;

	private int lastScreenWidth;

	private Rect origPixelInset;

	private Vector3 origTransformPosition;

	private static List<TouchJoystickExample> joysticks;

	private static bool enumeratedTouchJoysticks;

	private static float tapTimeDelta = 0.3f;

	public bool isFingerDown => lastFingerId != -1;

	public int latchedFinger
	{
		set
		{
			if (lastFingerId == value)
			{
				Restart();
			}
		}
	}

	public Vector2 position { get; private set; }

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		ReInput.EditorRecompileEvent += OnEditorRecompile;
		if (SystemInfo.deviceType == DeviceType.Handheld)
		{
			allowMouseControl = false;
		}
		gui = GetComponent<GUITexture>();
		if (gui.texture == null)
		{
			Debug.LogError("TouchJoystick object requires a valid texture!");
			base.gameObject.SetActive(value: false);
			return;
		}
		if (!enumeratedTouchJoysticks)
		{
			try
			{
				TouchJoystickExample[] array = (TouchJoystickExample[])UnityEngine.Object.FindObjectsOfType(typeof(TouchJoystickExample));
				joysticks = new List<TouchJoystickExample>(array.Length);
				TouchJoystickExample[] array2 = array;
				foreach (TouchJoystickExample item in array2)
				{
					joysticks.Add(item);
				}
				enumeratedTouchJoysticks = true;
			}
			catch (Exception ex)
			{
				Debug.LogError("Error collecting TouchJoystick objects: " + ex.Message);
				throw;
			}
		}
		origPixelInset = gui.pixelInset;
		origTransformPosition = base.transform.position;
		RefreshPosition();
		initialized = true;
	}

	private void RefreshPosition()
	{
		defaultRect = origPixelInset;
		defaultRect.x += origTransformPosition.x * (float)Screen.width;
		defaultRect.y += origTransformPosition.y * (float)Screen.height;
		gui.pixelInset = defaultRect;
		base.transform.position = new Vector3(0f, 0f, base.transform.position.z);
		if (touchPad)
		{
			touchZone = defaultRect;
		}
		else
		{
			guiTouchOffset.x = defaultRect.width * 0.5f;
			guiTouchOffset.y = defaultRect.height * 0.5f;
			guiCenter.x = defaultRect.x + guiTouchOffset.x;
			guiCenter.y = defaultRect.y + guiTouchOffset.y;
			guiBoundary.min.x = defaultRect.x - guiTouchOffset.x;
			guiBoundary.max.x = defaultRect.x + guiTouchOffset.x;
			guiBoundary.min.y = defaultRect.y - guiTouchOffset.y;
			guiBoundary.max.y = defaultRect.y + guiTouchOffset.y;
		}
		lastScreenWidth = Screen.width;
		Restart();
	}

	private void OnEditorRecompile()
	{
		initialized = false;
		enumeratedTouchJoysticks = false;
	}

	public void Enable()
	{
		base.enabled = true;
	}

	public void Disable()
	{
		base.enabled = false;
	}

	public void Restart()
	{
		gui.pixelInset = defaultRect;
		lastFingerId = -1;
		position = Vector2.zero;
		fingerDownPos = Vector2.zero;
		if (touchPad && fadeGUI)
		{
			gui.color = new Color(gui.color.r, gui.color.g, gui.color.b, 0.025f);
		}
		mouseActive = false;
	}

	private void Update()
	{
		if (lastScreenWidth != Screen.width)
		{
			RefreshPosition();
		}
		if (!ReInput.isReady)
		{
			return;
		}
		if (!initialized)
		{
			Initialize();
		}
		if (mouseActive && !ReInput.controllers.Mouse.GetButton(0))
		{
			mouseActive = false;
		}
		int num;
		if (allowMouseControl && (mouseActive || (ReInput.controllers.Mouse.GetButtonDown(0) && gui.HitTest(ReInput.controllers.Mouse.screenPosition))))
		{
			num = 1;
			mouseActive = true;
		}
		else
		{
			num = ReInput.touch.touchCount;
			if (mouseActive)
			{
				mouseActive = false;
			}
		}
		if (tapTimeWindow > 0f)
		{
			tapTimeWindow -= Time.deltaTime;
		}
		else
		{
			tapCount = 0;
		}
		if (num == 0)
		{
			Restart();
		}
		else
		{
			for (int i = 0; i < num; i++)
			{
				Vector2 screenPosition;
				int num2;
				int num3;
				TouchPhase touchPhase;
				if (mouseActive)
				{
					screenPosition = ReInput.controllers.Mouse.screenPosition;
					num2 = 0;
					num3 = 1;
					touchPhase = TouchPhase.Moved;
				}
				else
				{
					Touch touch = ReInput.touch.GetTouch(i);
					screenPosition = touch.position;
					num2 = touch.fingerId;
					num3 = touch.tapCount;
					touchPhase = touch.phase;
				}
				Vector2 vector = screenPosition - guiTouchOffset;
				bool flag = false;
				if (touchPad && touchZone.Contains(screenPosition))
				{
					flag = true;
				}
				else if (gui.HitTest(screenPosition))
				{
					flag = true;
				}
				if (flag && (lastFingerId == -1 || lastFingerId != num2))
				{
					if (touchPad)
					{
						if (fadeGUI)
						{
							gui.color = new Color(gui.color.r, gui.color.g, gui.color.b, 0.15f);
						}
						lastFingerId = num2;
						fingerDownPos = screenPosition;
					}
					lastFingerId = num2;
					if (tapTimeWindow > 0f)
					{
						tapCount++;
					}
					else
					{
						tapCount = 1;
						tapTimeWindow = tapTimeDelta;
					}
					foreach (TouchJoystickExample joystick in joysticks)
					{
						if (!(joystick == this))
						{
							joystick.latchedFinger = num2;
						}
					}
				}
				if (lastFingerId == num2)
				{
					if (num3 > tapCount)
					{
						tapCount = num3;
					}
					if (touchPad)
					{
						position = new Vector2(Mathf.Clamp((screenPosition.x - fingerDownPos.x) / (touchZone.width / 2f), -1f, 1f), Mathf.Clamp((screenPosition.y - fingerDownPos.y) / (touchZone.height / 2f), -1f, 1f));
					}
					else
					{
						gui.pixelInset = new Rect(Mathf.Clamp(vector.x, guiBoundary.min.x, guiBoundary.max.x), Mathf.Clamp(vector.y, guiBoundary.min.y, guiBoundary.max.y), gui.pixelInset.width, gui.pixelInset.height);
					}
					if (touchPhase == TouchPhase.Ended || touchPhase == TouchPhase.Canceled)
					{
						Restart();
					}
				}
			}
		}
		if (!touchPad)
		{
			position = new Vector2((gui.pixelInset.x + guiTouchOffset.x - guiCenter.x) / guiTouchOffset.x, (gui.pixelInset.y + guiTouchOffset.y - guiCenter.y) / guiTouchOffset.y);
		}
		float num4 = Mathf.Abs(position.x);
		float num5 = Mathf.Abs(position.y);
		if (num4 < deadZone.x)
		{
			position = new Vector2(0f, position.y);
		}
		else if (normalize)
		{
			position = new Vector2(Mathf.Sign(position.x) * (num4 - deadZone.x) / (1f - deadZone.x), position.y);
		}
		if (num5 < deadZone.y)
		{
			position = new Vector2(position.x, 0f);
		}
		else if (normalize)
		{
			position = new Vector2(position.x, Mathf.Sign(position.y) * (num5 - deadZone.y) / (1f - deadZone.y));
		}
	}
}
