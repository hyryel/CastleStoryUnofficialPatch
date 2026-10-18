using UnityEngine;

namespace Assets.Standard_Assets.Brix.Game.Components.Crafting;

internal class MachineShopAnimations : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	private const string _Working = "Working";

	public void Working(bool value)
	{
		animator.SetBool("Working", value);
	}
}
