using Brix.Game.Components;

public class CopyParentAffiliation : GameComponent
{
	protected override void Start()
	{
		base.Start();
		base.faction = base.transform.parent.GetComponent<AffiliationComponent>().faction;
	}
}
