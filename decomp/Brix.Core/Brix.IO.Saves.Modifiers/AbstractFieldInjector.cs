using System.Linq;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public abstract class AbstractFieldInjector : IJObjectLocalModifier
{
	protected string _fieldname;

	protected InjectionMode _injectionMode;

	protected AbstractFieldInjector(string fieldname, InjectionMode injectionMode)
	{
		_fieldname = fieldname;
		_injectionMode = injectionMode;
	}

	public virtual JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		JContainer jContainer = allData.DeepClone() as JContainer;
		JContainer objectToInsert = GetJContainerToInsert(allData);
		foreach (JObject item in toBeModifiedJObjectFetcher.Filter(allData).OfType<JObject>())
		{
			if (item[_fieldname] != null && item[_fieldname] is JObject)
			{
				JObjectEditor.RemovePropertyField(item, _fieldname, jContainer);
			}
			if (item[_fieldname] is JArray)
			{
				JObjectInjector.InjectInArray(-1, objectToInsert, item[_fieldname] as JArray, jContainer, _injectionMode);
			}
			else
			{
				JObjectInjector.InjectInProperty(_fieldname, ref objectToInsert, item, jContainer, _injectionMode);
			}
		}
		return jContainer;
	}

	protected abstract JContainer GetJContainerToInsert(JContainer allData);
}
