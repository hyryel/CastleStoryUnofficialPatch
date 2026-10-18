using Brix.Game.Utils;
using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Widget;

public class FrameAnimatorWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected Animator _frameAnimator;

	private readonly AnimatorIntSetter _flagSetter = new AnimatorIntSetter("Flag", 0);

	private int _v = -1;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (_frameAnimator != null && w.Is<IGetHighlight>())
		{
			string highlight = (w as IGetHighlight).GetHighlight();
			int num = 0;
			switch (highlight)
			{
			case "None":
				num = 0;
				break;
			case "Bottom":
				num = 1;
				break;
			case "Top":
				num = 2;
				break;
			case "Left":
				num = 4;
				break;
			case "Right":
				num = 8;
				break;
			case "All":
				num = 15;
				break;
			}
			string text = "Flag";
			if (_frameAnimator.isActiveAndEnabled && _frameAnimator.isInitialized)
			{
				AnimatorManager.Remove(_frameAnimator);
				if (num != _frameAnimator.GetInteger(text))
				{
					_frameAnimator.SetInteger(text, num);
				}
			}
			else
			{
				_v = num;
			}
		}
		return true;
	}

	private void OnEnable()
	{
		if (_v != -1)
		{
			AnimatorManager.Add(_frameAnimator, _flagSetter.SetValue(_v));
		}
	}

	private void OnDisable()
	{
		AnimatorManager.Remove(_frameAnimator);
	}
}
