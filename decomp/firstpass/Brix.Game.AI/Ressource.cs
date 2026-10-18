using System;
using Brix.Game.Semantique;
using Smooth.Slinq;

namespace Brix.Game.AI;

public class Ressource : Quantite
{
	private bool isBase;

	public Profile Profile;

	public Ressource()
	{
		isBase = GetType() == typeof(Ressource);
		Profile = new Profile(this);
	}

	private Adjectif GetSelfOnly(Description description)
	{
		return description.Get(GetType());
	}

	private Adjectif GetOrParent(Description description)
	{
		return description.Get(GetType()) ?? description.Adjectifs.Where((Adjectif a, Ressource self) => self.IsBaseOf(a), this).FirstOrDefault();
	}

	private bool IsBaseOf(Adjectif adjectif)
	{
		if (adjectif != null)
		{
			for (Type type = adjectif.GetType(); type != typeof(Adjectif); type = type.BaseType)
			{
				if (type == GetType())
				{
					return true;
				}
			}
		}
		return false;
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
			return (base.quantifiable.valeur >= adjectif.quantifiable.valeur) ? ReponseContrainte.Positif : ReponseContrainte.Echec;
		}
		return (!isBase || !IsBaseOf(adjectif)) ? ReponseContrainte.Neutre : ReponseContrainte.Positif;
	}
}
