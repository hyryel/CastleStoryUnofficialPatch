using UnityEngine;

namespace Brix.Lua.MetaDrivers;

[LuaMetaDrivable(typeof(RectTransform))]
public abstract class LuaMetaRectTransform : LuaMetaTransform
{
	[LuaDrivable("anchoredPosition", UseExtern = true)]
	public abstract Vector2 anchoredPosition { get; set; }

	[LuaDrivable("anchoredPosition3D", UseExtern = true)]
	public abstract Vector3 anchoredPosition3D { get; set; }

	[LuaDrivable("anchorMax", UseExtern = true)]
	public abstract Vector2 anchorMax { get; set; }

	[LuaDrivable("anchorMin", UseExtern = true)]
	public abstract Vector2 anchorMin { get; set; }

	[LuaDrivable("offsetMax", UseExtern = true)]
	public abstract Vector2 offsetMax { get; set; }

	[LuaDrivable("offsetMin", UseExtern = true)]
	public abstract Vector2 offsetMin { get; set; }

	[LuaDrivable("pivot", UseExtern = true)]
	public abstract Vector2 pivot { get; set; }

	[LuaDrivable("sizeDelta", UseExtern = true)]
	public abstract Vector2 sizeDelta { get; set; }
}
