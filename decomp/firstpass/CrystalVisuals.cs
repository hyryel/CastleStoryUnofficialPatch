using Brix.Audio;
using Brix.Components;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.UI.Icons;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Smooth.Slinq;
using UnityEngine;

public class CrystalVisuals : GameComponent
{
	public enum CrystalColorEnum
	{
		Neutral,
		Blue,
		Red,
		Green,
		Orange,
		Corrupted
	}

	public bool IsInTransition;

	public float ProgressSpeed;

	private Journee clock;

	public float TransitionPointOffset = 1f;

	public GameObject DriverGameObject;

	private bool _playedStartSound;

	private Animator crystalAnimator;

	private float captureProgress;

	private float lastCaptureProgress;

	private float timeAtLastIncrement;

	private static LuaEnumDescriptor<CrystalColorEnum> _enumCrystalColor = new LuaEnumDescriptor<CrystalColorEnum>();

	public CrystalColorEnum CrystalColor
	{
		get
		{
			return (CrystalColorEnum)crystalAnimator.GetInteger("Color");
		}
		set
		{
			crystalAnimator.SetInteger("Color", (int)value);
		}
	}

	public CrystalColorEnum CreepColor
	{
		get
		{
			return (CrystalColorEnum)crystalAnimator.GetInteger("Creep Color");
		}
		set
		{
			crystalAnimator.SetInteger("Creep Color", (int)value);
		}
	}

	public float TimeOfDay
	{
		get
		{
			return crystalAnimator.GetFloat("TimeOfDay");
		}
		set
		{
			crystalAnimator.SetFloat("TimeOfDay", value);
		}
	}

	public float Light
	{
		get
		{
			return crystalAnimator.GetFloat("Light");
		}
		set
		{
			crystalAnimator.SetFloat("Light", value);
		}
	}

	public float Growth
	{
		get
		{
			return crystalAnimator.GetFloat("Growth");
		}
		set
		{
			crystalAnimator.SetFloat("Growth", value);
		}
	}

	public float CreepProgression
	{
		get
		{
			return crystalAnimator.GetFloat("Creep");
		}
		set
		{
			crystalAnimator.SetFloat("Creep", value);
		}
	}

	public bool Growing
	{
		get
		{
			return crystalAnimator.GetBool("Growing");
		}
		set
		{
			crystalAnimator.SetBool("Growing", value);
		}
	}

	public bool Vibrate
	{
		get
		{
			return crystalAnimator.GetBool("Vibrate");
		}
		set
		{
			crystalAnimator.SetBool("Vibrate", value);
		}
	}

	public bool Breathing
	{
		get
		{
			return crystalAnimator.GetBool("Breathing");
		}
		set
		{
			crystalAnimator.SetBool("Breathing", value);
		}
	}

	public bool IsActive => DriverGameObject != null && DriverGameObject.activeInHierarchy;

	public float CaptureProgress
	{
		get
		{
			return captureProgress;
		}
		set
		{
			lastCaptureProgress = captureProgress;
			captureProgress = value;
			timeAtLastIncrement = Time.fixedTime;
		}
	}

	public void ShakeOnce()
	{
		crystalAnimator.SetTrigger("Shake");
	}

	public override void Initialize()
	{
		base.Initialize();
		crystalAnimator = GetComponent<Animator>();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		clock = BrixSingleton<AutoList>.Instance.GetInstances(Environment.Skybox3d).Slinq().First()
			.GetComponent<Journee>();
		GameSignals.LateConnectOnce(GameSignals.SoundEngineReady, delegate
		{
			if (IsActive)
			{
				Faction newFaction = GetComponentInParent<AffiliationComponent>().faction;
				PlayStartSound(newFaction);
			}
		});
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (IsInTransition)
		{
			EndTransition();
		}
		PlayStopSound();
	}

	public void StartTransition(Faction capturer, Faction owner)
	{
		IsInTransition = true;
		Vibrate = true;
		CreepColor = capturer.CrystalColor;
		if (IsActive)
		{
			PlayVibrateStartSound(capturer, owner);
		}
	}

	public void EndTransition()
	{
		IsInTransition = false;
		Vibrate = false;
		if (IsActive)
		{
			PlayVibrateStopSound();
		}
	}

	public void FactionChanged(Faction oldFaction, Faction newFaction)
	{
		CrystalColor = newFaction.CrystalColor;
		if (IsActive && GameSignals.Received(GameSignals.SoundEngineReady))
		{
			PlayCapturedSound(newFaction);
			PlayStartSound(newFaction);
		}
	}

	private void PlayStartSound(Faction newFaction)
	{
		bool isHome = GetComponentInParent<FireflyNest>().isHome;
		if (newFaction.IsAlliedToMe)
		{
			SoundEngine.Play((!isHome) ? "Crystal_Ally_Small_Start" : "Crystal_Ally_Big_Start", base.gameObject);
		}
		else if (newFaction.isAI)
		{
			SoundEngine.Play((!isHome) ? "Crystal_Corruptron_Small_Start" : "Crystal_Corruptron_Big_Start", base.gameObject);
		}
		else if (newFaction.IsEnemyToMe)
		{
			SoundEngine.Play((!isHome) ? "Crystal_Enemy_Small_Start" : "Crystal_Enemy_Big_Start", base.gameObject);
		}
		else
		{
			SoundEngine.Play((!isHome) ? "Crystal_Neutral_Small_Start" : "Crystal_Neutral_Big_Start", base.gameObject);
		}
		_playedStartSound = true;
	}

	private void PlayStopSound()
	{
		if (_playedStartSound)
		{
			SoundEngine.Play("Crystal_Stop", base.gameObject);
		}
		_playedStartSound = false;
	}

	private void PlayCapturedSound(Faction newFaction)
	{
		if (newFaction == Faction.DefaultAllied)
		{
			SoundEngine.Play("Crystal_Captured_ByPlayer", base.gameObject);
		}
		else if (newFaction.IsAlliedToMe)
		{
			SoundEngine.Play("Crystal_Captured_ByAlly", base.gameObject);
		}
		else if (newFaction.isAI)
		{
			SoundEngine.Play("Crystal_Captured_ByCorruptron", base.gameObject);
		}
		else if (newFaction.IsEnemyToMe)
		{
			SoundEngine.Play("Crystal_Captured_ByEnemy", base.gameObject);
		}
		else
		{
			SoundEngine.Play("Crystal_BackToNeutral", base.gameObject);
		}
	}

	private void PlayVibrateStartSound(Faction capturer, Faction owner)
	{
		if (capturer.IsNeutralToMe)
		{
			if (owner == Faction.DefaultAllied)
			{
				SoundEngine.Play("Crystal_Shake_Neutralizing_FromPlayer", base.gameObject);
			}
			else if (owner.IsAlliedToMe)
			{
				SoundEngine.Play("Crystal_Shake_Neutralizing_FromAlly", base.gameObject);
			}
			else if (owner.isAI)
			{
				SoundEngine.Play("Crystal_Shake_Neutralizing_FromCorruptron", base.gameObject);
			}
			else if (owner.IsEnemyToMe)
			{
				SoundEngine.Play("Crystal_Shake_Neutralizing_FromEnemy", base.gameObject);
			}
		}
		else if (capturer == Faction.DefaultAllied)
		{
			SoundEngine.Play("Crystal_Shake_PowerUp_ToPlayer", base.gameObject);
		}
		else if (capturer.IsAlliedToMe)
		{
			SoundEngine.Play("Crystal_Shake_PowerUp_ToAlly", base.gameObject);
		}
		else if (capturer.isAI)
		{
			SoundEngine.Play("Crystal_Shake_PowerUp_ToCorruptron", base.gameObject);
		}
		else if (capturer.IsEnemyToMe)
		{
			SoundEngine.Play("Crystal_Shake_PowerUp_ToEnemy", base.gameObject);
		}
	}

	private void PlayVibrateStopSound()
	{
		SoundEngine.Play("Crystal_Shake_Stop", base.gameObject);
	}

	private void Update()
	{
		if (IsInTransition)
		{
			float creepProgression = Mathf.Lerp(lastCaptureProgress, captureProgress, (Time.fixedTime - timeAtLastIncrement) / ProgressSpeed);
			CreepProgression = creepProgression;
		}
		if (clock != null)
		{
			TimeOfDay = Mathf.Lerp(1f, 0f, Mathf.Abs(clock.Heure - 12f) / 12f);
		}
	}

	public static void RegisterClass(Script script)
	{
		LuaLibs.RegisterUserData(typeof(IconKeys));
		script.Globals["CrystalColor"] = _enumCrystalColor.CreateDynValue();
	}
}
