using Brix.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.CenterMessage_;

public class CenterMessage : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Text message;

	private void SetMessage(string msg)
	{
		message.text = msg;
	}

	public void Show(string message, float forTime = -1f)
	{
		SetMessage(message);
		animator.SetBool("Show", value: true);
		if (forTime > 0f)
		{
			Neo.Relais(Hide, forTime, "hide CenterMessage");
		}
	}

	public void Hide()
	{
		animator.SetBool("Show", value: false);
	}
}
