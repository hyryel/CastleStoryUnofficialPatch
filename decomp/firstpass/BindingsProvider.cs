using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Input;
using Brix.Lua;
using Brix.NewUI.UserSettings_;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Rewired;
using Smooth.Slinq;
using UnityEngine;

public class BindingsProvider : LegacyBaseMenu
{
	public class BindItem
	{
		public abstract class Entry
		{
			public string _;

			private static readonly char[] CompositeSplitCharArray = new char[1] { CompositeSplitChar };

			public static char InputEventSplitChar => '|';

			public static char CompositeSplitChar => '+';

			public abstract string ToEntryString(bool includeInputEvent);

			public abstract string ToPrettyEntryString();

			public static List<Entry> FromCompositeEntryString(string compositeEntryString)
			{
				List<Entry> list = new List<Entry>();
				if (compositeEntryString != string.Empty)
				{
					if (compositeEntryString.Contains(CompositeSplitChar.ToString()))
					{
						list.AddRange(compositeEntryString.Split(CompositeSplitCharArray).Slinq().Select(FromEntryString)
							.ToList());
					}
					else
					{
						list.Add(FromEntryString(compositeEntryString));
					}
				}
				return list;
			}

			public static List<Entry> FromRewiredMouse(string compositeEntryString)
			{
				List<Entry> list = new List<Entry>();
				if (compositeEntryString != string.Empty)
				{
					switch (compositeEntryString)
					{
					case "Left Mouse Button":
						compositeEntryString = "Mouse0";
						break;
					case "Right Mouse Button":
						compositeEntryString = "Mouse1";
						break;
					case "Mouse Button 3":
						compositeEntryString = "Mouse2";
						break;
					case "Mouse Button 4":
						compositeEntryString = "Mouse3";
						break;
					case "Mouse Button 5":
						compositeEntryString = "Mouse4";
						break;
					case "Mouse Button 6":
						compositeEntryString = "Mouse5";
						break;
					case "Mouse Button 7":
						compositeEntryString = "Mouse6";
						break;
					}
					list.Add(FromEntryString(compositeEntryString));
				}
				return list;
			}

			public static Entry FromEntryString(string entryString)
			{
				entryString = entryString.Trim();
				if (entryString == "Shift")
				{
					entryString = "LeftShift";
				}
				if (entryString == "Alt")
				{
					entryString = "LeftAlt";
				}
				if (entryString == "Control")
				{
					entryString = "LeftControl";
				}
				string text = entryString;
				string text2 = string.Empty;
				string[] array = entryString.Split(InputEventSplitChar);
				if (array.Length == 2)
				{
					text = array[0];
					text2 = array[1];
				}
				else if (array.Length == 1)
				{
					text = array[0];
				}
				if (text.Contains(MouseWheelEntry.PrefixMatchString))
				{
					return new MouseWheelEntry(MouseWheelEntry.GetWheelDirection(text));
				}
				if (text.Contains(MouseButtonEntry.PrefixMatchString))
				{
					if (text2 == string.Empty)
					{
						text2 = MouseButtonEntry.GetDefaultInputEvent().ToString();
					}
					return new MouseButtonEntry(MouseButtonEntry.GetMouseButtonId(text), InputEventEntry.GetInputEvent(text2));
				}
				KeyCode keyCode = KeyboardEntry.GetKeyCode(text);
				if (text2 == string.Empty)
				{
					text2 = KeyboardEntry.GetDefaultInputEvent(keyCode).ToString();
				}
				return new KeyboardEntry(keyCode, InputEventEntry.GetInputEvent(text2));
			}
		}

		public class MouseWheelEntry : Entry
		{
			public enum WheelDirection
			{
				None,
				Up,
				Down
			}

			public WheelDirection _wheelDirection;

			public static string PrefixMatchString => I2Helper.TryGet("##mainmenu_options_key_mw");

			public MouseWheelEntry(WheelDirection wheelDirection)
			{
				_wheelDirection = wheelDirection;
			}

			public static string GetMouseWheelPrettyString(WheelDirection wheelDirection)
			{
				return wheelDirection switch
				{
					WheelDirection.Up => I2Helper.TryGet("##mainmenu_options_key_mwu"), 
					WheelDirection.Down => I2Helper.TryGet("##mainmenu_options_key_mwd"), 
					_ => string.Empty, 
				};
			}

			public override string ToEntryString(bool includeInputEvent)
			{
				return PrefixMatchString + _wheelDirection;
			}

			public override string ToPrettyEntryString()
			{
				return GetMouseWheelPrettyString(_wheelDirection);
			}

			public static WheelDirection GetWheelDirection(string wheelDirectionString)
			{
				wheelDirectionString = wheelDirectionString.Replace(PrefixMatchString, string.Empty);
				return (WheelDirection)Enum.Parse(typeof(WheelDirection), wheelDirectionString);
			}
		}

		public abstract class InputEventEntry : Entry
		{
			public enum InputEvent
			{
				None,
				Press,
				Hold,
				Release
			}

			public InputEvent _inputEvent;

			protected InputEventEntry(InputEvent inputEvent)
			{
				_inputEvent = inputEvent;
			}

			public static InputEvent GetInputEvent(string inputEventString)
			{
				return (InputEvent)Enum.Parse(typeof(InputEvent), inputEventString);
			}
		}

		public class MouseButtonEntry : InputEventEntry
		{
			public int _mouseButtonId = -1;

			public static string PrefixMatchString => "Mouse";

			public MouseButtonEntry(int mouseButtonId, InputEvent inputEvent)
				: base(inputEvent)
			{
				_mouseButtonId = mouseButtonId;
			}

			public static string GetMouseButtonPrettyString(int mouseButtonId)
			{
				return mouseButtonId switch
				{
					0 => I2Helper.TryGet("##mainmenu_options_key_lmb"), 
					1 => I2Helper.TryGet("##mainmenu_options_key_rmb"), 
					2 => I2Helper.TryGet("##mainmenu_options_key_mmb"), 
					_ => I2Helper.TryGet("##mainmenu_options_key_xxxmb").Replace("#xxx", mouseButtonId.ToString()), 
				};
			}

			public static string GetMouseButtonString(int mouseButtonId)
			{
				return PrefixMatchString + mouseButtonId;
			}

			public static int GetMouseButtonId(string mouseButtonString)
			{
				if (mouseButtonString.Contains("Button"))
				{
					mouseButtonString = mouseButtonString.Replace("Button", string.Empty);
					if (mouseButtonString.Contains("Left"))
					{
						mouseButtonString = mouseButtonString.Replace("Left", string.Empty).Trim() + 0;
					}
					else if (mouseButtonString.Contains("Right"))
					{
						mouseButtonString = mouseButtonString.Replace("Right", string.Empty).Trim() + 1;
					}
					else if (mouseButtonString.Contains("Middle"))
					{
						mouseButtonString = mouseButtonString.Replace("Middle", string.Empty).Trim() + 2;
					}
				}
				mouseButtonString = mouseButtonString.Replace(PrefixMatchString, string.Empty);
				if (mouseButtonString == string.Empty)
				{
					return -1;
				}
				return int.Parse(mouseButtonString);
			}

			public override string ToEntryString(bool includeInputEvent)
			{
				string text = GetMouseButtonString(_mouseButtonId);
				if (includeInputEvent)
				{
					text = text + Entry.InputEventSplitChar + _inputEvent.ToString();
				}
				return text;
			}

			public override string ToPrettyEntryString()
			{
				return GetMouseButtonPrettyString(_mouseButtonId);
			}

			public static InputEvent GetDefaultInputEvent()
			{
				return InputEvent.Release;
			}
		}

		public class KeyboardEntry : InputEventEntry
		{
			public KeyCode _keyCode;

			public KeyboardEntry(KeyCode keyCode, InputEvent inputEvent)
				: base(inputEvent)
			{
				_keyCode = keyCode;
			}

			public static string GetKeyboardPrettyString(KeyCode keyCode)
			{
				string input = keyCode.ToString();
				return Regex.Replace(input, "[A-Z]", " $0").TrimStart();
			}

			public override string ToEntryString(bool includeInputEvent)
			{
				string text = _keyCode.ToString();
				if (includeInputEvent)
				{
					text = text + Entry.InputEventSplitChar.ToString() + _inputEvent;
				}
				return text;
			}

			public override string ToPrettyEntryString()
			{
				return GetKeyboardPrettyString(_keyCode);
			}

			public static KeyCode GetKeyCode(string keyCodeString)
			{
				keyCodeString = keyCodeString.Replace(" ", string.Empty);
				return (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeString);
			}

			public static InputEvent GetDefaultInputEvent(KeyCode keyCode)
			{
				if (keyCode == KeyCode.LeftControl || keyCode == KeyCode.RightControl || keyCode == KeyCode.LeftAlt || keyCode == KeyCode.RightAlt || keyCode == KeyCode.LeftShift || keyCode == KeyCode.RightShift || keyCode == KeyCode.LeftCommand || keyCode == KeyCode.RightCommand || keyCode == KeyCode.LeftCommand || keyCode == KeyCode.RightCommand)
				{
					return InputEvent.Hold;
				}
				return InputEvent.Release;
			}
		}

		public string _trigger;

		public List<string> keys;

		public List<string> ctrls;

		public bool hovered;

		public bool hasChanged;

		public List<KeyCode> keyCodes;

		public List<KeyCode> savedKeyCodes;

		public int _rewiredElementIdentifierId;

		public bool _userAssignable;

		public List<Entry> _entries;

		public BindItem(BindItem other)
		{
			_trigger = other._trigger;
			_entries = other._entries;
		}

		public BindItem(string trigger)
		{
			_trigger = trigger;
			_entries = new List<Entry>();
		}

		public BindItem(string trigger, List<Entry> entries, int rewiredElementIdentifierId = -1, bool userAssignable = true)
		{
			_trigger = trigger;
			_entries = entries;
			_rewiredElementIdentifierId = rewiredElementIdentifierId;
			_userAssignable = userAssignable;
		}

		public BindItem(string trigger, List<string> keys, List<string> ctrls)
		{
			_trigger = trigger;
			this.keys = keys;
			this.ctrls = ctrls;
		}

		public static bool HasAnyChanged(List<BindItem> bindItems)
		{
			return bindItems.Slinq().Any((BindItem bindItem) => bindItem.keyCodes.Slinq().Intersect(bindItem.savedKeyCodes.Slinq()).Count() != bindItem.savedKeyCodes.Count);
		}
	}

	public enum KeyContext
	{
		None,
		Global,
		Bricktron,
		TaskPanel,
		BlocksPanel,
		StructuresPanel,
		Tasks,
		Crafting,
		Blueprint,
		WorldEditor,
		WorldEditor_Terrain,
		WorldEditor_Markers
	}

	private class ConflictReport
	{
		public class Conflict
		{
			public KeyContext keyContext;

			public string actionName;

			public string actionDisplayName;

			public bool isAlt;

			public string menuDisplayName;
		}

		public List<Conflict> conflicts = new List<Conflict>();

		public bool Success => conflicts.Count == 0;

		private static void GetBindingNames(string _actionName, out string _mainBinding, out string _altBinding)
		{
			string[] array = (from i in mergedBindItemMap[_actionName][0]._entries.Slinq()
				select i.ToPrettyEntryString()).ToArray();
			_mainBinding = ((array.Length != 0) ? string.Join(" + ", array) : string.Empty);
			string[] array2 = ((mergedBindItemMap[_actionName].Count != 1) ? (from i in mergedBindItemMap[_actionName][1]._entries.Slinq()
				select i.ToPrettyEntryString()).ToArray() : TmpEmptyStringArray);
			_altBinding = ((array2.Length != 0) ? string.Join(" + ", array2) : string.Empty);
		}

		private static string GetMenuName(KeyContext _keyContext)
		{
			foreach (Operation key in OperationKeyContextMap.Keys)
			{
				foreach (KeyContext item in OperationKeyContextMap[key])
				{
					if (item == _keyContext)
					{
						return OperationNameMap[key];
					}
				}
			}
			return null;
		}

		private static bool ConflictIsException(string _currentAction, string _otherAction, KeyContext _currentKeyContext, KeyContext _otherKeyContext)
		{
			switch (_currentAction)
			{
			case "command_BlocksMenu":
				if (_otherKeyContext == KeyContext.BlocksPanel)
				{
					return true;
				}
				break;
			case "command_StructuresMenu":
				if (_otherKeyContext == KeyContext.StructuresPanel)
				{
					return true;
				}
				break;
			case "command_CreateProject":
				if (_otherKeyContext == KeyContext.TaskPanel)
				{
					return true;
				}
				break;
			default:
				switch (_otherAction)
				{
				case "command_BlocksMenu":
					if (_currentKeyContext == KeyContext.BlocksPanel)
					{
						return true;
					}
					break;
				case "command_StructuresMenu":
					if (_currentKeyContext == KeyContext.StructuresPanel)
					{
						return true;
					}
					break;
				case "command_CreateProject":
					if (_currentKeyContext == KeyContext.TaskPanel)
					{
						return true;
					}
					break;
				}
				break;
			}
			return false;
		}

		public static ConflictReport Generate(KeyContext _keyContext, string _binding, string _actionName)
		{
			ConflictReport conflictReport = new ConflictReport();
			foreach (KeyContext item in keyContextConflictMap[_keyContext])
			{
				foreach (string item2 in keyContextCategoryNameMap[item])
				{
					foreach (InputAction item3 in ReInput.mapping.ActionsInCategory(item2))
					{
						if (item3.name == _actionName)
						{
							continue;
						}
						GetBindingNames(item3.name, out var _mainBinding, out var _altBinding);
						string menuName = GetMenuName(item);
						if (!_binding.ToUpper().Contains("NONE") && _binding != string.Empty)
						{
							if (_binding == _mainBinding && !ConflictIsException(_actionName, item3.name, _keyContext, item))
							{
								conflictReport.conflicts.Add(new Conflict
								{
									keyContext = item,
									actionName = item3.name,
									menuDisplayName = menuName,
									actionDisplayName = item3.descriptiveName
								});
							}
							if (_binding == _altBinding && !ConflictIsException(_actionName, item3.name, _keyContext, item))
							{
								conflictReport.conflicts.Add(new Conflict
								{
									keyContext = item,
									actionName = item3.name,
									menuDisplayName = menuName,
									actionDisplayName = item3.descriptiveName,
									isAlt = true
								});
							}
						}
					}
				}
			}
			return conflictReport;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(BindingsProvider).FullName);

	public List<ContainerBindingsPopulator> populators = new List<ContainerBindingsPopulator>();

	public Container operationsContainer;

	public static GameSignals.GameSignal<Operation> OperateSignal = new GameSignals.GameSignal<Operation>();

	private static readonly Action<Operation, BindingsProvider> OnOperate = delegate(Operation operation, BindingsProvider menu)
	{
		switch (operation)
		{
		case Operation.Apply:
			menu.SaveLuaFile();
			Reload();
			menu.NeedsSave = false;
			break;
		case Operation.Reset:
			Reload();
			menu.NeedsSave = false;
			menu.BuildAndPopulateMenuItems();
			break;
		case Operation.ResetKeysToDefault:
			Reload(useDefault: true);
			menu.SaveLuaFile();
			menu.NeedsSave = false;
			menu.BuildAndPopulateMenuItems();
			break;
		}
	};

	private static bool defaultBindsLoaded = false;

	private static Dictionary<string, List<BindItem>> defaultBindItemMap = new Dictionary<string, List<BindItem>>();

	public static Dictionary<string, List<BindItem>> mergedBindItemMap = new Dictionary<string, List<BindItem>>();

	private static Dictionary<string, List<BindItem>> userBindItemMap = new Dictionary<string, List<BindItem>>();

	public Operation TargetOperation;

	public static Dictionary<Operation, List<KeyContext>> OperationKeyContextMap = new Dictionary<Operation, List<KeyContext>>(FastEnumIntEqualityComparer<Operation>.Instance)
	{
		{
			Operation.Keybindings_Global,
			new List<KeyContext>
			{
				KeyContext.Global,
				KeyContext.Bricktron
			}
		},
		{
			Operation.Keybindings_BlocksPanel,
			new List<KeyContext> { KeyContext.BlocksPanel }
		},
		{
			Operation.Keybindings_StructuresPanel,
			new List<KeyContext> { KeyContext.StructuresPanel }
		},
		{
			Operation.Keybindings_TaskPanel,
			new List<KeyContext> { KeyContext.TaskPanel }
		},
		{
			Operation.Keybindings_Tasks,
			new List<KeyContext> { KeyContext.Tasks }
		},
		{
			Operation.Keybindings_Crafting,
			new List<KeyContext> { KeyContext.Crafting }
		},
		{
			Operation.Keybindings_Blueprint,
			new List<KeyContext> { KeyContext.Blueprint }
		},
		{
			Operation.Keybindings_WorldEditor,
			new List<KeyContext> { KeyContext.WorldEditor }
		}
	};

	public static Dictionary<Operation, string> OperationNameMap = new Dictionary<Operation, string>(FastEnumIntEqualityComparer<Operation>.Instance)
	{
		{
			Operation.Keybindings_Global,
			"##mainmenu_options_keybindings_command"
		},
		{
			Operation.Keybindings_BlocksPanel,
			"##mainmenu_options_keybindings_blocks"
		},
		{
			Operation.Keybindings_StructuresPanel,
			"##mainmenu_options_keybindings_structures"
		},
		{
			Operation.Keybindings_TaskPanel,
			"##mainmenu_options_keybindings_taskpanel"
		},
		{
			Operation.Keybindings_Tasks,
			"##mainmenu_options_keybindings_taskselected"
		},
		{
			Operation.Keybindings_Crafting,
			"##mainmenu_options_keybindings_crafting"
		},
		{
			Operation.Keybindings_Blueprint,
			"##mainmenu_options_keybindings_blueprint"
		},
		{
			Operation.Keybindings_WorldEditor,
			"##mainmenu_options_keybindings_worldeditor"
		}
	};

	public static Dictionary<KeyContext, List<string>> keyContextCategoryNameMap = new Dictionary<KeyContext, List<string>>(FastEnumIntEqualityComparer<KeyContext>.Instance)
	{
		{
			KeyContext.Global,
			new List<string>(new string[1] { "Camera" })
		},
		{
			KeyContext.Bricktron,
			new List<string>(new string[1] { "Bricktron" })
		},
		{
			KeyContext.BlocksPanel,
			new List<string>(new string[1] { "BlocksMenu" })
		},
		{
			KeyContext.StructuresPanel,
			new List<string>(new string[1] { "StructuresMenu" })
		},
		{
			KeyContext.TaskPanel,
			new List<string>(new string[1] { "TaskMenu" })
		},
		{
			KeyContext.Tasks,
			new List<string>(new string[1] { "Projects" })
		},
		{
			KeyContext.Crafting,
			new List<string>(new string[1] { "Crafting" })
		},
		{
			KeyContext.Blueprint,
			new List<string>(new string[1] { "Blueprints" })
		},
		{
			KeyContext.WorldEditor,
			new List<string>(new string[3] { "we_menu", "we_terrain_menu", "we_markers_menu" })
		}
	};

	private static Dictionary<KeyContext, List<KeyContext>> keyContextConflictMap = new Dictionary<KeyContext, List<KeyContext>>(FastEnumIntEqualityComparer<KeyContext>.Instance)
	{
		{
			KeyContext.Tasks,
			new List<KeyContext>
			{
				KeyContext.Tasks,
				KeyContext.Global,
				KeyContext.Blueprint
			}
		},
		{
			KeyContext.BlocksPanel,
			new List<KeyContext>
			{
				KeyContext.BlocksPanel,
				KeyContext.Global
			}
		},
		{
			KeyContext.StructuresPanel,
			new List<KeyContext>
			{
				KeyContext.StructuresPanel,
				KeyContext.Global
			}
		},
		{
			KeyContext.Blueprint,
			new List<KeyContext>
			{
				KeyContext.Blueprint,
				KeyContext.Global,
				KeyContext.Tasks
			}
		},
		{
			KeyContext.Crafting,
			new List<KeyContext>
			{
				KeyContext.Crafting,
				KeyContext.Global
			}
		},
		{
			KeyContext.TaskPanel,
			new List<KeyContext>
			{
				KeyContext.TaskPanel,
				KeyContext.Global
			}
		},
		{
			KeyContext.WorldEditor,
			new List<KeyContext>
			{
				KeyContext.WorldEditor,
				KeyContext.Global
			}
		},
		{
			KeyContext.Global,
			new List<KeyContext>
			{
				KeyContext.Global,
				KeyContext.BlocksPanel,
				KeyContext.StructuresPanel,
				KeyContext.TaskPanel,
				KeyContext.Tasks,
				KeyContext.Crafting,
				KeyContext.Blueprint,
				KeyContext.WorldEditor,
				KeyContext.Bricktron
			}
		},
		{
			KeyContext.Bricktron,
			new List<KeyContext>
			{
				KeyContext.Bricktron,
				KeyContext.Global
			}
		}
	};

	private List<Container.Item> menuItems = new List<Container.Item>();

	private List<BindItem> bindItems = new List<BindItem>();

	private List<BindItem> altBindItems = new List<BindItem>();

	private static readonly string[] TmpEmptyStringArray = new string[0];

	private List<string> captureActionNames = new List<string>();

	public bool isLeftAlt;

	public bool isRightAlt;

	public bool isLeftCtrl;

	public bool isRightCtrl;

	public bool isLeftShift;

	public bool isRightShift;

	private bool _needsSave;

	public string targetActionName;

	public bool captureIsAlt;

	public bool captureReassigned;

	public bool capturePausedByButton;

	public bool capturePausedByKeys;

	private List<string> upBindItemEntries = new List<string>();

	private List<string> downBindItemEntries = new List<string>();

	private List<string> captureBindItemEntries = new List<string>();

	public bool skipMouseCapture = true;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

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

	private static readonly Action<int, BindingsProvider> OnItemClick_Operations = delegate(int idx, BindingsProvider menu)
	{
		Container.Item item = menu.operationsContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		OnOperate(containerPopulatorOperation, menu);
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

	private static BindItem.InputEventEntry.InputEvent GetForcedInputEvent(BindItem.InputEventEntry.InputEvent inputEvent, string actionName)
	{
		if (actionName.Contains("cam_move_"))
		{
			inputEvent = BindItem.InputEventEntry.InputEvent.Hold;
		}
		else if (actionName.Contains("cam_toggle_"))
		{
			inputEvent = BindItem.InputEventEntry.InputEvent.Press;
		}
		else if (actionName.Contains("cam_zoom_"))
		{
			inputEvent = BindItem.InputEventEntry.InputEvent.Hold;
		}
		else if (actionName.Contains("cam_mouse_"))
		{
			inputEvent = BindItem.InputEventEntry.InputEvent.Hold;
		}
		else if (actionName.Contains("cam_rotate_"))
		{
			inputEvent = BindItem.InputEventEntry.InputEvent.Hold;
		}
		return inputEvent;
	}

	private static KeyContext GetKeyContext(string _actionName)
	{
		foreach (KeyContext key in keyContextCategoryNameMap.Keys)
		{
			foreach (string item in keyContextCategoryNameMap[key])
			{
				foreach (InputAction item2 in ReInput.mapping.ActionsInCategory(item))
				{
					if (item2.name == _actionName)
					{
						return key;
					}
				}
			}
		}
		return KeyContext.None;
	}

	public void BuildAndPopulateMenuItems()
	{
		foreach (ContainerBindingsPopulator populator in populators)
		{
			populator.BuildAndPopulateMenuItems();
		}
	}

	public static bool IsActionAssignable(string actionName)
	{
		foreach (string actionCategory in ActionCategories)
		{
			foreach (InputAction item in ReInput.mapping.ActionsInCategory(actionCategory))
			{
				if (item.name == actionName)
				{
					return item.userAssignable;
				}
			}
		}
		return false;
	}

	private void CaptureShift()
	{
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			Capture(new Event
			{
				keyCode = KeyCode.LeftShift,
				type = EventType.KeyDown
			});
		}
		else if (Input.GetKeyUp(KeyCode.LeftShift))
		{
			Capture(new Event
			{
				keyCode = KeyCode.LeftShift,
				type = EventType.KeyUp
			});
		}
		else if (Input.GetKeyDown(KeyCode.RightShift))
		{
			Capture(new Event
			{
				keyCode = KeyCode.RightShift,
				type = EventType.KeyDown
			});
		}
		else if (Input.GetKeyUp(KeyCode.RightShift))
		{
			Capture(new Event
			{
				keyCode = KeyCode.RightShift,
				type = EventType.KeyUp
			});
		}
	}

	private void Capture(Event currentEvent)
	{
		TryCaptureUnpauseByKeys();
		if (targetActionName == null || capturePausedByButton)
		{
			return;
		}
		EventType type = currentEvent.type;
		bool flag = type == EventType.KeyDown || type == EventType.KeyDown || type == EventType.KeyUp || type == EventType.KeyUp;
		bool flag2 = type == EventType.ScrollWheel || type == EventType.ScrollWheel;
		bool flag3 = type == EventType.MouseUp || type == EventType.MouseUp || type == EventType.MouseDown || type == EventType.MouseDown;
		if (flag)
		{
			if (currentEvent.keyCode == KeyCode.Delete)
			{
				downBindItemEntries.Clear();
				upBindItemEntries.Clear();
				captureBindItemEntries.Clear();
				captureReassigned = true;
				DisplayOkDialogForCapture();
				return;
			}
			bool flag4 = type == EventType.KeyDown || type == EventType.KeyDown;
			BindItem.KeyboardEntry keyboardEntry = new BindItem.KeyboardEntry(currentEvent.keyCode, flag4 ? BindItem.InputEventEntry.InputEvent.Press : BindItem.InputEventEntry.InputEvent.Release);
			string text = keyboardEntry.ToEntryString(includeInputEvent: false);
			if (text != "None")
			{
				if (flag4)
				{
					if (!downBindItemEntries.Contains(text) && !IsProhibited(text))
					{
						downBindItemEntries.Add(text);
						captureReassigned = true;
					}
				}
				else
				{
					if (downBindItemEntries.Contains(text))
					{
						downBindItemEntries.Remove(text);
					}
					TryCaptureSave(text);
				}
				DisplayOkDialogForCapture();
			}
		}
		else if (!skipMouseCapture)
		{
			if (flag2)
			{
				BindItem.MouseWheelEntry mouseWheelEntry = new BindItem.MouseWheelEntry((currentEvent.delta.y < 0f) ? BindItem.MouseWheelEntry.WheelDirection.Up : BindItem.MouseWheelEntry.WheelDirection.Down);
				string text2 = mouseWheelEntry.ToEntryString(includeInputEvent: false);
				if (text2 != "None")
				{
					TryCaptureSave(text2);
					DisplayOkDialogForCapture();
				}
			}
			else if (flag3)
			{
				bool flag5 = type == EventType.MouseDown || type == EventType.MouseDown;
				BindItem.MouseButtonEntry mouseButtonEntry = new BindItem.MouseButtonEntry(currentEvent.button, flag5 ? BindItem.InputEventEntry.InputEvent.Press : BindItem.InputEventEntry.InputEvent.Release);
				string text3 = mouseButtonEntry.ToEntryString(includeInputEvent: false);
				if (text3 != "None")
				{
					if (flag5)
					{
						if (!downBindItemEntries.Contains(text3))
						{
							downBindItemEntries.Add(text3);
							captureReassigned = true;
						}
					}
					else
					{
						if (downBindItemEntries.Contains(text3))
						{
							downBindItemEntries.Remove(text3);
						}
						TryCaptureSave(text3);
					}
					DisplayOkDialogForCapture();
				}
			}
		}
		TryCaptureUnpauseByKeys();
	}

	private void TryCaptureSave(string entryString)
	{
		if (!capturePausedByKeys)
		{
			upBindItemEntries.Clear();
			if (!IsProhibited(entryString))
			{
				upBindItemEntries.Add(entryString);
			}
			CaptureSave();
		}
	}

	public bool IsProhibited(string entryString)
	{
		if (downBindItemEntries.Count == 0)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < downBindItemEntries.Count; i++)
		{
			if (downBindItemEntries[i] == "LeftShift" || downBindItemEntries[i] == "LeftControl" || downBindItemEntries[i] == "LeftAlt")
			{
				flag2 = true;
			}
			else
			{
				flag = true;
			}
		}
		switch (entryString)
		{
		case "LeftShift":
		case "LeftControl":
		case "LeftAlt":
			flag3 = true;
			break;
		}
		if (flag && !flag3)
		{
			return true;
		}
		if (flag2 && flag3)
		{
			return true;
		}
		return false;
	}

	private void CaptureSave()
	{
		captureBindItemEntries.Clear();
		captureBindItemEntries.AddRange(downBindItemEntries);
		captureBindItemEntries.AddRange(upBindItemEntries);
		captureBindItemEntries = PruneEntryStringsForRewired(captureBindItemEntries);
		upBindItemEntries.Clear();
		captureReassigned = true;
		capturePausedByKeys = true;
	}

	private List<string> PruneEntryStringsForRewired(List<string> entryStrings)
	{
		string text = string.Empty;
		string text2 = string.Empty;
		foreach (string entryString in entryStrings)
		{
			if (BindItem.Entry.FromEntryString(entryString) is BindItem.KeyboardEntry keyboardEntry)
			{
				if (ToRewiredModifier(keyboardEntry._keyCode) != ModifierKeyFlags.None)
				{
					text2 = ((!(text2 != string.Empty)) ? entryString : text2);
				}
				else
				{
					text = ((!(text != string.Empty)) ? entryString : text);
				}
			}
		}
		if (text2 == string.Empty)
		{
			return entryStrings;
		}
		if (text == string.Empty)
		{
			return entryStrings;
		}
		return entryStrings;
	}

	private void TryCaptureUnpauseByKeys()
	{
		if (capturePausedByKeys && captureReassigned && downBindItemEntries.Count == 0)
		{
			capturePausedByKeys = false;
			DisplayOkDialogForCapture();
		}
	}

	private void OnEnable()
	{
		Connect();
		Reload();
		NeedsSave = false;
		BuildAndPopulateMenuItems();
	}

	private void OnDisable()
	{
		Disconnect();
	}

	private void Update()
	{
		if (targetActionName != null)
		{
			CaptureShift();
		}
	}

	private void OnGUI()
	{
		if (targetActionName != null)
		{
			Capture(Event.current);
		}
	}

	public void Connect()
	{
		_sgs.Add(GameSignals.Connect(OperateSignal, OnOperate, this));
		if (operationsContainer != null)
		{
			_sgs.Add(operationsContainer.OnItemClickSignal.Connect(OnItemClick_Operations, this));
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

	public static void Reload(bool useDefault = false)
	{
		if (!defaultBindsLoaded)
		{
			RewiredToBindings(defaultBindItemMap);
			defaultBindsLoaded = true;
		}
		if (!useDefault)
		{
			UserToBindings();
		}
		else
		{
			userBindItemMap.Clear();
			mergedBindItemMap.Clear();
		}
		MergeBindings(useDefault);
		BindingsToRewired(CameraKeyboardMap, CameraMouseMap, string.Empty);
		BindingsToRewired(BricktronKeyboardMap, ProjectsMouseMap, string.Empty);
		BindingsToRewired(ProjectsKeyboardMap, ProjectsMouseMap, string.Empty);
		BindingsToRewired(BlueprintsKeyboardMap, ProjectsMouseMap, string.Empty);
		BindingsToRewired(WorldEditorMenuKeyboardMap, ProjectsMouseMap, string.Empty);
		BindingsToRewired(WorldEditorTerrainMenuKeyboardMap, ProjectsMouseMap, string.Empty);
		BindingsToRewired(WorldEditorMarkersMenuKeyboardMap, ProjectsMouseMap, string.Empty);
	}

	private static void MergeBindings(bool useDefault = false)
	{
		mergedBindItemMap.Clear();
		foreach (KeyValuePair<string, List<BindItem>> item in defaultBindItemMap)
		{
			mergedBindItemMap.Add(item.Key, new List<BindItem>(item.Value));
		}
		if (useDefault)
		{
			return;
		}
		foreach (string key in userBindItemMap.Keys)
		{
			if (!mergedBindItemMap.ContainsKey(key))
			{
				continue;
			}
			List<BindItem> list = mergedBindItemMap[key];
			List<BindItem> list2 = userBindItemMap[key];
			for (int i = 0; i < list2.Count; i++)
			{
				BindItem bindItem = list2[i];
				if (i < list.Count)
				{
					bindItem._rewiredElementIdentifierId = list[i]._rewiredElementIdentifierId;
					list[i] = bindItem;
				}
				else
				{
					mergedBindItemMap[key].Add(bindItem);
				}
			}
		}
	}

	private static void DebugLogBindItemsFromMap(Dictionary<string, List<BindItem>> targetBindItemMap, bool compact = false)
	{
		string text = "BIND ITEMS\n";
		foreach (string key in targetBindItemMap.Keys)
		{
			List<BindItem> list = targetBindItemMap[key];
			foreach (BindItem item in list)
			{
				text = text + "\t" + item._trigger + "\n";
				foreach (BindItem.Entry entry in item._entries)
				{
					text = text + "\t\t" + entry.ToEntryString(includeInputEvent: true) + "\n";
					if (!compact)
					{
						string text2 = text;
						text = string.Concat(text2, "\t\t\t", entry.GetType(), "\n");
						if (entry.GetType() == typeof(BindItem.MouseButtonEntry))
						{
							BindItem.MouseButtonEntry mouseButtonEntry = entry as BindItem.MouseButtonEntry;
							text2 = text;
							text = text2 + "\t\t\t\t_mouseButtonId: " + mouseButtonEntry._mouseButtonId + "\n";
							text2 = text;
							text = string.Concat(text2, "\t\t\t\t_entryEvent: ", mouseButtonEntry._inputEvent, "\n");
						}
						else if (entry.GetType() == typeof(BindItem.MouseWheelEntry))
						{
							BindItem.MouseWheelEntry mouseWheelEntry = entry as BindItem.MouseWheelEntry;
							text2 = text;
							text = string.Concat(text2, "\t\t\t\t_wheelDirection: ", mouseWheelEntry._wheelDirection, "\n");
						}
						else if (entry.GetType() == typeof(BindItem.KeyboardEntry))
						{
							BindItem.KeyboardEntry keyboardEntry = entry as BindItem.KeyboardEntry;
							text2 = text;
							text = string.Concat(text2, "\t\t\t\t_keyCode: ", keyboardEntry._keyCode, "\n");
							text2 = text;
							text = string.Concat(text2, "\t\t\t\t_entryEvent: ", keyboardEntry._inputEvent, "\n");
						}
					}
				}
			}
		}
		logger.Info(text);
	}

	private void SaveLuaFile()
	{
		string text = string.Empty;
		foreach (string key in mergedBindItemMap.Keys)
		{
			foreach (BindItem item in mergedBindItemMap[key])
			{
				string empty = string.Empty;
				string text2 = string.Empty;
				empty += "Bind(";
				empty = empty + "\"" + item._trigger + "\"";
				empty += ", {";
				foreach (BindItem.Entry entry in item._entries)
				{
					text2 = text2 + "\"" + entry.ToEntryString(includeInputEvent: false) + "\", ";
				}
				empty += text2;
				empty += "}";
				empty = empty.Replace(", }", "}");
				empty += ", {}";
				empty += ")\n";
				text += empty;
			}
		}
		string path = "Info/Lua/userconfigs.lua";
		File.WriteAllText(path, text);
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

	private static void UserToBindings()
	{
		userBindItemMap.Clear();
		Action<Script> preloadAction = delegate(Script script)
		{
			script.Globals["Bind"] = new Action<string, Table, Table>(Bind);
			script.Globals["Unbind"] = new Action<string>(Unbind);
			script.Globals["UnbindAll"] = new Action(UnbindAll);
		};
		LuaLoader.Load("userconfigs.lua", preloadAction, null);
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

	private static void RewiredToBindings(Dictionary<string, List<BindItem>> targetBindItemMap)
	{
		targetBindItemMap.Clear();
		IEnumerable<KeyboardMap> allMaps = PickingUtility.Player.controllers.maps.GetAllMaps<KeyboardMap>();
		foreach (KeyboardMap item in allMaps)
		{
			ActionElementMap[] buttonMaps = item.GetButtonMaps();
			foreach (ActionElementMap actionElementMap in buttonMaps)
			{
				List<BindItem> value = null;
				InputAction action = ReInput.mapping.GetAction(actionElementMap.actionId);
				if (!targetBindItemMap.TryGetValue(action.name, out value))
				{
					value = new List<BindItem>();
					targetBindItemMap.Add(action.name, value);
				}
				string text = actionElementMap.keyCode.ToString();
				if (actionElementMap.modifierKey1 != ModifierKey.None)
				{
					text = string.Concat(actionElementMap.modifierKey1, " ", BindItem.Entry.CompositeSplitChar, " ", text);
				}
				value.Add(new BindItem(action.name, BindItem.Entry.FromCompositeEntryString(text), actionElementMap.id));
			}
		}
		ActionElementMap[] buttonMaps2 = CameraMouseMap.GetButtonMaps();
		foreach (ActionElementMap actionElementMap2 in buttonMaps2)
		{
			List<BindItem> value2 = null;
			InputAction action2 = ReInput.mapping.GetAction(actionElementMap2.actionId);
			if (!targetBindItemMap.TryGetValue(action2.name, out value2))
			{
				value2 = new List<BindItem>();
				targetBindItemMap.Add(action2.name, value2);
			}
			value2.Add(new BindItem(action2.name, BindItem.Entry.FromRewiredMouse(actionElementMap2.elementIdentifierName), actionElementMap2.id));
		}
	}

	private static void BindingsToRewired(KeyboardMap targetKeyboardMap, MouseMap targetMouseMap, string actionStringPrefix)
	{
		if (targetKeyboardMap == null)
		{
			logger.Warn("Invalid Rewired keyboard map.");
			return;
		}
		targetKeyboardMap.ClearElementMaps();
		foreach (string key in mergedBindItemMap.Keys)
		{
			foreach (BindItem item in mergedBindItemMap[key])
			{
				BindItem.Entry entry = ((item._entries.Count >= 1) ? item._entries[0] : null);
				BindItem.Entry entry2 = ((item._entries.Count >= 2) ? item._entries[1] : null);
				if (entry != null && (entry2 == null || entry2.GetType() == typeof(BindItem.KeyboardEntry)) && entry.GetType() == typeof(BindItem.KeyboardEntry))
				{
					BindItem.KeyboardEntry keyboardEntry = entry as BindItem.KeyboardEntry;
					BindItem.KeyboardEntry keyboardEntry2 = entry2 as BindItem.KeyboardEntry;
					if (keyboardEntry != null && keyboardEntry2 != null && ToRewiredModifier(keyboardEntry._keyCode) != ModifierKeyFlags.None)
					{
						keyboardEntry = entry2 as BindItem.KeyboardEntry;
						keyboardEntry2 = entry as BindItem.KeyboardEntry;
					}
					targetKeyboardMap.ReplaceOrCreateElementMap(new ElementAssignment(keyboardEntry._keyCode, (keyboardEntry2 != null) ? ToRewiredModifier(keyboardEntry2._keyCode) : ModifierKeyFlags.None, ToRewiredActionId(key), Pole.Positive, item._rewiredElementIdentifierId));
				}
			}
		}
	}

	public static void Bind(string actionName, Table Keys, Table Controls)
	{
		List<BindItem.Entry> list = new List<BindItem.Entry>();
		for (int i = 0; i != Keys.Length; i++)
		{
			string entryString = (string)Keys[i + 1];
			list.Add(BindItem.Entry.FromEntryString(entryString));
		}
		for (int j = 0; j != Controls.Length; j++)
		{
			string entryString2 = (string)Controls[j + 1];
			list.Add(BindItem.Entry.FromEntryString(entryString2));
		}
		foreach (BindItem.Entry item in list)
		{
			if (item is BindItem.InputEventEntry inputEventEntry)
			{
				inputEventEntry._inputEvent = GetForcedInputEvent(inputEventEntry._inputEvent, actionName);
			}
		}
		if (!userBindItemMap.ContainsKey(actionName))
		{
			userBindItemMap.Add(actionName, new List<BindItem>());
		}
		userBindItemMap[actionName].Add(new BindItem(actionName, list));
	}

	public static void Unbind(BindItem unboundItem)
	{
	}

	public static void Unbind(string actionName)
	{
	}

	public static void UnbindAll()
	{
	}

	private static void UnbindConflict(ConflictReport _report)
	{
		foreach (ConflictReport.Conflict conflict in _report.conflicts)
		{
			int index = (conflict.isAlt ? 1 : 0);
			mergedBindItemMap[conflict.actionName][index]._entries.Clear();
		}
	}

	public void DisplayOkDialogForCapture()
	{
		if (targetActionName == null || targetActionName == string.Empty)
		{
			return;
		}
		string actionName = targetActionName;
		int bindItemIdx = (captureIsAlt ? 1 : 0);
		string[] array = ((mergedBindItemMap[actionName].Count >= bindItemIdx + 1) ? (from i in mergedBindItemMap[actionName][bindItemIdx]._entries.Slinq()
			select i.ToPrettyEntryString()).ToArray() : new string[0]);
		string text = ((array.Length != 0) ? string.Join(" + ", array) : string.Empty);
		List<string> list = new List<string>(downBindItemEntries);
		string text2 = ((list.Count != 0) ? string.Join(" + ", (from i in list.Slinq()
			select BindItem.Entry.FromEntryString(i).ToPrettyEntryString()).ToArray()) : string.Empty);
		string text3 = ((captureBindItemEntries.Count != 0) ? string.Join(" + ", (from i in captureBindItemEntries.Slinq()
			select BindItem.Entry.FromEntryString(i).ToPrettyEntryString()).ToArray()) : string.Empty);
		string text4 = ((capturePausedByKeys || list.Count == 0) ? text3 : text2);
		string shownCompositeEntryString = ((!captureReassigned) ? text : text4);
		string text5 = string.Empty;
		KeyContext keyContext = GetKeyContext(actionName);
		ConflictReport conflictReport = null;
		if (keyContext != KeyContext.None)
		{
			conflictReport = ConflictReport.Generate(keyContext, shownCompositeEntryString, actionName);
			if (!conflictReport.Success)
			{
				foreach (ConflictReport.Conflict conflict in conflictReport.conflicts)
				{
					string text6 = text5;
					text5 = text6 + "<size=16><b><color=#ff0f0f>" + I2Helper.TryGet("##mainmenu_options_assignkeybind_conflict").ToUpper() + ": " + I2Helper.TryGet(conflict.menuDisplayName).ToUpper() + "  |  " + I2Helper.TryGet(conflict.actionDisplayName).ToUpper() + ((!conflict.isAlt) ? string.Empty : "(Alt)") + "</color></b></size>\n";
				}
				text5 = text5 + "\n<b>" + I2Helper.TryGet("##mainmenu_options_assignkeybind_unbindwarning") + "</b>";
			}
		}
		string descriptiveName = ReInput.mapping.GetAction(ToRewiredActionId(actionName)).descriptiveName;
		string text7 = descriptiveName + "_info";
		string text8 = I2Helper.TryGet(text7);
		text8 = ((!(text8 == text7) && !text8.Contains(text7)) ? text8.ToUpper() : string.Empty);
		string text9 = I2Helper.TryGet("##mainmenu_options_assignkeybind_message").Replace("#xxx", "<i>[" + I2Helper.TryGet("##mainmenu_options_key_delete") + "]</i>");
		string text10 = ((!string.IsNullOrEmpty(text)) ? text : ("<i>" + I2Helper.TryGet("##mainmenu_options_assignkeybind_none") + "</i>"));
		string text11 = ((!string.IsNullOrEmpty(shownCompositeEntryString)) ? shownCompositeEntryString : ("<i>" + I2Helper.TryGet("##mainmenu_options_assignkeybind_none") + "</i>"));
		GameSignals.Invoke(GameSignals.OnDisplayGenericDialog, new OkDialogContainerPopulator.GenericDialog(I2Helper.TryGet("##mainmenu_options_assignkeybind").ToUpper(), text9 + "\n\n<size=16><b>" + I2Helper.TryGet(descriptiveName).ToUpper() + "</b></size>\n" + text8 + "\n" + text5, new OkDialogContainerPopulator.GenericDialog.Btn[2]
		{
			new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##mainmenu_options_assignkeybind_ok").ToUpper() + ": " + text11, delegate
			{
				targetActionName = null;
				if (captureReassigned)
				{
					NeedsSave = true;
					UnbindConflict(conflictReport);
					while (mergedBindItemMap[actionName].Count < bindItemIdx + 1)
					{
						mergedBindItemMap[actionName].Add(new BindItem(actionName));
					}
					mergedBindItemMap[actionName][bindItemIdx]._entries = BindItem.Entry.FromCompositeEntryString(shownCompositeEntryString);
					BuildAndPopulateMenuItems();
				}
				GameSignals.Invoke(GameSignals.OnCloseDialog);
			}, string.Empty),
			new OkDialogContainerPopulator.GenericDialog.Btn(I2Helper.TryGet("##mainmenu_options_assignkeybind_cancel").ToUpper() + ": " + text10, delegate
			{
				targetActionName = null;
				GameSignals.Invoke(GameSignals.OnCloseDialog);
			}, string.Empty)
		}));
	}
}
