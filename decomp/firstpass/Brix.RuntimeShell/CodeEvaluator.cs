using System;
using System.Linq;
using Brix.Compile;
using Brix.RuntimeShell.Eval;
using Brix.RuntimeShell.Out;
using UnityEngine;

namespace Brix.RuntimeShell;

public class CodeEvaluator : MonoBehaviour
{
	[Serializable]
	public class LanguageBinding
	{
		public string language;

		public CodeEvaluatorComponent evaluator;
	}

	public LanguageBinding[] languages;

	public string[] imports;

	public string[] defines;

	public DrawerComponent output;

	public void SendEvalRequest(string code, string language)
	{
		Entry entry = new Entry();
		entry.input = code.Trim();
		entry.language = language;
		Entry entry2 = entry;
		try
		{
			entry2.output = Eval(code, language);
		}
		catch (Exception error)
		{
			entry2.error = error;
		}
		if (output != null)
		{
			output.AddEntry(entry2);
			output.Refresh();
		}
	}

	private object Eval(string code, string language)
	{
		AbstractContext abstractContext = new AbstractContext();
		abstractContext.Namespaces = imports;
		abstractContext.Defines = defines;
		LanguageBinding languageBinding = languages.FirstOrDefault((LanguageBinding b) => b.language == language);
		if (languageBinding != null)
		{
			return languageBinding.evaluator.Eval(code, abstractContext);
		}
		throw new Exception("Unsupported language");
	}
}
