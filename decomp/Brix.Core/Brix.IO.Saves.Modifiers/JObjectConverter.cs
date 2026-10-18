using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class JObjectConverter : IJObjectLocalModifier
{
	private string _fieldToDelete;

	private Func<JObject, JContainer, string> _conversionFunction;

	public JObjectConverter(Func<JObject, JContainer, string> conversionFunction)
	{
		_conversionFunction = conversionFunction;
	}

	public JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		JContainer jContainer = allData.DeepClone() as JContainer;
		foreach (JObject item in toBeModifiedJObjectFetcher.Filter(allData).Cast<JObject>())
		{
			string value = _conversionFunction(item, jContainer);
			JObjectEditor.ReplaceJObjectWithValue(item, new JValue(value), jContainer);
		}
		return jContainer;
	}
}
