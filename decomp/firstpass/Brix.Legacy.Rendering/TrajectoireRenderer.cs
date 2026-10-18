using Brix.Utils;
using UnityEngine;

namespace Brix.Legacy.Rendering;

[ExecuteInEditMode]
public class TrajectoireRenderer : MonoBehaviour
{
	public bool autoUpdate;

	public bool afficherDebut;

	public bool afficherCible;

	public bool afficherLigne;

	public GameObject modeleDebut;

	public GameObject modeleCible;

	public LineRenderer ligne;

	public bool angleConstant;

	public bool forceConstante;

	public bool vitesseRepereConstante;

	public Vector3 vitesseDebut;

	public Vector3 normaleCible;

	[HideInInspector]
	public float angle;

	[HideInInspector]
	public float force;

	public float forceMin;

	public float forceMax = float.PositiveInfinity;

	private Vector3 vitesseReelle;

	private bool exAfficherDebut;

	private bool exAfficherCible;

	private bool exAfficherLigne;

	private Vector3 exPositionDebut;

	private Vector3 exPositionCible;

	private Vector3 exVitesseDebut;

	private Vector3 exNormaleCible;

	public void SetDebut(Vector3 debut, Vector3 vitesse)
	{
		modeleDebut.transform.position = debut;
		vitesseDebut = vitesse;
	}

	public void SetCible(Vector3 cible, Vector3 normale)
	{
		modeleCible.transform.position = cible;
		normaleCible = normale;
	}

	public void SetVisible(bool debut, bool cible)
	{
		afficherDebut = debut;
		afficherCible = cible;
	}

	public void SetLigneVisible(bool ligne)
	{
		afficherLigne = ligne;
	}

	public void SetVitesseAvecAngle(float angle)
	{
		Vector3 relativeTarget = modeleCible.transform.position - modeleDebut.transform.position;
		Vector3 normalized = new Vector3(relativeTarget.x, 0f, relativeTarget.z).normalized;
		Vector3 vector = new Vector3(normalized.x * Mathf.Cos(angle), Mathf.Sin(angle), normalized.z * Mathf.Cos(angle));
		force = ProjectilePhysics.ComputeFiringPower(angle, relativeTarget);
		force = Mathf.Clamp(force, forceMin, forceMax);
		vitesseReelle = vector * force;
	}

	public void SetVitesseAvecForce(float force, bool balloune)
	{
		Vector3 relativeTarget = modeleCible.transform.position - modeleDebut.transform.position;
		angle = ProjectilePhysics.ComputeElevationAngle(force, relativeTarget, balloune);
		Vector3 normalized = new Vector3(relativeTarget.x, 0f, relativeTarget.z).normalized;
		Vector3 vector = new Vector3(normalized.x * Mathf.Cos(angle), Mathf.Sin(angle), normalized.z * Mathf.Cos(angle));
		vitesseReelle = vector * force;
	}

	public void SetVitesseAvecDuree(float duree)
	{
	}

	public Vector3 GetPositionDebut()
	{
		return modeleDebut.transform.position;
	}

	public Vector3 GetVitesseDebut()
	{
		CalculerVitesseReelle();
		return vitesseReelle;
	}

	public Vector3 GetPositionCible()
	{
		return modeleCible.transform.position;
	}

	public Vector3 GetNormaleCible()
	{
		return normaleCible;
	}

	public void UpdateImmediate()
	{
		Update();
	}

	private void Update()
	{
		if (afficherDebut == exAfficherDebut && afficherCible == exAfficherCible && afficherLigne == exAfficherLigne && modeleDebut.transform.position == exPositionDebut && modeleCible.transform.position == exPositionCible && vitesseDebut == exVitesseDebut && normaleCible == exNormaleCible)
		{
			return;
		}
		exAfficherDebut = afficherDebut;
		exAfficherCible = afficherCible;
		exAfficherLigne = afficherLigne;
		exPositionDebut = modeleDebut.transform.position;
		exPositionCible = modeleCible.transform.position;
		exVitesseDebut = vitesseDebut;
		exNormaleCible = normaleCible;
		if (modeleDebut != null)
		{
			modeleDebut.SetActive(afficherDebut);
		}
		if (modeleCible != null)
		{
			modeleCible.SetActive(afficherCible);
		}
		if (ligne != null)
		{
			ligne.enabled = afficherLigne;
		}
		if (autoUpdate)
		{
			CalculerVitesseReelle();
			Vector3 vector = vitesseDebut;
			vitesseDebut = vitesseReelle;
			if (afficherDebut || afficherCible)
			{
				Rebuild();
			}
			if (vitesseRepereConstante)
			{
				vitesseDebut = vector;
			}
		}
	}

	public void CalculerVitesseReelle()
	{
		Vector3 vector = vitesseDebut;
		vitesseReelle = vitesseDebut;
		if (angleConstant)
		{
			SetVitesseAvecAngle(Mathf.Atan2(vector.y, Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z)));
		}
		if (forceConstante)
		{
			SetVitesseAvecForce(vector.magnitude, balloune: false);
		}
	}

	public void Rebuild()
	{
		if (vitesseDebut != Vector3.zero)
		{
			modeleDebut.transform.rotation = Quaternion.LookRotation(vitesseDebut.normalized, Vector3.up);
		}
		if (normaleCible != Vector3.zero)
		{
			modeleCible.transform.rotation = Quaternion.LookRotation(normaleCible, Vector3.up);
		}
		if (ligne != null)
		{
			Vector3[] array = ProjectilePhysics.ComputeTrajectory(modeleDebut.transform.position, vitesseDebut, modeleCible.transform.position, 20);
			ligne.positionCount = array.Length;
			for (int i = 0; i < array.Length; i++)
			{
				ligne.SetPosition(i, array[i]);
			}
		}
	}
}
