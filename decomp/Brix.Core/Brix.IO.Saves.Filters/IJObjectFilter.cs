using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Filters;

public interface IJObjectFilter
{
	bool Matches(JContainer container);
}
