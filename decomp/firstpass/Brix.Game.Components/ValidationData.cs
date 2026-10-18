using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.Components;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ValidationData
{
	public static ValidationData None => default(ValidationData);

	public Blueprint Blueprint { get; private set; }

	public Faction Faction { get; private set; }

	public bool IsBlueprint { get; private set; }

	public bool IsNotDefault { get; private set; }

	public int MaxMissingSupport { get; private set; }

	public XYZ Position { get; set; }

	public int RotationIndex { get; private set; }

	public GameObject Target { get; private set; }

	public IValidator Validator { get; private set; }

	public IVolume Volume { get; set; }

	public Quaternion Rotation => PreprocessedRotations.DiscreteRotationForIndexes[RotationIndex];

	public AxisOrient Orientation => PreprocessedRotations.AxisOrientForIndexes[RotationIndex];

	public SupportState SupportStateType => Validator.SupportStateType;

	public ValidationData(ValidatorComponent VComp)
	{
		this = default(ValidationData);
		Blueprint = VComp.Blueprint;
		Faction = VComp.faction;
		IsNotDefault = true;
		IsBlueprint = Blueprint != null;
		MaxMissingSupport = VComp.MaxMissingSupport;
		Position = XYZ.FromVector3(VComp.transform.position);
		RotationIndex = VComp.voxel.RotationIndex;
		Target = VComp.gameObject;
		Validator = VComp;
		Volume = VComp.volume;
	}

	public ValidationData(XYZ position, int rotationIndex, IVolume vol, IValidator val, int maxMissingSupport = 0, Faction fac = null, GameObject target = null, Blueprint bp = null)
	{
		this = default(ValidationData);
		Blueprint = bp;
		Faction = fac;
		IsNotDefault = true;
		IsBlueprint = bp != null;
		MaxMissingSupport = maxMissingSupport;
		Position = position;
		RotationIndex = rotationIndex;
		Target = target;
		Validator = val;
		Volume = vol;
	}

	public SupportState GetSolidity(IEnumerable<ValidationData> validationVolumes = null)
	{
		return Validator.GetSolidity(ref this, validationVolumes);
	}

	public ValidationResult GetCurrentState(IEnumerable<ValidationData> validationVolumes = null)
	{
		ValidationData vData = this;
		return Validator.GetCurrentState(ref vData, validationVolumes);
	}

	public ValidationNode GenerateBlueprintValidationNode(BlueprintGraph graph, HashSet<GameObject> gosInGraph)
	{
		if (!graph.TryGetNode(Target, out var node))
		{
			return Validator.GenerateBlueprintValidationNode(ref this, graph, gosInGraph);
		}
		return node;
	}
}
