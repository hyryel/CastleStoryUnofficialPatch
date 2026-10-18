using UnityEngine;

namespace Brix.Lua.MetaDrivers;

[LuaMetaDrivable(typeof(Light))]
public abstract class LuaMetaLight
{
	[LuaDrivable("lightColor", UseExtern = true)]
	public abstract Color color { get; set; }
}
