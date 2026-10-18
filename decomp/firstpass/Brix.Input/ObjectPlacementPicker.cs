using Brix.Audio;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Lua;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Input;

public class ObjectPlacementPicker : GrabberComponentV2
{
	public class MarkerPlacementOptions : IPickerComponentOptions
	{
		public string name = string.Empty;

		public MarkerPlacementOptions(string name)
		{
			this.name = name;
		}
	}

	private BrixLogger logger = new BrixLogger(typeof(ObjectPlacementPicker).FullName);

	private IFitter _masterFitter;

	private string _markerName;

	private LuaMarker _marker;

	public override void OnHoverAny(bool hovering)
	{
		if (_heldObject == null)
		{
			if (_masterKey == null)
			{
				logger.Warn("ObjectPlacementPicker.OnHoverAny was invoked with no MasterKey. Picker is broken.");
			}
			HoldKey(_masterKey);
		}
		if (!_holdingClick)
		{
			if (_heldDragger != null)
			{
				_heldDragger.ClearSlaves();
			}
			_heldObject.GetHeldComponent<IFitter>().FitAndOrient(_voxelRayHit);
			_heldObject.GetHeldComponent<ValidatorComponent>().Validate();
		}
		if (_heldObject.HeldObject.activeSelf && _holdingClick && _heldDragger != null)
		{
			_heldDragger.GenerateSlaves(_voxelRayHit);
		}
	}

	protected override void ChangeFit()
	{
		if (_masterFitter != null)
		{
			_masterFitter.ChangeFit();
		}
		_heldObject.GetHeldComponent<IFitter>().ChangeFit();
		if (_heldDragger != null)
		{
			foreach (Slave slafe in _heldDragger.GetSlaves())
			{
				slafe.GetHeldComponent<IFitter>().ChangeFit();
			}
		}
		SoundEngine.Play("Brick_Turn");
	}

	protected override void PlaceHeldObjects()
	{
		Blueprint heldComponent = _heldObject.GetHeldComponent<Blueprint>();
		if (heldComponent == null || (heldComponent.State &= PlacementFlags.valid) > PlacementFlags.none)
		{
			if (_marker != null)
			{
				_marker.Placed = true;
				_marker = null;
			}
			base.PlaceHeldObjects();
		}
	}

	public override void HoldKey(Factory.AssetKey key, Vector3 position, Quaternion rotation)
	{
		Object.Destroy((Component)_masterFitter);
		GameObject gameObject = Factory.Peek(key);
		IFitter fitter = gameObject.GetInterface<IFitter>();
		if (fitter is RopeBridgePlacementFitter)
		{
			_masterFitter = null;
		}
		else
		{
			_masterFitter = fitter.CopyTo(base.gameObject);
		}
		base.HoldKey(key, position, rotation);
		_marker = _heldObject.GetHeldComponent<LuaMarker>();
		if (_marker != null && _marker.markerGroups.Count == 0)
		{
			_marker.markerGroups.Add(_markerName);
		}
	}

	public override void SetHeldObject(GameObject gameObject)
	{
		base.SetHeldObject(gameObject);
		if (_masterFitter != null)
		{
			gameObject.GetInterface<IFitter>().CloneInternals(_masterFitter);
		}
	}

	public override void SetOptions(IPickerComponentOptions options)
	{
		if (options is MarkerPlacementOptions markerPlacementOptions)
		{
			_markerName = markerPlacementOptions.name;
		}
	}
}
