using UnityEngine;

namespace Brix.NewUI.LoadingScreen;

public class TestLoadingScreen : MonoBehaviour
{
	public string TextToSet;

	public Color color;

	public float ratio;

	public LoadingScreen loadingScreen;

	public bool AbortLoading;

	public bool AbortRemove;

	public bool Warning;

	public bool WarningDismissed;

	public bool Tip;

	public bool HideTip;

	public bool setProgress;

	public bool changeColor;

	public bool revertColor;

	public bool startSpin;

	public bool stopSpin;

	public bool open;

	public bool close;

	public bool instantOpen;

	public bool instantClose;

	private void Start()
	{
	}

	private void Update()
	{
		if (setProgress)
		{
			loadingScreen.SetProgress(ratio);
			setProgress = false;
		}
		if (Tip)
		{
			loadingScreen.Tip(TextToSet);
			Tip = false;
		}
		if (HideTip)
		{
			loadingScreen.HideTip();
			HideTip = false;
		}
		if (AbortLoading)
		{
			loadingScreen.LoadingAbort(TextToSet);
			AbortLoading = false;
		}
		if (AbortRemove)
		{
			loadingScreen.WarningDismissed();
			AbortRemove = false;
		}
		if (Warning)
		{
			loadingScreen.Warning(TextToSet);
			Warning = false;
		}
		if (WarningDismissed)
		{
			loadingScreen.WarningDismissed();
			WarningDismissed = false;
		}
		if (open)
		{
			loadingScreen.Open();
			open = false;
		}
		if (close)
		{
			loadingScreen.Close();
			close = false;
		}
		if (instantOpen)
		{
			loadingScreen.InstantOpen();
			instantOpen = false;
		}
		if (instantClose)
		{
			loadingScreen.InstantClose();
			instantClose = false;
		}
		if (changeColor)
		{
			loadingScreen.ChangeColor(color);
			changeColor = false;
		}
		if (revertColor)
		{
			loadingScreen.RevertColor();
			revertColor = false;
		}
		if (startSpin)
		{
			loadingScreen.StartSpin();
			startSpin = false;
		}
		if (stopSpin)
		{
			loadingScreen.StopSpin();
			stopSpin = false;
		}
	}
}
