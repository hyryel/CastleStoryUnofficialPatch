using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class CombatSwing : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatSwing.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatSwing.ExitState(stateInfo);
	}
}
