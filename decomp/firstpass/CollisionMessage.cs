using UnityEngine;

public abstract class CollisionMessage
{
	public Collision Collision;

	protected CollisionMessage(Collision c)
	{
		Collision = c;
	}
}
