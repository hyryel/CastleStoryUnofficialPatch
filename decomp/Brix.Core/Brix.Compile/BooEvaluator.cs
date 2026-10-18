using System;
using System.Linq;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.MetaProgramming;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Reflection;

namespace Brix.Compile;

public class BooEvaluator
{
	public BooCodeBuilder CodeBuilder = new BooCodeBuilder();

	public ReflectionTypeSystemProvider TypeProvider = new ReflectionTypeSystemProvider();

	public CompilerContext Parse(string code)
	{
		BooCompiler booCompiler = new BooCompiler();
		booCompiler.Parameters.Input.Add(new StringInput("script", code));
		booCompiler.Parameters.Pipeline = new CompileToMemory();
		booCompiler.Parameters.Ducky = true;
		return booCompiler.Run();
	}

	public CompilerContext WrapExpressions(CompilerContext context)
	{
		ClassDefinition classDefinition = (ClassDefinition)Parse("class EvalClass (object):\n    public static _result as object\n    static def WriteResult (o as object):\n        _result = o\n    private def Example ():\n        WriteResult(null)\n").CompileUnit.Modules[0].Members[0];
		Method method = (Method)classDefinition.Members[2];
		ExpressionStatement expressionStatement = (ExpressionStatement)method.Body.Statements[0];
		MethodInvocationExpression methodInvocationExpression = (MethodInvocationExpression)expressionStatement.Expression;
		ClassDefinition classDefinition2 = (ClassDefinition)context.CompileUnit.Modules[0].Members[0];
		Method method2 = (Method)classDefinition2.Members[0];
		context.CompileUnit.Modules[0].Members.Add(classDefinition);
		classDefinition2.BaseTypes[0] = new SimpleTypeReference(classDefinition.Name);
		StatementCollection statements = method2.Body.Statements;
		Statement[] array = statements.ToArray();
		foreach (Statement statement in array)
		{
			if (statement.NodeType == NodeType.ExpressionStatement)
			{
				Expression expression = ((ExpressionStatement)statement).Expression;
				MethodInvocationExpression methodInvocationExpression2 = methodInvocationExpression.CloneNode();
				methodInvocationExpression2.Arguments[0] = expression;
				((ExpressionStatement)statement).Expression = methodInvocationExpression2;
			}
		}
		return Parse(context.CompileUnit.ToCodeString());
	}

	public object Eval(string code)
	{
		CompilerContext compilerContext = Parse(code);
		CompilerError[] enumerable = new CompilerError[0];
		bool flag = false;
		if (compilerContext.Errors.Count > 0)
		{
			enumerable = compilerContext.Errors.ToArray();
			compilerContext = WrapExpressions(compilerContext);
			flag = true;
		}
		if (compilerContext.Errors.Count > 0)
		{
			CompilerErrorCollection compilerErrorCollection = new CompilerErrorCollection();
			compilerErrorCollection.AddRange(enumerable);
			throw new CompilationErrorsException(compilerErrorCollection);
		}
		Assembly generatedAssembly = compilerContext.GeneratedAssembly;
		object result = null;
		if (generatedAssembly != null)
		{
			Type type = generatedAssembly.GetType("ScriptModule");
			if (flag)
			{
				BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.NonPublic;
				MethodInfo method = type.GetMethod("Main", bindingAttr);
				method.Invoke(null, new object[1] { new string[0] });
				result = type.BaseType.GetField("_result").GetValue(null);
			}
			else
			{
				MethodInfo methodInfo = type.GetMethods().FirstOrDefault();
				result = methodInfo.Invoke(null, new object[0]);
			}
		}
		return result;
	}
}
