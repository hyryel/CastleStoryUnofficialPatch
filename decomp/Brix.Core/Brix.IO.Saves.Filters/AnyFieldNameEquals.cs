using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public class AnyFieldNameEquals : IJObjectFilter
{
	private string _fieldname;

	private string _fieldname2;

	private string _fieldname3;

	public AnyFieldNameEquals(string fieldname, string fieldname2, string fieldname3)
	{
		_fieldname = fieldname;
		_fieldname2 = fieldname2;
		_fieldname3 = fieldname3;
	}

	public bool Matches(JContainer container)
	{
		return container[_fieldname] != null || container[_fieldname2] != null || container[_fieldname3] != null;
	}
}
