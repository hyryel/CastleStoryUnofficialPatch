using Brix.External.Signals;
using Brix.Utils.Unity;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Components;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyTracker : UpdatableGameComponent<RigidbodyTracker>
{
	public readonly Signal RigidBodyAwokenSignal = new Signal();

	public readonly Signal RigidBodyPseudoSleepingSignal = new Signal();

	public float _sleepingVelocityThreshold = 0.5f;

	[SerializeField]
	private int _numberOfFramesBeforePseudoSleeping = 5;

	[DisplayRules(DisplayFlags.ReadOnly | DisplayFlags.HideInEditMode)]
	[JsonProperty]
	private int _pseudoSleepingFramesCounter;

	[DisplayRules(DisplayFlags.ReadOnly | DisplayFlags.HideInEditMode)]
	[SerializeField]
	private bool _rigidBodyAlmostAsleep;

	private CompCache<Rigidbody> _rigidbody;

	private Rigidbody Rigidbody => _rigidbody.Get;

	protected override void Awake()
	{
		base.Awake();
		_rigidbody = new CompCache<Rigidbody>(base.gameObject);
	}

	protected override void Start()
	{
		base.Start();
		_rigidBodyAlmostAsleep = false;
	}

	protected override void BrixFixedUpdate()
	{
		if (_rigidBodyAlmostAsleep)
		{
			if (!Rigidbody.IsSleeping() && !IsRigidBodyAlmostAsleep())
			{
				_pseudoSleepingFramesCounter = 0;
				_rigidBodyAlmostAsleep = false;
				RigidBodyAwokenSignal.Invoke();
			}
			return;
		}
		if (Rigidbody.IsSleeping())
		{
			_pseudoSleepingFramesCounter = _numberOfFramesBeforePseudoSleeping;
		}
		else if (IsRigidBodyAlmostAsleep())
		{
			_pseudoSleepingFramesCounter++;
		}
		else
		{
			_pseudoSleepingFramesCounter = 0;
		}
		_rigidBodyAlmostAsleep = _pseudoSleepingFramesCounter >= _numberOfFramesBeforePseudoSleeping;
		if (_rigidBodyAlmostAsleep)
		{
			RigidBodyPseudoSleepingSignal.Invoke();
		}
	}

	private bool IsRigidBodyAlmostAsleep()
	{
		return Rigidbody.angularVelocity.magnitude < _sleepingVelocityThreshold || Rigidbody.velocity.magnitude < _sleepingVelocityThreshold;
	}

	public void WakeUp()
	{
		_rigidBodyAlmostAsleep = false;
	}
}
