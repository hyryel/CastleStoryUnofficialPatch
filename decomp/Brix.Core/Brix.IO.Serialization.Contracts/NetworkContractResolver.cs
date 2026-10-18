using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Brix.IO.Serialization.Contracts;

public class NetworkContractResolver : DefaultGameContractResolver
{
	public NetworkContractResolver(string target)
		: base(target)
	{
		_objectContractDictionary[typeof(IEnumerator)] = GenerateIEnumeratorObjectContract;
	}

	protected override JsonContract CreateContract(Type objectType)
	{
		JsonContract jsonContract = base.CreateContract(objectType);
		jsonContract.IsReference = false;
		return jsonContract;
	}

	private JsonObjectContract GenerateIEnumeratorObjectContract(Type objectType)
	{
		JsonObjectContract jsonObjectContract = base.CreateObjectContract(objectType);
		foreach (JsonProperty item in from f in objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			select CreateProperty(f, MemberSerialization.Fields))
		{
			jsonObjectContract.Properties.AddProperty(item);
		}
		return jsonObjectContract;
	}
}
