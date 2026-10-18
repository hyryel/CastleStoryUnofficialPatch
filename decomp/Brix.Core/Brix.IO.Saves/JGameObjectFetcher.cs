using System.Collections.Generic;
using System.Linq;
using Brix.IO.Saves.Filters;
using Brix.IO.Serialization.Json;
using Newtonsoft.Json.Linq;
using Smooth.Slinq;

namespace Brix.IO.Saves;

public class JGameObjectFetcher : AbstractFetcher
{
	public class JGameObjectFetcherBuilder : BaseFetcherBuilder<JGameObjectFetcherBuilder, JGameObjectFetcher>
	{
		internal JGameObjectFetcher _hierarchyFilter;

		internal bool _fullHierarchy;

		internal JGameObjectFetcherBuilder()
		{
			Type("UnityEngine.GameObject, UnityEngine");
		}

		public JGameObjectFetcherBuilder Path(string id)
		{
			FieldEqualsTo("$path", id);
			return this;
		}

		public JGameObjectFetcherBuilder AssetKey(string assetkey)
		{
			FieldEqualsTo("$assetKey", assetkey);
			return this;
		}

		public JGameObjectFetcherBuilder Name(string name)
		{
			FieldEqualsTo("name", name);
			return this;
		}

		public JGameObjectFetcherBuilder HasComponentOfType(string componentType)
		{
			filters.Add(new HasComponentOfTypeFilter(componentType));
			return this;
		}

		public JGameObjectFetcherBuilder HasNoComponentOfType(string componentType)
		{
			filters.Add(new HasNoComponentOfTypeFilter(componentType));
			return this;
		}

		public JGameObjectFetcherBuilder IsActive(bool active, bool defaultValueIfMissing)
		{
			FieldEqualsTo("activeSelf", active, defaultValueIfMissing);
			return this;
		}

		public JGameObjectFetcherBuilder NestedIn(JGameObjectFetcher filter)
		{
			_hierarchyFilter = filter;
			_fullHierarchy = true;
			return this;
		}

		public JGameObjectFetcherBuilder ParentIs(JGameObjectFetcher filter)
		{
			_hierarchyFilter = filter;
			_fullHierarchy = false;
			return this;
		}

		public override JGameObjectFetcher Make()
		{
			HierarchyFilter hierarchyFilter = null;
			if (_hierarchyFilter != null)
			{
				hierarchyFilter = (_fullHierarchy ? ((HierarchyFilter)new FullHierarchyFilter(_hierarchyFilter)) : ((HierarchyFilter)new ChildHierarchyFilter(_hierarchyFilter)));
				filters.Add(hierarchyFilter);
			}
			JGameObjectFetcher jGameObjectFetcher = base.Make();
			jGameObjectFetcher._hierarchyFilter = hierarchyFilter;
			return jGameObjectFetcher;
		}
	}

	public abstract class HierarchyFilter : IJObjectFilter
	{
		private JGameObjectFetcher filterFetcher;

		protected List<string> _validParents = new List<string>();

		public HierarchyFilter(JGameObjectFetcher hierarchyFilter)
		{
			filterFetcher = hierarchyFilter;
		}

		public void RefreshParents(JContainer c)
		{
			_validParents = (from container in filterFetcher.Filter(c)
				select container["$id"].Value<string>()).ToList();
		}

		public abstract bool Matches(JContainer container);
	}

	public class FullHierarchyFilter : HierarchyFilter
	{
		public FullHierarchyFilter(JGameObjectFetcher hierarchyFilter)
			: base(hierarchyFilter)
		{
		}

		public override bool Matches(JContainer container)
		{
			string parameter = container["$path"].Value<string>();
			return _validParents.Slinq().Any((string parentId, string _path) => _path.EndsWith(parentId) || _path.Contains(parentId + "/"), parameter);
		}
	}

	public class ChildHierarchyFilter : HierarchyFilter
	{
		public ChildHierarchyFilter(JGameObjectFetcher hierarchyFilter)
			: base(hierarchyFilter)
		{
		}

		public override bool Matches(JContainer container)
		{
			JToken jToken = container["$parent"];
			if (jToken == null)
			{
				return false;
			}
			string text = ((!(jToken is JObject)) ? jToken.Value<string>() : (jToken as JObject).Id());
			return text != null && _validParents.Slinq().Any((string parentId, string current) => parentId == current, text);
		}
	}

	internal HierarchyFilter _hierarchyFilter;

	protected override IEnumerable<JContainer> JObjectsToFilter(JContainer jContainer)
	{
		if (_hierarchyFilter != null)
		{
			_hierarchyFilter.RefreshParents(jContainer);
		}
		return jContainer.Descendants().OfType<JObject>().Cast<JContainer>();
	}

	public static JGameObjectFetcherBuilder New()
	{
		return new JGameObjectFetcherBuilder();
	}

	private IEnumerable<string> FetchIds(JContainer jContainer)
	{
		return from jObject in Fetch(jContainer)
			where jObject["$id"] != null
			select jObject["$id"].Value<string>();
	}

	public JContainer Delete(JContainer jContainer)
	{
		JContainer jContainer2 = jContainer.DeepClone() as JContainer;
		JGameObjectFetcher jGameObjectFetcher = New().ParentIs(this).Make();
		if (jGameObjectFetcher.Fetch(jContainer2).Any())
		{
			jContainer2 = jGameObjectFetcher.Delete(jContainer2);
		}
		List<string> list = FetchIds(jContainer).ToList();
		foreach (string item in list)
		{
			DeleteByID(jContainer2, item);
		}
		return jContainer2;
	}

	private void DeleteByID(JContainer containerCopy, string jObjectToDelete)
	{
		JObject originalJObjectToRemove = (from jObject in containerCopy.Descendants().OfType<JObject>()
			where jObject["$id"] != null && jObject["$id"].Value<string>() == jObjectToDelete
			select jObject).First();
		JObjectEditor.RemoveJObject(originalJObjectToRemove, containerCopy);
	}
}
