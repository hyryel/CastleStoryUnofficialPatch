using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class ImageLabelAnimatorWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected Text _label;

	[SerializeField]
	protected Text _count;

	[SerializeField]
	protected Image _image;

	[SerializeField]
	protected Image _imageCount;

	[SerializeField]
	protected Animator _colorAnimator;

	[SerializeField]
	protected Animator _iconAnimator;

	private Color _controlColor;

	private string _iconAnimatorTrigger = "ShowNone";

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (w.Is<IGetText>())
		{
			string text = (w as IGetText).GetText();
			if (_label.text != text)
			{
				_label.text = text;
			}
		}
		if (w.Is<IGetSprite>())
		{
			Sprite sprite = (w as IGetSprite).GetSprite();
			if (_image.sprite != sprite)
			{
				_image.sprite = sprite;
			}
			if (_imageCount.sprite != sprite)
			{
				_imageCount.sprite = sprite;
			}
			if (_image.sprite != null && w.Is<IGetSpriteSize>())
			{
				float spriteSize = (w as IGetSpriteSize).GetSpriteSize();
				Vector2 vector = new Vector2(spriteSize, spriteSize);
				if (_image.rectTransform.sizeDelta != vector)
				{
					_image.rectTransform.sizeDelta = vector;
				}
			}
		}
		if (w.Is<IGetColor>())
		{
			Color color = (w as IGetColor).GetColor();
			if (_label.color != color)
			{
				_label.color = color;
			}
			if (_image.color != color)
			{
				_image.color = color;
			}
		}
		if (w.Is<IGetCount>())
		{
			string count = (w as IGetCount).GetCount();
			if (_count.text != count)
			{
				_count.text = count;
			}
		}
		if (_colorAnimator != null && _colorAnimator.isInitialized && w.Is<IGetControlColor>())
		{
			Color controlColor = (w as IGetControlColor).GetControlColor();
			if (_controlColor != controlColor)
			{
				_controlColor = controlColor;
				if (controlColor == Color.yellow)
				{
					_colorAnimator.ResetTrigger("WhiteIcon");
					_colorAnimator.ResetTrigger("WhiteText");
					_colorAnimator.ResetTrigger("GreyIcon");
					_colorAnimator.ResetTrigger("GreyText");
					_colorAnimator.SetTrigger("YellowIcon");
					_colorAnimator.SetTrigger("YellowText");
				}
				else if (controlColor == Color.gray)
				{
					_colorAnimator.ResetTrigger("WhiteIcon");
					_colorAnimator.ResetTrigger("WhiteText");
					_colorAnimator.ResetTrigger("YellowIcon");
					_colorAnimator.ResetTrigger("YellowText");
					_colorAnimator.SetTrigger("GreyIcon");
					_colorAnimator.SetTrigger("GreyText");
				}
				else if (controlColor == Color.white)
				{
					_colorAnimator.ResetTrigger("GreyIcon");
					_colorAnimator.ResetTrigger("GreyText");
					_colorAnimator.ResetTrigger("WhiteText");
					_colorAnimator.ResetTrigger("YellowIcon");
					_colorAnimator.SetTrigger("WhiteIcon");
					_colorAnimator.SetTrigger("YellowText");
				}
			}
		}
		bool flag = _controlColor.a.Equals(0f);
		if (_iconAnimator != null && _iconAnimator.isInitialized)
		{
			string iconAnimatorTrigger = _iconAnimatorTrigger;
			iconAnimatorTrigger = (flag ? "ShowNone" : ((!w.Is<IGetIconAsCount>() || !(w as IGetIconAsCount).GetIconAsCount()) ? "ShowPrimary" : "ShowCounter"));
			if (_iconAnimatorTrigger != iconAnimatorTrigger)
			{
				_iconAnimatorTrigger = iconAnimatorTrigger;
				_iconAnimator.SetTrigger(iconAnimatorTrigger);
			}
		}
		return true;
	}
}
