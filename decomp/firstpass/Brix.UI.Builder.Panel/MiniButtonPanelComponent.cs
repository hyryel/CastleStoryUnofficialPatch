using System;
using Brix.External.Factories;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class MiniButtonPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<MiniButtonPanelComponent>
	{
		public enum Variant
		{
			Mini,
			Universal
		}

		private Variant _variant;

		protected override Factory.AssetKey AssetKey => _variant switch
		{
			Variant.Mini => Buttons.Mini, 
			Variant.Universal => Buttons.Universal, 
			_ => throw new NotImplementedException(), 
		};

		public BasePanel(Action<MiniButtonPanelComponent> onReset, Action<MiniButtonPanelComponent> onRefresh)
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

	public Button _button;

	public EventTrigger _eventTrigger;

	public Image _image;

	public Image _badgeImage;

	public RawImage _rawImage;

	public Text _label;

	public Animator _stateAnimator;

	public Animator _colorAnimator;

	public Animator _iconAnimator;

	public Animator _frameAnimator;

	public Text _hotkeyLabel;

	public static BasePanel Build(Action<MiniButtonPanelComponent> onReset = null, Action<MiniButtonPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
