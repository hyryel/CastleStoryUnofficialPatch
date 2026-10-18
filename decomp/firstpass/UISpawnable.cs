using Brix.External.Factories;
using Brix.Lua;
using UnityEngine;

public class UISpawnable : MonoBehaviour
{
	[LuaDrivable("isDebug")]
	public bool isDebug;

	[LuaDrivable("isBuildDebug")]
	public bool isBuildDebug;

	[LuaDrivable("text")]
	public string text;

	[LuaDrivable("key")]
	public string key;

	[LuaDrivable("family")]
	public string family;

	[LuaDrivable("subFamily")]
	public string subFamily;

	[LuaDrivable("icon")]
	public Sprite icon;

	[LuaDrivable("spawnsThis")]
	public Factory.AssetKey spawnsThis;
}
