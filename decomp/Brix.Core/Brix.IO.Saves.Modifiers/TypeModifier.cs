using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class TypeModifier : IJObjectLocalModifier
{
	private string newType;

	private List<ComponentChangeStruct> changes = new List<ComponentChangeStruct>();

	public TypeModifier(string newComponentType)
	{
		newType = newComponentType;
	}

	public JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		JContainer jContainer = allData.DeepClone() as JContainer;
		foreach (JContainer item in toBeModifiedJObjectFetcher.Filter(jContainer))
		{
			changes.Add(new ComponentChangeStruct(GetClosestJGameObjectParentId(item), item["$type"].Value<string>()));
			item["$type"] = newType;
		}
		foreach (ComponentChangeStruct change in changes.Where((ComponentChangeStruct componentChangeStruct) => !string.IsNullOrEmpty(componentChangeStruct.id)))
		{
			JObjectFetcher jObjectFetcher = JObjectFetcher.New().IdOrRef(change.id).Make();
			foreach (JObject item2 in from jobject in jObjectFetcher.Fetch(jContainer).OfType<JObject>()
				where jobject["$component"] != null && jobject["$component"].Value<string>().Equals(change.oldType)
				select jobject)
			{
				item2["$component"] = newType;
			}
		}
		return jContainer;
	}

	private string GetClosestJGameObjectParentId(JContainer componentJObject)
	{
		JObject jObject = componentJObject.Ancestors().OfType<JObject>().FirstOrDefault(IsJGameObject);
		return (jObject == null) ? "" : jObject["$id"].Value<string>();
	}

	private bool IsJGameObject(JObject parent)
	{
		JToken jToken = parent["$type"];
		return jToken != null && jToken.Value<string>() == "UnityEngine.GameObject, UnityEngine";
	}
}
