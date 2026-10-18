using Brix.Game.AI;

namespace Brix.Game.Semantique;

public class ProfileAccessoire : Profile
{
	private Accessoire accessoire;

	public ProfileAccessoire(Accessoire accessoire)
	{
		this.accessoire = accessoire;
	}

	public override bool MatchApparence(Apparence apparence)
	{
		return accessoire.PeutPrendre(apparence.description) == ReponseContrainte.Positif;
	}
}
