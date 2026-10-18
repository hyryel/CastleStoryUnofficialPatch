using System.Collections.Generic;
using Brix.Game.Network;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.Game.Components;

public class ScarfFactionColoring : GameComponent
{
	private static Dictionary<Color, Material> _materials = new Dictionary<Color, Material>();

	public Material baseMaterial;

	public MeshRenderer scarfRenderer;

	protected override void Start()
	{
		TryUpdate(GetComponentInParent<AffiliationComponent>());
		GetComponent<GenericParentTracker>().ObjectTrackedByNewParentSignal.Connect(delegate(GameObject go)
		{
			TryUpdate(go);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void TryUpdate(GameObject go)
	{
		if (go != null)
		{
			TryUpdate(go.GetComponent<AffiliationComponent>());
		}
	}

	private void TryUpdate(AffiliationComponent affcom)
	{
		if (affcom != null)
		{
			UpdateColor(affcom.faction.FactionColor);
		}
	}

	public void UpdateColor(Color newColor)
	{
		if (!_materials.TryGetValue(newColor, out var value))
		{
			value = new Material(baseMaterial);
			value.SetColor("_Color", newColor);
			_materials.Add(newColor, value);
		}
		scarfRenderer.sharedMaterial = value;
	}
}
