using UnityEngine.UI;

public class TogglableVerticalLayoutGroup : VerticalLayoutGroup
{
	private ContentSizeFitter SizeFitter;

	protected override void Start()
	{
		base.Start();
		SizeFitter = GetComponent<ContentSizeFitter>();
	}

	public void Refresh()
	{
		base.enabled = true;
	}

	public override void CalculateLayoutInputVertical()
	{
		if (IsActive())
		{
			base.CalculateLayoutInputVertical();
		}
	}

	public override void CalculateLayoutInputHorizontal()
	{
		if (IsActive())
		{
			base.CalculateLayoutInputHorizontal();
		}
	}

	public override void SetLayoutVertical()
	{
		if (IsActive())
		{
			base.SetLayoutVertical();
			base.enabled = false;
		}
	}

	public override void SetLayoutHorizontal()
	{
		if (IsActive())
		{
			base.SetLayoutHorizontal();
		}
	}

	protected override void OnEnable()
	{
		if (SizeFitter != null)
		{
			SizeFitter.enabled = true;
		}
	}

	protected override void OnDisable()
	{
		if (SizeFitter != null)
		{
			SizeFitter.enabled = false;
		}
	}
}
