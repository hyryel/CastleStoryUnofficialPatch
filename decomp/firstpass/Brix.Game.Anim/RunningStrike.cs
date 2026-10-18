using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class RunningStrike : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().RunningStrike.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().RunningStrike.ExitState(stateInfo);
	}
}
