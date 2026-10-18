using System;
using UnityEngine;

namespace Brix.NewUI.GameModes;

public class FuncTexture2DSymbol : ContentSymbol
{
	public Func<Texture2D> value;

	public FuncTexture2DSymbol(Key key, Func<Texture2D> value)
		: base(key)
	{
		this.value = value;
	}
}
