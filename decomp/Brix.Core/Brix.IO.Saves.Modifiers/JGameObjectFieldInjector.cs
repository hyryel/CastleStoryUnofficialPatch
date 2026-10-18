using System.Linq;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public class JGameObjectFieldInjector : AbstractFieldInjector
{
	private readonly JGameObjectFetcher _injecterFetcher;

	public JGameObjectFieldInjector(string fieldname, JGameObjectFetcher injectionFetcher)
		: base(fieldname, InjectionMode.JGameObject)
	{
		_injecterFetcher = injectionFetcher;
	}

	protected override JContainer GetJContainerToInsert(JContainer allData)
	{
		return _injecterFetcher.Filter(allData).First();
	}
}
