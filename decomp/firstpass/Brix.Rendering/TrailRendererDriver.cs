using Brix.Components;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Rendering;

public class TrailRendererDriver : BrixComponent
{
	[SerializeField]
	private TrailRenderer _trailRenderer;

	private Material _sharedMaterial;

	private Material _clonedMaterial;

	private bool unParented;

	public override void Initialize()
	{
		base.Initialize();
		CloneTrailMaterial();
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		unParented = false;
	}

	public void UnparentTrail()
	{
		if (unParented)
		{
			return;
		}
		unParented = true;
		if (base.gameObject.IsNullOrReleased())
		{
			return;
		}
		base.transform.SetParent(null);
		Neo.Relais(delegate
		{
			if (!base.gameObject.IsNullOrReleased())
			{
				ObjectPoolSingleton.Release(base.transform.gameObject);
			}
		}, 1f, "arrow trail release");
	}

	private void CloneTrailMaterial()
	{
		if (_trailRenderer != null)
		{
			_sharedMaterial = _trailRenderer.sharedMaterial;
			_clonedMaterial = new Material(_sharedMaterial);
			_trailRenderer.sharedMaterial = _clonedMaterial;
			_trailRenderer.enabled = true;
		}
	}
}
