using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class CombatAbortStrike : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatAbortStrike.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatAbortStrike.ExitState(stateInfo);
	}
}
