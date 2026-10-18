using Brix.Utils;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Legacy.Rendering;

public class TrajectoireComposee : MonoBehaviour
{
	public bool autoInit;

	public bool actif;

	public bool update;

	public TrajectoireRenderer trajetDebut;

	public TrajectoireRenderer trajetFin;

	public Vector3 positionDepart;

	public Vector3 vitesseDepart;

	public Vector3 positionCible;

	public Vector3 normaleCible;

	public bool split;

	public Vector3 positionHit;

	public Vector3 vitesseHit;

	public Vector3 normaleHit;

	public float dureeHit;

	public float dureePrevue;

	public float forcePrevue;

	public bool autoSplit;

	public float delaiCheck = 0.5f;

	public float distanceMin = 1f;

	public float distanceMax = 1000f;

	public float distanceSplitMin = 1f;

	public float dureeMax = 20f;

	public int layerMask;

	public float forceMin;

	public float forceMax = float.PositiveInfinity;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public TrajectoireRenderer trajetCible;

	private Vector3 exPositionDepart;

	private Vector3 exVitesseDepart;

	private Vector3 exPositionCible;

	private Vector3 exNormaleCible;

	private float prochainCalcul;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool visible;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool intercepted;

	private void Start()
	{
		trajetCible = trajetDebut;
		if (autoInit)
		{
			CalculerParametres();
		}
		if (layerMask == 0)
		{
			layerMask = 607232;
		}
	}

	public void SetForceMinMax(float forceMin, float forceMax)
	{
		this.forceMin = forceMin;
		this.forceMax = forceMax;
	}

	public void SetDebut(Vector3 debut, Vector3 vitesse)
	{
		trajetDebut.SetDebut(debut, vitesse);
		update = true;
	}

	public void SetCible(Vector3 cible, Vector3 normale)
	{
		trajetCible = trajetDebut;
		trajetCible.SetCible(cible, normale);
		update = true;
	}

	public void SetActif(bool active)
	{
		if (actif != active)
		{
			actif = active;
			trajetDebut.gameObject.SetActive(actif);
			trajetFin.gameObject.SetActive(actif);
		}
	}

	public void UpdateImmediate()
	{
		trajetDebut.UpdateImmediate();
		trajetFin.UpdateImmediate();
		update = true;
		Update();
	}

	public bool TrySetCible(Vector3 cible, Vector3 normale)
	{
		Vector3 cible2 = trajetCible.GetPositionCible();
		Vector3 normale2 = trajetCible.GetNormaleCible();
		bool flag = actif;
		trajetDebut.gameObject.SetActive(value: false);
		trajetFin.gameObject.SetActive(value: false);
		actif = true;
		SetCible(cible, normale);
		UpdateImmediate();
		if (forcePrevue < forceMin || forcePrevue > forceMax)
		{
			actif = flag;
			SetCible(cible2, normale2);
			UpdateImmediate();
			return false;
		}
		return true;
	}

	private void Update()
	{
		trajetDebut.gameObject.SetActive(actif);
		trajetFin.gameObject.SetActive(actif);
		if (!actif)
		{
			return;
		}
		float num = (forcePrevue = trajetDebut.force);
		visible = num >= forceMin && num <= forceMax;
		trajetDebut.SetLigneVisible(visible);
		trajetFin.SetLigneVisible(visible);
		if (autoInit)
		{
			CalculerParametres();
		}
		if (exPositionDepart != positionDepart || exVitesseDepart != vitesseDepart || exPositionCible != positionCible || exNormaleCible != normaleCible)
		{
			update = true;
		}
		if (update || Time.time >= prochainCalcul)
		{
			exPositionDepart = positionDepart;
			exVitesseDepart = vitesseDepart;
			exPositionCible = positionCible;
			exNormaleCible = normaleCible;
			prochainCalcul = Time.time + delaiCheck;
			if (autoSplit)
			{
				CalculerCollision();
			}
			CalculerTrajectoireComposee();
		}
		update = false;
	}

	public void CalculerParametres()
	{
		positionDepart = trajetDebut.GetPositionDebut();
		vitesseDepart = trajetDebut.GetVitesseDebut();
		positionCible = trajetCible.GetPositionCible();
		normaleCible = trajetCible.GetNormaleCible();
	}

	public void CalculerCollision()
	{
		intercepted = false;
		if (!visible || !split || !(Vector3.Distance(positionDepart, positionCible) > distanceMin))
		{
			return;
		}
		ProjectilePhysics.CollisionTrajectory collisionTrajectory = ProjectilePhysics.ComputeExpectedCollision(positionDepart, vitesseDepart, distanceMin, distanceMax, dureeMax, layerMask);
		if (!collisionTrajectory.IsNull)
		{
			RaycastHit hit = collisionTrajectory.hit;
			if (Vector3.Distance(positionCible, hit.point) > distanceSplitMin)
			{
				Split(hit);
			}
		}
	}

	private void Split(RaycastHit hit)
	{
		positionHit = hit.point;
		normaleHit = hit.normal;
		dureeHit = ProjectilePhysics.ComputeFlightDuration(vitesseDepart, positionHit - positionDepart);
		vitesseHit = vitesseDepart;
		ProjectilePhysics.ComputeNextPosition(positionDepart, ref vitesseHit, dureeHit);
		intercepted = true;
	}

	public void CalculerTrajectoireComposee()
	{
		if (intercepted)
		{
			trajetDebut.SetVisible(debut: true, cible: true);
			trajetFin.SetVisible(debut: true, cible: true);
			dureePrevue = ProjectilePhysics.ComputeFlightDuration(vitesseDepart, positionCible - positionDepart);
			float num = Mathf.Min(dureePrevue, dureeHit);
			trajetCible = ((num != dureePrevue) ? trajetFin : trajetDebut);
			Vector3 currentVelocity = vitesseDepart;
			Vector3 vector = ProjectilePhysics.ComputeNextPosition(positionDepart, ref currentVelocity, num);
			Vector3 normale = ((num != dureePrevue) ? normaleHit : normaleCible);
			Vector3 cible = ((num != dureePrevue) ? positionCible : positionHit);
			Vector3 normale2 = ((num != dureePrevue) ? normaleCible : normaleHit);
			trajetDebut.SetCible(vector, normale);
			trajetFin.SetDebut(vector, currentVelocity);
			trajetFin.SetCible(cible, normale2);
		}
		else if (forcePrevue >= forceMin && forcePrevue <= forceMax)
		{
			trajetDebut.SetVisible(debut: true, cible: true);
			trajetFin.SetVisible(debut: false, cible: false);
			trajetCible = trajetDebut;
			trajetDebut.SetCible(positionCible, normaleCible);
		}
		else
		{
			trajetDebut.SetVisible(debut: false, cible: false);
			trajetFin.SetVisible(debut: true, cible: true);
			trajetCible = trajetFin;
			trajetFin.SetCible(positionCible, normaleCible);
		}
	}
}
