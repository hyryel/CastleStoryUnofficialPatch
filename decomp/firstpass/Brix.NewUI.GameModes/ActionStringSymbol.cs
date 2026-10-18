using System;

namespace Brix.NewUI.GameModes;

public class ActionStringSymbol : CallbackSymbol
{
	public Action<string> value;

	public ActionStringSymbol(Key key, Action<string> value, params Func<bool>[] conditions)
		: base(key, conditions)
	{
		this.value = value;
	}

	public ActionStringSymbol(Key key, Action<string> value)
		: base(key)
	{
		this.value = value;
	}
}
