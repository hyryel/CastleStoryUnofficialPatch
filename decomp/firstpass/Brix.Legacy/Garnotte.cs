using System;
using Brix.Audio;
using Brix.Components;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Transactions;
using Brix.Utils;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Legacy;

[JsonObject(MemberSerialization.OptIn)]
public class Garnotte : BrixComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(Garnotte).FullName);

	public static Signal<Garnotte> Enabled = new Signal<Garnotte>();

	public static Signal<Garnotte> Disabled = new Signal<Garnotte>();

	public VoxelTerrainType terrainType = VoxelTerrainType.Sand;

	public Factory.AssetKey particlesAsset;

	public bool CanMergeback = true;

	public bool Sinking => GetComponent<SinkingObject>() != null;

	private Rigidbody Rigidbody
	{
		get
		{
			Rigidbody rigidbody = GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				logger.WarnFormat("Garnotte {0} has no Rigidbody, but should always have one. Adding one.", this);
				rigidbody = base.gameObject.AddComponent<Rigidbody>();
			}
			return rigidbody;
		}
	}

	private Collider Collider => GetComponent<Collider>();

	public static void Nouvelle(Vector3 position, int qte, Factory.AssetKey assetKey, bool naturelle = true)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (qte == 0)
		{
			throw new ArgumentOutOfRangeException("qte", "Garnotte.Nouvelle must spawn at least 1 garnotte");
		}
		Faction defaultNeutral = Faction.DefaultNeutral;
		if (qte == 1 && Voxel.IsEmpty(XYZ.FromVector3(position)))
		{
			Spawn(position, UnityEngine.Random.rotation, assetKey, defaultNeutral);
			return;
		}
		SphereCollection sphereCollection = new SphereCollection();
		for (int i = 0; i < qte; i++)
		{
			sphereCollection.Add(position, 0.25f);
		}
		sphereCollection.Shuffle(position, avoidCollisions: true, (Vector3 p) => Voxel.IsEmpty(XYZ.FromVector3(p)));
		for (int num = 0; num < qte; num++)
		{
			Spawn(sphereCollection.Spheres[num].center, UnityEngine.Random.rotation, assetKey, defaultNeutral);
		}
	}

	private static void Spawn(Vector3 position, Quaternion rotation, Factory.AssetKey assetKey, Faction faction)
	{
		Transactor.SpawnUNet(assetKey, faction, position, rotation);
	}

	private void OnEnable()
	{
		Enabled.Invoke(this);
	}

	private void OnDisable()
	{
		Disabled.Invoke(this);
	}

	public bool SinkIfPossible()
	{
		if (!CanMergeback)
		{
			return false;
		}
		if (!Sinking)
		{
			DisablePhysics();
			base.gameObject.AddComponent<SinkingObject>();
		}
		return true;
	}

	public void DisablePhysics()
	{
		Rigidbody.isKinematic = true;
		Collider.enabled = false;
	}

	private void ActivatePhysics()
	{
		Collider.enabled = true;
		Rigidbody.isKinematic = false;
		Rigidbody.velocity = Vector3.zero;
		Rigidbody.WakeUp();
	}

	private void OnCollisionEnter(Collision collision)
	{
		float magnitude = collision.relativeVelocity.magnitude;
		if (!(magnitude < 0.5f))
		{
			SoundEngine.SetParam("Impact_Strength", 100f * Mathf.Clamp01((magnitude - 0.5f) / 49.5f), base.gameObject);
			SoundEngine.Play("Garnotte_Impact", base.gameObject);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		Rigidbody.sleepThreshold = 0.1f;
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		ActivatePhysics();
	}
}
