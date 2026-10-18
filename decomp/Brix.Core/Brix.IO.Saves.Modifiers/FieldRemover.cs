using System.Linq;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class FieldRemover : IJObjectLocalModifier
{
	private string _fieldToDelete;

	public FieldRemover(string fieldToDelete)
	{
		_fieldToDelete = fieldToDelete;
	}

	public JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		JContainer jContainer = allData.DeepClone() as JContainer;
		foreach (JObject item in toBeModifiedJObjectFetcher.Filter(allData).OfType<JObject>())
		{
			JObjectEditor.RemovePropertyField(item, _fieldToDelete, jContainer);
		}
		return jContainer;
	}
}
