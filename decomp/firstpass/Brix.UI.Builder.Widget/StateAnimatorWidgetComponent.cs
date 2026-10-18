using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Widget;

public class StateAnimatorWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected Animator _stateAnimator;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (_stateAnimator != null && _stateAnimator.isInitialized)
		{
			string text = "CanHover";
			bool flag = !w.Is<IGetCanHover>() || (w as IGetCanHover).GetCanHover();
			if (_stateAnimator.GetBool(text) != flag)
			{
				_stateAnimator.SetBool(text, flag);
			}
			string text2 = "CanPress";
			bool flag2 = !w.Is<IGetCanPress>() || (w as IGetCanPress).GetCanPress();
			if (_stateAnimator.GetBool(text2) != flag2)
			{
				_stateAnimator.SetBool(text2, flag2);
			}
			string text3 = "Discreet";
			bool flag3 = !w.Is<IGetDiscreet>() || (w as IGetDiscreet).GetDiscreet();
			if (_stateAnimator.GetBool(text3) != flag3)
			{
				_stateAnimator.SetBool(text3, flag3);
			}
		}
		return true;
	}
}
