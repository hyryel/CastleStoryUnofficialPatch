using System;
using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.Engine;
using Brix.Legacy;
using Brix.Legacy.Rendering;
using Brix.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.CatapultSpace;

public class CatapultBody : CatapultState
{
	public CatapultControl control;

	public GameObject modeleFrame;

	public GameObject modeleBras;

	public GameObject modeleProjectile;

	public GameObject modeleGizmos;

	public GameObject modeleBrasMax;

	public GameObject cibleFrame;

	public GameObject cibleProjectile;

	public TrajectoireComposee trajetApercu;

	public TrajectoireComposee trajetCible;

	public ArcRenderer arcLarge;

	public ArcRenderer arcCentre;

	public float angleDeTir = (float)Math.PI / 4f;

	public float rotationVitesseMax = 20f;

	public float crinqueVitesseMax = 20f;

	public float crinqueMax = 95f;

	public float forceMin = 8f;

	public float forceMax = 50f;

	[JsonProperty]
	public float rotation;

	[SyncVar(hook = "crinqueChanged")]
	[JsonProperty]
	public float crinque;

	[JsonProperty]
	[SyncVar]
	public float cooldown;

	[JsonProperty]
	[SyncVar]
	public float rotationVoulue;

	[JsonProperty]
	public float forceVoulue;

	[SyncVar]
	public float crinqueVoulue;

	public bool afficherApercu;

	public bool afficherCible;

	public bool afficherTrajets;

	public bool afficherOperateur;

	public bool afficherGUI;

	private float tempsEjection;

	public float Networkcrinque
	{
		get
		{
			return crinque;
		}
		[param: In]
		set
		{
			ref float fieldValue = ref crinque;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				crinqueChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public float Networkcooldown
	{
		get
		{
			return cooldown;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref cooldown, 2u);
		}
	}

	public float NetworkrotationVoulue
	{
		get
		{
			return rotationVoulue;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref rotationVoulue, 4u);
		}
	}

	public float NetworkcrinqueVoulue
	{
		get
		{
			return crinqueVoulue;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref crinqueVoulue, 8u);
		}
	}

	private void crinqueChanged(float value)
	{
		Networkcrinque = value;
		if (!NetworkServer.active)
		{
			AjusterCrinqueModele();
		}
	}

	public bool OnTarget()
	{
		return forceVoulue > 0f && Mathf.Abs(rotationVoulue - rotation) < 0.05f && Crinqued();
	}

	public bool Crinqued()
	{
		return Mathf.Abs(crinqueVoulue - crinque) < 0.5f;
	}

	private static float MovementAngulaire(float valeur, float objectif, float changement)
	{
		float num = Mathf.Abs(Mathf.DeltaAngle(valeur, objectif));
		valeur = ((num < changement) ? objectif : ((!(Mathf.Abs(Mathf.DeltaAngle(valeur + changement, objectif)) < num)) ? (valeur - changement) : (valeur + changement)));
		return valeur;
	}

	private static Vector3 CalculerArc(Vector3 dir, float rayon, float angle)
	{
		Vector3 vector = new Vector3(dir.x, 0f, dir.z).normalized * Mathf.Cos(angle) + new Vector3(0f, Mathf.Sin(angle), 0f);
		return vector * rayon;
	}

	private static void SetEulerY(Transform t, float eulerY)
	{
		Vector3 eulerAngles = t.eulerAngles;
		eulerAngles.y = eulerY;
		t.eulerAngles = eulerAngles;
	}

	private void InitialiserModele()
	{
		if (!GetComponent<CatapultControl>().initialisee)
		{
			rotation = modeleFrame.transform.eulerAngles.y;
			Networkcrinque = Mathf.Abs(Quaternion.Angle(modeleBras.transform.localRotation, modeleBrasMax.transform.localRotation));
			NetworkrotationVoulue = rotation;
			NetworkcrinqueVoulue = crinqueMax;
		}
		else
		{
			Neo.Relais(delegate
			{
				if (control.ouvrier != null)
				{
					HandleMessage(new MessagePiloter());
				}
				if (control.apercu != Vector3.zero)
				{
					HandleMessage(new MessageApercu());
				}
				if (control.cible != Vector3.zero)
				{
					HandleMessage(new MessageViser());
				}
			}, "caterput legacy init stuff");
		}
		base.transform.Find("Catapulte_Frame").rotation = Quaternion.AngleAxis(rotation, Vector3.up);
	}

	public void AjusterRotationModele()
	{
		SetEulerY(modeleFrame.transform, rotation);
		float num = Mathf.DeltaAngle(rotationVoulue, rotation);
		arcLarge.angleMin = Mathf.Max(90f, 90f + num);
		arcLarge.angleMax = Mathf.Min(90f, 90f + num) + 360f;
		arcCentre.angleMin = Mathf.Min(90f, 90f + num);
		arcCentre.angleMax = Mathf.Max(90f, 90f + num);
	}

	public void AjusterCrinqueModele()
	{
		modeleBras.transform.localRotation = modeleBrasMax.transform.localRotation * Quaternion.AngleAxis(crinque, -Vector3.right);
	}

	public void AjusterVoulu()
	{
		Vector3 forward = control.cible - cibleProjectile.transform.position;
		NetworkrotationVoulue = Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y;
		forceVoulue = Mathf.Clamp(ProjectilePhysics.ComputeFiringPower(angleDeTir, Mathf.Sqrt(forward.x * forward.x + forward.z * forward.z), forward.y), forceMin, forceMax);
		NetworkcrinqueVoulue = crinqueMax;
		SetEulerY(cibleFrame.transform, rotationVoulue);
	}

	public void AjusterGizmos()
	{
		trajetApercu.SetActif(afficherApercu && afficherTrajets && afficherGUI);
		trajetCible.SetActif(afficherCible && afficherTrajets && afficherGUI);
		modeleGizmos.SetActive(afficherGUI);
	}

	protected override void Entry_Turning()
	{
		SoundEngine.Play("Catapult_Move", base.gameObject);
	}

	protected override void React_Turning_DeltaTime(DeltaTime m)
	{
		float num = rotationVitesseMax;
		rotation = MovementAngulaire(rotation, rotationVoulue, num * m.DeltaTime);
		AjusterRotationModele();
	}

	protected override void Exit_Turning()
	{
		SoundEngine.Play("Catapult_Stop", base.gameObject);
	}

	protected override bool Trans_Aligned_Turning(DeltaTime m)
	{
		return control.OperatorOnSpot() && control.initialisee && rotation != rotationVoulue;
	}

	protected override bool Trans_Turning_Aligned(DeltaTime m)
	{
		return !Trans_Aligned_Turning(m);
	}

	protected override void Entry_Relaxed()
	{
		control.Networkanimee = false;
		afficherTrajets = true;
		AjusterGizmos();
	}

	protected override bool Trans_Relaxed_Cranking(DeltaTime m)
	{
		return control.OperatorOnSpot() && control.initialisee;
	}

	protected override void Entry_Cranking()
	{
		SoundEngine.Play("Catapult_Wind", base.gameObject);
	}

	protected override void React_Cranking_DeltaTime(DeltaTime m)
	{
		float num = crinqueVitesseMax;
		Networkcrinque = MovementAngulaire(crinque, crinqueVoulue, num * m.DeltaTime);
		AjusterCrinqueModele();
	}

	protected override void Exit_Cranking()
	{
		if (Crinqued())
		{
			SoundEngine.Play("Catapult_Wind_Finish", base.gameObject);
		}
		else
		{
			SoundEngine.Play("Catapult_Wind_Stop", base.gameObject);
		}
	}

	protected override bool Trans_Cranking_Relaxed(DeltaTime m)
	{
		return !control.OperatorOnSpot() || !control.initialisee;
	}

	protected override bool Trans_Cranking_Cranked(DeltaTime m)
	{
		return crinque == crinqueVoulue;
	}

	protected override bool Trans_Cranked_Cranking(MessageViser m)
	{
		return crinque != crinqueVoulue;
	}

	protected override bool Trans_Cranked_Firing(DeltaTime m)
	{
		return control.OperatorOnSpot() && control.feu && rotation == rotationVoulue;
	}

	protected override void Entry_Cranked()
	{
		control.Networkcrinquee = true;
	}

	protected override void Exit_Cranked()
	{
		control.Networkcrinquee = false;
	}

	protected override void Entry_Firing()
	{
		RpcEntry_Firing();
	}

	protected void RpcEntry_Firing()
	{
		tempsEjection = Time.time + 1f / 3f;
		control.Networkfeu = false;
		control.Networkanimee = true;
		afficherTrajets = false;
		AjusterGizmos();
		modeleBras.GetComponent<Animation>().Play("Tirer");
		SoundEngine.Play("Catapult_Fire", base.gameObject);
	}

	protected override void Exit_Firing()
	{
		RpcExit_Firing();
	}

	[ServerCallback]
	protected void RpcExit_Firing()
	{
		if (NetworkServer.active)
		{
			Networkcooldown = Time.time + 2f;
			Networkcrinque = 0f;
			if (control.boulet != null)
			{
				Vector3 initialVelocity = CalculerArc(modeleFrame.transform.forward, forceVoulue, angleDeTir);
				Physics.IgnoreCollision(modeleBras.transform.GetComponent<Collider>(), control.boulet.GetComponent<Collider>());
				control.boulet.transform.parent = null;
				CatapultProjectile catapultProjectile = control.boulet.AddComponent<CatapultProjectile>();
				catapultProjectile.sourceCatapult = GetComponent<Catapult>();
				catapultProjectile.Thrust(initialVelocity, control.inaccuracy);
				control.Networkboulet = null;
				control.Networkfeu = false;
			}
		}
	}

	protected override bool Trans_Firing_Cooldown(DeltaTime m)
	{
		return Time.time >= tempsEjection;
	}

	protected override bool Trans_Cooldown_Relaxed(DeltaTime m)
	{
		return Time.time >= cooldown;
	}

	protected override bool Trans_Initializing_Healthy(DeltaTime m)
	{
		afficherApercu = false;
		afficherCible = false;
		InitialiserModele();
		AjusterGizmos();
		control.initialisee = true;
		return true;
	}

	protected override void React_Healthy_Armer(MessageArmer m)
	{
		RpcReact_Healthy_Armer(m);
	}

	private void RpcReact_Healthy_Armer(MessageArmer m)
	{
		GameObject boulet = control.boulet;
		if (NetworkServer.active)
		{
			boulet.transform.parent = modeleProjectile.transform;
			boulet.transform.position = modeleProjectile.transform.position;
			boulet.transform.rotation = modeleProjectile.transform.rotation;
		}
		boulet.SetActive(value: true);
		if (boulet.GetComponent<CatapultAmmoComponent>() == null)
		{
			boulet.AddComponent<CatapultAmmoComponent>().catapult = GetComponent<Catapult>();
		}
		if (boulet.GetComponent<Rigidbody>() != null && !boulet.GetComponent<Rigidbody>().isKinematic)
		{
			boulet.GetComponent<Rigidbody>().velocity = Vector3.zero;
			boulet.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			boulet.GetComponent<Rigidbody>().isKinematic = true;
			UnityEngine.Object.Destroy(boulet.GetComponent<Rigidbody>());
		}
		string data = string.Empty;
		if (boulet.GetComponent<Roche>() != null)
		{
			data = "Stone";
		}
		else if ((bool)boulet.GetComponent<BarilScript>())
		{
			data = "Barrel";
		}
		SoundEngine.SetSwitch("Missile", data, base.gameObject);
		SoundEngine.Play("Catapult_Load", base.gameObject);
	}

	protected override void React_Healthy_Desarmer(MessageDesarmer m)
	{
		if (control.boulet.GetComponent<CatapultAmmoComponent>() != null)
		{
			UnityEngine.Object.DestroyObject(control.boulet.GetComponent<CatapultAmmoComponent>());
		}
	}

	protected override void React_Healthy_Piloter(MessagePiloter m)
	{
	}

	protected override void React_Healthy_CesserDePiloter(MessageCesserDePiloter m)
	{
	}

	protected override void React_Healthy_Apercu(MessageApercu m)
	{
		trajetApercu.SetForceMinMax(forceMin, forceMax);
		trajetApercu.SetCible(control.apercu, control.apercuNormale);
		afficherApercu = true;
		AjusterGizmos();
	}

	protected override void React_Healthy_CesserApercu(MessageCesserApercu m)
	{
		afficherApercu = false;
		AjusterGizmos();
	}

	protected override void React_Healthy_Viser(MessageViser m)
	{
		afficherApercu = false;
		trajetCible.SetForceMinMax(forceMin, forceMax);
		if (trajetCible.TrySetCible(control.cible, control.cibleNormale))
		{
			afficherCible = true;
			AjusterVoulu();
			SoundEngine.Play("Catapult_Set_Target", base.gameObject);
		}
		AjusterGizmos();
	}

	protected override void React_Healthy_Declancher(MessageDeclancher m)
	{
		GetComponent<CatapultOperations>().OnShoot();
	}

	protected override void React_Healthy_DeltaTime(DeltaTime m)
	{
	}

	protected override bool Trans_Deselected_Selected(MessageSelectionner m)
	{
		return true;
	}

	protected override bool Trans_Selected_Deselected(MessageDeselectionner m)
	{
		return true;
	}

	public void Selected()
	{
		afficherGUI = true;
		AjusterGizmos();
	}

	public void Unselected()
	{
		afficherGUI = false;
		AjusterGizmos();
	}

	protected override void React_Deselected_DeltaTime(DeltaTime m)
	{
	}

	protected override void React_Selected_DeltaTime(DeltaTime m)
	{
	}

	protected override void Entry_Driving()
	{
		afficherOperateur = true;
		AjusterGizmos();
	}

	protected override void Entry_Leaving()
	{
	}

	protected override bool Trans_Leaving_Vacant(DeltaTime m)
	{
		return true;
	}

	protected override bool Trans_Leaving_Driving(MessagePiloter m)
	{
		return true;
	}

	protected override void Entry_Vacant()
	{
		afficherOperateur = false;
		AjusterGizmos();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(crinque);
			writer.Write(cooldown);
			writer.Write(rotationVoulue);
			writer.Write(crinqueVoulue);
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
			writer.Write(crinque);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(cooldown);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(rotationVoulue);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(crinqueVoulue);
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
			crinque = reader.ReadSingle();
			cooldown = reader.ReadSingle();
			rotationVoulue = reader.ReadSingle();
			crinqueVoulue = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			crinqueChanged(reader.ReadSingle());
		}
		if ((num & 2) != 0)
		{
			cooldown = reader.ReadSingle();
		}
		if ((num & 4) != 0)
		{
			rotationVoulue = reader.ReadSingle();
		}
		if ((num & 8) != 0)
		{
			crinqueVoulue = reader.ReadSingle();
		}
	}
}
