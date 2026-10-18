using UnityEngine;

namespace Brix.NewUI.GameModes;

public class Texture2DSymbol : ContentSymbol
{
	public Texture2D value;

	public Texture2DSymbol(Key key, Texture2D texture2D)
		: base(key)
	{
		value = texture2D;
	}
}
