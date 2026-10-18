using System.Collections.Generic;
using Brix.Engine;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public class VolumeRegistryComponent : MonoBehaviour
{
	private class ModifiedVoxels
	{
		public HashSet<XYZ> ObstructionVoxelChanged = new HashSet<XYZ>();

		public HashSet<XYZ> SupportVoxelChanged = new HashSet<XYZ>();

		public HashSet<XYZ> TerrainSupportVoxelChanged = new HashSet<XYZ>();

		public bool IsEmpty => ObstructionVoxelChanged.Count + SupportVoxelChanged.Count + TerrainSupportVoxelChanged.Count == 0;

		public void AddVolumeData(BaseVolume vol)
		{
			vol.GetCases(VPropertyMask.occupy | VPropertyMask.fantome | VPropertyMask.meshSwapTrigger).Slinq().AddTo(ObstructionVoxelChanged);
			vol.GetCases(VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.braceSupport | VPropertyMask.beamSupport | VPropertyMask.plankSupport | VPropertyMask.notWallSupport | VPropertyMask.canHookBeam | VPropertyMask.ropeSupport | VPropertyMask.stickySupport).Slinq().AddTo(SupportVoxelChanged);
			vol.GetCases(VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.beamSupport).Slinq().AddTo(TerrainSupportVoxelChanged);
		}

		public void AddTerrainVoxelChanged(XYZ voxel)
		{
			ObstructionVoxelChanged.Add(voxel);
			SupportVoxelChanged.Add(voxel);
			TerrainSupportVoxelChanged.Add(voxel);
		}

		public void Clear()
		{
			ObstructionVoxelChanged.Clear();
			SupportVoxelChanged.Clear();
			TerrainSupportVoxelChanged.Clear();
		}
	}

	private ModifiedVoxels _voxelsNeedingValidation = new ModifiedVoxels();

	private ModifiedVoxels _voxelsNeedingValidationBuffer = new ModifiedVoxels();

	private void Awake()
	{
		GameSignals.Once(GameSignals.LevelLoaded, GetVoxelEngine);
		GameSignals.Connect(GameSignals.TerrainRemoved, AddToVoxelNeedingRevalidation_Terrain);
		GameSignals.Connect(GameSignals.TerrainChanged, AddToVoxelNeedingRevalidation_Terrain);
		GameSignals.Once(GameSignals.LevelReady, RevalidateAllRegisteredObjects);
		VolumeRegistrator.Instance.VolumeDataAddedSignal.Connect(AddToVoxelNeedingRevalidation);
		VolumeRegistrator.Instance.VolumeDataRemovedSignal.Connect(AddToVoxelNeedingRevalidation);
		base.enabled = false;
	}

	private void RevalidateAllRegisteredObjects()
	{
		List<IVolume> list = VolumeRegistry.GetAllRegisteredObjects().Slinq().ToPooledList();
		list.Slinq().ForEach(delegate(IVolume o)
		{
			o.Validate();
		});
		list.Dispose();
	}

	private void GetVoxelEngine()
	{
		_voxelsNeedingValidation.Clear();
		base.enabled = true;
	}

	private void LateUpdate()
	{
		if (!_voxelsNeedingValidation.IsEmpty)
		{
			Donnees.Swap(ref _voxelsNeedingValidation, ref _voxelsNeedingValidationBuffer);
			List<XYZ> list = _voxelsNeedingValidationBuffer.SupportVoxelChanged.ToPooledList();
			HashSet<IVolume> hashSet = VolumeRegistry.VolumesWithPropertyAt(list, VPropertyMask.mainSupportReq | VPropertyMask.auxiliarySupportReq).Slinq().ToPooledHashSet();
			list.Dispose();
			List<XYZ> list2 = _voxelsNeedingValidationBuffer.ObstructionVoxelChanged.ToPooledList();
			VolumeRegistry.VolumesWithPropertyAt(list2, VPropertyMask.occupy | VPropertyMask.fantome | VPropertyMask.meshSwapTrigger).Slinq().AddTo(hashSet);
			list2.Dispose();
			Voxel.Engine.BeginChanges();
			ValidateTerrain();
			Voxel.Engine.ApplyChanges();
			_voxelsNeedingValidationBuffer.Clear();
			VUtil.NewVID();
			hashSet.Slinq().ForEach(delegate(IVolume v)
			{
				v.Validate();
			});
			hashSet.Dispose();
		}
	}

	private void OnDestroy()
	{
		Reset();
	}

	[ContextMenu("Reset Volume Registry")]
	private void Reset()
	{
		VolumeRegistrator.Instance.Reset();
	}

	private void AddToVoxelNeedingRevalidation(BaseVolume changedData)
	{
		_voxelsNeedingValidation.AddVolumeData(changedData);
	}

	private void AddToVoxelNeedingRevalidation_Terrain(XYZ invalidatedVoxel)
	{
		_voxelsNeedingValidation.AddTerrainVoxelChanged(invalidatedVoxel);
		_voxelsNeedingValidation.AddTerrainVoxelChanged(invalidatedVoxel + XYZ.up);
		_voxelsNeedingValidation.TerrainSupportVoxelChanged.Add(invalidatedVoxel + XYZ.up + XYZ.forward);
		_voxelsNeedingValidation.TerrainSupportVoxelChanged.Add(invalidatedVoxel + XYZ.up - XYZ.forward);
		_voxelsNeedingValidation.TerrainSupportVoxelChanged.Add(invalidatedVoxel + XYZ.up + XYZ.right);
		_voxelsNeedingValidation.TerrainSupportVoxelChanged.Add(invalidatedVoxel + XYZ.up - XYZ.right);
	}

	private void ValidateTerrain()
	{
		List<TerrainBlockData> list = (from p in _voxelsNeedingValidationBuffer.TerrainSupportVoxelChanged.Slinq()
			select Voxel.GetTerrainBlockData(p) as TerrainBlockData).NotNull().ToPooledList();
		foreach (TerrainBlockData item in list)
		{
			ValidationResult validationResult = item.Validate();
			if (validationResult == ValidationResult.Invalid)
			{
				item.DestroyBlock(withParticles: true, withGarnotte: true);
			}
			item.Dispose();
		}
		list.Dispose();
	}
}
