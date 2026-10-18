using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class LadderGoingUpCycle : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingUpCycle.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingUpCycle.ExitState(stateInfo);
	}
}
