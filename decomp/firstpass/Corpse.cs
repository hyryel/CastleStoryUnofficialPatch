using System.Collections;
using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Audio;
using Brix.Game.Bricktron;
using Brix.Game.Components;
using Brix.Game.Rules;
using Brix.Lifecycle.Pooling;
using Brix.Transactions;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Factories;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

public class Corpse : NetworkBehaviour
{
	[JsonProperty]
	public Vector3 ImpactOrigin;

	[JsonProperty]
	public Vector3 ImpactForce;

	[JsonProperty]
	public float UpwardsEffect;

	public Rigidbody ImpactReceiver;

	public Transform[] DebrisOrigins;

	public Factory.AssetKey debrisAsset;

	public int debrisCount = 4;

	public float debrisDelay = 3f;

	[JsonProperty]
	private string originalName;

	[JsonProperty]
	private Face.Expression originalFace;

	[JsonProperty]
	private Faction originalFaction;

	[JsonProperty]
	private Occupation.Type originalOccupation;

	[JsonProperty]
	private string fireflyName;

	[JsonProperty]
	private Face.Expression fireflyFace;

	[JsonProperty]
	private Occupation.Type fireflyOccupation;

	[JsonProperty]
	private int fireflyEnergy;

	[JsonProperty]
	private Faction killersFaction;

	[JsonProperty]
	public Firefly firefly;

	public Factory.AssetKey fireflyAsset;

	[JsonProperty]
	private BricktronSoundEffects sounds;

	public bool corrompu;

	public static Corpse Spawn(Factory.AssetKey corpseAssetKey, Occupation.Type occupationBeforeDeath, GameObject original, Locomotion4.Impact impact)
	{
		ObjectPoolSingleton.Request(out var go, corpseAssetKey, original.transform.position, original.transform.rotation, active: false);
		Rigidbody[] componentsInChildren = go.GetComponentsInChildren<Rigidbody>(includeInactive: true);
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.isKinematic = true;
			rigidbody.detectCollisions = false;
		}
		TransferTransforms(original.transform, go.transform);
		Transferable.TransferOnTransform(original.transform, go.transform);
		Corpse component = go.GetComponent<Corpse>();
		component.ImpactOrigin = impact.Origin;
		component.ImpactForce = impact.Force;
		component.corrompu = original.WithComponent((Occupation o) => o.CurrentOccupation == Occupation.Type.Corruptron || o.CurrentOccupation == Occupation.Type.Biftron || o.CurrentOccupation == Occupation.Type.Minitron, _default: false);
		component.UpwardsEffect = impact.UpwardsEffect;
		component.originalName = original.GetComponent<Nom>().GetNom();
		Face component2 = original.GetComponent<Face>();
		component.originalFace = ((!component2.IsNullOrReleased()) ? component2.CurrentExpression : Face.Expression.Blank);
		component.originalFaction = original.GetComponent<AffiliationComponent>().faction;
		component.originalOccupation = occupationBeforeDeath;
		component.sounds = original.GetComponent<Occupation>().GetProfession().Sounds;
		go.SetActive(value: true);
		return component;
	}

	public static void PostSpawn(Corpse corpse)
	{
		corpse.InitFireflyVariables();
		corpse.EmitFirefly();
	}

	private void InitFireflyVariables()
	{
		killersFaction = ((!originalFaction.isAI) ? originalFaction : Faction.DefaultAllied);
		fireflyName = ((!originalFaction.isAI) ? originalName : string.Empty);
		fireflyFace = originalFace;
		fireflyOccupation = ((!originalFaction.isAI) ? originalOccupation : Occupation.Type.Unassigned);
		if (fireflyOccupation != Occupation.Type.Unassigned && !GeneralRules.Main.RespawnWithTools)
		{
			fireflyOccupation = Occupation.Type.Builder;
		}
		if (originalOccupation == Occupation.Type.Builder || originalOccupation == Occupation.Type.Knight || originalOccupation == Occupation.Type.Archer)
		{
			fireflyEnergy = GetRefundEnergy();
		}
		else if (originalOccupation == Occupation.Type.Biftron)
		{
			fireflyEnergy = GeneralRules.Main.biftronRefund;
		}
		else if (originalOccupation == Occupation.Type.Corruptron)
		{
			fireflyEnergy = GeneralRules.Main.corruptronRefund;
		}
		else if (originalOccupation == Occupation.Type.Minitron)
		{
			fireflyEnergy = GeneralRules.Main.minitronRefund;
		}
	}

	private int GetRefundEnergy()
	{
		if (GeneralRules.Main.proportionalRefund > 0f)
		{
			int activeCharacters = originalFaction.UnitCount + Firefly.SpawnableFirefliesForFaction(originalFaction);
			int homeCrystalSpawnRequiredEnergy = GeneralRules.Main.GetHomeCrystalSpawnRequiredEnergy(activeCharacters);
			float proportionalRefund = GeneralRules.Main.proportionalRefund;
			return Mathf.RoundToInt((float)homeCrystalSpawnRequiredEnergy * proportionalRefund);
		}
		return GeneralRules.Main.bricktronRefund;
	}

	private void EmitFirefly()
	{
		if (NetworkServer.active)
		{
			GameObject gameObject = Transactor.SpawnUNet(Environment.Firefly, originalFaction, DebrisOrigins[0].transform.position, Quaternion.identity, null, spawnActive: false);
			firefly = gameObject.GetComponent<Firefly>();
			firefly.SetFirefly(originalFaction, fireflyName, fireflyFace, fireflyOccupation, isOld: true);
			firefly.pureEnergy = fireflyEnergy;
		}
	}

	private static void TransferTransforms(Transform from, Transform to)
	{
		to.position = from.position;
		to.rotation = from.rotation;
		for (int i = 0; i < from.childCount; i++)
		{
			Transform child = from.GetChild(i);
			Transform transform = to.Find(child.name);
			if (transform != null)
			{
				TransferTransforms(child, transform);
			}
		}
	}

	protected void Start()
	{
		StartCoroutine(WaitAndApplyForces());
		StartCoroutine(WaitAndSpawnDebris());
	}

	private IEnumerator WaitAndApplyForces()
	{
		yield return GameClock.WaitForEndOfFrame;
		ApplyRigidbodyForces();
	}

	private IEnumerator WaitAndSpawnDebris()
	{
		yield return new WaitForSeconds(debrisDelay);
		ForceSpawns();
	}

	public void ForceSpawns()
	{
		Transferable.TransferOutOfDyingTransform(base.transform);
		SpawnDebris();
		if (NetworkServer.active)
		{
			firefly.gameObject.SetActive(value: true);
			firefly.GoWanderAround(firefly.GetNearestBestNest(killersFaction));
		}
		PlayDyingSound();
		FactoryTemplate[] componentsInChildren = GetComponentsInChildren<FactoryTemplate>();
		foreach (FactoryTemplate factoryTemplate in componentsInChildren)
		{
			if (factoryTemplate.gameObject != base.gameObject)
			{
				ObjectPoolSingleton.Release(factoryTemplate.gameObject);
			}
		}
		ObjectPoolSingleton.Release(base.gameObject);
		base.gameObject.SetActive(value: false);
	}

	private static bool IsMobileRigidbody(Rigidbody r)
	{
		Transferable component = r.GetComponent<Transferable>();
		if (component == null)
		{
			return true;
		}
		return component.dropBehaviour == Transferable.DropBehaviour.TransferToCorpseAndDrop;
	}

	private void ApplyRigidbodyForces()
	{
		List<Rigidbody> list = GetComponentsInChildren<Rigidbody>().Slinq().Where(IsMobileRigidbody).ToPooledList();
		float num = list.Slinq().Aggregate(0f, (float w, Rigidbody r) => w + r.mass);
		foreach (Rigidbody item in list)
		{
			if (!(item == null))
			{
				float num2 = item.mass / num;
				item.isKinematic = false;
				item.detectCollisions = true;
				item.AddForceAtPosition(ImpactForce * num2, ImpactOrigin, ForceMode.Impulse);
				if (UpwardsEffect > 0f)
				{
					item.AddForce(Vector3.up * ImpactForce.magnitude * UpwardsEffect * num2, ForceMode.Impulse);
				}
			}
		}
		list.Dispose();
	}

	private void SpawnDebris()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		Vector3 position = base.transform.position;
		for (int i = 0; i < debrisCount; i++)
		{
			if (DebrisOrigins.Length > 0)
			{
				position = DebrisOrigins[i % DebrisOrigins.Length].transform.position;
			}
			Factory.AssetKey assetKey = debrisAsset;
			ObjectPoolSingleton.Request(out var go, assetKey, position, Quaternion.identity);
			go.transform.LookAt(position + Random.insideUnitSphere, Vector3.up);
			go.GetComponent<Rigidbody>().AddExplosionForce(10f, position, 2f, 2f);
			go.GetComponent<Rigidbody>().AddTorque(Random.insideUnitSphere);
			Neo.PrendCetteMerde(go);
			NetworkServer.Spawn(go, assetKey.AssetId);
		}
	}

	private void PlayDyingSound()
	{
		SoundEngine.Play(sounds.Disintegrate, base.gameObject);
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
