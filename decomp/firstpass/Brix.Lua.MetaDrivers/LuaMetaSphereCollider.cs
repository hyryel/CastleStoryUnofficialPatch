using UnityEngine;

namespace Brix.Lua.MetaDrivers;

[LuaMetaDrivable(typeof(SphereCollider))]
public abstract class LuaMetaSphereCollider : LuaMetaCollider
{
	[LuaDrivable("radius", UseExtern = true)]
	public abstract float radius { get; set; }
}
