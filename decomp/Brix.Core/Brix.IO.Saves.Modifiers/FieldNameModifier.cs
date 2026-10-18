using System.Linq;
using Brix.IO.Serialization.Json;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class FieldNameModifier : IJObjectLocalModifier
{
	private string currentFieldName;

	private string newFieldname;

	public FieldNameModifier(string currentFieldName, string newFieldname)
	{
		this.currentFieldName = currentFieldName;
		this.newFieldname = newFieldname;
	}

	public JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		JContainer jContainer = allData.DeepClone() as JContainer;
		foreach (JObject item in toBeModifiedJObjectFetcher.Filter(jContainer).OfType<JObject>())
		{
			JProperty jProperty = item.Property(currentFieldName);
			jProperty.AddAfterSelf(new JProperty(newFieldname, jProperty.Value.Value<object>()));
			jProperty.Delete();
		}
		return jContainer;
	}
}
