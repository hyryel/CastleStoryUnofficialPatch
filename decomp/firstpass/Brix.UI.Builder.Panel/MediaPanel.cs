using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class MediaPanel : BaseWidgetPanel<MediaPanelComponent, MediaPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		Media
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Images.Default;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public MediaPanel(bool luaDriven = false)
		: base((Action<MediaPanelComponent>)null, (Action<MediaPanelComponent>)null, luaDriven)
	{
	}

	public MediaPanel(Action<MediaPanelComponent> onReset, Action<MediaPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.Media => base.Instance._mediaWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public MediaPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Images.Default;
		}
		return this;
	}

	public static MediaPanel LuaNew()
	{
		return new MediaPanel(luaDriven: true);
	}

	public static MediaPanel New(Action<MediaPanelComponent> onReset = null, Action<MediaPanelComponent> onRefresh = null)
	{
		return new MediaPanel(onReset, onRefresh);
	}
}
