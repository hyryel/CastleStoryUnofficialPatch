using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class WorkingEntering : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().WorkingEntering.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		GridLocomotionControllerInterface component = animator.GetComponent<GridLocomotionControllerInterface>();
		if (component.WorkState.Get() == 0)
		{
			component.WorkingLeaving.ExitState(stateInfo);
		}
	}
}
