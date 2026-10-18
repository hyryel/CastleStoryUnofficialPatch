using Brix.Compile;

namespace Brix.RuntimeShell.Eval;

public class UnityScriptRuntime : CodeEvaluatorComponent
{
	public override object Eval(string code, AbstractContext context)
	{
		return JS.Eval(new JS.Context(context), code);
	}
}
