using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Network;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class StateRenderer : NetworkGameComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(StateRenderer).FullName);

	[SerializeField]
	private Blueprint _blueprint;

	[SerializeField]
	private List<MeshRenderer> _renderers;

	[SerializeField]
	private BlueprintMaterialCache.MaterialTrio.Mode passes = BlueprintMaterialCache.MaterialTrio.Mode.ThreePass;

	[SerializeField]
	public Texture NormalMap;

	[SerializeField]
	private Material ProgressMaterial;

	public Signal<Material[]> OnMaterialChange = new Signal<Material[]>();

	private bool _progressOverride;

	private bool _isVisible;

	public bool IsVisible => _isVisible;

	public override void OnRequested()
	{
		base.OnRequested();
		_progressOverride = false;
		if (_blueprint == null)
		{
			_blueprint = GetComponentInParent<Blueprint>();
			logger.WarnFormat("StateRenderer._blueprint {0} must be assigned for {1} ({2})", _blueprint, this, _blueprint.AssetKey);
		}
		_blueprint.OnStateChangedSignal.Connect(StateChange).DisconnectWhen(base.OnReleaseOrDestroySignal);
		RefreshVisual();
	}

	public void RefreshVisual()
	{
		UpdateRenderersMaterial(_blueprint.State);
	}

	public void Show(bool value)
	{
		foreach (MeshRenderer renderer in _renderers)
		{
			if (renderer != null)
			{
				renderer.enabled = value;
				_isVisible = value;
			}
			else
			{
				logger.ErrorFormat("StateRenderer {0} has a null object in rendererlist", base.gameObject.name);
			}
		}
	}

	private void UpdateRenderersMaterial(PlacementFlags mode)
	{
		if (_progressOverride)
		{
			return;
		}
		IBlueprintMaterialProvider blueprintMaterialProvider = null;
		if (_blueprint.Project != null)
		{
			blueprintMaterialProvider = _blueprint.Project.GetComponent<IBlueprintMaterialProvider>();
		}
		Material[] parameter;
		float scale;
		if (blueprintMaterialProvider != null)
		{
			BlueprintMaterialData sharedMaterialForAsset = blueprintMaterialProvider.GetSharedMaterialForAsset(_blueprint.AssetKey, mode);
			parameter = sharedMaterialForAsset.materials;
			scale = sharedMaterialForAsset.Scale;
		}
		else
		{
			BlueprintMaterialCache.BlueprintDisplay blueprintDisplay = BlueprintMaterialCache.ForMode(mode);
			parameter = blueprintDisplay.ForKey(_blueprint.AssetKey, UseCache: true).WithPasses(passes);
			scale = blueprintDisplay.Scale;
		}
		_renderers.Slinq().ForEach(delegate(MeshRenderer x, Material[] p)
		{
			x.sharedMaterials = p;
		}, parameter);
		if ((bool)GetComponent<RopeBridgeConfigurator>())
		{
			GetComponentsInChildren<LineRenderer>().Slinq().ForEach(delegate(LineRenderer x, Material[] p)
			{
				x.sharedMaterials = p;
			}, parameter);
		}
		base.transform.localScale = Vector3.one * scale;
	}

	private void StateChange(Blueprint blueprint, PlacementFlags previousMode, PlacementFlags newMode)
	{
		UpdateRenderersMaterial(newMode);
	}

	public void ProgressOverride(bool toggle)
	{
		bool flag = toggle && _blueprint.faction.IsOwnedByMe;
		if (flag)
		{
			_progressOverride = flag;
			MeshRenderer component = GetComponent<MeshRenderer>();
			component.sharedMaterials = new Material[1] { ProgressMaterial };
			component.enabled = true;
			MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				meshRenderer.enabled = flag;
			}
		}
	}

	[ContextMenu("Setup")]
	private void Setup()
	{
		_blueprint = GetComponent<Blueprint>();
		_renderers.Add(GetComponent<MeshRenderer>());
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
