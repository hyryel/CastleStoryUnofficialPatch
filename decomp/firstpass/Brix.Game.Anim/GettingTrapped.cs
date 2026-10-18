using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class GettingTrapped : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().GettingTrapped.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().GettingTrapped.ExitState(stateInfo);
	}
}
