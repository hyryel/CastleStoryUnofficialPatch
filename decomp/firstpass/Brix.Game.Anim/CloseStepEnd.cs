using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class CloseStepEnd : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CloseStepEnd.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CloseStepEnd.ExitState(stateInfo);
	}
}
