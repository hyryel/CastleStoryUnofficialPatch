namespace Brix.Compile;

public class Boo
{
	public class Context : AbstractContext
	{
		internal BooEvaluator evaluator;

		public Context()
		{
			evaluator = new BooEvaluator();
		}

		public Context(AbstractContext other)
			: this()
		{
			Copy(other);
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
