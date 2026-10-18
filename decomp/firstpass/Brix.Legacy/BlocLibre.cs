using Brix.Audio;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Legacy;

[RequireComponent(typeof(VoxelTransform))]
[RequireComponent(typeof(Volume))]
[RequireComponent(typeof(RigidbodyTracker))]
public class BlocLibre : GameComponent, PieMenuable, ISoulevable
{
	private CompCache<RigidbodyTracker> _rigidbodyTracker;

	public override void Initialize()
	{
		base.Initialize();
		_rigidbodyTracker = new CompCache<RigidbodyTracker>(base.gameObject);
	}

	protected override void Start()
	{
		base.Start();
		_rigidbodyTracker.Get.RigidBodyAwokenSignal.Connect(OnFreeBlockMovement);
		_rigidbodyTracker.Get.RigidBodyPseudoSleepingSignal.Connect(OnFreeBlockSleep);
	}

	private void OnFreeBlockSleep()
	{
		GetComponent<VoxelTransform>().enabled = true;
		base.volume.Register();
	}

	private void OnFreeBlockMovement()
	{
		GetComponent<VoxelTransform>().enabled = false;
		base.volume.Unregister();
	}

	public void Soulever()
	{
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<VoxelTransform>().enabled = false;
		_rigidbodyTracker.Get.enabled = false;
	}

	public void Deposer()
	{
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<VoxelTransform>().enabled = true;
		_rigidbodyTracker.Get.enabled = true;
		_rigidbodyTracker.Get.WakeUp();
	}

	public void ReactiverPhysics()
	{
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().WakeUp();
	}

	private void OnCollisionEnter(Collision collision)
	{
		float magnitude = collision.relativeVelocity.magnitude;
		if (!(magnitude < 0.1f))
		{
			Description description = base.apparence.description;
			if (description.Has<StoneBlock>())
			{
				SoundEngine.SetParam("Impact_Strength", 100f * magnitude / 50f, base.gameObject);
				SoundEngine.Play("Brick_Impact", base.gameObject);
			}
			else if (description.Has<WoodBlock>() || description.Has<PlankBlock>())
			{
				SoundEngine.SetParam("Impact_Strength", 100f * magnitude / 50f, base.gameObject);
				SoundEngine.Play("Wood_Impact", base.gameObject);
			}
		}
	}

	public override bool CheckTag(string tag)
	{
		return "BlocLibre".Contains(tag);
	}
}
