using System;
using Brix.External.Factories;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class TitleTextPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<TitleTextPanelComponent>
	{
		public enum Variant
		{
			Standard,
			StandardReskin,
			SidebarHelpText
		}

		private Variant _variant;

		protected override Factory.AssetKey AssetKey => _variant switch
		{
			Variant.Standard => Texts.Title, 
			Variant.StandardReskin => Texts.TitleReskin, 
			Variant.SidebarHelpText => Texts.SidebarHelpText, 
			_ => null, 
		};

		public BasePanel(Action<TitleTextPanelComponent> onReset, Action<TitleTextPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}

		public BasePanel As(Variant variant)
		{
			_variant = variant;
			return this;
		}
	}

	public Text _label;

	public LayoutElement _layoutElement;

	public ContentSizeFitter _fitter;

	public static BasePanel Build(Action<TitleTextPanelComponent> onReset = null, Action<TitleTextPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
