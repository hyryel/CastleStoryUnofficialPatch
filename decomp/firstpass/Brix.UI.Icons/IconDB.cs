using System;
using System.Collections.Generic;
using Brix.Lua;
using Brix.Utils;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Icons;

[Serializable]
public class IconDB : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(IconDB).FullName);

	private static Dictionary<string, IconDB> _databases = new Dictionary<string, IconDB>();

	private Dictionary<string, MetaIcon> _icons = new Dictionary<string, MetaIcon>();

	[SerializeField]
	public string UniqueName = "noname";

	[SerializeField]
	public List<IconKey> _keys;

	[SerializeField]
	public List<MetaIcon> _values;

	private static LuaObjectDescriptor<IconKey> _iconKeyDesc = new LuaObjectDescriptor<IconKey>(typeof(IconDB));

	public static void AddIcon(string db, string name, MetaIcon icon)
	{
		_databases.GetValueOrCreate(db).AddIcon(name, icon);
	}

	private void AddIcon(string name, MetaIcon icon)
	{
		_icons.Add(name, icon);
	}

	public static MetaIcon _Unsafe_GetIcon_NEVERUSE(string database, string name)
	{
		MetaIcon value = null;
		if (database == null)
		{
			using Dictionary<string, IconDB>.Enumerator enumerator = _databases.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current.Value._icons.TryGetValue(name, out value))
			{
			}
		}
		else
		{
			_databases[database]._icons.TryGetValue(name, out value);
		}
		return value;
	}

	private static MetaIcon GetIcon(string key)
	{
		throw new NotImplementedException();
	}

	public static MetaIcon GetIcon(IconKey key)
	{
		MetaIcon result = null;
		try
		{
			result = _databases[key.Database]._icons[key.Name];
		}
		catch
		{
			Debug.Log(key.Name);
			logger.Error("IconDB: The iconkey is invalid, that's not supposed to happen, report to JokieW");
		}
		return result;
	}

	private void Awake()
	{
		if (_keys != null)
		{
			for (int i = 0; i != _keys.Count; i++)
			{
				_icons.Add(_keys[i].Name, _values[i]);
			}
		}
		_keys = null;
		_values = null;
		_databases.Add(UniqueName, this);
	}

	public static void RegisterClass(Script script)
	{
		LuaLibs.RegisterUserData(typeof(IconKeys));
		script.Globals["IconKeys"] = UserData.CreateStatic(typeof(IconKeys));
	}

	[LuaDrivable("Get64")]
	private static DynValue lua_Get64(ScriptExecutionContext context, CallbackArguments args)
	{
		IconKey iconKey = args.AsUserData<IconKey>(0, "Get64");
		return DynValue.FromObject(context.OwnerScript, iconKey.Get64());
	}

	[LuaDrivable("Get32")]
	private static DynValue lua_Get32(ScriptExecutionContext context, CallbackArguments args)
	{
		IconKey iconKey = args.AsUserData<IconKey>(0, "Get32");
		return DynValue.FromObject(context.OwnerScript, iconKey.Get32());
	}
}
