using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.UI.GameSelector;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.Factories;

[RequireComponent(typeof(BlockInfoGenerator))]
public class PlacedBlockFactoryAssetCreator : MonoBehaviour
{
	private static VPropertyMask PlaceBlockVProperties = VPropertyMask.fantome | VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.braceSupport | VPropertyMask.beamSupport | VPropertyMask.plankSupport | VPropertyMask.mainSupportReq | VPropertyMask.auxiliarySupportReq | VPropertyMask.meshSwapTrigger | VPropertyMask.notWallSupport | VPropertyMask.canHookBeam | VPropertyMask.ropeSupport | VPropertyMask.stickySupport;

	private static List<Tuple<VProperty, VProperty>> convertedPropertyForFullBlock;

	private static List<Tuple<VProperty, VProperty>> convertedPropertyForPartialBlock;

	private void Awake()
	{
		convertedPropertyForFullBlock = new List<Tuple<VProperty, VProperty>>();
		convertedPropertyForFullBlock.Add(Tuple.Create(VProperty.fantome, VProperty.occupy));
		convertedPropertyForFullBlock.Add(Tuple.Create(VProperty.fantome, VProperty.debloc));
		convertedPropertyForPartialBlock = new List<Tuple<VProperty, VProperty>>();
		convertedPropertyForPartialBlock.Add(Tuple.Create(VProperty.fantome, VProperty.occupy));
		convertedPropertyForPartialBlock.Add(Tuple.Create(VProperty.fantome, VProperty.deblocTerrain));
		GetComponent<BlockInfoGenerator>().BlocInfoCreatedSignal.Connect(CreatePlacedBlock);
	}

	public void CreatePlacedBlock(BlockInfo bi)
	{
		if (!bi.BlueprintKey.IsNullOrInvalid())
		{
			CreatePlacedBlockInFactory(bi, ToPlacedBlockAssetKey(bi.BlueprintKey), bi.BlueprintKey, bi.Key);
		}
	}

	private Factory.AssetKey ToPlacedBlockAssetKey(Factory.AssetKey blueprintKey)
	{
		return new Factory.AssetKey("PlacedBlock", blueprintKey.Name);
	}

	private void CreatePlacedBlockInFactory(BlockInfo bi, Factory.AssetKey placedBlockAssetKey, Factory.AssetKey blueprintKey, Factory.AssetKey freeBlockKey)
	{
		GameObject gameObject = Factory.Peek(blueprintKey);
		GameObject orCreateAsset = GetOrCreateFactory(placedBlockAssetKey).GetOrCreateAsset(placedBlockAssetKey.Name);
		ConfigureComponents(orCreateAsset, bi, placedBlockAssetKey, gameObject, freeBlockKey);
		ConfigureVolumeData(bi, placedBlockAssetKey, blueprintKey);
		gameObject.GetComponent<Blueprint>().BlueprintResult = placedBlockAssetKey;
		gameObject.GetComponent<ValidatorComponent>().AttachBlockValidatorCounterpart(orCreateAsset);
	}

	private static void ConfigureComponents(GameObject asset, BlockInfo bi, Factory.AssetKey assetKey, GameObject blueprintGO, Factory.AssetKey freeBlockKey)
	{
		asset.AddComponent<VoxelTransform>().InitialCenterOffset = blueprintGO.GetComponent<VoxelTransform>().InitialCenterOffset;
		asset.AddComponent<FixedVolume>();
		asset.AddComponent<AffiliationComponent>();
		asset.AddComponent<VoxelBlockRequestComponent>()._bi = bi;
		if (assetKey.Name == Blocks.RopeBridgePlank.Name)
		{
			asset.AddComponent<RopeBridgePlankDamageReceiver>();
		}
		else
		{
			PlacedBlockDamageReceiver placedBlockDamageReceiver = asset.AddComponent<PlacedBlockDamageReceiver>();
			placedBlockDamageReceiver.MaxHP = Factory.Peek(freeBlockKey).GetComponent<IDamageReceiver>().MaxHP;
		}
		UISelectable component = blueprintGO.GetComponent<UISelectable>();
		component.CopyTo(asset, SelectionType.Blocks, "PlacedBlock");
	}

	private static Factory GetOrCreateFactory(Factory.AssetKey placedBlockAssetKey)
	{
		Factory orCreateFactory = Factory.GetOrCreateFactory(placedBlockAssetKey.Factory);
		orCreateFactory.InstantiatedObjectDefaultPath = "/Jeu/Environnement/BlockEngine";
		return orCreateFactory;
	}

	private static void ConfigureVolumeData(BlockInfo bi, Factory.AssetKey placedBlockAssetKey, Factory.AssetKey blueprintKey)
	{
		List<Tuple<VProperty, VProperty>> conversionRules = ((!bi.IsPartialBloc) ? convertedPropertyForFullBlock : convertedPropertyForPartialBlock);
		VolumePreprocessor.CreatePreprocessedVolume(placedBlockAssetKey, blueprintKey);
		VolumePreprocessor.KeepOnlySelectedPropertiesForVolume(PlaceBlockVProperties, placedBlockAssetKey);
		VolumePreprocessor.CopyPropertyIndicesToPropertyFromVolume(conversionRules, placedBlockAssetKey);
		VolumePreprocessor.RemovePropertiesFromVolume(VProperty.fantome, placedBlockAssetKey);
	}
}
