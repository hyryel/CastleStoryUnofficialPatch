using UnityEngine;

namespace Brix.Input.WorldEditor;

public class BrushSettings : ISize
{
	private float sizeRatio = -1f;

	protected virtual float defaultSizeRatio => 0.4f;

	protected virtual int maxSize => 20;

	protected virtual int minSize => 1;

	public virtual float Size
	{
		get
		{
			if (sizeRatio < 0f)
			{
				sizeRatio = defaultSizeRatio;
			}
			return Mathf.Clamp01(sizeRatio);
		}
		set
		{
			sizeRatio = value;
			Picking.Instance.BrushHighlight.Size = ConvertedSize;
			Picking.Instance.WorldEditorGrid.Size = ConvertedSize;
		}
	}

	public virtual int ConvertedSize => Mathf.RoundToInt((float)(maxSize - minSize) * Size) + minSize;

	public virtual string SliderString => ConvertedSize.ToString();
}
