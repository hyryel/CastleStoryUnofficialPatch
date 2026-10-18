using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.Compass;

public abstract class Compass : MonoBehaviour
{
	public Image CirclerUnder;

	public Image CircleOver;

	[SerializeField]
	private Image m_IconImage;

	private Sprite m_Icon;

	[SerializeField]
	private float m_TimeOfDiscard = -1f;

	[SerializeField]
	private bool m_DiscardOnClick;

	[SerializeField]
	private Text m_CountText;

	[SerializeField]
	private GameObject m_CountGo;

	private int m_Count;

	[SerializeField]
	private RectTransform m_ArrowRectTransform;

	[SerializeField]
	private ScreenEdgeConstraint m_ScreenEdgeConstraint;

	[SerializeField]
	private RectTransform m_ContentRectTransform;

	private RectTransform m_RectTransform;

	public Sprite icon
	{
		get
		{
			return m_Icon;
		}
		set
		{
			m_Icon = value;
			m_IconImage.sprite = value;
		}
	}

	public bool discardOnClick
	{
		get
		{
			return m_DiscardOnClick;
		}
		set
		{
			m_DiscardOnClick = value;
		}
	}

	public int count
	{
		get
		{
			return m_Count;
		}
		set
		{
			m_Count = value;
			m_CountText.text = value.GetSharedString();
			if (value > 1)
			{
				m_CountGo.SetActive(value: true);
			}
			else if (value == 1)
			{
				m_CountGo.SetActive(value: false);
			}
		}
	}

	public RectTransform arrowRectTransform
	{
		get
		{
			return m_ArrowRectTransform;
		}
		set
		{
			m_ArrowRectTransform = value;
		}
	}

	public ScreenEdgeConstraint screenEdgeConstraint
	{
		get
		{
			return m_ScreenEdgeConstraint;
		}
		set
		{
			m_ScreenEdgeConstraint = value;
		}
	}

	public RectTransform contentRectTransform
	{
		get
		{
			return m_ContentRectTransform;
		}
		set
		{
			m_ContentRectTransform = value;
		}
	}

	public RectTransform rectTransform => m_RectTransform = ((!(m_RectTransform != null)) ? (base.transform as RectTransform) : m_RectTransform);

	public void DiscardOnTime(float timeBeforeDiscard)
	{
		m_TimeOfDiscard = Time.time + timeBeforeDiscard;
	}

	public void TryDiscardOnClick()
	{
		if (m_DiscardOnClick)
		{
			Discard();
		}
	}

	private void TryDiscardOnTime()
	{
		if (m_TimeOfDiscard != -1f && !(Time.time < m_TimeOfDiscard))
		{
			Discard();
		}
	}

	public void Discard()
	{
		m_TimeOfDiscard = -1f;
		Discard(forceKill: false);
	}

	public void Discard(bool forceKill)
	{
		ContentHide();
		if (forceKill)
		{
			OnKill();
		}
	}

	protected virtual void Create(Sprite icon)
	{
		this.icon = icon;
	}

	protected virtual void OnCreate()
	{
		count = 1;
		ResetPosition();
	}

	protected virtual void OnBeforeCreate()
	{
	}

	protected virtual void OnAfterCreate()
	{
	}

	protected virtual void OnKill()
	{
		Object.DestroyImmediate(base.gameObject);
	}

	public void ResetPosition()
	{
		rectTransform.anchoredPosition3D = Vector3.zero;
		rectTransform.anchoredPosition = Vector3.zero;
		rectTransform.localPosition = Vector3.zero;
	}

	protected void ContentShow()
	{
		ContentToggle(flag: true);
	}

	protected void ContentHide()
	{
		ContentToggle(flag: false);
	}

	protected virtual void ContentToggle(bool flag)
	{
		contentRectTransform.gameObject.SetActive(flag);
	}

	protected virtual void Update()
	{
		TryDiscardOnTime();
	}
}
