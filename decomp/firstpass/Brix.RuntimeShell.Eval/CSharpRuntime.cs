using Brix.Compile;

namespace Brix.RuntimeShell.Eval;

public class CSharpRuntime : CodeEvaluatorComponent
{
	public override object Eval(string code, AbstractContext context)
	{
		return CS.Eval(new CS.Context(context), code);
	}
}
