using Brix.Game.AI;
using Brix.Game.Components;

public class ContrainteAnyBlueprint : Contrainte
{
	public override ReponseContrainte Match(GameComponent component)
	{
		return (!(component.gameObject.GetComponent<BuildGoal>() != null)) ? ReponseContrainte.Neutre : ReponseContrainte.Succes;
	}
}
