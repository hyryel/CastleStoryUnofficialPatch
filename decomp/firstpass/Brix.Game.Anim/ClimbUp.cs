using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class ClimbUp : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ClimbUp.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ClimbUp.ExitState(stateInfo);
	}
}
