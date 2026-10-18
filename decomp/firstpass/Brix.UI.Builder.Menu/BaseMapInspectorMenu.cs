using System;
using Brix.Engine;
using Brix.UI.Builder.Panel;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public abstract class BaseMapInspectorMenu : BaseAssetMenu
{
	protected virtual bool IsCreditIslandMenu => GameContext == MenuGameContext.CreditIsland;

	protected virtual bool IsWorldEditorMenu => GameContext == MenuGameContext.WorldEditor;

	protected virtual bool IsExistingAsset => TypeContext == MenuTypeContext.Existing;

	protected virtual bool AsModal => false;

	protected virtual bool IsReskin => false;

	protected virtual bool UseGamemodeFilter => false;

	protected virtual bool IsLobby => false;

	protected virtual bool HasHeader => true;

	protected virtual float HeaderHeight => 20f;

	protected virtual Action<VerticalLayoutPanelComponent> RootLayout_onReset => null;

	private IPanel ModalLayout
	{
		get
		{
			Action<ModalPanelComponent> onReset = delegate(ModalPanelComponent panel)
			{
				panel._headerLineImage.gameObject.SetActive(value: false);
				panel._footerLineImage.gameObject.SetActive(value: false);
				float headerHeight = HeaderHeight;
				headerHeight += ((!(SubtitleLabel == string.Empty)) ? SubtitleHeight : 0f);
				headerHeight += ((!(TitleLabel == string.Empty)) ? SubtitleHeight : 0f);
				panel._headerLayoutElement.minHeight = headerHeight;
				panel._headerLayoutElement.preferredHeight = headerHeight;
			};
			return ModalPanel.New(onReset).AddPanelAtSocket(ModalPanel.Socket.Header, Header).AddPanelAtSocket(ModalPanel.Socket.Content, Content)
				.AddPanelsAtSocketNoLeak(ModalPanel.Socket.Footer, Footer);
		}
	}

	private IPanel VerticalLayout
	{
		get
		{
			ILayoutPanel layoutPanel = VerticalLayoutPanel.New(RootLayout_onReset);
			if (IsReskin)
			{
				layoutPanel.AddPanel(Content);
				if (HasHeader)
				{
					layoutPanel.AddPanel(Header);
				}
			}
			else
			{
				if (HasHeader)
				{
					layoutPanel.AddPanel(Header);
				}
				layoutPanel.AddPanel(Content);
			}
			layoutPanel.AddPanels(Footer);
			return layoutPanel;
		}
	}

	private IPanel Header
	{
		get
		{
			Action<VerticalLayoutPanelComponent> onReset = delegate(VerticalLayoutPanelComponent panel)
			{
				panel._fitter.enabled = false;
				panel._layoutElement.flexibleWidth = 1f;
				panel._layoutGroup.spacing = 0f;
				panel._layoutGroup.padding = new RectOffset(0, 0, 12, 8);
				if (IsReskin)
				{
					panel._layoutGroup.padding = new RectOffset(8, 8, 12, 24);
					panel._layoutGroup.spacing = 4f;
					panel._layoutElement.minHeight = 0f;
					panel._layoutElement.preferredHeight = -1f;
					panel._layoutElement.flexibleHeight = -1f;
				}
				panel._layoutGroup.childAlignment = TextAnchor.MiddleCenter;
				panel._image.enabled = false;
			};
			return VerticalLayoutPanel.New(onReset).AddPanel(Title).AddPanel(Subtitle);
		}
	}

	private IPanel Content => Thumbnail;

	protected abstract string TitleLabel { get; }

	protected virtual bool TitleHiddenIfEmpty => true;

	protected virtual int TitleFontSize => 20;

	protected virtual float TitleHeight => 28f;

	private IPanel Title
	{
		get
		{
			Action<TitleTextPanelComponent> action = delegate(TitleTextPanelComponent panel)
			{
				string text = TitleLabel;
				bool flag = text != string.Empty;
				if (IsReskin)
				{
					text = text.ToUpper();
				}
				panel._label.text = text;
				panel._label.fontSize = TitleFontSize;
				panel._layoutElement.minHeight = TitleHeight;
				panel._layoutElement.preferredHeight = TitleHeight;
				panel._layoutElement.flexibleWidth = 1f;
				if (IsReskin)
				{
					panel._fitter.enabled = true;
					panel._fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
					panel._layoutElement.minHeight = 0f;
					panel._layoutElement.preferredHeight = -1f;
					panel._layoutElement.flexibleHeight = -1f;
					panel._label.color = Colors.Black;
				}
				panel.gameObject.SetActive(flag && TitleHiddenIfEmpty);
			};
			return TitleTextPanelComponent.Build(action, action).As(IsReskin ? TitleTextPanelComponent.BasePanel.Variant.StandardReskin : TitleTextPanelComponent.BasePanel.Variant.Standard);
		}
	}

	protected virtual string SubtitleLabel => string.Empty;

	protected virtual bool SubtitleHiddenIfEmpty => true;

	protected virtual int SubtitleFontSize => 14;

	protected virtual float SubtitleHeight => 28f;

	private IPanel Subtitle
	{
		get
		{
			Action<TitleTextPanelComponent> action = delegate(TitleTextPanelComponent panel)
			{
				string subtitleLabel = SubtitleLabel;
				bool flag = subtitleLabel != string.Empty;
				panel._label.text = subtitleLabel;
				panel._label.fontSize = SubtitleFontSize;
				panel._layoutElement.minHeight = SubtitleHeight;
				panel._layoutElement.preferredHeight = SubtitleHeight;
				panel._layoutElement.flexibleWidth = 1f;
				if (IsReskin)
				{
					panel._fitter.enabled = true;
					panel._fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
					panel._layoutElement.minHeight = 0f;
					panel._layoutElement.preferredHeight = -1f;
					panel._layoutElement.flexibleHeight = -1f;
					panel._label.color = Colors.Black;
				}
				panel.gameObject.SetActive(flag && SubtitleHiddenIfEmpty);
			};
			return TitleTextPanelComponent.Build(action, action);
		}
	}

	protected abstract Texture2D ThumbnailTex { get; }

	protected virtual bool ThumbnailIsHeightmap => false;

	protected virtual bool ThumbnailShowsBg => true;

	protected virtual bool ThumbnailShowsLoadingIfNull => false;

	protected virtual bool ThumbnailStretches => false;

	private IPanel Thumbnail
	{
		get
		{
			Action<TexturePanelComponent> action = delegate(TexturePanelComponent panel)
			{
				Texture2D thumbnailTex = ThumbnailTex;
				bool flag = ThumbnailTex != null;
				panel._rawImage.gameObject.SetActive(flag);
				panel._missingImage.gameObject.SetActive(!flag);
				panel._loadingImageGo.gameObject.SetActive(value: false);
				float x = 320f;
				float num = 180f;
				if (!flag)
				{
					if (ThumbnailShowsLoadingIfNull)
					{
						panel._layoutElement.minWidth = 64f;
						panel._layoutElement.preferredWidth = 64f;
						panel._layoutElement.minHeight = 64f;
						panel._layoutElement.preferredHeight = 64f;
						panel._layoutElement.flexibleWidth = -1f;
						panel._layoutElement.flexibleHeight = -1f;
						panel._missingImage.gameObject.SetActive(value: false);
						panel._loadingImageGo.gameObject.SetActive(value: true);
					}
					float num2 = num - 108f;
					panel._layoutElement.minWidth = -1f;
					panel._layoutElement.preferredWidth = -1f;
					panel._layoutElement.minHeight = num2;
					panel._layoutElement.preferredHeight = num2;
					panel._layoutElement.flexibleWidth = 1f;
					panel._layoutElement.flexibleHeight = -1f;
					panel._bgImage.rectTransform.sizeDelta = new Vector2(x, num);
				}
				else
				{
					x = ThumbnailTex.width;
					num = ThumbnailTex.height;
					float num3 = x / num;
					float num4 = 1f / num3;
					x = ((!(x > 320f)) ? x : 320f);
					num = x * num4;
					num = ((!(num > 260f)) ? num : 260f);
					x = num * num3;
					if (ThumbnailStretches)
					{
						float num5 = num;
						num5 = ((!(num5 > 308f)) ? num5 : 308f);
						if (IsReskin)
						{
							num5 = num - 108f;
						}
						panel._layoutElement.minWidth = -1f;
						panel._layoutElement.preferredWidth = -1f;
						panel._layoutElement.minHeight = num5;
						panel._layoutElement.preferredHeight = num5;
						panel._layoutElement.flexibleWidth = 1f;
						panel._layoutElement.flexibleHeight = -1f;
						panel._rawImageFitter.aspectRatio = num3;
					}
					else
					{
						float num6 = 146f;
						if (IsReskin)
						{
							num6 = num - 108f;
						}
						panel._layoutElement.minWidth = 258f;
						panel._layoutElement.preferredWidth = 258f;
						panel._layoutElement.minHeight = num6;
						panel._layoutElement.preferredHeight = num6;
						panel._layoutElement.flexibleWidth = -1f;
						panel._layoutElement.flexibleHeight = -1f;
					}
					panel._rawImage.texture = thumbnailTex;
					panel._bgImage.enabled = ThumbnailShowsBg;
					if (panel._bgImage2 != null)
					{
						panel._bgImage2.enabled = ThumbnailIsHeightmap;
						panel._bgImage2.color = Colors.White;
					}
					panel._bgImage.rectTransform.sizeDelta = new Vector2(x, num);
					if (ThumbnailIsHeightmap)
					{
						Texture2D texture2D = panel._rawImage.texture as Texture2D;
						if (texture2D != null)
						{
							panel._rawImage.uvRect = Minimap.GetScaleAndOffsetFromBounds(Minimap.GetTerrainBounds(texture2D), 32f);
						}
					}
				}
			};
			TexturePanelComponent.BasePanel basePanel = TexturePanelComponent.Build(action, action);
			if (IsReskin)
			{
				basePanel.As(TexturePanelComponent.BasePanel.Variant.MapTextureReskin);
			}
			return basePanel;
		}
	}

	protected abstract PanelList Footer { get; }

	protected BaseMapInspectorMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanel((!AsModal) ? VerticalLayout : ModalLayout);
	}
}
