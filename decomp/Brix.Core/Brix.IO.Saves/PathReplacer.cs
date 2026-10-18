using Brix.IO.Saves.Modifiers;
using Brix.Utils;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public class PathReplacer : IJObjectLocalModifier
{
	private string tokenToReplace;

	private string tokenReplacing;

	public PathReplacer(string tokenToReplace, string tokenReplacing)
	{
		this.tokenToReplace = tokenToReplace;
		this.tokenReplacing = tokenReplacing;
	}

	public JContainer ApplyChange(AbstractFetcher toBeModifiedJObjectFetcher, JContainer allData)
	{
		foreach (JContainer item in toBeModifiedJObjectFetcher.Fetch(allData))
		{
			if (item["$path"] != null)
			{
				item["$path"] = item["$path"].Value<string>().ReplaceFirst(tokenToReplace, tokenReplacing);
			}
		}
		return allData;
	}
}
