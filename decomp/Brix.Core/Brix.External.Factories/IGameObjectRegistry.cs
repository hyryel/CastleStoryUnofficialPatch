using UnityEngine;

namespace Brix.External.Factories;

public interface IGameObjectRegistry
{
	void Register(string id, GameObject go);

	GameObject Find(string id);

	bool Unregister(string id);

	void Clear();

	bool Contains(string id);
}
