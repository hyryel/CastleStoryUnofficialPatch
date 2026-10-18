using UnityEngine;

namespace Brix.Legacy;

public class Sunlight : MonoBehaviour
{
	public GameObject Soleil;

	public GameObject Lune;

	public GameObject Ocean;

	public GameObject SkyboxSphere;

	public GameObject StarSphere;

	public Material MaterielNuage;

	public Journee Journee;

	public Orbite SoleilOrbite;

	public Orbite LuneOrbite;

	public Gradient SoleilGradient;

	public Gradient LuneGradient;

	public Gradient AmbientGradient;

	public Gradient FlareGradient;

	public Gradient SkyGradient;

	public Gradient SkyRimGradient;

	public Gradient WaterGradient;

	public Gradient WaterGlossiness;

	public Gradient CloudGradient;

	public Color SoleilCouleur;

	public Color HorizonCouleur;

	public Color CielCouleur;

	public float WaterRimPower;

	public float SkyRimPower;

	public Color BrumeCouleur;

	public Camera SkyboxCamera;

	public Texture2D Couleurs;

	public float BrumeDistance = 10000f;

	public void SetBrumeDistance(float d)
	{
		BrumeDistance = d;
	}

	private void Start()
	{
	}

	private void Update()
	{
		float heure = Journee.Heure;
		if ((bool)SoleilOrbite)
		{
			SoleilOrbite.Phase = heure / 24f - 0.25f;
		}
		if ((bool)LuneOrbite)
		{
			LuneOrbite.Phase = heure / 24f + 0.25f;
		}
		SkyboxSphere.transform.LookAt(Soleil.transform);
		SkyboxSphere.transform.Rotate(90f, 0f, 0f);
		BrumeCouleur = InterpolateCouleurs(Couleurs, heure / 24f, 0.273f);
		HorizonCouleur = InterpolateCouleurs(Couleurs, heure / 24f, 0.414f);
		SoleilCouleur = SoleilGradient.Evaluate(heure / 24f);
		Soleil.GetComponent<Light>().color = SoleilCouleur;
		Soleil.GetComponent<Light>().intensity = SoleilGradient.Evaluate(heure / 24f).a;
		Soleil.GetComponent<LensFlare>().color = SoleilCouleur;
		Soleil.GetComponent<LensFlare>().brightness = FlareGradient.Evaluate(heure / 24f).a;
		Lune.GetComponent<Light>().intensity = LuneGradient.Evaluate(heure / 24f).a;
		Lune.GetComponent<Light>().color = LuneGradient.Evaluate(heure / 24f);
		CielCouleur = SkyGradient.Evaluate(heure / 24f);
		SkyRimPower = SkyRimGradient.Evaluate(heure / 24f).a;
		SkyboxSphere.GetComponent<Renderer>().material.SetColor("_sunColor", SoleilCouleur);
		SkyboxSphere.GetComponent<Renderer>().material.SetColor("_horizonColor", HorizonCouleur);
		SkyboxSphere.GetComponent<Renderer>().material.SetFloat("_horizonPower", Mathf.Lerp(0.8f, 0.3f, SkyRimPower));
		SkyboxSphere.GetComponent<Renderer>().material.SetFloat("_sunPower", Mathf.Lerp(100f, 25f, SkyRimPower));
		SkyboxSphere.GetComponent<Renderer>().material.SetColor("_skyColor", CielCouleur);
		Color color = StarSphere.GetComponent<Renderer>().material.GetColor("_TintColor");
		color.a = CielCouleur.a;
		StarSphere.GetComponent<Renderer>().material.SetColor("_TintColor", color);
		Ocean.GetComponent<Renderer>().material.SetColor("_RimColor", HorizonCouleur);
		Ocean.GetComponent<Renderer>().material.SetFloat("_RimPower", Mathf.Lerp(20f, 10f, WaterGradient.Evaluate(heure / 24f).a));
		Ocean.GetComponent<Renderer>().material.SetFloat("_Glossiness", Mathf.Lerp(0.1f, 2f, WaterGlossiness.Evaluate(heure / 24f).grayscale));
		Ocean.GetComponent<Renderer>().material.SetColor("_DiffuseColor", WaterGradient.Evaluate(heure / 24f));
		RenderSettings.ambientLight = AmbientGradient.Evaluate(heure / 24f);
		RenderSettings.fogColor = BrumeCouleur;
		RenderSettings.fogEndDistance = BrumeDistance;
		MaterielNuage.color = CloudGradient.Evaluate(heure / 24f);
	}

	public Color InterpolateCouleurs(Texture2D Tex, float Index, float Rangee)
	{
		Color pixel = Tex.GetPixel(Mathf.FloorToInt((float)Tex.width * Index), Tex.height - Mathf.FloorToInt((float)Tex.height * Rangee));
		Color pixel2 = Tex.GetPixel(Mathf.CeilToInt((float)Tex.width * Index), Tex.height - Mathf.FloorToInt((float)Tex.height * Rangee));
		return Color.Lerp(pixel, pixel2, (float)Tex.width * Index % 1f);
	}
}
