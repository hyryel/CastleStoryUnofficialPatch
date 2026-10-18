using System;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class RawJObjectFieldInjector : AbstractFieldInjector
{
	private readonly JObject _dataToInject;

	public RawJObjectFieldInjector(string fieldToInjectIn, JContainer injectedData)
		: base(fieldToInjectIn, InjectionMode.JObject)
	{
		_dataToInject = injectedData as JObject;
	}

	protected override JContainer GetJContainerToInsert(JContainer allData)
	{
		return GenerateIDedJObject(_dataToInject);
	}

	private JObject GenerateIDedJObject(JObject dataToInject)
	{
		if (dataToInject["$id"] != null)
		{
			return dataToInject;
		}
		JObject jObject = dataToInject.DeepClone() as JObject;
		jObject["$id"] = Guid.NewGuid().ToString();
		return jObject;
	}
}
