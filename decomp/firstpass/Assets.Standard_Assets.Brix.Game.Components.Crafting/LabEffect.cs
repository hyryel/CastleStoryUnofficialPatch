using Brix.Audio;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Standard_Assets.Brix.Game.Components.Crafting;

internal class LabEffect : NetworkBehaviour
{
	private static readonly int _Spin = Animator.StringToHash("Spin");

	private static readonly string LabStartSound = "AlchemistLab_Work_Start";

	private static readonly string LabEndSound = "AlchemistLab_Work_Stop";

	[SerializeField]
	private ParticleSystem particles;

	[SerializeField]
	private Animator animator;

	public void StartWork()
	{
		SoundEngine.Play(LabStartSound, base.gameObject);
		Spin(value: true);
		ParticleSystem.EmissionModule emission = particles.emission;
		emission.enabled = true;
		particles.gameObject.SetActive(value: true);
	}

	public void EndWork()
	{
		SoundEngine.Play(LabEndSound, base.gameObject);
		Spin(value: false);
		ParticleSystem.EmissionModule emission = particles.emission;
		emission.enabled = false;
	}

	private void Spin(bool value)
	{
		animator.SetBool(_Spin, value);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
