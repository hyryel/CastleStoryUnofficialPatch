using System;

public class RearValue
{
	private Type _refType;

	private object _ref;

	public object Reference => _ref;

	public RearValue(string type, object instance)
	{
		SetReference(type, instance);
	}

	public RearValue(Type type, object instance)
	{
		SetReference(type, instance);
	}

	public void SetReference(string type, object instance)
	{
		_ref = instance;
		_refType = RearViewMirror.GetType(type);
	}

	public void SetReference(Type type, object instance)
	{
		_ref = instance;
		_refType = type;
	}

	public object CallGenericMethod(string builtinType, string name, params object[] parameters)
	{
		return RearViewMirror.CallMethod(RearViewMirror.GetGenericType(_refType, builtinType), _ref, name, parameters);
	}

	public object CallMethod(string name, params object[] parameters)
	{
		return RearViewMirror.CallMethod(_refType, _ref, name, parameters);
	}

	public object GetField(string name)
	{
		return RearViewMirror.GetField(_refType, _ref, name);
	}

	public object[] GetArray(string name)
	{
		return RearViewMirror.GetArray(_refType, _ref, name);
	}

	public int GetInt(string name)
	{
		return RearViewMirror.GetInt(_refType, _ref, name);
	}

	public float GetFloat(string name)
	{
		return RearViewMirror.GetFloat(_refType, _ref, name);
	}

	public string GetString(string name)
	{
		return RearViewMirror.GetString(_refType, _ref, name);
	}

	public bool GetBool(string name)
	{
		return RearViewMirror.GetBool(_refType, _ref, name);
	}

	public void SetFieldAtIndex(string name, object value, int index)
	{
		RearViewMirror.SetFieldAtIndex(_refType, _ref, name, value, index);
	}

	public void SetField(string name, object value)
	{
		RearViewMirror.SetField(_refType, _ref, name, value);
	}

	public object GetProperty(string name)
	{
		return RearViewMirror.GetProperty(_refType, _ref, name);
	}

	public void SetProperty(string name, object value)
	{
		RearViewMirror.SetProperty(_refType, _ref, name, value);
	}
}
