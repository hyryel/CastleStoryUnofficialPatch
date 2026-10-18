using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class ArcheryAiming : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryAiming.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryAiming.ExitState(stateInfo);
	}
}
