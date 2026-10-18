using System;

namespace Brix.NewUI.GameModes;

public class ActionSymbol : CallbackSymbol
{
	public Action value;

	public ActionSymbol(Key key, Action value, params Func<bool>[] conditions)
		: base(key, conditions)
	{
		this.value = value;
	}

	public ActionSymbol(Key key, Action value)
		: base(key)
	{
		this.value = value;
	}
}
