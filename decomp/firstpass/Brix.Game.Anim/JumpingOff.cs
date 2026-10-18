using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class JumpingOff : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().JumpingOff.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().JumpingOff.ExitState(stateInfo);
	}
}
