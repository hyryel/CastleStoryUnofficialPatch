using System;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class JObjectCustomModifier : IJObjectLocalModifier
{
	protected Action<JObject, JContainer> func;

	public JObjectCustomModifier(Action<JObject, JContainer> conversionFunction)
	{
		func = conversionFunction;
	}

	public JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		JContainer jContainer = allData.DeepClone() as JContainer;
		foreach (JContainer item in toBeModifiedJObjectFetcher.Filter(jContainer))
		{
			func(item as JObject, jContainer);
		}
		return jContainer;
	}
}
