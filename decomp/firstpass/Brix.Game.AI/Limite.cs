namespace Brix.Game.AI;

public class Limite : Quantite
{
	public override AdjectiveKey GetKey()
	{
		return Adjectif.keyLimite;
	}

	public override ReponseContrainte PeutPrendre(Adjectif adjectif)
	{
		return (!Match(adjectif) || base.quantifiable.valeur >= adjectif.quantifiable.valeur) ? ReponseContrainte.Positif : ReponseContrainte.Echec;
	}
}
