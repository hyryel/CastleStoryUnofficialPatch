using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class CombatStep : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatStep.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatStep.ExitState(stateInfo);
	}
}
