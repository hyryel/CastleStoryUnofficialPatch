using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Input;
using Brix.NewUI.UserSettings_;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Brix.Utils.Logging;
using Rewired;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Container))]
public class ContainerBindingsPopulator : LegacyBaseMenu
{
	public BindingsProvider bindingsProvider;

	private static BrixLogger logger = new BrixLogger(typeof(ContainerBindingsPopulator).FullName);

	public Container selectionsContainer;

	public Containee contextPrefabContainee;

	public Containee userActionPrefabContainee;

	public Containee keySetPrefabContainee;

	public Containee keySetPrefabContaineeSub;

	public Scrollbar scrollBar;

	public ScrollRect scrollRect;

	public Containee newKeySetPrefabContainee;

	public Color DefaultColor;

	public Color DisabledColor;

	private static bool defaultBindsLoaded = false;

	public Operation TargetOperation;

	private List<Container.Item> menuItems = new List<Container.Item>();

	private static readonly string[] TmpEmptyStringArray = new string[0];

	private List<string> contextActionNames = new List<string>();

	private List<int> actionIndices = new List<int>();

	public bool isLeftAlt;

	public bool isRightAlt;

	public bool isLeftCtrl;

	public bool isRightCtrl;

	public bool isLeftShift;

	public bool isRightShift;

	private bool _needsSave;

	private int captureIdx = -1;

	private bool captureIsAlt;

	private bool captureReassigned;

	private bool capturePausedByButton;

	private bool capturePausedByKeys;

	private List<string> upBindItemEntries = new List<string>();

	private List<string> downBindItemEntries = new List<string>();

	private List<string> captureBindItemEntries = new List<string>();

	public bool skipMouseCapture = true;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<ContainerBindingsPopulator> OnAfterContainerEnable = delegate(ContainerBindingsPopulator menu)
	{
		menu.RefreshMenuItems();
	};

	public static List<string> ActionCategories = new List<string>
	{
		"Camera", "General", "tb_constr_menu", "Bricktron", "TaskMenu", "Projects", "BlocksMenu", "StructuresMenu", "Blueprints", "Crafting",
		"we_menu", "we_terrain_menu", "we_markers_menu"
	};

	private static readonly Dictionary<string, string> LocalizedBindMap = new Dictionary<string, string>
	{
		{ "Space", "##mainmenu_options_key_space" },
		{ "Left Shift", "##mainmenu_options_key_leftshift" },
		{ "Right Shift", "##mainmenu_options_key_rightshift" },
		{ "Left Alt", "##mainmenu_options_key_leftalt" },
		{ "Right Alt", "##mainmenu_options_key_rightalt" },
		{ "Left Control", "##mainmenu_options_key_leftctrl" },
		{ "Right Control", "##mainmenu_options_key_rightctrl" },
		{ "Left Arrow", "##mainmenu_options_key_leftarrow" },
		{ "Right Arrow", "##mainmenu_options_key_rightarrow" },
		{ "Up Arrow", "##mainmenu_options_key_uparrow" },
		{ "Down Arrow", "##mainmenu_options_key_downarrow" },
		{ "Delete", "##mainmenu_options_key_delete" },
		{ "Insert", "##mainmenu_options_key_insert" },
		{ "Home", "##mainmenu_options_key_home" },
		{ "End", "##mainmenu_options_key_end" },
		{ "None", "##mainmenu_options_assignkeybind_none" }
	};

	private static readonly Action<int, ContainerBindingsPopulator> OnItemClick_Selections = delegate(int idx, ContainerBindingsPopulator menu)
	{
		if (BindingsProvider.IsActionAssignable(menu.contextActionNames[idx]))
		{
			menu.bindingsProvider.targetActionName = menu.contextActionNames[idx];
			menu.bindingsProvider.capturePausedByButton = false;
			menu.bindingsProvider.captureReassigned = false;
			menu.bindingsProvider.captureIsAlt = false;
			menu.bindingsProvider.DisplayOkDialogForCapture();
		}
	};

	private static readonly Action<int, ContainerBindingsPopulator> OnItemAltClick = delegate(int idx, ContainerBindingsPopulator menu)
	{
		if (BindingsProvider.IsActionAssignable(menu.contextActionNames[idx]))
		{
			menu.bindingsProvider.targetActionName = menu.contextActionNames[idx];
			menu.bindingsProvider.capturePausedByButton = false;
			menu.bindingsProvider.captureReassigned = false;
			menu.bindingsProvider.captureIsAlt = true;
			menu.bindingsProvider.DisplayOkDialogForCapture();
		}
	};

	private bool NeedsSave
	{
		get
		{
			return _needsSave;
		}
		set
		{
			_needsSave = value;
			if (_needsSave)
			{
				UserSettingManager.TempSettings.SetDirty();
			}
		}
	}

	private static KeyboardMap CameraKeyboardMap => PickingUtility.Player.controllers.maps.GetMap<KeyboardMap>(0, "Camera", "QWERTY");

	private static KeyboardMap BricktronKeyboardMap => PickingUtility.Player.controllers.maps.GetMap<KeyboardMap>(0, "Bricktron", "QWERTY");

	private static KeyboardMap ProjectsKeyboardMap => PickingUtility.Player.controllers.maps.GetMap<KeyboardMap>(0, "Projects", "QWERTY");

	private static KeyboardMap BlueprintsKeyboardMap => PickingUtility.Player.controllers.maps.GetMap<KeyboardMap>(0, "tb_constr_blueprints_menu", "QWERTY");

	private static KeyboardMap WorldEditorMenuKeyboardMap => PickingUtility.Player.controllers.maps.GetMap<KeyboardMap>(0, "we_menu", "QWERTY");

	private static KeyboardMap WorldEditorTerrainMenuKeyboardMap => PickingUtility.Player.controllers.maps.GetMap<KeyboardMap>(0, "we_terrain_menu", "QWERTY");

	private static KeyboardMap WorldEditorMarkersMenuKeyboardMap => PickingUtility.Player.controllers.maps.GetMap<KeyboardMap>(0, "we_markers_menu", "QWERTY");

	private static MouseMap CameraMouseMap => PickingUtility.Player.controllers.maps.GetMap<MouseMap>(0, "Camera", "Camera");

	private static MouseMap BricktronMouseMap => PickingUtility.Player.controllers.maps.GetMap<MouseMap>(0, "Bricktron", "Camera");

	private static MouseMap ProjectsMouseMap => null;

	private List<Container.Item> GetMenuItems()
	{
		menuItems.Clear();
		contextActionNames.Clear();
		if (BindingsProvider.OperationKeyContextMap.ContainsKey(TargetOperation))
		{
			List<BindingsProvider.KeyContext> list = BindingsProvider.OperationKeyContextMap[TargetOperation];
			foreach (BindingsProvider.KeyContext item2 in list)
			{
				foreach (string item3 in BindingsProvider.keyContextCategoryNameMap[item2])
				{
					(from j in ReInput.mapping.ActionsInCategory(item3).Slinq()
						select j.name).AddTo(contextActionNames);
				}
			}
		}
		Dictionary<string, List<BindingsProvider.BindItem>> mergedBindItemMap = BindingsProvider.mergedBindItemMap;
		foreach (string key in mergedBindItemMap.Keys)
		{
			Container.Item item = new Container.Item();
			if (contextActionNames.Contains(key))
			{
				string[] array = (from i in mergedBindItemMap[key][0]._entries.Slinq()
					select i.ToPrettyEntryString()).ToArray();
				string text = ((array.Length != 0) ? string.Join(" + ", array) : string.Empty);
				text = ((!(text == string.Empty)) ? text : ("<i>" + I2Helper.TryGet("##mainmenu_options_assignkeybind_none") + "</i>"));
				string[] array2 = ((mergedBindItemMap[key].Count != 1) ? (from i in mergedBindItemMap[key][1]._entries.Slinq()
					select i.ToPrettyEntryString()).ToArray() : TmpEmptyStringArray);
				string text2 = ((array2.Length != 0) ? string.Join(" + ", array2) : string.Empty);
				text2 = ((!(text2 == string.Empty)) ? text2 : ("<i>" + I2Helper.TryGet("##mainmenu_options_assignkeybind_none") + "</i>"));
				item.original = newKeySetPrefabContainee;
				item.currentState = Containee.State.Opened;
				string text3 = ((!key.Contains("bricktron_")) ? string.Empty : "Bricktron: ");
				InputAction action = ReInput.mapping.GetAction(ToRewiredActionId(key));
				if (action == null)
				{
					Debug.Log(key + " not found");
				}
				string text4 = ((action != null) ? action.descriptiveName : string.Empty);
				item.titleText = (text3 + I2Helper.TryGet(text4)).ToUpper();
				item.localizeTitle = item.titleText;
				item.subtitleText = ToLocalizedBind(text);
				item.subsubtitleText = ToLocalizedBind(text2);
				string text5 = text4 + "_info";
				string text6 = I2Helper.TryGet(text5);
				if (text6.Contains(text5) || text6 == text5)
				{
					text6 = string.Empty;
				}
				item.subsubsubtitleText = text6.ToUpper();
				item.localizeSubtitle = item.subtitleText;
				item.instance = null;
				item.parentTrs = null;
				item.imageTexture = null;
				item.color = new Color(0f, 0f, 0f, 0f);
				item.bgColor = new Color(0f, 0f, 0f, 0f);
				item.idxOffset = 0;
				item.group = Container.Item.Group.None;
				item.isDynamic = true;
				menuItems.Add(item);
			}
		}
		return menuItems;
	}

	public void BuildMenuItems()
	{
		menuItems = GetMenuItems();
	}

	public void BuildAndPopulateMenuItems()
	{
		BuildMenuItems();
		if (selectionsContainer != null)
		{
			selectionsContainer.Populate(isChain: false, purgeOnly: true);
			selectionsContainer.items.Clear();
			selectionsContainer.items.AddRange(menuItems);
			selectionsContainer.Populate(isChain: false);
		}
		RefreshMenuItems();
		AutoActivateScrollBar();
	}

	private void RefreshMenuItems()
	{
		for (int i = 0; i < menuItems.Count; i++)
		{
			Container.Item item = menuItems[i];
			IPlug plug = item.instance as IPlug;
			string actionName = contextActionNames[i];
			Text plugComponent = plug.GetPlugComponent<Text>("Primary Binding");
			Text plugComponent2 = plug.GetPlugComponent<Text>("Alternate Binding");
			Color color = (plugComponent.color = ((!BindingsProvider.IsActionAssignable(actionName)) ? DisabledColor : DefaultColor));
			plugComponent2.color = color;
		}
	}

	private void OnEnable()
	{
		Connect();
		BuildAndPopulateMenuItems();
		NeedsSave = false;
	}

	private void OnDisable()
	{
		Disconnect();
	}

	private void Update()
	{
		AutoActivateScrollBar();
	}

	public void Connect()
	{
		if (selectionsContainer != null)
		{
			_sgs.Add(selectionsContainer.OnItemAltClickSignal.Connect(OnItemAltClick, this));
		}
		if (selectionsContainer != null)
		{
			_sgs.Add(selectionsContainer.OnItemClickSignal.Connect(OnItemClick_Selections, this));
		}
		if (selectionsContainer != null)
		{
			_sgs.Add(selectionsContainer.OnAfterEnableSignal.Connect(OnAfterContainerEnable, this));
		}
	}

	public void Disconnect()
	{
		foreach (SignalConnection sg in _sgs)
		{
			sg.Disconnect();
		}
		_sgs.Clear();
	}

	private void AutoActivateScrollBar()
	{
		if (scrollBar.gameObject != null && selectionsContainer != null)
		{
			RectTransform rectTransform = selectionsContainer.transform as RectTransform;
			bool flag = scrollBar.direction == Scrollbar.Direction.LeftToRight || scrollBar.direction == Scrollbar.Direction.RightToLeft;
			scrollBar.gameObject.SetActive(((!flag) ? rectTransform.sizeDelta.y : rectTransform.sizeDelta.x) > 0f);
		}
	}

	public static int ToRewiredActionId(string actionName)
	{
		foreach (InputAction action in ReInput.mapping.Actions)
		{
			if (actionName == action.name)
			{
				return action.id;
			}
		}
		logger.Warn("Missing action string in Rewired mapping\n");
		return -1;
	}

	private static ModifierKeyFlags ToRewiredModifier(KeyCode keyCode)
	{
		return keyCode switch
		{
			KeyCode.LeftAlt => ModifierKeyFlags.LeftAlt, 
			KeyCode.RightAlt => ModifierKeyFlags.RightAlt, 
			KeyCode.LeftControl => ModifierKeyFlags.LeftControl, 
			KeyCode.RightControl => ModifierKeyFlags.RightControl, 
			KeyCode.LeftShift => ModifierKeyFlags.LeftShift, 
			KeyCode.RightShift => ModifierKeyFlags.RightShift, 
			KeyCode.LeftCommand => ModifierKeyFlags.LeftCommand, 
			KeyCode.RightCommand => ModifierKeyFlags.RightCommand, 
			_ => ModifierKeyFlags.None, 
		};
	}

	private static string ToLocalizedBind(string bindString)
	{
		foreach (string key in LocalizedBindMap.Keys)
		{
			if (bindString.Contains(key))
			{
				bindString = bindString.Replace(key, I2Helper.TryGet(LocalizedBindMap[key]));
			}
		}
		return bindString;
	}
}
