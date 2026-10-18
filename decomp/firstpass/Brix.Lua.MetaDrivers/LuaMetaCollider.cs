using UnityEngine;

namespace Brix.Lua.MetaDrivers;

[LuaMetaDrivable(typeof(Collider))]
public abstract class LuaMetaCollider
{
	[LuaDrivable("colliderEnabled", UseExtern = true)]
	public abstract bool enabled { get; set; }
}
