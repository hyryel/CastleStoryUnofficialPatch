using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class DuplicateRemovingFilter : IJObjectFilter
{
	private HashSet<string> evaluatedPaths;

	public DuplicateRemovingFilter(HashSet<string> evaluatedPaths)
	{
		this.evaluatedPaths = evaluatedPaths;
	}

	public bool Matches(JContainer container)
	{
		return evaluatedPaths.Add(container.Path);
	}
}
