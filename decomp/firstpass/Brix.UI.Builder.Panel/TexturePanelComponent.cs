using System;
using Brix.External.Factories;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class TexturePanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<TexturePanelComponent>
	{
		public enum Variant
		{
			MapTexture,
			ThumbTexture,
			MapTextureReskin
		}

		private Variant _variant;

		protected override Factory.AssetKey AssetKey => _variant switch
		{
			Variant.MapTexture => Images.MapTexture, 
			Variant.MapTextureReskin => Images.MapTextureReskin, 
			_ => Images.ThumbTexture, 
		};

		public BasePanel(Action<TexturePanelComponent> onReset, Action<TexturePanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}

		public BasePanel As(Variant variant)
		{
			_variant = variant;
			return this;
		}
	}

	public RawImage _rawImage;

	public AspectRatioFitter _rawImageFitter;

	public Image _bgImage;

	public Image _bgImage2;

	public GameObject _loadingImageGo;

	public Image _missingImage;

	public LayoutElement _layoutElement;

	public static BasePanel Build(Action<TexturePanelComponent> onReset = null, Action<TexturePanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
