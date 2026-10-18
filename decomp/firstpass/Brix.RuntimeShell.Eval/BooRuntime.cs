using Brix.Compile;

namespace Brix.RuntimeShell.Eval;

public class BooRuntime : CodeEvaluatorComponent
{
	public override object Eval(string code, AbstractContext context)
	{
		return Boo.Eval(new Boo.Context(context), code);
	}
}
