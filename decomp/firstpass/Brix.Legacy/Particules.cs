using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Legacy;

public class Particules
{
	public static GameObject Emettre(Factory.AssetKey key, Vector3 position, Quaternion orientation, float duree)
	{
		ObjectPoolSingleton.Request(out var go, key, position, orientation);
		go.AddComponent<TimedDestruction>().TimeBeforeDestruction = duree;
		return go;
	}
}
