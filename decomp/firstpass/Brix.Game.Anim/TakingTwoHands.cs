using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class TakingTwoHands : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().TakingTwoHands.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().TakingTwoHands.ExitState(stateInfo);
	}
}
