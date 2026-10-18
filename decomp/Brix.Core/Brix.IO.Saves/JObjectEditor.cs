using System.Collections.Generic;
using System.Linq;
using Brix.IO.Serialization.Json;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

internal static class JObjectEditor
{
	private static readonly JObjectFetcher _jobjectWithIdsFetcher = JObjectFetcher.New().WithId().Make();

	private static readonly JObjectFetcher _jobjectWithRefsFetcher = JObjectFetcher.New().WithIdRefOrParent().Make();

	public static void RemovePropertyField(JObject originalFieldContainer, string fieldToDelete, JContainer data)
	{
		if (originalFieldContainer[fieldToDelete] == null)
		{
			return;
		}
		JObject jObject = FetchJObjectInModifiedDataSet(originalFieldContainer, data);
		if (jObject != null)
		{
			jObject.Property(fieldToDelete).Delete();
			if (originalFieldContainer[fieldToDelete].Type == JTokenType.Object || originalFieldContainer[fieldToDelete].Type == JTokenType.Array)
			{
				PropagatedJObject(originalFieldContainer.Property(fieldToDelete).Value as JContainer, data);
			}
		}
	}

	public static void ReplaceJObjectWithValue(JObject originalJObjectToReplace, JValue newValue, JContainer data, bool removeIfNullInArray = true)
	{
		JObject jObject = FetchJObjectInModifiedDataSet(originalJObjectToReplace, data);
		if (jObject != null)
		{
			jObject.Replace(newValue);
			ReplaceRefsToValue(originalJObjectToReplace, newValue, data, removeIfNullInArray);
			PropagatedJObject(originalJObjectToReplace, data);
		}
	}

	public static void RemoveJObject(JObject originalJObjectToRemove, JContainer data)
	{
		JObject jObject = FetchJObjectInModifiedDataSet(originalJObjectToRemove, data);
		if (jObject != null)
		{
			RemoveAllRefs(originalJObjectToRemove, data);
			if (jObject.Parent.Type == JTokenType.Array)
			{
				jObject.Delete();
			}
			else
			{
				jObject.Replace(JValue.CreateNull());
			}
			PropagatedJObject(originalJObjectToRemove, data);
		}
	}

	private static void ReplaceRefsToValue(JObject originalJObjectToReplace, JValue newValue, JContainer data, bool removeIfNullInArray)
	{
		if (string.IsNullOrEmpty(originalJObjectToReplace.Id()))
		{
			return;
		}
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().IdOrRef(originalJObjectToReplace.Id()).Make();
		List<JContainer> list = new List<JContainer>();
		foreach (JContainer item in jObjectFetcher.Fetch(data))
		{
			if (removeIfNullInArray && newValue == null && item.Parent.Type == JTokenType.Array)
			{
				list.Add(item);
			}
			else
			{
				item.Replace(newValue);
			}
		}
		foreach (JContainer item2 in list)
		{
			item2.Delete();
		}
	}

	private static void RemoveAllRefs(JObject originalJObjectToReplace, JContainer data)
	{
		if (string.IsNullOrEmpty(originalJObjectToReplace.Id()))
		{
			return;
		}
		JObjectFetcher jObjectFetcher = JObjectFetcher.New().WithRef().IdOrRef(originalJObjectToReplace.Id())
			.Make();
		List<JContainer> list = new List<JContainer>();
		foreach (JContainer item in jObjectFetcher.Fetch(data))
		{
			if (item.Parent.Type == JTokenType.Array)
			{
				list.Add(item);
			}
			else
			{
				item.Replace(JValue.CreateNull());
			}
		}
		foreach (JContainer item2 in list)
		{
			item2.Delete();
		}
	}

	private static JObject FetchJObjectInModifiedDataSet(JObject jobjectSource, JContainer data)
	{
		string text = jobjectSource.Id();
		if (string.IsNullOrEmpty(text))
		{
			return data.SelectToken(jobjectSource.Path) as JObject;
		}
		return JObjectFetcher.New().Id(text).Make()
			.Filter(data)
			.Cast<JObject>()
			.FirstOrDefault();
	}

	private static void PropagatedJObject(JContainer propagatedJObject, JContainer data)
	{
		bool flag = true;
		while (flag)
		{
			Dictionary<string, JContainer> dictionary = GenerateIdToJObjectDictionary(propagatedJObject);
			flag = false;
			foreach (JContainer item in _jobjectWithRefsFetcher.Filter(data))
			{
				string text = item.Id();
				if (text != null)
				{
					dictionary.Remove(text);
				}
				JContainer jContainer = item;
				string text2 = jContainer.RefOrParent();
				if (text2 == null || !dictionary.ContainsKey(text2))
				{
					continue;
				}
				ConvertToInstance(text2, jContainer, dictionary[text2].DeepClone() as JContainer);
				JToken jToken = propagatedJObject.Ancestors().LastOrDefault() ?? propagatedJObject;
				ConvertToReference(text2, jToken.SelectToken(dictionary[text2].Path) as JObject);
				flag = true;
				break;
			}
		}
	}

	private static Dictionary<string, JContainer> GenerateIdToJObjectDictionary(JContainer fieldCopy)
	{
		Dictionary<string, JContainer> dictionary = _jobjectWithIdsFetcher.Filter(fieldCopy).ToDictionary((JContainer jobject) => jobject["$id"].Value<string>());
		if (fieldCopy.Type == JTokenType.Object && fieldCopy.Id() != null)
		{
			dictionary.Add(fieldCopy.Id(), fieldCopy);
		}
		return dictionary;
	}

	private static void ConvertToInstance(string id, JContainer jObjectWithARef, JContainer newInstance)
	{
		if (jObjectWithARef["$component"] != null)
		{
			newInstance.AddFirst(new JProperty("$component", jObjectWithARef["$component"]));
		}
		if (jObjectWithARef.Ref() != null)
		{
			jObjectWithARef.Replace(newInstance);
		}
		else
		{
			jObjectWithARef["$parent"].Replace(newInstance);
		}
	}

	private static void ConvertToReference(string id, JObject tokenToRemove)
	{
		JObject jObject = new JObject();
		jObject.Add("$ref", id);
		if (tokenToRemove["$component"] != null)
		{
			jObject.Add("$component", tokenToRemove["$component"]);
		}
		tokenToRemove.Replace(jObject);
	}
}
