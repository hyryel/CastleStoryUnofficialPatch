using Brix.Game.AI.Damage;

namespace Brix.Engine.Nature;

public class NullNatureData : NatureData
{
	public override NatureType Type => NatureType.none;

	public override IDamageReceiver DamageReceiver => NullDamageReceiver.Get;
}
