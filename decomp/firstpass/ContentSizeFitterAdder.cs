using System.Collections.Generic;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContentSizeFitterAdder : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(ContentSizeFitterAdder).FullName);

	public RectTransform baseRectTransform;

	public RectTransform contentRectTransform;

	public List<RectTransform> rectTransforms = new List<RectTransform>();

	public Text fakeText;

	public Scrollbar scrollbar;

	public InputField inputField;

	private float preferedHeight;

	public Button uiSubmitBtn;

	public bool queueResetScrollbarValue;

	private string previousText = string.Empty;

	public bool queueSetPreviousText;

	public bool hasShift => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

	private void SetHeight()
	{
		if (fakeText == null)
		{
			return;
		}
		float num = 0f;
		num = ((!(baseRectTransform == null)) ? baseRectTransform.sizeDelta.y : num);
		float num2 = preferedHeight;
		num2 += 16f;
		num2 += 4f;
		if (num2 < num)
		{
			num2 = num;
			if (scrollbar != null)
			{
				scrollbar.size = 1f;
				scrollbar.value = 0f;
			}
		}
		foreach (RectTransform rectTransform in rectTransforms)
		{
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num2);
		}
		if (contentRectTransform != null)
		{
			Vector2 anchoredPosition = contentRectTransform.anchoredPosition;
			if (anchoredPosition.y > num2)
			{
				contentRectTransform.anchoredPosition = new Vector2(anchoredPosition.x, num2);
			}
		}
	}

	public void Submit(BaseEventData baseEventData)
	{
		logger.Info(string.Concat(base.name, ".Submit()\nbaseEventData = ", baseEventData, "\n"));
	}

	public void SetText(string text)
	{
		if (fakeText != null)
		{
			string empty = string.Empty;
			if (previousText != null && previousText != string.Empty)
			{
				empty = ((!text.Contains(previousText)) ? empty : text.Replace(previousText, string.Empty));
				if (empty.Length == 1 && (empty[0] == '\n' || empty[0] == '\r') && hasShift)
				{
					text = previousText;
					inputField.text = text;
					Submit();
				}
			}
			fakeText.text = text;
			preferedHeight = fakeText.preferredHeight;
			SetHeight();
			if (scrollbar != null)
			{
				scrollbar.value = 0f;
				queueResetScrollbarValue = true;
			}
		}
		previousText = text;
	}

	public void Submit()
	{
		if (uiSubmitBtn != null)
		{
			uiSubmitBtn.onClick.Invoke();
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		SetHeight();
		if (queueResetScrollbarValue)
		{
			queueResetScrollbarValue = false;
			if (scrollbar != null)
			{
				scrollbar.value = 0f;
			}
		}
	}
}
