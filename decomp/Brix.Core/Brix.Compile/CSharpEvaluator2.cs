using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Brix.Compile;

internal class CSharpEvaluator2
{
	private const string NamespaceName = "_TmpNamespace";

	private const string TypeName = "_TmpType";

	private const string MethodName = "_TmpMethod";

	private AbstractContext _context;

	private CodeDomProvider _provider;

	private CompilerParameters _parameters;

	private string _importCode;

	private int _firstCodeLine;

	public static object Eval(AbstractContext context, string code)
	{
		return new CSharpEvaluator2(context).Run(code);
	}

	public CSharpEvaluator2(AbstractContext context)
	{
		_context = context;
		_provider = GetProvider();
		_parameters = GetParameters(context);
		_importCode = GetImportCode(context);
		_firstCodeLine = context.Defines.Length + context.Namespaces.Length + 1;
	}

	private CodeDomProvider GetProvider()
	{
		CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
		(codeDomProvider.GetType().GetField("providerOptions", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(codeDomProvider) as IDictionary<string, string>)["CompilerVersion"] = "3.0";
		return codeDomProvider;
	}

	private CompilerParameters GetParameters(AbstractContext context)
	{
		CompilerParameters compilerParameters = new CompilerParameters();
		compilerParameters.GenerateInMemory = true;
		compilerParameters.TempFiles.KeepFiles = false;
		Assembly[] assemblies = _context.Assemblies;
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				compilerParameters.ReferencedAssemblies.Add(assembly.Location);
			}
			catch (Exception)
			{
			}
		}
		return compilerParameters;
	}

	private CompilerParameters GetCleanedParameters(CompilerParameters other)
	{
		CompilerParameters compilerParameters = new CompilerParameters();
		compilerParameters.GenerateInMemory = other.GenerateInMemory;
		compilerParameters.TempFiles.KeepFiles = other.TempFiles.KeepFiles;
		compilerParameters.TreatWarningsAsErrors = false;
		StringEnumerator enumerator = other.ReferencedAssemblies.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				compilerParameters.ReferencedAssemblies.Add(current);
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		return compilerParameters;
	}

	private string GetImportCode(AbstractContext context)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] defines = context.Defines;
		foreach (string text in defines)
		{
			stringBuilder.Append("#define " + text + "\n");
		}
		string[] namespaces = context.Namespaces;
		foreach (string text2 in namespaces)
		{
			stringBuilder.Append("using " + text2 + ";\n");
		}
		return stringBuilder.ToString();
	}

	public object Run(string code)
	{
		Assembly assembly = Compile(code);
		Type type = assembly.GetTypes()[0];
		object obj = Activator.CreateInstance(type);
		MethodInfo method = type.GetMethod("_TmpMethod", BindingFlags.Instance | BindingFlags.NonPublic);
		return method.Invoke(obj, new object[0]);
	}

	private Assembly Compile(string code)
	{
		string statements = "";
		string expression = code;
		int val = code.LastIndexOf(";");
		int val2 = code.LastIndexOf("}");
		int num = Math.Max(val, val2);
		if (num >= 0)
		{
			statements = code.Substring(0, num + 1);
			expression = code.Substring(num + 1);
		}
		string standaloneExpression = GetStandaloneExpression(statements, expression);
		CompilerResults compilerResults = _provider.CompileAssemblyFromSource(_parameters, standaloneExpression);
		string text = standaloneExpression;
		CompilerResults compilerResults2 = compilerResults;
		if (HasStatementExpressionMismatchErrors(compilerResults))
		{
			string standaloneStatement = GetStandaloneStatement(code);
			CompilerResults compilerResults3 = _provider.CompileAssemblyFromSource(GetCleanedParameters(_parameters), standaloneStatement);
			if (!HasStatementExpressionMismatchErrors(compilerResults3))
			{
				compilerResults2 = compilerResults3;
				text = standaloneStatement;
			}
		}
		if (compilerResults2.Errors.Count > 0)
		{
			bool flag = false;
			string text2 = "";
			foreach (CompilerError error in compilerResults2.Errors)
			{
				error.FileName = "";
				error.Line -= _firstCodeLine;
				if (error.ErrorNumber == "CS1525" && error.Line > code.Count((char c) => c == '\n'))
				{
					error.ErrorText = "Unexpected end of input";
				}
				text2 = string.Concat(text2, error, "\n");
				if (!error.IsWarning)
				{
					flag = true;
				}
			}
			if (flag)
			{
				throw new Exception("EvalCS() compile error: " + text2 + "\n<Code>\n" + code + "\n</Code>\n\n<Standalone>\n" + text + "\n</Standalone>\n");
			}
		}
		return compilerResults2.CompiledAssembly;
	}

	private string GetStandalone(string methodBody, string returnType)
	{
		return _importCode + "namespace _TmpNamespace { class _TmpType { " + returnType + " _TmpMethod () { \n" + methodBody + "\n; } } }";
	}

	private string GetStandaloneExpression(string statements, string expression)
	{
		return GetStandalone(statements + "; return " + expression, "object");
	}

	private string GetStandaloneStatement(string statements)
	{
		return GetStandalone(statements, "void");
	}

	private bool HasStatementExpressionMismatchErrors(CompilerResults results)
	{
		return results.Errors.Cast<CompilerError>().Any((CompilerError error) => error.ErrorNumber == "CS0126");
	}
}
