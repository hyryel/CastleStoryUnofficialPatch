using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;

public class ContrainteAdjectifSpecific : Contrainte
{
	private Adjectif adjectif;

	public ContrainteAdjectifSpecific(Adjectif adjectif)
	{
		this.adjectif = adjectif;
	}

	public override ReponseContrainte Match(GameComponent component)
	{
		return component.gameObject.GetComponent<IDescriptor>().Description.Has(adjectif) ? ReponseContrainte.Neutre : ReponseContrainte.Echec;
	}
}
