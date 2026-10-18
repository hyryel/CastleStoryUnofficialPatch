using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Demos;

[RequireComponent(typeof(GUITexture))]
[AddComponentMenu("")]
public class TouchButtonExample : MonoBehaviour
{
	private class Boundary
	{
		public Vector2 min = Vector2.zero;

		public Vector2 max = Vector2.zero;
	}

	public bool allowMouseControl = true;

	public int tapCount = -1;

	private int lastFingerId = -1;

	private float tapTimeWindow;

	private float firstDeltaTime;

	private GUITexture gui;

	private Rect defaultRect;

	private Boundary guiBoundary = new Boundary();

	private Vector2 guiTouchOffset;

	private Vector2 guiCenter;

	private bool mouseActive;

	private int lastScreenWidth;

	private Rect origPixelInset;

	private Vector3 origTransformPosition;

	private static List<TouchButtonExample> buttons;

	private static float tapTimeDelta = 0.3f;

	public bool isFingerDown => lastFingerId != -1;

	public bool isPressed { get; private set; }

	private void Reset()
	{
	}

	private void Awake()
	{
		if (SystemInfo.deviceType == DeviceType.Handheld)
		{
			allowMouseControl = false;
		}
		gui = GetComponent<GUITexture>();
		if (gui.texture == null)
		{
			Debug.LogError("TouchButton object requires a valid texture!");
			base.gameObject.SetActive(value: false);
		}
		else
		{
			origPixelInset = gui.pixelInset;
			origTransformPosition = base.transform.position;
			RefreshPosition();
		}
	}

	private void RefreshPosition()
	{
		defaultRect = origPixelInset;
		defaultRect.x += origTransformPosition.x * (float)Screen.width;
		defaultRect.y += origTransformPosition.y * (float)Screen.height;
		gui.pixelInset = defaultRect;
		base.transform.position = new Vector3(0f, 0f, base.transform.position.z);
		guiTouchOffset.x = defaultRect.width * 0.5f;
		guiTouchOffset.y = defaultRect.height * 0.5f;
		guiCenter.x = defaultRect.x + guiTouchOffset.x;
		guiCenter.y = defaultRect.y + guiTouchOffset.y;
		guiBoundary.min.x = defaultRect.x - guiTouchOffset.x;
		guiBoundary.max.x = defaultRect.x + guiTouchOffset.x;
		guiBoundary.min.y = defaultRect.y - guiTouchOffset.y;
		guiBoundary.max.y = defaultRect.y + guiTouchOffset.y;
		lastScreenWidth = Screen.width;
		Restart();
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
		isPressed = false;
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
			return;
		}
		for (int i = 0; i < num; i++)
		{
			Vector2 vector;
			int num2;
			int num3;
			TouchPhase touchPhase;
			if (mouseActive)
			{
				vector = ReInput.controllers.Mouse.screenPosition;
				num2 = 0;
				num3 = 1;
				touchPhase = TouchPhase.Moved;
			}
			else
			{
				Touch touch = ReInput.touch.GetTouch(i);
				vector = touch.position;
				num2 = touch.fingerId;
				num3 = touch.tapCount;
				touchPhase = touch.phase;
			}
			if (!gui.HitTest(vector))
			{
				continue;
			}
			if (lastFingerId == -1 || lastFingerId != num2)
			{
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
			}
			if (lastFingerId == num2)
			{
				if (num3 > tapCount)
				{
					tapCount = num3;
				}
				isPressed = true;
				if (touchPhase == TouchPhase.Ended || touchPhase == TouchPhase.Canceled)
				{
					Restart();
				}
			}
		}
	}
}
