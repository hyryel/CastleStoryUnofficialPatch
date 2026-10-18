using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class FieldNameEqual : IJObjectFilter
{
	private string _fieldname;

	public FieldNameEqual(string fieldname)
	{
		_fieldname = fieldname;
	}

	public bool Matches(JContainer container)
	{
		return container[_fieldname] != null;
	}
}
