using System.Collections.Generic;
using Brix.Game.Components;
using Brix.Utils;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Semantique;

public class ProfileReceveur : Profile
{
	private static BrixLogger logger = new BrixLogger(typeof(ProfileReceveur).FullName);

	private List<GameComponent> objets = CollectionPool.RequestList<GameComponent>();

	public ProfileReceveur(GameComponent objet)
	{
		objets.Add(objet);
	}

	public ProfileReceveur(GameObject objet)
	{
		GameComponent component = objet.GetComponent<GameComponent>();
		if (component != null)
		{
			objets.Add(component);
		}
	}

	public ProfileReceveur(IEnumerable<GameComponent> objets)
	{
		objets.Slinq().AddTo(this.objets);
	}

	public ProfileReceveur(List<GameObject> objets)
	{
		(from go in objets.Slinq()
			select go.GetComponent<GameComponent>()).NotNull().AddTo(this.objets);
	}

	public override bool MatchApparence(Apparence apparence)
	{
		return MatchComponent(apparence);
	}

	public override bool MatchComponent(GameComponent component)
	{
		if (objets == null || objets.Count == 0)
		{
			logger.Warn("Object from ProfilReceveur has been destroyed but you're still trying to match it. Plese find out who asked that question and in why. Then report to Francois.");
			return false;
		}
		if (component == null || component.recepteur == null || component.recepteur.TakeOnly)
		{
			return false;
		}
		bool result = Fit(component);
		foreach (Contrainte contrainte in contraintes)
		{
			switch (contrainte.Match(component))
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

	private bool Fit(GameComponent component)
	{
		return (from o in objets.Slinq()
			where o != null
			select o).Select((GameComponent o, Profile p) => p.GetMatchedGameComponent(o), component.recepteur.Profile()).Any((GameComponent a) => a != null);
	}

	~ProfileReceveur()
	{
		objets.Dispose();
	}
}
