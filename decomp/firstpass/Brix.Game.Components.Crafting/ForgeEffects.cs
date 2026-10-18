using UnityEngine;

namespace Brix.Game.Components.Crafting;

internal class ForgeEffects : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	private const string _Fire = "Fire";

	public void Fire(bool value)
	{
		animator.SetBool("Fire", value);
	}
}
