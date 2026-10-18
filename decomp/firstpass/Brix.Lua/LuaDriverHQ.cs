using System;
using System.Collections.Generic;
using System.Reflection;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.Utils;
using Brix.Utils.Factories;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.Lua;

public static class LuaDriverHQ
{
	private static BrixLogger logger;

	private static BindingFlags _standardFlags;

	private static Dictionary<string, LuaStaticDriver> _allStatics;

	private static Dictionary<string, InfoContainer[]> _allInfos;

	private static List<Type> _allDrivableComponents;

	public static Dictionary<string, LuaStaticDriver> AllStatics
	{
		get
		{
			if (_allStatics == null)
			{
				if (_allInfos == null)
				{
					InitInfoList();
				}
				FactorySignalInvoker.OnFactoriesLoadedSig.LateConnectOnce(CompileFactoryDrivers);
			}
			return _allStatics;
		}
	}

	public static Dictionary<string, InfoContainer[]> AllInfos
	{
		get
		{
			if (_allInfos == null)
			{
				InitInfoList();
			}
			return _allInfos;
		}
	}

	public static List<Type> AllDrivableComponents
	{
		get
		{
			if (_allInfos == null)
			{
				InitInfoList();
			}
			return _allDrivableComponents;
		}
	}

	static LuaDriverHQ()
	{
		logger = new BrixLogger(typeof(LuaDriverHQ).FullName);
		_standardFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
	}

	private static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
	{
		foreach (KeyValuePair<string, LuaStaticDriver> allStatic in AllStatics)
		{
			if (allStatic.Key == "fy_User")
			{
				if (arg1.name == "SceneMenu")
				{
					allStatic.Value.ClearInstances();
				}
			}
			else if (allStatic.Value.baseDriver != null && !allStatic.Value.baseDriver.IsUnique)
			{
				allStatic.Value.ClearInstances();
			}
			allStatic.Value.CleanInstances();
		}
	}

	public static void InitInfoList()
	{
		_allInfos = new Dictionary<string, InfoContainer[]>(1024);
		_allDrivableComponents = new List<Type>(128);
		Assembly[] array = new Assembly[1] { Assembly.Load("Assembly-CSharp-firstpass") };
		Assembly[] array2 = new Assembly[4]
		{
			Assembly.Load("UnityEngine"),
			Assembly.Load("UnityEngine.UI"),
			Assembly.Load("Assembly-CSharp-firstpass"),
			Assembly.Load("Assembly-CSharp")
		};
		Dictionary<Type, Type> dictionary = CollectionPool.RequestDictionary<Type, Type>();
		List<Type> list = CollectionPool.RequestList<Type>();
		List<Type> list2 = CollectionPool.RequestList<Type>();
		Assembly[] array3 = array;
		foreach (Assembly assembly in array3)
		{
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				LuaMetaDrivable[] array4 = (LuaMetaDrivable[])type.GetCustomAttributes(typeof(LuaMetaDrivable), inherit: false);
				if (array4.Length > 0 && typeof(Component).IsAssignableFrom(array4[0].targetType))
				{
					dictionary.Add(array4[0].targetType, type);
				}
			}
		}
		Assembly[] array5 = array2;
		foreach (Assembly assembly2 in array5)
		{
			Type[] types2 = assembly2.GetTypes();
			foreach (Type type2 in types2)
			{
				if (typeof(Component).IsAssignableFrom(type2))
				{
					list.Add(type2);
				}
			}
		}
		list.Add(typeof(LuaStaticDriver));
		foreach (Type item in list)
		{
			bool flag = false;
			Type value = null;
			dictionary.TryGetValue(item, out value);
			MethodInfo[] methods = (value ?? item).GetMethods(_standardFlags);
			MethodInfo[] array6 = methods;
			foreach (MethodInfo methodInfo in array6)
			{
				MethodInfo info = methodInfo;
				LuaDrivable[] array7 = (LuaDrivable[])methodInfo.GetCustomAttributes(typeof(LuaDrivable), inherit: false);
				if (array7 == null || array7.Length <= 0)
				{
					continue;
				}
				if (array7[0].UseExtern)
				{
					info = item.GetMethod(methodInfo.Name, _standardFlags, null, (from x in methodInfo.GetParameters().Slinq()
						select x.ParameterType).ToArray(), null);
				}
				AddInfo(array7[0].Name, InfoContainer.From(info, item));
				flag = true;
			}
			PropertyInfo[] properties = (value ?? item).GetProperties(_standardFlags);
			PropertyInfo[] array8 = properties;
			foreach (PropertyInfo propertyInfo in array8)
			{
				PropertyInfo info2 = propertyInfo;
				LuaDrivable[] array9 = (LuaDrivable[])propertyInfo.GetCustomAttributes(typeof(LuaDrivable), inherit: false);
				if (array9 != null && array9.Length > 0)
				{
					if (array9[0].UseExtern)
					{
						info2 = item.GetProperty(propertyInfo.Name, _standardFlags);
					}
					AddInfo(array9[0].Name, InfoContainer.From(info2, item));
					flag = true;
				}
			}
			FieldInfo[] fields = (value ?? item).GetFields(_standardFlags);
			FieldInfo[] array10 = fields;
			foreach (FieldInfo fieldInfo in array10)
			{
				FieldInfo info3 = fieldInfo;
				LuaDrivable[] array11 = (LuaDrivable[])fieldInfo.GetCustomAttributes(typeof(LuaDrivable), inherit: false);
				if (array11 != null && array11.Length > 0)
				{
					if (array11[0].UseExtern)
					{
						info3 = item.GetField(fieldInfo.Name, _standardFlags);
					}
					AddInfo(array11[0].Name, InfoContainer.From(info3, item));
					flag = true;
				}
			}
			if (flag)
			{
				_allDrivableComponents.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		List<Type> list3 = CollectionPool.RequestList<Type>();
		foreach (Type item2 in list2)
		{
			foreach (Type allDrivableComponent in _allDrivableComponents)
			{
				if (allDrivableComponent.IsAssignableFrom(item2))
				{
					list3.Add(item2);
				}
			}
		}
		_allDrivableComponents.AddRange(list3);
		CollectionPool.Release(ref dictionary);
		CollectionPool.Release(ref list);
		CollectionPool.Release(ref list2);
	}

	public static void CompileFactoryDrivers()
	{
		_allStatics = new Dictionary<string, LuaStaticDriver>(128);
		IEnumerable<Factory> enumerable = null;
		if (Application.isPlaying)
		{
			enumerable = Factory.GetFactories();
		}
		else
		{
			FactoryLoader factoryLoader = UnityEngine.Object.FindObjectOfType<FactoryLoader>();
			if (factoryLoader == null)
			{
				Debug.LogError("No FactoryLoader found in scenes, are you sure PersistentScene is opened?");
			}
			factoryLoader.EditorForcedLoad();
			enumerable = Factory.GetFactories_Editor();
		}
		foreach (Factory item in enumerable)
		{
			LuaDriver[] componentsInChildren = item.GetComponentsInChildren<LuaDriver>(includeInactive: true);
			LuaDriver[] array = componentsInChildren;
			foreach (LuaDriver driver in array)
			{
				AddNewDriver(driver, fromFactory: true);
			}
		}
	}

	private static void AddInfo(string name, InfoContainer info)
	{
		if (!_allInfos.TryGetValue(name, out var value))
		{
			value = new InfoContainer[1] { info };
			_allInfos.Add(name, value);
			return;
		}
		InfoContainer[] array = new InfoContainer[value.Length + 1];
		for (int i = 0; i != value.Length; i++)
		{
			array[i] = value[i];
		}
		array[value.Length] = info;
		_allInfos[name] = array;
	}

	public static bool GetInfos(string name, out InfoContainer[] info)
	{
		return AllInfos.TryGetValue(name, out info);
	}

	public static Component[] GetAllDrivableComponentOf(GameObject gameObject, bool ignoreChildren)
	{
		List<Component> list = new List<Component>();
		Component[] array = ((!ignoreChildren) ? gameObject.GetComponentsInChildren<Component>(includeInactive: true) : gameObject.GetComponents<Component>());
		foreach (Component component in array)
		{
			if (component == null)
			{
				logger.Warn("LuaDriver with Missing Component on gameObject '" + gameObject.name + "'");
				continue;
			}
			Type type = component.GetType();
			if (AllDrivableComponents.Contains(type))
			{
				list.Add(component);
			}
		}
		return list.ToArray();
	}

	public static void AddNewDriver(LuaDriver driver, bool fromFactory = false)
	{
		if (string.IsNullOrEmpty(driver.Handle) && fromFactory)
		{
			logger.Error(string.Concat("LuaDriver on ", driver.gameObject, " in factory has no handle! Not registering."));
			return;
		}
		if (!AllStatics.TryGetValue(driver.Handle, out var value))
		{
			value = new LuaStaticDriver(driver.Handle, driver.luaDriverTag);
			AllStatics.Add(driver.Handle, value);
		}
		if (driver.drivableComponents == null || driver.drivableComponents.Length == 0)
		{
			driver.drivableComponents = GetAllDrivableComponentOf(driver.gameObject, driver.IgnoreChildren);
			for (int i = 0; i != driver.SubHandles.Count; i++)
			{
				LuaDriver.SubHandle subHandle = driver.SubHandles[i];
				if (subHandle.Target is GameObject)
				{
					subHandle.drivableComponents = GetAllDrivableComponentOf((GameObject)subHandle.Target, ignoreChildren: true);
				}
				else if (subHandle.Target is LuaDriver)
				{
					LuaDriver luaDriver = (LuaDriver)subHandle.Target;
					subHandle.drivableComponents = GetAllDrivableComponentOf(luaDriver.gameObject, luaDriver.IgnoreChildren);
				}
				else if (subHandle.Target is Component)
				{
					subHandle.drivableComponents = new Component[1] { (Component)subHandle.Target };
				}
			}
		}
		if (driver.IsUnique || fromFactory)
		{
			value.baseDriver = driver;
		}
		if (fromFactory)
		{
			value.baseDriver = driver;
			if (driver.TryGetComponent<CloneTemplate>(out var component))
			{
				value.AssetKey = component.AssetKey;
			}
			else
			{
				logger.Warn("FactoryTemplate not found on " + driver);
			}
		}
		else
		{
			value.AddInstance(driver);
		}
	}

	public static void RemoveDriver(LuaDriver driver)
	{
		if (driver != null)
		{
			AllStatics[driver.Handle].RemoveInstance(driver);
		}
	}

	public static LuaStaticDriver GetStaticDriver(string handle)
	{
		LuaStaticDriver value = null;
		AllStatics.TryGetValue(handle, out value);
		return value;
	}
}
