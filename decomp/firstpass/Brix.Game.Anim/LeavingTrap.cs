using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class LeavingTrap : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LeavingTrap.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().LeavingTrap.ExitState(stateInfo);
	}
}
