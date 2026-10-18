using System.Collections.Generic;
using System.Linq;
using Brix.IO.Saves.Filters;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public abstract class AbstractFetcher
{
	internal List<IJObjectFilter> filters;

	protected HashSet<string> evaluatedPaths = new HashSet<string>();

	internal AbstractFetcher()
	{
	}

	public IEnumerable<JContainer> Fetch(JContainer jContainer)
	{
		evaluatedPaths.Clear();
		return Filter(jContainer);
	}

	internal IEnumerable<JContainer> Filter(JContainer jContainer)
	{
		return from container in JObjectsToFilter(jContainer)
			where filters.All((IJObjectFilter filter) => filter.Matches(container))
			select container;
	}

	protected bool HasBeenEvaluatedBefore(JContainer container)
	{
		string path = container.Path;
		return !string.IsNullOrEmpty(path) && evaluatedPaths.Contains(path);
	}

	protected virtual IEnumerable<JContainer> JObjectsToFilter(JContainer jContainer)
	{
		return jContainer.Descendants().OfType<JObject>().Cast<JContainer>();
	}
}
