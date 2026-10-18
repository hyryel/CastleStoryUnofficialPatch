using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class LadderGoingUpStart : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingUpStart.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingUpStart.ExitState(stateInfo);
	}
}
