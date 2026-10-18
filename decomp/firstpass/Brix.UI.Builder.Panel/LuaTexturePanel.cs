using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class LuaTexturePanel : BaseWidgetPanel<LuaTexturePanelComponent, LuaTexturePanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		RawImage
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Images.LuaTexture;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public LuaTexturePanel(bool luaDriven = false)
		: base((Action<LuaTexturePanelComponent>)null, (Action<LuaTexturePanelComponent>)null, luaDriven)
	{
	}

	public LuaTexturePanel(Action<LuaTexturePanelComponent> onReset, Action<LuaTexturePanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.RawImage => base.Instance._textureWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public LuaTexturePanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Images.LuaTexture;
		}
		return this;
	}

	public static LuaTexturePanel LuaNew()
	{
		return new LuaTexturePanel(luaDriven: true);
	}

	public static LuaTexturePanel New(Action<LuaTexturePanelComponent> onReset = null, Action<LuaTexturePanelComponent> onRefresh = null)
	{
		return new LuaTexturePanel(onReset, onRefresh);
	}
}
