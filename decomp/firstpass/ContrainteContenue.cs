using System.Collections.Generic;
using Brix.Game.Components;
using Brix.Game.Semantique;

public class ContrainteContenue : Contrainte
{
	private HashSet<Apparence> liste;

	public ContrainteContenue(HashSet<Apparence> liste)
	{
		this.liste = liste;
	}

	public override ReponseContrainte Match(GameComponent component)
	{
		Apparence matchingDescription = component.GetMatchingDescription((Apparence a) => true);
		return liste.Contains(matchingDescription) ? ReponseContrainte.Neutre : ReponseContrainte.Echec;
	}
}
