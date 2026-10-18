using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Legacy;

namespace Brix.Game.Components;

internal class LetterDropOnDestroy : GameComponent
{
	public int Quantity = 10;

	public Factory.AssetKey Object = RawResources.RawStone;

	protected override void Awake()
	{
		base.Awake();
		ObjectDamageReceiver component = GetComponent<ObjectDamageReceiver>();
		if (!component.IsNullOrReleased())
		{
			component.OnDeathSignal.ConnectOnce(DropStuff);
		}
	}

	private void DropStuff()
	{
		Garnotte.Nouvelle(base.transform.position, Quantity, RawResources.RawStone);
	}
}
