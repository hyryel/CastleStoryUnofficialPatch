using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;

public class BlockInfoGenerator : MonoBehaviour
{
	public static Dictionary<int, BlockInfo> _idToBlockInfos = new Dictionary<int, BlockInfo>();

	public static Dictionary<Factory.AssetKey, BlockInfo> _blueprintKeyToBlockInfos = new Dictionary<Factory.AssetKey, BlockInfo>();

	public static Dictionary<Factory.AssetKey, BlockInfo> _blockKeyToBlockInfos = new Dictionary<Factory.AssetKey, BlockInfo>();

	public static Dictionary<BlockGroups, List<BlockInfo>> _blockGroupToBlockInfos = new Dictionary<BlockGroups, List<BlockInfo>>(FastEnumIntEqualityComparer<BlockGroups>.Instance);

	public Signal<BlockInfo> BlocInfoCreatedSignal = new Signal<BlockInfo>();

	public void CreateMetaBlockInfos()
	{
		_idToBlockInfos.Clear();
		_blueprintKeyToBlockInfos.Clear();
		_blockKeyToBlockInfos.Clear();
		_blockGroupToBlockInfos.Clear();
		foreach (Factory item in Factory.GetFactories().ToList())
		{
			BlockInfoMeta[] componentsInChildren = item.GetComponentsInChildren<BlockInfoMeta>(includeInactive: true);
			for (int i = 0; i != componentsInChildren.Length; i++)
			{
				try
				{
					BlockInfo blockInfo = componentsInChildren[i].CreateBlocInfo();
					_idToBlockInfos[blockInfo.ID] = blockInfo;
					_blueprintKeyToBlockInfos[blockInfo.BlueprintKey] = blockInfo;
					_blockKeyToBlockInfos[blockInfo.Key] = blockInfo;
					_blockGroupToBlockInfos.GetValueOrCreate(blockInfo.Groups).Add(blockInfo);
					BlocInfoCreatedSignal.Invoke(blockInfo);
					UnityEngine.Object.Destroy(componentsInChildren[i]);
				}
				catch (Exception innerException)
				{
					throw new Exception($"Block Metada creation for Block Type '{componentsInChildren[i].name}'", innerException);
				}
			}
		}
	}

	private void OnDestroy()
	{
		_idToBlockInfos.Clear();
		_blueprintKeyToBlockInfos.Clear();
		_blockKeyToBlockInfos.Clear();
		_blockGroupToBlockInfos.Clear();
	}
}
