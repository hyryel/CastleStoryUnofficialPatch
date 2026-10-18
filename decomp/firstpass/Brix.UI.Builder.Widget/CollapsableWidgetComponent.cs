using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class CollapsableWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected GameObject _go;

	[SerializeField]
	protected Image _image;

	[SerializeField]
	protected Button _button;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (w.Is<IGetCollapsable>())
		{
			bool collapsable = (w as IGetCollapsable).GetCollapsable();
			_go.SetActive(collapsable);
			if (collapsable && w.Is<IGetCollapsed>())
			{
				bool collapsed = (w as IGetCollapsed).GetCollapsed();
				Sprite sprite = ((!w.Is<IGetCollapsedSprite>()) ? null : (w as IGetCollapsedSprite).GetCollapsedSprite());
				Sprite sprite2 = ((!w.Is<IGetExpandedSprite>()) ? null : (w as IGetExpandedSprite).GetExpandedSprite());
				_image.sprite = ((!collapsed) ? sprite2 : sprite);
			}
		}
		_button.onClick.RemoveAllListeners();
		_button.onClick.AddListener(delegate
		{
			if (w.Is<IOnToggleCollapse>())
			{
				(w as IOnToggleCollapse).OnToggleCollapse();
			}
		});
		return true;
	}
}
