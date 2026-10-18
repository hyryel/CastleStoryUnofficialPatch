using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class ArcheryIdle : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryIdle.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().ArcheryIdle.ExitState(stateInfo);
	}
}
