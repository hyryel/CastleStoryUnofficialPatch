using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class ModalPanelComponent : BaseSocketLayoutPanelComponent
{
	public RectTransform _headerBin;

	public RectTransform _contentBin;

	public RectTransform _footerBin;

	public LayoutElement _headerLayoutElement;

	public VerticalLayoutGroup _contentLayoutGroup;

	public LayoutElement _contentLayoutElement;

	public ContentSizeFitterPlus _contentFitterPlus;

	public ScrollRect _scrollRect;

	public Image _headerLineImage;

	public Image _footerLineImage;
}
