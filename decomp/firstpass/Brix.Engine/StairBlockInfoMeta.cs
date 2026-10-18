using System;
using System.Collections.Generic;
using System.IO;

namespace Brix.Engine;

public class StairBlockInfoMeta : BlockInfoMeta
{
	private class StairBlockInfo : BlockInfo
	{
		public override int CalculateNiveaux(IBlockData bd)
		{
			XYZ key = bd.Position - bd.RootOrSelf.Position;
			Dictionary<XYZ, int> value = null;
			if (NiveauxMapping.TryGetValue((StairType)ID, out value))
			{
				return value[key];
			}
			return NiveauxMapping[StairType.Normal][key];
		}

		public override float GetSurfaceHeight(IBlockData bd, float xOffset, float zOffset)
		{
			XYZ xYZ = bd.Position - bd.RootOrSelf.Position;
			if (xYZ == XYZ.zero)
			{
				return 0.5f;
			}
			if (xYZ == XYZ.forward)
			{
				return 0f - zOffset;
			}
			if (xYZ == -XYZ.forward)
			{
				return zOffset;
			}
			if (xYZ == XYZ.right)
			{
				return 0f - xOffset;
			}
			return xOffset;
		}
	}

	public enum StairType
	{
		Normal = 16,
		SpiralLeft = 503,
		SpiralRight = 504,
		CornerLeft = 505,
		CornerRight = 506
	}

	public static Dictionary<StairType, Dictionary<XYZ, int>> NiveauxMapping = new Dictionary<StairType, Dictionary<XYZ, int>>
	{
		{
			StairType.Normal,
			new Dictionary<XYZ, int>
			{
				{
					XYZ.zero,
					-1
				},
				{
					XYZ.forward,
					13056
				},
				{
					-XYZ.forward,
					51
				},
				{
					XYZ.right,
					12291
				},
				{
					-XYZ.right,
					816
				}
			}
		},
		{
			StairType.SpiralLeft,
			new Dictionary<XYZ, int>
			{
				{
					XYZ.zero,
					-1
				},
				{
					XYZ.forward,
					816
				},
				{
					-XYZ.forward,
					12291
				},
				{
					XYZ.right,
					13056
				},
				{
					-XYZ.right,
					51
				}
			}
		},
		{
			StairType.SpiralRight,
			new Dictionary<XYZ, int>
			{
				{
					XYZ.zero,
					-1
				},
				{
					XYZ.forward,
					12291
				},
				{
					-XYZ.forward,
					816
				},
				{
					XYZ.right,
					51
				},
				{
					-XYZ.right,
					13056
				}
			}
		},
		{
			StairType.CornerLeft,
			new Dictionary<XYZ, int>
			{
				{
					XYZ.zero,
					-1
				},
				{
					XYZ.forward,
					13104
				},
				{
					-XYZ.forward,
					12339
				},
				{
					XYZ.right,
					13059
				},
				{
					-XYZ.right,
					819
				}
			}
		},
		{
			StairType.CornerRight,
			new Dictionary<XYZ, int>
			{
				{
					XYZ.zero,
					-1
				},
				{
					XYZ.forward,
					13059
				},
				{
					-XYZ.forward,
					819
				},
				{
					XYZ.right,
					12339
				},
				{
					-XYZ.right,
					13104
				}
			}
		}
	};

	protected override BlockInfo InstantiateConcreteBlocInfoInstance()
	{
		return new StairBlockInfo();
	}

	protected override void Validate()
	{
		if (Type != BlockType.Block)
		{
			throw new InvalidDataException($"Configuration error! Block '{base.gameObject.name}' was set as BlockType.{Enum.GetName(typeof(BlockType), Type)} but it is configured using a StairBlockInfoMeta in the Factory.");
		}
	}
}
