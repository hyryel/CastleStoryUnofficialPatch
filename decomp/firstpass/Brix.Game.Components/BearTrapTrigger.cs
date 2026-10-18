using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Network;
using Brix.Utils;
using Motus.Behavior;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class BearTrapTrigger : NetworkGameComponent, ISoulevable
{
	private enum TrapState
	{
		shut,
		open,
		halfshut
	}

	[SerializeField]
	private Animator _anim;

	private CompCache<ObjectActivator> _activator;

	private bool _spawnGarnottesOnDeath = true;

	private Labor _trappedLabor;

	private Collider _trappedLaborCollider;

	private bool _isTrapped;

	[SyncVar(hook = "ShutStateChanged")]
	private TrapState isShut;

	public TrapState NetworkisShut
	{
		get
		{
			return isShut;
		}
		[param: In]
		set
		{
			ref TrapState fieldValue = ref isShut;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				ShutStateChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_activator = new CompCache<ObjectActivator>(base.gameObject);
	}

	protected override void Start()
	{
		base.Start();
		_activator.Get.ActivationSignal.Connect(delegate
		{
			NetworkisShut = TrapState.open;
		});
		_activator.Get.DeactivationSignal.Connect(delegate
		{
			NetworkisShut = TrapState.shut;
		});
		_activator.Get.Active = true;
	}

	private void ShutStateChanged(TrapState value)
	{
		NetworkisShut = value;
		switch (isShut)
		{
		case TrapState.open:
			_anim.SetBool("Half Shut", value: false);
			_anim.SetBool("Shut", value: false);
			SoundEngine.Play("Bear_Trap_Rearm", base.gameObject);
			break;
		case TrapState.halfshut:
			_anim.SetBool("Half Shut", value: true);
			_anim.SetBool("Shut", value: true);
			SoundEngine.Play("Bear_Trap_Close", base.gameObject);
			SoundEngine.Play("Bear_Trap_Catch_Enemy", base.gameObject);
			break;
		case TrapState.shut:
			_anim.SetBool("Half Shut", value: false);
			_anim.SetBool("Shut", value: true);
			SoundEngine.Play("Bear_Trap_Close", base.gameObject);
			break;
		}
	}

	private void FixedUpdate()
	{
		if (_trappedLabor == null)
		{
			base.enabled = false;
		}
		else if (_trappedLabor.volume.Position == base.volume.Position)
		{
			_activator.Get.Active = false;
			ApplyTrap();
			base.enabled = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!NetworkServer.active || !_activator.Get.Active || other.isTrigger)
		{
			return;
		}
		GameComponent gameComponent = other.GetComponent<GameComponent>() ?? other.GetComponentInParent<GameComponent>();
		if (gameComponent != null && Affiliation.IsEnemy(base.gameObject, gameComponent.gameObject) && other.GetComponent<RaidProject>() == null)
		{
			Labor componentInParent = other.GetComponentInParent<Labor>();
			if (componentInParent != null && faction != componentInParent.faction)
			{
				_trappedLabor = componentInParent;
				_trappedLaborCollider = other;
				base.enabled = true;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (_trappedLabor != null && other == _trappedLaborCollider)
		{
			if (_isTrapped)
			{
				NetworkisShut = TrapState.shut;
			}
			_isTrapped = false;
			_trappedLabor = null;
			_trappedLaborCollider = null;
		}
	}

	private void ApplyTrap()
	{
		BricktronDamageReceiver component = _trappedLabor.GetComponent<BricktronDamageReceiver>();
		component.ReceiveAttack(150f, DamageType.Melee, DamageReaction.Ignored, Vector3.zero, Vector3.zero, 0f);
		Occupation.Type currentOccupation = _trappedLabor.Occupation.CurrentOccupation;
		if (currentOccupation == Occupation.Type.Biftron || currentOccupation == Occupation.Type.Magitron)
		{
			Profession component2 = _trappedLabor.GetComponent<Profession>();
			if (component2 as Biftron != null && (component2 as Biftron).EquipOneMoreTrap())
			{
				_spawnGarnottesOnDeath = false;
			}
			if (component2 as Magitron != null && (component2 as Magitron).EquipOneMoreTrap())
			{
				_spawnGarnottesOnDeath = false;
			}
			Particules.Emettre(EffetsParticules.BreakStones, base.transform.position, Quaternion.identity, 3f);
			ObjectPoolSingleton.Release(base.gameObject);
		}
		else if (component.CurrentHP > 0f)
		{
			_isTrapped = true;
			NetworkisShut = TrapState.halfshut;
			base.transform.rotation = _trappedLabor.transform.rotation;
			_trappedLabor.navigation.Fail();
			Node workNode = (StaticNode)Node.Sequence.Do(delegate(Labor _labor, Vector3 _position)
			{
				_labor.navigation.Freeze(_position);
			}, _trappedLabor, base.transform.position).Do(Node.WaitFor.Milliseconds(500)).Do(delegate(Labor _labor, Vector3 _position)
			{
				_labor.navigation.GetTrapped(_position, 2.5f);
			}, _trappedLabor, base.transform.position)
				.Do(Node.WaitFor.Milliseconds(2500));
			Task task = new Task();
			task.Worker = _trappedLabor;
			task.Description = "BearTrap freeze";
			task.WorkNode = workNode;
			task.CanInterrupt = false;
			task.EndAction = delegate
			{
				base.signal.WorkEnded.Invoke();
			};
			Task task2 = task;
			_trappedLabor.SendOrder(task2, Labor.DecisionSource.Unspecified);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_spawnGarnottesOnDeath)
		{
			Garnotte.Nouvelle(base.transform.position + Vector3.up, 5, RawResources.RawIron);
		}
		Particules.Emettre(EffetsParticules.BreakStones, base.transform.position, Quaternion.identity, 3f);
	}

	public void Soulever()
	{
	}

	public void Deposer()
	{
		base.voxel.position = XYZ.FromVector3(base.transform.position);
		if (!Voxel.IsOpenAndWalkable(base.voxel.position))
		{
			if (Voxel.IsOpenAndWalkable(base.voxel.position - XYZ.up))
			{
				base.voxel.position -= XYZ.up;
			}
			else if (Voxel.IsOpenAndWalkable(base.voxel.position - XYZ.up * 2))
			{
				base.voxel.position -= XYZ.up * 2;
			}
		}
		base.transform.position = Architecte.SurfaceRaycast(base.voxel.position.ToVector3());
		ClasseMoi.Move(base.gameObject);
		SurfaceSpawnFitter component = GetComponent<SurfaceSpawnFitter>();
		if ((bool)component)
		{
			component.Fit();
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write((int)isShut);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write((int)isShut);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			isShut = (TrapState)reader.ReadInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			ShutStateChanged((TrapState)reader.ReadInt32());
		}
	}
}
