using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class ArcheryCooldown : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryCooldown.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryCooldown.ExitState(stateInfo);
	}
}
