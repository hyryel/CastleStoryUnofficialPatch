using System.Linq;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class RawJObjectArrayInjector : AbstractFieldInjector
{
	private readonly JContainer _dataToInject;

	public RawJObjectArrayInjector(string fieldToInjectIn, JContainer injectedData)
		: base(fieldToInjectIn, InjectionMode.JObject)
	{
		_dataToInject = injectedData;
	}

	public override JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		JContainer data = allData.DeepClone() as JContainer;
		JContainer objectToInsert = GetJContainerToInsert(allData);
		foreach (JObject item in toBeModifiedJObjectFetcher.Filter(allData).Cast<JObject>())
		{
			JObjectEditor.RemovePropertyField(item, _fieldname, data);
			JObjectInjector.InjectInProperty(_fieldname, ref objectToInsert, item, data, _injectionMode);
		}
		return allData;
	}

	protected override JContainer GetJContainerToInsert(JContainer allData)
	{
		return _dataToInject;
	}
}
