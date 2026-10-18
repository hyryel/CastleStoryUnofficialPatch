using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class WalkCycle : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().WalkCycle.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().WalkCycle.ExitState(stateInfo);
	}
}
