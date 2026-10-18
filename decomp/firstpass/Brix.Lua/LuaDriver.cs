using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Network.Util;
using Brix.Utils.Factories;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Lua;

[Serializable]
[DisallowMultipleComponent]
[SerializerTarget(new string[] { "save" })]
[JsonObject(MemberSerialization.OptIn)]
public class LuaDriver : GameComponent, ILuaDriverDescribable
{
	[Serializable]
	public class SubHandle : ILuaDriverDescribable
	{
		[MoonSharpVisible(false)]
		[SerializeField]
		public string Handle;

		[SerializeField]
		[MoonSharpVisible(false)]
		public UnityEngine.Object Target;

		private Dictionary<object, object> _userTable;

		[SerializeField]
		public Component[] drivableComponents;

		[MoonSharpVisible(false)]
		public Dictionary<object, object> UserTable
		{
			get
			{
				if (_userTable == null)
				{
					_userTable = new Dictionary<object, object>();
				}
				return _userTable;
			}
			set
			{
				_userTable = value;
			}
		}

		public T CachedGetComponent<T>()
		{
			Type typeFromHandle = typeof(T);
			for (int i = 0; i != drivableComponents.Length; i++)
			{
				if (drivableComponents[i].GetType() == typeFromHandle)
				{
					return (T)(object)drivableComponents[i];
				}
			}
			return (T)(object)null;
		}

		public bool TargetIsNullOrReleased()
		{
			return Target == null;
		}

		public UnityEngine.Object GetUnityObject()
		{
			return Target;
		}

		public bool GetTarget(InfoContainer container, out object result)
		{
			result = null;
			for (int i = 0; i != drivableComponents.Length; i++)
			{
				if (container.target.IsAssignableFrom(drivableComponents[i].GetType()))
				{
					if ((container.isCalled && container.isStatic) || container.isStatic)
					{
						return true;
					}
					result = drivableComponents[i];
					return true;
				}
			}
			return false;
		}

		public override string ToString()
		{
			return Handle;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(LuaDriver).FullName);

	public static LuaDriverDescriptor desc = new LuaDriverDescriptor(typeof(ILuaDriverDescribable));

	public static LuaEnumDescriptor<LuaDriverTag> descLuaDriverTag = new LuaEnumDescriptor<LuaDriverTag>();

	private static SignalConnection _beforeReleasedConnection = SignalConnection.Empty;

	public LuaDriverTag luaDriverTag;

	[SerializeField]
	public bool IgnoreChildren = true;

	[SerializeField]
	public bool IsUnique;

	public string Handle;

	public List<SubHandle> SubHandles;

	[SerializeField]
	public Component[] drivableComponents;

	public static int countLD = 0;

	public static int countLDGC = 0;

	[SerializeField]
	[JsonProperty]
	private Dictionary<object, object> _userTable;

	private static LuaDriverDescriptor _desc = new LuaDriverDescriptor(typeof(ILuaDriverDescribable));

	private string _driverName;

	private bool properDead;

	[MoonSharpVisible(false)]
	public Dictionary<object, object> UserTable
	{
		get
		{
			if (_userTable == null)
			{
				_userTable = new Dictionary<object, object>();
			}
			return _userTable;
		}
		set
		{
			_userTable = value;
		}
	}

	[LuaDrivable("isSelected")]
	private DynValue lua_selection => DynValue.NewBoolean(base.selection);

	[LuaDrivable("isHovered")]
	private DynValue lua_hover => DynValue.NewBoolean(base.hover);

	[LuaDrivable("isInDanger")]
	private DynValue lua_isInDanger => DynValue.NewBoolean(base.isInDanger);

	[LuaDrivable("isActive")]
	private DynValue lua_IsActive => DynValue.NewBoolean(base.isActiveAndEnabled);

	[LuaDrivable("activeSelf")]
	private DynValue lua_activeSelf
	{
		get
		{
			return DynValue.NewBoolean(base.gameObject.activeSelf);
		}
		set
		{
			base.gameObject.SetActive(value.Boolean);
		}
	}

	[LuaDrivable("type")]
	private DynValue lua_type => UserData.CreateWithDescriptor(LuaDriverHQ.GetStaticDriver(Handle), desc);

	[LuaDrivable("name")]
	private DynValue lua_name
	{
		get
		{
			return DynValue.NewString(base.gameObject.name);
		}
		set
		{
			base.gameObject.name = value.String;
		}
	}

	[LuaDrivable("driverTag")]
	private DynValue lua_driverTag => UserData.CreateWithDescriptor(new LuaEnumProxy<LuaDriverTag>(luaDriverTag), descLuaDriverTag);

	public LuaDriver()
	{
		countLD++;
		countLDGC++;
	}

	public bool GetTarget(InfoContainer container, out object result)
	{
		result = null;
		for (int i = 0; i != drivableComponents.Length; i++)
		{
			if (container.target.IsAssignableFrom(drivableComponents[i].GetType()))
			{
				if ((container.isCalled && container.isStatic) || container.isStatic)
				{
					return true;
				}
				result = drivableComponents[i];
				return true;
			}
		}
		return false;
	}

	public bool TargetIsNullOrReleased()
	{
		return this == null || base.Released || base.Destroyed || base.gameObject == null;
	}

	public UnityEngine.Object GetUnityObject()
	{
		return base.gameObject;
	}

	public static void RegisterClass(Script script)
	{
		LuaLibs.RegisterScriptToClr<Component>(DataType.UserData, (DynValue v, Type t) => v.UserData.TryGet<LuaDriver>(out var param) ? param.CachedGetComponent(t) : null);
		LuaLibs.RegisterClrToScript(delegate(Script sc, Component c)
		{
			DynValue nil = DynValue.Nil;
			if (c is LuaDriver)
			{
				return UserData.CreateWithDescriptor(c, _desc);
			}
			return (c is IGameComponent) ? UserData.CreateWithDescriptor(((IGameComponent)c).luaDriver, _desc) : UserData.CreateWithDescriptor(c.GetComponent<LuaDriver>(), _desc);
		});
		LuaLibs.RegisterScriptToClr<GameObject>(DataType.UserData, (DynValue v, Type t) => v.UserData.TryGet<Component>(out param) ? param.gameObject : null);
		LuaLibs.RegisterClrToScript(delegate(Script sc, GameObject c)
		{
			LuaDriver component = c.GetComponent<LuaDriver>();
			return (component == null) ? DynValue.NewNil() : UserData.CreateWithDescriptor(component, _desc);
		});
		_beforeReleasedConnection.Disconnect();
		_beforeReleasedConnection = ObjectPoolSingleton.Instance.GameObjectBeforeReleasedSignal.Connect(delegate(Factory.AssetKey assetKey, GameObject gameObject)
		{
			OnGameObjectBeforeReleased(gameObject);
		});
	}

	protected override void Awake()
	{
		base.Awake();
		if (FactorySignalInvoker.OnFactoriesLoadedSig.InvokedCount > 0)
		{
			LuaDriverHQ.AddNewDriver(this);
			return;
		}
		FactorySignalInvoker.OnFactoriesLoadedSig.LateConnectOnce(delegate
		{
			LuaDriverHQ.AddNewDriver(this);
		});
	}

	protected override void Start()
	{
		if (FactorySignalInvoker.OnFactoriesLoadedSig.InvokedCount > 0)
		{
			LuaHookBase.InvokeFromCLR(LuaDriverHQ.GetStaticDriver(Handle).OnNew, this);
		}
		else
		{
			FactorySignalInvoker.OnFactoriesLoadedSig.LateConnectOnce(delegate
			{
				LuaHookBase.InvokeFromCLR(LuaDriverHQ.GetStaticDriver(Handle).OnNew, this);
			});
		}
		if (luaDriverTag == LuaDriverTag.Dynamic)
		{
			StatsTracker.ObjectBuilt(base.faction, AssetKey);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		LuaDriverHQ.RemoveDriver(this);
		if (_userTable != null)
		{
			_userTable.Clear();
			_userTable = null;
		}
	}

	public T CachedGetComponent<T>()
	{
		Type typeFromHandle = typeof(T);
		for (int i = 0; i != drivableComponents.Length; i++)
		{
			if (drivableComponents[i].GetType() == typeFromHandle)
			{
				return (T)(object)drivableComponents[i];
			}
		}
		return GetComponent<T>();
	}

	public object CachedGetComponent(Type wantedType)
	{
		for (int i = 0; i != drivableComponents.Length; i++)
		{
			if (drivableComponents[i].GetType() == wantedType)
			{
				return drivableComponents[i];
			}
		}
		return GetComponent(wantedType);
	}

	public override string ToString()
	{
		if (_driverName == null)
		{
			_driverName = base.gameObject.name + " (Driver)";
		}
		return _driverName;
	}

	protected override void OnDestroy()
	{
		if (!base.Released && base.Requested)
		{
			logger.WarnFormat("LuaDriver for '{0}' was destroyed but not released through Object Pool. This may cause bugs in the future.", base.name);
			InvokeTypeDestroy();
		}
		if (_userTable != null)
		{
			_userTable.Clear();
			_userTable = null;
		}
		if (SubHandles != null)
		{
			SubHandles.Clear();
			SubHandles = null;
		}
		drivableComponents = null;
		base.OnDestroy();
		if (!properDead)
		{
			countLD--;
			properDead = true;
		}
	}

	~LuaDriver()
	{
		if (!properDead)
		{
			countLD--;
			properDead = true;
		}
		countLDGC--;
	}

	private static void OnGameObjectBeforeReleased(GameObject gameObject)
	{
		LuaDriver component = gameObject.GetComponent<LuaDriver>();
		if (!component.IsNullOrReleased())
		{
			component.InvokeTypeDestroy();
		}
	}

	private void InvokeTypeDestroy()
	{
		LuaStaticDriver staticDriver = LuaDriverHQ.GetStaticDriver(Handle);
		LuaHookBase.InvokeFromCLR(staticDriver.OnTypeDestroy, this);
	}

	public Material GetMaterial()
	{
		MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
		return componentInChildren.sharedMaterial;
	}

	public void SetMaterial(Material mat)
	{
		MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
		componentInChildren.sharedMaterial = mat;
	}

	public void SetMesh(Mesh mesh)
	{
		MeshFilter componentInChildren = GetComponentInChildren<MeshFilter>();
		componentInChildren.sharedMesh = mesh;
	}

	public void Remove()
	{
		ObjectPoolSingleton.Release(base.gameObject);
	}

	[LuaDrivable("Destroy")]
	private static DynValue _Remove(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaDriver luaDriver = args.AsUserData<LuaDriver>(0, "Destroy");
		luaDriver.Remove();
		return DynValue.Void;
	}

	[LuaDrivable("SetMaterial")]
	private static DynValue _SetMaterial(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaDriver luaDriver = args.AsUserData<LuaDriver>(0, "SetMaterial");
		Material material = args.AsUserData<Material>(1, "SetMaterial");
		luaDriver.SetMaterial(material);
		return DynValue.Void;
	}

	[LuaDrivable("GetMaterial")]
	private static DynValue _GetMaterial(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaDriver luaDriver = args.AsUserData<LuaDriver>(0, "GetMaterial");
		return UserData.CreateWithDescriptor(luaDriver.GetMaterial(), LuaMaterial.desc);
	}

	[LuaDrivable("SetMesh")]
	private static DynValue _SetMesh(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaDriver luaDriver = args.AsUserData<LuaDriver>(0, "SetMesh");
		Mesh mesh = args.AsUserData<Mesh>(1, "SetMesh");
		luaDriver.SetMesh(mesh);
		return DynValue.Void;
	}
}
