using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Brix.IO.Serialization.Json;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

internal static class JObjectInjector
{
	public static void InjectInProperty(string fieldname, ref JContainer objectToInsert, JObject jObjectToBeInjectedIn, JContainer data, InjectionMode injectionMode = InjectionMode.JObject)
	{
		if (InjectionNeedPropagation(objectToInsert.Path, jObjectToBeInjectedIn.Path, data))
		{
			data.SelectToken(jObjectToBeInjectedIn.Path)[fieldname] = (objectToInsert = ToInjectedInstance(objectToInsert, injectionMode));
			PropagateInjection(data);
		}
		else
		{
			data.SelectToken(jObjectToBeInjectedIn.Path)[fieldname] = ToInjectedReference(objectToInsert, injectionMode);
		}
	}

	public static void InjectInArray(JContainer objectToInsert, JArray jObjectToBeInjectedIn, JContainer data, InjectionMode injectionMode = InjectionMode.JObject)
	{
		InjectInArray(-1, objectToInsert, jObjectToBeInjectedIn, data, injectionMode);
	}

	public static void InjectInArray(int arrayIndex, JContainer objectToInsert, JArray jArrayToBeInjectedIn, JContainer data, InjectionMode injectionMode = InjectionMode.JObject)
	{
		int num = ((arrayIndex == -1) ? arrayIndex : (jArrayToBeInjectedIn.Count - 1));
		if (InjectionNeedPropagation(objectToInsert.Path, jArrayToBeInjectedIn.Path + "[" + num + "]", data))
		{
			ToInjectedInstance(objectToInsert, injectionMode);
		}
		else
		{
			ToInjectedReference(objectToInsert, injectionMode);
		}
		if (num != -1 && num > jArrayToBeInjectedIn.Count - 1)
		{
			throw new IndexOutOfRangeException("Injection of the arrayIndex");
		}
		if (arrayIndex == -1)
		{
			(data.SelectToken(jArrayToBeInjectedIn.Path) as JArray).Add(objectToInsert);
		}
		else
		{
			(data.SelectToken(jArrayToBeInjectedIn.Path) as JArray).Insert(arrayIndex, objectToInsert);
		}
		PropagateInjection(data);
	}

	private static JContainer ToInjectedReference(JContainer objectToInsert, InjectionMode injectionMode)
	{
		JObject jObject = new JObject();
		jObject.Add("$ref", objectToInsert.Id());
		return jObject;
	}

	private static JContainer ToInjectedInstance(JContainer objectToInsert, InjectionMode injectionMode)
	{
		JContainer jContainer = objectToInsert.DeepClone() as JContainer;
		if (injectionMode == InjectionMode.JGameObject && jContainer["$component"] != null)
		{
			jContainer["$component"].Parent.Delete();
		}
		return jContainer;
	}

	private static bool InjectionNeedPropagation(string objectToInsert, string insertionPoint, JContainer allDataCopy)
	{
		if (string.IsNullOrEmpty(objectToInsert))
		{
			return true;
		}
		int num = 0;
		string[] array = insertionPoint.Split('.');
		string[] array2 = objectToInsert.Split('.');
		string pathNodeName;
		string pathNodeName2;
		while (true)
		{
			pathNodeName = GetPathNodeName(array[num]);
			pathNodeName2 = GetPathNodeName(array2[num]);
			if (!(pathNodeName == pathNodeName2))
			{
				break;
			}
			if (int.TryParse(Regex.Match(array[num], "(\\d+)").Value, out var result) && int.TryParse(Regex.Match(array2[num], "(\\d+)").Value, out var result2) && result != result2)
			{
				return result2 > result;
			}
			if (array.Length == ++num || array2.Length == num)
			{
				return true;
			}
			bool flag = true;
		}
		foreach (JProperty item in allDataCopy.OfType<JProperty>())
		{
			if (item.Name == pathNodeName)
			{
				return true;
			}
			if (item.Name == pathNodeName2)
			{
				return false;
			}
		}
		throw new Exception("No path were able to be resolved for either InsertionPoint or the ObjectToInsert.\nInsertionPoint Path:" + insertionPoint + "\nObjectToInsert Path:" + objectToInsert);
	}

	private static void PropagateInjection(JContainer allData)
	{
		bool flag = true;
		while (flag)
		{
			flag = false;
			Dictionary<string, JContainer> dictionary = new Dictionary<string, JContainer>();
			foreach (JObject item in JObjectFetcher.New().Make().Filter(allData)
				.OfType<JObject>())
			{
				string text = item.Id();
				string text2 = item.Ref();
				if (text != null)
				{
					if (dictionary.ContainsKey(text))
					{
						ConvertToReference(text, item);
						flag = true;
						break;
					}
					dictionary.Add(text, item);
				}
				else if (text2 != null && !dictionary.ContainsKey(text2))
				{
					ConvertToInstance(text2, item, allData);
					flag = true;
					break;
				}
			}
		}
	}

	private static string GetPathNodeName(string pathNodeWithJArrayIndex)
	{
		return pathNodeWithJArrayIndex.Substring(0, pathNodeWithJArrayIndex.LastIndexOf('['));
	}

	private static void ConvertToInstance(string id, JObject tokenToConvert, JContainer data)
	{
		if (JObjectFetcher.New().Id(id).Make()
			.Filter(data.DeepClone() as JContainer)
			.FirstOrDefault() is JObject jObject)
		{
			tokenToConvert.Replace(jObject);
			ConvertToReference(id, jObject);
		}
		else
		{
			tokenToConvert.Delete();
		}
	}

	private static void ConvertToReference(string id, JObject tokenToConvert)
	{
		JObject jObject = new JObject();
		jObject.Add("$ref", id);
		if (tokenToConvert["$component"] != null)
		{
			jObject.Add("$component", tokenToConvert["$component"]);
		}
		tokenToConvert.Replace(jObject);
	}
}
