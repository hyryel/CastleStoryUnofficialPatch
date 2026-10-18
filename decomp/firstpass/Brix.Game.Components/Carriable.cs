using UnityEngine;

namespace Brix.Game.Components;

internal class Carriable : GameComponent, ISoulevable
{
	public void Soulever()
	{
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<VoxelTransform>().enabled = false;
	}

	public void Deposer()
	{
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<VoxelTransform>().enabled = true;
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		Deposer();
	}
}
