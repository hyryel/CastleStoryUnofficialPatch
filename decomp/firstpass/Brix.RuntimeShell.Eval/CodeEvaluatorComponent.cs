using Brix.Compile;
using UnityEngine;

namespace Brix.RuntimeShell.Eval;

public abstract class CodeEvaluatorComponent : MonoBehaviour
{
	public abstract object Eval(string code, AbstractContext context);

	public object Eval(string code)
	{
		return Eval(code, null);
	}
}
