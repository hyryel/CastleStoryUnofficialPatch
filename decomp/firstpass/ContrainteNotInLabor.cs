using Brix.Game.AI;
using Brix.Game.Components;

public class ContrainteNotInLabor : Contrainte
{
	public override ReponseContrainte Match(GameComponent component)
	{
		return (!(component.gameObject.GetComponent<Labor>() != null)) ? ReponseContrainte.Neutre : ReponseContrainte.Echec;
	}
}
