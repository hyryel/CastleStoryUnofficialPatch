using System;
using Brix.External.Factories;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class InputFieldPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<InputFieldPanelComponent>
	{
		public enum Variant
		{
			Standard,
			Short,
			Password,
			ReskinLeft,
			Reskin
		}

		private Variant _variant;

		protected override Factory.AssetKey AssetKey => _variant switch
		{
			Variant.Short => InputFields.Short, 
			Variant.Password => InputFields.Password, 
			Variant.ReskinLeft => InputFields.ReskinInputLeft, 
			Variant.Reskin => InputFields.ReskinInput, 
			_ => InputFields.Standard, 
		};

		public BasePanel(Action<InputFieldPanelComponent> onReset, Action<InputFieldPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}

		public BasePanel As(Variant variant)
		{
			_variant = variant;
			return this;
		}
	}

	public InputField _inputField;

	public Text _inputFieldText;

	public EventTrigger _inputFieldEventTrigger;

	public Text _inputFieldPlaceholderText;

	public LayoutElement _layoutElement;

	public Animator _frameAnimator;

	public static BasePanel Build(Action<InputFieldPanelComponent> onReset = null, Action<InputFieldPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
