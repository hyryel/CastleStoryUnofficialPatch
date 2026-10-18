using System;
using Brix.External.Factories;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class SpacerPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<SpacerPanelComponent>
	{
		public enum Variant
		{
			Standard,
			Sidebar
		}

		private Variant _variant;

		protected override Factory.AssetKey AssetKey => _variant switch
		{
			Variant.Standard => Layouts.Spacer, 
			Variant.Sidebar => Layouts.SpacerSidebar, 
			_ => null, 
		};

		public BasePanel(Action<SpacerPanelComponent> onReset, Action<SpacerPanelComponent> onRefresh)
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

	public static BasePanel Build(Action<SpacerPanelComponent> onReset = null, Action<SpacerPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
