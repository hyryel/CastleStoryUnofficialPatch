using UnityEngine;

namespace Brix.Game.Locomotion;

[SharedBetweenAnimators]
public class SoftHurt : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().SoftHurt.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		GridLocomotionControllerInterface component = animator.GetComponent<GridLocomotionControllerInterface>();
		component.SoftHurt.ExitState(stateInfo);
	}
}
