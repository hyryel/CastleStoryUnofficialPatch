using System;
using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Game.Semantique;

public class Profile
{
	public Description description;

	public List<Contrainte> contraintes = new List<Contrainte>();

	public Profile()
	{
	}

	public Profile(params Adjectif[] adjectives)
	{
		description = new Description(adjectives);
	}

	public Profile(Adjectif adjectif)
	{
		description = new Description(adjectif);
	}

	public Profile(Type adjectif)
	{
		description = new Description(Adjectif.New(adjectif));
	}

	public Profile(Description description)
	{
		this.description = description;
	}

	public Profile(HashSet<Apparence> liste)
	{
		contraintes.Add(new ContrainteContenue(liste));
	}

	public Profile(IEnumerable<Adjectif> liste)
	{
		description = new Description(liste);
	}

	public static ProfileAccessoire Of(Accessoire tool)
	{
		return new ProfileAccessoire(tool);
	}

	public static ProfileReceveur ToFill(Recepteur recepteur)
	{
		return recepteur.ProfileReceveur;
	}

	public static ProfileReceveur ToFit(GameObject objectToFit)
	{
		return new ProfileReceveur(objectToFit);
	}

	public static ProfileReceveur ToFit(GameComponent objectToFit)
	{
		return new ProfileReceveur(objectToFit);
	}

	public virtual bool MatchApparence(Apparence apparence)
	{
		if (apparence.IsNullOrReleased())
		{
			return false;
		}
		if (description != null && !description.PeutPrendre(apparence))
		{
			return false;
		}
		bool result = true;
		foreach (Contrainte contrainte in contraintes)
		{
			switch (contrainte.Match(apparence))
			{
			case ReponseContrainte.Echec:
				return false;
			case ReponseContrainte.Positif:
				result = true;
				break;
			case ReponseContrainte.Succes:
				return true;
			}
		}
		return result;
	}

	public virtual GameComponent GetMatchedGameComponent(GameComponent component, FonctionDeContenant fonction = FonctionDeContenant.entrepot)
	{
		return component.GetMatchingDescription(fonction, (Apparence a, Profile self) => self.MatchApparence(a), this);
	}

	public virtual GameObject GetMatchedGameObject(GameObject gameObject, FonctionDeContenant fonction = FonctionDeContenant.entrepot)
	{
		GameComponent component = gameObject.GetComponent<GameComponent>();
		Apparence matchingDescription = component.GetMatchingDescription(fonction, (Apparence a, Profile self) => self.MatchApparence(a), this);
		return (!(matchingDescription != null)) ? null : matchingDescription.gameObject;
	}

	public bool TryGetMatchedGameObject(GameObject gameObject, out GameObject answer, FonctionDeContenant fonction = FonctionDeContenant.entrepot)
	{
		answer = GetMatchedGameObject(gameObject, fonction);
		return answer != null;
	}

	public virtual bool MatchComponent(GameComponent component)
	{
		return component.GetMatchingDescription((Apparence a) => MatchApparence(a));
	}

	public bool MatchGameObject(GameObject gameObject, Recepteur self = null)
	{
		bool result = false;
		if (gameObject != null)
		{
			GameComponent component = gameObject.GetComponent<GameComponent>();
			if (component != null && (component.ParentRecepteur == null || component.ParentRecepteur == self || component.ParentRecepteur.Fonction == FonctionDeContenant.entrepot))
			{
				result = MatchComponent(component);
			}
		}
		return result;
	}

	public static Profile Combiner(Profile a, Profile b)
	{
		Profile profile = new Profile();
		profile.description = a.description & b.description;
		Profile profile2 = profile;
		foreach (Contrainte contrainte in a.contraintes)
		{
			profile2.contraintes.Add(contrainte);
		}
		foreach (Contrainte contrainte2 in b.contraintes)
		{
			profile2.contraintes.Add(contrainte2);
		}
		return profile2;
	}
}
