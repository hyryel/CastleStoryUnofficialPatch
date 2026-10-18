using System;
using System.Collections.Generic;
using System.Text;
using Brix.Assets;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Compounders;
using Brix.Game.Network;
using Brix.Game.Rules;
using Brix.Game.Semantique;
using Brix.Input;
using Brix.Input.WorldEditor;
using Brix.Legacy;
using Brix.Network;
using Brix.NewUI.CornerMenus;
using Brix.UI;
using Brix.UI.Builder.Menu;
using Brix.UI.GameSelector;
using Brix.UI.Icons;
using Brix.Utils.Factories;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Smooth.Slinq;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Brix.Lua;

public class LuaLibs
{
	private static BrixLogger logger;

	private static List<Type> _userDataRegistered;

	private static List<Type> _scriptToClrRegistered;

	private static List<Type> _clrToScriptRegistered;

	private static Signal _OnSceneChanged;

	private static LuaEnumDescriptor<Occupation.Type> _enumOccupationType;

	private static LuaEnumDescriptor<Occupation.Job> _enumOccupationJob;

	private static LuaEnumDescriptor<Diggables> _enumDiggables;

	private static LuaEnumDescriptor<TaskType> _enumTaskType;

	private static LuaEnumDescriptor<Affiliation.Relation> _enumRelations;

	private static LuaEnumDescriptor<DamageType> _enumDamageType;

	private static LuaEnumDescriptor<SelectionType> _enumSelectionType;

	private static LuaEnumDescriptor<NatureType> _enumNatureType;

	private static LuaEnumDescriptor<VoxelTerrainType> _enumTerrainType;

	public static LuaObjectDescriptor<Sprite> spriteDesc;

	private static DynValue _BeginSample;

	private static DynValue _EndSample;

	private static DynValue IsNilOrReleased;

	private static DynValue DisplayStartDialog;

	private static DynValue DisplayDialog;

	private static DynValue DisplayOkDialog;

	public static DynValue LuaPrint;

	static LuaLibs()
	{
		logger = new BrixLogger(typeof(LuaLibs).FullName);
		_userDataRegistered = new List<Type>();
		_scriptToClrRegistered = new List<Type>();
		_clrToScriptRegistered = new List<Type>();
		_OnSceneChanged = new Signal();
		_enumOccupationType = new LuaEnumDescriptor<Occupation.Type>();
		_enumOccupationJob = new LuaEnumDescriptor<Occupation.Job>();
		_enumDiggables = new LuaEnumDescriptor<Diggables>();
		_enumTaskType = new LuaEnumDescriptor<TaskType>();
		_enumRelations = new LuaEnumDescriptor<Affiliation.Relation>();
		_enumDamageType = new LuaEnumDescriptor<DamageType>();
		_enumSelectionType = new LuaEnumDescriptor<SelectionType>();
		_enumNatureType = new LuaEnumDescriptor<NatureType>();
		_enumTerrainType = new LuaEnumDescriptor<VoxelTerrainType>();
		spriteDesc = new LuaObjectDescriptor<Sprite>();
		_BeginSample = DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => DynValue.Void);
		_EndSample = DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => DynValue.Void);
		IsNilOrReleased = DynValue.NewCallback(_IsNilOrReleased);
		DisplayStartDialog = DynValue.NewCallback(_DisplayStartDialog);
		DisplayDialog = DynValue.NewCallback(_DisplayDialog);
		DisplayOkDialog = DynValue.NewCallback(_DisplayOkDialog);
		LuaPrint = DynValue.NewCallback(_LuaPrint);
		SceneManager.activeSceneChanged += delegate
		{
			_OnSceneChanged.Invoke();
		};
	}

	public static void LuaExceptionCatcher(Exception e)
	{
		if (e is InterpreterException)
		{
			InterpreterException ex = (InterpreterException)e;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(ex.DecoratedMessage);
			if (ex.CallStack != null)
			{
				for (int i = 0; i != ex.CallStack.Count; i++)
				{
					stringBuilder.AppendLine(ex.CallStack[i].ToString());
				}
				Debug.LogError(stringBuilder.ToString());
			}
			else
			{
				logger.Error(e);
				Debug.LogException(e);
			}
		}
		else
		{
			logger.Error(e);
			Debug.LogException(e);
		}
	}

	public static DynValue CreateLuaDescribable(LuaDriver ld)
	{
		if (ld == null)
		{
			return DynValue.Nil;
		}
		return UserData.CreateWithDescriptor(ld, LuaDriver.desc);
	}

	public static DynValue CreateLuaDescribable(Component c)
	{
		if (c == null)
		{
			return DynValue.Nil;
		}
		LuaDriver component = c.GetComponent<LuaDriver>();
		if (component == null)
		{
			return DynValue.Nil;
		}
		return UserData.CreateWithDescriptor(component, LuaDriver.desc);
	}

	public static DynValue CreateLuaDescribable(GameObject go)
	{
		if (go == null)
		{
			return DynValue.Nil;
		}
		LuaDriver component = go.GetComponent<LuaDriver>();
		if (component == null)
		{
			return DynValue.Nil;
		}
		return UserData.CreateWithDescriptor(component, LuaDriver.desc);
	}

	public static Dictionary<object, object> SerialiseTable(Table table)
	{
		if (table != null)
		{
			try
			{
				Dictionary<object, object> dictionary = new Dictionary<object, object>(table.Length);
				foreach (TablePair pair in table.Pairs)
				{
					object obj = pair.Value.ToObject<object>();
					if (obj != null && obj.GetType() == typeof(Table))
					{
						obj = SerialiseTable((Table)obj);
					}
					dictionary.Add(pair.Key.ToObject<object>(), obj);
				}
				return dictionary;
			}
			catch (Exception message)
			{
				logger.Error(message);
			}
		}
		return null;
	}

	public static Table DeserialiseTable(Dictionary<object, object> dictionary, Script targetScript)
	{
		if (dictionary != null)
		{
			Table table = new Table(targetScript);
			{
				foreach (KeyValuePair<object, object> item in dictionary)
				{
					object obj = item.Value;
					if (obj != null && obj.GetType() == typeof(Dictionary<object, object>))
					{
						obj = DeserialiseTable((Dictionary<object, object>)obj, targetScript);
					}
					table[item.Key] = DynValue.FromObject(targetScript, obj);
				}
				return table;
			}
		}
		return null;
	}

	public static IUserDataDescriptor RegisterUserData<T>(InteropAccessMode accessMode = InteropAccessMode.Default, string friendlyName = null)
	{
		Type typeFromHandle = typeof(T);
		IUserDataDescriptor result = null;
		if (!_userDataRegistered.Contains(typeFromHandle))
		{
			result = UserData.RegisterType<T>(accessMode, friendlyName);
			_userDataRegistered.Add(typeFromHandle);
		}
		return result;
	}

	public static IUserDataDescriptor RegisterUserData(Type type, InteropAccessMode accessMode = InteropAccessMode.Default, string friendlyName = null)
	{
		IUserDataDescriptor result = null;
		if (!_userDataRegistered.Contains(type))
		{
			result = UserData.RegisterType(type, accessMode, friendlyName);
			_userDataRegistered.Add(type);
		}
		return result;
	}

	public static void RegisterScriptToClr<T>(DataType datatype, Func<DynValue, Type, object> conversion)
	{
		Type typeFromHandle = typeof(T);
		if (!_scriptToClrRegistered.Contains(typeFromHandle))
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(datatype, typeFromHandle, conversion);
			_scriptToClrRegistered.Add(typeFromHandle);
		}
	}

	public static void RegisterClrToScript<T>(Func<Script, T, DynValue> conversion)
	{
		Type typeFromHandle = typeof(T);
		if (!_clrToScriptRegistered.Contains(typeFromHandle))
		{
			Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(conversion);
			_clrToScriptRegistered.Add(typeFromHandle);
		}
	}

	public static void BigLib(Script script)
	{
		LuaTimer.RegisterClass(script);
		LuaMarker.RegisterClass(script);
		LuaTexture.RegisterClass(script);
		LuaMaterial.RegisterClass(script);
		LuaMesh.RegisterClass(script);
		LuaCrafting.Register(script);
		CraftingStation.LuaRegister(script);
		script.Globals["Occupations"] = _enumOccupationType.CreateDynValue();
		script.Globals["Jobs"] = _enumOccupationJob.CreateDynValue();
		script.Globals["Diggables"] = _enumDiggables.CreateDynValue();
		script.Globals["TaskType"] = _enumTaskType.CreateDynValue();
		script.Globals["Relation"] = _enumRelations.CreateDynValue();
		script.Globals["DamageType"] = _enumDamageType.CreateDynValue();
		script.Globals["SelectionType"] = _enumSelectionType.CreateDynValue();
		script.Globals["DriverTag"] = LuaDriver.descLuaDriverTag.CreateDynValue();
		script.Globals["NatureType"] = _enumNatureType.CreateDynValue();
		script.Globals["TerrainType"] = _enumTerrainType.CreateDynValue();
		RegisterHooks(script);
		RegisterPerf(script);
		RegisterUnity(script);
		RegisterInputs(script);
		RegisterUI(script);
		RegisterLoading(script);
		RegisterHacks(script);
		LuaDriver.RegisterClass(script);
		LuaStaticDriver.RegisterClass(script);
		script.DoFile("Coroutines/Yielders.lua");
	}

	public static void RegisterUnity(Script script)
	{
		LuaMath.RegisterClass(script);
		LuaVector2.RegisterClass(script);
		LuaVector3.RegisterClass(script);
		LuaVector4.RegisterClass(script);
		LuaQuaternion.RegisterClass(script);
		LuaTexture.RegisterClass(script);
		LuaColor.RegisterClass(script);
		LuaFactory.RegisterClass(script);
		LuaAssetKey.RegisterClass(script);
		LuaPhysics.RegisterClass(script);
		LuaRandom.RegisterClass(script);
		LuaMatrix4x4.RegisterClass(script);
		Project.RegisterClass(script);
		FireflyNest.RegisterClass(script);
		FactionStorage.RegisterClass(script);
		MiniMap.RegisterClass(script);
		InputModeController.RegisterClass(script);
		UISelectable.RegisterClass(script);
		Recepteur.RegisterClass(script);
		BlueprintCompounder.RegisterClass(script);
		LuaMetaIVoxel.RegisterClass(script);
	}

	public static void RegisterHooks(Script script)
	{
		LuaHookBase.RegisterClass(script);
		script.Globals["hk_OnFactoriesReady"] = new LuaHook(FactorySignalInvoker.OnFactoriesReadySig);
		script.Globals["hk_OnBeforeTerrainLoaded"] = new LuaHook(GameSignals.BeforeTerrainLoaded);
		script.Globals["hk_OnGameObjectsLoaded"] = new LuaHook(GameSignals.GameObjectsLoaded);
		script.Globals["hk_OnLevelLoaded"] = new LuaHook(GameSignals.LevelLoaded);
		script.Globals["hk_OnLevelReady"] = new LuaHook(GameSignals.LevelReady);
		script.Globals["hk_OnNewLevelLoaded"] = new LuaHook(GameSignals.LevelFirstLoaded);
		script.Globals["hk_OnNewLevelReady"] = new LuaHook(GameSignals.LoadGamemode);
		script.Globals["hk_OnSaveLevelLoaded"] = new LuaHook(GameSignals.LevelSaveLoaded);
		script.Globals["hk_OnSaveLevelReady"] = new LuaHook(GameSignals.LevelSaveReady);
		script.Globals["hk_OnTerrainReady"] = new LuaHook(GameSignals.TerrainReady);
		script.Globals["hk_DisclaimerAcknowledged"] = new LuaHook(GameSignals.DisclaimerAcknowledged);
		script.Globals["hk_OnLaborCreated"] = new LuaHook<Labor>(Labor.LaborStarted);
		script.Globals["hk_OnLaborDeleted"] = new LuaHook<Labor>(Labor.LaborDeleted);
		script.Globals["hk_OnPlayerLost"] = new LuaHook<User>(GameSignals.PlayerLost);
		script.Globals["hk_OnFactoryInstantiated"] = new LuaHook<Factory.AssetKey, GameObject>(Factory.GameObjectCreatedSignal);
		script.Globals["hk_OnAdditiveMapLoaded"] = new LuaHook(GameSignals.AdditiveMapLoaded);
		script.Globals["hk_CallEndGame"] = new LuaHook(GameSignals.GameOver);
		script.Globals["hk_OnGameModeTimersStarted"] = new LuaHook(GameSignals.GameModeTimersStarted);
		script.Globals["hk_OnSceneChanged"] = new LuaHook(_OnSceneChanged);
		script.Globals["hk_OnLeaveGame"] = new LuaHook(UNetManager.Disconnected);
		script.Globals["hk_OnLoadUI"] = new LuaHook(GameSignals.LoadUI);
		script.Globals["hk_NewWave"] = new LuaHook<int>(StatsTracker.OnInvasionWave);
		script.Globals["hk_OnVictory"] = new LuaHook(StatsTracker.OnGameWon);
		script.Globals["hk_OnDefeat"] = new LuaHook(StatsTracker.OnGameLost);
	}

	public static void RegisterUI(Script script)
	{
		IconDB.RegisterClass(script);
		LuaMenuUtility.RegisterClass(script);
		MenuCalls.RegisterClass(script);
		script.Globals["DisplayOkDialog"] = DisplayOkDialog;
		script.Globals["DisplayDialog"] = DisplayDialog;
		script.Globals["DisplayStartDialog"] = DisplayStartDialog;
	}

	public static void RegisterPerf(Script script)
	{
		script.Globals.Set("BeginSample", _BeginSample);
		script.Globals.Set("EndSample", _EndSample);
	}

	public static void RegisterInputs(Script script)
	{
		script.Globals["Bind"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.Void);
		script.Globals["Unbind"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.Void);
		script.Globals["UnbindAll"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.Void);
	}

	public static void RegisterLoading(Script script)
	{
		script.Globals["FlushDrivers"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			for (int i = 0; i != args.Count; i++)
			{
				LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(i, "FlushDrivers");
				luaStaticDriver.ClearInstances(hard: true);
			}
			return DynValue.Void;
		});
		script.Globals["FlushTags"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			for (int i = 0; i != args.Count; i++)
			{
				LuaDriverTag luaDriverTag = args.AsEnum<LuaDriverTag>(i, "FlushTags");
				foreach (KeyValuePair<string, LuaStaticDriver> allStatic in LuaDriverHQ.AllStatics)
				{
					if (allStatic.Value.tag == luaDriverTag)
					{
						allStatic.Value.ClearInstances(hard: true);
					}
				}
			}
			return DynValue.Void;
		});
		script.Globals["SwitchToGamemode"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			string codename = args.AsString(0, "SwitchToGamemode");
			GameParam.Gamemode = AssetDispensary.gamemodes.SendCodenameQuery(codename);
			if (args.Count >= 2)
			{
				string presetName = args.AsString(1, "SwitchToGamemode");
				GameParam.Preset = GameParam.Gamemode.GetPresets().Slinq().FirstOrDefault((Asset_GamemodePreset x) => x.codename == presetName);
			}
			else
			{
				GameParam.Preset = GameParam.Gamemode.GetDefaultPreset();
			}
			LuaDriverHQ.AllStatics["fy_Bricktron"].ClearInstances(hard: true);
			LuaDriverHQ.AllStatics["fy_Minitron"].ClearInstances(hard: true);
			LuaDriverHQ.AllStatics["fy_Corruptron"].ClearInstances(hard: true);
			LuaDriverHQ.AllStatics["fy_Biftron"].ClearInstances(hard: true);
			LuaDriverHQ.AllStatics["fy_Firefly"].ClearInstances(hard: true);
			LuaDriverHQ.AllStatics["fy_Crystal"].ClearInstances(hard: true);
			LuaDriverHQ.AllStatics["fy_Bricktron"].ClearInstances(hard: true);
			GameParam.Gamemode.Disk_LoadGamemode(out var gamemodeScript, GameParam.Preset);
			GameSignals.Connect(GameSignals.LeaveGame, delegate
			{
				Script.Kill(ref gamemodeScript);
			});
			return DynValue.Void;
		});
		script.Globals["ConnectLifecycle"] = LuaRegistry.cb_connectLifecycle;
		script.Globals["DisconnectLifecycle"] = LuaRegistry.cb_disconnectLifecycle;
		script.Globals["SetSaveData"] = LuaRegistry.cb_setSaveData;
		script.Globals["GetSaveData"] = LuaRegistry.cb_getSaveData;
		script.Globals["IsNilOrReleased"] = IsNilOrReleased;
	}

	public static void RegisterHacks(Script script)
	{
		script.Globals["IsMultiplayer"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.NewBoolean(Neo.IsMultiplayer));
		script.Globals["IsWorldEditor"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.NewBoolean(InputModeController.DefaultMode == InputMode.worldEditor));
		script.Globals["IsServer"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.NewBoolean(NetworkServer.active));
		script.Globals["IsPathfindingEnabled"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.NewBoolean(Architecte.isPathfindingEnabled));
		script.Globals["SetIsPathfindingEnabled"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			Architecte.isPathfindingEnabled = args.AsBool(0, "SetIsPathfindingEnabled");
			return DynValue.Void;
		});
		script.Globals["IsIdleProjectSpawnEnabled"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.NewBoolean(IdleProjectInstatiator.isAutoSpawnEnabled));
		script.Globals["SetIsIdleProjectSpawnEnabled"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			IdleProjectInstatiator.isAutoSpawnEnabled = args.AsBool(0, "SetIsIdleProjectSpawnEnabled");
			return DynValue.Void;
		});
		script.Globals["GetStaticDriversTagged"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			LuaDriverTag tag = args.AsEnum<LuaDriverTag>(0, "GetStaticDriversTagged");
			Dictionary<string, LuaStaticDriver> allStatics = LuaDriverHQ.AllStatics;
			return DynValue.FromObject(context.OwnerScript, (from kvp in allStatics.Slinq()
				where kvp.Value.tag == tag
				select kvp.Value).ToList());
		});
		script.Globals["IsBuildDebug"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.False);
		script.Globals["AbortLoading"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			string t = args.AsString(0, "AbortLoading");
			GameSignals.Invoke(GameSignals.StartError, t);
			Neo.Relais(delegate
			{
				GameSignals.Invoke(GameSignals.GameOver);
			}, 2f, "gameover on abort");
			return DynValue.Void;
		});
		script.Globals["BREAK"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.Void);
		script.Globals["TreePresentAt"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			Vector3 v = args.AsUserData<Vector3>(0, "TreePresentAt");
			return DynValue.NewBoolean(TreeRegistry.Instance.HasTree(XYZ.FromVector3(v)) || DamagedTree.AllDamagedTrees.ContainsKey(XYZ.FromVector3(v)));
		});
		script.Globals["BlockPresentAt"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			Vector3 v = args.AsUserData<Vector3>(0, "BlockPresentAt");
			return DynValue.NewBoolean(Voxel.IsBlock(XYZ.FromVector3(v)));
		});
		script.Globals["BlueprintPresentAt"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			Vector3 v = args.AsUserData<Vector3>(0, "BlueprintPresentAt");
			return DynValue.NewBoolean(VolumeRegistry.HasVolumesWithPropertyAt(XYZ.FromVector3(v), VProperty.fantome));
		});
		script.Globals["ToggleEditorMode"] = (Action)delegate
		{
			Architecte.editorMode = !Architecte.editorMode;
			GameSignals.Invoke(GameSignals.OnEditorModeChanged, Architecte.editorMode);
		};
		script.Globals["ToggleAllowDebugMenu"] = (Action)delegate
		{
			Architecte.allowDebugMenu = !Architecte.allowDebugMenu;
		};
		script.Globals["AllowsDebugMenu"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.NewBoolean(Architecte.allowDebugMenu));
		script.Globals["showHelpAtGameStart"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.NewBoolean(Neo.showHelpAtGameStart));
		script.Globals["TogglePause"] = (Action)delegate
		{
			ClockConfig.Paused = !ClockConfig.Paused;
		};
		script.Globals["ToggleHalfSpeed"] = (Action)delegate
		{
			ClockConfig.ToggleClockSpeed(ClockConfig.ClockSpeed.Half);
		};
		script.Globals["ToggleTenthSpeed"] = (Action)delegate
		{
			ClockConfig.ToggleClockSpeed(ClockConfig.ClockSpeed.Tenth);
		};
		script.Globals["ToggleFastSpeed"] = (Action)delegate
		{
			ClockConfig.ToggleClockSpeed(ClockConfig.ClockSpeed.Fast);
		};
		script.Globals["ToggleDoubleSpeed"] = (Action)delegate
		{
			ClockConfig.ToggleClockSpeed(ClockConfig.ClockSpeed.Double);
		};
		script.Globals["ToggleVeryFastSpeed"] = (Action)delegate
		{
			ClockConfig.ToggleClockSpeed(ClockConfig.ClockSpeed.VeryFast);
		};
		script.Globals["ResetClockSpeed"] = (Action)delegate
		{
			ClockConfig.SetClockSpeed(ClockConfig.ClockSpeed.Default);
		};
		script.Globals["SetExplosionPicker"] = (Action)delegate
		{
			Picking.Instance.SendPicker(Pickers.Explosion);
		};
		script.Globals["SetArrowPicker"] = (Action)delegate
		{
			Picking.Instance.SendPicker(Pickers.ArrowPicker);
		};
		script.Globals["SetRegularPicker"] = (Action)delegate
		{
			Picking.Instance.ClearPick();
		};
		script.Globals["TabulaRasa"] = (Action)delegate
		{
			Neo.Vidanges();
		};
		script.Globals["StepOneSecond"] = (Action)delegate
		{
			Neo.StepOneSecond();
		};
		script.Globals["BeginProfiling"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			string text = args.AsString(0, "BeginProfiling");
			return DynValue.Void;
		});
		script.Globals["EndProfiling"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.Void);
		script.Globals["SetAchievement"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			if (SteamManager.Initialized)
			{
				string achievement = args.AsString(0, "SetAchievement");
				SteamUserStats.SetAchievement(achievement);
				SteamUserStats.StoreStats();
			}
			return DynValue.Void;
		});
		script.Globals["ClearAchievement"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			if (SteamManager.Initialized)
			{
				string pchName = args.AsString(0, "ClearAchievement");
				SteamUserStats.ClearAchievement(pchName);
				SteamUserStats.StoreStats();
			}
			return DynValue.Void;
		});
		script.Globals["StepOneTenth"] = (Action)delegate
		{
			Neo.StepOneTenth();
		};
		script.Globals["ToggleMaxLodBias"] = (Action)delegate
		{
			if (!Neo.lodSetToExtreme)
			{
				int terrainLodQuality = Neo.terrainLodQuality;
				Neo.terrainLodQuality = Neo.alternateLodQuality;
				Neo.alternateLodQuality = terrainLodQuality;
				Neo.lodSetToExtreme = !Neo.lodSetToExtreme;
			}
			else
			{
				Neo.terrainLodQuality = Neo.alternateLodQuality;
				Neo.alternateLodQuality = 3;
				Neo.lodSetToExtreme = !Neo.lodSetToExtreme;
			}
		};
		script.Globals["AdjustFogOpacity"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			float opacity = args.AsFloat(0, "AdjustFogOpacity");
			FogOfWarRenderer.Opacity = opacity;
			return DynValue.Void;
		});
		script.Globals["SetFogStartDistance"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			float fogStartDistance = args.AsFloat(0, "SetFogDistance");
			RenderSettings.fogStartDistance = fogStartDistance;
			return DynValue.Void;
		});
		script.Globals["SetFogEndDistance"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			float fogEndDistance = args.AsFloat(0, "SetFogDistance");
			RenderSettings.fogEndDistance = fogEndDistance;
			return DynValue.Void;
		});
		script.Globals["GetFogStartDistance"] = DynValue.NewCallback(delegate
		{
			float fogStartDistance = RenderSettings.fogStartDistance;
			return DynValue.NewNumber(fogStartDistance);
		});
		script.Globals["GetFogEndDistance"] = DynValue.NewCallback(delegate
		{
			float fogEndDistance = RenderSettings.fogEndDistance;
			return DynValue.NewNumber(fogEndDistance);
		});
		script.Globals["SetFogColor"] = DynValue.NewCallback(delegate(ScriptExecutionContext context, CallbackArguments args)
		{
			float r = args.AsFloat(0, "SetFogColor");
			float g = args.AsFloat(1, "SetFogColor");
			float b = args.AsFloat(2, "SetFogColor");
			RenderSettings.fogColor = new Color(r, g, b, 1f);
			return DynValue.Void;
		});
		script.Globals["GetFogColor"] = DynValue.NewCallback(delegate
		{
			DynValue value = DynValue.NewNumber(RenderSettings.fogColor.r);
			DynValue value2 = DynValue.NewNumber(RenderSettings.fogColor.r);
			DynValue value3 = DynValue.NewNumber(RenderSettings.fogColor.r);
			return DynValue.NewTuple(value, value2, value3);
		});
		script.Globals["GetBiomeName"] = DynValue.NewCallback((ScriptExecutionContext context, CallbackArguments args) => DynValue.NewString(GameParam.GetBiomeName()));
		script.Globals["ToggleHideGizmos"] = (Action)delegate
		{
			Camera.main.GetComponent<CameraScript>().Cinematique = !Camera.main.GetComponent<CameraScript>().Cinematique;
		};
		script.Globals["ToggleDebugCamera"] = (Action)delegate
		{
			if (!Camera.main.GetComponent<CameraScript>().editorCamera)
			{
				Camera.main.GetComponent<CameraScript>().ToggleEditorCamera(value: true);
			}
			else
			{
				Camera.main.GetComponent<CameraScript>().ToggleEditorCamera(value: false);
			}
		};
		script.Globals["MemoryCollection"] = (Action)delegate
		{
			Vidangeur.CollecteManuelle(ramasseUnity: true, ramassePool: true, ramasseDotNet: true, ramasseOutputLog: true);
		};
		Table table = new Table(script);
		table.Set("GrowIron", DynValue.NewCallback(delegate(ScriptExecutionContext con, CallbackArguments args)
		{
			Vector3 center = args.AsUserData<Vector3>(0, "GrowIron");
			int areaSize = args.AsInt(1, "GrowIron");
			float intensity = (float)args.AsType(2, "GrowIron", DataType.Number).Number;
			bool boolean = args.AsType(3, "GrowIron", DataType.Boolean).Boolean;
			IronBrush.GrowIron(center, areaSize, intensity, boolean);
			return DynValue.Void;
		}));
		table.Set("GrowPyrite", DynValue.NewCallback(delegate(ScriptExecutionContext con, CallbackArguments args)
		{
			Vector3 center = args.AsUserData<Vector3>(0, "GrowPyrite");
			int areaSize = args.AsInt(1, "GrowPyrite");
			float intensity = (float)args.AsType(2, "GrowPyrite", DataType.Number).Number;
			bool boolean = args.AsType(3, "GrowPyrite", DataType.Boolean).Boolean;
			PyriteBrush.GrowPyrite(center, areaSize, intensity, boolean);
			return DynValue.Void;
		}));
		table.Set("GrowBoulders", DynValue.NewCallback(delegate(ScriptExecutionContext con, CallbackArguments args)
		{
			Vector3 center = args.AsUserData<Vector3>(0, "GrowBoulders");
			int areaSize = args.AsInt(1, "GrowBoulders");
			float intensity = (float)args.AsType(2, "GrowBoulders", DataType.Number).Number;
			bool boolean = args.AsType(3, "GrowBoulders", DataType.Boolean).Boolean;
			BoulderBrush.GrowBoulders(center, areaSize, intensity, boolean);
			return DynValue.Void;
		}));
		table.Set("GrowPlants", DynValue.NewCallback(delegate(ScriptExecutionContext con, CallbackArguments args)
		{
			Vector3 center = args.AsUserData<Vector3>(0, "GrowPlants");
			int plantType = args.AsInt(1, "GrowPlants");
			int areaSize = args.AsInt(2, "GrowPlants");
			float intensity = (float)args.AsType(3, "GrowPlants", DataType.Number).Number;
			bool boolean = args.AsType(4, "GrowPlants", DataType.Boolean).Boolean;
			bool boolean2 = args.AsType(5, "GrowPlants", DataType.Boolean).Boolean;
			PlantBrush.GrowPlants(center, plantType, areaSize, intensity, boolean, boolean2);
			return DynValue.Void;
		}));
		table.Set("GrowTrees", DynValue.NewCallback(delegate(ScriptExecutionContext con, CallbackArguments args)
		{
			Vector3 center = args.AsUserData<Vector3>(0, "GrowTrees");
			int areaSize = args.AsInt(1, "GrowTrees");
			float intensity = (float)args.AsType(2, "GrowTrees", DataType.Number).Number;
			bool boolean = args.AsType(3, "GrowTrees", DataType.Boolean).Boolean;
			TreeBrush.GrowTrees(center, areaSize, intensity, boolean);
			return DynValue.Void;
		}));
		script.Globals["Brush"] = DynValue.NewTable(table);
	}

	private static DynValue _IsNilOrReleased(ScriptExecutionContext context, CallbackArguments args)
	{
		if (args.Count != 1)
		{
			return DynValue.True;
		}
		DynValue dynValue = args.RawGet(0, translateVoids: true);
		if (dynValue.IsNil())
		{
			return DynValue.True;
		}
		if (dynValue.Type == DataType.UserData)
		{
			if (dynValue.UserData == null || !dynValue.UserData.HasValue())
			{
				return DynValue.True;
			}
			if (dynValue.UserData.TryGet<IBrixComponent>(out var param))
			{
				return DynValue.NewBoolean(param.IsNullOrReleased());
			}
		}
		return DynValue.False;
	}

	private static DynValue _DisplayStartDialog(ScriptExecutionContext context, CallbackArguments args)
	{
		string titleText = args.AsString(0, "DisplayStartDialog");
		string contentText = args.AsString(1, "DisplayStartDialog");
		string checkboxText = args.AsString(2, "DisplayStartDialog");
		string okText = args.AsString(3, "DisplayStartDialog");
		Closure checkboxClosure = args.AsType(4, "DisplayStartDialog", DataType.Function).Function;
		Closure okClosure = args.AsType(5, "DisplayStartDialog", DataType.Function).Function;
		Closure checkboxValueClosure = args.AsType(6, "DisplayStartDialog", DataType.Function).Function;
		Action checkboxAction = delegate
		{
			checkboxClosure.Call();
		};
		Action okAction = delegate
		{
			okClosure.Call();
			GameSignals.Invoke(GameSignals.OnCloseDialog);
		};
		Func<bool> checkboxValueFunc = () => checkboxValueClosure.Call().Boolean;
		OkDialogContainerPopulator.StartDialog t = new OkDialogContainerPopulator.StartDialog(titleText, contentText, checkboxText, okText, checkboxAction, okAction, checkboxValueFunc);
		GameSignals.Invoke(GameSignals.OnDisplayStartDialog, t);
		return DynValue.Void;
	}

	private static DynValue _DisplayDialog(ScriptExecutionContext context, CallbackArguments args)
	{
		string t = args.AsString(0, "DisplayDialog");
		string t2 = args.AsString(1, "DisplayDialog");
		Table table = args.AsTable(2, "DisplayDialog");
		Table table2 = args.AsTable(3, "DisplayDialog");
		List<string> list = new List<string>();
		foreach (TablePair pair in table.Pairs)
		{
			list.Add(pair.Value.String);
		}
		List<Action> list2 = new List<Action>();
		foreach (TablePair kvp in table2.Pairs)
		{
			list2.Add(delegate
			{
				kvp.Value.Function.Call();
				GameSignals.Invoke(GameSignals.OnCloseDialog);
			});
		}
		GameSignals.Invoke(GameSignals.OnDisplayDialog, t, t2, list.ToArray(), list2.ToArray());
		return DynValue.Void;
	}

	private static DynValue _DisplayOkDialog(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "DisplayOkDialog");
		string text2 = args.AsString(1, "DisplayOkDialog");
		string t = args.AsString(2, "DisplayOkDialog");
		Closure callback = args.AsType(3, "DisplayOkDialog", DataType.Function).Function;
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = text2.Replace("\\n", "\n");
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Replace("\\n", "\n");
		}
		GameSignals.Invoke(GameSignals.OnDisplayOkDialog, text, text2, t, delegate
		{
			callback.Call();
		});
		return DynValue.Void;
	}

	private static DynValue _LuaPrint(ScriptExecutionContext context, CallbackArguments args)
	{
		DynValue dynValue = args.RawGet(0, translateVoids: true);
		string message = ((dynValue.Type != DataType.String) ? dynValue.ToString() : dynValue.String);
		logger.Info(message);
		Debug.Log(message);
		return DynValue.Void;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
