using System;

namespace Brix.NewUI.GameModes;

public class FuncStringSymbol : ContentSymbol
{
	public Func<string> value;

	public FuncStringSymbol(Key key, Func<string> funcString)
		: base(key)
	{
		value = funcString;
	}
}
