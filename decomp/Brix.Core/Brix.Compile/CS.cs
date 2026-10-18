namespace Brix.Compile;

public class CS
{
	public class Context : AbstractContext
	{
		public Context()
		{
		}

		public Context(AbstractContext other)
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
		return CSharpEvaluator2.Eval(context, code);
	}

	public static object Eval(Context context, string code)
	{
		return CSharpEvaluator2.Eval(context, code);
	}
}
