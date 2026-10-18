using System;
using Brix.Compile;

namespace Brix.RuntimeShell.Eval;

public class LuaRuntime : CodeEvaluatorComponent
{
	public override object Eval(string code, AbstractContext context)
	{
		throw new Exception("Lua is not initialized.");
	}
}
