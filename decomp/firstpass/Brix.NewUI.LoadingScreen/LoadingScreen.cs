using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.LoadingScreen;

public class LoadingScreen : MonoBehaviour
{
	public Signal AnimationStart = new Signal();

	public Signal AnimationEnd = new Signal();

	public Signal BackButtonPressed = new Signal();

	public Signal CancelButtonPressed = new Signal();

	public Signal AbortButtonPressed = new Signal();

	public Signal OkButtonPressed = new Signal();

	[SerializeField]
	private bool startOpen;

	[SerializeField]
	private Color IssueColor;

	[SerializeField]
	private string ErrorTitle;

	[SerializeField]
	private string TipTitle;

	[SerializeField]
	private string WarningTitle;

	[SerializeField]
	private Text Title;

	[SerializeField]
	private LoadingScreenSlide Slide;

	[SerializeField]
	private LoadingScreenMessage Message;

	[SerializeField]
	private LoadingScreenBars Bars;

	[SerializeField]
	private Rotate1 SpinRotateScript;

	[SerializeField]
	private GameObject BackButtonGO;

	[SerializeField]
	private Image SpinImage;

	private bool spinStopped;

	private bool colored;

	private float spinSpeed = 1f;

	private Color titleColor;

	private Color spinColor;

	private void Start()
	{
		Slide.AnimStart.Connect(AnimationStart.Invoke);
		Slide.AnimEnd.Connect(AnimationEnd.Invoke);
		Title.text = I2Helper.TryGet(Title.text);
		Init();
	}

	private void Init()
	{
		if (startOpen)
		{
			InstantOpen();
		}
		else
		{
			InstantClose();
		}
		SetProgress(0f);
		Message.Hide();
	}

	public void InstantOpen()
	{
		Slide.InstantOpen();
	}

	public void InstantClose()
	{
		Slide.InstantClose();
	}

	public void Open()
	{
		Bars.Show();
		Slide.Open();
		Message.Hide();
	}

	public void Close()
	{
		Bars.Show();
		Slide.Close();
		Message.Hide();
	}

	public void OnButtonOK()
	{
		OkButtonPressed.Invoke();
	}

	public void OnButtonBack()
	{
		BackButtonPressed.Invoke();
	}

	public void OnButtonCancel()
	{
		BackButtonPressed.Invoke();
	}

	public void Tip(string tip)
	{
		Message.WithoutButtons(TipTitle, tip);
	}

	public void HideTip()
	{
		Message.Hide();
	}

	public void LoadingAbort(string reason)
	{
		ChangeColor(IssueColor);
		StopSpin();
		Message.WithMidButton(ErrorTitle, reason);
	}

	public void Warning(string message)
	{
		ChangeColor(IssueColor);
		StopSpin();
		Message.WithTwoButtons(WarningTitle, message);
	}

	public void WarningDismissed()
	{
		RevertColor();
		StartSpin();
		Message.Hide();
	}

	public void SetProgress(float ratio)
	{
		Bars.SetProgress(Mathf.Clamp(ratio, 0f, 1f));
	}

	public void ChangeColor(Color color)
	{
		if (!colored)
		{
			colored = true;
			titleColor = Title.color;
			spinColor = SpinImage.color;
			Bars.SetColor(color);
			Title.color = color;
			SpinImage.color = color;
		}
	}

	public void RevertColor()
	{
		if (colored)
		{
			colored = false;
			Title.color = titleColor;
			SpinImage.color = spinColor;
			Bars.RevertColor();
		}
	}

	public void StopSpin()
	{
		if (SpinRotateScript.Speed != 0f)
		{
			spinSpeed = SpinRotateScript.Speed;
			SpinRotateScript.Speed = 0f;
		}
	}

	public void StartSpin()
	{
		if (SpinRotateScript.Speed == 0f)
		{
			SpinRotateScript.Speed = spinSpeed;
		}
	}

	public void SetLoadingTitle(string text)
	{
		Title.text = I2Helper.TryGet(text);
	}
}
