using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Input;
using Brix.Input.WorldEditor;
using Brix.UI.Icons;
using Brix.Utils;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public static class WeTerrain
{
	public enum Brush
	{
		None,
		Island,
		Volume,
		Plateau,
		Smooth,
		Grass,
		Tree,
		Plant,
		Boulder,
		Iron,
		Pyrite,
		Underside
	}

	public enum SubBrush
	{
		None,
		IslandAdd,
		IslandHeight,
		IslandRemove,
		VolumeAdd,
		VolumeRemove,
		Plateau,
		Smooth,
		GrassAdd,
		GrassRemove,
		TreeAdd,
		TreeRemove,
		PlantAdd,
		PlantRemove,
		BoulderAdd,
		BoulderRemove,
		IronAdd,
		IronRemove,
		PyriteAdd,
		PyriteRemove,
		Underside
	}

	public enum SubBrushPrefix
	{
		Main,
		Ctrl,
		Shift
	}

	private static SubBrush _currentSubBrush;

	private static readonly Dictionary<Brush, string> LabelMap = new Dictionary<Brush, string>(FastEnumIntEqualityComparer<Brush>.Instance)
	{
		{
			Brush.Island,
			"Island"
		},
		{
			Brush.Volume,
			"Elevation"
		},
		{
			Brush.Plateau,
			"Plateau"
		},
		{
			Brush.Smooth,
			"Smooth"
		},
		{
			Brush.Grass,
			"Grass"
		},
		{
			Brush.Tree,
			"Tree"
		},
		{
			Brush.Plant,
			"Plant"
		},
		{
			Brush.Boulder,
			"Boulder"
		},
		{
			Brush.Iron,
			"Iron"
		},
		{
			Brush.Pyrite,
			"Brimstone"
		},
		{
			Brush.Underside,
			"Underside"
		}
	};

	private static readonly Dictionary<Brush, Sprite> IconMap = new Dictionary<Brush, Sprite>(FastEnumIntEqualityComparer<Brush>.Instance)
	{
		{
			Brush.Island,
			IconKeys.UI_PlatformBrush.Get32()
		},
		{
			Brush.Volume,
			IconKeys.UI_RaiseBrush.Get32()
		},
		{
			Brush.Plateau,
			IconKeys.UI_PlateauBrush.Get32()
		},
		{
			Brush.Smooth,
			IconKeys.UI_SmoothBrush.Get32()
		},
		{
			Brush.Grass,
			IconKeys.UI_GrassBrush.Get32()
		},
		{
			Brush.Tree,
			IconKeys.UI_TreeBrush.Get32()
		},
		{
			Brush.Plant,
			IconKeys.UI_PlantBrush.Get32()
		},
		{
			Brush.Boulder,
			IconKeys.BoulderBrush.Get32()
		},
		{
			Brush.Iron,
			IconKeys.IronBrush.Get32()
		},
		{
			Brush.Pyrite,
			IconKeys.PyriteBrush.Get32()
		},
		{
			Brush.Underside,
			IconKeys.UI_GenerateUnderside.Get32()
		}
	};

	private static readonly Dictionary<Brush, Factory.AssetKey> PickerKeyMap = new Dictionary<Brush, Factory.AssetKey>(FastEnumIntEqualityComparer<Brush>.Instance)
	{
		{
			Brush.Island,
			Pickers.IslandBrush
		},
		{
			Brush.Volume,
			Pickers.ElevationBrush
		},
		{
			Brush.Plateau,
			Pickers.PlateauBrush
		},
		{
			Brush.Smooth,
			Pickers.SmoothBrush
		},
		{
			Brush.Grass,
			Pickers.GrassBrush
		},
		{
			Brush.Tree,
			Pickers.TreeBrush
		},
		{
			Brush.Plant,
			Pickers.PlantBrush
		},
		{
			Brush.Boulder,
			Pickers.BoulderBrush
		},
		{
			Brush.Iron,
			Pickers.IronBrush
		},
		{
			Brush.Pyrite,
			Pickers.PyriteBrush
		}
	};

	private static readonly Dictionary<SubBrush, Brush> BrushMap = new Dictionary<SubBrush, Brush>(FastEnumIntEqualityComparer<SubBrush>.Instance)
	{
		{
			SubBrush.None,
			Brush.None
		},
		{
			SubBrush.IslandAdd,
			Brush.Island
		},
		{
			SubBrush.IslandHeight,
			Brush.Island
		},
		{
			SubBrush.IslandRemove,
			Brush.Island
		},
		{
			SubBrush.VolumeAdd,
			Brush.Volume
		},
		{
			SubBrush.VolumeRemove,
			Brush.Volume
		},
		{
			SubBrush.Plateau,
			Brush.Plateau
		},
		{
			SubBrush.Smooth,
			Brush.Smooth
		},
		{
			SubBrush.GrassAdd,
			Brush.Grass
		},
		{
			SubBrush.GrassRemove,
			Brush.Grass
		},
		{
			SubBrush.TreeAdd,
			Brush.Tree
		},
		{
			SubBrush.TreeRemove,
			Brush.Tree
		},
		{
			SubBrush.PlantAdd,
			Brush.Plant
		},
		{
			SubBrush.PlantRemove,
			Brush.Plant
		},
		{
			SubBrush.BoulderAdd,
			Brush.Boulder
		},
		{
			SubBrush.BoulderRemove,
			Brush.Boulder
		},
		{
			SubBrush.IronAdd,
			Brush.Iron
		},
		{
			SubBrush.IronRemove,
			Brush.Iron
		},
		{
			SubBrush.PyriteAdd,
			Brush.Pyrite
		},
		{
			SubBrush.PyriteRemove,
			Brush.Pyrite
		},
		{
			SubBrush.Underside,
			Brush.Underside
		}
	};

	private static readonly Dictionary<Brush, Dictionary<SubBrushPrefix, SubBrush>> SubBrushMap = new Dictionary<Brush, Dictionary<SubBrushPrefix, SubBrush>>(FastEnumIntEqualityComparer<Brush>.Instance)
	{
		{
			Brush.None,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance) { 
			{
				SubBrushPrefix.Main,
				SubBrush.None
			} }
		},
		{
			Brush.Island,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance)
			{
				{
					SubBrushPrefix.Main,
					SubBrush.IslandAdd
				},
				{
					SubBrushPrefix.Shift,
					SubBrush.IslandHeight
				},
				{
					SubBrushPrefix.Ctrl,
					SubBrush.IslandRemove
				}
			}
		},
		{
			Brush.Volume,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance)
			{
				{
					SubBrushPrefix.Main,
					SubBrush.VolumeAdd
				},
				{
					SubBrushPrefix.Ctrl,
					SubBrush.VolumeRemove
				}
			}
		},
		{
			Brush.Plateau,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance) { 
			{
				SubBrushPrefix.Main,
				SubBrush.Plateau
			} }
		},
		{
			Brush.Smooth,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance) { 
			{
				SubBrushPrefix.Main,
				SubBrush.Smooth
			} }
		},
		{
			Brush.Grass,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance)
			{
				{
					SubBrushPrefix.Main,
					SubBrush.GrassAdd
				},
				{
					SubBrushPrefix.Ctrl,
					SubBrush.GrassRemove
				}
			}
		},
		{
			Brush.Tree,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance)
			{
				{
					SubBrushPrefix.Main,
					SubBrush.TreeAdd
				},
				{
					SubBrushPrefix.Ctrl,
					SubBrush.TreeRemove
				}
			}
		},
		{
			Brush.Plant,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance)
			{
				{
					SubBrushPrefix.Main,
					SubBrush.PlantAdd
				},
				{
					SubBrushPrefix.Ctrl,
					SubBrush.PlantRemove
				}
			}
		},
		{
			Brush.Boulder,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance)
			{
				{
					SubBrushPrefix.Main,
					SubBrush.BoulderAdd
				},
				{
					SubBrushPrefix.Ctrl,
					SubBrush.BoulderRemove
				}
			}
		},
		{
			Brush.Iron,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance)
			{
				{
					SubBrushPrefix.Main,
					SubBrush.IronAdd
				},
				{
					SubBrushPrefix.Ctrl,
					SubBrush.IronRemove
				}
			}
		},
		{
			Brush.Pyrite,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance)
			{
				{
					SubBrushPrefix.Main,
					SubBrush.PyriteAdd
				},
				{
					SubBrushPrefix.Ctrl,
					SubBrush.PyriteRemove
				}
			}
		},
		{
			Brush.Underside,
			new Dictionary<SubBrushPrefix, SubBrush>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance) { 
			{
				SubBrushPrefix.Main,
				SubBrush.Underside
			} }
		}
	};

	private static readonly Dictionary<SubBrush, string> InfoMap = new Dictionary<SubBrush, string>(FastEnumIntEqualityComparer<SubBrush>.Instance)
	{
		{
			SubBrush.None,
			string.Empty
		},
		{
			SubBrush.IslandAdd,
			"str_we_tool_islandadd_info"
		},
		{
			SubBrush.IslandHeight,
			"str_we_tool_islandheight_info"
		},
		{
			SubBrush.IslandRemove,
			"str_we_tool_islandremove_info"
		},
		{
			SubBrush.VolumeAdd,
			"str_we_tool_volumeadd_info"
		},
		{
			SubBrush.VolumeRemove,
			"str_we_tool_volumeremove_info"
		},
		{
			SubBrush.Plateau,
			"str_we_tool_plateau_info"
		},
		{
			SubBrush.Smooth,
			"str_we_tool_smooth_info"
		},
		{
			SubBrush.GrassAdd,
			"str_we_tool_grassadd_info"
		},
		{
			SubBrush.GrassRemove,
			"str_we_tool_grassremove_info"
		},
		{
			SubBrush.TreeAdd,
			"str_we_tool_treeadd_info"
		},
		{
			SubBrush.TreeRemove,
			"str_we_tool_treeremove_info"
		},
		{
			SubBrush.Underside,
			"str_we_tool_underside_info"
		}
	};

	private static readonly Dictionary<SubBrush, string> HelpMap = new Dictionary<SubBrush, string>(FastEnumIntEqualityComparer<SubBrush>.Instance)
	{
		{
			SubBrush.None,
			string.Empty
		},
		{
			SubBrush.IslandAdd,
			"str_we_addislandbrush_help"
		},
		{
			SubBrush.IslandRemove,
			"str_we_removeislandbrush_help"
		},
		{
			SubBrush.VolumeAdd,
			"str_we_addvolumebrush_help"
		},
		{
			SubBrush.VolumeRemove,
			"str_we_removevolumebrush_help"
		},
		{
			SubBrush.Plateau,
			"str_we_plateaubrush_help"
		},
		{
			SubBrush.Smooth,
			"str_we_smoothbrush_help"
		},
		{
			SubBrush.GrassAdd,
			"str_we_addgrassbrush_help"
		},
		{
			SubBrush.GrassRemove,
			"str_we_removegrassbrush_help"
		},
		{
			SubBrush.TreeAdd,
			"str_we_addtreebrush_help"
		},
		{
			SubBrush.TreeRemove,
			"str_we_removetreebrush_help"
		},
		{
			SubBrush.Underside,
			"str_we_tool_underside_help"
		}
	};

	private static readonly List<string> TmpMessageList = new List<string>();

	private static readonly Dictionary<SubBrushPrefix, string> SubBrushPrefixNameMap = new Dictionary<SubBrushPrefix, string>(FastEnumIntEqualityComparer<SubBrushPrefix>.Instance)
	{
		{
			SubBrushPrefix.Ctrl,
			"Ctrl"
		},
		{
			SubBrushPrefix.Main,
			"Main"
		},
		{
			SubBrushPrefix.Shift,
			"Shift"
		}
	};

	public static readonly Dictionary<Brush, string> BindingActionNameMap = new Dictionary<Brush, string>(FastEnumIntEqualityComparer<Brush>.Instance)
	{
		{
			Brush.Island,
			"we_create_tool_select"
		},
		{
			Brush.Volume,
			"we_raise_tool_select"
		},
		{
			Brush.Plateau,
			"we_flatten_tool_select"
		},
		{
			Brush.Smooth,
			"we_smooth_tool_select"
		},
		{
			Brush.Grass,
			"we_grass_tool_select"
		},
		{
			Brush.Tree,
			"we_tree_tool_select"
		},
		{
			Brush.Plant,
			"we_plant_tool_select"
		},
		{
			Brush.Boulder,
			"we_boulder_tool_select"
		},
		{
			Brush.Iron,
			"we_iron_tool_select"
		},
		{
			Brush.Pyrite,
			"we_pyrite_tool_select"
		},
		{
			Brush.Underside,
			"we_underside_tool_select"
		}
	};

	public static readonly Dictionary<Brush, string> BindingCategoryNameMap = new Dictionary<Brush, string>(FastEnumIntEqualityComparer<Brush>.Instance)
	{
		{
			Brush.Island,
			"we_terrain_menu"
		},
		{
			Brush.Volume,
			"we_terrain_menu"
		},
		{
			Brush.Plateau,
			"we_terrain_menu"
		},
		{
			Brush.Smooth,
			"we_terrain_menu"
		},
		{
			Brush.Grass,
			"we_terrain_menu"
		},
		{
			Brush.Tree,
			"we_terrain_menu"
		},
		{
			Brush.Plant,
			"we_terrain_menu"
		},
		{
			Brush.Boulder,
			"we_terrain_menu"
		},
		{
			Brush.Iron,
			"we_terrain_menu"
		},
		{
			Brush.Pyrite,
			"we_terrain_menu"
		},
		{
			Brush.Underside,
			"we_terrain_menu"
		}
	};

	public static readonly Dictionary<Brush, Action> BindingActionMap = new Dictionary<Brush, Action>(FastEnumIntEqualityComparer<Brush>.Instance)
	{
		{
			Brush.Island,
			delegate
			{
				SendBrush(Brush.Island);
			}
		},
		{
			Brush.Volume,
			delegate
			{
				SendBrush(Brush.Volume);
			}
		},
		{
			Brush.Plateau,
			delegate
			{
				SendBrush(Brush.Plateau);
			}
		},
		{
			Brush.Smooth,
			delegate
			{
				SendBrush(Brush.Smooth);
			}
		},
		{
			Brush.Grass,
			delegate
			{
				SendBrush(Brush.Grass);
			}
		},
		{
			Brush.Tree,
			delegate
			{
				SendBrush(Brush.Tree);
			}
		},
		{
			Brush.Plant,
			delegate
			{
				SendBrush(Brush.Plant);
			}
		},
		{
			Brush.Boulder,
			delegate
			{
				SendBrush(Brush.Boulder);
			}
		},
		{
			Brush.Iron,
			delegate
			{
				SendBrush(Brush.Iron);
			}
		},
		{
			Brush.Pyrite,
			delegate
			{
				SendBrush(Brush.Pyrite);
			}
		},
		{
			Brush.Underside,
			delegate
			{
				SendBrush(Brush.Underside);
			}
		}
	};

	public static string[] CategoryNameInclusions = new string[1] { "we_terrain_menu" };

	public static string[] CategoryNameExclusions = new string[0];

	public static Signal AfterChangeBrushSignal = new Signal();

	public static Signal AfterChangeBrushSettingsSignal = new Signal();

	public static SubBrush CurrentSubBrush => _currentSubBrush;

	public static Brush CurrentBrush => ToBrush(_currentSubBrush);

	public static void ReceiveBrush(Brush brush)
	{
		ReceiveSubBrush(ToSubBrush(brush));
	}

	public static void ReceiveSubBrush(SubBrush subBrush)
	{
		if (subBrush != CurrentSubBrush)
		{
			_currentSubBrush = subBrush;
			if (CurrentBrush == Brush.Underside)
			{
				Picking.Instance.WorldEditorGrid.ActiveByUndersideMenu = true;
			}
			else
			{
				Picking.Instance.WorldEditorGrid.ActiveByUndersideMenu = false;
			}
			AfterChangeBrushSignal.Invoke();
		}
	}

	public static void SendBrush(Brush brush)
	{
		switch (brush)
		{
		case Brush.Underside:
			Picking.Instance.ClearPick();
			ReceiveBrush(brush);
			break;
		case Brush.None:
			Picking.Instance.ClearPick();
			if (CurrentBrush == Brush.Underside)
			{
				ReceiveBrush(brush);
			}
			break;
		default:
		{
			Factory.AssetKey assetKey = ((!PickerKeyMap.ContainsKey(brush)) ? null : PickerKeyMap[brush]);
			if (assetKey != null)
			{
				Picking.Instance.SendPicker(assetKey);
			}
			break;
		}
		}
	}

	public static BrushSettings GetBrushSettings()
	{
		Picking instance = Picking.Instance;
		if (instance == null)
		{
			return null;
		}
		Picker overridePicker = instance.OverridePicker;
		if (overridePicker == null)
		{
			return null;
		}
		BrushSettings sharedSettings = overridePicker.SharedSettings;
		if (sharedSettings == null)
		{
			return null;
		}
		return sharedSettings;
	}

	public static DynValue GetLuaBrushSettings()
	{
		DynValue result = DynValue.NewPrimeTable();
		Table table = result.Table;
		BrushSettings brushSettings = GetBrushSettings();
		if (brushSettings == null)
		{
			return result;
		}
		if (brushSettings != null)
		{
			ISize size = brushSettings;
			Table table2 = (Table)(table["size"] = DynValue.NewPrimeTable().Table);
			table2["size"] = size.Size;
			table2["sizeString"] = size.SliderString;
		}
		if (brushSettings is IIntensity)
		{
			IIntensity intensity = brushSettings as IIntensity;
			Table table4 = (Table)(table["intensity"] = DynValue.NewPrimeTable().Table);
			table4["intensity"] = intensity.Intensity;
		}
		if (brushSettings is ICreateIsland)
		{
			ICreateIsland createIsland = brushSettings as ICreateIsland;
			Table table6 = (Table)(table["createIsland"] = DynValue.NewPrimeTable().Table);
			table6["dontCreate"] = createIsland.DontCreate;
			table6["dontLower"] = createIsland.DontLower;
			table6["dontRaise"] = createIsland.DontRaise;
		}
		if (brushSettings is ILockHeight)
		{
			ILockHeight lockHeight = brushSettings as ILockHeight;
			Table table8 = (Table)(table["lockHeight"] = DynValue.NewPrimeTable().Table);
			table8["lockHeight"] = lockHeight.LockHeight;
		}
		if (brushSettings is ITerrainType)
		{
			ITerrainType terrainType = brushSettings as ITerrainType;
			Table table10 = (Table)(table["terrainType"] = DynValue.NewPrimeTable().Table);
			table10["terrainType"] = terrainType.TerrainType;
		}
		if (brushSettings is IPlantType)
		{
			IPlantType plantType = brushSettings as IPlantType;
			Table table12 = (Table)(table["plantType"] = DynValue.NewPrimeTable().Table);
			table12["plantType"] = plantType.PlantType;
		}
		if (brushSettings is IDistanceToEdge)
		{
			IDistanceToEdge distanceToEdge = brushSettings as IDistanceToEdge;
			Table table14 = (Table)(table["distanceToEdge"] = DynValue.NewPrimeTable().Table);
			table14["distanceToEdge"] = distanceToEdge.DistanceToEdge;
		}
		if (brushSettings is ILevelOverride)
		{
			ILevelOverride levelOverride = brushSettings as ILevelOverride;
			Table table16 = (Table)(table["levelOverride"] = DynValue.NewPrimeTable().Table);
			table16["levelOverride"] = levelOverride.LevelOverride;
			table16["useLevelOverride"] = levelOverride.UseLevelOverride;
		}
		if (brushSettings is IValidate)
		{
			IValidate validate = brushSettings as IValidate;
			Table table18 = (Table)(table["validate"] = DynValue.NewPrimeTable().Table);
			table18["validate"] = validate.Validate;
			table18["validationName"] = validate.ValidationName;
		}
		return result;
	}

	public static void SetLuaBrushSettings(DynValue d)
	{
		BrushSettings brushSettings = GetBrushSettings();
		if (brushSettings != null)
		{
			Table table = d.Table;
			if (brushSettings != null)
			{
				ISize size = brushSettings;
				size.Size = (float)table.Get("size").Table.Get("size").Number;
			}
			if (brushSettings is IIntensity)
			{
				IIntensity intensity = brushSettings as IIntensity;
				intensity.Intensity = (float)table.Get("intensity").Table.Get("intensity").Number;
			}
			if (brushSettings is ICreateIsland)
			{
				ICreateIsland createIsland = brushSettings as ICreateIsland;
				createIsland.DontCreate = table.Get("createIsland").Table.Get("dontCreate").Boolean;
				createIsland.DontLower = table.Get("createIsland").Table.Get("dontLower").Boolean;
				createIsland.DontRaise = table.Get("createIsland").Table.Get("dontRaise").Boolean;
			}
			if (brushSettings is ILockHeight)
			{
				ILockHeight lockHeight = brushSettings as ILockHeight;
				lockHeight.LockHeight = table.Get("lockHeight").Table.Get("lockHeight").Boolean;
			}
			if (brushSettings is ITerrainType)
			{
				ITerrainType terrainType = brushSettings as ITerrainType;
				terrainType.TerrainType = (int)table.Get("terrainType").Table.Get("terrainType").Number;
			}
			if (brushSettings is IPlantType)
			{
				IPlantType plantType = brushSettings as IPlantType;
				plantType.PlantType = (int)table.Get("plantType").Table.Get("plantType").Number;
			}
			if (brushSettings is IDistanceToEdge)
			{
				IDistanceToEdge distanceToEdge = brushSettings as IDistanceToEdge;
				distanceToEdge.DistanceToEdge = (int)table.Get("distanceToEdge").Table.Get("distanceToEdge").Number;
			}
			if (brushSettings is ILevelOverride)
			{
				ILevelOverride levelOverride = brushSettings as ILevelOverride;
				levelOverride.LevelOverride = (int)table.Get("levelOverride").Table.Get("levelOverride").Number;
				levelOverride.UseLevelOverride = table.Get("levelOverride").Table.Get("useLevelOverride").Boolean;
			}
			if (brushSettings is IValidate)
			{
				IValidate validate = brushSettings as IValidate;
				validate.Validate = table.Get("validate").Table.Get("validate").Boolean;
			}
			AfterChangeBrushSettingsSignal.Invoke();
		}
	}

	public static Brush GetBrushFromPickerKey(Factory.AssetKey pickerKey)
	{
		foreach (KeyValuePair<Brush, Factory.AssetKey> item in PickerKeyMap)
		{
			if (item.Value == pickerKey)
			{
				return item.Key;
			}
		}
		return Brush.None;
	}

	public static string GetLabelFromBrush(Brush brush)
	{
		return (!LabelMap.ContainsKey(brush)) ? string.Empty : LabelMap[brush];
	}

	public static Sprite GetIconFromBrush(Brush brush)
	{
		return (!IconMap.ContainsKey(brush)) ? null : IconMap[brush];
	}

	public static string GetInfoFromSubBrush(SubBrush subBrush)
	{
		return (!InfoMap.ContainsKey(subBrush)) ? string.Empty : InfoMap[subBrush];
	}

	public static string GetHelpFromSubBrush(SubBrush subBrush)
	{
		return (!HelpMap.ContainsKey(subBrush)) ? string.Empty : HelpMap[subBrush];
	}

	public static Brush ToBrush(SubBrush subBrush)
	{
		return BrushMap[subBrush];
	}

	public static SubBrush ToSubBrush(Brush brush, SubBrushPrefix subBrushPrefix = SubBrushPrefix.Main)
	{
		if (SubBrushMap.ContainsKey(brush))
		{
			Dictionary<SubBrushPrefix, SubBrush> dictionary = SubBrushMap[brush];
			if (dictionary.ContainsKey(subBrushPrefix))
			{
				return dictionary[subBrushPrefix];
			}
			return dictionary[SubBrushPrefix.Main];
		}
		return SubBrush.None;
	}

	public static Dictionary<SubBrushPrefix, SubBrush> ToSubBrushPrefixMap(SubBrush subBrush)
	{
		return SubBrushMap[ToBrush(subBrush)];
	}

	public static string GetInfoMessageFromSubBrush(SubBrush subBrush)
	{
		Dictionary<SubBrushPrefix, SubBrush> dictionary = ToSubBrushPrefixMap(subBrush);
		TmpMessageList.Clear();
		foreach (SubBrushPrefix key in dictionary.Keys)
		{
			SubBrush subBrush2 = dictionary[key];
			string text = I2Helper.TryGet(GetInfoFromSubBrush(subBrush2));
			if (text == string.Empty || text == "*")
			{
				continue;
			}
			if (CurrentBrush != Brush.Underside)
			{
				text = ((key != SubBrushPrefix.Main) ? $"[{SubBrushPrefixNameMap[key]} + Click] {text}" : $"[Click] {text}");
				if (subBrush2 == CurrentSubBrush)
				{
					text = $"<color=white>{text}</color>";
				}
			}
			else
			{
				text = $"<color=white>{text}</color>";
			}
			TmpMessageList.Add(text);
		}
		return string.Join("\n", TmpMessageList.ToArray());
	}

	public static void OnOverridePickerSetWithOptions()
	{
		SubBrush subBrush = SubBrush.None;
		if (Picking.Instance == null)
		{
			return;
		}
		if (Picking.Instance.OverridePicker == null && CurrentBrush != Brush.Underside)
		{
			ReceiveSubBrush(subBrush);
		}
		else if (Picking.Instance.OverridePicker != null && Picking.Instance.OverridePicker.mainComponent is BrushComponent)
		{
			Factory.AssetKey assetKey = Picking.Instance.OverridePicker.AssetKey;
			Brush brushFromPickerKey = GetBrushFromPickerKey(assetKey);
			SubBrushPrefix subBrushPrefix = SubBrushPrefix.Main;
			subBrushPrefix = ((!Picking.Instance.PickingObserver.IsSecondaryAction) ? subBrushPrefix : SubBrushPrefix.Shift);
			subBrushPrefix = (Picking.Instance.PickingObserver.IsAltAction ? SubBrushPrefix.Ctrl : subBrushPrefix);
			subBrush = ToSubBrush(brushFromPickerKey, subBrushPrefix);
			if (subBrush != SubBrush.None && CurrentBrush != Brush.Underside)
			{
				ReceiveSubBrush(subBrush);
			}
		}
	}

	public static SubBrushPrefix GetSubBrushPrefix()
	{
		Picking instance = Picking.Instance;
		if (instance == null)
		{
			return SubBrushPrefix.Main;
		}
		Picker overridePicker = instance.OverridePicker;
		if (overridePicker == null)
		{
			return SubBrushPrefix.Main;
		}
		if (instance.OverridePicker.CurrentPickerComponent == instance.OverridePicker.mainComponent)
		{
			return SubBrushPrefix.Main;
		}
		if (instance.OverridePicker.CurrentPickerComponent == instance.OverridePicker.ctrlComponent)
		{
			return SubBrushPrefix.Ctrl;
		}
		if (instance.OverridePicker.CurrentPickerComponent == instance.OverridePicker.shiftComponent)
		{
			return SubBrushPrefix.Shift;
		}
		return SubBrushPrefix.Main;
	}

	public static void OnPickerCompActivated(PickerComponent activatedComponent)
	{
		if ((bool)Picking.Instance.OverridePicker)
		{
			Brush brushFromPickerKey = GetBrushFromPickerKey(Picking.Instance.OverridePicker.AssetKey);
			ReceiveSubBrush(ToSubBrush(brushFromPickerKey, GetSubBrushPrefix()));
		}
	}
}
