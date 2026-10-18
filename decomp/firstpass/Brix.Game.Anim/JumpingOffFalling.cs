using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class JumpingOffFalling : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().JumpingOffFalling.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().JumpingOffFalling.ExitState(stateInfo);
	}
}
