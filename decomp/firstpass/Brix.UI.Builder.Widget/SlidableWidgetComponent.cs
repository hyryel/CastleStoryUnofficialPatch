using Brix.UI.Builder.Menu;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class SlidableWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private LayoutConstraintB _layoutConstraint;

	[SerializeField]
	private ContentSizeFitter _fitter;

	private Closure _onEndShowing;

	private Closure _onEndHiding;

	public override bool Refresh(IWidget w)
	{
		if (!base.Refresh(w))
		{
			return false;
		}
		if (w.Is<IGetVisible>() && _animator != null && _animator.isInitialized)
		{
			_animator.SetBool("Visible", (w as IGetVisible).GetVisible());
		}
		_onEndShowing = ((!w.Is<IGetOnEndShowing>()) ? null : (w as IGetOnEndShowing).GetOnEndShowing());
		_onEndHiding = ((!w.Is<IGetOnEndHiding>()) ? null : (w as IGetOnEndHiding).GetOnEndHiding());
		_layoutConstraint.ResetCache();
		_layoutConstraint.Constrain();
		return true;
	}

	public void OnEndShowing()
	{
		if (_onEndShowing != null)
		{
			_onEndShowing.Call();
		}
	}

	public void OnEndHiding()
	{
		if (_onEndHiding != null)
		{
			_onEndHiding.Call();
		}
	}
}
