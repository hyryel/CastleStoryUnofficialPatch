using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class HasComponentOfTypeFilter : IJObjectFilter
{
	private string _typeName;

	public HasComponentOfTypeFilter(string typename)
	{
		_typeName = typename;
	}

	public bool Matches(JContainer container)
	{
		JArray jArray = container["components"].Value<JArray>();
		foreach (JToken item in jArray)
		{
			if (item["$type"] != null && item["$type"].Value<string>() == _typeName)
			{
				return true;
			}
		}
		return false;
	}
}
