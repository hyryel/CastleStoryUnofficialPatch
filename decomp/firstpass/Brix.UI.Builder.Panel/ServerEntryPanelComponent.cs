using System;
using Brix.External.Factories;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class ServerEntryPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<ServerEntryPanelComponent>
	{
		public enum Variant
		{
			Default,
			Reskin
		}

		private Factory.AssetKey _assetKey = InputFields.ServerEntry;

		protected override Factory.AssetKey AssetKey => _assetKey;

		public BasePanel(Action<ServerEntryPanelComponent> onReset, Action<ServerEntryPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}

		public BasePanel As(Variant variant)
		{
			switch (variant)
			{
			case Variant.Default:
				_assetKey = InputFields.ServerEntry;
				break;
			case Variant.Reskin:
				_assetKey = InputFields.ServerEntryB;
				break;
			}
			return this;
		}
	}

	public Text _keyLabel;

	public InputField _inputField;

	public EventTrigger _inputFieldEventTrigger;

	public Text _inputFieldPlaceholderText;

	public LayoutElement _layoutElement;

	public Image _selectImage;

	public Button _selectButton;

	public EventTrigger _selectEventTrigger;

	public LayoutElement _selectLayoutElement;

	public Button _fullSelectButton;

	public EventTrigger _fullSelectEventTrigger;

	public LayoutElement _fullSelectLayoutElement;

	public static BasePanel Build(Action<ServerEntryPanelComponent> onReset = null, Action<ServerEntryPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
