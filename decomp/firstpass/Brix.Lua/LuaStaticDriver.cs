using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Lua;

public class LuaStaticDriver : ILuaDriverDescribable
{
	private static LuaDriverDescriptor _desc = new LuaDriverDescriptor(typeof(LuaStaticDriver));

	private string _handle = string.Empty;

	private LuaDriverTag _tag;

	private string _driverPrint;

	private string _methodName_New;

	private List<LuaDriver> _allInstances = new List<LuaDriver>(64);

	private LuaDriver _baseDriver;

	private Factory.AssetKey _assetKey;

	[LuaDrivable("hk_OnNew")]
	public LuaHook<LuaDriver> OnNew = new LuaHook<LuaDriver>();

	private LuaHook<LuaDriver> _onTypeDestroy;

	private Dictionary<object, object> _userTable;

	[LuaDrivable("hk_OnTypeDestroy")]
	public LuaHook<LuaDriver> OnTypeDestroy
	{
		get
		{
			if (_onTypeDestroy == null)
			{
				_onTypeDestroy = new LuaHook<LuaDriver>();
			}
			return _onTypeDestroy;
		}
	}

	public string handle => _handle;

	public LuaDriverTag tag => _tag;

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

	[LuaDrivable("__Factory")]
	public LuaDriver baseDriver
	{
		get
		{
			return _baseDriver;
		}
		set
		{
			_baseDriver = value;
		}
	}

	public Factory.AssetKey AssetKey
	{
		get
		{
			return _assetKey;
		}
		set
		{
			_assetKey = value;
		}
	}

	private LuaStaticDriver()
	{
	}

	public LuaStaticDriver(string handle, LuaDriverTag tag)
	{
		_handle = handle;
		_tag = tag;
		_driverPrint = handle + " (Static Driver)";
		_methodName_New = handle + ".New";
	}

	public static void RegisterClass(Script script)
	{
		foreach (KeyValuePair<string, LuaStaticDriver> allStatic in LuaDriverHQ.AllStatics)
		{
			script.Globals[allStatic.Key] = allStatic.Value;
		}
	}

	public override string ToString()
	{
		return _driverPrint;
	}

	public bool GetTarget(InfoContainer container, out object result)
	{
		if (container.target == typeof(LuaStaticDriver))
		{
			result = this;
			return true;
		}
		if (_baseDriver == null)
		{
			result = null;
			return true;
		}
		return _baseDriver.GetTarget(container, out result);
	}

	public T CachedGetComponent<T>()
	{
		return _baseDriver.CachedGetComponent<T>();
	}

	public bool TargetIsNullOrReleased()
	{
		return false;
	}

	public Object GetUnityObject()
	{
		return (!(_baseDriver == null)) ? _baseDriver.gameObject : null;
	}

	public void AddInstance(LuaDriver ld)
	{
		_allInstances.Add(ld);
	}

	public bool ContainsInstance(LuaDriver ld)
	{
		return _allInstances.Contains(ld);
	}

	public void RemoveInstance(LuaDriver ld)
	{
		if (_allInstances.Contains(ld))
		{
			_allInstances.Remove(ld);
		}
		else if (ld == _baseDriver)
		{
			_baseDriver = null;
		}
	}

	public void ClearInstances(bool hard = false)
	{
		if (hard)
		{
			for (int num = _allInstances.Count - 1; num != -1; num--)
			{
				_allInstances[num].Remove();
			}
		}
		_allInstances.Clear();
	}

	public void CleanInstances()
	{
		for (int num = _allInstances.Count - 1; num != -1; num--)
		{
			if (_allInstances[num] == null)
			{
				_allInstances.RemoveAt(num);
			}
		}
	}

	[LuaDrivable("New")]
	private static DynValue InstantiateSelf(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "New");
		Faction faction = args.AsComponent<Faction>(1, luaStaticDriver._methodName_New);
		Vector3 position = args.AsUserData<Vector3>(2, luaStaticDriver._methodName_New);
		Quaternion rotation = ((args.Count < 4) ? Quaternion.identity : args.AsUserData<Quaternion>(3, luaStaticDriver._methodName_New));
		return DynValue.FromObject(context.OwnerScript, LuaFactory.InstantiateFromCLR(luaStaticDriver._assetKey, faction, position, rotation));
	}

	[LuaDrivable("GetFirst")]
	private static DynValue GetFirst(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetFirst");
		if (luaStaticDriver._allInstances.Count == 0)
		{
			return DynValue.Nil;
		}
		return DynValue.FromObject(context.OwnerScript, luaStaticDriver._allInstances[0]);
	}

	public LuaDriver GetFirst()
	{
		if (_allInstances.Count == 0)
		{
			return null;
		}
		return _allInstances[0];
	}

	[LuaDrivable("GetFirstOfFaction")]
	private static DynValue GetFirstOfFaction(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetFirstOfFaction");
		Faction faction = args.AsComponent<Faction>(1, "GetFirstOfFaction");
		if (luaStaticDriver._allInstances.Count == 0)
		{
			return DynValue.Nil;
		}
		return DynValue.FromObject(context.OwnerScript, luaStaticDriver._allInstances.Slinq().FirstOrDefault((LuaDriver x) => x.faction == faction));
	}

	public LuaDriver GetFirstOfFaction(Faction faction)
	{
		if (_allInstances.Count == 0)
		{
			return null;
		}
		return _allInstances.Slinq().FirstOrDefault((LuaDriver x) => x.faction == faction);
	}

	[LuaDrivable("GetLast")]
	private static DynValue GetLast(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetLast");
		if (luaStaticDriver._allInstances.Count == 0)
		{
			return DynValue.Nil;
		}
		return DynValue.FromObject(context.OwnerScript, luaStaticDriver._allInstances[luaStaticDriver._allInstances.Count - 1]);
	}

	public LuaDriver GetLast()
	{
		if (_allInstances.Count == 0)
		{
			return null;
		}
		return _allInstances[_allInstances.Count - 1];
	}

	[LuaDrivable("GetLastOfFaction")]
	private static DynValue GetLastOfFaction(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetLastOfFaction");
		Faction faction = args.AsComponent<Faction>(1, "GetLastOfFaction");
		if (luaStaticDriver._allInstances.Count == 0)
		{
			return DynValue.Nil;
		}
		return DynValue.FromObject(context.OwnerScript, luaStaticDriver._allInstances.Slinq().LastOrDefault((LuaDriver x) => x.faction == faction));
	}

	public LuaDriver GetLastOfFaction(Faction faction)
	{
		if (_allInstances.Count == 0)
		{
			return null;
		}
		return _allInstances.Slinq().LastOrDefault((LuaDriver x) => x.faction == faction);
	}

	[LuaDrivable("GetAtIndex")]
	private static DynValue GetAtIndex(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetAtIndex");
		int num = args.AsInt(1, "GetAtIndex");
		if (num < 0 || num >= luaStaticDriver._allInstances.Count)
		{
			return DynValue.Nil;
		}
		return DynValue.FromObject(context.OwnerScript, luaStaticDriver._allInstances[num]);
	}

	public LuaDriver GetAtIndex(int i)
	{
		if (i < 0 || i >= _allInstances.Count)
		{
			return null;
		}
		return _allInstances[i];
	}

	[LuaDrivable("GetAtIndexOfFaction")]
	private static DynValue GetAtIndexOfFaction(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetAtIndexOfFaction");
		int num = args.AsInt(1, "GetAtIndexOfFaction");
		Faction parameter = args.AsComponent<Faction>(2, "GetAtIndexOfFaction");
		if (num < 0 || num >= luaStaticDriver._allInstances.Count)
		{
			return DynValue.Invalid;
		}
		return DynValue.FromObject(context.OwnerScript, luaStaticDriver._allInstances.Slinq().Where((LuaDriver x, Faction _fac) => x.faction == _fac, parameter).ElementAt(num));
	}

	public LuaDriver GetAtIndexOfFaction(int i, Faction faction)
	{
		if (i < 0 || i >= _allInstances.Count)
		{
			return null;
		}
		return _allInstances.Slinq().Where((LuaDriver x, Faction _fac) => x.faction == _fac, faction).ElementAt(i);
	}

	[LuaDrivable("GetAll")]
	private static DynValue GetAll(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetAll");
		List<DynValue> arrayValues = luaStaticDriver._allInstances.Slinq().Select((LuaDriver x, ScriptExecutionContext _context) => DynValue.FromObject(_context.OwnerScript, x), context).ToList();
		return DynValue.NewTable(context.OwnerScript, arrayValues);
	}

	public List<LuaDriver> GetAll()
	{
		return _allInstances;
	}

	[LuaDrivable("GetAllOfUser")]
	private static DynValue GetAllOfUser(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetAllOfUser");
		User parameter = args.AsComponent<User>(1, "GetAllOfUser");
		List<DynValue> arrayValues = luaStaticDriver._allInstances.Slinq().Where((LuaDriver x, User _user) => x.faction == _user.DefaultAlliedFactionGO, parameter).Select((LuaDriver x, ScriptExecutionContext _context) => DynValue.FromObject(_context.OwnerScript, x), context)
			.ToList();
		return DynValue.NewTable(context.OwnerScript, arrayValues);
	}

	public List<LuaDriver> GetAllOfUser(User user)
	{
		if (user == null)
		{
			return new List<LuaDriver>();
		}
		return _allInstances.Slinq().Where((LuaDriver x, User _user) => x.faction == _user.DefaultAlliedFactionGO, user).ToList();
	}

	[LuaDrivable("GetAllOfFaction")]
	private static DynValue GetAllOfFaction(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetAllOfFaction");
		Faction parameter = args.AsComponent<Faction>(1, "GetAllOfFaction");
		List<DynValue> arrayValues = luaStaticDriver._allInstances.Slinq().Where((LuaDriver x, Faction _fac) => x.faction == _fac, parameter).Select((LuaDriver x, ScriptExecutionContext _context) => DynValue.FromObject(_context.OwnerScript, x), context)
			.ToList();
		return DynValue.NewTable(context.OwnerScript, arrayValues);
	}

	public List<LuaDriver> GetAllOfFaction(Faction faction)
	{
		return _allInstances.Slinq().Where((LuaDriver x, Faction _fac) => x.faction == _fac, faction).ToList();
	}

	[LuaDrivable("GetAllOfFactions")]
	private static DynValue GetAllOfFactions(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "GetAllOfFactions");
		Table table = args.AsTable(1, "GetAllOfFactions");
		List<DynValue> list = new List<DynValue>();
		foreach (DynValue value in table.Values)
		{
			luaStaticDriver._allInstances.Slinq().Where((LuaDriver x, DynValue _dv) => x.faction == _dv.ToObject<Faction>(), value).Select((LuaDriver x, ScriptExecutionContext _context) => DynValue.FromObject(_context.OwnerScript, x), context)
				.AddTo(list);
		}
		return DynValue.NewTable(context.OwnerScript, list);
	}

	public List<LuaDriver> GetAllOfFactions(Faction[] factions)
	{
		List<LuaDriver> list = new List<LuaDriver>();
		foreach (Faction parameter in factions)
		{
			_allInstances.Slinq().Where((LuaDriver x, Faction _fac) => x.faction == _fac, parameter).AddTo(list);
		}
		return list;
	}

	[LuaDrivable("Count")]
	private static DynValue Count(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "Count");
		if (args.Count >= 2 && args.AsBool(1, "Count"))
		{
			return DynValue.NewNumber((from x in luaStaticDriver._allInstances.Slinq()
				where x.gameObject.activeInHierarchy
				select x).Count());
		}
		return DynValue.NewNumber(luaStaticDriver._allInstances.Count);
	}

	public int Count(bool activeOnly = false)
	{
		if (activeOnly)
		{
			return (from x in _allInstances.Slinq()
				where x.gameObject.activeInHierarchy
				select x).Count();
		}
		return _allInstances.Count;
	}

	[LuaDrivable("CountOfFaction")]
	private static DynValue CountOfFaction(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "CountOfFaction");
		Faction parameter = args.AsComponent<Faction>(1, "CountOfFaction");
		return DynValue.NewNumber(luaStaticDriver._allInstances.Slinq().Where((LuaDriver x, Faction _fac) => x.faction == _fac, parameter).Count());
	}

	public int CountOfFaction(Faction faction)
	{
		return _allInstances.Slinq().Where((LuaDriver x, Faction _fac) => x.faction == _fac, faction).Count();
	}

	[LuaDrivable("CountOfFactionAndAllied")]
	private static DynValue CountOfFactionAndAllied(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "CountOfFactionAndAllied");
		Faction faction = args.AsComponent<Faction>(1, "CountOfFactionAndAllied");
		return DynValue.NewNumber((from x in luaStaticDriver._allInstances.Slinq()
			where x.faction == faction || Affiliation.IsAllied(faction.gameObject, x.gameObject)
			select x).Count());
	}

	public int CountOfFactionAndAllied(Faction faction)
	{
		return _allInstances.Slinq().Where((LuaDriver x, Faction _fac) => x.faction == _fac || Affiliation.IsAllied(_fac.gameObject, x.gameObject), faction).Count();
	}
}
