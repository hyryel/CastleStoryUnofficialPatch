using System.Linq;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class JObjectFieldInjector : AbstractFieldInjector
{
	private readonly JObjectFetcher _injecterFetcher;

	public JObjectFieldInjector(string fieldname, JObjectFetcher injectionFetcher)
		: base(fieldname, InjectionMode.JObject)
	{
		_injecterFetcher = injectionFetcher;
	}

	protected override JContainer GetJContainerToInsert(JContainer allData)
	{
		return _injecterFetcher.Filter(allData).First();
	}
}
