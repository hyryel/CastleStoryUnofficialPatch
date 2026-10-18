using System;
using Brix.External.Factories;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class ChatPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<ChatPanelComponent>
	{
		protected override Factory.AssetKey AssetKey => Panels.Chat;

		public BasePanel(Action<ChatPanelComponent> onReset, Action<ChatPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}
	}

	public InputField _inputField;

	public EventTrigger _inputFieldEventTrigger;

	public Text _inputFieldPlaceholderText;

	public Text _outputText;

	public ScrollRect _outputScrollRect;

	public static BasePanel Build(Action<ChatPanelComponent> onReset = null, Action<ChatPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
