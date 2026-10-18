using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class Stabilizing : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().Stabilizing.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().Stabilizing.ExitState(stateInfo);
	}
}
