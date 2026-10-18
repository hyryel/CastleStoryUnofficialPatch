using Brix.Game.Semantique;

namespace Brix.Game.Components;

public class SpillContentOnDeath : GameComponent
{
	public override void OnRequested()
	{
		base.OnRequested();
		base.damageReceiver.OnDeathSignal.ConnectOnce(delegate(Recepteur r)
		{
			r.DropAll();
		}, base.recepteur);
	}
}
