using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class CombatMissed : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatMissed.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatMissed.ExitState(stateInfo);
	}
}
