using System;
using Newtonsoft.Json.Serialization;

namespace Brix.Utils.IO;

public class DynamicFactoryContractCreator : DefaultContractResolver
{
	private Type baseType;

	public DynamicFactoryContractCreator(Type baseType)
	{
		this.baseType = baseType;
	}

	protected override JsonObjectContract CreateObjectContract(Type objectType)
	{
		JsonObjectContract jsonObjectContract = base.CreateObjectContract(objectType);
		if (objectType.IsSubclassOf(baseType))
		{
			jsonObjectContract.DefaultCreator = () => objectType.GetMethod("Instantiate")?.Invoke(null, new object[0]);
		}
		return jsonObjectContract;
	}
}
