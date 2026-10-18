using Brix.Game.Semantique;
using Smooth.Slinq;

namespace Brix.Game.AI;

public class CategorieOutil : Adjectif
{
	private bool isBase;

	public Profile Profile;

	public CategorieOutil()
	{
		isBase = GetType() == typeof(CategorieOutil);
		Profile = new Profile(this);
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
		return description.Get(GetType()) ?? description.Adjectifs.Where((Adjectif a, CategorieOutil self) => self.IsBaseType(a), this).FirstOrDefault();
	}

	private bool IsBaseType(Adjectif adjectifDans)
	{
		return adjectifDans.GetType().BaseType == GetType();
	}

	public override AdjectiveKey GetKey()
	{
		return Adjectif.keyRessource;
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
