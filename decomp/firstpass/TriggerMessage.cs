using UnityEngine;

public abstract class TriggerMessage
{
	public Collider Collider;

	protected TriggerMessage(Collider c)
	{
		Collider = c;
	}
}
