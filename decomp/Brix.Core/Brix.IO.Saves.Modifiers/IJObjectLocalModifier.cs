using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves.Modifiers;

public interface IJObjectLocalModifier
{
	JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData);
}
