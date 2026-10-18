using UnityEngine;

namespace Assets.Standard_Assets.Brix.Game.Components.Crafting;

internal class LoomAnimatorInterface : MonoBehaviour
{
	public enum DisplayState
	{
		None,
		Blueprint,
		Item
	}

	[SerializeField]
	private Animator animator;

	private const string ActiveRope = "ActiveRope";

	private const string ActiveFabric = "ActiveFabric";

	private const string Rope = "Rope";

	private const string Fabric = "Fabric";

	private const string Fill = "Fill";

	public void SpinRope(bool value)
	{
		animator.SetBool("ActiveRope", value);
	}

	public void SpinFabric(bool value)
	{
		animator.SetBool("ActiveFabric", value);
	}

	public void ShowRope(DisplayState state)
	{
		animator.SetInteger("Rope", (int)state);
	}

	public void ShowFabric(DisplayState state)
	{
		animator.SetInteger("Fabric", (int)state);
	}

	public void FillAt(float value)
	{
		animator.SetFloat("Fill", value);
	}
}
