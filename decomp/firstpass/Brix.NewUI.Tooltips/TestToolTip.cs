using UnityEngine;

namespace Brix.NewUI.Tooltips;

public class TestToolTip : MonoBehaviour
{
	public Tooltip tooltip;

	public TooltipPositioner positioner;

	public RectTransform target;

	public Vector3 offset3D;

	public Vector2 offset2D;

	public Vector2 mousePosition;

	public string InputStr;

	public Sprite InputImg;

	public Tooltip.Width width;

	public bool arrowFollowMouse;

	public bool show;

	public bool hide;

	public bool setMainImage;

	public bool noMainImage;

	public bool setTitle;

	public bool noTitle;

	public bool setSmallImage;

	public bool noSmallImage;

	public bool setText;

	public bool noText;

	public bool setOffset2D;

	public bool setOffset3D;

	public bool setWidth;

	private void Start()
	{
	}

	private void Update()
	{
		if (setOffset3D)
		{
			tooltip.SetPositionOffset3D(offset3D);
			setOffset3D = false;
		}
		if (setWidth)
		{
			tooltip.SetWidth(width);
			setWidth = false;
		}
		if (setOffset2D)
		{
			tooltip.SetPositionOffset2D(offset2D);
			setOffset2D = false;
		}
		if (show)
		{
			tooltip.Show();
			show = false;
		}
		if (hide)
		{
			tooltip.Hide();
			hide = false;
		}
		if (setMainImage)
		{
			tooltip.SetTitleImage(InputImg);
			setMainImage = false;
		}
		if (noMainImage)
		{
			tooltip.NoImage();
			noMainImage = false;
		}
		if (setTitle)
		{
			tooltip.SetTitle(InputStr);
			setTitle = false;
		}
		if (noTitle)
		{
			tooltip.NoTitle();
			noTitle = false;
		}
		if (setText)
		{
			tooltip.SetMainText(InputStr);
			setText = false;
		}
		if (noText)
		{
			tooltip.NoMainText();
			noText = false;
		}
		if (setSmallImage)
		{
			tooltip.SetSubImage(InputImg);
			setSmallImage = false;
		}
		if (noSmallImage)
		{
			tooltip.NoSubImage();
			noSmallImage = false;
		}
		if (target != null)
		{
			tooltip.SetTarget2D(target);
			tooltip.SetFollowDynamic(value: true);
		}
	}
}
