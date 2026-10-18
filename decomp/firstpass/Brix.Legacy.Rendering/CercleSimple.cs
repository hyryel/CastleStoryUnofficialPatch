using Brix.Audio;
using Brix.Game.Components;
using Brix.Lua;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Legacy.Rendering;

public class CercleSimple : GameComponent, IRadialRenderer
{
	[JsonProperty]
	public float rayon = 2f;

	[JsonProperty]
	public float epaisseur = 0.2f;

	[JsonProperty]
	public Color couleur;

	public float progres;

	public bool ouvert;

	public bool flash;

	public ArcRenderer arcGrand;

	public ArcRenderer arcPetit;

	public SphereCollider sphereCollider;

	public AnimationCurve courbeOuvre;

	public AnimationCurve courbeFerme;

	public float delaiOuvreFerme = 0.3f;

	public float rayonFacteurFerme = 0.1f;

	public AnimationCurve courbeDebutProgres;

	public float delaiDebutProgres = 0.1f;

	public float epaisseurFacteurProgres = 0.5f;

	public AnimationCurve courbeFlash;

	public float delaiFlash = 0.1f;

	public Color couleurFlash = Color.white;

	private string sonOuvre = "Tutorial_Ring_Appear";

	private string sonFerme = "Tutorial_Ring_Disappear";

	private string sonDebutProgres = "Tutorial_Ring_Activate";

	private string sonFinProgres = string.Empty;

	private string sonFlash = "Tutorial_Ring_Complete";

	private bool utiliserSonsLocaux = true;

	private bool exOuvert;

	private float exProgres;

	private float exRayon;

	private float exEpaisseur;

	private Color exCouleur;

	private float startChangeOuvert;

	private float fractionOuvert = 1f;

	private float startDebutProgres;

	private float fractionDebutProgres = 1f;

	private float startFlash;

	private float fractionFlash;

	public float Radius
	{
		get
		{
			return rayon;
		}
		set
		{
			rayon = value;
			if (sphereCollider != null)
			{
				sphereCollider.radius = rayon;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Refresh();
	}

	private void Update()
	{
		if (exOuvert != ouvert)
		{
			exOuvert = ouvert;
			startChangeOuvert = Time.time;
			Son((!ouvert) ? sonFerme : sonOuvre, utiliserSonsLocaux);
		}
		if (exProgres > 0f != progres > 0f)
		{
			exProgres = progres;
			startDebutProgres = Time.time;
			if (progres > 0f)
			{
				Son(sonDebutProgres, utiliserSonsLocaux);
			}
		}
		if (exProgres >= 1f != progres >= 1f)
		{
			exProgres = progres;
			if (progres >= 1f)
			{
				Son(sonFinProgres, utiliserSonsLocaux);
			}
		}
		if (flash)
		{
			flash = false;
			startFlash = Time.time;
			Son(sonFlash, utiliserSonsLocaux);
		}
		if (exProgres != progres || exRayon != rayon || exEpaisseur != epaisseur || exCouleur != couleur || startChangeOuvert > 0f || startDebutProgres > 0f || startFlash > 0f)
		{
			exProgres = progres;
			exRayon = rayon;
			exEpaisseur = epaisseur;
			exCouleur = couleur;
			Refresh();
		}
	}

	private void CalculerFractionOuvert()
	{
		if (startChangeOuvert > 0f)
		{
			float num = Mathf.Clamp((Time.time - startChangeOuvert) / delaiOuvreFerme, 0f, 1f);
			if (num >= 1f)
			{
				startChangeOuvert = 0f;
			}
			fractionOuvert = ((!ouvert) ? (1f - courbeFerme.Evaluate(num)) : courbeOuvre.Evaluate(num));
		}
		else
		{
			fractionOuvert = (ouvert ? 1 : 0);
		}
	}

	private void CalculerFractionDebutProgres()
	{
		if (startDebutProgres > 0f)
		{
			float num = Mathf.Clamp((Time.time - startDebutProgres) / delaiDebutProgres, 0f, 1f);
			if (num >= 1f)
			{
				startDebutProgres = 0f;
			}
			fractionDebutProgres = ((!(progres > 0f)) ? courbeDebutProgres.Evaluate(1f - num) : courbeDebutProgres.Evaluate(num));
		}
		else
		{
			fractionDebutProgres = ((progres > 0f) ? 1 : 0);
		}
	}

	private void CalculerFractionFlash()
	{
		if (startFlash > 0f)
		{
			float num = Mathf.Clamp((Time.time - startFlash) / delaiFlash, 0f, 1f);
			if (num >= 1f)
			{
				startFlash = 0f;
			}
			fractionFlash = courbeFlash.Evaluate(num);
		}
		else
		{
			fractionFlash = 0f;
		}
	}

	private void Son(string son, bool local)
	{
		if (!string.IsNullOrEmpty(son))
		{
			if (local)
			{
				SoundEngine.Play(son, base.gameObject);
			}
			else
			{
				SoundEngine.Play(son);
			}
		}
	}

	private float Lerp(float a, float b, float p)
	{
		return a + (b - a) * p;
	}

	private void Refresh()
	{
		CalculerFractionOuvert();
		CalculerFractionDebutProgres();
		CalculerFractionFlash();
		float num = Lerp(rayonFacteurFerme, 1f, fractionOuvert) * rayon;
		float num2 = Lerp(1f, epaisseurFacteurProgres, fractionDebutProgres) * epaisseur;
		Color color = Color.Lerp(couleur, couleurFlash, fractionFlash);
		bool flag = progres < 1f && !Mathf.Approximately(num2, epaisseur);
		bool actif = fractionOuvert > 0f;
		if (arcPetit != null)
		{
			arcPetit.actif = flag;
		}
		if (arcGrand != null)
		{
			arcGrand.actif = actif;
		}
		if (arcGrand != null && arcGrand.actif)
		{
			arcGrand.petitRayon = num - epaisseur / 2f;
			arcGrand.grandRayon = num + epaisseur / 2f;
			arcGrand.angleMin = 0f;
			arcGrand.angleMax = ((!flag) ? 360f : (360f * Mathf.Clamp(progres, 0f, 1f)));
			arcGrand.GetComponent<Renderer>().material.color = color;
		}
		if (arcPetit != null && arcPetit.actif)
		{
			arcPetit.angleMin = 0f;
			arcPetit.angleMax = 360f;
			arcPetit.petitRayon = num + epaisseur / 2f - num2;
			arcPetit.grandRayon = num + epaisseur / 2f;
			arcPetit.GetComponent<Renderer>().material.color = color;
		}
	}

	[ContextMenu("Appear")]
	public void Appear()
	{
		ouvert = true;
	}

	[ContextMenu("Disappear")]
	public void Disappear()
	{
		ouvert = false;
	}

	public void SetRadius(float rayon)
	{
		Radius = rayon;
	}

	public void SetFraction(float fraction)
	{
		progres = fraction;
	}

	[ContextMenu("Flash")]
	public void Flash()
	{
		flash = true;
	}

	[LuaDrivable("Appear")]
	private static DynValue lua_Appear(ScriptExecutionContext context, CallbackArguments args)
	{
		CercleSimple cercleSimple = args.AsComponent<CercleSimple>(0, "Appear");
		cercleSimple.ouvert = true;
		return DynValue.Void;
	}

	[LuaDrivable("Disappear")]
	private static DynValue lua_Disappear(ScriptExecutionContext context, CallbackArguments args)
	{
		CercleSimple cercleSimple = args.AsComponent<CercleSimple>(0, "Disappear");
		cercleSimple.ouvert = false;
		return DynValue.Void;
	}

	[LuaDrivable("SetRadius")]
	private static DynValue lua_SetRadius(ScriptExecutionContext context, CallbackArguments args)
	{
		CercleSimple cercleSimple = args.AsComponent<CercleSimple>(0, "SetRadius");
		cercleSimple.Radius = args.AsFloat(1, "SetRadius");
		return DynValue.Void;
	}

	[LuaDrivable("SetProgress")]
	private static DynValue lua_SetFraction(ScriptExecutionContext context, CallbackArguments args)
	{
		CercleSimple cercleSimple = args.AsComponent<CercleSimple>(0, "SetProgress");
		cercleSimple.progres = args.AsFloat(1, "SetProgress");
		return DynValue.Void;
	}

	[LuaDrivable("Flash")]
	private static DynValue lua_Flash(ScriptExecutionContext context, CallbackArguments args)
	{
		CercleSimple cercleSimple = args.AsComponent<CercleSimple>(0, "Flash");
		cercleSimple.flash = true;
		return DynValue.Void;
	}
}
