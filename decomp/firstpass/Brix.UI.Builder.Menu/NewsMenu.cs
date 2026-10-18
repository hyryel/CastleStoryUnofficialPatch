using System;
using System.Diagnostics;
using Brix.Assets;
using Brix.Components;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Utils;
using Brix.Network;
using Brix.UI.Builder.Panel;
using Brix.UI.Icons;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public sealed class NewsMenu : BaseMenu
{
	private static class VerticalLayout_Main
	{
		public static ILayoutPanel Panel => VerticalLayoutPanel.New(OnReset);

		private static Action<VerticalLayoutPanelComponent> OnReset => delegate(VerticalLayoutPanelComponent panel)
		{
			panel._fitter.enabled = false;
			panel._image.enabled = false;
		};
	}

	private static class VerticalLayout_Content
	{
		public static ILayoutPanel Panel => VerticalLayoutPanel.New(OnReset);

		private static Action<VerticalLayoutPanelComponent> OnReset => delegate(VerticalLayoutPanelComponent panel)
		{
			panel._fitter.enabled = false;
			panel._image.enabled = false;
			panel._layoutGroup.padding = new RectOffset(16, 16, 0, 0);
		};
	}

	private static class Text_SectionTitle
	{
		public static IPanel Panel => TitleTextPanelComponent.Build(OnReset);

		private static Action<TitleTextPanelComponent> OnReset => delegate(TitleTextPanelComponent panel)
		{
			panel._layoutElement.minWidth = -1f;
			panel._layoutElement.minHeight = 0f;
			panel._layoutElement.preferredWidth = -1f;
			panel._layoutElement.preferredHeight = -1f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
			panel._fitter.enabled = true;
			panel._fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			panel._label.text = I2Helper.TryGet("##mainmenu_news").ToUpper();
			panel._label.color = Colors.Black;
			panel._label.fontSize = 29;
			panel._label.font = BrixSingleton<LayoutPanelManager>.Instance.ProximaNovaExtrabold;
		};
	}

	private static class Image_Thumbnail
	{
		public static IPanel Panel => TexturePanelComponent.Build(OnReset).As(TexturePanelComponent.BasePanel.Variant.MapTextureReskin);

		private static Action<TexturePanelComponent> OnReset => delegate(TexturePanelComponent panel)
		{
			panel._layoutElement.minWidth = 320f;
			panel._layoutElement.minHeight = 180f;
			panel._layoutElement.preferredWidth = 320f;
			panel._layoutElement.preferredHeight = 180f;
			panel._layoutElement.flexibleWidth = -1f;
			panel._layoutElement.flexibleHeight = -1f;
			panel._missingImage.gameObject.SetActive(value: false);
			panel._loadingImageGo.gameObject.SetActive(value: false);
			panel._rawImage.gameObject.SetActive(value: true);
			panel._rawImage.texture = BrixSingleton<LayoutPanelManager>.Instance.HalloweenTexture;
			panel._bgImage.rectTransform.sizeDelta = new Vector2(328f, 188f);
			panel._bgImage.rectTransform.anchorMin = new Vector2(1f, 0.5f);
			panel._bgImage.rectTransform.anchorMax = new Vector2(1f, 0.5f);
			panel._bgImage.rectTransform.anchoredPosition = new Vector2(-98f, 0f);
			panel._bgImage.rectTransform.pivot = new Vector2(1f, 0.5f);
			panel._bgImage2.enabled = false;
			panel._rawImageFitter.enabled = false;
		};
	}

	private static class Spacer
	{
		public static IPanel Panel => SpacerPanelComponent.Build(OnReset);

		private static Action<SpacerPanelComponent> OnReset => delegate(SpacerPanelComponent panel)
		{
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = 1f;
		};
	}

	private static class Text_Title
	{
		public static IPanel Panel => TitleTextPanelComponent.Build(OnReset);

		private static Action<TitleTextPanelComponent> OnReset => delegate(TitleTextPanelComponent panel)
		{
			panel._layoutElement.minWidth = -1f;
			panel._layoutElement.minHeight = 0f;
			panel._layoutElement.preferredWidth = -1f;
			panel._layoutElement.preferredHeight = -1f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
			panel._fitter.enabled = true;
			panel._fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			panel._label.text = I2Helper.TryGet("##news_mastodonte").ToUpper();
			panel._label.color = Colors.Black;
			panel._label.fontSize = 20;
			panel._label.font = BrixSingleton<LayoutPanelManager>.Instance.ProximaNovaExtrabold;
		};
	}

	private static class Text_DateTime
	{
		public static IPanel Panel => TitleTextPanelComponent.Build(OnReset);

		private static Action<TitleTextPanelComponent> OnReset => delegate(TitleTextPanelComponent panel)
		{
			panel._layoutElement.minWidth = -1f;
			panel._layoutElement.minHeight = 0f;
			panel._layoutElement.preferredWidth = -1f;
			panel._layoutElement.preferredHeight = -1f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
			panel._fitter.enabled = true;
			panel._fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			panel._label.text = I2Helper.TryGet("##news_mastodonte_datetime").Replace("#datetime", I2Helper.DateTimeUtility.GetLongDateString(new DateTime(2018, 12, 13))).ToUpper();
			panel._label.color = Colors.Black;
			panel._label.fontSize = 12;
			panel._label.font = BrixSingleton<LayoutPanelManager>.Instance.ProximaNovaLight;
		};
	}

	private static class Text_Info
	{
		public static IPanel Panel => TitleTextPanelComponent.Build(OnReset);

		private static Action<TitleTextPanelComponent> OnReset => delegate(TitleTextPanelComponent panel)
		{
			panel._layoutElement.minWidth = -1f;
			panel._layoutElement.minHeight = 0f;
			panel._layoutElement.preferredWidth = -1f;
			panel._layoutElement.preferredHeight = -1f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
			panel._fitter.enabled = false;
			panel._label.text = I2Helper.TryGet("##news_mastodonte_info");
			panel._label.color = Colors.Black;
			panel._label.fontSize = 12;
			panel._label.font = BrixSingleton<LayoutPanelManager>.Instance.ProximaNovaRegular;
			panel._label.alignment = TextAnchor.UpperLeft;
		};
	}

	private static class Spacer_8px
	{
		public static IPanel Panel => SpacerPanelComponent.Build(OnReset);

		private static Action<SpacerPanelComponent> OnReset => delegate(SpacerPanelComponent panel)
		{
			panel._layoutElement.minHeight = -1f;
			panel._layoutElement.preferredHeight = 8f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
		};
	}

	private static class Spacer_24px
	{
		public static IPanel Panel => SpacerPanelComponent.Build(OnReset);

		private static Action<SpacerPanelComponent> OnReset => delegate(SpacerPanelComponent panel)
		{
			panel._layoutElement.minHeight = -1f;
			panel._layoutElement.preferredHeight = 24f;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = -1f;
		};
	}

	private static class Button_Start
	{
		private static readonly ColorBlock ColorBlock = new ColorBlock
		{
			colorMultiplier = 1f,
			disabledColor = Colors.LightYellow,
			normalColor = Colors.LightYellow,
			highlightedColor = Colors.LighterYellow,
			pressedColor = Colors.White
		};

		public static IPanel Panel => LegacyButtonPanelComponent.Build(OnReset).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_DblTxt_RightIcon);

		private static Action<LegacyButtonPanelComponent> OnReset => delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##news_mastodonte_start").ToUpper();
			panel._label.color = Colors.Black;
			panel._keyLabel.text = I2Helper.TryGet("##news_mastodonte_start_info");
			panel._keyLabel.color = Colors.Black;
			panel._selectionImage.color = Colors.Black;
			panel._selectionImage.sprite = IconKeys._UI_Big_Arrow_Right.Get64();
			panel._button.colors = ColorBlock;
			panel._button.interactable = true;
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				UNetManager.instance.LocalPlayerName = ((!string.IsNullOrEmpty(Neo.multiplayerName)) ? Neo.multiplayerName : "Player");
				BaseMapLibraryMenu.SetLoadedMap(AssetDispensary.maps.SendCodenameQuery("Invictus_07a8b3e0-94d1-4cd8-9641-fc815121715b"));
				BaseMapLibraryMenu.SetLoadedGamemode(AssetDispensary.gamemodes.SendCodenameQuery("sandbox"));
				BaseMapLibraryMenu.SetLoadedPreset(AssetDispensary.presets.SendCodenameQuery("sandbox_default_31f05863-96bf-4cdd-b21d-186415bb4794"));
				GameParam.StartGame(delegate
				{
					BaseMapLibraryMenu.EmptySelected();
				});
				MenuSound.PlaySound(MenuSound.Prefix.Title, MenuSound.Suffix.GameStart);
			});
		};
	}

	private static class Button_ReadMore
	{
		private static readonly ColorBlock ColorBlock = new ColorBlock
		{
			colorMultiplier = 1f,
			disabledColor = Colors.LightYellow,
			normalColor = Colors.LightYellow,
			highlightedColor = Colors.LighterYellow,
			pressedColor = Colors.White
		};

		public static IPanel Panel => LegacyButtonPanelComponent.Build(OnReset).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_DblTxt_RightIcon);

		private static Action<LegacyButtonPanelComponent> OnReset => delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##news_mastodonte_readmore").ToUpper();
			panel._label.color = Colors.Black;
			panel._keyLabel.text = I2Helper.TryGet("##news_mastodonte_readmore_info");
			panel._keyLabel.color = Colors.Black;
			panel._selectionImage.color = Colors.Black;
			panel._selectionImage.sprite = IconKeys._UI_Book.Get64();
			panel._button.colors = ColorBlock;
			panel._button.interactable = true;
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				string text = "http://www.castlestory.net/en/update/6288-119-The-Blueprint-Library-Update";
				Process.Start("steam://openurl/" + text);
			});
		};
	}

	private static readonly Signal RefreshSignal = new Signal();

	public NewsMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NewsMenu New(Func<RectTransform> binGetter)
	{
		return new NewsMenu(binGetter);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanel(VerticalLayout_Main.Panel.AddPanel(Spacer_24px.Panel).AddPanel(Spacer_24px.Panel).AddPanel(Text_SectionTitle.Panel)
			.AddPanel(Spacer_24px.Panel)
			.AddPanel(Image_Thumbnail.Panel)
			.AddPanel(Spacer_24px.Panel)
			.AddPanel(VerticalLayout_Content.Panel.AddPanel(Text_Title.Panel).AddPanel(Spacer_8px.Panel).AddPanel(Text_DateTime.Panel)
				.AddPanel(Spacer_8px.Panel)
				.AddPanel(Text_Info.Panel))
			.AddPanel(Spacer.Panel)
			.AddPanel(Button_Start.Panel)
			.AddPanel(Button_ReadMore.Panel)
			.AddPanel(Spacer_24px.Panel)
			.AddPanel(Spacer_24px.Panel));
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}
}
