using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class ArcheryIdleToAiming : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryIdleToAiming.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryIdleToAiming.ExitState(stateInfo);
	}
}
