using Smooth.Slinq;

namespace Brix.Game.AI;

public class Accessoire : Quantite
{
	private bool isBase;

	public Accessoire()
	{
		isBase = GetType() == typeof(Accessoire);
	}

	private bool Any(Adjectif adjectif)
	{
		return adjectif != null && adjectif.GetType().BaseType == GetType();
	}

	private Adjectif GetSelfOnly(Description description)
	{
		return description.Get(GetType());
	}

	private Adjectif GetOrParent(Description description)
	{
		return description.Get(GetType()) ?? description.Adjectifs.Where((Adjectif a, Accessoire self) => self.IsBaseType(a), this).FirstOrDefault();
	}

	private bool IsBaseType(Adjectif adjectifDans)
	{
		return adjectifDans.GetType().BaseType == GetType();
	}

	public override AdjectiveKey GetKey()
	{
		return Adjectif.keyAccessoire;
	}

	public override ReponseContrainte PeutPrendre(Description description)
	{
		Adjectif adjectif = ((!isBase) ? GetSelfOnly(description) : GetOrParent(description));
		return PeutPrendre(adjectif);
	}

	public override ReponseContrainte PeutPrendre(Adjectif adjectif)
	{
		if (Match(adjectif))
		{
			return ReponseContrainte.Positif;
		}
		return (!isBase || !Any(adjectif)) ? ReponseContrainte.Neutre : ReponseContrainte.Positif;
	}
}
