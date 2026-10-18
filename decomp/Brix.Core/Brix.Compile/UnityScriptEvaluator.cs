#define DEBUG
using System;
using System.Diagnostics;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.IO;
using UnityScript;
using UnityScript.Scripting;
using UnityScript.Scripting.Pipeline;
using UnityScript.Steps;

namespace Brix.Compile;

internal class UnityScriptEvaluator
{
	private static object TaintedAnnotation = new object();

	private EvaluationContext _context;

	private string _code;

	private EvaluationScriptCacheKey _cacheKey;

	private CompilerContext _compilationResult;

	public static object Eval(EvaluationContext context, string code)
	{
		return new UnityScriptEvaluator(context, code).Run();
	}

	public static void Taint(CompileUnit cu)
	{
		cu[TaintedAnnotation] = TaintedAnnotation;
	}

	public static bool IsTainted(CompileUnit cu)
	{
		return cu.ContainsAnnotation(TaintedAnnotation);
	}

	public UnityScriptEvaluator(EvaluationContext context, string code)
	{
		_context = context;
		_code = code;
		_cacheKey = new EvaluationScriptCacheKey(context.GetType(), code);
	}

	public object Run()
	{
		Type scriptType = CompileScript();
		EvaluationScript evaluationScript = ActivateScript(scriptType);
		return evaluationScript.Run();
	}

	public EvaluationScript ActivateScript(Type scriptType)
	{
		EvaluationScript evaluationScript = (EvaluationScript)Activator.CreateInstance(scriptType, _context);
		_context.AddScript(evaluationScript);
		return evaluationScript;
	}

	private Type CompileScript()
	{
		Type cachedScript = GetCachedScript();
		if (cachedScript != null)
		{
			return cachedScript;
		}
		cachedScript = DoCompile();
		CacheScript(cachedScript);
		return cachedScript;
	}

	private Type GetCachedScript()
	{
		return GetEvaluationDomain().GetCachedScript(_cacheKey);
	}

	private void CacheScript(Type type)
	{
		if (!IsTainted(_compilationResult.CompileUnit))
		{
			GetEvaluationDomain().CacheScript(_cacheKey, type);
		}
	}

	private EvaluationDomain GetEvaluationDomain()
	{
		EvaluationDomain evaluationDomain = _context.ScriptContainer.GetEvaluationDomain();
		Debug.Assert(evaluationDomain != null);
		return evaluationDomain;
	}

	private Type DoCompile()
	{
		UnityScriptCompiler unityScriptCompiler = new UnityScriptCompiler();
		unityScriptCompiler.Parameters.Pipeline = AdjustPipeline(_context, UnityScriptCompiler.Pipelines.CompileToMemory());
		unityScriptCompiler.Parameters.ScriptBaseType = typeof(EvaluationScript);
		unityScriptCompiler.Parameters.GlobalVariablesBecomeFields = false;
		unityScriptCompiler.Parameters.ScriptMainMethod = "Run";
		unityScriptCompiler.Parameters.Input.Add(new StringInput("script", _code + ";"));
		unityScriptCompiler.Parameters.Debug = false;
		unityScriptCompiler.Parameters.GenerateInMemory = true;
		Assembly assembly = _context.GetType().Assembly;
		unityScriptCompiler.Parameters.References.Add(assembly);
		AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
		foreach (AssemblyName assemblyRef in referencedAssemblies)
		{
			unityScriptCompiler.Parameters.References.Add(Assembly.Load(assemblyRef));
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly2 in assemblies)
		{
			unityScriptCompiler.Parameters.References.Add(assembly2);
		}
		_compilationResult = unityScriptCompiler.Run();
		if (_compilationResult.Errors.Count > 0)
		{
			throw new CompilationErrorsException(_compilationResult.Errors);
		}
		return _compilationResult.GeneratedAssembly.GetType("script");
	}

	private static CompilerPipeline AdjustPipeline(EvaluationContext context, CompilerPipeline pipeline)
	{
		pipeline.InsertAfter(typeof(IntroduceUnityGlobalNamespaces), new IntroduceScriptingNamespace(context));
		pipeline.InsertAfter(typeof(IntroduceScriptingNamespace), new IntroduceImports(context));
		pipeline.InsertAfter(typeof(ApplySemantics), new IntroduceEvaluationContext(context));
		pipeline.Replace(typeof(ProcessUnityScriptMethods), new ProcessScriptingMethods(context));
		pipeline.InsertAfter(typeof(ProcessScriptingMethods), new IntroduceReturnValue());
		return pipeline;
	}
}
