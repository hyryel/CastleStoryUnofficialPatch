using System.Reflection;
using UnityScript.Scripting;

namespace Brix.Compile;

public class JS
{
	public class Context : AbstractContext
	{
		public EvaluationContext evalContext = new EvaluationContext();

		public Context()
		{
			string[] value = base.Namespaces;
			IEvaluationDomainProvider scriptContainer = evalContext.ScriptContainer;
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			scriptContainer.GetType().GetField("_imports", bindingAttr).SetValue(scriptContainer, value);
		}

		public Context(AbstractContext other)
		{
			Copy(other);
			string[] value = base.Namespaces;
			IEvaluationDomainProvider scriptContainer = evalContext.ScriptContainer;
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			scriptContainer.GetType().GetField("_imports", bindingAttr).SetValue(scriptContainer, value);
		}
	}

	private static Context context;

	public static object Eval(string code)
	{
		if (context == null)
		{
			context = new Context();
		}
		return UnityScriptEvaluator.Eval(context.evalContext, code);
	}

	public static object Eval(Context context, string code)
	{
		return UnityScriptEvaluator.Eval(context.evalContext, code);
	}
}
