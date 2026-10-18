using System;
using Brix.External.Factories;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class SpawnerEntryPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<SpawnerEntryPanelComponent>
	{
		protected override Factory.AssetKey AssetKey => Buttons.SpawnerEntry;

		public BasePanel(Action<SpawnerEntryPanelComponent> onReset, Action<SpawnerEntryPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}
	}

	public Dropdown _dropdown;

	public LayoutElement _dropdownLayoutElement;

	public EventTrigger _dropdownEventTrigger;

	public InputField _inputField;

	public Text _inputFieldPlaceholderText;

	public EventTrigger _inputFieldEventTrigger;

	public Image _selectImage;

	public Button _selectButton;

	public Image _deleteImage;

	public Button _deleteButton;

	public LayoutElement _layoutElement;

	public static BasePanel Build(Action<SpawnerEntryPanelComponent> onReset = null, Action<SpawnerEntryPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
