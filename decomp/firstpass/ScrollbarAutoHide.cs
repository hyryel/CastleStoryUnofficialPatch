using UnityEngine;
using UnityEngine.UI;

public class ScrollbarAutoHide : MonoBehaviour
{
	public Scrollbar uiScrollbar;

	public GameObject uiScrollbarGo;

	private void AutoHide(float v)
	{
		AutoHide();
	}

	public void AutoHide()
	{
		if (uiScrollbar != null && uiScrollbarGo != null)
		{
			uiScrollbarGo.SetActive(Mathf.Round(uiScrollbar.size * 100f) / 100f != 1f);
		}
	}

	private void Update()
	{
		AutoHide();
	}

	private void OnEnable()
	{
		if (uiScrollbar != null)
		{
			AutoHide();
			uiScrollbar.onValueChanged.AddListener(AutoHide);
		}
	}

	private void OnDisable()
	{
		if (uiScrollbar != null)
		{
			uiScrollbar.onValueChanged.RemoveListener(AutoHide);
		}
	}
}
