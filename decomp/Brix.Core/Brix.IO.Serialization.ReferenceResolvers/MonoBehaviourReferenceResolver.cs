using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Brix.IO.Serialization.ReferenceResolvers;

public class MonoBehaviourReferenceResolver : IReferenceResolver
{
	private HashSet<int> _registry = new HashSet<int>();

	public object ResolveReference(object context, string reference)
	{
		throw new NotImplementedException();
	}

	public string GetReference(object context, object value)
	{
		UnityEngine.Object obj = value as UnityEngine.Object;
		_registry.Add(obj.GetInstanceID());
		return null;
	}

	public bool IsReferenced(object context, object value)
	{
		UnityEngine.Object obj = value as UnityEngine.Object;
		if (obj != null)
		{
			return _registry.Contains(obj.GetInstanceID());
		}
		return false;
	}

	public void AddReference(object context, string reference, object value)
	{
		throw new NotImplementedException();
	}

	public void Clear()
	{
		_registry.Clear();
	}
}
