using UnityEngine.UI;

public class TogglableHorizontalLayoutGroup : HorizontalLayoutGroup
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

	protected override void OnRectTransformDimensionsChange()
	{
		if (IsActive())
		{
			base.OnRectTransformDimensionsChange();
		}
	}

	protected override void OnCanvasHierarchyChanged()
	{
		if (IsActive())
		{
			base.OnCanvasHierarchyChanged();
		}
	}

	public override void SetLayoutVertical()
	{
		if (IsActive())
		{
			base.SetLayoutVertical();
		}
	}

	public override void SetLayoutHorizontal()
	{
		if (IsActive())
		{
			base.SetLayoutHorizontal();
			base.enabled = false;
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
