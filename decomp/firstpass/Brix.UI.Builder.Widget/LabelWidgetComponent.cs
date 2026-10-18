using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class LabelWidgetComponent : BaseWidgetComponent
{
	public enum FontEnum
	{
		KorolevLight,
		KorolevMedium,
		KorolevBold,
		KorolevHeavy,
		KorolevThin,
		Plakkaat,
		ProximaNovaBold,
		ProximaNovaExtrabold,
		ProximaNovaSemibold,
		ProximaNovaLight,
		ProximaNovaRegular,
		AgencyB,
		AgencyR
	}

	[SerializeField]
	protected Text _label;

	[SerializeField]
	protected TextLayoutConstraint _textLayoutConstraint;

	public Font KorolevLight;

	public Font KorolevMedium;

	public Font KorolevBold;

	public Font KorolevHeavy;

	public Font KorolevThin;

	public Font Plakkaat;

	public Font ProximaNovaBold;

	public Font ProximaNovaExtrabold;

	public Font ProximaNovaSemibold;

	public Font ProximaNovaLight;

	public Font ProximaNovaRegular;

	public Font AgencyB;

	public Font AgencyR;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (w.Is<IGetEnabled>())
		{
			bool flag = (w as IGetEnabled).GetEnabled();
			if (_label.enabled != flag)
			{
				_label.enabled = flag;
			}
		}
		if (w.Is<IGetRaycasts>())
		{
			bool raycasts = (w as IGetRaycasts).GetRaycasts();
			if (_label.raycastTarget != raycasts)
			{
				_label.raycastTarget = raycasts;
			}
		}
		if (w.Is<IGetText>())
		{
			string text = (w as IGetText).GetText();
			if (_label.text != text)
			{
				_label.text = text;
			}
		}
		if (w.Is<IGetColor>())
		{
			Color color = (w as IGetColor).GetColor();
			if (_label.color != color)
			{
				_label.color = color;
			}
		}
		if (w.Is<IGetFont>())
		{
			Font font = GetFont((w as IGetFont).GetFont());
			if (_label.font != font)
			{
				_label.font = font;
			}
		}
		if (w.Is<IGetFontSize>())
		{
			int fontSize = (w as IGetFontSize).GetFontSize();
			if (_label.fontSize != fontSize)
			{
				_label.fontSize = fontSize;
			}
		}
		if (w.Is<IGetAlignment>())
		{
			TextAnchor alignment = (w as IGetAlignment).GetAlignment();
			if (_label.alignment != alignment)
			{
				_label.alignment = alignment;
			}
		}
		if (w.Is<IGetAutoPreferredHeight>() && (w as IGetAutoPreferredHeight).GetAutoPreferredHeight())
		{
			_textLayoutConstraint.ConstrainPreferredHeight();
		}
		if (w.Is<IGetLineSpacing>())
		{
			float lineSpacing = (w as IGetLineSpacing).GetLineSpacing();
			if (_label.lineSpacing != lineSpacing)
			{
				_label.lineSpacing = lineSpacing;
			}
		}
		return true;
	}

	private Font GetFont(FontEnum fontEnum)
	{
		return fontEnum switch
		{
			FontEnum.KorolevLight => KorolevLight, 
			FontEnum.KorolevMedium => KorolevMedium, 
			FontEnum.KorolevBold => KorolevBold, 
			FontEnum.KorolevHeavy => KorolevHeavy, 
			FontEnum.KorolevThin => KorolevThin, 
			FontEnum.Plakkaat => Plakkaat, 
			FontEnum.ProximaNovaBold => ProximaNovaBold, 
			FontEnum.ProximaNovaExtrabold => ProximaNovaExtrabold, 
			FontEnum.ProximaNovaSemibold => ProximaNovaSemibold, 
			FontEnum.ProximaNovaLight => ProximaNovaLight, 
			FontEnum.ProximaNovaRegular => ProximaNovaRegular, 
			FontEnum.AgencyB => AgencyB, 
			FontEnum.AgencyR => AgencyR, 
			_ => KorolevLight, 
		};
	}
}
