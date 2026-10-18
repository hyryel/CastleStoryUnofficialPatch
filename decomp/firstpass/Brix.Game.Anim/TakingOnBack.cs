using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class TakingOnBack : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().TakingOnBack.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().TakingOnBack.ExitState(stateInfo);
	}
}
