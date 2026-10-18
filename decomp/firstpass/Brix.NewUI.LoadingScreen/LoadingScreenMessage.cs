using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.LoadingScreen;

public class LoadingScreenMessage : MonoBehaviour
{
	[SerializeField]
	private GameObject Content;

	[SerializeField]
	private Text Body;

	[SerializeField]
	private Text Title;

	[SerializeField]
	private GameObject ButtonLeft;

	[SerializeField]
	private GameObject ButtonMid;

	[SerializeField]
	private GameObject ButtonRight;

	private void ShowMessage(string title, string message)
	{
		Content.SetActive(value: true);
		Title.GetComponent<Localize>().SetTerm(title);
		Body.GetComponent<Localize>().SetTerm(message);
	}

	public void WithoutButtons(string title, string message)
	{
		ShowMessage(title, message);
		ButtonLeft.SetActive(value: false);
		ButtonMid.SetActive(value: false);
		ButtonRight.SetActive(value: false);
	}

	public void WithMidButton(string title, string message)
	{
		ShowMessage(title, message);
		ButtonLeft.SetActive(value: false);
		ButtonMid.SetActive(value: true);
		ButtonRight.SetActive(value: false);
	}

	public void WithTwoButtons(string title, string message)
	{
		ShowMessage(title, message);
		ButtonLeft.SetActive(value: true);
		ButtonMid.SetActive(value: false);
		ButtonRight.SetActive(value: true);
	}

	public void Hide()
	{
		if (!(Content == null))
		{
			Content.SetActive(value: false);
		}
	}
}
