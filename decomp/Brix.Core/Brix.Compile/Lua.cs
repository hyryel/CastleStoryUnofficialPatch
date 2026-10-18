using UnityEngine;

namespace Brix.Compile;

public class Lua
{
	public class Context : AbstractContext
	{
		public Context()
		{
		}

		public Context(AbstractContext other)
			: this()
		{
			Copy(other);
		}

		public object Eval(string code)
		{
			if (Target != null)
			{
				RearValue rearValue = new RearValue("LuaStandalone", Target.GetComponent(RearViewMirror.GetType("LuaStandalone")));
				if (rearValue.Reference != null)
				{
					return RearViewMirror.CallMethod("Script", rearValue.GetProperty("script"), "DoString", code, null);
				}
				Target = null;
				Debug.LogError("LuaShell: Target did not have a valid LuaStandalone, removing.");
			}
			return RearViewMirror.CallMethod("LuaLoader", null, "LoadString", code);
		}
	}

	public static GameObject Target;

	private static Context context = null;

	public static object Eval(string code)
	{
		if (context == null)
		{
			context = new Context();
		}
		return context.Eval(code);
	}

	public static object Eval(Context context, string code)
	{
		return context.Eval(code);
	}
}
