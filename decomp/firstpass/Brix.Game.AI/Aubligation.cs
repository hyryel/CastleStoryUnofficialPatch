namespace Brix.Game.AI;

public class Aubligation : Adjectif
{
	public override AdjectiveKey GetKey()
	{
		return Adjectif.keyAubligation;
	}

	public override ReponseContrainte PeutPrendre(Adjectif adjectif)
	{
		return Match(adjectif) ? ReponseContrainte.Neutre : ReponseContrainte.Echec;
	}
}
