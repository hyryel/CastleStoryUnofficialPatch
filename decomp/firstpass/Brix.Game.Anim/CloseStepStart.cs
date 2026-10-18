using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class CloseStepStart : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CloseStepStart.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CloseStepStart.ExitState(stateInfo);
	}
}
