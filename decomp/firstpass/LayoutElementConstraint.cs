using UnityEngine;
using UnityEngine.UI;

public class LayoutElementConstraint : MonoBehaviour
{
	public enum Measurement
	{
		SumOfChildren,
		BiggestChild,
		ParentOnly
	}

	public float offsetWidth;

	public float offsetHeight;

	public float offsetElementWidth;

	public float offsetElementHeight;

	public float maxWidth = -1f;

	public float maxHeight = -1f;

	public bool m_IsConstrainOnEnable;

	public bool m_IsConstrainOnStart;

	public bool m_IsConstrainOnUpdate;

	public bool m_IsConstrainOnLateUpdate;

	public bool m_IsConstrainOnRectChange;

	public bool m_IsOverrideMinWidth;

	public bool m_IsOverrideMinHeight;

	public bool m_IsOverridePreferredWidth;

	public bool m_IsOverridePreferredHeight;

	public Measurement m_Measurement = Measurement.ParentOnly;

	public RectTransform m_MasterRectTransform;

	public LayoutElement m_SlaveLayoutElement;

	[Disabled]
	public float outWidth;

	[Disabled]
	public float outHeight;

	private float cWidth = -99f;

	private float cHeight = -99f;

	public void Constrain()
	{
		if (m_MasterRectTransform == null)
		{
			return;
		}
		bool activeSelf = m_MasterRectTransform.gameObject.activeSelf;
		float num = 0f;
		float num2 = 0f;
		if (activeSelf)
		{
			num = offsetWidth;
			num2 = offsetHeight;
		}
		if (m_Measurement == Measurement.ParentOnly)
		{
			if (activeSelf)
			{
				float x = m_MasterRectTransform.sizeDelta.x;
				float y = m_MasterRectTransform.sizeDelta.y;
				num += x;
				num2 += y;
			}
		}
		else
		{
			foreach (Transform item in m_MasterRectTransform)
			{
				RectTransform rectTransform = item as RectTransform;
				if (!(rectTransform == null) && rectTransform.gameObject.activeSelf)
				{
					float x2 = rectTransform.sizeDelta.x;
					float y2 = rectTransform.sizeDelta.y;
					switch (m_Measurement)
					{
					case Measurement.BiggestChild:
						num = ((!(x2 > num)) ? num : x2);
						num2 = ((!(y2 > num2)) ? num2 : y2);
						break;
					case Measurement.SumOfChildren:
						x2 += offsetElementWidth;
						y2 += offsetElementHeight;
						num += x2;
						num2 += y2;
						break;
					}
				}
			}
			if (m_Measurement == Measurement.SumOfChildren)
			{
				num -= offsetElementWidth;
				num2 -= offsetElementHeight;
			}
		}
		num = ((maxWidth == -1f) ? num : ((!(num < maxWidth)) ? maxWidth : num));
		num2 = ((maxHeight == -1f) ? num2 : ((!(num2 < maxHeight)) ? maxHeight : num2));
		outWidth = num;
		outHeight = num2;
		if (num != cWidth)
		{
			if (m_IsOverrideMinWidth)
			{
				m_SlaveLayoutElement.minWidth = num;
			}
			if (m_IsOverridePreferredWidth)
			{
				m_SlaveLayoutElement.preferredWidth = num;
			}
			cWidth = num;
		}
		if (num2 != cHeight)
		{
			if (m_IsOverrideMinHeight)
			{
				m_SlaveLayoutElement.minHeight = num2;
			}
			if (m_IsOverridePreferredHeight)
			{
				m_SlaveLayoutElement.preferredHeight = num2;
			}
			cHeight = num2;
		}
	}

	private void OnEnable()
	{
		if (m_IsConstrainOnEnable)
		{
			Constrain();
		}
	}

	private void Start()
	{
		if (m_IsConstrainOnStart)
		{
			Constrain();
		}
	}

	private void Update()
	{
		if (m_IsConstrainOnUpdate)
		{
			Constrain();
		}
	}

	private void LateUpdate()
	{
		if (m_IsConstrainOnLateUpdate)
		{
			Constrain();
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		if (m_IsConstrainOnRectChange)
		{
			Constrain();
		}
	}
}
