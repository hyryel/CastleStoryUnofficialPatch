using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class FieldValueModifier<T> : IJObjectLocalModifier
{
	protected string fieldName;

	protected T newValue;

	protected FieldValueModifier()
	{
	}

	public FieldValueModifier(string fieldName, T newValue)
	{
		this.fieldName = fieldName;
		this.newValue = newValue;
	}

	public JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		JContainer jContainer = allData.DeepClone() as JContainer;
		foreach (JContainer item in toBeModifiedJObjectFetcher.Filter(jContainer))
		{
			item[fieldName] = JToken.FromObject(newValue);
		}
		return jContainer;
	}
}
