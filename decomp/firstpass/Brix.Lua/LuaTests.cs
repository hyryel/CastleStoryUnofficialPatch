using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public static class LuaTests
{
	public static void TestLuaHooks()
	{
		LuaHook hook = new LuaHook();
		Closure function = LuaLoader.LoadString("return || print(\"GAPOUEL\")").Function;
		LuaHookBase.ConnectFromCLR(hook, function, null);
		LuaHookBase.DisconnectFromCLR(hook, function);
		LuaHookBase.ConnectFromCLR(hook, function, null);
		LuaHookBase.InvokeFromCLR(hook);
		LuaLoader.LoadString("\r\nlocal func = || print(\"LAMPADAIRE\")\r\nHooks.Connect(hk_testSig, func)\r\nHooks.Disconnect(hk_testSig, func)\r\nHooks.Connect(hk_testSig, func)\r\nHooks.Invoke(hk_testSig, {}) --Prints \"LAMPADAIRE\"\r\n", delegate(Script script)
		{
			LuaLibs.BigLib(script);
			script.Globals["hk_testSig"] = new LuaHook();
		}, null);
		LuaHook hook2 = new LuaHook();
		LuaHook luaHook = new LuaHook();
		Closure function2 = LuaLoader.LoadString("return || print(\"BARBEAU\")").Function;
		LuaHookBase.ConnectFromCLR(hook2, function2, luaHook);
		LuaHookBase.InvokeFromCLR(luaHook);
		LuaHookBase.ConnectFromCLR(hook2, function2, null);
		LuaHookBase.InvokeFromCLR(hook2);
		LuaLoader.LoadString("\r\nlocal func = || print(\"MARCEL\")\r\nHooks.Connect(hk_testSig, func, hk_disSig)\r\nHooks.Invoke(hk_disSig, {})\r\nHooks.Connect(hk_testSig, func)\r\nHooks.Invoke(hk_testSig, {}) --Prints \"MARCEL\"\r\n", delegate(Script script)
		{
			LuaLibs.BigLib(script);
			script.Globals["hk_testSig"] = new LuaHook();
			script.Globals["hk_disSig"] = new LuaHook();
		}, null);
	}

	public static void TestTable()
	{
		Table table = LuaLoader.LoadString("return {\"POPOSICLE\"}", null, null).Table;
		Debug.Log("Table う full: " + table.Length + " " + table[1]);
		table.Clear();
		Debug.Log("Table う cleared: " + table.Length + " " + table[1]);
		Table table2 = LuaLoader.LoadString("return {}", null, null).Table;
		Debug.Log("Table え full: " + table2.Length + " " + table2[1]);
		table2.Clear();
		Debug.Log("Table え cleared: " + table2.Length + " " + table2[1]);
		Table table3 = LuaLoader.LoadString("return {\"DONDAINE\"}").Table;
		Table table4 = LuaLoader.LoadString("return {}").Table;
		Debug.Log("一 か = " + table3.ReferenceID + " き = " + table4.ReferenceID);
		table3 = table4;
		Debug.Log("二 か = " + table3.ReferenceID + " き = " + table4.ReferenceID);
		LuaLoader.LoadString("ka = {\"Dondaine\"}; ki = {}; ka = ki", null, delegate(Script script)
		{
			Debug.Log("三 か = " + ((Table)script.Globals["ka"]).ReferenceID + " き = " + ((Table)script.Globals["ki"]).ReferenceID);
		});
	}
}
