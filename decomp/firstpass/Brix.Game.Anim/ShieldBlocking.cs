using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class ShieldBlocking : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ShieldBlocking.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ShieldBlocking.ExitState(stateInfo);
	}
}
