using System;
using System.Collections.Generic;
using System.Reflection;
using Brix.Utils;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Smooth.Slinq;

namespace Brix.Lua;

public class LuaObjectDescriptor<T> : IUserDataDescriptor
{
	private static BindingFlags _standardFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private Type m_metaType;

	private LuaObjectStaticProxy _proxy;

	public Dictionary<string, InfoContainer> _allInfos = new Dictionary<string, InfoContainer>();

	public string Name => Type.FullName;

	public Type Type => typeof(T);

	public Type MetaType => m_metaType;

	public LuaObjectDescriptor(Type metaType = null)
	{
		UserData.RegisterType(typeof(T), this);
		_proxy = new LuaObjectStaticProxy(this);
		if (metaType == null)
		{
			m_metaType = Type;
		}
		else
		{
			m_metaType = metaType;
		}
		GetAllDrivables();
	}

	public DynValue GetStaticProxy()
	{
		return UserData.CreateWithDescriptor(_proxy, LuaObjectStaticProxyDescriptor.desc);
	}

	public Table GetCallbackTable(Script script)
	{
		Table table = new Table(script);
		foreach (KeyValuePair<string, InfoContainer> allInfo in _allInfos)
		{
			if (allInfo.Value.isCalled)
			{
				table.Set(allInfo.Key, allInfo.Value.GetCallback());
			}
		}
		return table;
	}

	private void GetAllDrivables()
	{
		MethodInfo[] methods = m_metaType.GetMethods(_standardFlags);
		foreach (MethodInfo methodInfo in methods)
		{
			MethodInfo info = methodInfo;
			Type target = m_metaType;
			LuaDrivable[] array = (LuaDrivable[])methodInfo.GetCustomAttributes(typeof(LuaDrivable), inherit: false);
			if (array == null || array.Length <= 0)
			{
				continue;
			}
			if (array[0].UseExtern)
			{
				info = Type.GetMethod(methodInfo.Name, _standardFlags, null, (from x in methodInfo.GetParameters().Slinq()
					select x.ParameterType).ToArray(), null);
				target = Type;
			}
			_allInfos.Add(array[0].Name, InfoContainer.From(info, target));
		}
		PropertyInfo[] properties = m_metaType.GetProperties(_standardFlags);
		foreach (PropertyInfo propertyInfo in properties)
		{
			PropertyInfo info2 = propertyInfo;
			Type target2 = m_metaType;
			LuaDrivable[] array2 = (LuaDrivable[])propertyInfo.GetCustomAttributes(typeof(LuaDrivable), inherit: false);
			if (array2 != null && array2.Length > 0)
			{
				if (array2[0].UseExtern)
				{
					info2 = Type.GetProperty(propertyInfo.Name, _standardFlags);
					target2 = Type;
				}
				_allInfos.Add(array2[0].Name, InfoContainer.From(info2, target2));
			}
		}
		FieldInfo[] fields = m_metaType.GetFields(_standardFlags);
		foreach (FieldInfo fieldInfo in fields)
		{
			FieldInfo info3 = fieldInfo;
			Type target3 = m_metaType;
			LuaDrivable[] array3 = (LuaDrivable[])fieldInfo.GetCustomAttributes(typeof(LuaDrivable), inherit: false);
			if (array3 != null && array3.Length > 0)
			{
				if (array3[0].UseExtern)
				{
					info3 = Type.GetField(fieldInfo.Name, _standardFlags);
					target3 = Type;
				}
				_allInfos.Add(array3[0].Name, InfoContainer.From(info3, target3));
			}
		}
	}

	public DynValue Index(Script script, IUserData obj, DynValue index, bool isDirectIndexing)
	{
		index = index.ToScalar();
		if (index.Type != DataType.String)
		{
			throw ScriptRuntimeException.BadArgument(1, $"userdata<{Name}>.__index", "string", index.Type.ToLuaTypeString(), allowNil: false);
		}
		string key = index.String;
		DynValue result = DynValue.NewNil();
		InfoContainer value = null;
		if (_allInfos.TryGetValue(key, out value))
		{
			result = ((!value.isCalled) ? value.GetValue(script, obj) : value.GetCallback());
		}
		return result;
	}

	public bool SetIndex(Script script, IUserData obj, DynValue index, DynValue value, bool isDirectIndexing)
	{
		bool result = false;
		index = index.ToScalar();
		if (index.Type != DataType.String)
		{
			throw ScriptRuntimeException.BadArgument(1, $"userdata<{Name}>.__setindex", "string", index.Type.ToLuaTypeString(), allowNil: false);
		}
		string key = index.String;
		InfoContainer value2 = null;
		if (_allInfos.TryGetValue(key, out value2))
		{
			value2.SetValue(script, obj, value);
			result = true;
		}
		return result;
	}

	public string AsString(IUserData obj)
	{
		obj.TryGet<T>(out var param);
		return param.ToString();
	}

	public DynValue MetaIndex(Script script, IUserData obj, string metaname)
	{
		DynValue result = Index(script, obj, DynValue.NewString(metaname), isDirectIndexing: true);
		if (result.IsNil())
		{
			return DynValue.Invalid;
		}
		return result;
	}

	public bool IsTypeCompatible(Type type, IUserData obj)
	{
		return obj.UnderlyingType == type;
	}
}
