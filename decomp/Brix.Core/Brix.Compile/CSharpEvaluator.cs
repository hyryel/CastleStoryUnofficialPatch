using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.CSharp;
using UnityEngine;

namespace Brix.Compile;

internal class CSharpEvaluator
{
	private Assembly[] assemblies = null;

	private string[] namespaces = null;

	private string initNamespaces = null;

	private Action Init = null;

	public AbstractContext Context = null;

	public CSharpEvaluator(AbstractContext context)
	{
		Context = context;
		Init = FirstInit;
	}

	private void InitAssembly()
	{
		assemblies = Context.Assemblies;
	}

	private void PrepareAssembly()
	{
		List<string> list = new List<string>();
		Exception ex = null;
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			try
			{
				Evaluator.ReferenceAssembly(assembly);
			}
			catch (Exception ex2)
			{
				if (ex == null)
				{
					ex = ex2;
				}
				list.Add(assembly.FullName);
			}
		}
		if (ex != null)
		{
			Debug.LogWarning(string.Concat("Failed to load ", list.Count, " assemblies:\n", ex, "\n\n<assemblies>\n\t", string.Join("\n\t", list.ToArray()), "\n</assemblies>\n\n"));
		}
	}

	private void InitNamespace()
	{
		namespaces = Context.Namespaces;
		initNamespaces = "";
		string[] array = namespaces;
		foreach (string text in array)
		{
			initNamespaces = initNamespaces + "using " + text + ";\n";
		}
	}

	private void PrepareNamespace()
	{
		Evaluator.Run(initNamespaces);
	}

	private void FirstInit()
	{
		Init = SubsequentInit;
		InitAssembly();
		InitNamespace();
		SubsequentInit();
		Eval("null");
	}

	private void SubsequentInit()
	{
		PrepareAssembly();
		PrepareNamespace();
	}

	public object Eval(string s)
	{
		Init();
		if (!s.EndsWith(";"))
		{
			s += ";";
		}
		object retvalue = null;
		CompiledMethod compiled;
		string text = Evaluator.Compile(s, out compiled);
		if (text != null)
		{
			throw new Exception("EvalCS() compile error: " + text + "\n\n<Code>\n" + s + "\n</Code>");
		}
		if (compiled == null)
		{
			throw new Exception("EvalCS() compilation failed.\n\n<Code>\n" + s + "\n</Code>");
		}
		compiled(ref retvalue);
		return retvalue;
	}
}
