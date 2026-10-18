using Brix.External.Factories;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.UI.GameSelector;
using Brix.Utils.Factories;
using UnityEngine;

public class SelectionCircle : GameComponent
{
	public enum MainColors
	{
		Local,
		Allies,
		Enemies
	}

	public enum SecondaryColors
	{
		Stamina,
		Heat
	}

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private MeshRenderer _renderer;

	private float RADIUS_SCALE = 2f;

	private int _Selected = Animator.StringToHash("Selected");

	private int _ShowHp = Animator.StringToHash("ShowHP");

	private int _ShowStam = Animator.StringToHash("ShowStam");

	private int _HP = Animator.StringToHash("HP");

	private int _Stam = Animator.StringToHash("Stam");

	private int _HPFlash = Animator.StringToHash("HPFlash");

	private int _StamFlash = Animator.StringToHash("StamFlash");

	private int _HPFlicker = Animator.StringToHash("HPFlicker");

	private int _StamFlicker = Animator.StringToHash("StamFlicker");

	private int _MainColor = Animator.StringToHash("MainColor");

	private int _SecondaryColor = Animator.StringToHash("SecondaryColor");

	public override void ResetComponent()
	{
		base.ResetComponent();
		_renderer.enabled = false;
		Radius(1f);
		Selected(value: false);
		ShowHp(value: false);
		ShowStamina(value: false);
		Hp(0f);
		Stamina(0f);
		animator.ResetTrigger(_HPFlash);
		animator.ResetTrigger(_StamFlash);
	}

	public override void OnRequested()
	{
		base.OnRequested();
		_renderer.enabled = true;
	}

	public static SelectionCircle New(GameObject go)
	{
		ObjectPoolSingleton.RequestComponent<SelectionCircle>(out var t, Test.SelectionCircle);
		t.transform.parent = go.transform;
		t.transform.localPosition = GetCircleOffset(go);
		t.transform.localEulerAngles = Vector3.zero;
		return t;
	}

	public static SelectionCircle New(Vector3 pos)
	{
		ObjectPoolSingleton.RequestComponent<SelectionCircle>(out var t, Test.SelectionCircle);
		t.transform.position = pos;
		t.transform.localEulerAngles = Vector3.zero;
		return t;
	}

	public void AdaptToBricktron(Labor labor)
	{
		BricktronDamageReceiver bricktronDamageReceiver = labor.damageReceiver as BricktronDamageReceiver;
		Selected(UIGameSelector.IsFocused(labor.gameObject));
		Radius(labor.Occupation.Profession.Size);
		Hp(bricktronDamageReceiver.CurrentHP / bricktronDamageReceiver.MaxHP);
		ShowHp(bricktronDamageReceiver.IsDamaged);
		Stamina(labor, bricktronDamageReceiver);
	}

	private void Stamina(Labor labor, BricktronDamageReceiver damageReceiver)
	{
		if (labor.Occupation.Profession.UsesStamina)
		{
			SecondaryColor(SecondaryColors.Stamina);
			ShowStamina(!damageReceiver.FullStamina);
			Stamina(damageReceiver.CurrentStamina / damageReceiver.MaxStamina);
		}
		else if (labor.Occupation.CurrentJob == Occupation.Job.Alchemist)
		{
			SecondaryColor(SecondaryColors.Heat);
			Stamina((float)labor.GetComponent<CharacterState>().AmmoCount / 10f);
			ShowStamina(Affiliation.BelongsToAllied(labor.gameObject));
		}
		else
		{
			ShowStamina(value: false);
		}
	}

	private static Vector3 GetCircleOffset(GameObject go)
	{
		Vector3 pos = Vector3.zero;
		return (!go.WithComponent((UISelectable s) => s.CirclePositionOverride(out pos), _default: false)) ? ComputeOffsetFromMesh(go) : pos;
	}

	private static Vector3 ComputeOffsetFromMesh(GameObject go)
	{
		float y = go.WithComponent((MeshFilter m) => m.sharedMesh.bounds.center.y - m.sharedMesh.bounds.extents.y, 0f) + 0.2f;
		return new Vector3(0f, y, 0f);
	}

	public void Radius(float value)
	{
		base.transform.localScale = Vector3.one * value * RADIUS_SCALE;
	}

	public void MainColor(MainColors color)
	{
		animator.SetInteger(_MainColor, (int)color);
	}

	public void SecondaryColor(SecondaryColors color)
	{
		animator.SetInteger(_SecondaryColor, (int)color);
	}

	public void Selected(bool value)
	{
		animator.SetBool(_Selected, value);
	}

	public void ShowHp(bool value)
	{
		animator.SetBool(_ShowHp, value);
	}

	public void ShowStamina(bool value)
	{
		animator.SetBool(_ShowStam, value);
	}

	public void Hp(float value)
	{
		animator.SetFloat(_HP, value);
	}

	public void Stamina(float value)
	{
		animator.SetFloat(_Stam, value);
	}

	public void FlashHp(bool value)
	{
		if (value)
		{
			animator.SetTrigger(_HPFlash);
		}
	}

	public void FlashStamina(bool value)
	{
		if (value)
		{
			animator.SetTrigger(_StamFlash);
		}
	}

	public void HpFlicker(bool value)
	{
		animator.SetBool(_HPFlicker, value);
	}

	public void StamFlicker(bool value)
	{
		animator.SetBool(_StamFlicker, value);
	}

	private void Update()
	{
		base.transform.up = Vector3.up;
	}
}
