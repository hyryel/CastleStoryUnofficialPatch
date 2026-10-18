using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Input;
using Brix.UI.Icons;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public static class WeMarkers
{
	public enum Tool
	{
		None,
		PlayerSpawn,
		Crystal,
		Mine,
		EnemySpawn,
		Tutorial
	}

	public enum SubTool
	{
		None,
		PlayerSpawnAdd,
		PlayerSpawnRemove,
		CrystalAdd,
		CrystalRemove,
		MineAdd,
		MineRemove,
		EnemySpawnAdd,
		EnemySpawnRemove,
		TutorialAdd,
		TutorialRemove
	}

	public enum SubToolPrefix
	{
		Main,
		Ctrl,
		Shift
	}

	private static SubTool _currentSubTool;

	private static readonly Dictionary<Tool, string> LabelMap = new Dictionary<Tool, string>(FastEnumIntEqualityComparer<Tool>.Instance)
	{
		{
			Tool.PlayerSpawn,
			"Crystal"
		},
		{
			Tool.Crystal,
			"Shard"
		},
		{
			Tool.Mine,
			"Mine"
		},
		{
			Tool.EnemySpawn,
			"Enemy"
		},
		{
			Tool.Tutorial,
			"Tutorial"
		}
	};

	private static readonly Dictionary<Tool, Sprite> IconMap = new Dictionary<Tool, Sprite>(FastEnumIntEqualityComparer<Tool>.Instance)
	{
		{
			Tool.PlayerSpawn,
			IconKeys.UI_CrystalMarker.Get32()
		},
		{
			Tool.Crystal,
			IconKeys.UI_PlayerMarker.Get32()
		},
		{
			Tool.Mine,
			IconKeys.UI_MineMarker.Get32()
		},
		{
			Tool.EnemySpawn,
			IconKeys.UI_EnemyMarker.Get32()
		},
		{
			Tool.Tutorial,
			IconKeys.UI_MissingIcon.Get32()
		}
	};

	private static readonly Dictionary<Tool, UIMarkersInterface.SpawnerObject> SpawnerObjectMap = new Dictionary<Tool, UIMarkersInterface.SpawnerObject>(FastEnumIntEqualityComparer<Tool>.Instance)
	{
		{
			Tool.PlayerSpawn,
			UIMarkersInterface.SpawnerObject.PlayerSpawn
		},
		{
			Tool.Crystal,
			UIMarkersInterface.SpawnerObject.Crystal
		},
		{
			Tool.EnemySpawn,
			UIMarkersInterface.SpawnerObject.CorruptronSpawn
		},
		{
			Tool.Tutorial,
			UIMarkersInterface.SpawnerObject.Tutorial
		}
	};

	private static readonly Dictionary<SubTool, Tool> ToolMap = new Dictionary<SubTool, Tool>(FastEnumIntEqualityComparer<SubTool>.Instance)
	{
		{
			SubTool.None,
			Tool.None
		},
		{
			SubTool.PlayerSpawnAdd,
			Tool.PlayerSpawn
		},
		{
			SubTool.PlayerSpawnRemove,
			Tool.PlayerSpawn
		},
		{
			SubTool.CrystalAdd,
			Tool.Crystal
		},
		{
			SubTool.CrystalRemove,
			Tool.Crystal
		},
		{
			SubTool.MineAdd,
			Tool.Mine
		},
		{
			SubTool.MineRemove,
			Tool.Mine
		},
		{
			SubTool.EnemySpawnAdd,
			Tool.EnemySpawn
		},
		{
			SubTool.EnemySpawnRemove,
			Tool.EnemySpawn
		},
		{
			SubTool.TutorialAdd,
			Tool.Tutorial
		},
		{
			SubTool.TutorialRemove,
			Tool.Tutorial
		}
	};

	private static readonly Dictionary<Tool, Dictionary<SubToolPrefix, SubTool>> SubToolMap = new Dictionary<Tool, Dictionary<SubToolPrefix, SubTool>>(FastEnumIntEqualityComparer<Tool>.Instance)
	{
		{
			Tool.None,
			new Dictionary<SubToolPrefix, SubTool>(FastEnumIntEqualityComparer<SubToolPrefix>.Instance) { 
			{
				SubToolPrefix.Main,
				SubTool.None
			} }
		},
		{
			Tool.PlayerSpawn,
			new Dictionary<SubToolPrefix, SubTool>(FastEnumIntEqualityComparer<SubToolPrefix>.Instance) { 
			{
				SubToolPrefix.Main,
				SubTool.PlayerSpawnAdd
			} }
		},
		{
			Tool.Crystal,
			new Dictionary<SubToolPrefix, SubTool>(FastEnumIntEqualityComparer<SubToolPrefix>.Instance) { 
			{
				SubToolPrefix.Main,
				SubTool.CrystalAdd
			} }
		},
		{
			Tool.Mine,
			new Dictionary<SubToolPrefix, SubTool>(FastEnumIntEqualityComparer<SubToolPrefix>.Instance)
			{
				{
					SubToolPrefix.Main,
					SubTool.MineAdd
				},
				{
					SubToolPrefix.Ctrl,
					SubTool.MineRemove
				}
			}
		},
		{
			Tool.EnemySpawn,
			new Dictionary<SubToolPrefix, SubTool>(FastEnumIntEqualityComparer<SubToolPrefix>.Instance) { 
			{
				SubToolPrefix.Main,
				SubTool.EnemySpawnAdd
			} }
		},
		{
			Tool.Tutorial,
			new Dictionary<SubToolPrefix, SubTool>(FastEnumIntEqualityComparer<SubToolPrefix>.Instance)
			{
				{
					SubToolPrefix.Main,
					SubTool.TutorialAdd
				},
				{
					SubToolPrefix.Ctrl,
					SubTool.TutorialRemove
				}
			}
		}
	};

	private static readonly Dictionary<SubTool, string> InfoMap = new Dictionary<SubTool, string>(FastEnumIntEqualityComparer<SubTool>.Instance)
	{
		{
			SubTool.None,
			string.Empty
		},
		{
			SubTool.PlayerSpawnAdd,
			"str_we_tool_playerspawnadd_info"
		},
		{
			SubTool.PlayerSpawnRemove,
			"str_we_tool_playerspawnremove_info"
		},
		{
			SubTool.CrystalAdd,
			"str_we_tool_crystaladd_info"
		},
		{
			SubTool.CrystalRemove,
			"str_we_tool_crystalremove_info"
		},
		{
			SubTool.MineAdd,
			"str_we_tool_mineadd_info"
		},
		{
			SubTool.MineRemove,
			"str_we_tool_mineremove_info"
		},
		{
			SubTool.EnemySpawnAdd,
			"str_we_tool_corruptronadd_info"
		},
		{
			SubTool.EnemySpawnRemove,
			"str_we_tool_corruptronremove_info"
		},
		{
			SubTool.TutorialAdd,
			"str_we_tool_tutorialadd_info"
		},
		{
			SubTool.TutorialRemove,
			"str_we_tool_tutorialremove_info"
		}
	};

	private static readonly Dictionary<SubTool, string> HelpMap = new Dictionary<SubTool, string>(FastEnumIntEqualityComparer<SubTool>.Instance)
	{
		{
			SubTool.None,
			string.Empty
		},
		{
			SubTool.PlayerSpawnAdd,
			"str_we_tool_playerspawnadd_help"
		},
		{
			SubTool.PlayerSpawnRemove,
			"str_we_tool_playerspawnremove_help"
		},
		{
			SubTool.CrystalAdd,
			"str_we_tool_crystaladd_help"
		},
		{
			SubTool.CrystalRemove,
			"str_we_tool_crystalremove_help"
		},
		{
			SubTool.MineAdd,
			"str_we_tool_mineadd_help"
		},
		{
			SubTool.MineRemove,
			"str_we_tool_mineremove_help"
		},
		{
			SubTool.EnemySpawnAdd,
			"str_we_tool_corruptronadd_help"
		},
		{
			SubTool.EnemySpawnRemove,
			"str_we_tool_corruptronremove_help"
		},
		{
			SubTool.TutorialAdd,
			"str_we_tool_tutorialadd_help"
		},
		{
			SubTool.TutorialRemove,
			"str_we_tool_tutorialremove_help"
		}
	};

	private static readonly Dictionary<Tool, string> DescMap = new Dictionary<Tool, string>(FastEnumIntEqualityComparer<Tool>.Instance)
	{
		{
			Tool.None,
			string.Empty
		},
		{
			Tool.PlayerSpawn,
			"str_we_tool_player_desc"
		},
		{
			Tool.Crystal,
			"str_we_tool_crystal_desc"
		},
		{
			Tool.Mine,
			"str_we_tool_mine_desc"
		},
		{
			Tool.EnemySpawn,
			"str_we_tool_corruptron_desc"
		},
		{
			Tool.Tutorial,
			"str_we_tool_tutorial_desc"
		}
	};

	private static readonly Dictionary<SubToolPrefix, string> SubToolPrefixNameMap = new Dictionary<SubToolPrefix, string>(FastEnumIntEqualityComparer<SubToolPrefix>.Instance)
	{
		{
			SubToolPrefix.Ctrl,
			"Ctrl"
		},
		{
			SubToolPrefix.Main,
			"Main"
		},
		{
			SubToolPrefix.Shift,
			"Shift"
		}
	};

	public static readonly Dictionary<Tool, string> BindingActionNameMap = new Dictionary<Tool, string>(FastEnumIntEqualityComparer<Tool>.Instance)
	{
		{
			Tool.PlayerSpawn,
			"we_player_tool_select"
		},
		{
			Tool.EnemySpawn,
			"we_enemy_tool_select"
		},
		{
			Tool.Crystal,
			"we_crystal_tool_select"
		},
		{
			Tool.Mine,
			"we_mine_tool_select"
		},
		{
			Tool.Tutorial,
			"we_tutorial_tool_select"
		}
	};

	public static readonly Dictionary<Tool, string> BindingCategoryNameMap = new Dictionary<Tool, string>(FastEnumIntEqualityComparer<Tool>.Instance)
	{
		{
			Tool.PlayerSpawn,
			"we_markers_menu"
		},
		{
			Tool.EnemySpawn,
			"we_markers_menu"
		},
		{
			Tool.Crystal,
			"we_markers_menu"
		},
		{
			Tool.Mine,
			"we_markers_menu"
		},
		{
			Tool.Tutorial,
			"we_markers_menu"
		}
	};

	public static readonly Dictionary<Tool, Action> BindingActionMap = new Dictionary<Tool, Action>(FastEnumIntEqualityComparer<Tool>.Instance)
	{
		{
			Tool.PlayerSpawn,
			delegate
			{
				SendTool(Tool.PlayerSpawn);
			}
		},
		{
			Tool.EnemySpawn,
			delegate
			{
				SendTool(Tool.EnemySpawn);
			}
		},
		{
			Tool.Crystal,
			delegate
			{
				SendTool(Tool.Crystal);
			}
		},
		{
			Tool.Mine,
			delegate
			{
				SendTool(Tool.Mine);
			}
		},
		{
			Tool.Tutorial,
			delegate
			{
				SendTool(Tool.Tutorial);
			}
		}
	};

	public static string[] CategoryNameInclusions = new string[1] { "we_markers_menu" };

	public static string[] CategoryNameExclusions = new string[0];

	public static Signal AfterChangeToolSignal = new Signal();

	public static SubTool CurrentSubTool => _currentSubTool;

	public static Tool CurrentTool => ToTool(_currentSubTool);

	public static void ReceiveTool(Tool tool)
	{
		ReceiveSubTool(ToSubTool(tool));
	}

	public static void ReceiveSubTool(SubTool subTool)
	{
		if (subTool != CurrentSubTool)
		{
			_currentSubTool = subTool;
			AfterChangeToolSignal.Invoke();
		}
	}

	public static void SendTool(Tool tool)
	{
		if (tool != Tool.None && We.CurrentTab != We.Tab.Markers && We.CurrentTab != We.Tab.None)
		{
			return;
		}
		if (tool == Tool.None)
		{
			Picking.Instance.ClearPick();
			return;
		}
		UIMarkersInterface.SpawnerObject spawnerObject = (SpawnerObjectMap.ContainsKey(tool) ? SpawnerObjectMap[tool] : UIMarkersInterface.SpawnerObject.Unknown);
		if (spawnerObject != UIMarkersInterface.SpawnerObject.Unknown)
		{
			UIMarkersInterface.SendToPicker(spawnerObject);
		}
	}

	public static Tool GetToolFromSpawnerObject(UIMarkersInterface.SpawnerObject spawnerObject)
	{
		foreach (KeyValuePair<Tool, UIMarkersInterface.SpawnerObject> item in SpawnerObjectMap)
		{
			if (item.Value == spawnerObject)
			{
				return item.Key;
			}
		}
		return Tool.None;
	}

	public static UIMarkersInterface.SpawnerObject GetSpawnerObjectFromTool(Tool tool)
	{
		return SpawnerObjectMap.ContainsKey(tool) ? SpawnerObjectMap[tool] : UIMarkersInterface.SpawnerObject.Unknown;
	}

	public static string GetLabelFromTool(Tool tool)
	{
		return (!LabelMap.ContainsKey(tool)) ? string.Empty : LabelMap[tool];
	}

	public static Sprite GetIconFromTool(Tool tool)
	{
		return (!IconMap.ContainsKey(tool)) ? null : IconMap[tool];
	}

	public static string GetInfoFromSubTool(SubTool subTool)
	{
		return (!InfoMap.ContainsKey(subTool)) ? string.Empty : InfoMap[subTool];
	}

	public static string GetHelpFromSubTool(SubTool subTool)
	{
		return (!HelpMap.ContainsKey(subTool)) ? string.Empty : HelpMap[subTool];
	}

	public static Tool ToTool(SubTool subTool)
	{
		return ToolMap[subTool];
	}

	public static SubTool ToSubTool(Tool tool, SubToolPrefix subToolPrefix = SubToolPrefix.Main)
	{
		if (SubToolMap.ContainsKey(tool))
		{
			Dictionary<SubToolPrefix, SubTool> dictionary = SubToolMap[tool];
			if (dictionary.ContainsKey(subToolPrefix))
			{
				return dictionary[subToolPrefix];
			}
			return dictionary[SubToolPrefix.Main];
		}
		return SubTool.None;
	}

	public static Dictionary<SubToolPrefix, SubTool> ToSubToolPrefixMap(SubTool subTool)
	{
		return SubToolMap[ToTool(subTool)];
	}

	public static string GetInfoMessageFromSubTool(SubTool subTool)
	{
		Dictionary<SubToolPrefix, SubTool> dictionary = ToSubToolPrefixMap(subTool);
		List<string> list = new List<string>();
		Tool key = ToTool(subTool);
		string text = I2Helper.TryGet(DescMap[key]);
		if (!string.IsNullOrEmpty(text))
		{
			list.Add($"<color=white>{text}</color>");
		}
		list.Add(string.Empty);
		foreach (SubToolPrefix key2 in dictionary.Keys)
		{
			SubTool subTool2 = dictionary[key2];
			string text2 = I2Helper.TryGet(GetInfoFromSubTool(subTool2));
			if (!(text2 == string.Empty) && !(text2 == "*"))
			{
				text2 = ((key2 != SubToolPrefix.Main) ? $"[{SubToolPrefixNameMap[key2]} + Click] {text2}" : $"[Click] {text2}");
				if (subTool2 == CurrentSubTool)
				{
					text2 = $"<color=white>{text2}</color>";
				}
				list.Add(text2);
			}
		}
		return string.Join("\n", list.ToArray());
	}

	public static void OnOverridePickerSetWithOptions()
	{
		SubTool subTool = SubTool.None;
		if (Picking.Instance.OverridePicker != null)
		{
			Tool tool = Tool.None;
			UIMarkersInterface.SpawnerObject spawnerObject = UIMarkersInterface.FromPicker(Picking.Instance.OverridePicker);
			if (spawnerObject != UIMarkersInterface.SpawnerObject.Unknown)
			{
				tool = GetToolFromSpawnerObject(spawnerObject);
			}
			SubToolPrefix subToolPrefix = SubToolPrefix.Main;
			subToolPrefix = ((!Picking.Instance.PickingObserver.IsSecondaryAction) ? subToolPrefix : SubToolPrefix.Shift);
			subToolPrefix = (Picking.Instance.PickingObserver.IsAltAction ? SubToolPrefix.Ctrl : subToolPrefix);
			subTool = ToSubTool(tool, subToolPrefix);
		}
		ReceiveSubTool(subTool);
	}

	public static SubToolPrefix GetToolPrefix()
	{
		Picking instance = Picking.Instance;
		if (instance == null)
		{
			return SubToolPrefix.Main;
		}
		Picker overridePicker = instance.OverridePicker;
		if (overridePicker == null)
		{
			return SubToolPrefix.Main;
		}
		if (instance.OverridePicker.CurrentPickerComponent == instance.OverridePicker.mainComponent)
		{
			return SubToolPrefix.Main;
		}
		if (instance.OverridePicker.CurrentPickerComponent == instance.OverridePicker.ctrlComponent)
		{
			return SubToolPrefix.Ctrl;
		}
		if (instance.OverridePicker.CurrentPickerComponent == instance.OverridePicker.shiftComponent)
		{
			return SubToolPrefix.Shift;
		}
		return SubToolPrefix.Main;
	}

	public static void OnPickerCompActivated(PickerComponent arg1)
	{
		ReceiveSubTool(ToSubTool(CurrentTool, GetToolPrefix()));
	}
}
