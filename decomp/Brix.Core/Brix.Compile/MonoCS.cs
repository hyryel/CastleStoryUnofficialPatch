namespace Brix.Compile;

public class MonoCS
{
	public class Context : AbstractContext
	{
		internal CSharpEvaluator evaluator;

		public Context()
		{
			evaluator = new CSharpEvaluator(this);
		}

		public Context(AbstractContext other)
		{
			Copy(other);
			evaluator = new CSharpEvaluator(this);
		}
	}

	private static Context context = null;

	public static object Eval(string code)
	{
		if (context == null)
		{
			context = new Context();
		}
		return context.evaluator.Eval(code);
	}

	public static object Eval(Context context, string code)
	{
		return context.evaluator.Eval(code);
	}
}
