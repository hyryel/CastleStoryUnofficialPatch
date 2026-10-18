using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Input;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Logging;
using Rewired;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public static class We
{
	public enum Tab
	{
		None,
		Terrain,
		Markers,
		Time,
		Map,
		Grid,
		Modes,
		Save,
		Workshop
	}

	[Serializable]
	public class KeyBindingCategoryUtil
	{
		public string CategoryName;

		private readonly Dictionary<KeyboardMap, bool> _keyboardMapFlagCache = new Dictionary<KeyboardMap, bool>();

		private readonly Dictionary<MouseMap, bool> _mouseMapFlagCache = new Dictionary<MouseMap, bool>();

		private List<KeyboardMap> KeyboardMaps => PickingUtility.Player.controllers.maps.GetAllMapsInCategory<KeyboardMap>(CategoryName).Slinq().ToList();

		private List<MouseMap> MouseMaps => PickingUtility.Player.controllers.maps.GetAllMapsInCategory<MouseMap>(CategoryName).Slinq().ToList();

		public KeyBindingCategoryUtil(string categoryName)
		{
			CategoryName = categoryName;
		}

		private void BuildKeyboardMapCache(IEnumerable<KeyboardMap> maps)
		{
			if (_keyboardMapFlagCache.Count != 0)
			{
				logger.WarnFormat("Non-empty cache rebuilt for CategoryName:\n{0}", CategoryName);
				return;
			}
			foreach (KeyboardMap map in maps)
			{
				_keyboardMapFlagCache.Add(map, map.enabled);
			}
		}

		private void BuildMouseMapCache(IEnumerable<MouseMap> maps)
		{
			if (_mouseMapFlagCache.Count != 0)
			{
				logger.WarnFormat("Non-empty cache rebuilt for CategoryName:\n{0}", CategoryName);
				return;
			}
			foreach (MouseMap map in maps)
			{
				_mouseMapFlagCache.Add(map, map.enabled);
			}
		}

		public void Set(bool flag)
		{
			List<KeyboardMap> keyboardMaps = KeyboardMaps;
			BuildKeyboardMapCache(keyboardMaps);
			foreach (KeyboardMap item in keyboardMaps)
			{
				item.enabled = flag;
			}
			List<MouseMap> mouseMaps = MouseMaps;
			BuildMouseMapCache(mouseMaps);
			foreach (MouseMap item2 in mouseMaps)
			{
				item2.enabled = flag;
			}
		}

		public void Reset()
		{
			foreach (KeyValuePair<KeyboardMap, bool> item in _keyboardMapFlagCache)
			{
				item.Key.enabled = item.Value;
			}
			_keyboardMapFlagCache.Clear();
			foreach (KeyValuePair<MouseMap, bool> item2 in _mouseMapFlagCache)
			{
				item2.Key.enabled = item2.Value;
			}
			_mouseMapFlagCache.Clear();
		}

		public void Reset(bool flag)
		{
			foreach (KeyValuePair<KeyboardMap, bool> item in _keyboardMapFlagCache)
			{
				item.Key.enabled = flag;
			}
			_keyboardMapFlagCache.Clear();
			foreach (KeyValuePair<MouseMap, bool> item2 in _mouseMapFlagCache)
			{
				item2.Key.enabled = flag;
			}
			_mouseMapFlagCache.Clear();
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(We).FullName);

	private static Tab _currentTab;

	public static Tab DefaultTab = Tab.Terrain;

	public static readonly Dictionary<Tab, string> BindingActionNameMap = new Dictionary<Tab, string>(FastEnumIntEqualityComparer<Tab>.Instance)
	{
		{
			Tab.Terrain,
			"we_terrain_menu_toggle"
		},
		{
			Tab.Markers,
			"we_markers_menu_toggle"
		},
		{
			Tab.Time,
			"we_time_menu_toggle"
		},
		{
			Tab.Map,
			"we_map_menu_toggle"
		},
		{
			Tab.Grid,
			"we_grid_menu_toggle"
		},
		{
			Tab.Modes,
			"we_modes_menu_toggle"
		},
		{
			Tab.Save,
			"we_save_menu_toggle"
		},
		{
			Tab.Workshop,
			"we_workshop_menu_toggle"
		}
	};

	public static readonly Dictionary<Tab, string> BindingCategoryNameMap = new Dictionary<Tab, string>(FastEnumIntEqualityComparer<Tab>.Instance)
	{
		{
			Tab.Terrain,
			"we_menu"
		},
		{
			Tab.Markers,
			"we_menu"
		},
		{
			Tab.Time,
			"we_menu"
		},
		{
			Tab.Map,
			"we_menu"
		},
		{
			Tab.Grid,
			"we_menu"
		},
		{
			Tab.Modes,
			"we_menu"
		},
		{
			Tab.Save,
			"we_menu"
		},
		{
			Tab.Workshop,
			"we_menu"
		}
	};

	public static readonly Dictionary<Tab, Action> BindingActionMap = new Dictionary<Tab, Action>(FastEnumIntEqualityComparer<Tab>.Instance)
	{
		{
			Tab.Terrain,
			delegate
			{
				CurrentTab = Tab.Terrain;
			}
		},
		{
			Tab.Markers,
			delegate
			{
				CurrentTab = Tab.Markers;
			}
		},
		{
			Tab.Time,
			delegate
			{
				CurrentTab = Tab.Time;
			}
		},
		{
			Tab.Map,
			delegate
			{
				CurrentTab = Tab.Map;
			}
		},
		{
			Tab.Grid,
			delegate
			{
				CurrentTab = Tab.Grid;
			}
		},
		{
			Tab.Modes,
			delegate
			{
				CurrentTab = Tab.Modes;
			}
		},
		{
			Tab.Save,
			delegate
			{
				CurrentTab = Tab.Save;
			}
		},
		{
			Tab.Workshop,
			delegate
			{
				CurrentTab = Tab.Workshop;
			}
		}
	};

	public static readonly Dictionary<Tab, string> TitleMap = new Dictionary<Tab, string>(FastEnumIntEqualityComparer<Tab>.Instance)
	{
		{
			Tab.None,
			string.Empty
		},
		{
			Tab.Terrain,
			"str_we_terrain_menu_title"
		},
		{
			Tab.Markers,
			"str_we_markers_menu_title"
		},
		{
			Tab.Time,
			"str_we_time_menu_title"
		},
		{
			Tab.Map,
			"str_we_map_menu_title"
		},
		{
			Tab.Grid,
			"str_we_grid_menu_title"
		},
		{
			Tab.Modes,
			"str_we_modes_menu_title"
		},
		{
			Tab.Save,
			"str_we_save_menu_title"
		},
		{
			Tab.Workshop,
			"str_we_workshop_menu_title"
		}
	};

	public static readonly Dictionary<Tab, Sprite> IconMap = new Dictionary<Tab, Sprite>(FastEnumIntEqualityComparer<Tab>.Instance)
	{
		{
			Tab.Terrain,
			IconKeys.UI_EditWorld.Get64()
		},
		{
			Tab.Markers,
			IconKeys.UI_Flag.Get64()
		},
		{
			Tab.Time,
			IconKeys.UI_Moon.Get64()
		},
		{
			Tab.Map,
			IconKeys.UI_Map.Get64()
		},
		{
			Tab.Grid,
			IconKeys.Grid.Get64()
		},
		{
			Tab.Modes,
			IconKeys.UI_Sword.Get64()
		},
		{
			Tab.Save,
			IconKeys.UI_Save.Get64()
		},
		{
			Tab.Workshop,
			IconKeys.UI_Workshop.Get64()
		}
	};

	public static Signal AfterChangeTabSignal = new Signal();

	public static Tab CurrentTab
	{
		get
		{
			return _currentTab;
		}
		set
		{
			Tab currentTab = _currentTab;
			if (value != currentTab)
			{
				_currentTab = value;
				AfterChangeTabSignal.Invoke();
			}
		}
	}

	public static string[] CategoryNameInclusions => new string[1] { "we_menu" };

	public static string[] CategoryNameExclusions => new string[0];
}
