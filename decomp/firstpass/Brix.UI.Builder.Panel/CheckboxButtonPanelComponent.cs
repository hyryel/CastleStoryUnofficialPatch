using System;
using Brix.External.Factories;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class CheckboxButtonPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<CheckboxButtonPanelComponent>
	{
		protected override Factory.AssetKey AssetKey => Buttons.Checkbox;

		public BasePanel(Action<CheckboxButtonPanelComponent> onReset, Action<CheckboxButtonPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}
	}

	public LayoutElement _spacerLayoutElement;

	public Toggle _toggle;

	public Text _label;

	public Animator _animator;

	public LayoutElement _layoutElement;

	public ScrollRectHelper _scrollRectHelper;

	public static BasePanel Build(Action<CheckboxButtonPanelComponent> onReset = null, Action<CheckboxButtonPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
