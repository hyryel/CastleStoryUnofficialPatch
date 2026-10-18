using System.Runtime.Serialization;
using Brix.Engine;
using Brix.Game.Components.CatapultSpace;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Semantique;

[JsonObject(MemberSerialization.OptIn)]
public class CatapultOperatorComponent : MonoBehaviour
{
	[JsonProperty]
	public Catapult catapult;

	[OnDeserialized]
	private void Deserialized(StreamingContext c)
	{
		Neo.Relais(Init, 0.1f, "CatapultOperatorComponent init");
	}

	private void Init()
	{
		if (catapult != null)
		{
			catapult.Piloter(base.gameObject);
		}
	}

	private void Update()
	{
		if (catapult == null || catapult.control.ouvrier == null)
		{
			Object.Destroy(this);
		}
	}

	protected void OnDestroy()
	{
		if (catapult != null && catapult.control.ouvrier == base.gameObject)
		{
			catapult.CesserDePiloter();
		}
	}
}
