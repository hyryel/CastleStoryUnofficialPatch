using Brix.Game.Components;

namespace Brix.Game.AI.Damage;

public static class DamageReceiverExtender
{
	public static bool IsNullOrReleased(this IDamageReceiver dr)
	{
		return dr?.GameObject.IsNullOrReleased() ?? true;
	}
}
