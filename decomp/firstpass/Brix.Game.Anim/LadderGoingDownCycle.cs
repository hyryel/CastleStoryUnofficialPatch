using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class LadderGoingDownCycle : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingDownCycle.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingDownCycle.ExitState(stateInfo);
	}
}
