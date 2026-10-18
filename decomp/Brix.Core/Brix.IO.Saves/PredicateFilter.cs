using System;
using Brix.IO.Saves.Filters;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public class PredicateFilter : IJObjectFilter
{
	private Func<JContainer, bool> predicate;

	public PredicateFilter(Func<JContainer, bool> predicate)
	{
		this.predicate = predicate;
	}

	public bool Matches(JContainer container)
	{
		return predicate(container);
	}
}
