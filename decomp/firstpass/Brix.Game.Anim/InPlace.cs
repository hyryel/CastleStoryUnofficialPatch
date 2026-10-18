using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class InPlace : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().InPlace.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		GridLocomotionControllerInterface component = animator.GetComponent<GridLocomotionControllerInterface>();
		component.InPlace.ExitState(stateInfo);
	}
}
