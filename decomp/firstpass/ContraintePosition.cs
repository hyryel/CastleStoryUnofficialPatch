using Brix.Game.Components;
using UnityEngine;

public class ContraintePosition : Contrainte
{
	private Vector3 position;

	private float rayon;

	public ContraintePosition(Vector3 p, float r)
	{
		position = p;
		rayon = r;
	}

	public override ReponseContrainte Match(GameComponent component)
	{
		return (Vector3.Distance(component.transform.position, position) <= rayon) ? ReponseContrainte.Neutre : ReponseContrainte.Echec;
	}
}
