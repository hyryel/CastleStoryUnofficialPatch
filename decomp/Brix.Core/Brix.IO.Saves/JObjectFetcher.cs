using System.Collections.Generic;
using System.Linq;
using Brix.IO.Saves.Filters;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public class JObjectFetcher : AbstractFetcher
{
	public class JObjectFetcherBuilder : BaseFetcherBuilder<JObjectFetcherBuilder, JObjectFetcher>
	{
		private JObjectFetcher HierarchyFilter;

		internal JObjectFetcherBuilder()
		{
		}

		public JObjectFetcherBuilder NestedIn(JObjectFetcher filter)
		{
			HierarchyFilter = filter;
			return this;
		}

		public override JObjectFetcher Make()
		{
			JObjectFetcher jObjectFetcher = base.Make();
			jObjectFetcher.HierarchyFilter = HierarchyFilter;
			if (HierarchyFilter != null)
			{
				jObjectFetcher.filters.Insert(0, new DuplicateRemovingFilter(jObjectFetcher.evaluatedPaths));
			}
			return jObjectFetcher;
		}
	}

	private JObjectFetcher HierarchyFilter;

	public static JObjectFetcherBuilder New()
	{
		return new JObjectFetcherBuilder();
	}

	protected override IEnumerable<JContainer> JObjectsToFilter(JContainer jContainer)
	{
		if (HierarchyFilter == null)
		{
			return base.JObjectsToFilter(jContainer);
		}
		return (from container in HierarchyFilter.Filter(jContainer)
			where !HasBeenEvaluatedBefore(container)
			select container).Descendants().OfType<JObject>().Cast<JContainer>();
	}

	public JContainer Delete(JContainer jContainer)
	{
		JContainer jContainer2 = jContainer.DeepClone() as JContainer;
		List<string> list = FetchIds(jContainer).ToList();
		foreach (string item in list)
		{
			DeleteByID(jContainer2, item);
		}
		JContainer jContainer3;
		while ((jContainer3 = Fetch(jContainer2).FirstOrDefault()) != null)
		{
			DeleteByPath(jContainer2, jContainer2.SelectToken(jContainer3.Path) as JObject);
		}
		return jContainer2;
	}

	private IEnumerable<string> FetchIds(JContainer jContainer)
	{
		return from jObject in Fetch(jContainer)
			where jObject["$id"] != null
			select jObject["$id"].Value<string>();
	}

	private void DeleteByID(JContainer containerCopy, string jObjectToDelete)
	{
		JObject originalJObjectToRemove = (from jObject in containerCopy.Descendants().OfType<JObject>()
			where jObject["$id"] != null && jObject["$id"].Value<string>() == jObjectToDelete
			select jObject).First();
		JObjectEditor.RemoveJObject(originalJObjectToRemove, containerCopy);
	}

	private static bool DeleteByPath(JContainer allData, JObject jObjectToDelete)
	{
		if (jObjectToDelete == null)
		{
			return false;
		}
		JObjectEditor.RemoveJObject(jObjectToDelete, allData);
		return true;
	}
}
