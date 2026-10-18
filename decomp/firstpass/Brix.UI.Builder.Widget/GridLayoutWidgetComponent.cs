using Brix.Game.Utils;
using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class GridLayoutWidgetComponent : BaseLayoutGroupWidgetComponent
{
	[SerializeField]
	protected GridLayoutGroup _layoutGroup;

	public override bool Refresh(IWidget w)
	{
		if (!base.Refresh(w))
		{
			return false;
		}
		if (w.Is<IGetCellSize>())
		{
			Vector2 cellSize = (w as IGetCellSize).GetCellSize();
			if (_layoutGroup.cellSize != cellSize)
			{
				_layoutGroup.cellSize = cellSize;
			}
		}
		if (w.Is<IGetPadding>())
		{
			RectOffset padding = (w as IGetPadding).GetPadding();
			if (_layoutGroup.padding != padding)
			{
				_layoutGroup.padding = padding;
			}
		}
		if (w.Is<IGetSpacingXy>())
		{
			Vector2 spacingXy = (w as IGetSpacingXy).GetSpacingXy();
			if (_layoutGroup.spacing != spacingXy)
			{
				_layoutGroup.spacing = spacingXy;
			}
		}
		if (w.Is<IGetAlignment>())
		{
			TextAnchor alignment = (w as IGetAlignment).GetAlignment();
			if (_layoutGroup.childAlignment != alignment)
			{
				_layoutGroup.childAlignment = alignment;
			}
		}
		Mute();
		return true;
	}

	private void OnEnable()
	{
		Mute();
	}

	private void Mute()
	{
		LayoutPanelManager.Add(_layoutGroup);
	}
}
