using System.Runtime.Serialization;
using Brix.Engine;
using Brix.Game.Components.CatapultSpace;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

[JsonObject(MemberSerialization.OptIn)]
public class CatapultAmmoComponent : NetworkBehaviour
{
	[JsonProperty]
	public Catapult catapult;

	[OnDeserialized]
	private void Deserialized(StreamingContext c)
	{
		Neo.Relais(Init, 0.1f, "caterput ammo init");
	}

	private void Init()
	{
		if (catapult != null)
		{
			catapult.Armer(base.gameObject);
		}
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		base.transform.localScale = Vector3.one;
	}

	private void Update()
	{
		if (catapult == null || catapult.control.boulet == null)
		{
			Object.Destroy(this);
		}
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
