using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class TurnOnSpot : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().TurnOnSpot.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().TurnOnSpot.ExitState(stateInfo);
	}
}
