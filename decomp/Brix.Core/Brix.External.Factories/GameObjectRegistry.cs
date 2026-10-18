using System;
using System.Collections.Generic;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.External.Factories;

public class GameObjectRegistry : IGameObjectRegistry
{
	private Dictionary<string, GameObject> _registeredGameObjects = new Dictionary<string, GameObject>();

	public Signal<string, GameObject> OnRegisteredGameObjectSignal = new Signal<string, GameObject>();

	public void Register(string id, GameObject go)
	{
		try
		{
			_registeredGameObjects[id] = go;
		}
		catch (ArgumentException)
		{
			throw new ArgumentException(string.Concat("GameObjectRegistry registration failed for entry {Key='", id, "', Value='", go, "'}. Existing entry's GameObject was '", _registeredGameObjects[id], "'."));
		}
		OnRegisteredGameObjectSignal.Invoke(id, go);
	}

	public GameObject Find(string id)
	{
		_registeredGameObjects.TryGetValue(id, out var value);
		return value;
	}

	public bool Unregister(string id)
	{
		return _registeredGameObjects.Remove(id);
	}

	public void Clear()
	{
		_registeredGameObjects.Clear();
	}

	public bool Contains(string id)
	{
		return _registeredGameObjects.ContainsKey(id);
	}
}
