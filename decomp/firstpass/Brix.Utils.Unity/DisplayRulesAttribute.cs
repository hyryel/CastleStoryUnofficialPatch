using UnityEngine;

namespace Brix.Utils.Unity;

public class DisplayRulesAttribute : PropertyAttribute
{
	private DisplayFlags _rules;

	public DisplayFlags Rules => _rules;

	public DisplayRulesAttribute()
		: this((DisplayFlags)0)
	{
	}

	public DisplayRulesAttribute(DisplayFlags rules)
	{
		_rules = rules;
	}
}
