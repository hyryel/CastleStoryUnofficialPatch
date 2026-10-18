using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class LadderGoingUpEnd : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingUpEnd.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingUpEnd.ExitState(stateInfo);
	}
}
