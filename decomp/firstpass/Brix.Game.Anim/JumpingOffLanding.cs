using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class JumpingOffLanding : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().JumpingOffLanding.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().JumpingOffLanding.ExitState(stateInfo);
	}
}
