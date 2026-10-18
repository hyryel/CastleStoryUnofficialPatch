using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine;

namespace Brix.Lua;

public class LuaDriverDescriptor : IUserDataDescriptor
{
	private Type m_type;

	private UserDataRef _uDataRef = new UserDataRef();

	public string Name => m_type.FullName;

	public Type Type => m_type;

	public LuaDriverDescriptor(Type type)
	{
		UserData.RegisterType<ILuaDriverDescribable>(this);
		m_type = type;
	}

	public DynValue Index(Script script, IUserData obj, DynValue index, bool isDirectIndexing)
	{
		if (obj == null)
		{
			return DynValue.Invalid;
		}
		obj.TryGet<ILuaDriverDescribable>(out var param);
		if (param.TargetIsNullOrReleased())
		{
			throw new ScriptRuntimeException("Target is null or released.");
		}
		index = index.ToScalar();
		if (index.Type != DataType.String)
		{
			throw ScriptRuntimeException.BadArgument(1, "userdata<" + Name + ">.__index", "string", index.Type.ToLuaTypeString(), allowNil: false);
		}
		DynValue dynValue = DynValue.Invalid;
		InfoContainer[] info = null;
		if (LuaDriverHQ.GetInfos(index.String, out info))
		{
			for (int i = 0; i != info.Length; i++)
			{
				InfoContainer infoContainer = info[i];
				object result = null;
				if (param.GetTarget(infoContainer, out result))
				{
					_uDataRef.TrySet(result);
					dynValue = ((!infoContainer.isCalled) ? infoContainer.GetValue(script, _uDataRef) : infoContainer.GetCallback());
					_uDataRef.TrySet<object>(null);
					break;
				}
			}
		}
		if (!dynValue.IsValid)
		{
			List<LuaDriver.SubHandle> list = null;
			if (param is LuaDriver)
			{
				list = ((LuaDriver)param).SubHandles;
			}
			else if (param is LuaStaticDriver)
			{
				list = ((LuaStaticDriver)param).baseDriver.SubHandles;
			}
			if (list != null && list.Count > 0)
			{
				for (int j = 0; j != list.Count; j++)
				{
					if (list[j].Handle == index.String)
					{
						dynValue = DynValue.FromObject(script, list[j]);
						break;
					}
				}
			}
		}
		if (!dynValue.IsValid && param.UserTable.TryGetValue(index.ToObject<object>(), out var value))
		{
			bool flag = false;
			if (value == null)
			{
				dynValue = DynValue.Nil;
				flag = true;
			}
			else if (value is GameObject)
			{
				if ((GameObject)value == null)
				{
					dynValue = DynValue.Nil;
					flag = true;
				}
			}
			else if (typeof(Component).IsAssignableFrom(value.GetType()) && (Component)value == null)
			{
				dynValue = DynValue.Nil;
				flag = true;
			}
			if (!flag)
			{
				dynValue = DynValue.FromObject(script, value);
			}
		}
		return (!dynValue.IsValid) ? DynValue.Nil : dynValue;
	}

	public bool SetIndex(Script script, IUserData obj, DynValue index, DynValue value, bool isDirectIndexing)
	{
		if (obj == null)
		{
			return false;
		}
		obj.TryGet<ILuaDriverDescribable>(out var param);
		if (param.TargetIsNullOrReleased())
		{
			throw new ScriptRuntimeException("Target is null or released.");
		}
		bool flag = false;
		index = index.ToScalar();
		if (index.Type != DataType.String)
		{
			throw ScriptRuntimeException.BadArgument(1, $"userdata<{Name}>.__setindex", "string", index.Type.ToLuaTypeString(), allowNil: false);
		}
		InfoContainer[] info = null;
		if (LuaDriverHQ.GetInfos(index.String, out info))
		{
			for (int i = 0; i != info.Length; i++)
			{
				InfoContainer infoContainer = info[i];
				object result = null;
				if (param.GetTarget(infoContainer, out result))
				{
					_uDataRef.TrySet(result);
					infoContainer.SetValue(script, _uDataRef, value);
					_uDataRef.TrySet<object>(null);
					flag = true;
				}
			}
		}
		if (!flag)
		{
			if (value.Type == DataType.Table)
			{
				param.UserTable[index.String] = LuaLibs.SerialiseTable(value.Table);
				flag = true;
			}
			else
			{
				param.UserTable[index.String] = value.ToObject<object>();
				flag = true;
			}
		}
		return flag;
	}

	public string AsString(IUserData obj)
	{
		obj.TryGet<ILuaDriverDescribable>(out var param);
		return param.ToString();
	}

	public DynValue MetaIndex(Script script, IUserData obj, string metaname)
	{
		return DynValue.Invalid;
	}

	public bool IsTypeCompatible(Type type, IUserData obj)
	{
		return obj.UnderlyingType.IsAssignableFrom(type);
	}
}
