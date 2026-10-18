using System;

namespace Brix.NewUI.GameModes;

public class FuncBoolSymbol : CallbackSymbol
{
	public Func<bool> value;

	public FuncBoolSymbol(Key key, Func<bool> value, params Func<bool>[] conditions)
		: base(key, conditions)
	{
		this.value = value;
	}

	public FuncBoolSymbol(Key key, Func<bool> value)
		: base(key)
	{
		this.value = value;
	}
}
