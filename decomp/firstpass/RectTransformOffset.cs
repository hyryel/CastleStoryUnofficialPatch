using UnityEngine;

public class RectTransformOffset : MonoBehaviour
{
	public bool forceAnchoredPositionX;

	public bool forceAnchoredPositionY;

	public bool forceOnStart;

	public bool forceOnUpdate;

	private void Force()
	{
		RectTransform rectTransform = base.transform as RectTransform;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.x = ((!forceAnchoredPositionX) ? anchoredPosition.x : 0f);
		anchoredPosition.y = ((!forceAnchoredPositionY) ? anchoredPosition.y : 0f);
		rectTransform.anchoredPosition = anchoredPosition;
	}

	private void Start()
	{
		if (forceOnStart)
		{
			Force();
		}
	}

	private void Update()
	{
		if (forceOnUpdate)
		{
			Force();
		}
	}
}
