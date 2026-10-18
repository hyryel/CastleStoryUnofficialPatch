using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class EitherFilter : IJObjectFilter
{
	private List<IJObjectFilter> filters = new List<IJObjectFilter>();

	public EitherFilter(params IJObjectFilter[] filters)
	{
		this.filters = filters.ToList();
	}

	public bool Matches(JContainer container)
	{
		return filters.Any((IJObjectFilter filter) => filter.Matches(container));
	}
}
