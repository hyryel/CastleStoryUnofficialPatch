using Brix.Components;
using UnityEngine;

namespace Brix.External.Factories;

public class MainGameObjectRegistry : BrixSingleton<MainGameObjectRegistry>, IGameObjectRegistry
{
	private IGameObjectRegistry registry = new GameObjectRegistry();

	public void Register(string id, GameObject go)
	{
		registry.Register(id, go);
	}

	public GameObject Find(string id)
	{
		return registry.Find(id);
	}

	public bool Unregister(string id)
	{
		return registry.Unregister(id);
	}

	public void Clear()
	{
		registry.Clear();
	}

	public bool Contains(string id)
	{
		return registry.Contains(id);
	}
}
