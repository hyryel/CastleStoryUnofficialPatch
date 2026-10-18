using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class ImagePanel : BaseWidgetPanel<ImagePanelComponent, ImagePanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		Image
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Images.Default;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public ImagePanel(bool luaDriven = false)
		: base((Action<ImagePanelComponent>)null, (Action<ImagePanelComponent>)null, luaDriven)
	{
	}

	public ImagePanel(Action<ImagePanelComponent> onReset, Action<ImagePanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.Image => base.Instance._imageWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public ImagePanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Images.Default;
		}
		return this;
	}

	public static ImagePanel LuaNew()
	{
		return new ImagePanel(luaDriven: true);
	}

	public static ImagePanel New(Action<ImagePanelComponent> onReset = null, Action<ImagePanelComponent> onRefresh = null)
	{
		return new ImagePanel(onReset, onRefresh);
	}
}
