using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class CombatCrank : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatCrank.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().CombatCrank.ExitState(stateInfo);
	}
}
