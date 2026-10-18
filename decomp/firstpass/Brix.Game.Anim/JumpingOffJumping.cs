using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class JumpingOffJumping : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().JumpingOffJumping.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().JumpingOffJumping.ExitState(stateInfo);
	}
}
