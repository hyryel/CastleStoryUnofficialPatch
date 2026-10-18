using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.AI;

public class Quantite : Adjectif, Quantifiable
{
	[JsonProperty]
	[SerializeField]
	private int _valeur;

	public int valeur
	{
		get
		{
			return _valeur;
		}
		set
		{
			_valeur = value;
		}
	}

	public Quantite()
	{
		_valeur = 1;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ (base.quantifiable.valeur << 10);
	}

	public override bool Equals(Adjectif adjectif)
	{
		return base.Equals(adjectif) && adjectif.quantifiable.valeur == base.quantifiable.valeur;
	}

	public override AdjectiveKey GetKey()
	{
		return Adjectif.keyQuantite;
	}

	public int CompareTo(Adjectif adjectif)
	{
		return Match(adjectif) ? adjectif.quantifiable.valeur.CompareTo(base.quantifiable) : 0;
	}

	public override Adjectif Project(Adjectif adjectif)
	{
		if (!Match(adjectif))
		{
			return null;
		}
		Adjectif adjectif2 = Clone();
		adjectif2.quantifiable.valeur = Mathf.Min(adjectif.quantifiable.valeur, base.quantifiable.valeur);
		return adjectif2;
	}

	public override Adjectif Add(Adjectif adjectif)
	{
		if (!Match(adjectif))
		{
			return Adjectif.New(GetType(), base.quantifiable.valeur);
		}
		int num = base.quantifiable.valeur + adjectif.quantifiable.valeur;
		return Adjectif.New(GetType(), num);
	}

	public override void AddTo(Adjectif adjectif)
	{
		if (adjectif is Quantite)
		{
			AddTo(adjectif as Quantite);
		}
	}

	public void AddTo(Quantite q)
	{
		if (q != null)
		{
			valeur += q._valeur;
		}
	}

	public override Adjectif Substract(Adjectif adjectif)
	{
		if (!Match(adjectif))
		{
			return Adjectif.New(GetType(), base.quantifiable.valeur);
		}
		int num = base.quantifiable.valeur - adjectif.quantifiable.valeur;
		return Adjectif.New(GetType(), (num >= 0) ? num : 0);
	}

	public override Adjectif Clone()
	{
		return Adjectif.New(GetType(), base.quantifiable.valeur);
	}

	public override void Set(Adjectif reference)
	{
		if (reference is Quantite)
		{
			_valeur = (reference as Quantite)._valeur;
		}
	}

	public override bool HasPresenceOrValue()
	{
		return valeur > 0;
	}
}
