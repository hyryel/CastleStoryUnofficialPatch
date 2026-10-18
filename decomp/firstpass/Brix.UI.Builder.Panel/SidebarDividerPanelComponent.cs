using System;
using Brix.External.Factories;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class SidebarDividerPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<SidebarDividerPanelComponent>
	{
		public enum Variant
		{
			Content,
			Tab
		}

		private Variant _variant;

		protected override Factory.AssetKey AssetKey => _variant switch
		{
			Variant.Content => Buttons.SidebarContentDivider, 
			Variant.Tab => Buttons.SidebarTabDivider, 
			_ => null, 
		};

		public BasePanel(Action<SidebarDividerPanelComponent> onReset, Action<SidebarDividerPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}

		public BasePanel As(Variant variant)
		{
			_variant = variant;
			return this;
		}
	}

	public LayoutElement _layoutElement;

	public Image _dividerImage;

	public static BasePanel Build(Action<SidebarDividerPanelComponent> onReset = null, Action<SidebarDividerPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
