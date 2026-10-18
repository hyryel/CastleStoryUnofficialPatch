using System.Collections.Generic;
using Brix.Game.Components;
using Brix.Game.Semantique;
using UnityEngine;

public class ContrainteDansQqun : Contrainte
{
	private HashSet<GameObject> candidats;

	public ContrainteDansQqun(IEnumerable<GameObject> liste)
	{
		candidats = new HashSet<GameObject>();
		foreach (GameObject item in liste)
		{
			candidats.Add(item);
		}
	}

	public override ReponseContrainte Match(GameComponent component)
	{
		Recepteur recepteur = component.recepteur;
		if (recepteur != null && candidats.Contains(recepteur.gameObject))
		{
			return ReponseContrainte.Succes;
		}
		return ReponseContrainte.Neutre;
	}
}
