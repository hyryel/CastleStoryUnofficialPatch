using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class Unburrow : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().Unburrow.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().Unburrow.ExitState(stateInfo);
	}
}
