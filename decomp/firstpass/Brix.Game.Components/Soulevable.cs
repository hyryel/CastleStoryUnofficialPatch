using UnityEngine;

namespace Brix.Game.Components;

internal class Soulevable : GameComponent, ISoulevable
{
	private bool isLifted;

	public void Soulever()
	{
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().isKinematic = true;
		}
		isLifted = true;
	}

	public void Deposer()
	{
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().isKinematic = false;
		}
		isLifted = false;
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (isLifted)
		{
			Deposer();
		}
	}
}
