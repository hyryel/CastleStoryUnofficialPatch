using Brix.External.Factories;
using MoonSharp.Interpreter;

namespace Brix.Lua;

public abstract class LuaAssetKey
{
	public static LuaObjectDescriptor<Factory.AssetKey> desc = new LuaObjectDescriptor<Factory.AssetKey>(typeof(LuaAssetKey));

	[LuaDrivable("factory", UseExtern = true)]
	public string Factory;

	[LuaDrivable("name", UseExtern = true)]
	public string Name;

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("AssetKey", desc.GetStaticProxy());
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext context, CallbackArguments args)
	{
		string factory = args.AsString(0, "AssetKey.New");
		string name = args.AsString(1, "AssetKey.New");
		return UserData.CreateWithDescriptor(new Factory.AssetKey(factory, name), desc);
	}

	[LuaDrivable("__eq")]
	private static DynValue __eq(ScriptExecutionContext context, CallbackArguments args)
	{
		Factory.AssetKey assetKey = args.AsUserData<Factory.AssetKey>(0, "AssetKey.__eq", allowNil: true);
		Factory.AssetKey assetKey2 = args.AsUserData<Factory.AssetKey>(1, "AssetKey.__eq", allowNil: true);
		return DynValue.NewBoolean(assetKey == assetKey2);
	}
}
