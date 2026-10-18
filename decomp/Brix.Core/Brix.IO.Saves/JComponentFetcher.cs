using System.Collections.Generic;
using System.Linq;
using Brix.IO.Serialization.Json;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public class JComponentFetcher : AbstractFetcher
{
	public class JComponentFetcherBuilder : BaseFetcherBuilder<JComponentFetcherBuilder, JComponentFetcher>
	{
		private JGameObjectFetcher HierarchyFilter;

		internal JComponentFetcherBuilder()
		{
		}

		public JComponentFetcherBuilder NestedIn(JGameObjectFetcher filter)
		{
			HierarchyFilter = filter;
			return this;
		}

		public override JComponentFetcher Make()
		{
			JComponentFetcher jComponentFetcher = base.Make();
			jComponentFetcher.HierarchyFilter = HierarchyFilter ?? JGameObjectFetcher.New().Make();
			return jComponentFetcher;
		}
	}

	private JGameObjectFetcher HierarchyFilter;

	public static JComponentFetcherBuilder New()
	{
		return new JComponentFetcherBuilder();
	}

	protected override IEnumerable<JContainer> JObjectsToFilter(JContainer jContainer)
	{
		return (from jGo in HierarchyFilter.Filter(jContainer)
			where jGo["components"] != null
			select jGo).SelectMany((JContainer jGo) => from container in jGo["components"].Children<JContainer>()
			select DereferenceComponentReference(container));
	}

	private JContainer DereferenceComponentReference(JContainer container)
	{
		string text = container.Ref();
		return (text == null) ? container : FindReferencedComponent(container, text);
	}

	private static JContainer FindReferencedComponent(JContainer container, string refId)
	{
		return (from c in JObjectFetcher.New().Type(container["$component"].Value<string>()).Make()
				.Fetch(container.Parent)
			where GetGOId(c) == refId
			select c).First();
	}

	public JContainer Delete(JContainer jContainer)
	{
		JContainer jContainer2 = jContainer.DeepClone() as JContainer;
		List<JContainer> list = Fetch(jContainer).ToList();
		List<string> list2 = new List<string>();
		foreach (JContainer item in list)
		{
			string gOId = GetGOId(item);
			string text = item["$type"].Value<string>();
			list2.Add(gOId);
			DeleteByGOID(jContainer2, gOId, text);
			jContainer2 = JObjectFetcher.New().IdOrRef(gOId).FieldEqualsTo("$component", text)
				.Make()
				.Delete(jContainer2);
		}
		return jContainer2;
	}

	private void DeleteByGOID(JContainer containerCopy, string goId, string type)
	{
		JGameObjectFetcher filter = JGameObjectFetcher.New().Id(goId).Make();
		JComponentFetcher jComponentFetcher = New().Type(type).NestedIn(filter).Make();
		JObjectEditor.RemoveJObject(jComponentFetcher.Fetch(containerCopy).OfType<JObject>().FirstOrDefault(), containerCopy);
	}

	private static string GetGOId(JContainer jComponentToDelete)
	{
		JContainer parent = jComponentToDelete.Parent.Parent.Parent;
		while (parent != null && !parent.IsJGameObject())
		{
			parent = parent.Parent;
		}
		return parent?.Id();
	}
}
