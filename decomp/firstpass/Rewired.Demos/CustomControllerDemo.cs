using System;
using UnityEngine;

namespace Rewired.Demos;

[RequireComponent(typeof(TouchControllerExample))]
[AddComponentMenu("")]
public class CustomControllerDemo : MonoBehaviour
{
	public int playerId;

	public string controllerTag;

	public bool useUpdateCallbacks;

	private int buttonCount;

	private int axisCount;

	private float[] axisValues;

	private bool[] buttonValues;

	private TouchControllerExample touchController;

	private CustomController controller;

	[NonSerialized]
	private bool initialized;

	private void Awake()
	{
		if (SystemInfo.deviceType == DeviceType.Handheld && Screen.orientation != ScreenOrientation.LandscapeLeft)
		{
			Screen.orientation = ScreenOrientation.LandscapeLeft;
		}
		Initialize();
	}

	private void Initialize()
	{
		ReInput.InputSourceUpdateEvent += OnInputSourceUpdate;
		touchController = GetComponent<TouchControllerExample>();
		axisCount = touchController.joysticks.Length * 2;
		buttonCount = touchController.buttons.Length;
		axisValues = new float[axisCount];
		buttonValues = new bool[buttonCount];
		Player player = ReInput.players.GetPlayer(playerId);
		controller = player.controllers.GetControllerWithTag<CustomController>(controllerTag);
		if (controller == null)
		{
			Debug.LogError("A matching controller was not found for tag \"" + controllerTag + "\"");
		}
		if (controller.buttonCount != buttonValues.Length || controller.axisCount != axisValues.Length)
		{
			Debug.LogError("Controller has wrong number of elements!");
		}
		if (useUpdateCallbacks && controller != null)
		{
			controller.SetAxisUpdateCallback(GetAxisValueCallback);
			controller.SetButtonUpdateCallback(GetButtonValueCallback);
		}
		initialized = true;
	}

	private void Update()
	{
		if (ReInput.isReady && !initialized)
		{
			Initialize();
		}
	}

	private void OnInputSourceUpdate()
	{
		GetSourceAxisValues();
		GetSourceButtonValues();
		if (!useUpdateCallbacks)
		{
			SetControllerAxisValues();
			SetControllerButtonValues();
		}
	}

	private void GetSourceAxisValues()
	{
		for (int i = 0; i < axisValues.Length; i++)
		{
			if (i % 2 != 0)
			{
				axisValues[i] = touchController.joysticks[i / 2].position.y;
			}
			else
			{
				axisValues[i] = touchController.joysticks[i / 2].position.x;
			}
		}
	}

	private void GetSourceButtonValues()
	{
		for (int i = 0; i < buttonValues.Length; i++)
		{
			buttonValues[i] = touchController.buttons[i].isPressed;
		}
	}

	private void SetControllerAxisValues()
	{
		for (int i = 0; i < axisValues.Length; i++)
		{
			controller.SetAxisValue(i, axisValues[i]);
		}
	}

	private void SetControllerButtonValues()
	{
		for (int i = 0; i < buttonValues.Length; i++)
		{
			controller.SetButtonValue(i, buttonValues[i]);
		}
	}

	private float GetAxisValueCallback(int index)
	{
		if (index >= axisValues.Length)
		{
			return 0f;
		}
		return axisValues[index];
	}

	private bool GetButtonValueCallback(int index)
	{
		if (index >= buttonValues.Length)
		{
			return false;
		}
		return buttonValues[index];
	}
}
