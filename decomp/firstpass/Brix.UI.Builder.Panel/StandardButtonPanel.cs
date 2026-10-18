using System;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;

namespace Brix.UI.Builder.Panel;

public class StandardButtonPanel : BaseWidgetPanel<StandardButtonPanelComponent, StandardButtonPanel.WidgetId>
{
	public enum WidgetId
	{
		LayoutElement,
		Button,
		FrameAnimator
	}

	public enum Variant
	{
		Default
	}

	private Factory.AssetKey _assetKey = Buttons.Standard;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public StandardButtonPanel(bool luaDriven = false)
		: base((Action<StandardButtonPanelComponent>)null, (Action<StandardButtonPanelComponent>)null, luaDriven)
	{
	}

	public StandardButtonPanel(Action<StandardButtonPanelComponent> onReset, Action<StandardButtonPanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override IWidgetComponent GetWidgetComponent(WidgetId widgetId)
	{
		return widgetId switch
		{
			WidgetId.Button => base.Instance._buttonWidget, 
			WidgetId.LayoutElement => base.Instance._layoutElementWidget, 
			WidgetId.FrameAnimator => base.Instance._frameAnimatorWidget, 
			_ => throw new NotImplementedException(), 
		};
	}

	public StandardButtonPanel As(Variant variant)
	{
		if (variant == Variant.Default)
		{
			_assetKey = Buttons.Standard;
		}
		return this;
	}

	public static StandardButtonPanel LuaNew()
	{
		return new StandardButtonPanel(luaDriven: true);
	}

	public static StandardButtonPanel New(Action<StandardButtonPanelComponent> onReset = null, Action<StandardButtonPanelComponent> onRefresh = null)
	{
		return new StandardButtonPanel(onReset, onRefresh);
	}
}
