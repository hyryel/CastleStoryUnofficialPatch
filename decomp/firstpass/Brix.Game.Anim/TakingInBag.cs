using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class TakingInBag : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().TakingInBag.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().TakingInBag.ExitState(stateInfo);
	}
}
