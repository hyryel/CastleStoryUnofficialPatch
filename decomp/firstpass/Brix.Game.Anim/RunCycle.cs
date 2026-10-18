using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class RunCycle : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().RunCycle.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().RunCycle.ExitState(stateInfo);
	}
}
