using System;
using System.Collections.Generic;
using System.IO;
using Brix.Util;

namespace Brix.Engine;

public class SimpleStairBlockInfoMeta : BlockInfoMeta
{
	private class SimpleStairBlockInfo : BlockInfo
	{
		public override int CalculateNiveaux(IBlockData bd)
		{
			int key = Calcule.RotationToFaceIndex(bd.Rotation);
			Dictionary<int, int> value = null;
			if (NiveauxMapping.TryGetValue((StairType)ID, out value))
			{
				return value[key];
			}
			return NiveauxMapping[StairType.SingleNormal][key];
		}

		public override float GetSurfaceHeight(IBlockData bd, float xOffset, float zOffset)
		{
			return Calcule.RotationToFaceIndex(bd.Rotation) switch
			{
				1 => zOffset, 
				2 => 0f - zOffset, 
				3 => xOffset, 
				4 => 0f - xOffset, 
				_ => -0.5f, 
			};
		}
	}

	public enum StairType
	{
		SingleNormal = 507,
		SingleCorner,
		SingleInvertCorner
	}

	public static Dictionary<StairType, Dictionary<int, int>> NiveauxMapping = new Dictionary<StairType, Dictionary<int, int>>
	{
		{
			StairType.SingleNormal,
			new Dictionary<int, int>
			{
				{ 1, 13056 },
				{ 2, 51 },
				{ 3, 12291 },
				{ 4, 816 }
			}
		},
		{
			StairType.SingleCorner,
			new Dictionary<int, int>
			{
				{ 1, 13104 },
				{ 2, 12339 },
				{ 3, 13059 },
				{ 4, 819 }
			}
		},
		{
			StairType.SingleInvertCorner,
			new Dictionary<int, int>
			{
				{ 1, 768 },
				{ 2, 3 },
				{ 3, 12288 },
				{ 4, 48 }
			}
		}
	};

	protected override BlockInfo InstantiateConcreteBlocInfoInstance()
	{
		return new SimpleStairBlockInfo();
	}

	protected override void Validate()
	{
		if (Type != BlockType.Block)
		{
			throw new InvalidDataException($"Configuration error! Block '{base.gameObject.name}' was set as BlockType.{Enum.GetName(typeof(BlockType), Type)} but it is configured using a StairBlockInfoMeta in the Factory.");
		}
	}
}
