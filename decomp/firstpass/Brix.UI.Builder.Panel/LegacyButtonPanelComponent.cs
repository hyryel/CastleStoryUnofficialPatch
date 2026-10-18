using System;
using Brix.External.Factories;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class LegacyButtonPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<LegacyButtonPanelComponent>
	{
		public enum Variant
		{
			Legacy,
			LegacyBig,
			LegacyBigPlay,
			LegacyReskin_SglTxt_LeftIcon,
			LegacyReskin_DblTxt_RightIcon,
			LegacyReskinCheckbox
		}

		private Variant _variant;

		protected override Factory.AssetKey AssetKey => _variant switch
		{
			Variant.Legacy => Buttons.Legacy, 
			Variant.LegacyBig => Buttons.LegacyBig, 
			Variant.LegacyBigPlay => Buttons.LegacyBigPlay, 
			Variant.LegacyReskin_SglTxt_LeftIcon => Buttons.LegacyReskin, 
			Variant.LegacyReskin_DblTxt_RightIcon => Buttons.LegacyReskinIcon, 
			Variant.LegacyReskinCheckbox => Buttons.LegacyReskinCheckbox, 
			_ => throw new NotImplementedException(), 
		};

		public BasePanel(Action<LegacyButtonPanelComponent> onReset, Action<LegacyButtonPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}

		public BasePanel As(Variant variant)
		{
			_variant = variant;
			return this;
		}
	}

	public Button _button;

	public Text _label;

	public Text _keyLabel;

	public Animator _animator;

	public LayoutElement _layoutElement;

	public Image _selectionImage;

	public static BasePanel Build(Action<LegacyButtonPanelComponent> onReset = null, Action<LegacyButtonPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
