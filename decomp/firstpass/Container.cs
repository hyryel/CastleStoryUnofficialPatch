using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Container : MonoBehaviour
{
	[Serializable]
	public class Item
	{
		public enum Group
		{
			None,
			Alpha,
			Beta,
			Gamma,
			Delta,
			Epsilon
		}

		[Serializable]
		public class ActionItem
		{
			public enum Action
			{
				None,
				Open,
				Close,
				Toggle
			}

			public Action action;

			public Containee target;

			public bool doSimulate;
		}

		public enum ActivationAction
		{
			None,
			Activate,
			Deactivate,
			Toggle
		}

		public bool usesOriginalAssetKey;

		public Factory.AssetKey originalAssetKey;

		public Containee original;

		public Containee instance;

		public Transform parentTrs;

		public bool keepImageEnabled;

		public bool keepSpriteImageEnabled;

		public Texture2D imageTexture;

		public Sprite sprite;

		public int size;

		public static string StrEnabled = "##mainmenu_options_enabled";

		public static string StrDisabled = "##mainmenu_options_disabled";

		public Sprite toggleImageTexture;

		public string titleText = string.Empty;

		public string subtitleText = string.Empty;

		public string subsubtitleText = string.Empty;

		public string subsubsubtitleText = string.Empty;

		public string inputFieldText = string.Empty;

		public bool toggleFlag;

		public bool toggleEnabled = true;

		public float sliderValue;

		public string sliderValueTitleText = string.Empty;

		public bool followParentMode;

		public MenuContext menuContext = MenuContext.Both;

		public NetworkContext networkContext = NetworkContext.Both;

		public DebugContext debugContext;

		public WorldEditorContext worldEditorContext = WorldEditorContext.Both;

		public PlayerContext playerContext = PlayerContext.Both;

		public bool usePreferredSpreadOffset;

		public float preferredSpreadOffset;

		public bool usePreferredSpreadRangeSize;

		public float preferredSpreadRangeSize;

		public Color color;

		public Color bgColor;

		public string localizeTitle;

		public string localizeSubtitle;

		public int idxOffset;

		public float MinWidth = -1f;

		public float MinHeight = -1f;

		public float PrefWidth = -1f;

		public float PrefHeight = -1f;

		public float FlexWidth = 1f;

		public float FlexHeight = 1f;

		public bool isOverrideCurrentState;

		public Containee.State currentState = Containee.State.Closed;

		public bool isOverrideTargetState;

		public Containee.State targetState;

		public Group group;

		public bool isDefaultInGroup;

		public bool isDynamic;

		public bool doClick;

		public List<ActionItem> actionItems = new List<ActionItem>();

		public LegacyBaseMenu.Operation containerPopulatorOperation;

		public bool IsTitleCaps = true;

		public bool IsSubtitleCaps = true;

		public bool IsSubsubtitleCaps = true;

		public bool IsSubsubsubtitleCaps = true;

		public bool IsSlidertitleCaps = true;

		public bool IsToggletitleCaps = true;

		public Action<ContaineeExtended> plugUpdateAction;

		public Item()
		{
		}

		public Item(Containee original, Containee instance, Transform parentTrs, Texture2D imageTexture, string titleText, string subtitleText, bool toggleFlag, bool toggleEnabled, Color color, Color bgColor, int idxOffset, bool isDefault, Group group, bool isDynamic, bool followParentMode, string localizeTitle = "", string localizeSubtitle = "")
		{
			this.original = original;
			this.instance = instance;
			this.parentTrs = parentTrs;
			this.imageTexture = imageTexture;
			this.titleText = I2Helper.TryGet(titleText);
			this.subtitleText = I2Helper.TryGet(subtitleText);
			this.toggleFlag = toggleFlag;
			this.toggleEnabled = toggleEnabled;
			this.color = color;
			this.bgColor = bgColor;
			this.idxOffset = idxOffset;
			this.group = group;
			this.isDynamic = isDynamic;
			this.localizeTitle = I2Helper.TryGet(localizeTitle);
			this.localizeSubtitle = I2Helper.TryGet(localizeSubtitle);
			this.followParentMode = followParentMode;
		}

		public void Purge()
		{
			if (instance != null)
			{
				UnityEngine.Object.DestroyImmediate(instance.gameObject);
				instance = null;
			}
		}
	}

	public enum MenuContext
	{
		None,
		Title,
		Game,
		Both
	}

	public enum NetworkContext
	{
		None,
		Client,
		Host,
		Both
	}

	public enum DebugContext
	{
		Both,
		DebugOnly,
		NonDebugOnly
	}

	public enum PlayerContext
	{
		None,
		Single,
		Multi,
		Both
	}

	public enum WorldEditorContext
	{
		None,
		Both,
		IsEditor,
		IsNotEditor
	}

	public bool DebugThis;

	private static BrixLogger logger = new BrixLogger(typeof(Container).FullName);

	public List<Item> items = new List<Item>();

	public Signal OnPopulateSignal = new Signal();

	private static readonly Dictionary<Containee.State, string> StateNameMap = new Dictionary<Containee.State, string>(FastEnumIntEqualityComparer<Containee.State>.Instance)
	{
		{
			Containee.State.None,
			"None"
		},
		{
			Containee.State.Opened,
			"Opened"
		},
		{
			Containee.State.Closed,
			"Closed"
		}
	};

	public Signal<int, bool> OnItemHoverSignal = new Signal<int, bool>();

	public Signal<int> OnItemClickSignal = new Signal<int>();

	public Signal<int> OnItemAltClickSignal = new Signal<int>();

	public Signal<int, bool> OnItemToggleSignal = new Signal<int, bool>();

	public Signal<int, float> OnItemScrollSignal = new Signal<int, float>();

	public Signal<int, float> OnItemScrollEndSignal = new Signal<int, float>();

	public Signal<int, string> OnItemTextSignal = new Signal<int, string>();

	public Signal OnAfterEnableSignal = new Signal();

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	public List<Item.ActionItem> _skipToLobbyActions = new List<Item.ActionItem>();

	public List<Item.ActionItem> _unskipToLobbyActions = new List<Item.ActionItem>();

	public List<Item.ActionItem> _closeSaveGameMenuActions = new List<Item.ActionItem>();

	public static GameSignals.GameSignal CloseSaveGameMenuSignal = new GameSignals.GameSignal();

	private static readonly Action<Container> OnCloseSaveGameMenu = delegate(Container menu)
	{
		RunActions(menu._closeSaveGameMenuActions);
	};

	public List<Item.ActionItem> _reOpenOptionsMenu = new List<Item.ActionItem>();

	public static GameSignals.GameSignal ReOpenOptionsMenuSignal = new GameSignals.GameSignal();

	private static readonly Action<Container> OnReOpenOptionsMenu = delegate(Container menu)
	{
		RunActions(menu._reOpenOptionsMenu);
	};

	public MenuSound.Prefix soundPrefix => (!(SceneManager.GetActiveScene().name == "SceneMenu")) ? MenuSound.Prefix.Main : MenuSound.Prefix.Title;

	public MenuContext menuContext => (SceneManager.GetActiveScene().name == "SceneMenu") ? MenuContext.Title : MenuContext.Game;

	public WorldEditorContext worldEditorContext => (InputModeController.DefaultMode != InputMode.worldEditor) ? WorldEditorContext.IsNotEditor : WorldEditorContext.IsEditor;

	public PlayerContext playerContext => (!Neo.multiplayer) ? PlayerContext.Single : PlayerContext.Multi;

	public void Populate(bool isChain)
	{
		Populate(items, isChain, purgeOnly: false);
	}

	public void Populate(List<Item> items, bool isChain)
	{
		Populate(items, isChain, purgeOnly: false);
	}

	public void Populate(bool isChain, bool purgeOnly)
	{
		Populate(items, isChain, purgeOnly);
	}

	public void Populate(List<Item> items, bool isChain, bool purgeOnly)
	{
		GameSignals.Invoke(GameSignals.OnMenuApply, this);
		foreach (Item item in items)
		{
			if (IsFiltered(item))
			{
				continue;
			}
			if (!item.isDynamic)
			{
				if (item.instance == null)
				{
					string t = ((!(base.transform.parent == null)) ? $"{base.transform.parent.name}/" : string.Empty);
					logger.ErrorFormat("Non-Dynamic entry should contain instance: #{0} {1}{2}", items.IndexOf(item), t, base.name);
				}
				else if (item.instance.interpolatorTargetable != null)
				{
					item.instance.interpolatorTargetable.PlayTo(StateNameMap[item.targetState]);
					return;
				}
				continue;
			}
			item.Purge();
			if (!purgeOnly)
			{
				GameObject go;
				if (item.usesOriginalAssetKey)
				{
					ObjectPoolSingleton.Request(out go, item.originalAssetKey);
				}
				else
				{
					go = UnityEngine.Object.Instantiate(item.original.gameObject);
				}
				go.name = go.name.Replace("(Clone)", " *");
				go.hideFlags = HideFlags.DontSave;
				go.transform.SetParent((!(item.parentTrs != null)) ? base.transform : item.parentTrs, worldPositionStays: false);
				go.transform.localScale = Vector3.one;
				go.transform.SetSiblingIndex(go.transform.GetSiblingIndex() + item.idxOffset);
				if (item.followParentMode)
				{
					go.transform.localPosition = Vector3.zero;
					go.transform.right = Vector3.zero;
					go.GetComponent<LayoutElement>().ignoreLayout = true;
				}
				item.instance = go.GetComponent<Containee>();
				item.instance.container = this;
			}
		}
		Activate();
		Apply();
		GameSignals.Invoke(GameSignals.OnMenuApply, this);
		OnPopulateSignal.Invoke();
	}

	private bool IsFiltered(Item item)
	{
		if (item.debugContext == DebugContext.DebugOnly)
		{
			return true;
		}
		if (menuContext == MenuContext.None)
		{
			return true;
		}
		if (item.menuContext == MenuContext.None)
		{
			return true;
		}
		if (item.menuContext != MenuContext.Both)
		{
			if (menuContext == MenuContext.Game && item.menuContext != MenuContext.Game)
			{
				return true;
			}
			if (menuContext == MenuContext.Title && item.menuContext != MenuContext.Title)
			{
				return true;
			}
		}
		if (item.worldEditorContext == WorldEditorContext.None)
		{
			return true;
		}
		if (item.worldEditorContext != WorldEditorContext.Both)
		{
			if (worldEditorContext == WorldEditorContext.IsEditor && item.worldEditorContext != WorldEditorContext.IsEditor)
			{
				return true;
			}
			if (worldEditorContext == WorldEditorContext.IsNotEditor && item.worldEditorContext != WorldEditorContext.IsNotEditor)
			{
				return true;
			}
		}
		if (item.playerContext == PlayerContext.None)
		{
			return true;
		}
		if (item.playerContext != PlayerContext.Both)
		{
			if (playerContext == PlayerContext.Single && item.playerContext != PlayerContext.Single)
			{
				return true;
			}
			if (playerContext == PlayerContext.Multi && item.playerContext != PlayerContext.Multi)
			{
				return true;
			}
		}
		bool isHost = LobbyMenu.IsHost;
		switch (item.networkContext)
		{
		case NetworkContext.Client:
			if (isHost)
			{
				return true;
			}
			break;
		case NetworkContext.Host:
			if (!isHost)
			{
				return true;
			}
			break;
		case NetworkContext.None:
			return true;
		}
		return false;
	}

	public void Apply()
	{
		foreach (Item item in items)
		{
			if (!IsFiltered(item))
			{
				if (item.instance == null)
				{
					break;
				}
				Apply(item);
			}
		}
	}

	public void Activate()
	{
		foreach (Item item in items)
		{
			if (item.instance == null)
			{
				continue;
			}
			item.instance.currentState = item.currentState;
			item.instance.targetState = item.targetState;
			if (item.currentState == Containee.State.Closed)
			{
				item.instance.gameObject.SetActive(value: false);
			}
			else if (item.currentState == Containee.State.Opened && !IsFiltered(item))
			{
				if (item.titleText.Contains("Debug"))
				{
					item.instance.gameObject.SetActive(value: false);
				}
				else
				{
					item.instance.gameObject.SetActive(value: true);
				}
			}
		}
	}

	private void Apply(Item item)
	{
		if (item.instance.image != null)
		{
			item.instance.image.texture = item.imageTexture;
			if (item.imageTexture != null || item.keepImageEnabled)
			{
				item.instance.image.enabled = true;
				SetImageColor(item.instance.image, item.color);
				SetImageSize(item.instance.image, item.size);
			}
			else
			{
				item.instance.image.enabled = false;
			}
		}
		if (item.instance.spriteImage != null)
		{
			item.instance.spriteImage.sprite = item.sprite;
			if (item.sprite != null || item.keepSpriteImageEnabled)
			{
				item.instance.spriteImage.enabled = true;
				SetImageColor(item.instance.spriteImage, item.color);
				SetImageSize(item.instance.spriteImage, item.size);
			}
			else
			{
				item.instance.spriteImage.enabled = false;
			}
		}
		if (item.instance.toggleImage != null)
		{
			item.instance.toggleImage.sprite = item.toggleImageTexture;
			if (item.toggleImageTexture != null)
			{
				item.instance.toggleImage.enabled = true;
				item.instance.toggleImage.color = Color.white;
			}
			else
			{
				item.instance.toggleImage.enabled = false;
			}
		}
		if (item.instance.title != null)
		{
			item.instance.title.text = I2Helper.TryGet(item.titleText);
			if (item.IsTitleCaps)
			{
				item.instance.title.text = item.instance.title.text.ToUpper();
			}
		}
		if (item.instance.subtitle != null)
		{
			item.instance.subtitle.text = I2Helper.TryGet(item.subtitleText);
			if (item.IsSubtitleCaps)
			{
				item.instance.subtitle.text = item.instance.subtitle.text.ToUpper();
			}
		}
		if (item.instance.subsubtitle != null)
		{
			item.instance.subsubtitle.text = I2Helper.TryGet(item.subsubtitleText);
			if (item.IsSubsubtitleCaps)
			{
				item.instance.subsubtitle.text = item.instance.subsubtitle.text.ToUpper();
			}
		}
		if (item.instance.subsubsubtitle != null)
		{
			item.instance.subsubsubtitle.text = I2Helper.TryGet(item.subsubsubtitleText);
			if (item.IsSubsubsubtitleCaps)
			{
				item.instance.subsubsubtitle.text = item.instance.subsubsubtitle.text.ToUpper();
			}
		}
		if (item.instance.sliderValueTitle != null)
		{
			item.instance.sliderValueTitle.text = I2Helper.TryGet(item.sliderValueTitleText);
			if (item.IsSlidertitleCaps)
			{
				item.instance.sliderValueTitle.text = item.instance.sliderValueTitle.text.ToUpper();
			}
		}
		if (item.usePreferredSpreadOffset)
		{
			RadialLayoutElement component = item.instance.GetComponent<RadialLayoutElement>();
			if (component != null)
			{
				component.usePreferredSpreadOffset = true;
				component.preferredSpreadOffset = item.preferredSpreadOffset;
			}
		}
		if (item.usePreferredSpreadRangeSize)
		{
			RadialLayoutElement component2 = item.instance.GetComponent<RadialLayoutElement>();
			if (component2 != null)
			{
				component2.usePreferredSpreadRangeSize = true;
				component2.preferredSpreadRangeSize = item.preferredSpreadRangeSize;
			}
		}
		if (item.instance.inputField != null)
		{
			item.instance.inputField.text = I2Helper.TryGet(item.inputFieldText);
		}
		if (item.instance.toggle != null)
		{
			item.instance.toggle.isOn = item.toggleFlag;
		}
		if (item.instance.toggleText != null)
		{
			item.instance.toggleText.text = I2Helper.TryGet((!item.toggleFlag) ? Item.StrDisabled : Item.StrEnabled);
			if (item.IsToggletitleCaps)
			{
				item.instance.toggleText.text = item.instance.toggleText.text.ToUpper();
			}
		}
		if (item.instance.toggle != null)
		{
			item.instance.toggle.interactable = item.toggleEnabled;
		}
		if (item.instance.slider != null)
		{
			item.instance.slider.value = item.sliderValue;
		}
		if (item.instance.layoutElement != null)
		{
			item.instance.layoutElement.minWidth = item.MinWidth;
			item.instance.layoutElement.minHeight = item.MinHeight;
			item.instance.layoutElement.preferredWidth = item.PrefWidth;
			item.instance.layoutElement.preferredHeight = item.PrefHeight;
			item.instance.layoutElement.flexibleWidth = item.FlexWidth;
			item.instance.layoutElement.flexibleHeight = item.FlexHeight;
		}
		ContaineeExtended containeeExtended = item.instance as ContaineeExtended;
		if (containeeExtended != null && item.plugUpdateAction != null)
		{
			item.plugUpdateAction(containeeExtended);
		}
	}

	private void SetImageColor(Graphic image, Color color)
	{
		image.color = color;
	}

	private void SetImageSize(Graphic image, int customSize)
	{
		switch (customSize)
		{
		case -1:
			image.SetNativeSize();
			break;
		default:
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, customSize);
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, customSize);
			break;
		case 0:
			break;
		}
	}

	public Item GetItem(LegacyBaseMenu.Operation operation)
	{
		foreach (Item item in items)
		{
			if (item.containerPopulatorOperation == operation)
			{
				return item;
			}
		}
		return null;
	}

	public Item GetItem(int idx)
	{
		return (idx < 0 || idx >= items.Count) ? null : items[idx];
	}

	public int GetItemIdx(Containee containee)
	{
		for (int i = 0; i < items.Count; i++)
		{
			Item item = items[i];
			if (item.instance == containee)
			{
				return i;
			}
		}
		return -1;
	}

	public void CloseAll()
	{
		foreach (Item item in items)
		{
			if (item.instance == null || item.instance.currentState == Containee.State.Closed)
			{
				continue;
			}
			if (item.instance.interpolatorTargetable != null)
			{
				item.instance.interpolatorTargetable.PlayTo(StateNameMap[Containee.State.Closed]);
				continue;
			}
			item.instance.currentState = Containee.State.Closed;
			item.instance.targetState = Containee.State.None;
			if (false)
			{
				if (item.titleText.Contains("Debug"))
				{
					item.instance.gameObject.SetActive(value: false);
				}
				else
				{
					item.instance.gameObject.SetActive(value: true);
				}
			}
			else
			{
				item.instance.gameObject.SetActive(value: false);
			}
		}
	}

	public void ExecActivationAction(int itemIdx, Item.ActivationAction activationAction)
	{
		Item item = GetItem(itemIdx);
		if (item != null)
		{
			ExecActivationAction(item.instance, activationAction);
		}
	}

	public static void ExecActivationAction(Containee targetContainee, bool flag)
	{
		targetContainee.container.ExecActivationAction(targetContainee, flag ? Item.ActivationAction.Activate : Item.ActivationAction.Deactivate);
	}

	public void ExecActivationAction(Containee targetContainee, Item.ActivationAction activationAction)
	{
		int itemIdx = GetItemIdx(targetContainee);
		if (itemIdx == -1)
		{
			logger.ErrorFormat("Containee missing from container: {0}, {1}", targetContainee.name, base.gameObject.name);
			return;
		}
		Item item = items[itemIdx];
		Containee.State targetState = targetContainee.targetState;
		targetState = ((targetState != Containee.State.None) ? targetState : targetContainee.currentState);
		Containee.State state = targetState;
		switch (activationAction)
		{
		case Item.ActivationAction.Activate:
			state = Containee.State.Opened;
			break;
		case Item.ActivationAction.Deactivate:
			state = Containee.State.Closed;
			break;
		case Item.ActivationAction.Toggle:
			state = ((targetState != Containee.State.Opened) ? Containee.State.Opened : Containee.State.Closed);
			break;
		}
		List<Item> list = CollectionPool.RequestList<Item>();
		List<Item> list2 = CollectionPool.RequestList<Item>();
		foreach (Item item2 in items)
		{
			if (item2.instance == null)
			{
				continue;
			}
			if (item2 == item)
			{
				Containee.State state2 = state;
				item2.instance.currentState = state2;
				item2.instance.targetState = Containee.State.None;
				switch (state2)
				{
				case Containee.State.Opened:
					list2.Add(item2);
					break;
				default:
					list.Add(item2);
					break;
				case Containee.State.None:
					break;
				}
			}
			else if (state == Containee.State.Opened)
			{
				if (item2.group == Item.Group.None || item2.group != item.group)
				{
					continue;
				}
				if (item2.instance.interpolatorTargetable != null)
				{
					item2.instance.interpolatorTargetable.PlayTo(StateNameMap[Containee.State.Closed]);
					continue;
				}
				item2.instance.currentState = Containee.State.Closed;
				item2.instance.targetState = Containee.State.None;
				if (false)
				{
					list2.Add(item2);
				}
				else
				{
					list.Add(item2);
				}
			}
			else if (item2.group != Item.Group.None && item2.group == item.group && item2.isDefaultInGroup && list2.Count == 0)
			{
				list.Remove(item2);
				list2.Add(item2);
			}
		}
		DoSetActive(list, list2);
		CollectionPool.Release(ref list);
		CollectionPool.Release(ref list2);
	}

	public static void DoSetActive(List<Item> closeItems, List<Item> openItems)
	{
		foreach (Item closeItem in closeItems)
		{
			closeItem.instance.gameObject.SetActive(value: false);
		}
		foreach (Item openItem in openItems)
		{
			openItem.instance.gameObject.SetActive(value: true);
		}
	}

	public static void RunActions(List<Item.ActionItem> actionItems)
	{
		foreach (Item.ActionItem actionItem in actionItems)
		{
			RunAction(actionItem);
		}
	}

	public static void RunAction(Item.ActionItem actionItem)
	{
		switch (actionItem.action)
		{
		case Item.ActionItem.Action.Open:
			if (actionItem.target.interpolatorTargetable != null)
			{
				actionItem.target.interpolatorTargetable.PlayTo("Opened");
			}
			else
			{
				actionItem.target.EnableActivation();
			}
			break;
		case Item.ActionItem.Action.Close:
			if (actionItem.target.interpolatorTargetable != null)
			{
				actionItem.target.interpolatorTargetable.PlayTo("Closed");
			}
			else
			{
				actionItem.target.DisableActivation();
			}
			break;
		case Item.ActionItem.Action.Toggle:
			if (!(actionItem.target.interpolatorTargetable != null))
			{
				actionItem.target.ToggleActivation();
			}
			break;
		}
	}

	public void Select(Item targetItem)
	{
		foreach (Item item in items)
		{
			if (item != targetItem)
			{
				continue;
			}
			{
				foreach (Item.ActionItem actionItem in targetItem.actionItems)
				{
					if (actionItem.target == null)
					{
						logger.WarnFormat("Target missing from actionItem: {0}, {1}", item.instance.name, base.gameObject.name);
						continue;
					}
					RunAction(actionItem);
					foreach (Action item2 in actionItem.target.actionsOnSelect)
					{
						item2?.Invoke();
					}
				}
				break;
			}
		}
	}

	public void Select(Containee targetContainee)
	{
		int itemIdx = GetItemIdx(targetContainee);
		if (itemIdx == -1)
		{
			logger.ErrorFormat("Containee missing from container: {0}, {1}", targetContainee.name, base.gameObject.name);
			return;
		}
		Item targetItem = items[itemIdx];
		Select(targetItem);
	}

	public void Hover(Containee targetContainee, bool flag)
	{
		int itemIdx = GetItemIdx(targetContainee);
		if (itemIdx == -1)
		{
			logger.ErrorFormat("Containee missing from container: {0}, {1}", targetContainee.name, base.gameObject.name);
		}
		else
		{
			ItemHover(itemIdx, flag);
		}
	}

	public void Click(Containee targetContainee)
	{
		int itemIdx = GetItemIdx(targetContainee);
		if (itemIdx == -1)
		{
			logger.ErrorFormat("Containee missing from container: {0}, {1}", targetContainee.name, base.gameObject.name);
		}
		else
		{
			ItemClick(itemIdx);
		}
	}

	public void AltClick(Containee targetContainee)
	{
		int itemIdx = GetItemIdx(targetContainee);
		if (itemIdx == -1)
		{
			logger.ErrorFormat("Containee missing from container: {0}, {1}", targetContainee.name, base.gameObject.name);
		}
		else
		{
			ItemAltClick(itemIdx);
		}
	}

	public void Toggle(Containee targetContainee, bool flag)
	{
		int itemIdx = GetItemIdx(targetContainee);
		if (itemIdx == -1)
		{
			logger.ErrorFormat("Containee missing from container: {0}, {1}", targetContainee.name, base.gameObject.name);
		}
		else
		{
			ItemToggle(itemIdx, flag);
		}
	}

	public void Scroll(Containee targetContainee, float value)
	{
		int itemIdx = GetItemIdx(targetContainee);
		if (itemIdx == -1)
		{
			logger.ErrorFormat("Containee missing from container: {0}, {1}", targetContainee.name, base.gameObject.name);
		}
		else
		{
			ItemScroll(itemIdx, value);
		}
	}

	public void ScrollEnd(Containee targetContainee, float value)
	{
		int itemIdx = GetItemIdx(targetContainee);
		if (itemIdx == -1)
		{
			logger.ErrorFormat("Containee missing from container: {0}, {1}", targetContainee.name, base.gameObject.name);
		}
		else
		{
			ItemScrollEnd(itemIdx, value);
		}
	}

	public void Text(Containee targetContainee, string text)
	{
		int itemIdx = GetItemIdx(targetContainee);
		if (itemIdx == -1)
		{
			logger.ErrorFormat("Containee missing from container: {0}, {1}", targetContainee.name, base.gameObject.name);
		}
		else
		{
			ItemText(itemIdx, text);
		}
	}

	public void ItemHover(int itemIdx, bool flag)
	{
		OnItemHoverSignal.Invoke(itemIdx, flag);
	}

	public void ItemClick(int itemIdx)
	{
		OnItemClickSignal.Invoke(itemIdx);
	}

	public void ItemAltClick(int itemIdx)
	{
		OnItemAltClickSignal.Invoke(itemIdx);
	}

	public void ItemToggle(int idx, bool flag)
	{
		OnItemToggleSignal.Invoke(idx, flag);
	}

	public void ItemScroll(int idx, float value)
	{
		OnItemScrollSignal.Invoke(idx, value);
	}

	public void ItemScrollEnd(int idx, float value)
	{
		OnItemScrollEndSignal.Invoke(idx, value);
	}

	public void ItemText(int idx, string text)
	{
		OnItemTextSignal.Invoke(idx, text);
	}

	private void OnEnable()
	{
		Connect();
		Populate(isChain: false);
		OnAfterEnableSignal.Invoke();
	}

	private void OnDisable()
	{
		Disconnect();
	}

	private void Connect()
	{
		_sgs.Add(GameSignals.Connect(CloseSaveGameMenuSignal, OnCloseSaveGameMenu, this));
		_sgs.Add(GameSignals.Connect(ReOpenOptionsMenuSignal, OnReOpenOptionsMenu, this));
	}

	private void Disconnect()
	{
		foreach (SignalConnection sg in _sgs)
		{
			sg.Disconnect();
		}
		_sgs.Clear();
	}
}
