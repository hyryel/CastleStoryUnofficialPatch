using System;

namespace Brix.NewUI.GameModes;

public class CallbackSymbol : Symbol
{
	public Func<bool>[] conditions;

	public CallbackSymbol(Key key, params Func<bool>[] conditions)
		: base(key)
	{
		this.conditions = conditions;
	}
}
