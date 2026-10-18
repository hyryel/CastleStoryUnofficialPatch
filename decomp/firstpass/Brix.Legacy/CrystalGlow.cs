using System.Collections;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Legacy;

public class CrystalGlow : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(CrystalGlow).FullName);

	public static Journee journee;

	private bool allume;

	public Vector2 intervalLight;

	public Vector2 intervalIlumin;

	public Vector2 fumeeAlphaEmission;

	public Vector2 delaisPulseur;

	public bool modulationLumiere;

	public bool emissionFumee;

	public GameObject pointLight;

	public GameObject particule;

	public bool alwaysGlow;

	private float intensite;

	private float force;

	private Color couleur;

	private float fade;

	private float longFade;

	private float shortFade;

	private bool fadingOut;

	public Pulseur pulseur;

	public float heure18
	{
		get
		{
			float num = ((!journee) ? 4f : journee.Heure);
			return (num + 24f - 18f) % 24f;
		}
	}

	private void Start()
	{
		if (journee == null)
		{
			journee = Journee.instance;
		}
		allume = false;
		particule.GetComponent<ParticleSystem>().Stop();
		pulseur = new Pulseur(delaisPulseur.x, delaisPulseur.y, 0f, 1f);
		if (fumeeAlphaEmission == Vector2.zero)
		{
			fumeeAlphaEmission = new Vector2(particule.GetComponent<Renderer>().material.GetColor("_TintColor").a, particule.GetComponent<ParticleSystem>().emissionRate);
		}
	}

	private void Update()
	{
		intensite = pulseur.ShortPulseInterval((!modulationLumiere) ? intervalLight.y : intervalLight.x, intervalLight.y);
		if ((heure18 < 12f || alwaysGlow) && !allume && emissionFumee)
		{
			Allume();
		}
		pointLight.GetComponent<Light>().intensity = intensite;
		if (!particule.GetComponent<Renderer>().material.HasProperty("_TintColor"))
		{
			logger.Info(string.Concat("IT'S ALL ", base.gameObject, "'S FAULT."));
		}
		Color color = particule.GetComponent<Renderer>().material.GetColor("_TintColor");
		color.a = fumeeAlphaEmission.x * pulseur.LongFade;
		particule.GetComponent<Renderer>().material.SetColor("_TintColor", color);
		GetComponent<Renderer>().material.SetFloat("_IllumPower", pulseur.ShortPulseInterval((!modulationLumiere) ? intervalIlumin.y : intervalIlumin.x, intervalIlumin.y));
		particule.GetComponent<ParticleSystem>().emissionRate = fumeeAlphaEmission.y * pulseur.LongFade;
	}

	public IEnumerator Allume()
	{
		allume = true;
		particule.GetComponent<ParticleSystem>().Play();
		while ((double)heure18 < 11.5)
		{
			yield return new WaitForSeconds(0f);
		}
		while (pulseur.LongFade > 0f)
		{
			yield return new WaitForSeconds(0f);
		}
		allume = false;
		particule.GetComponent<ParticleSystem>().Stop();
	}

	public void SetFade(float f)
	{
		pulseur.longFade = f;
		pulseur.dernierCheck = Time.frameCount;
	}
}
