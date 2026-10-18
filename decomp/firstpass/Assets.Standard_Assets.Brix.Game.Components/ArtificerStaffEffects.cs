using Brix.Audio;
using Brix.Components;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.UI;
using Brix.Utils.Unity;
using UnityEngine;

namespace Assets.Standard_Assets.Brix.Game.Components;

internal class ArtificerStaffEffects : GameComponent
{
	private enum Mode
	{
		Heal,
		Sentinel
	}

	private static readonly string SoundChangeGem = "Artificier_Change_Gem";

	private static readonly string SoundHealingStart = "Artificier_Heal_Start";

	private static readonly string SoundHealingEnd = "Artificier_Heal_Stop";

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private Material AttackMaterial;

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private Material HealMaterial;

	[SerializeField]
	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	private SphereHighlight.Spawner highlight;

	[SerializeField]
	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	private GameObject aura;

	private WardRadiusRenderer.RendererEntry _rendererEntry;

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private ParticleSystem particle;

	[DisplayRules(DisplayFlags.ReadOnlyInEditMode)]
	[SerializeField]
	private MeshRenderer crystal;

	private Mode mode;

	public void LinkToVisual()
	{
		Transform transform = base.transform.Find("Visual").transform.Find("Crystal");
		crystal = transform.GetComponent<MeshRenderer>();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		_rendererEntry = new WardRadiusRenderer.RendererEntry
		{
			Color = Factory.Peek(ObjetsDynamiques.HealingWard).GetComponent<WardController>().HoverDisplayColor,
			Group = WardController.WardHoverGroup.Healing
		};
	}

	public void ToSentinelStone()
	{
		if (mode != Mode.Sentinel)
		{
			particle.Play();
			SoundEngine.Play(SoundChangeGem, base.gameObject);
		}
		mode = Mode.Sentinel;
		crystal.sharedMaterial = AttackMaterial;
		SoundEngine.Play(SoundHealingEnd, base.gameObject);
	}

	public void ToHealStone()
	{
		if (mode != Mode.Heal)
		{
			SoundEngine.Play(SoundChangeGem, base.gameObject);
		}
		mode = Mode.Heal;
		crystal.sharedMaterial = HealMaterial;
		SoundEngine.Play(SoundHealingStart, base.gameObject);
	}

	public void ShowAura(float radius)
	{
		Vector3 vector = Vector3.one * (radius * 2f);
		highlight.scale = vector;
		aura.transform.localScale = vector;
		highlight.active = true;
		aura.SetActive(value: true);
		_rendererEntry.Component = GetComponent<Tool>().CurrentUser.GetComponent<GameComponent>();
		_rendererEntry.Range = radius;
		BrixSingleton<WardRadiusRenderer>.Instance.AddTemporaryEntry(_rendererEntry);
	}

	public void HideAura()
	{
		highlight.active = false;
		aura.SetActive(value: false);
		if (_rendererEntry.Component != null)
		{
			BrixSingleton<WardRadiusRenderer>.Instance.RemoveTemporaryEntry(_rendererEntry);
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		highlight.Release();
		if (_rendererEntry.Component != null)
		{
			BrixSingleton<WardRadiusRenderer>.Instance.RemoveTemporaryEntry(_rendererEntry);
		}
		SoundEngine.Play(SoundHealingEnd, base.gameObject);
	}
}
