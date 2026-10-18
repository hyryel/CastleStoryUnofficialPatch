using UnityEngine;

namespace Brix.Game.Anim;

[SharedBetweenAnimators]
public class HitBrick : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().HitBrick.EnterState(stateInfo);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<GridLocomotionControllerInterface>().HitBrick.ExitState(stateInfo);
	}
}
