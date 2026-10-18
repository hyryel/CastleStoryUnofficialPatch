using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.LoadingScreen;

public class LoadingScreenBars : MonoBehaviour
{
	[SerializeField]
	private GameObject Content;

	[SerializeField]
	private BarFollow Follow;

	[SerializeField]
	private Image MasterBar;

	[SerializeField]
	private Image SlaveBar;

	[SerializeField]
	private Text ProgressText;

	private Color progressTextColor;

	private Color masterBarColor;

	private Color slaveBarColor;

	public void SetColor(Color color)
	{
		progressTextColor = ProgressText.color;
		masterBarColor = MasterBar.color;
		slaveBarColor = SlaveBar.color;
		ProgressText.color = color;
		MasterBar.color = color;
		SlaveBar.color = color;
	}

	public void RevertColor()
	{
		ProgressText.color = progressTextColor;
		MasterBar.color = masterBarColor;
		SlaveBar.color = slaveBarColor;
	}

	public void SetProgress(float ratio)
	{
		Follow.SetMasterValue(ratio);
		ProgressText.text = $"{Mathf.CeilToInt(ratio * 100f).ToString()} %";
	}

	public void Show()
	{
		if (!(Content == null))
		{
			Content.SetActive(value: true);
		}
	}

	public void Hide()
	{
		if (!(Content == null))
		{
			Content.SetActive(value: false);
		}
	}
}
