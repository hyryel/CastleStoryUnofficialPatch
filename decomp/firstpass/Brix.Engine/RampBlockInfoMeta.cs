using System;
using System.Collections.Generic;
using System.IO;
using Brix.External.Factories;
using Brix.Util;

namespace Brix.Engine;

public class RampBlockInfoMeta : BlockInfoMeta
{
	private class RampBlockInfo : BlockInfo
	{
		private static Dictionary<Factory.AssetKey, int[]> _keyLevelsLibrary = new Dictionary<Factory.AssetKey, int[]>
		{
			{
				Blueprints.Wood_Ramp_TopA,
				new int[2] { 8755, 9010 }
			},
			{
				Blueprints.Wood_Ramp_TopB,
				new int[2] { 13090, 12835 }
			},
			{
				Blueprints.Wood_Ramp_BotA,
				new int[2] { 17, 272 }
			},
			{
				Blueprints.Wood_Ramp_BotB,
				new int[2] { 4352, 4097 }
			},
			{
				Blueprints.Wood_Ramp_MidA,
				new int[2] { 4386, 4641 }
			},
			{
				Blueprints.Wood_Ramp_MidB,
				new int[2] { 8721, 8466 }
			},
			{
				Blueprints.Wood_Ramp_JunctionA,
				new int[2] { 8755, 9010 }
			},
			{
				Blueprints.Wood_Ramp_JunctionB,
				new int[2] { 13090, 12835 }
			},
			{
				Blueprints.Wood_Ramp_JunctionC,
				new int[2] { 13107, 13107 }
			},
			{
				Blueprints.Wood_Ramp_JunctionD,
				new int[2] { 13107, 13107 }
			},
			{
				Blueprints.Wood_Ramp_JunctionE,
				new int[2] { 8738, 8738 }
			},
			{
				Blueprints.Wood_Ramp_JunctionF,
				new int[2] { 13107, 13107 }
			}
		};

		public override int CalculateNiveaux(IBlockData bd)
		{
			return Calcule.RotationToFaceIndex(bd.Rotation) switch
			{
				1 => _keyLevelsLibrary[bd.BlockInfo.BlueprintKey][0], 
				3 => _keyLevelsLibrary[bd.BlockInfo.BlueprintKey][1], 
				_ => 0, 
			};
		}
	}

	protected override BlockInfo InstantiateConcreteBlocInfoInstance()
	{
		return new RampBlockInfo();
	}

	protected override void Validate()
	{
		if (Type != BlockType.Block)
		{
			throw new InvalidDataException($"Configuration error! Block '{base.gameObject.name}' was set as BlockType.{Enum.GetName(typeof(BlockType), Type)} but it is configured using a StairBlockInfoMeta in the Factory.");
		}
	}
}
