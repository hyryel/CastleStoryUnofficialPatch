using UnityEngine;
using UnityEngine.UI;

public class VerticalLayoutGroupB : VerticalLayoutGroup
{
	public override void CalculateLayoutInputVertical()
	{
		MonoBehaviour.print("CalculateLayoutInputVertical\n" + IsActive());
		if (IsActive())
		{
			base.CalculateLayoutInputVertical();
		}
	}

	public override void CalculateLayoutInputHorizontal()
	{
		MonoBehaviour.print("CalculateLayoutInputHorizontal\n" + IsActive());
		if (IsActive())
		{
			base.CalculateLayoutInputHorizontal();
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		MonoBehaviour.print("OnRectTransformDimensionsChange\n" + IsActive());
		if (IsActive())
		{
			base.OnRectTransformDimensionsChange();
		}
	}

	protected override void OnCanvasHierarchyChanged()
	{
		MonoBehaviour.print("OnCanvasHierarchyChanged\n" + IsActive());
		if (IsActive())
		{
			base.OnCanvasHierarchyChanged();
		}
	}

	public override void SetLayoutVertical()
	{
		MonoBehaviour.print("SetLayoutVertical\n" + IsActive());
		if (IsActive())
		{
			base.SetLayoutVertical();
		}
	}

	public override void SetLayoutHorizontal()
	{
		MonoBehaviour.print("SetLayoutHorizontal\n" + IsActive());
		if (IsActive())
		{
			base.SetLayoutHorizontal();
		}
	}
}
