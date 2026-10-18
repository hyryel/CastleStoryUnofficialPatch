using System.Collections.Generic;
using Brix.Lua;
using Brix.UI.GameSelector;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;

public class LuaUIGameObserver : MonoBehaviour
{
	[LuaDrivable("GetSelecteds")]
	private static DynValue GetSelecteds(ScriptExecutionContext context, CallbackArguments args)
	{
		List<GameObject> gos = new List<GameObject>();
		UIGameSelector.GetSelectedOfType(ref gos, SelectionType.Spawner);
		return DynValue.NewTable(new Table(context.OwnerScript, (from go in gos.Slinq()
			select DynValue.FromObject(null, go)).ToList()));
	}

	[LuaDrivable("GetSelected")]
	private static DynValue GetSelected(ScriptExecutionContext context, CallbackArguments args)
	{
		List<GameObject> gos = new List<GameObject>();
		UIGameSelector.GetSelectedOfType(ref gos, SelectionType.Spawner);
		return DynValue.FromObject(context.OwnerScript, gos.Slinq().FirstOrDefault());
	}

	[LuaDrivable("AddToSelectionList")]
	private static DynValue AddToSelectionList(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject obj = args.AsGameObject(0, "AddToSelectionList");
		UIGameSelector.AddToSelection(obj);
		return DynValue.Void;
	}

	[LuaDrivable("RemoveSelectionList")]
	private static DynValue RemoveSelectionList(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject obj = args.AsGameObject(0, "RemoveSelectionList");
		UIGameSelector.Unselect(obj);
		return DynValue.Void;
	}
}
