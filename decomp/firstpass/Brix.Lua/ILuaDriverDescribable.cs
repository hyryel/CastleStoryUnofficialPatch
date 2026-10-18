using System.Collections.Generic;
using UnityEngine;

namespace Brix.Lua;

public interface ILuaDriverDescribable
{
	Dictionary<object, object> UserTable { get; set; }

	Object GetUnityObject();

	bool GetTarget(InfoContainer container, out object result);

	T CachedGetComponent<T>();

	bool TargetIsNullOrReleased();
}
