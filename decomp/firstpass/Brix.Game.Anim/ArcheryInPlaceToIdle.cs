using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class ArcheryInPlaceToIdle : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryInPlaceToIdle.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryInPlaceToIdle.ExitState(stateInfo);
	}
}
