using Brix.Game.AI.Damage;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class DebugAttackInput : MonoBehaviour
{
	public bool Enable;

	public DamageReceiver Target;

	public bool Attack;

	public bool JumpOff;

	public bool Block;

	public Node DebugCombat()
	{
		return (StaticNode)Node.Selector.When(() => Attack).Do(() => Attack = false).Do(delegate
		{
			GetComponent<Locomotion4>().Wield(GetComponent<Profession>().GetMeleeWeapon());
		})
			.Do(() => Node.WaitForCondition(() => GetComponent<Locomotion4>().IsReadyToStrike))
			.Do(() => GetComponent<Locomotion4>().DoStrike(Target, base.transform.position))
			.When(() => JumpOff)
			.Do(() => JumpOff = false)
			.Do(delegate
			{
				GetComponent<Locomotion4>().JumpOff();
			})
			.When(() => Block)
			.Do(delegate
			{
				GetComponent<Locomotion4>().Wield(GetComponent<Profession>().GetMeleeWeapon());
			})
			.Do(delegate
			{
				GetComponent<Locomotion4>().DoStartBlocking(float.PositiveInfinity);
			})
			.When(() => !Block)
			.Do(delegate
			{
				GetComponent<Locomotion4>().DoStopBlocking();
			})
			.Else()
			.Do(delegate
			{
			})
			.Loop();
	}
}
