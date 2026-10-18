using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class LadderGoingDownStart : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingDownStart.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LadderGoingDownStart.ExitState(stateInfo);
	}
}
