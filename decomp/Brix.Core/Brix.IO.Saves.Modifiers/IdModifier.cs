using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class IdModifier : IJObjectLocalModifier
{
	protected string newId;

	protected IdModifier()
	{
	}

	public IdModifier(string newId)
	{
		this.newId = newId;
	}

	public JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		IEnumerable<JObject> enumerable = toBeModifiedJObjectFetcher.Filter(allData).OfType<JObject>();
		JObject jObject = null;
		using (IEnumerator<JObject> enumerator = enumerable.GetEnumerator())
		{
			if (!enumerator.MoveNext())
			{
				return allData;
			}
			jObject = enumerator.Current;
			if (enumerator.MoveNext())
			{
				throw new InvalidOperationException($"JObject Id modification must be done on only ONE JObject. The Fecther returned more than one matching entry tried to apply the Id modification '{newId}'. ");
			}
		}
		if (jObject["$ref"] != null)
		{
			throw new InvalidOperationException(string.Format("JObject Id modification can only be done on a JObject without a '$ref' property. Make sure the JObject ID '{0}' requires a change to ID '{1}'", jObject["$ref"].Value<string>(), newId));
		}
		string oldId = jObject["$id"].Value<string>();
		if (oldId.StartsWith("go:") && !newId.StartsWith("go:"))
		{
			newId = "go:" + newId;
		}
		jObject["$id"] = newId;
		if (!string.IsNullOrEmpty(oldId))
		{
			JObjectFetcher jObjectFetcher = JObjectFetcher.New().IdOrRef(oldId).Make();
			foreach (JContainer item in jObjectFetcher.Fetch(allData))
			{
				item["$ref"] = newId;
			}
			JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().Make();
			foreach (JObject item2 in jGameObjectFetcher.Fetch(allData).OfType<JObject>())
			{
				JToken jToken = item2["$path"];
				if (jToken == null)
				{
					continue;
				}
				string[] array = jToken.Value<string>().Split('/');
				if (array.Length == 0 || !array.Any((string pathNode) => pathNode == oldId))
				{
					continue;
				}
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string item3 in array.Where((string pathnode) => !string.IsNullOrEmpty(pathnode)))
				{
					stringBuilder.Append("/");
					stringBuilder.Append((item3 == oldId) ? newId : item3);
				}
				item2["$path"] = stringBuilder.ToString();
			}
		}
		return allData;
	}
}
