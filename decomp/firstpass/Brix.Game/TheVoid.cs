using System;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game;

public class TheVoid : NetworkGameComponent
{
	public class Attack : IAttack
	{
		public TheVoid TheVoid;

		public float DamageValue => 0f;

		public GameObject Attacker => TheVoid.gameObject;

		public void Apply(IDamageReceiver receiver)
		{
			receiver.Kill();
		}
	}

	private Action<Labor> OnLaborCollision;

	private Action<Corpse> OnCorpseCollision;

	private Action<GameObject> OnOtherCollision;

	private Action<GameObject> OnOtherExitCollision;

	private Attack KilledByTheVoid;

	protected override void Awake()
	{
		base.Awake();
		KilledByTheVoid = new Attack
		{
			TheVoid = this
		};
	}

	public override void OnStartPureClient()
	{
		OnLaborCollision = delegate
		{
		};
		OnCorpseCollision = StopCorpse;
		OnOtherCollision = StopOther;
		OnOtherExitCollision = ExitOther;
	}

	public override void OnStartServerOrHost()
	{
		OnLaborCollision = ShredLabor;
		OnCorpseCollision = ShredCorpse;
		OnOtherCollision = ShredOther;
		OnOtherExitCollision = delegate
		{
		};
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger)
		{
			return;
		}
		Labor componentInParentOrChildren = other.GetComponentInParentOrChildren<Labor>();
		if (componentInParentOrChildren != null)
		{
			OnLaborCollision(componentInParentOrChildren);
			return;
		}
		Corpse componentInParentOrChildren2 = other.GetComponentInParentOrChildren<Corpse>();
		if (componentInParentOrChildren2 != null)
		{
			OnCorpseCollision(componentInParentOrChildren2);
		}
		else
		{
			OnOtherCollision(other.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.isTrigger)
		{
			OnOtherExitCollision(other.gameObject);
		}
	}

	private static void ExitOther(GameObject obj)
	{
		obj.SetActive(value: true);
	}

	private void ShredLabor(Labor labor)
	{
		KilledByTheVoid.Apply(labor.GetComponent<IDamageReceiver>());
	}

	private static void ShredCorpse(Corpse corpse)
	{
		corpse.ForceSpawns();
	}

	private static void StopCorpse(Corpse corpse)
	{
		corpse.enabled = false;
	}

	private static void ShredOther(GameObject gameObject)
	{
		gameObject.SafeNetworkDestroy();
	}

	private static void StopOther(GameObject go)
	{
		Rigidbody component = go.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.Sleep();
		}
		go.SetActive(value: false);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
