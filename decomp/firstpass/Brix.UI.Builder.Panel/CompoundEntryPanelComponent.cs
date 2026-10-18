using System;
using Brix.External.Factories;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class CompoundEntryPanelComponent : BasePanelComponent
{
	public enum Variant
	{
		Default,
		Reskin
	}

	public class BasePanel : BasePanel<CompoundEntryPanelComponent>
	{
		private Factory.AssetKey _assetKey = Buttons.CompoundEntry;

		protected override Factory.AssetKey AssetKey => _assetKey;

		public BasePanel(Action<CompoundEntryPanelComponent> onReset, Action<CompoundEntryPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}

		public BasePanel As(Variant variant)
		{
			switch (variant)
			{
			case Variant.Default:
				_assetKey = Buttons.CompoundEntry;
				break;
			case Variant.Reskin:
				_assetKey = Buttons.CompoundEntryReskin;
				break;
			}
			return this;
		}
	}

	public Text _label;

	public Button _button;

	public EventTrigger _eventTrigger;

	public LayoutElement _layoutElement;

	public Image _selectImage;

	public Button _selectButton;

	public EventTrigger _selectEventTrigger;

	public GameObject _spaceGo;

	public GameObject _loadingWheelGo;

	public static BasePanel Build(Action<CompoundEntryPanelComponent> onReset = null, Action<CompoundEntryPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
