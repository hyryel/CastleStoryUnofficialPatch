using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Legacy;
using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

[RequireComponent(typeof(Locomotion4))]
public class UnburrowOnSpawn : NetworkGameComponent
{
	public float InvisibleUntil = 0.1f;

	public float ParticleDuration = 3f;

	public Factory.AssetKey ParticleAsset = EffetsParticules.BreakStones;

	public SkinnedMeshRenderer Skin;

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (ParticleDuration > 0f)
		{
			Particules.Emettre(ParticleAsset, base.transform.position, base.transform.rotation, ParticleDuration);
		}
	}

	protected override void Start()
	{
		base.Start();
		if (NetworkServer.active && GameSignals.Received(GameSignals.LevelReady))
		{
			Unburrow();
		}
	}

	private void LateUpdate()
	{
		if (InvisibleUntil > 0f)
		{
			Show(show: false);
			InvisibleUntil -= Time.deltaTime;
		}
		else
		{
			Show(GetComponent<LocalFactionVision>().IsVisibleForMe());
			base.enabled = false;
		}
	}

	private void Unburrow()
	{
		GetComponent<Locomotion4>().DoUnburrow();
	}

	private void Show(bool show)
	{
		if (Skin == null)
		{
			Skin = GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if (Skin.enabled != show)
		{
			Skin.enabled = show;
		}
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
