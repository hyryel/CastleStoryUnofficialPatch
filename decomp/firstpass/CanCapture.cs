using System;
using Brix.Game.Components;

public class CanCapture : GameComponent
{
	[Flags]
	public enum CaptureActions
	{
		Defend = 1,
		Attack = 2,
		Neutralize = 4
	}

	public int CapturePower;

	private int _lastCapturePower;

	[EnumFlag]
	public CaptureActions AllowedCaptureActions;

	public override void Initialize()
	{
		base.Initialize();
		_lastCapturePower = CapturePower;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		ToggleCapturePower(base.enabled);
	}

	private void ToggleCapturePower(bool value)
	{
		if (value)
		{
			CapturePower = _lastCapturePower;
			return;
		}
		_lastCapturePower = CapturePower;
		CapturePower = 0;
	}

	private void OnEnable()
	{
		ToggleCapturePower(value: true);
	}

	private void OnDisable()
	{
		ToggleCapturePower(value: false);
	}
}
