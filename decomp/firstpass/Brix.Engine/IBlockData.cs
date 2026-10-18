using System;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine;

public interface IBlockData : IVoxel, IPoolable, IResetable, IDisposable
{
	new IBlockData RootOrSelf { get; }

	void RedrawVoxel(MaterialCombineDispatcher materialCombineDispatcher);

	bool MatchesGameObject(GameObject go);

	bool RemoveBlockWithMatchingRoot(BlockData rootBD);
}
