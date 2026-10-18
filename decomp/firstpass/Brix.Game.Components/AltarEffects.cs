using UnityEngine;

namespace Brix.Game.Components;

public class AltarEffects : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Spin spin;

	private const string _Light = "Light";

	public void StarWork()
	{
		Work(value: true);
	}

	public void EndWork()
	{
		Work(value: false);
	}

	private void Light(bool value)
	{
		animator.SetBool("Light", value);
	}

	private void Spin(bool value)
	{
		spin.enabled = value;
	}

	private void Work(bool value)
	{
		Spin(value);
		Light(value);
	}
}
