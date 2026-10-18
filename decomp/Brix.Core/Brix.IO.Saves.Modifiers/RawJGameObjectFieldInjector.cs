using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class RawJGameObjectFieldInjector : AbstractFieldInjector
{
	private readonly JContainer _dataToInject;

	public RawJGameObjectFieldInjector(string fieldToInjectIn, JContainer injectedData)
		: base(fieldToInjectIn, InjectionMode.JGameObject)
	{
		_dataToInject = injectedData;
	}

	protected override JContainer GetJContainerToInsert(JContainer allData)
	{
		GenerateId(_dataToInject, allData);
		return _dataToInject;
	}

	private void GenerateId(JContainer dataToInject, JContainer allData)
	{
		if (dataToInject["$id"] == null)
		{
			ILookup<string, string> lookup = (from jObject in JGameObjectFetcher.New().Make().Fetch(allData)
				select jObject["$id"].Value<string>()).ToLookup((string s) => s);
			Random random = new Random();
			string text;
			while (lookup.Contains(text = "go:" + random.Next(1073741823, int.MaxValue)))
			{
			}
			dataToInject["$id"] = text;
		}
	}
}
