using Brix.Game.AI;
using Motus.Behavior;
using UnityEngine;

public class DebugArcheryInput : MonoBehaviour
{
	public bool Enable;

	public GameObject Target;

	public bool Shoot;

	public bool ResetStance;

	public GameObject GrabTarget;

	public Node DebugArchery()
	{
		Profession archery = GetComponent<Profession>();
		Locomotion4 loco = GetComponent<Locomotion4>();
		return (StaticNode)Node.Sequence.Do(delegate
		{
			GetComponent<Occupation>().SetOccupation(Occupation.Type.Archer);
		}).Do(() => Node.Selector.When(() => Shoot).Do(() => archery.ShootAt(new ObjectLocation(Target))).Do(() => Shoot = false)
			.When(() => GrabTarget)
			.Do(delegate
			{
				loco.DoGrab(new ObjectLocation(GrabTarget));
			})
			.Do(() => GrabTarget = null)
			.Else()
			.Do(delegate
			{
			})
			.Loop());
	}
}
